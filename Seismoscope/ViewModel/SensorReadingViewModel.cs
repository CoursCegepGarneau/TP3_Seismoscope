﻿using Microsoft.Win32;
using Seismoscope.Model;
using Seismoscope.Utils;
using Seismoscope.Utils.Commands;

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Seismoscope.Model.Interfaces;
using Seismoscope.Utils.Services.Interfaces;
using Seismoscope.Utils.Services;
using Seismoscope.Data.Repositories.Interfaces;
using Seismoscope.Model.Services;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Seismoscope.ViewModel
{
    public class SensorReadingViewModel : BaseViewModel
    {
        private readonly static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ISensorService _sensorService;
        private readonly INavigationService _navigationService;
        private readonly IUserSessionService _userSessionService;
        private readonly IHistoryService _historyService;
        private readonly ISensorAdjustementService _sensorAdjustementService;


        public ObservableCollection<SeismicEvent> HistoriqueEvenements { get; set; } = new();
        public ObservableCollection<SeismicEvent> EvenementsFiltres { get; set; } = new();
        public ObservableCollection<ISeries> SensorSeries { get; set; }

        private ObservableCollection<Sensor> _sensors = null!;

        public ObservableCollection<Sensor> Sensors
        {
            get => _sensors;
            set
            {
                _sensors = value;
                OnPropertyChanged();
            }
        }


        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }




        public RelayCommand GoBackCommand { get; }
        public ICommand LoadCsvCommand { get; }
        public ICommand StartReadingCommand => new RelayCommand(async () => await StartReadingAsync());
        public ICommand StopReadingCommand => new RelayCommand(() => _cts?.Cancel());

        public Sensor? SelectedSensor { get; set; }

        private string _csvFilePath;



        private List<SeismicEvent> _donneesSismiques;
        public ObservableCollection<double> AmplitudeValues { get; set; } = new();
        public ISeries[] Series { get; set; }

        private int _tempsSimulé = 0;

        public ObservableCollection<double> Amplitudes { get; set; } = new();
        public ObservableCollection<string> Timestamps { get; set; } = new();
        

        private CancellationTokenSource? _cts;

        
        public ICommand NavigateToHistoryViewCommand { get; set; }





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


        private string _messageRegle;
        public string MessageRegle
        {
            get => _messageRegle;
            set
            {
                _messageRegle = value;
                OnPropertyChanged();
            }
        }


        public SensorReadingViewModel(ISensorService sensorService, INavigationService navigationService, IUserSessionService userSessionService, IHistoryService historyService, ISensorAdjustementService sensorAdjustementService)
        {

            _userSessionService = userSessionService;
            _sensorService = sensorService;
            _navigationService = navigationService;
            _sensorAdjustementService = sensorAdjustementService;

            _historyService = historyService;

            Sensors = new ObservableCollection<Sensor>(_sensorService.GetAllSensors());
            RefreshSensors();
            SetupSensorChart();


            
            LoadCsvCommand = new RelayCommand(OpenCsvDialog);
            GoBackCommand = new RelayCommand(() => navigationService.NavigateTo<SensorViewModel>());
            NavigateToHistoryViewCommand = new RelayCommand(() => navigationService.NavigateTo<EventHistoryViewModel>());

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


        public override void OnNavigated(object? parameter = null)
        {
            if (parameter is Sensor sensor)
            {
                SelectedSensor = sensor;
            }
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
                //_donneesSismiques = CsvUtils.LireLecturesDepuisCsv(_csvFilePath);
                DonneesImportees = true;
                
                //_donneesSismiques = CsvUtils.LireLecturesDepuisCsv(_csvFilePath, SelectedSensor, _sensorAdjustementService, out messages);

                _donneesSismiques = CsvUtils.LireLecturesDepuisCsv(_csvFilePath);


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



        public ObservableCollection<string> MessagesUI { get; set; } = new();

        public void TraiterLigne(int index, SeismicEvent data)
        {
            int maxPoints = 30;
            if (Amplitudes.Count >= maxPoints)
            {
                Amplitudes.RemoveAt(0);
                Timestamps.RemoveAt(0);
            }

            Amplitudes.Add(data.Amplitude);
            Timestamps.Add($"t{index}");

            if (data.Amplitude > SelectedSensor.Treshold)
            {
                logger.Info($"Événement détecté par {SelectedSensor.Name} | Onde : {data.TypeOnde} | Amplitude : {data.Amplitude:F2} mm | Seuil : {SelectedSensor.Treshold:F2} mm");
                EvenementsFiltres.Add(new SeismicEvent
                {
                    Timestamp = DateTime.Now,
                    SensorName = SelectedSensor.Name,
                    TypeOnde = data.TypeOnde,
                    Amplitude = data.Amplitude,
                    SeuilAtteint = SelectedSensor.Treshold
                });

                _historyService.AjouterHistory(new HistoriqueEvenement
                {
                    DateHeure = DateTime.Now,
                    Amplitude = data.Amplitude,
                    TypeOnde = data.TypeOnde,
                    SeuilAuMoment = SelectedSensor.Treshold,
                    SensorId = SelectedSensor.Id,
                    SensorName = SelectedSensor.Name
                });
            }

            var messages = _sensorAdjustementService.AdjustSensors(data, SelectedSensor);
            foreach (var msg in messages)
            {
                MessagesUI.Add(msg); // Pour afficher les messages de retroaction

                logger.Info($"Ajustement appliqué sur {SelectedSensor.Name} → {msg}");
            }
        }

        public async Task StartReadingAsync()
        {
            int maxPoints = 30;

            if (_donneesSismiques == null || _donneesSismiques.Count == 0 || SelectedSensor == null)
                return;

            _cts = new CancellationTokenSource();
            IsReading = true;

            for (int i = 0; i < _donneesSismiques.Count; i++)
            {
                if (_cts.IsCancellationRequested)
                    break;

                var data = _donneesSismiques[i];

                Application.Current.Dispatcher.Invoke(() =>
                {
                    TraiterLigne(i, data);
                });

                // Fréquence dynamique 
                double seconds = 1.0 / SelectedSensor.Frequency;
                int intervalMs = (int)(seconds * 1000);
                await Task.Delay(intervalMs);
            }

            IsReading = false;
        }

        public void SetDonneesSismiques(List<SeismicEvent> donnees)
        {
            _donneesSismiques = donnees;
        }




        //public void FiltrerEvenements()
        //{
        //    EvenementsFiltres.Clear();
        //    var filtrés = SelectedTypeOnde == "Tous"
        //        ? HistoriqueEvenements
        //        : new ObservableCollection<SeismicEvent>(HistoriqueEvenements.Where(e => e.TypeOnde == SelectedTypeOnde));

        //    foreach (var evt in filtrés)
        //        EvenementsFiltres.Add(evt);
        //}

        /*private async Task StartReadingAsync()
        {
            int maxPoints = 30; // On affiche les 30 dernières lectures

            if (_donneesSismiques == null || _donneesSismiques.Count == 0 || SelectedSensor == null)
                return;

            _cts = new CancellationTokenSource();
            IsReading = true;
            double seuil = SelectedSensor.Treshold;
            double seconds = 1.0 / SelectedSensor.Frequency;
            int intervalMs = (int)(seconds * 1000);


            for (int i = 0; i < _donneesSismiques.Count; i++)
            {
                if (_cts.IsCancellationRequested)
                    break;

                var data = _donneesSismiques[i];

                Application.Current.Dispatcher.Invoke(() =>
                {

                    if (Amplitudes.Count >= maxPoints)
                    {
                        Amplitudes.RemoveAt(0);
                        Timestamps.RemoveAt(0);
                    }

                    Amplitudes.Add(data.Amplitude);
                    Timestamps.Add($"t{i}");

                    if (data.Amplitude > seuil)
                    {
                        MessageBox.Show($"🌍 Événement détecté: {data.TypeOnde} - {data.Amplitude} mm", "Alerte", MessageBoxButton.OK, MessageBoxImage.Warning);

                        // TODO : Ajouter à l'historique de la station ici


                        EvenementsFiltres.Add(new SeismicEvent
                        {
                            Timestamp = DateTime.Now,
                            SensorName = SelectedSensor.Name,
                            TypeOnde = data.TypeOnde,
                            Amplitude = data.Amplitude,
                            SeuilAtteint = SelectedSensor.Treshold
                        });
                        //FiltrerEvenements();


                        var evenement = new HistoriqueEvenement
                        {
                            DateHeure = DateTime.Now,
                            Amplitude = data.Amplitude,
                            TypeOnde = data.TypeOnde,
                            SeuilAuMoment = SelectedSensor.Treshold,
                            SensorId = SelectedSensor.Id,
                            SensorName = SelectedSensor.Name,
                        };

                        _historyService.AjouterHistory(evenement);
                    }
                });

                await Task.Delay(intervalMs);
            }

            IsReading = false;
        }*/


    }
}
