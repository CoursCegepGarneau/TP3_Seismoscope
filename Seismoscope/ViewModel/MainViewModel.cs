using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Seismoscope.Model;
using Seismoscope.Model.Interfaces;
using Seismoscope.Utils;
using Seismoscope.Utils.Commands;
using Seismoscope.Utils.Services;
using Seismoscope.Utils.Services.Interfaces;

namespace Seismoscope.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IUserSessionService _userSessionService;

        public INavigationService NavigationService
        {
            get => _navigationService;
        }

        public IUserSessionService UserSessionService
        {
            get => _userSessionService;
        }

        public ICommand NavigateToConnectUserViewCommand { get; set; }
        public ICommand NavigateToHomeViewCommand { get; set; }

        public ICommand? DisconnectCommand { get; }

        private bool _isWelcomeVisible = true;
        public bool IsWelcomeVisible
        {
            get => _isWelcomeVisible;
            set { _isWelcomeVisible = value; OnPropertyChanged(); }
        }

        public MainViewModel(INavigationService navigationService, IUserSessionService userSessionService)
        {
            _navigationService = navigationService;
            _userSessionService = userSessionService;

            NavigateToConnectUserViewCommand = new RelayCommand(() =>
            {
                IsWelcomeVisible = false;
                NavigationService.NavigateTo<ConnectUserViewModel>();
            });
            NavigateToHomeViewCommand = new RelayCommand(() => NavigationService.NavigateTo<HomeViewModel>());
           
        }
    }
}
