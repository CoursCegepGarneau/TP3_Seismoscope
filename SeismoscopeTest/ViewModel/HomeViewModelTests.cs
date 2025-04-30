using Moq;
using Seismoscope.ViewModel;
using Seismoscope.Utils.Services.Interfaces;
using Seismoscope.Model;
using System;
using Xunit;
using Seismoscope.Model.Interfaces;
using System.Globalization;
using System.Threading;

namespace SeismoscopeTest.ViewModel
{
    public class HomeViewModelTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<INavigationService> _mockNavigationService;
        private readonly Mock<IUserSessionService> _mockUserSessionService;
        private readonly HomeViewModel _viewModel;

        public HomeViewModelTests()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-CA");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("fr-CA");
            _mockUserService = new Mock<IUserService>();
            _mockNavigationService = new Mock<INavigationService>();
            _mockUserSessionService = new Mock<IUserSessionService>();

            _viewModel = new HomeViewModel(
                _mockUserService.Object,
                _mockNavigationService.Object,
                _mockUserSessionService.Object);
        }

        [Fact]
        public void StationInformations_ShouldReturnCorrectInformation()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-CA");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("fr-CA");
            var user = new Employe
            {
                Station = new Station
                {
                    Nom = "Station A",
                    Région = "Québec",
                    Latitude = 45.5,
                    Longitude = -73.6
                }
            };
            _mockUserSessionService.Setup(s => s.ConnectedUser).Returns(user);
            _mockUserSessionService.Setup(s => s.AsEmploye).Returns(user);

            var result = _viewModel.StationInformations;
            Assert.Contains("📍 Station assignée :", result);
            Assert.Contains("Nom       : Station A", result);
            Assert.Contains("Région    : Québec", result);
            Assert.Contains("Latitude  : 45,5", result); 
            Assert.Contains("Longitude : -73,6", result); 
        }

        [Fact]
        public void StationInformations_ShouldReturnNoStationMessage_WhenNoStationAssigned()
        {
            // Arrange
            var user = new Employe { Station = null };
            _mockUserSessionService.Setup(s => s.ConnectedUser).Returns(user);
            _mockUserSessionService.Setup(s => s.AsEmploye).Returns(user);

            // Act
            var result = _viewModel.StationInformations;

            // Assert
            Assert.Equal("Aucune station assignée.", result);
        }

        [Fact]
        public void LogoutButton_ShouldNavigateToConnectUserViewModel_WhenUsed()
        {
            // Arrange
            var user = new Admin { Prenom = "John" };
            _mockUserSessionService.Setup(s => s.ConnectedUser).Returns(user);
            _mockUserSessionService.Setup(s => s.IsUserConnected).Returns(true);

            // Act
            _viewModel.LogoutCommand.Execute(null);

            // Assert
            _mockUserSessionService.VerifySet(s => s.ConnectedUser = null, Times.Once);
            _mockNavigationService.Verify(n => n.NavigateTo<ConnectUserViewModel>(), Times.Once);
        }

        [Fact]
        public void NavigateToSensorViewButton_ShouldNavigateToSensorViewModel_WhenUsed()
        {
            // Act
            _viewModel.NavigateToSensorViewCommand.Execute(null);

            // Assert
            _mockNavigationService.Verify(n => n.NavigateTo<SensorViewModel>(), Times.Once);
        }

        [Fact]
        public void IsAdmin_ShouldReturnTrue_WhenUserIsAdmin()
        {
            // Arrange
            var admin = new Admin
            {
                Prenom = "John",
                Nom = "Doe",
                Username = "johndoe",
                Password = "password123"
            };

            _mockUserSessionService.Setup(s => s.ConnectedUser).Returns(admin);
            _mockUserSessionService.Setup(s => s.IsAdmin).Returns(true);

            // Act
            var result = _viewModel.IsAdmin;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEmploye_ShouldReturnTrue_WhenUserIsEmploye()
        {
            // Arrange
            var employe = new Employe();
            _mockUserSessionService.Setup(s => s.ConnectedUser).Returns(employe);
            _mockUserSessionService.Setup(s => s.IsEmploye).Returns(true);

            // Act
            var result = _viewModel.IsEmploye;

            // Assert
            Assert.True(result);
        }
    }
}
