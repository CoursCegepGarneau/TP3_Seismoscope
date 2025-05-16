using Seismoscope.Model.Interfaces;
using Seismoscope.Model;
using Seismoscope.Utils.Services.Interfaces;

namespace Seismoscope.Model.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User? AuthenticateUser(string username, string password)
        {
            return _userRepository.FindByUsernameAndPassword(username, password);
        }
    }
}
