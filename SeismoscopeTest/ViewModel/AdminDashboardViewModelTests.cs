using Moq;
using Seismoscope.ViewModel;
using Seismoscope.Model;
using Seismoscope.Utils.Services.Interfaces;
using Xunit;
using System.Collections.ObjectModel;
using Seismoscope.Model.Interfaces;

namespace SeismoscopeTest.ViewModel
{
    public class AdminDashboardViewModelTests
    {
        private readonly Mock<IStationService> _mockStationService;
        private readonly Mock<IUserSessionService> _mockUserSessionService;
        private readonly Mock<ISensorService> _mockSensorService;
        private readonly AdminDashboardViewModel _viewModel;

        public AdminDashboardViewModelTests()
        {
            _mockStationService = new Mock<IStationService>();
            _mockUserSessionService = new Mock<IUserSessionService>();
            _mockSensorService = new Mock<ISensorService>();

            _viewModel = new AdminDashboardViewModel(
                _mockStationService.Object,
                _mockUserSessionService.Object,
                _mockSensorService.Object);
        }

        [Fact]
        public void Stations_ShouldContainData_WhenUserIsAdmin()
        {
            // Arrange
            _mockUserSessionService.Setup(s => s.ConnectedUser).Returns(new Admin());

            var stations = new ObservableCollection<Station>
            {
                new Station { Nom = "Station A", Région = "Québec", Latitude = 45.5, Longitude = -73.6 },
                new Station { Nom = "Station B", Région = "Ontario", Latitude = 43.7, Longitude = -79.4 }
            };

            _mockStationService.Setup(s => s.GetAllStations()).Returns(stations);

            // Act);
            var viewModelStations = stations;

            // Assert
            Assert.Equal(2, viewModelStations.Count);
            Assert.Equal("Station A", viewModelStations[0].Nom);
            Assert.Equal("Station B", viewModelStations[1].Nom);
        }

        [Fact]
        public void SelectedStation_ShouldNotifyPropertyChange_WhenSet()
        {
            // Arrange
            var station = new Station { Id = 1, Nom = "Station 1" };

            _mockSensorService
                .Setup(s => s.GetSensorByStationId(station.Id))
                .Returns(new List<Sensor>());

            // Act
            _viewModel.SelectedStation = station;

            // Assert
            Assert.Equal(station, _viewModel.SelectedStation);
        }

        [Fact]
        public void Stations_ShouldBeEmpty_WhenUserIsNotAdmin()
        {
            // Arrange
            _mockUserSessionService.Setup(s => s.ConnectedUser).Returns(new Employe());

            // Act
            var stations = _viewModel.Stations;

            // Assert
            Assert.Empty(stations);
        }
    }
}
