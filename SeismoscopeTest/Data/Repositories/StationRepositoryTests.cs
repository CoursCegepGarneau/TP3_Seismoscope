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
    public class StationRepositoryTests: IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IStationRepository _repository;

        public StationRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _repository = new StationRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void Add_ShouldAddStation()
        {
            // Arrange
            var station = new Station { Nom = "Station A", Région = "Québec", Latitude = 45.5, Longitude = -73.6 };

            // Act
            _repository.Add(station);

            // Assert
            var result = _context.Stations.FirstOrDefault(s => s.Nom == "Station A");
            Assert.NotNull(result);
            Assert.Equal("Station A", result.Nom);
        }

        [Fact]
        public void GetAll_ShouldReturnAllStations()
        {
            // Arrange
            _repository.Add(new Station { Nom = "Station A", Région = "Québec", Latitude = 45.5, Longitude = -73.6 });
            _repository.Add(new Station { Nom = "Station B", Région = "Ontario", Latitude = 43.7, Longitude = -79.4 });

            // Act
            var stations = _repository.GetAll().ToList();

            // Assert
            Assert.Equal(2, stations.Count);
            Assert.Contains(stations, s => s.Nom == "Station A");
            Assert.Contains(stations, s => s.Nom == "Station B");
        }

        [Fact]
        public void GetById_ShouldReturnCorrectStation()
        {
            // Arrange
            var station = new Station { Nom = "Station A", Région = "Québec", Latitude = 45.5, Longitude = -73.6 };
            _repository.Add(station);

            // Act
            var result = _repository.GetById(station.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Station A", result.Nom);
        }

        [Fact]
        public void Update_ShouldModifyStation()
        {
            // Arrange
            var station = new Station { Nom = "Station A", Région = "Québec", Latitude = 45.5, Longitude = -73.6 };
            _repository.Add(station);

            // Act
            station.Nom = "Montreal";
            _repository.Update(station);

            var updated = _repository.GetById(station.Id);

            // Assert
            Assert.Equal("Montreal", updated.Nom);
        }

        [Fact]
        public void Delete_ShouldRemoveStation()
        {
            // Arrange
            var station = new Station { Nom = "Station A", Région = "Québec", Latitude = 45.5, Longitude = -73.6 };
            _repository.Add(station);

            // Act
            _repository.Delete(station.Id);
            var result = _repository.GetById(station.Id);

            // Assert
            Assert.Null(result);
        }
    }
}
