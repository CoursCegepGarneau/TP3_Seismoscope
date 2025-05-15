using System;
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

namespace Seismoscope.ViewModel
{

    public class SensorViewModel : BaseViewModel
    {
        private readonly static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ISensorService _sensorService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
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
        public ICommand? NavigateToSensorManagementViewCommand { get; }
        public ICommand? NavigateToSensorManagementForAssignmentCommand { get; }
        public ICommand? AddSensorToStationCommand { get; }
        public ICommand UpdateSensorStatusCommand { get; set; }
        public ICommand ChangeFrequencyCommand { get; set; }
        public ICommand ChangeTresholdCommand { get; set; }
        public ICommand AddSensorCommand { get; set; }
        public ICommand DeleteSensorCommand { get; set; }

        public SensorViewModel(ISensorService sensorService, INavigationService navigationService,IDialogService dialogService, IUserSessionService userSessionService)
        {
            _sensorService = sensorService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _userSessionService = userSessionService;
            Sensors = new ObservableCollection<Sensor>(_sensorService.GetAllSensors());
            NavigateToHomeViewCommand = new RelayCommand(() => navigationService.NavigateTo<HomeViewModel>());
            UpdateSensorStatusCommand = new RelayCommand(UpdateSensorStatus);
            ChangeFrequencyCommand = new RelayCommand(ChangeFrequency);
            ChangeTresholdCommand = new RelayCommand(ChangeTreshold);
            AddSensorCommand = new RelayCommand(NavigateToSensorManagementForAssignment);
            DeleteSensorCommand = new RelayCommand(Delete, CanDelete);
            RefreshSensors();
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
    }
}
