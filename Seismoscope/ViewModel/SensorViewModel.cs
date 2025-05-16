using System;
using System.IO;
using System.Timers;
using System.Windows.Threading;
using System.Collections.Generic;
using Seismoscope.Model.Interfaces;
using Seismoscope.Utils;
using NLog;
using Seismoscope.Model;
using Seismoscope.Model.DAL;
using Seismoscope.Utils.Commands;
using Seismoscope.Utils.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Seismoscope.Services;
using Seismoscope.Services.Interfaces;
using Seismoscope.Utils.Enums;
using Seismoscope.View;
using System.Windows;
using System.Globalization;
using System.Diagnostics.Metrics;
using Seismoscope.Utils.Services;
using LiveChartsCore;
using Microsoft.Win32;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting;

namespace Seismoscope.ViewModel
{

    public class SensorViewModel : BaseViewModel
    {
        private readonly static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ISensorService _sensorService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IUserSessionService _userSessionService;
        private readonly ReaderService _csvReaderService;
        private ObservableCollection<Sensor> _sensors = null!;
        private DispatcherTimer _readingTimer;
        private int _csvIndex = 0;



        public ObservableCollection<Sensor> Sensors
        {
            get => _sensors;
            set
            {
                _sensors = value;
                OnPropertyChanged();
            }
        }

        private Station? _selectedStation;
        public Station? SelectedStation
        {
            get => _selectedStation;
            set
            {
                _selectedStation = value;
                OnPropertyChanged();
            }
        }

        private Sensor? _selectedSensor;

        public Sensor? SelectedSensor
        {
            get => _selectedSensor;
            set
            {
                _selectedSensor = value;
                OnPropertyChanged(nameof(SelectedSensor));
            }
        }

        private int _selectedSensorIndex;
        public int SelectedSensorIndex
        {
            get { return _selectedSensorIndex; }
            set
            {
                ((RelayCommand)DeleteSensorCommand).RaiseCanExecuteChanged();
                _selectedSensorIndex = value;
                OnPropertyChanged();
            }
        }

        public ICommand NavigateToHomeViewCommand { get; set; }
        public ICommand ChargerDonneesCommand { get; set; }
        
        public ICommand? NavigateToSensorManagementViewCommand { get; }
        public ICommand? NavigateToSensorManagementForAssignmentCommand { get; }
        public ICommand? AddSensorToStationCommand { get; }
        public ICommand UpdateSensorStatusCommand { get; set; }
        public ICommand ChangeFrequencyCommand { get; set; }
        public ICommand ChangeTresholdCommand { get; set; }
        public ICommand AddSensorCommand { get; set; }
        public ICommand DeleteSensorCommand { get; set; }


        public ICommand AnalyzeSensorCommand { get; }

        public ICommand LoadCsvCommand { get; }

        private string _csvFilePath;


        private List<SeismicEvent> _donneesSismiques;
        public ObservableCollection<double> AmplitudeValues { get; set; } = new();

        public ObservableCollection<SeismicEvent> HistoriqueEvenements { get; set; } = new();
        public ObservableCollection<SeismicEvent> EvenementsFiltres { get; set; } = new();
        public string SelectedTypeOnde { get; set; } = "Tous"; // Pour ComboBox



        private int _tempsSimulé = 0;

        public ObservableCollection<double> Amplitudes { get; set; } = new();
        public ObservableCollection<string> Timestamps { get; set; } = new();


        private CancellationTokenSource? _cts;

        public ICommand StartReadingCommand => new RelayCommand(async () => await StartReadingAsync());
        public ICommand StopReadingCommand => new RelayCommand(() => _cts?.Cancel());



        private bool _donneesImportees;
        private bool _isReading;

        public bool DonneesImportees
        {
            get => _donneesImportees;
            set
            {
                _donneesImportees = value;
                OnPropertyChanged();

            }
        }

        public bool IsReading
        {
            get => _isReading;
            set
            {
                _isReading = value;
                OnPropertyChanged();

            }
        }

