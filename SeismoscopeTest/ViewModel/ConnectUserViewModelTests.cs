using Moq;
using Seismoscope.ViewModel;
using Seismoscope.Model;
using Seismoscope.Utils.Services.Interfaces;
using Seismoscope.Model.Interfaces;
using Seismoscope.Utils.Commands;
using Xunit;

namespace SeismoscopeTest.ViewModel
{
    public class ConnectUserViewModelTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<INavigationService> _mockNavigationService;
        private readonly Mock<IUserSessionService> _mockUserSessionService;
        private readonly Mock<IStationService> _mockStationService;
        private readonly Mock<ISensorService> _mockSensorService;
        private readonly ConnectUserViewModel _viewModel;

        public ConnectUserViewModelTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockNavigationService = new Mock<INavigationService>();
            _mockUserSessionService = new Mock<IUserSessionService>();
            _mockStationService = new Mock<IStationService>();
            _mockSensorService = new Mock<ISensorService>();

            _viewModel = new ConnectUserViewModel(
                _mockUserService.Object,
                _mockNavigationService.Object,
                _mockUserSessionService.Object,
                _mockStationService.Object,
                _mockSensorService.Object);
        }

        [Fact]
        public void ConnectCommand_ShouldNavigateToHome_WhenUserIsEmploye()
        {
            // Arrange
            var employe = new Employe { Username = "user", Password = "password" };
            _mockUserService.Setup(e => e.AuthenticateUser("user", "password")).Returns(employe);

            // Act
            _viewModel.Username = "user";
            _viewModel.Password = "password";
            _viewModel.ConnectCommand.Execute(null);

            // Assert
            _mockUserSessionService.VerifySet(s => s.ConnectedUser = It.Is<Employe>(e => e.Username == "user" && e.Password == "password"), Times.Once);
            _mockNavigationService.Verify(n => n.NavigateTo<HomeViewModel>(), Times.Once);
        }

        [Fact]
        public void ConnectCommand_ShouldNavigateToAdminDashboard_WhenUserIsAdmin()
        {
            // Arrange
            var admin = new Admin { Username = "admin", Password = "adminPassword" };

            _mockUserService.Setup(a => a.AuthenticateUser("admin", "adminPassword")).Returns(admin);
            _mockNavigationService.Setup(n => n.NavigateTo<AdminDashboardViewModel>());

            // Act
            _viewModel.Username = "admin";
            _viewModel.Password = "adminPassword";
            _viewModel.ConnectCommand.Execute(null);

            // Assert
            _mockUserSessionService.VerifySet(s => s.ConnectedUser = It.Is<Admin>(e => e.Username == "admin" && e.Password == "adminPassword"), Times.Once);
            _mockNavigationService.Verify(n => n.NavigateTo(It.IsAny<AdminDashboardViewModel>()), Times.Once);

        }


        [Fact]
        public void ConnectCommand_ShouldAddError_WhenUserNotFound()
        {
            // Arrange
            _mockUserService.Setup(u => u.AuthenticateUser("resu", "drowssap")).Returns((User?)null);

            // Act
            _viewModel.Username = "resu";
            _viewModel.Password = "drowssap";
            _viewModel.ConnectCommand.Execute(null);

            // Assert
            Assert.Contains("Utilisateur ou mot de passe invalide", _viewModel.ErrorMessages);
        }

        [Fact]
        public void CanConnect_ShouldReturnFalse_WhenUsernameOrPasswordIsEmpty()
        {
            // Arrange
            _viewModel.Username = "";
            _viewModel.Password = "password";

            // Act and Assert
            Assert.False(_viewModel.ConnectCommand.CanExecute(null));

            // Arrange
            _viewModel.Username = "user";
            _viewModel.Password = "";

            // Act and Assert
            Assert.False(_viewModel.ConnectCommand.CanExecute(null));
        }

        [Fact]
        public void CanConnect_ShouldReturnTrue_WhenUsernameAndPasswordAreValid()
        {
            // Arrange
            _viewModel.Username = "user";
            _viewModel.Password = "password";

            // Act and Assert
            Assert.True(_viewModel.ConnectCommand.CanExecute(null));
        }
    }
}
