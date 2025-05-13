using Microsoft.EntityFrameworkCore;
using Seismoscope.Model;
using Seismoscope.Utils.Services;
using Seismoscope.Utils.Enums;
using System.IO;
using Seismoscope.Utils.Services.Interfaces;
using System.Configuration;

public class ApplicationDbContext : DbContext
{

    private readonly IConfigurationService? _configurationService;

    public ApplicationDbContext() : base() { }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfigurationService configurationService) : base(options)
    {
        _configurationService = configurationService;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbPath = Path.Combine(@"C:\Seismoscope\", "Seismoscope.db");
            var directory = Path.GetDirectoryName(dbPath)
                          ?? throw new InvalidOperationException("Chemin du répertoire invalide.");
            Directory.CreateDirectory(directory);
            var connectionString = $"Data Source={dbPath}";
            optionsBuilder.UseSqlite(connectionString);
        }
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Station> Stations { get; set; }

    public DbSet<Sensor> Sensors { get; set; }

    public DbSet<Employe> Employes { get; set; }
    public DbSet<Admin> Admins { get; set; }

    public void SeedData()
    {
        try
        {
            var config = new ConfigurationService();

            AddDefaultStations();

            var stationA = Stations.First(s => s.Nom == "Station A");
            var stationB = Stations.First(s => s.Nom == "Station B");
            var stationC = Stations.First(s => s.Nom == "Station C");
            var stationD = Stations.First(s => s.Nom == "Station D");
            var stationE = Stations.First(s => s.Nom == "Station E");

            AddDefaultUsers();

            if (!Sensors.Any())
            {
                Sensors.AddRange(
                        new Sensor { Name = "Sensor 1", Treshold = 3.5, Frequency = 77, Delivered = false, Operational = false, SensorStatus = false, assignedStation = stationA },
                        new Sensor { Name = "Sensor 2", Treshold = 4.0, Frequency = 80, Delivered = false, Operational = true, SensorStatus = true, assignedStation = stationA },
                        new Sensor { Name = "Sensor 3", Treshold = 2.8, Frequency = 65, Delivered = true, Operational = true, SensorStatus = false, assignedStation = stationB },
                        new Sensor { Name = "Sensor 4", Treshold = 3.2, Frequency = 90, Delivered = false, Operational = false, SensorStatus = true, assignedStation = stationB },
                        new Sensor { Name = "Sensor 5", Treshold = 5.0, Frequency = 100, Delivered = true, Operational = false, SensorStatus = false, assignedStation = stationC },
                        new Sensor { Name = "Sensor 6", Treshold = 3.7, Frequency = 85, Delivered = true, Operational = true, SensorStatus = true, assignedStation = stationC },
                        new Sensor { Name = "Sensor 7", Treshold = 2.5, Frequency = 72, Delivered = false, Operational = false, SensorStatus = false, assignedStation = stationD },
                        new Sensor { Name = "Sensor 8", Treshold = 4.2, Frequency = 88, Delivered = true, Operational = true, SensorStatus = true, assignedStation = stationD },
                        new Sensor { Name = "Sensor 9", Treshold = 3.0, Frequency = 60, Delivered = false, Operational = false, SensorStatus = false, assignedStation = stationE },
                        new Sensor { Name = "Sensor 10", Treshold = 4.5, Frequency = 95, Delivered = true, Operational = true, SensorStatus = true, assignedStation = stationE });
                SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'ajout des données : {ex.Message}");
        }
    }
    private void AddDefaultStations()
    {
        var stationList = new[]
            {
            new Station { Nom = "Station A", Région = "Québec", Latitude = 45.5, Longitude = -73.6 },
            new Station { Nom = "Station B", Région = "Ontario", Latitude = 43.7, Longitude = -79.4 },
            new Station { Nom = "Station C", Région = "Colombie-Britannique", Latitude = 49.2, Longitude = -123.1 },
            new Station { Nom = "Station D", Région = "Alberta", Latitude = 53.5, Longitude = -113.5 },
            new Station { Nom = "Station E", Région = "Manitoba", Latitude = 49.9, Longitude = -97.1 }
            };

        foreach (var station in stationList)
        {
            if (!Stations.Any(s => s.Nom == station.Nom))
            {
                Stations.Add(station);
            }
        }
        SaveChanges();
    }

    private void AddDefaultUsers()
    {
        int count = int.Parse(ConfigurationManager.AppSettings["DefaultUsers.Count"]);
        for (int i = 1; i <= count; i++)
        {
            var raw = ConfigurationManager.AppSettings[$"DefaultUser.{i}"];
            var parts = raw.Split('|');

            string prenom = parts[0];
            string nom = parts[1];
            string username = parts[2];
            string password = parts[3];
            string role = parts[4];
            string stationSuffix = parts.Length > 5 ? parts[5] : null;

            if (Users.Any(u => u.Username == username))
                continue;

            if (role == "Admin")
            {
                Admins.Add(new Admin
                {
                    Prenom = prenom,
                    Nom = nom,
                    Username = username,
                    Password = BCrypt.Net.BCrypt.HashPassword(password)
                });
            }
            else if (role == "Employe")
            {
                Station? station = null;

                if (!string.IsNullOrEmpty(stationSuffix))
                {
                    station = Stations.FirstOrDefault(s => s.Nom.EndsWith(stationSuffix));
                }

                Employes.Add(new Employe
                {
                    Prenom = prenom,
                    Nom = nom,
                    Username = username,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    Station = station
                });
            }
        }

        SaveChanges();
    }
}