        private ISeries[] _series;
        public ISeries[] Series
        {
            get => _series;
            set
            {
                _series = value;
                OnPropertyChanged();
            }
        }

        private Axis[] _xAxes;
        public Axis[] XAxes
        {
            get => _xAxes;
            set
            {
                _xAxes = value;
                OnPropertyChanged();
            }
        }

        private Axis[] _yAxes;
        public Axis[] YAxes
        {
            get => _yAxes;
            set
            {
                _yAxes = value;
                OnPropertyChanged();
            }
        }

        private readonly Dictionary<Sensor, SKColor> _sensorColorMap = new();
        private readonly Random _rand = new();

        private SKColor GenerateRandomColor()
        {
            // Évite les couleurs trop pâles ou trop proches
            byte r = (byte)_rand.Next(50, 200);
            byte g = (byte)_rand.Next(50, 200);
            byte b = (byte)_rand.Next(50, 200);
            return new SKColor(r, g, b);
        }








        public SensorViewModel(ISensorService sensorService, INavigationService navigationService,IDialogService dialogService, IUserSessionService userSessionService)
        {
            _sensorService = sensorService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _userSessionService = userSessionService;
            _csvReaderService = new ReaderService();
            
            Sensors = new ObservableCollection<Sensor>(_sensorService.GetAllSensors());
            ChargerDonneesCommand = new RelayCommand(ChargerDonnees);
            
            NavigateToHomeViewCommand = new RelayCommand(() => navigationService.NavigateTo<HomeViewModel>());
            UpdateSensorStatusCommand = new RelayCommand(UpdateSensorStatus);
            ChangeFrequencyCommand = new RelayCommand(ChangeFrequency);
            ChangeTresholdCommand = new RelayCommand(ChangeTreshold);
            AddSensorCommand = new RelayCommand(NavigateToSensorManagementForAssignment);
            DeleteSensorCommand = new RelayCommand(Delete, CanDelete);
            RefreshSensors();


            AnalyzeSensorCommand = new RelayCommand<Sensor>(sensor =>
            {
                if (sensor != null)
                    _navigationService.NavigateTo<SensorReadingViewModel>(sensor);
            });


            LoadCsvCommand = new RelayCommand(OpenCsvDialog);
            SetupSensorChart();
        }


        public void RefreshSensors()
        {
            var station = _userSessionService.AsEmploye?.Station;

            if (station != null)
            {
                Sensors = new ObservableCollection<Sensor>(_sensorService.GetSensorByStationId(station.Id));
            }
            else
            {
                Sensors = new ObservableCollection<Sensor>();
            }

            OnPropertyChanged(nameof(Sensors));
        }

        private void ChargerDonnees()
        {
            var path = _dialogService.OpenFile("CSV files|*.csv");
            if (string.IsNullOrEmpty(path))
            {
                _dialogService.ShowDialog("⚠️Aucun fichier sélectionné.");
                return;
            }
                
            string[] allLines = File.ReadAllLines(path);
            _csvReaderService.LoadCsv(path);
            if (_csvReaderService.GetTotalLines() == 0)
            {
                _dialogService.ShowDialog("⚠️ Le fichier est vide");
                return;
            }
            _csvIndex = 0;
        }
        
        

        private void StartReadingData()
        {
            if (_csvReaderService.GetTotalLines() == 0)
                return;
            _csvIndex = 0;
            _readingTimer = new DispatcherTimer();//A Adapter éventuellement 
            _readingTimer.Interval = TimeSpan.FromSeconds(5);
            _readingTimer.Tick += ReadNextLine;
            _readingTimer.Start();
        }

        private void StopReadingData()
        {
            if (_readingTimer == null)
                return;

            _readingTimer.Stop();
            _readingTimer.Tick -= ReadNextLine;
            _readingTimer = null;
        }

