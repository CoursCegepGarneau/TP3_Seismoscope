using System;
using Seismoscope.Model.Interfaces;
using Seismoscope.Model;
using Microsoft.EntityFrameworkCore;

namespace Seismoscope.Model.DAL
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>
        /// Retourne soit un Admin soit un Employe (puisqu’ils héritent tous deux de User)
        /// Et dans le cas d'un employé, on va aussi charger la station assignée
        /// </returns>
        public User? FindByUsernameAndPassword(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username && u.Password == password);

            // Si c'est un Employe, on charge explicitement la Station
            if (user is Employe employe)
            {
                _context.Entry(employe).Reference(e => e.Station).Load();
            }

            return user;
        }
    }
}
