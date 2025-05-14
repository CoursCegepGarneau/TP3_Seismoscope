using Seismoscope.Model;
using Seismoscope.Utils.Services.Interfaces;
using System.Collections.ObjectModel;
using Seismoscope.Model.Interfaces;
using Seismoscope.Utils;
using NLog;
using Seismoscope.Utils.Commands;
using Seismoscope.Utils.Services;
using System.Windows.Input;
using System.Text;
using Seismoscope.Model.Services;

namespace Seismoscope.ViewModel
{
    public class AdminDashboardViewModel : BaseViewModel
    {
        readonly static ILogger logger = LogManager.GetCurrentClassLogger();
        private readonly IStationService _stationService;
        private readonly IUserSessionService _userSession;
        private readonly ISensorService _sensorService;
        private Station? _selectedStation;
        private ObservableCollection<Sensor> _selectedStationSensors =null!;

        public Station? SelectedStation
        {
            get => _selectedStation;
            set
            {
                if (_selectedStation != value)
                {
                    _selectedStation = value;
                    OnPropertyChanged(nameof(SelectedStation));
                    RefreshSensors();
                }
            }
        }
        public ObservableCollection<Sensor> SelectedStationSensors
        {
            get => _selectedStationSensors;
            set
            {
                _selectedStationSensors = value;
                OnPropertyChanged();
            }
        }


        public ObservableCollection<Station> Stations { get; set; } = new();

        public AdminDashboardViewModel(IStationService stationService, IUserSessionService userSession, ISensorService sensorService)
        {
            _stationService = stationService;
            _userSession = userSession;
            _sensorService = sensorService;

            LoadStations();
        }

        private void LoadStations()
        {
            if (_userSession.ConnectedUser is Admin)
            {
                var stations = _stationService.GetAllStations();
                Stations = new ObservableCollection<Station>(stations);
                OnPropertyChanged(nameof(Stations));
            }
        }

        private void RefreshSensors()
        {
            var station = SelectedStation;

            if (station != null)
            {
                var sensors = _sensorService.GetSensorByStationId(station.Id);
                logger.Info($"Station sélectionnée : {station.Nom} ({sensors.Count()} capteurs).");
                SelectedStationSensors = new ObservableCollection<Sensor>(sensors);
            }
            else
            {
                SelectedStationSensors = new ObservableCollection<Sensor>();
            }

            OnPropertyChanged(nameof(SelectedStationSensors));
        }
    }

}

