using System.Windows;
using Seismoscope.Utils;
using Seismoscope.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Seismoscope.Model.Interfaces;
using Seismoscope.Model.DAL;
using Seismoscope.Utils.Services.Interfaces;
using Seismoscope.Utils.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using Seismoscope.View;
using Seismoscope.Services.Interfaces;
using Seismoscope.Services;
using Seismoscope.Model.Services;
using Seismoscope.Data.Repositories.Interfaces;
using Seismoscope.Model;
using NLog;

namespace Seismoscope
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public App()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            IConfiguration configuration = builder.Build();
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<ConnectUserViewModel>();
            services.AddSingleton<SensorViewModel>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<SensorManagementViewModel>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IUserSessionService, UserSessionService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<ISensorService, SensorService>();
            services.AddSingleton<ISensorRepository, SensorRepository>();
            services.AddSingleton<IStationRepository, StationRepository>();
            services.AddSingleton<IStationService, StationService>();
            services.AddSingleton<Func<Type, BaseViewModel>>(serviceProvider =>
            {
                BaseViewModel ViewModelFactory(Type viewModelType)
                {
                    return (BaseViewModel)serviceProvider.GetRequiredService(viewModelType);
                }
                return ViewModelFactory;
            });

            services.AddDbContext<ApplicationDbContext>((provider, options) =>
            {
                try
                {
                    var config = provider.GetRequiredService<IConfigurationService>();
                    var dbPath = config.GetDbPath();

                    if (string.IsNullOrWhiteSpace(dbPath))
                        throw new InvalidOperationException("DbPath manquant.");

                    var connectionString = $"Data Source={dbPath}";
                    options.UseSqlite(connectionString);
                    logger.Info("Base de donnée configurée avec succès");
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex, "Erreur lors de la configuration de la base de données.");
                    throw;
                }
            });

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            logger.Info("Démarrage de l'application");
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
                dbContext.SeedData();
            }

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
    }
}
