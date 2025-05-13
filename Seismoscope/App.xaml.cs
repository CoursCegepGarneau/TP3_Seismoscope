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

namespace Seismoscope
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        public App()
        {
            // Note à moi-même, mieux séparer en fonctions ici. 
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
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<ISensorService,SensorService>();
            services.AddSingleton<ISensorRepository, SensorRepository>();
            services.AddSingleton<IStationRepository, StationRepository>();
            services.AddSingleton<IStationService, StationService>();
            services.AddDbContext<ApplicationDbContext>();


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
                var config = provider.GetRequiredService<IConfigurationService>();
                var dbPath = config.GetDbPath() ?? throw new InvalidOperationException("DbPath manquant.");
                var connectionString = $"Data Source={dbPath}";
                options.UseSqlite(connectionString);
            });


            _serviceProvider = services.BuildServiceProvider();
        }


        //EnsureCreated() ne tient pas compte les migrations, donc on le remplace direct par Database.Migrate()
        protected override void OnStartup(StartupEventArgs e)
        {
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
