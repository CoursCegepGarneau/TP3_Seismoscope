using Moq;
using Xunit;
using Seismoscope.ViewModel;
using Seismoscope.Model;
using Seismoscope.Utils.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using Seismoscope.Model.Interfaces;
using Seismoscope.Utils.Services.Interfaces;
using Seismoscope.Services.Interfaces;


namespace SeismoscopeTest.ViewModel
{
    public class SensorManagementViewModelTests
    {
        private readonly Mock<ISensorService> _mockSensorService;
        private readonly Mock<INavigationService> _mockNavigationService;
        private readonly SensorManagementViewModel _viewModel;
        private readonly Mock<IUserSessionService> _mockUserSessionService;
        private readonly Mock<IDialogService> _mockDialogService;

        public SensorManagementViewModelTests()
        {
            _mockSensorService = new Mock<ISensorService>();
            _mockNavigationService = new Mock<INavigationService>();
            _mockUserSessionService = new Mock<IUserSessionService>();
            _mockDialogService = new Mock<IDialogService>();

            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            var sensors = new ObservableCollection<Sensor>
            {
                new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation },
                new Sensor { Id = 0002, Name = "Sensor 2", Treshold = 4.0, Frequency = 80, Delivered = false, Operational = true, SensorStatus = true, assignedStation = mockStation },
            };

            _mockSensorService.Setup(s => s.GetAllSensors()).Returns(sensors);

            _viewModel = new SensorManagementViewModel(
                _mockSensorService.Object,
                _mockNavigationService.Object,
                _mockUserSessionService.Object,
                _mockDialogService.Object
            );
        }

        [Fact]
        public void AddSensorCommand_ShouldAddSensor_WhenExecuted()
        {
            // Arrange
            var sensorList = new List<Sensor>();

            var mockSensorService = new Mock<ISensorService>();
            var mockDialogService = new Mock<IDialogService>();
            var mockNavigationService = new Mock<INavigationService>();
            var mockUserSessionService = new Mock<IUserSessionService>();
            mockSensorService.Setup(s => s.GetAllSensors())
                .Returns(() => sensorList.ToList());
            mockSensorService.Setup(s => s.AddSensor(It.IsAny<Sensor>()))
                .Callback<Sensor>(sensorList.Add);
            mockDialogService.Setup(d => d.ShowDialog(It.IsAny<object>()))
                .Returns((object dialog) =>
                {
                    if (dialog is SensorDialogViewModel vm)
                    {
                        vm.Name = "Nouveau Capteur";
                        vm.Frequency = "10,0"; // Culture FR
                        vm.Treshold = "5,0";
                        vm.ShowName = true;
                        vm.ShowFrequency = true;
                        vm.ShowTreshold = true;
                    }
                    return true;
                });

            var viewModel = new SensorManagementViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockUserSessionService.Object,
                mockDialogService.Object
            );
            var initialCount = sensorList.Count;
            viewModel.AddSensorCommand.Execute(null);
            Assert.Equal(initialCount + 1, sensorList.Count);
            mockSensorService.Verify(s => s.AddSensor(It.IsAny<Sensor>()), Times.Once);
        }

        [Fact]
        public void DeleteSensorCommand_ShouldDeleteSensor_WhenCanDelete()
        {

            var sensorToDelete = _viewModel.AllSensors.FirstOrDefault();
            Assert.NotNull(sensorToDelete);
            _viewModel.SelectedSensor = sensorToDelete;
            _viewModel.DeleteSensorCommand.Execute(null);
            _mockSensorService.Verify(s => s.DeleteSensor(sensorToDelete), Times.Once);
            _mockSensorService.Verify(s => s.GetAllSensors(), Times.Exactly(2)); // <-- correction ici
        }


        [Fact]
        public void DeleteSensorCommand_ShouldNotDeleteSensor_WhenCanNotDelete()
        {
            var sensorToDelete = _viewModel.AllSensors.FirstOrDefault();
            Assert.NotNull(sensorToDelete);
            sensorToDelete.SensorStatus = true;
            _viewModel.SelectedSensor = sensorToDelete;

            _viewModel.DeleteSensorCommand.Execute(null);
            _mockSensorService.Verify(s => s.DeleteSensor(It.IsAny<Sensor>()), Times.Never);
        }


        [Fact]
        public void DeliverSensorCommand_ShouldNotExecute_WhenNoSensorSelected()
        {
            _viewModel.SelectedSensor = null;
            _viewModel.DeliverSensorCommand.Execute(null);
            _mockSensorService.Verify(s => s.UpdateSensorDeliveryStatus(It.IsAny<Sensor>()), Times.Never);
        }

        [Fact]
        public void DeliverSensorCommand_ShouldBeDisabled_WhenSensorAlreadyDelivered()
        {
            // Arrange
            var mockSensorService = new Mock<ISensorService>();
            var mockNavigationService = new Mock<INavigationService>();
            var mockDialogService = new Mock<IDialogService>();
            var mockUserSessionService = new Mock<IUserSessionService>();

            // 👉 Évite l'exception ici
            mockSensorService.Setup(s => s.GetAllSensors()).Returns(new List<Sensor>());

            var viewModel = new SensorManagementViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockUserSessionService.Object,
                mockDialogService.Object
            );

            var sensor = new Sensor { Delivered = true };
            viewModel.SelectedSensor = sensor;

            // Act
            var canExecute = viewModel.DeliverSensorCommand.CanExecute(null);

            // Assert
            Assert.False(canExecute);
        }


    }
}

