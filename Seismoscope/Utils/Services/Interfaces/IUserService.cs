using Seismoscope.Model;
using System;

namespace Seismoscope.Utils.Services.Interfaces
{
    public interface IUserService
    {
        User? AuthenticateUser(string username, string password);
    }
}