        private void ReadNextLine(object? sender, EventArgs e) // TODO A revoir fortement
        {
            while (_csvIndex < _csvReaderService.GetTotalLines())
            {
                var line = _csvReaderService.GetNextLine(_csvIndex);
                _csvIndex++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var data = line.Split(',');
                if (data.Length < 2 || !double.TryParse(data[1], out double amplitude))
                    continue;

                // Vérification du capteur sélectionné
                if (SelectedSensor == null)
                {
                    StopReadingData();
                    return;
                }

                if (amplitude > SelectedSensor.Treshold)
                {
                    _dialogService.ShowDialog($"⚠️ Alerte : Amplitude {amplitude} mm détectée !");
                    //_sensorService.LogEvent(SelectedSensor, amplitude);
                }

                break;
            }
        }

        private void UpdateSensorStatus()
        {
            if (SelectedSensor != null)
            {
                _sensorService.UpdateSensorStatus(SelectedSensor);
                logger.Info($"[Capteur] Statut mis à jour : {SelectedSensor.Name}");
                OnPropertyChanged(nameof(SelectedSensor));
                RefreshSensors();
            }
        }

        private void ChangeFrequency()
        {
            if (SelectedSensor == null)
                return;

            var dialogVM = new SensorDialogViewModel
            {
                ShowName = false,
                ShowFrequency = true,
                ShowTreshold = false,
                Frequency = SelectedSensor.Frequency.ToString("0.00")
            };

            bool? result = _dialogService.ShowDialog(dialogVM);
            if (result == true)
            {
                double oldFreq = SelectedSensor.Frequency;
                SelectedSensor.Frequency = double.Parse(dialogVM.Frequency, CultureInfo.InvariantCulture);
                _sensorService.ChangeSensorFrequency(SelectedSensor);
                logger.Info($"[Capteur] Fréquence modifiée : {SelectedSensor.Name}, de {oldFreq} à {SelectedSensor.Frequency}");
                OnPropertyChanged(nameof(SelectedSensor));
                RefreshSensors();
            }
        }

        private void ChangeTreshold()
        {
            if (SelectedSensor == null)
                return;

            var dialogVM = new SensorDialogViewModel
            {
                ShowName = false,
                ShowFrequency = false,
                ShowTreshold = true,
                Treshold = SelectedSensor.Treshold.ToString("0.00")
            };

            SelectedSensor.Treshold = double.Parse(dialogVM.Treshold, CultureInfo.InvariantCulture);

            bool? result = _dialogService.ShowDialog(dialogVM);
            if (result == true)
            {
                double oldTreshold = SelectedSensor.Treshold;
                SelectedSensor.Treshold = double.Parse(dialogVM.Treshold, CultureInfo.InvariantCulture);
                _sensorService.ChangeSensorTreshold(SelectedSensor);
                logger.Info($"[Capteur] Seuil modifié : {SelectedSensor.Name}, de {oldTreshold} à {SelectedSensor.Treshold}");
                OnPropertyChanged(nameof(SelectedSensor));
                RefreshSensors();
            }
        }

        private void NavigateToSensorManagementForAssignment()
        {
            logger.Info("[Navigation] Vers SensorManagementViewModel (assignation)");
            _userSessionService.IsAssignationMode = true;
            _navigationService.NavigateTo<SensorManagementViewModel>();
        }

        private void Delete()
        {
            if (SelectedSensor is null)
                return;
            logger.Info($"[Capteur] Supprimé : {SelectedSensor.Name} (ID: {SelectedSensor.Id})");
            _sensorService.DeleteSensor(SelectedSensor);
            Sensors.Remove(SelectedSensor);
            SelectedSensor = null;
            OnPropertyChanged(nameof(Sensors));
        }

        private bool CanDelete()
        {
            return SelectedSensor != null && SelectedSensor.SensorStatus == false;
        }



        private void OpenCsvDialog()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Sélectionner un fichier de données sismiques"
            };

