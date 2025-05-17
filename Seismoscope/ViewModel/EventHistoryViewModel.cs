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
        public ObservableCollection<double> Amplitudes { get; set; } = new();
        public ObservableCollection<string> Timestamps { get; set; } = new();

        private ObservableCollection<HistoriqueEvenement> _allHistory = new();
        public ObservableCollection<HistoriqueEvenement> AllHistory
        {
            get => _allHistory;
            set
            {
                _allHistory = value;
                OnPropertyChanged();

                ApplyFilters(); //Pour appliquer les filtres pour la table d'historique
            }
        }
        public ObservableCollection<string> SensorNames { get; set; }
        public ObservableCollection<string> TypeOndes { get; set; } = new();
        public ObservableCollection<HistoriqueEvenement> FilteredHistory { get; set; } = new();





        


        private string _selectedSensorName = "Tous";
        public string SelectedSensorName
        {
            get => _selectedSensorName;
            set
            {
                _selectedSensorName = value;
                OnPropertyChanged();
                ApplyFilters();
                
            }
        }

        private string _selectedTypeOnde = "Tous";
        public string SelectedTypeOnde
        {
            get => _selectedTypeOnde;
            set
            {
                _selectedTypeOnde = value;
                OnPropertyChanged();
                ApplyFilters();
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




        public EventHistoryViewModel(ISensorService sensorService, INavigationService navigationService, IUserSessionService userSessionService, IHistoryService historyService)
        {
            _sensorService = sensorService;
            _navigationService = navigationService;
            _userSessionService = userSessionService;

            _historyService = historyService;

            Sensors = new ObservableCollection<Sensor>(_sensorService.GetAllSensors());

            AllHistory = new ObservableCollection<HistoriqueEvenement>(_historyService.GetAllHistory());

            SensorNames = new ObservableCollection<string> { "Tous" };




            NavigateToHomeViewCommand = new RelayCommand(() => navigationService.NavigateTo<HomeViewModel>());

            NavigateToHistoryViewCommand = new RelayCommand(() => navigationService.NavigateTo<EventHistoryViewModel>());
            
            AddSensorCommand = new RelayCommand(NavigateToSensorManagementForAssignment);

            ChargerHistorique();

            InitialiserFiltres();

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

        

        private void InitialiserFiltres()
        {
            SensorNames.Clear();
            SensorNames.Add("Tous");
            foreach (var name in AllHistory.Select(h => h.SensorName).Distinct())
                SensorNames.Add(name);

            TypeOndes.Clear();
            TypeOndes.Add("Tous");
            foreach (var type in AllHistory.Select(h => h.TypeOnde).Distinct())
                TypeOndes.Add(type);
        }

        public void ApplyFilters()
        {
            var filtre = AllHistory.AsEnumerable();

            if (SelectedSensorName != "Tous")
                filtre = filtre.Where(e => e.SensorName == SelectedSensorName);

            if (SelectedTypeOnde != "Tous")
                filtre = filtre.Where(e => e.TypeOnde == SelectedTypeOnde);

            FilteredHistory.Clear();
            foreach (var item in filtre)
            {
                FilteredHistory.Add(item);

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


        

    }
}
