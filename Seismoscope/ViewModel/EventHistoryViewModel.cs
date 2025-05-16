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
using Seismoscope.Data.Repositories.Interfaces;

namespace Seismoscope.ViewModel
{

    public class EventHistoryViewModel : BaseViewModel
    {
        private readonly static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ISensorService _sensorService;
        private readonly INavigationService _navigationService;
        private readonly IHistoryService _historyService;
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

        

        public ICommand NavigateToHomeViewCommand { get; set; }
        public ICommand NavigateToHistoryViewCommand { get; set; }
        
        
        public ICommand? NavigateToSensorManagementViewCommand { get; }
        public ICommand? NavigateToSensorManagementForAssignmentCommand { get; }
        
        public ICommand AddSensorCommand { get; set; }
        


        private List<SeismicEvent> _donneesSismiques;
        public ObservableCollection<double> AmplitudeValues { get; set; } = new();

        public ObservableCollection<SeismicEvent> HistoriqueEvenements { get; set; } = new();
        public ObservableCollection<SeismicEvent> EvenementsFiltres { get; set; } = new();
        
        public string SelectedTypeOnde { get; set; } = "Tous"; // Pour ComboBox



        private int _tempsSimulé = 0;

        public ObservableCollection<double> Amplitudes { get; set; } = new();
        public ObservableCollection<string> Timestamps { get; set; } = new();

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







        private ObservableCollection<HistoriqueEvenement> _allHistory = new();
        public ObservableCollection<HistoriqueEvenement> AllHistory
        {
            get => _allHistory;
            set
            {
                _allHistory = value;
                OnPropertyChanged();
            }
        }



        public EventHistoryViewModel(ISensorService sensorService, INavigationService navigationService,IDialogService dialogService, IUserSessionService userSessionService, IHistoryService historyService)
        {
            _sensorService = sensorService;
            _navigationService = navigationService;
            _userSessionService = userSessionService;

            _historyService = historyService;

             


            Sensors = new ObservableCollection<Sensor>(_sensorService.GetAllSensors());

            AllHistory = new ObservableCollection<HistoriqueEvenement>(_historyService.GetAllHistory());

            
            
            
            NavigateToHomeViewCommand = new RelayCommand(() => navigationService.NavigateTo<HomeViewModel>());

            NavigateToHistoryViewCommand = new RelayCommand(() => navigationService.NavigateTo<EventHistoryViewModel>());
            
            AddSensorCommand = new RelayCommand(NavigateToSensorManagementForAssignment);

            ChargerHistorique();


            
            
        }


        private void ChargerHistorique()
        {
            AllHistory.Clear();

            var evenements = _historyService.GetAllHistory();

            foreach (var e in evenements)
            {
                AllHistory.Add(new HistoriqueEvenement
                {
                    DateHeure = e.DateHeure,
                    Amplitude = e.Amplitude,
                    SensorName = e.SensorName,
                    TypeOnde = e.TypeOnde,
                    SeuilAuMoment = e.SeuilAuMoment
                });
            }
        }

        public override void OnNavigated(object? parameter = null)
        {
            ChargerHistorique();
        }










        private void NavigateToSensorManagementForAssignment()
        {
            logger.Info("[Navigation] Vers SensorManagementViewModel (assignation)");
            _userSessionService.IsAssignationMode = true;
            _navigationService.NavigateTo<SensorManagementViewModel>();
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
