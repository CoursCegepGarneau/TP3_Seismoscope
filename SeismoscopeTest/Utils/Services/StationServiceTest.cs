using Moq;
using Seismoscope.Model;
using Seismoscope.Data.Repositories.Interfaces;
using Seismoscope.Model.Services;
using System.Collections.Generic;
using Xunit;

namespace SeismoscopeTest.Model.Services
{
    public class StationServiceTests
    {
        private readonly Mock<IStationRepository> _mockRepository;
        private readonly StationService _stationService;

        public StationServiceTests()
        {
            _mockRepository = new Mock<IStationRepository>();
            _stationService = new StationService(_mockRepository.Object);
        }

        [Fact]
        public void GetAllStations_ShouldReturnAllStations()
        {
            // Arrange
            var stations = new List<Station>
            {
                new Station { Nom = "Station A", Région = "Québec", Latitude = 45.5, Longitude = -73.6 },
                new Station { Nom = "Station B", Région = "Ontario", Latitude = 43.7, Longitude = -79.4 }
            };

            _mockRepository.Setup(r => r.GetAll()).Returns(stations);

            // Act
            var result = _stationService.GetAllStations();

            // Assert
            Assert.Equal(2, ((List<Station>)result).Count);
        }

        [Fact]
        public void GetStationById_ShouldReturnCorrectStation()
        {
            // Arrange
            var station = new Station {Nom = "Station A", Région = "Québec", Latitude = 45.5, Longitude = -73.6 };
            _mockRepository.Setup(r => r.GetById(1)).Returns(station);

            // Act
            var result = _stationService.GetStationById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Station A", result.Nom);
        }

        [Fact]
        public void AddStation_ShouldCallAddStationMethod()
        {
            // Arrange
            var station = new Station { Nom = "Station A", Région = "Québec", Latitude = 45.5, Longitude = -73.6 };

            // Act
            _stationService.AddStation(station);

            // Assert
            _mockRepository.Verify(r => r.Add(station), Times.Once);
        }

        [Fact]
        public void UpdateStation_ShouldCallRepositoryUpdateStationMethod()
        {
            // Arrange
            var station = new Station { Nom = "Station A", Région = "Québec", Latitude = 45.5, Longitude = -73.6 };

            // Act
            _stationService.UpdateStation(station);

            // Assert
            _mockRepository.Verify(r => r.Update(station), Times.Once);
        }

        [Fact]
        public void DeleteStation_ShouldCallRepositoryDeleteStationMethod()
        {
            // Arrange
            var station = new Station { Id = 1, Nom = "Station A", Région = "Québec", Latitude = 45.5, Longitude = -73.6 };
            _stationService.AddStation(station);

            // Act
            _stationService.DeleteStation(1);

            // Assert
            _mockRepository.Verify(r => r.Delete(1), Times.Once);
        }
    }
}
