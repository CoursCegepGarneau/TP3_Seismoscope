using Seismoscope.Model;
using System.ComponentModel;

namespace Seismoscope.Utils.Services.Interfaces
{
    public interface IUserSessionService : INotifyPropertyChanged
    {
        User? ConnectedUser { get; set; }

        bool IsUserConnected { get; }

        public bool IsAdmin => ConnectedUser is Admin;
        public bool IsEmploye => ConnectedUser is Employe;

        public Admin? AsAdmin => ConnectedUser as Admin;
        public Employe? AsEmploye => ConnectedUser as Employe;
        bool IsAssignationMode { get; set; }
    }
}
