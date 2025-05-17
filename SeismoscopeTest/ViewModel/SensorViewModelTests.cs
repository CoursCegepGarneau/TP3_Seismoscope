using Moq;
using Xunit;
using Seismoscope.ViewModel;
using Seismoscope.Model.Interfaces;
using Seismoscope.Utils.Commands;

using System.Collections.ObjectModel;
using Seismoscope.Model;
using Seismoscope.Utils;
using Seismoscope.Utils.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Seismoscope.Utils.Enums;


namespace SeismoscopeTest.ViewModel
{
    public class SensorViewModelTests
    {
        private readonly Mock<ISensorService> _mockSensorService;
        private readonly Mock<INavigationService> _mockNavigationService;
        private readonly Mock<IDialogService> _mockDialogService;
        private readonly SensorViewModel _viewModel;
        private readonly Mock<IUserSessionService> _mockUserSessionService;

        public SensorViewModelTests()
        {
            _mockSensorService = new Mock<ISensorService>();
            _mockNavigationService = new Mock<INavigationService>();
            _mockDialogService = new Mock<IDialogService>();
            _mockUserSessionService = new Mock<IUserSessionService>();

            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            _mockSensorService.Setup(s => s.GetAllSensors()).Returns(new ObservableCollection<Sensor>
            {
                new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation },
                new Sensor { Id = 0002, Name = "Sensor 2", Treshold = 4.0, Frequency = 80, Delivered = false, Operational = true, SensorStatus = true, assignedStation = mockStation }
            });

            _viewModel = new SensorViewModel(
                _mockSensorService.Object,
                _mockNavigationService.Object,
                _mockDialogService.Object,
                _mockUserSessionService.Object);
        }

        [Fact]
        public void AddSensorCommand_ShouldNavigateToSensorManagementViewModel_WhenExecuted()
        {
            // Arrange
            var mockNavService = new Mock<INavigationService>();
            var mockUserSessionService = new Mock<IUserSessionService>();

            var viewModel = new SensorViewModel(
                _mockSensorService.Object,
                mockNavService.Object,
                _mockDialogService.Object,
                mockUserSessionService.Object
            );

            // Act
            viewModel.AddSensorCommand.Execute(null);

            // Assert
            mockNavService.Verify(n => n.NavigateTo<SensorManagementViewModel>(), Times.Once);
            mockUserSessionService.VerifySet(s => s.IsAssignationMode = true, Times.Once);
        }


        [Fact]
        public void AddSensorCommand_ShouldAddSensorToSensorList_WhenDialogIsConfirmed()
        {
            // Arrange
            var station = new Station { Id = 1, Nom = "Station A" };
            var employe = new Employe { Station = station };
            var sensorList = new List<Sensor>();
            var mockSensorService = new Mock<ISensorService>();
            var mockDialogService = new Mock<IDialogService>();
            var mockNavigationService = new Mock<INavigationService>();
            var mockUserSessionService = new Mock<IUserSessionService>();

            mockUserSessionService.Setup(s => s.IsEmploye).Returns(true);
            mockUserSessionService.Setup(s => s.AsEmploye).Returns(employe);
            mockSensorService.Setup(s => s.GetSensorByStationId(It.IsAny<int>()))
                .Returns(() => sensorList.Where(s => s.assignedStation?.Id == station.Id).ToList());
            mockSensorService.Setup(s => s.GetAllSensors())
                .Returns(sensorList);
            mockSensorService.Setup(s => s.AddSensor(It.IsAny<Sensor>()))
                .Callback<Sensor>(sensorList.Add);
            mockDialogService.Setup(d => d.ShowDialog(It.IsAny<object>()))
                .Returns((object dialog) =>
                {
                    if (dialog is SensorDialogViewModel vm)
                    {
                        vm.ShowName = true;
                        vm.ShowFrequency = true;
                        vm.ShowTreshold = true;
                        vm.Name = "Sensor 1";
                        vm.Frequency = "8.5";
                        vm.Treshold = "1.3";
                    }
                    return true;
                });

            var sensorViewModel = new SensorViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockDialogService.Object,
                mockUserSessionService.Object
            );
            var newSensor = new Sensor
            {
                Name = "Sensor 1",
                Frequency = 8.5,
                Treshold = 1.3,
                Delivered = true,
                Usage = SensorUsage.Assigne,
                Operational = true,
                assignedStation = station
            };
            sensorList.Add(newSensor);

