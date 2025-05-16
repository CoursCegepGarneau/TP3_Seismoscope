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
        public ICommand NavigateToHistoryViewCommand { get; set; }

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
            NavigateToHistoryViewCommand = new RelayCommand(() => navigationService.NavigateTo<EventHistoryViewModel>());

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
