using Moq;
using Seismoscope.ViewModel;
using Seismoscope.Utils.Services.Interfaces;
using System;
using Xunit;

namespace SeismoscopeTest.ViewModel
{
    public class MainViewModelTests
    {
        private readonly Mock<INavigationService> _mockNavigationService;
        private readonly Mock<IUserSessionService> _mockUserSessionService;
        private readonly MainViewModel _viewModel;

        public MainViewModelTests()
        {
            _mockNavigationService = new Mock<INavigationService>();
            _mockUserSessionService = new Mock<IUserSessionService>();
            _viewModel = new MainViewModel(_mockNavigationService.Object, _mockUserSessionService.Object);
        }

        [Fact]
        public void NavigateToConnectUserViewCommand_ShouldNavigateToConnectUserView()
        {
            // Arrange
            _mockNavigationService.Setup(n => n.NavigateTo<ConnectUserViewModel>()).Verifiable();

            // Act
            _viewModel.NavigateToConnectUserViewCommand.Execute(null);

            // Assert
            _mockNavigationService.Verify(n => n.NavigateTo<ConnectUserViewModel>(), Times.Once);
        }

        [Fact]
        public void NavigateToHomeViewCommand_ShouldNavigateToHomeView()
        {
            // Arrange
            _mockNavigationService.Setup(n => n.NavigateTo<HomeViewModel>()).Verifiable();

            // Act
            _viewModel.NavigateToHomeViewCommand.Execute(null);

            // Assert
            _mockNavigationService.Verify(n => n.NavigateTo<HomeViewModel>(), Times.Once);
        }

        [Fact]
        public void IsWelcomeVisible_ShouldBeFalse_AfterNavigateToConnectUserViewCommandExecuted()
        {
            // Act
            _viewModel.NavigateToConnectUserViewCommand.Execute(null);

            // Assert
            Assert.False(_viewModel.IsWelcomeVisible);
        }
    }
}
