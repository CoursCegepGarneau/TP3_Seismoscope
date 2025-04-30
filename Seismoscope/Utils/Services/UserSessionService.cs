using System.ComponentModel;
using System.Runtime.CompilerServices;
using Seismoscope.Utils.Services.Interfaces;

namespace Seismoscope.Utils.Services
{
    public class UserSessionService : IUserSessionService
    {
        private User? _connectedUser;
        public User? ConnectedUser
        {
            get => _connectedUser;
            set
            {
                _connectedUser = value;
                OnPropertyChanged(nameof(ConnectedUser));
                OnPropertyChanged(nameof(IsUserConnected));
                OnPropertyChanged(nameof(IsUserDisconnected));
            }
        }

        public bool IsUserConnected
        {
            get => _connectedUser != null;
        }
        public bool IsUserDisconnected
        {
            get => _connectedUser == null;
        }
        public bool IsAssignationMode { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
