using Microsoft.EntityFrameworkCore;
using Seismoscope.Data.Repositories.Interfaces;
using Seismoscope.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeismoscopeTest.Data.Repositories
{
    public class SensorRepositoryTests: IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ISensorRepository _repository;

        public SensorRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _repository = new SensorRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void AddSensor_ShouldAddSensorToDatabase()
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
            _repository.AddSensor(sensor);
            _context.SaveChanges();

            var result = _context.Sensors.Find(1);
            Assert.NotNull(result);
            Assert.Equal(77, result.Frequency);
        }

        [Fact]
        public void GetAll_ShouldReturnAllSensors()
        {
            var mockStation = new Station
            {
                Id = 1,
                Nom = "Station A",
                Région = "Québec",
                Latitude = 45.5,
                Longitude = -73.6
            };

            var sensor1 = new Sensor { Id = 0001, Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = mockStation };
            var sensor2 = new Sensor { Id = 0002, Name = "Sensor 2", Treshold = 4.0, Frequency = 80, Delivered = false, Operational = true, SensorStatus = true, assignedStation = mockStation };
            _context.Sensors.AddRange(sensor1, sensor2);
            _context.SaveChanges();

            var sensors = _repository.GetAll();

            Assert.Equal(2, sensors.Count);
        }

        [Fact]
        public void ChangeFrequency_ShouldUpdateFrequency()
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
            _context.Sensors.Add(sensor);
            _context.SaveChanges();

            sensor.Frequency = 42;
            _repository.ChangeFrequency(sensor);

            var updatedSensor = _context.Sensors.Find(1);
            Assert.NotNull(updatedSensor);
            Assert.Equal(42, updatedSensor.Frequency);
        }

        [Fact]
        public void DeleteSensor_ShouldRemoveSensor()
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
            _context.Sensors.Add(sensor);
            _context.SaveChanges();

            _repository.DeleteSensor(sensor);

            var deleted = _context.Sensors.Find(1);
            Assert.Null(deleted);
        }

        [Fact]
        public void UpdateSensor_ShouldUpdateTheSensor()
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
            _context.Sensors.Add(sensor);
            _context.SaveChanges();
            
            sensor.Name = "Sensor 10";
            sensor.Treshold = 5.0;
            sensor.Frequency = 7;

            _repository.UpdateSensor(sensor);
            _context.SaveChanges();

            var result = _context.Sensors.Find(1);
            Assert.NotNull(result);

            Assert.Equal(sensor.Name, result.Name);
            Assert.Equal(sensor.Treshold, result.Treshold);
            Assert.Equal(sensor.Frequency, result.Frequency);
        }

        [Fact]
        public void UpdateDeliveryStatus_ShouldToggleStatus()
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
            _context.Sensors.Add(sensor);
            _context.SaveChanges();

            _repository.UpdateDeliveryStatus(sensor);

            var updated = _context.Sensors.Find(1);
            Assert.NotNull(updated);
            Assert.True(updated.Delivered);
        }

    }
}
