using Moq;
using Seismoscope.Model.Interfaces;
using Seismoscope.Model;
using Seismoscope.Utils.Services.Interfaces;
using Seismoscope.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeismoscopeTest.ViewModel
{
    public class SensorReadingViewModelTests
    {
        [Fact]
        public void Constructor_ShouldInitializeSensors_FromSensorService()
        {
            // Arrange
            var mockSensorService = new Mock<ISensorService>();
            var mockNavigationService = new Mock<INavigationService>();
            var mockUserSessionService = new Mock<IUserSessionService>();
            var mockAdjustementService = new Mock<ISensorAdjustementService>();

            var station = new Station { Id = 1 };
            var sensors = new List<Sensor>
            {
                new Sensor { Id = 1, Frequency = 10 },
                new Sensor { Id = 2, Frequency = 20 }
            };

            mockUserSessionService.Setup(us => us.AsEmploye)
                .Returns(new Employe { Station = station });

            mockSensorService.Setup(s => s.GetSensorByStationId(station.Id)).Returns(sensors);

            mockSensorService.Setup(s => s.GetAllSensors()).Returns(sensors);

            // Act
            var vm = new SensorReadingViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockUserSessionService.Object,
                mockAdjustementService.Object
            );

            // Assert
            Assert.NotNull(vm.Sensors);
            Assert.Equal(2, vm.Sensors.Count);
        }


        [Fact]
        public void RefreshSensors_ShouldUpdateSensorsFromStation()
        {
            // Arrange
            var mockSensorService = new Mock<ISensorService>();
            var mockNavigationService = new Mock<INavigationService>();
            var mockUserSessionService = new Mock<IUserSessionService>();
            var mockAdjustementService = new Mock<ISensorAdjustementService>();

            var station = new Station { Id = 123 };
            var sensors = new List<Sensor>
        {
            new Sensor { Id = 1, Frequency = 10 },
        };

            mockUserSessionService.Setup(us => us.AsEmploye)
                .Returns(new Employe { Station = station });

            mockSensorService.Setup(s => s.GetAllSensors())
                .Returns(new List<Sensor>());

            mockSensorService.Setup(s => s.GetSensorByStationId(station.Id))
                .Returns(sensors);

            var vm = new SensorReadingViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockUserSessionService.Object,
                mockAdjustementService.Object
            );

            // Act
            vm.RefreshSensors();

            // Assert
            Assert.Single(vm.Sensors);
            Assert.Equal(10, vm.Sensors[0].Frequency);
        }

        [Fact]
        public void GoBackCommand_ShouldCallNavigationService()
        {
            // Arrange
            var mockSensorService = new Mock<ISensorService>();
            var mockNavigationService = new Mock<INavigationService>();
            var mockUserSessionService = new Mock<IUserSessionService>();
            var mockAdjustementService = new Mock<ISensorAdjustementService>();

            mockSensorService.Setup(s => s.GetAllSensors()).Returns(new List<Sensor>());

            var vm = new SensorReadingViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockUserSessionService.Object,
                mockAdjustementService.Object
            );

            // Act
            vm.GoBackCommand.Execute(null);

            // Assert
            mockNavigationService.Verify(ns => ns.NavigateTo<SensorViewModel>(), Times.Once);
        }

        [Fact]
        public void OnNavigated_WithSensorParameter_ShouldSetSelectedSensor()
        {
            // Arrange
            var sensor = new Sensor { Id = 99, Frequency = 5 };

            var mockSensorService = new Mock<ISensorService>();
            var mockNavigationService = new Mock<INavigationService>();
            var mockUserSessionService = new Mock<IUserSessionService>();
            var mockAdjustementService = new Mock<ISensorAdjustementService>();

            mockSensorService.Setup(s => s.GetAllSensors()).Returns(new List<Sensor>());

            var vm = new SensorReadingViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockUserSessionService.Object,
                mockAdjustementService.Object
            );

            // Act
            vm.OnNavigated(sensor);

            // Assert
            Assert.Equal(sensor, vm.SelectedSensor);
        }
    }
}
