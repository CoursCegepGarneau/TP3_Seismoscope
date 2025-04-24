using Moq;
using Seismoscope.Data.Repositories.Interfaces;
using Seismoscope.Model;
using Seismoscope.Model.Interfaces;
using Seismoscope.Model.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeismoscopeTest.Utils.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockRepository;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockRepository = new Mock<IUserRepository>();
            _userService = new UserService(_mockRepository.Object);
        }

        [Fact]
        public void Authenticate_ReturnsAdmin_WhenCredentialsAreValid()
        {
            // Arrange
            var admin = new Admin
            {
                Username = "admin",
                Password = "admin123",
                Nom = "Admin",
                Prenom = "Test"
            };

            _mockRepository.Setup(r => r.FindByUsernameAndPassword("admin", "admin123"))
                           .Returns(admin);

            // Act
            var result = _userService.AuthenticateUser("admin", "admin123");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Admin>(result);
            Assert.Equal("admin", result.Username);
        }

        [Fact]
        public void Authenticate_ReturnsEmploye_WhenCredentialsAreValid()
        {
            // Arrange
            var station = new Station { Nom = "Station A", Région = "Québec", Latitude = 45.5, Longitude = -73.6 };

            var employe = new Employe
            {
                Username = "employe",
                Password = "employe123",
                Nom = "Emplyoe",
                Prenom = "Employe",
                Station = station
            };

            _mockRepository.Setup(r => r.FindByUsernameAndPassword("employe", "employe123"))
                           .Returns(employe);

            // Act
            var result = _userService.AuthenticateUser("employe", "employe123");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Employe>(result);
            Assert.Equal("employe", result.Username);
        }

        [Fact]
        public void Authenticate_ReturnsNull_WhenCredentialsAreInvalid()
        {
            // Arrange
            _mockRepository.Setup(r => r.FindByUsernameAndPassword("jean", "motdepassse"))
                           .Returns((User?)null);

            // Act
            var result = _userService.AuthenticateUser("jean", "motdepasse");

            // Assert
            Assert.Null(result);
        }


    }
}
