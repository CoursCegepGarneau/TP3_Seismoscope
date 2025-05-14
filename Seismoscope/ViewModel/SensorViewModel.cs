using System;
using System.IO;
using System.Timers;
using System.Windows.Threading;
using System.Collections.Generic;
using Seismoscope.Model.Interfaces;
using Seismoscope.Utils;
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

namespace Seismoscope.ViewModel
{

    public class SensorViewModel : BaseViewModel
    {

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
        public ICommand StartReadingCommand { get; }
        public ICommand StopReadingCommand { get; }
        public ICommand? NavigateToSensorManagementViewCommand { get; }
        public ICommand? NavigateToSensorManagementForAssignmentCommand { get; }
        public ICommand? AddSensorToStationCommand { get; }
        public ICommand UpdateSensorStatusCommand { get; set; }
        public ICommand ChangeFrequencyCommand { get; set; }
        public ICommand ChangeTresholdCommand { get; set; }
        public ICommand AddSensorCommand { get; set; }
        public ICommand DeleteSensorCommand { get; set; }


        public ICommand AnalyzeSensorCommand { get; }

        public SensorViewModel(ISensorService sensorService, INavigationService navigationService,IDialogService dialogService, IUserSessionService userSessionService)
        {
            _sensorService = sensorService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _userSessionService = userSessionService;
            _csvReaderService = new ReaderService();
            StartReadingCommand = new RelayCommand(StartReadingData);
            StopReadingCommand = new RelayCommand(StopReadingData);
            Sensors = new ObservableCollection<Sensor>(_sensorService.GetAllSensors());
            ChargerDonneesCommand = new RelayCommand(ChargerDonnees);
            StartReadingCommand = new RelayCommand(StartReadingData);
            StopReadingCommand = new RelayCommand(StopReadingData);
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
                SelectedSensor.Frequency = double.Parse(dialogVM.Frequency, CultureInfo.InvariantCulture);
                _sensorService.ChangeSensorFrequency(SelectedSensor);
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
                SelectedSensor.Treshold = double.Parse(dialogVM.Treshold, CultureInfo.InvariantCulture);
                _sensorService.ChangeSensorTreshold(SelectedSensor);
                OnPropertyChanged(nameof(SelectedSensor));
                RefreshSensors();
            }
        }

        private void NavigateToSensorManagementForAssignment()
        {
            _userSessionService.IsAssignationMode = true;
            _navigationService.NavigateTo<SensorManagementViewModel>();
        }


        private void Delete()
        {
            if (SelectedSensor is null)
                return;
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