            if (dialog.ShowDialog() == true)
            {
                _csvFilePath = dialog.FileName;
                _donneesSismiques = CsvUtils.LireLecturesDepuisCsv(_csvFilePath);

                // Reset collections
                Amplitudes.Clear();
                Timestamps.Clear();

                for (int i = 0; i < _donneesSismiques.Count && i < 5; i++)
                {
                    Amplitudes.Add(_donneesSismiques[i].Amplitude);
                    Timestamps.Add($"t{i}");
                }

                


                DonneesImportees = true;

                MessageBox.Show("Fichier chargé avec succès.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        
       

        private void SetupSensorChart()
        {
            Series = new ISeries[]
            {
        new LineSeries<double>
        {
            Values = Amplitudes,
            GeometrySize = 8,
            Fill = null,
            Name = "Amplitude (mm)"
        }
            };

            XAxes = new Axis[]
            {
        new Axis
        {
            Name = "Temps",
            Labels = Timestamps,
            LabelsRotation = 15
        }
            };

            YAxes = new Axis[]
            {
        new Axis
        {
            Name = "Amplitude (mm)"
        }
            };
        }




        private async Task StartReadingAsync()
        {

            _sensorColorMap.Clear();

            foreach (var sensor in Sensors)
            {
                if (!_sensorColorMap.ContainsKey(sensor))
                    _sensorColorMap[sensor] = GenerateRandomColor();
            }
            if (_donneesSismiques == null || _donneesSismiques.Count == 0 || Sensors == null)
                return;

            _cts = new CancellationTokenSource();
            IsReading = true;

            int maxPoints = 30;

            // Dictionnaire : capteur -> liste des amplitudes
            var sensorSeriesDict = new Dictionary<Sensor, ObservableCollection<double>>();
            var timestamps = new ObservableCollection<string>();

            foreach (var sensor in Sensors)
            {
                sensorSeriesDict[sensor] = new ObservableCollection<double>();
            }

            for (int i = 0; i < _donneesSismiques.Count; i++)
            {
                if (_cts.IsCancellationRequested)
                    break;

                var data = _donneesSismiques[i];

                Application.Current.Dispatcher.Invoke(() =>
                {
                    timestamps.Add($"t{i}");
                    if (timestamps.Count > maxPoints)
                        timestamps.RemoveAt(0);

                    foreach (var sensor in Sensors)
                    {
                        var values = sensorSeriesDict[sensor];

                        if (values.Count >= maxPoints)
                            values.RemoveAt(0);

                        values.Add(data.Amplitude);

                        if (data.Amplitude > sensor.Treshold)
                        {
                            MessageBox.Show($"🌍 {sensor.Name} a détecté un événement : {data.TypeOnde} - {data.Amplitude} mm", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);

                            // Pour historique

                            HistoriqueEvenements.Add(new SeismicEvent
                            {
                                Timestamp = DateTime.Now,
                                SensorName = sensor.Name,
                                TypeOnde = data.TypeOnde,
                                Amplitude = data.Amplitude,
                                SeuilAtteint = sensor.Treshold
                            });
                            FiltrerEvenements();
                        }
                    }

                    // Rafraîchit toutes les courbes à chaque tick
                    Series = sensorSeriesDict.Select(kv =>
                    {
                        var color = _sensorColorMap[kv.Key];

                        return new LineSeries<double>
                        {
                            Name = kv.Key.Name,
                            Values = kv.Value,
                            GeometrySize = 8,
                            Fill = null,
                            Stroke = new SolidColorPaint(color, 2),
                            GeometryStroke = new SolidColorPaint(color, 2),
                            GeometryFill = new SolidColorPaint(color)
                        };
                    }).ToArray();


                    XAxes = new[]
                    {
                new Axis
                {
                    Name = "Temps",
                    Labels = timestamps,
                    LabelsRotation = 15
                }
            };
                });

                // Intervalle fixe ou ajusté par capteur
                await Task.Delay(4000);
            }

            IsReading = false;
        }

        public void FiltrerEvenements()
        {
            EvenementsFiltres.Clear();
            var filtrés = SelectedTypeOnde == "Tous"
                ? HistoriqueEvenements
                : new ObservableCollection<SeismicEvent>(HistoriqueEvenements.Where(e => e.TypeOnde == SelectedTypeOnde));

            foreach (var evt in filtrés)
                EvenementsFiltres.Add(evt);
        }


    }
}
