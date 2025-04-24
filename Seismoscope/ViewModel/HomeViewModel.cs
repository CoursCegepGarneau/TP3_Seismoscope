using Seismoscope.Model.Interfaces;
using Seismoscope.Utils;
using Seismoscope.Utils.Commands;
using Seismoscope.Utils.Services.Interfaces;
using Seismoscope.Utils.Services;
using System.Windows.Input;
using System.Text;

namespace Seismoscope.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;
        private readonly IUserSessionService _userSessionService;

        public bool IsAdmin => _userSessionService.IsAdmin;
        public bool IsEmploye => _userSessionService.IsEmploye;

        public User? ConnectedUser
        {
            get => _userSessionService.ConnectedUser;
        }

        public string WelcomeMessage
        {
            get => ConnectedUser == null
                ? "Bienvenue chère personne inconnue!"
                : $"Bienvenue {ConnectedUser.Prenom}!";
        }

        

        public string StationInformations
        {
            get
            {
                var station = _userSessionService.AsEmploye?.Station;

                if (station == null)
                    return "Aucune station assignée.";

                var sb = new StringBuilder();
                sb.AppendLine("📍 Station assignée :");
                sb.AppendLine($"Nom       : {station.Nom}");
                sb.AppendLine($"Région    : {station.Région}");
                sb.AppendLine($"Latitude  : {station.Latitude}");
                sb.AppendLine($"Longitude : {station.Longitude}");

                return sb.ToString();
            }
        }



        public ICommand LogoutCommand { get; set; }
        public ICommand NavigateToSensorViewCommand { get; } // !!!**!! PÖSSIBLEMENT TEMPORAIRE, DEBUG SENSOR VIEW SECTION


        public HomeViewModel(IUserService userService, INavigationService navigationService, IUserSessionService userSessionService)
        {
            _userService = userService;
            _navigationService = navigationService;
            _userSessionService = userSessionService;

            LogoutCommand = new RelayCommand(Logout, CanLogout);

            // 👇 Notifie la vue que les propriétés doivent être rafraîchies
            OnPropertyChanged(nameof(WelcomeMessage));

            ////OnPropertyChanged(nameof(StationDetails));
            OnPropertyChanged(nameof(StationInformations));

            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(IsEmploye));

            NavigateToSensorViewCommand = new RelayCommand(() =>
            {
                _navigationService.NavigateTo<SensorViewModel>();
            });

        }

        private void Logout()
        {
            _userSessionService.ConnectedUser = null;
            _navigationService.NavigateTo<ConnectUserViewModel>();
        }

        private bool CanLogout()
        {
            return _userSessionService.IsUserConnected;
        }
    }
}
