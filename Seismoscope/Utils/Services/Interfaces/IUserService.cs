using Seismoscope.Model;
using System;

namespace Seismoscope.Model.Interfaces
{
    public interface IUserService
    {
        User? AuthenticateUser(string username, string password);
    }
}
