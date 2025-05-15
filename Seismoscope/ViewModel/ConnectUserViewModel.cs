using System;
using System.Windows.Input;
using Seismoscope.Model;
using Seismoscope.Model.DAL;
using Seismoscope.Model.Interfaces;
using Seismoscope.Utils;
using NLog;
using Seismoscope.Utils.Commands;
using Seismoscope.Utils.Services.Interfaces;
using Seismoscope.Utils.Services;
using System.Diagnostics;

namespace Seismoscope.ViewModel
{
    public class ConnectUserViewModel : BaseViewModel
    {
        readonly static ILogger logger = LogManager.GetCurrentClassLogger();
        private readonly IUserService _userService;
        private INavigationService _navigationService;
        private IUserSessionService _userSessionService;
        private ISensorService _sensorService;
        private readonly IStationService _stationService;

        private string? _username;
        public string? Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                    ValidateProperty(nameof(Username), value!);
                }
            }
        }

        private string? _password;
        public string? Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                    ValidateProperty(nameof(Password), value!);
                }
            }
        }

        public ConnectUserViewModel(IUserService userService, INavigationService navigationService, IUserSessionService userSessionService, IStationService stationService, ISensorService sensorService)
        {
            _userService = userService;
            _navigationService = navigationService;
            _userSessionService = userSessionService;
            _stationService = stationService;
            _sensorService = sensorService;
            ConnectCommand = new RelayCommand(Connect, CanConnect);
        }

        public ICommand ConnectCommand { get; set; }
        private void Connect()
        {
            User? user = _userService.AuthenticateUser(Username!, Password!);
            if (user != null)
            {
                logger.Info($"L'utilisateur {user.Username} s'est connecté avec succès.");
                _userSessionService.ConnectedUser = user;

                if (user is Employe)
                {
                    _navigationService.NavigateTo<HomeViewModel>();
                }
                else if (user is Admin)
                {
                    var vm = new AdminDashboardViewModel(_stationService, _userSessionService, _sensorService);
                    _navigationService.NavigateTo(vm);
                }
            }
            else
            {
                logger.Warn($"Échec de la connexion pour l'utilisateur {Username}.");
                if (HasErrors)
                    logger.Warn("Erreurs de validation présentes : " + string.Join(" | ", ErrorMessages));
                
                AddError(nameof(Password), "Utilisateur ou mot de passe invalide.");
                OnPropertyChanged(nameof(ErrorMessages));
            }
        }

        private bool CanConnect()
        {
            bool allRequiredFieldsAreEntered = Username.NotEmpty() && Password.NotEmpty();
            return !HasErrors && allRequiredFieldsAreEntered;
        }

        // Valide les propriétés Username et Password
        private void ValidateProperty(string propertyName, string value)
        {
            ClearErrors(propertyName);
            switch (propertyName)
            {
                case nameof(Username):
                    if (value.Empty())
                    {
                        AddError(propertyName, "Le nom d'utilisateur est requis.");
                    }
                    else if (value.Length < 2)
                    {
                        AddError(propertyName, "Le nom d'utilisateur doit contenir au moins 2 caractères.");
                    }
                    break;
                case nameof(Password):
                    if (value.Empty())
                    {
                        AddError(propertyName, "Le mot de passe est requis.");
                    }
                    break;
            }
            OnPropertyChanged(nameof(ErrorMessages));
        }
    }
}