            sensorViewModel.RefreshSensors();
            Assert.Single(sensorViewModel.Sensors);
            Assert.Equal("Sensor 1", sensorViewModel.Sensors.First().Name);
        }


        [Fact]
        public void AssignSensorToStation_ShouldUpdateSensor_WhenEligible()
        {
            var station = new Station { Id = 1, Nom = "Station A" };
            var employe = new Employe { Station = station };
            var sensor = new Sensor
            {
                Id = 1,
                Name = "Sensor 1",
                Delivered = true,
                Usage = SensorUsage.Disponible,
                Operational = false,
                SensorStatus = false,
                assignedStation = null
            };

            var mockSensorService = new Mock<ISensorService>();
            var mockDialogService = new Mock<IDialogService>();
            var mockNavigationService = new Mock<INavigationService>();
            var mockUserSessionService = new Mock<IUserSessionService>();
            mockSensorService.Setup(s => s.GetAllSensors())
                .Returns(new List<Sensor>());

            mockUserSessionService.Setup(s => s.IsEmploye).Returns(true);
            mockUserSessionService.Setup(s => s.AsEmploye).Returns(employe);

            bool updateCalled = false;
            mockSensorService.Setup(s => s.UpdateSensor(It.IsAny<Sensor>()))
                .Callback<Sensor>(_ => updateCalled = true);

            var viewModel = new SensorManagementViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockUserSessionService.Object,
                mockDialogService.Object
            );

            viewModel.SelectedSensor = sensor;

            // Act
            viewModel.AssignSensorToStationCommand.Execute(null);

            // Assert
            Assert.True(updateCalled, "UpdateSensor n’a pas été appelé.");
            Assert.Equal(SensorUsage.Assigne, sensor.Usage);
            Assert.True(sensor.Operational);
            Assert.Equal(station, sensor.assignedStation);
        }

        [Fact]
        public void SensorViewModel_ShouldLoadAssignedSensors_ForEmployeeStation()
        {
            // Arrange
            var station = new Station { Id = 1, Nom = "Station A" };
            var employe = new Employe { Station = station };

            var sensorAssigné = new Sensor
            {
                Id = 1,
                Name = "Sensor 1",
                assignedStation = station,
                Usage = SensorUsage.Assigne
            };

            var mockSensorService = new Mock<ISensorService>();
            var mockDialogService = new Mock<IDialogService>();
            var mockNavigationService = new Mock<INavigationService>();
            var mockUserSessionService = new Mock<IUserSessionService>();

            mockUserSessionService.Setup(s => s.AsEmploye).Returns(employe);
            mockSensorService.Setup(s => s.GetAllSensors())
                .Returns(new List<Sensor>());
            mockSensorService.Setup(s => s.GetSensorByStationId(It.IsAny<int>()))
                .Returns(new List<Sensor> { sensorAssigné });

            var viewModel = new SensorViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockDialogService.Object,
                mockUserSessionService.Object
            );

            Assert.Single(viewModel.Sensors);
            var capteur = viewModel.Sensors.First();
            Assert.Equal(sensorAssigné.Id, capteur.Id);
            Assert.Equal("Sensor 1", capteur.Name);
            Assert.Equal(station, capteur.assignedStation);
        }


        [Fact]
        public void EndToEnd_AddAndAssignSensor_ShouldAppearInSensorList()
        {
            var station = new Station { Id = 1, Nom = "Station A" };
            var employe = new Employe { Station = station };

            var sensorList = new List<Sensor>();

            var mockSensorService = new Mock<ISensorService>();
            var mockDialogService = new Mock<IDialogService>();
            var mockNavigationService = new Mock<INavigationService>();
            var mockUserSessionService = new Mock<IUserSessionService>();

            mockUserSessionService.Setup(s => s.IsEmploye).Returns(true);
            mockUserSessionService.Setup(s => s.AsEmploye).Returns(employe);
            mockUserSessionService.SetupProperty(s => s.IsAssignationMode, false);

            // Retourne toujours la liste de capteurs globale
            mockSensorService.Setup(s => s.GetAllSensors())
                .Returns(() => sensorList.ToList());

            mockSensorService.Setup(s => s.GetSensorByStationId(It.IsAny<int>()))
                .Returns((int stationId) => sensorList.Where(s => s.assignedStation?.Id == stationId).ToList());

            // Ajout du capteur dans la "base"
            mockSensorService.Setup(s => s.AddSensor(It.IsAny<Sensor>()))
                .Callback<Sensor>(sensorList.Add);

            // Mise à jour simulée du capteur
            mockSensorService.Setup(s => s.UpdateSensor(It.IsAny<Sensor>())).Callback<Sensor>(s => {});

            // Simule le dialogue d’ajout (avec des valeurs valides)
            mockDialogService.Setup(d => d.ShowDialog(It.IsAny<object>()))
                .Returns((object dialog) =>
                {
                    if (dialog is SensorDialogViewModel vm)
                    {
                        vm.ShowName = true;
                        vm.ShowFrequency = true;
                        vm.ShowTreshold = true;
                        vm.Name = "Sensor 1";
                        vm.Frequency = "8,5";
                        vm.Treshold = "1,3";
                    }
                    return true;
                });

            // Ajout via SensorManagement
            var managementVM = new SensorManagementViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockUserSessionService.Object,
                mockDialogService.Object
            );

            managementVM.AddSensorCommand.Execute(null);

            // Vérifie qu’un capteur a été ajouté
            Assert.Single(sensorList);
            var sensor = sensorList.First();

            // Livraison en livré 
            sensor.Delivered = true;

            // 3 : assignation à la station
            managementVM.SelectedSensor = sensor;
            managementVM.AssignSensorToStationCommand.Execute(null);

            // 4 : rechargement dans SensorViewModel
            var viewModel = new SensorViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockDialogService.Object,
                mockUserSessionService.Object
            );

            // le capteur est visible dans la liste assignée
            Assert.Single(viewModel.Sensors);
            var capteur = viewModel.Sensors.First();
            Assert.Equal("Sensor 1", capteur.Name);
            Assert.Equal(station, capteur.assignedStation);
            Assert.Equal(SensorUsage.Assigne, capteur.Usage);
            Assert.True(capteur.Operational);
        }


        [Fact]
        public void UpdateSensorStatusCommand_ShouldUpdateSensorStatus_WhenExecuted()
        {
            // Arrange
            var sensorToUpdate = new Sensor
            {
                Name = "Sensor 1",
                Frequency = 1.0,
                Treshold = 10.0,
                SensorStatus = false
            };

            _viewModel.Sensors.Add(sensorToUpdate);
            _viewModel.SelectedSensor = sensorToUpdate;

            _mockSensorService.Setup(s => s.UpdateSensorStatus(It.IsAny<Sensor>()))
                .Callback<Sensor>(sensor => sensor.SensorStatus = true);

            // Act
            _viewModel.UpdateSensorStatusCommand.Execute(null);

            _mockSensorService.Verify(s => s.UpdateSensorStatus(It.IsAny<Sensor>()), Times.Once);
            Assert.True(sensorToUpdate.SensorStatus);
        }


        [Fact]
        public void ChangeFrequencyCommand_ShouldUpdateFrequency_WhenExecuted()
        {
            // Arrange
            var sensor = new Sensor
            {
                Id = 1,
                Name = "Sensor 1",
                Frequency = 10.0,
                Delivered = true,
                Usage = SensorUsage.Disponible,
                Operational = false,
                SensorStatus = false,
                assignedStation = null
            };

            var mockSensorService = new Mock<ISensorService>();
            var mockDialogService = new Mock<IDialogService>();
            var mockNavigationService = new Mock<INavigationService>();
            var mockUserSessionService = new Mock<IUserSessionService>();

            var sensorList = new List<Sensor> { sensor };
            mockSensorService.Setup(s => s.GetAllSensors()).Returns(sensorList);

            mockDialogService.Setup(d => d.ShowDialog(It.IsAny<object>()))
                .Callback<object>(dialog =>
                {
                    if (dialog is SensorDialogViewModel vm)
                    {
                        vm.Frequency = "20.0";
                    }
                })
                .Returns(true);

            var viewModel = new SensorViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockDialogService.Object,
                mockUserSessionService.Object
            );

            if (!viewModel.Sensors.Any())
            {
                foreach (var s in sensorList)
                    viewModel.Sensors.Add(s);
            }

            viewModel.SelectedSensor = viewModel.Sensors.First();

            // Act
            viewModel.ChangeFrequencyCommand.Execute(null);

            // Assert
            mockSensorService.Verify(s => s.ChangeSensorFrequency(sensor), Times.Once);
            Assert.Equal(20.0, sensor.Frequency);
        }




        [Fact]
        public void ChangeTresholdCommand_ShouldUpdateTreshold_WhenExecuted()
        {
            // Arrange
            var sensor = new Sensor
            {
                Id = 1,
                Name = "Sensor 1",
                Treshold = 1.0,
                Delivered = true,
                Usage = SensorUsage.Disponible,
                Operational = false,
                SensorStatus = false,
                assignedStation = null
            };

            var mockSensorService = new Mock<ISensorService>();
            var mockDialogService = new Mock<IDialogService>();
            var mockNavigationService = new Mock<INavigationService>();
            var mockUserSessionService = new Mock<IUserSessionService>();

            var sensorList = new List<Sensor> { sensor };
            mockSensorService.Setup(s => s.GetAllSensors()).Returns(sensorList);

            mockDialogService.Setup(d => d.ShowDialog(It.IsAny<object>()))
                .Callback<object>(dialog =>
                {
                    if (dialog is SensorDialogViewModel vm)
                    {
                        vm.Treshold = "8.0";
                    }
                })
                .Returns(true);

            var viewModel = new SensorViewModel(
                mockSensorService.Object,
                mockNavigationService.Object,
                mockDialogService.Object,
                mockUserSessionService.Object
            );

            if (!viewModel.Sensors.Any())
            {
                foreach (var s in sensorList)
                    viewModel.Sensors.Add(s);
            }

            viewModel.SelectedSensor = viewModel.Sensors.First();

            // Act
            viewModel.ChangeTresholdCommand.Execute(null);

            // Assert
            mockSensorService.Verify(s => s.ChangeSensorTreshold(sensor), Times.Once);
            Assert.Equal(8.0, sensor.Treshold);
        }



        [Fact]
        public void DeleteSensorCommand_ShouldNotExecute_WhenSensorIsActive()
        {
            var activeSensor = new Sensor { Id = 3, Name = "Active Sensor", SensorStatus = true };
            _viewModel.SelectedSensor = activeSensor;
            var canDelete = _viewModel.DeleteSensorCommand.CanExecute(null);
            Assert.False(canDelete);
        }

        [Fact]
        public void DeleteSensorCommand_ShouldExecute_WhenSensorIsInactive()
        {
            var inactiveSensor = new Sensor { Id = 4, Name = "Inactive Sensor", SensorStatus = false };
            _viewModel.SelectedSensor = inactiveSensor;
            var canDelete = _viewModel.DeleteSensorCommand.CanExecute(null);
            Assert.True(canDelete);
        }
    }
}
