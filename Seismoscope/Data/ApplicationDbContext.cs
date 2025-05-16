using Microsoft.EntityFrameworkCore;
using Seismoscope.Model;
using Seismoscope.Utils.Services;
using Seismoscope.Utils.Enums;
using System.IO;
using Seismoscope.Utils.Services.Interfaces;
using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Seismoscope.Utils;

public class ApplicationDbContext : DbContext
{
    readonly static ILogger logger = LogManager.GetCurrentClassLogger();
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
         : base(options)
    {
    }

    public ApplicationDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {

            var dbPath = ConfigurationManager.AppSettings["DbPath"];
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                logger.Fatal("DbPath manquant dans App.config");
                throw new InvalidOperationException("DbPath manquant dans App.config");
            }
            var directory = Path.GetDirectoryName(dbPath)!;
            Directory.CreateDirectory(directory);
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Station> Stations { get; set; }
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<Employe> Employes { get; set; }
    public DbSet<Admin> Admins { get; set; }

    public DbSet<HistoriqueEvenement> Historiques { get; set; }

    public void SeedData()
    {
        try
        {
            logger.Info("Ajout des stations par défaut...");
            AddDefaultStations();
            logger.Info("Ajout des utilisateurs par défaut...");
            AddDefaultUsers();
            logger.Info("Ajout des capteurs par défaut...");
            AddDefaultSensors();
        }
        catch (Exception ex)
        {
            logger.Warn(ex, $"Erreur lors de l'ajout des données");
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

    private void AddDefaultSensors()
    {
        if (Sensors.Any())
            return;

        var stationA = Stations.First(s => s.Nom == "Station A");
        var stationB = Stations.First(s => s.Nom == "Station B");
        var stationC = Stations.First(s => s.Nom == "Station C");
        var stationD = Stations.First(s => s.Nom == "Station D");
        var stationE = Stations.First(s => s.Nom == "Station E");

        Sensors.AddRange(
        // ➤ Test Règle 1 (amplitude > 130% seuil)
        new Sensor { Name = "Sensor 1", Treshold = 42, Frequency = 5, Delivered = true, Operational = true, SensorStatus = true, assignedStation = stationA },

        // ➤ Test Règle 2 (5 valeurs consécutives ≥ 80% du seuil)
        new Sensor { Name = "Sensor 2", Treshold = 49, Frequency = 5, Delivered = true, Operational = true, SensorStatus = true, assignedStation = stationA },

        // ➤ Test Règle 3 (amplitude > 160% du seuil → augmente fréquence)
        new Sensor { Name = "Sensor 3", Treshold = 50, Frequency = 10, Delivered = true, Operational = true, SensorStatus = true, assignedStation = stationB },

        // ➤ Test Règle 4 (10 lectures calmes pour revenir à fréquence par défaut)
        new Sensor { Name = "Sensor 4", Treshold = 50, Frequency = 2, Delivered = true, Operational = true, SensorStatus = true, assignedStation = stationB },

        // ➤ Valeur de seuil basse (événements déclenchés facilement)
        new Sensor { Name = "Sensor 5", Treshold = 20, Frequency = 5, Delivered = true, Operational = true, SensorStatus = true, assignedStation = stationC },

        // ➤ Valeur de seuil haute (événements rares)
        new Sensor { Name = "Sensor 6", Treshold = 80, Frequency = 5, Delivered = true, Operational = true, SensorStatus = true, assignedStation = stationC },

        // ➤ Capteur désactivé (non livré / non opérationnel)
        new Sensor { Name = "Sensor 7", Treshold = 60, Frequency = 5, Delivered = false, Operational = false, SensorStatus = false, assignedStation = stationD },

        // ➤ Capteur actif sans seuil particulier (contrôle)
        new Sensor { Name = "Sensor 8", Treshold = 45, Frequency = 5, Delivered = true, Operational = true, SensorStatus = true, assignedStation = stationD },

        // ➤ Capteur avec seuil moyen pour détecter micro-secousses
        new Sensor { Name = "Sensor 9", Treshold = 35, Frequency = 7, Delivered = true, Operational = true, SensorStatus = true, assignedStation = stationE },

        // ➤ Capteur limite (seuil proche du max, difficile à déclencher)
        new Sensor { Name = "Sensor 10", Treshold = 95, Frequency = 5, Delivered = true, Operational = true, SensorStatus = true, assignedStation = stationE }
        );


        SaveChanges();
    }

    private void AddDefaultUsers()
    {
        int count = int.Parse(ConfigurationManager.AppSettings["DefaultUsers.Count"] ?? "0");
        logger.Debug($"Nombre d'utilisateurs par défaut à traiter:{count}");
        for (int i = 1; i <= count; i++)
        {
            var DefaultUser = ConfigurationManager.AppSettings[$"DefaultUser.{i}"];
            try
            {
                if (DefaultUser.Empty())
                {
                    logger.Error($"Clé 'DefaultUser.{i}' introuvable dans App.config");
                    throw new ConfigurationErrorsException($"Clé 'DefaultUser.{i}' manquant");
                }


                var parts = DefaultUser!.Split('|');
                if (parts.Length < 5)
                {
                    logger.Warn($"Format Invalide pour un defaultUser, attributs:{parts.Length}");
                    throw new ConfigurationErrorsException($"Le Format est invalide pour 'DefaultUser.{i}'");
                }

                string prenom = parts[0];
                string nom = parts[1];
                string username = parts[2];
                string password = parts[3];
                string role = parts[4];
                string? stationSuffix = parts.Length > 5 ? parts[5] : null;

                if (Users.Any(u => u.Username == username))
                {
                    logger.Info($"L'utilisateur avec le nom d'utilisateur '{username}' existe déjà.");
                    continue;
                }
                   
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

                    if (stationSuffix.NotEmpty())
                    {
                        station = Stations.FirstOrDefault(s => s.Nom.EndsWith(stationSuffix!));
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
            catch (Exception ex)
            {
                logger.Error(ex, $"Erreur lors de l'ajout des utilisateurs par défaut '{DefaultUser}'");
            }
        }
        SaveChanges();
    }
}

