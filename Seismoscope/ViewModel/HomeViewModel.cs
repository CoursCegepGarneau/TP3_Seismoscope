using Seismoscope.Model.Interfaces;
using NLog;
using Seismoscope.Utils;
using Seismoscope.Utils.Commands;
using Seismoscope.Utils.Services.Interfaces;
using Seismoscope.Utils.Services;
using System.Windows.Input;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Seismoscope.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        readonly static Logger logger = LogManager.GetCurrentClassLogger();
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
                {
                    logger.Warn($"L'utilisateur {_userSessionService.ConnectedUser?.Username?? "Inconnu"} est connecté sans station assignée.");
                    return "Aucune station assignée.";
                }


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
        public ICommand NavigateToSensorViewCommand { get; }
        public HomeViewModel(IUserService userService, INavigationService navigationService, IUserSessionService userSessionService)
        {
            logger.Info("Connexion de l'utilisateur à l'accueil");
            _userService = userService;
            _navigationService = navigationService;
            _userSessionService = userSessionService;

            LogoutCommand = new RelayCommand(Logout, CanLogout);
            OnPropertyChanged(nameof(WelcomeMessage));
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
            logger.Info("Déconnexion de l'utilisateur.");

        }

        private bool CanLogout()
        {
            return _userSessionService.IsUserConnected;
        }
    }
}
