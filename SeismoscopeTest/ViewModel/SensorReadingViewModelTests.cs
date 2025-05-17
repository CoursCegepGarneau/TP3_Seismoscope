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
            var mockHistoryService = new Mock<IHistoryService>();

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
                mockHistoryService.Object,
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
            var mockHistoryService = new Mock<IHistoryService>();


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
                mockHistoryService.Object,
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
            var mockHistoryService = new Mock<IHistoryService>();


            mockSensorService.Setup(s => s.GetAllSensors()).Returns(new List<Sensor>());

            var vm = new SensorReadingViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockUserSessionService.Object,
                mockHistoryService.Object,
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
            var mockHistoryService = new Mock<IHistoryService>();


            mockSensorService.Setup(s => s.GetAllSensors()).Returns(new List<Sensor>());

            var vm = new SensorReadingViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockUserSessionService.Object,
                mockHistoryService.Object,
                mockAdjustementService.Object
            );

            // Act
            vm.OnNavigated(sensor);

            // Assert
            Assert.Equal(sensor, vm.SelectedSensor);
        }


        [Fact]
        public void TraiterLigne_ShouldAddAmplitudeAndEvent_IfAboveThreshold()
        {
            // Arrange
            var mockSensorService = new Mock<ISensorService>();
            var mockNavigationService = new Mock<INavigationService>();
            var mockUserSessionService = new Mock<IUserSessionService>();
            var mockAdjustementService = new Mock<ISensorAdjustementService>();
            var mockHistoryService = new Mock<IHistoryService>();

            mockSensorService.Setup(s => s.GetAllSensors()).Returns(new List<Sensor>());

            // Retourne une liste vide de messages pour éviter le null
            mockAdjustementService
                .Setup(a => a.AdjustSensors(It.IsAny<SeismicEvent>(), It.IsAny<Sensor>()))
                .Returns(new List<string>());

            var vm = new SensorReadingViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockUserSessionService.Object,
                mockHistoryService.Object,
                mockAdjustementService.Object
            );

            vm.SelectedSensor = new Sensor { Treshold = 10, Id = 1, Name = "Sensor 1" };

            var seismicEvent = new SeismicEvent
            {
                Amplitude = 20,
                TypeOnde = "P"
            };

            // Act
            vm.TraiterLigne(0, seismicEvent);

            // Assert
            Assert.Single(vm.Amplitudes);
            Assert.Single(vm.Timestamps);
            Assert.Single(vm.EvenementsFiltres);
            Assert.Empty(vm.MessagesUI); 

            mockAdjustementService.Verify(a => a.AdjustSensors(seismicEvent, vm.SelectedSensor), Times.Once);
            mockHistoryService.Verify(h => h.AjouterHistory(It.IsAny<HistoriqueEvenement>()), Times.Once);
        }


        [Fact]
        public void EventHistoryViewModel_ShouldLoadHistorySuccessfully()
        {
            // Arrange
            var mockSensorService = new Mock<ISensorService>();
            var mockNavigationService = new Mock<INavigationService>();
            var mockUserSessionService = new Mock<IUserSessionService>();
            var mockHistoryService = new Mock<IHistoryService>();
            var mockDialogService = new Mock<IDialogService>();

            // Simuler une liste vide de capteurs au minimum
            mockSensorService.Setup(s => s.GetAllSensors()).Returns(new List<Sensor>());

            // Simuler un historique
            mockHistoryService.Setup(h => h.GetAllHistory()).Returns(new List<HistoriqueEvenement>
            {
                new HistoriqueEvenement
                {
                    DateHeure = DateTime.Now,
                    Amplitude = 25,
                    TypeOnde = "P",
                    SeuilAuMoment = 20,
                    SensorName = "Capteur X",
                }
            });

            // Act
            var vm = new EventHistoryViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockUserSessionService.Object,
                mockHistoryService.Object
            );

            // Assert
            Assert.Single(vm.AllHistory);
            Assert.Contains("Capteur X", vm.AllHistory.First().SensorName);
        }


    }
}
