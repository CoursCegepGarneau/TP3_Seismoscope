using Microsoft.EntityFrameworkCore;
using Seismoscope.Model;
using Seismoscope.Utils.Services;
using Seismoscope.Utils.Enums;
using System.IO;

public class ApplicationDbContext : DbContext
{

    public ApplicationDbContext() : base() { }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnConfiguring(
       DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Définir le chemin absolu pour la base de données dans le répertoire AppData
            var dbPath = Path.Combine(@"C:\Seismoscope\", "Seismoscope.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            var connectionString = $"Data Source={dbPath}";

            // Configurer le DbContext pour utiliser la chaîne de connexion
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
        // Étape 1 : Ajout des stations
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


        var stationA = Stations.First(s => s.Nom == "Station A");
        var stationB = Stations.First(s => s.Nom == "Station B");
        var stationC = Stations.First(s => s.Nom == "Station C");
        var stationD = Stations.First(s => s.Nom == "Station D");
        var stationE = Stations.First(s => s.Nom == "Station E");



        // Étape 2 : Ajout des utilisateurs si aucun
        if (!Users.Any())
        {
            Users.AddRange(
                new Admin
                {
                    Prenom = "John",
                    Nom = "Doe",
                    Username = "johndoe",
                    Password = "password123"
                },
                new Employe
                {
                    Prenom = "Jane",
                    Nom = "Doe",
                    Username = "janedoe",
                    Password = "password123",
                    Station = stationA
                },
                new Employe
                {
                    Prenom = "Riadh",
                    Nom = "Cadi",
                    Username = "riadh",
                    Password = "123456",
                    Station = stationC
                }
            );
        }



        Sensors.RemoveRange(Sensors); // Supprime tous les capteurs
        SaveChanges();

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
