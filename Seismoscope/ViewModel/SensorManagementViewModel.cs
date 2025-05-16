using Seismoscope.Model;
using System.Collections.ObjectModel;
using NLog;
using Seismoscope.Utils;
using Seismoscope.Model.Interfaces;
using Seismoscope.Utils.Commands;
using Seismoscope.Utils.Services.Interfaces;
using System.Windows.Input;
using Seismoscope.Utils.Enums;
using System.Reflection.Metadata;
using static Seismoscope.ViewModel.SensorViewModel;
using System.Windows;

namespace Seismoscope.ViewModel
{
    public class SensorManagementViewModel : BaseViewModel
    {
        private readonly IUserSessionService _userSessionService;
        private readonly ISensorService _sensorService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private Sensor? _selectedSensor;

        public ObservableCollection<Sensor> AllSensors { get; set; }

        public Sensor? SelectedSensor
        {
            get => _selectedSensor;
            set
            {
                _selectedSensor = value;
                OnPropertyChanged();
                ((RelayCommand)DeleteSensorCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeliverSensorCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand NavigateToHomeViewCommand { get; }
        public ICommand DeliverSensorCommand { get; }
        public ICommand UpdateSensorStatusCommand { get; }
        public ICommand AddSensorCommand { get; }
        public ICommand DeleteSensorCommand { get; }
        public ICommand AssignSensorToStationCommand { get; }

        public bool IsAssignationMode { get; }


        public SensorManagementViewModel(ISensorService sensorService, INavigationService navigationService, IUserSessionService userSessionService, IDialogService dialogService)
        {
            _userSessionService = userSessionService;
            _sensorService = sensorService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            IsAssignationMode = userSessionService.IsAssignationMode;
            userSessionService.IsAssignationMode = false;
            AllSensors = new ObservableCollection<Sensor>(_sensorService.GetAllSensors());
            NavigateToHomeViewCommand = new RelayCommand(() =>
            {
                logger.Info("[Navigation] vers vue HomeView depuis SensorManagement.");
                _navigationService.NavigateTo<HomeViewModel>();
            });
            DeliverSensorCommand = new RelayCommand(DeliverSensor, DeliveredStatus);
            UpdateSensorStatusCommand = new RelayCommand(UpdateSensorStatus, CanUpdateStatus);
            AddSensorCommand = new RelayCommand(AddSensor);
            DeleteSensorCommand = new RelayCommand(DeleteSensor, CanDelete);
            AssignSensorToStationCommand = new RelayCommand(AssignSensorToStation, CanAssignSensorToStation);
        }

        private void RefreshSensors()
        {
            AllSensors = new ObservableCollection<Sensor>(_sensorService.GetAllSensors());
            OnPropertyChanged(nameof(AllSensors));
        }

        private void DeliverSensor()
        {
            if (SelectedSensor != null && !SelectedSensor.Delivered)
            {
                var result = MessageBox.Show(
                    "Confirmer que le capteur a été livré.",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result != MessageBoxResult.Yes)
                    return;

                _sensorService.UpdateSensorDeliveryStatus(SelectedSensor);
                logger.Info($"Capteur livré : {SelectedSensor.Name}");
                OnPropertyChanged(nameof(SelectedSensor));
                RefreshSensors();
            }
        }


        /// <summary>
        /// Met à jour l'état d'un capteur sélectionné s'il est livré
        /// </summary>
        private void UpdateSensorStatus()
        {
            if (SelectedSensor != null && SelectedSensor.Delivered)
            {
                _sensorService.UpdateSensorStatus(SelectedSensor);
                logger.Info($"Statut du capteur mis à jour : {SelectedSensor.Name}");
                OnPropertyChanged(nameof(SelectedSensor));
                RefreshSensors();
            }
            else
            {
                MessageBox.Show("Erreur : aucun capteur sélectionné ou le capteur n'est pas livré.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanUpdateStatus()
        {
            return SelectedSensor != null && SelectedSensor.Delivered == true;
        }
        private bool DeliveredStatus()
        {
            return SelectedSensor != null && SelectedSensor.Delivered == false;
        }
        private void AddSensor()
        {
            var dialogVM = new SensorDialogViewModel
            {
                ShowName = true,
                ShowFrequency = true,
                ShowTreshold = true
            };

            bool? result = _dialogService.ShowDialog(dialogVM);
            if (result == true &&
                !(dialogVM.Name).Empty() &&
                double.TryParse(dialogVM.Frequency, out double freq) &&
                double.TryParse(dialogVM.Treshold, out double thr))
            {
                var newSensor = new Sensor
                {
                    Name = dialogVM.Name,
                    Frequency = freq,
                    Treshold = thr,
                    Delivered = false,      // Statut initial : "En livraison"
                    Operational = false,    // Non opérationnel par défaut
                    SensorStatus = false,
                    Usage = SensorUsage.Disponible
                };

                _sensorService.AddSensor(newSensor);
                logger.Info($"Nouveau capteur ajouté avec succès à la base de donnée: Nom={newSensor.Name}, Fréquence={newSensor.Frequency}, Seuil={newSensor.Treshold}");
                RefreshSensors();
            }
            else
                logger.Warn("Tentative d'ajout de capteur échouée: Champs invalides ou annulés");
            
        }

        private void DeleteSensor()
        {
            if (SelectedSensor != null && !SelectedSensor.SensorStatus)
            {
                _sensorService.DeleteSensor(SelectedSensor);
                logger.Info($"Capteur supprimé : ID={SelectedSensor.Id}, Nom={SelectedSensor.Name}");
                RefreshSensors();
            }
        }

        private bool CanDelete()
        {
            return SelectedSensor != null && !SelectedSensor.SensorStatus;
        }

        private void AssignSensorToStation()
        {
            if (SelectedSensor == null)
            {
                System.Windows.MessageBox.Show("Veuillez sélectionner un capteur.");
                return;
            }

            if (!_userSessionService.IsEmploye)
            {
                System.Windows.MessageBox.Show("Seuls les employés peuvent assigner un capteur.");
                return;
            }

            var station = _userSessionService.AsEmploye?.Station;
            if (station == null)
            {
                System.Windows.MessageBox.Show("Aucune station n'est assignée à l'employé.");
                return;
            }

            if (!SelectedSensor.Delivered)
            {
                System.Windows.MessageBox.Show("Le capteur doit être livré avant d’être assigné.");
                return;
            }

            if (SelectedSensor.Usage != SensorUsage.Disponible)
            {
                System.Windows.MessageBox.Show("Le capteur n'est pas disponible.");
                return;
            }

            if (SelectedSensor.assignedStation != null)
            {
                System.Windows.MessageBox.Show("Le capteur est déjà assigné à une station.");
                return;
            }

            SelectedSensor.assignedStation = station;
            SelectedSensor.Usage = SensorUsage.Assigne;
            SelectedSensor.Operational = true;
            _sensorService.UpdateSensor(SelectedSensor);
            logger.Info($"Capteur assigné : {SelectedSensor.Name} à la station {station.Nom}");
            _navigationService.NavigateTo<SensorViewModel>();
            if (_navigationService.CurrentView is SensorViewModel sensorVM)
                sensorVM.RefreshSensors();

        }

        private bool CanAssignSensorToStation()
        {
            return SelectedSensor != null
                && SelectedSensor.Usage == SensorUsage.Disponible
                && SelectedSensor.Delivered
                && SelectedSensor.assignedStation == null;
        }


    }
}
