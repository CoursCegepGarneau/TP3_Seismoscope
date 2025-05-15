using Microsoft.EntityFrameworkCore;
using Seismoscope.Data.Repositories.Interfaces;
using Seismoscope.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Seismoscope.Model.Interfaces;
using Seismoscope.Model.DAL;

namespace SeismoscopeTest.Data.Repositories
{
    public class UserRepositoryTests: IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _repository = new UserRepository(_context);

            SeedData();
        }

        private void SeedData()
        {
            var station = new Station { Nom = "Station A", Région = "Québec", Latitude = 45.5, Longitude = -73.6 };
            var admin = new Admin
            {
                Id = 1,
                Username = "admin",
                Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Nom = "Admin",
                Prenom = "Admin"
            };

            var employe = new Employe
            {
                Id = 2,
                Username = "employe",
                Password = BCrypt.Net.BCrypt.HashPassword("employe123"),
                Nom = "Employé",
                Prenom = "Test",
                Station = station
            };

            _context.Stations.Add(station);
            _context.Users.AddRange(admin, employe);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void FindByUsernameAndPassword_ReturnsAdmin_WhenValidCredentials()
        {
            var result = _repository.FindByUsernameAndPassword("admin", "admin123");

            Assert.NotNull(result);
            Assert.IsType<Admin>(result);
            Assert.Equal("admin", result.Username);
        }

        [Fact]
        public void FindByUsernameAndPassword_ReturnsEmployeWithStation_WhenValidCredentials()
        {
            var result = _repository.FindByUsernameAndPassword("employe", "employe123");

            Assert.NotNull(result);
            var employe = Assert.IsType<Employe>(result);
            Assert.Equal("employe", employe.Username);
            Assert.NotNull(employe.Station);
            Assert.Equal("Station A", employe.Station.Nom);
        }

        [Fact]
        public void FindByUsernameAndPassword_ReturnsNull_WhenInvalidCredentials()
        {
            var result = _repository.FindByUsernameAndPassword("jean", "motdepasse");

            Assert.Null(result);
        }


    }
}
