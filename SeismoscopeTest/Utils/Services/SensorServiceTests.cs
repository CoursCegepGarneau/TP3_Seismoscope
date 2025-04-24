using GMap.NET.MapProviders;
using Moq;
using Seismoscope.Data.Repositories.Interfaces;
using Seismoscope.Model;
using Seismoscope.Model.Interfaces;
using Seismoscope.Model.Services;
using System.Collections.Generic;
using Xunit;

namespace SeismoscopeTest.Services
{
    public class SensorServiceTests
    {
        private readonly ISensorService _sensorService;
        private readonly Mock<ISensorRepository> _sensorRepositoryMock;

        public SensorServiceTests()
        {
            _sensorRepositoryMock = new Mock<ISensorRepository>();
            _sensorService = new SensorService(_sensorRepositoryMock.Object);
        }

        [Fact]
        public void AddSensor_ShouldCallRepositoryAddSensor()
        {
            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            var sensor = new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation };

            _sensorService.AddSensor(sensor);

            _sensorRepositoryMock.Verify(repo => repo.AddSensor(sensor), Times.Once);
        }

        [Fact]
        public void GetAllSensors_ShouldReturnAllFromRepository()
        {
            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            var sensors = new List<Sensor>
            {
                new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation },
                new Sensor { Id = 0002, Name = "Sensor 2", Treshold = 4.0, Frequency = 80, Delivered = false, Operational = true, SensorStatus = true, assignedStation = mockStation }
            };

            _sensorRepositoryMock.Setup(repo => repo.GetAll()).Returns(sensors);

            var result = _sensorService.GetAllSensors();

            Assert.Equal(2, result.Count);
            Assert.Same(sensors, result);
        }

        [Fact]
        public void GetSensorById_ShouldReturnCorrectSensor()
        {
            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            var sensor = new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation };

            _sensorRepositoryMock.Setup(repo => repo.GetById(1)).Returns(sensor);

            var result = _sensorService.GetSensorById(1);

            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void GetSensorsByStationId_ShouldReturnTheCorrectSensors()
        {
            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            var sensors = new List<Sensor>
            {
                new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation },
                new Sensor { Id = 0002, Name = "Sensor 2", Treshold = 4.0, Frequency = 80, Delivered = false, Operational = true, SensorStatus = true, assignedStation = mockStation }
            };

            _sensorRepositoryMock.Setup(repo => repo.GetByStationId(mockStation.Id)).Returns(sensors);

            var results = _sensorService.GetSensorByStationId(mockStation.Id);

            Assert.Equal(2, results.Count);
            Assert.Same(sensors, results);
        }

        [Fact]
        public void ActivateSensor_ShouldSetStatusTrue_AndCallUpdate()
        {
            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            var sensor = new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation };

            _sensorService.ActivateSensor(sensor);

            Assert.True(sensor.SensorStatus);
            _sensorRepositoryMock.Verify(repo => repo.UpdateSensorStatus(sensor), Times.Once);
        }

        [Fact]
        public void DeactivateSensor_ShouldSetStatusFalse_AndCallUpdate()
        {
            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            var sensor = new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation };

            _sensorService.DeactivateSensor(sensor);

            Assert.False(sensor.SensorStatus);
            _sensorRepositoryMock.Verify(repo => repo.UpdateSensorStatus(sensor), Times.Once);
        }

        [Fact]
        public void DeleteSensor_ShouldCallRepositoryDelete()
        {
            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            var sensor = new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation };

            _sensorService.DeleteSensor(sensor);

            _sensorRepositoryMock.Verify(repo => repo.DeleteSensor(sensor), Times.Once);
        }

        [Fact]
        public void ChangeSensorFrequency_ShouldCallRepository()
        {
            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            var sensor = new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation };

            _sensorService.ChangeSensorFrequency(sensor);

            _sensorRepositoryMock.Verify(repo => repo.ChangeFrequency(sensor), Times.Once);
        }

        [Fact]
        public void ChangeSensorTreshold_ShouldCallRepository()
        {
            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            var sensor = new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation };

            _sensorService.ChangeSensorTreshold(sensor);

            _sensorRepositoryMock.Verify(repo => repo.ChangeTreshold(sensor), Times.Once);
        }

        [Fact]
        public void UpdateSensor_ShouldCallRepository()
        {

            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            var sensor = new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation };

            _sensorService.UpdateSensor(sensor);

            _sensorRepositoryMock.Verify(repo => repo.UpdateSensor(sensor), Times.Once);
        }

        [Fact]
        public void UpdateSensorDeliveryStatus_ShouldCallRepository()
        {
            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            var sensor = new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation };

            _sensorService.UpdateSensorDeliveryStatus(sensor);

            _sensorRepositoryMock.Verify(repo => repo.UpdateDeliveryStatus(sensor), Times.Once);
        }

        [Fact]
        public void UpdateSensorOperationalStatus_ShouldCallRepository()
        {
            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            var sensor = new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation };

            _sensorService.UpdateSensorOperationalStatus(sensor);

            _sensorRepositoryMock.Verify(repo => repo.UpdateOperationalStatus(sensor), Times.Once);
        }
    }
}
