using System;
using Seismoscope.Model;

namespace Seismoscope.Model.Interfaces
{
    public interface IUserRepository
    {
        User? FindByUsernameAndPassword(string username, string password);
    }
}
