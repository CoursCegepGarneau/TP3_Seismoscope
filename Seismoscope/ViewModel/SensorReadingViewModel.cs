using Microsoft.Win32;
using Seismoscope.Model;
using Seismoscope.Utils;
using Seismoscope.Utils.Commands;

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Seismoscope.Model.Interfaces;
using Seismoscope.Utils.Services.Interfaces;
using Seismoscope.Utils.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Seismoscope.ViewModel
{
    public class SensorReadingViewModel : BaseViewModel
    {
        //public ObservableCollection<SensorReading> Readings { get; set; }
        //public ObservableCollection<SismicEvent> EventHistory { get; set; }

        private readonly ISensorService _sensorService;
        private readonly INavigationService _navigationService;
        private readonly IUserSessionService _userSessionService;
        private readonly ISensorAdjustementService _sensorAdjustementService;



        private ObservableCollection<Sensor> _sensors = null!;

        public ICommand LoadCsvCommand { get; }
        public ICommand StartReadingCommand { get; }
        public ICommand StopReadingCommand { get; }

        public RelayCommand GoBackCommand { get; }

        public Sensor? SelectedSensor { get; set; }

        private Timer _readingTimer;

        private string _csvFilePath;

        private List<SeismicEvent> _donneesSismiques;

        public ObservableCollection<Sensor> Sensors
        {
            get => _sensors;
            set
            {
                _sensors = value;
                OnPropertyChanged();
            }
        }

        Station station;

        public SensorReadingViewModel(ISensorService sensorService, INavigationService navigationService, IUserSessionService userSessionService, ISensorAdjustementService sensorAdjustementService)
        {

            _userSessionService = userSessionService;
            _sensorService = sensorService;
            _navigationService = navigationService;
            _sensorAdjustementService = sensorAdjustementService;

            Sensors = new ObservableCollection<Sensor>(_sensorService.GetAllSensors());
            RefreshSensors();
            SetupSensorChart();


            station = _userSessionService.AsEmploye?.Station;
            LoadCsvCommand = new RelayCommand(OpenCsvDialog);
            GoBackCommand = new RelayCommand(() => navigationService.NavigateTo<SensorViewModel>());
            _sensorAdjustementService = sensorAdjustementService;
        }

        public void RefreshSensors()
        {
            var station = _userSessionService.AsEmploye?.Station;

            if (station != null)
            {
                Sensors = new ObservableCollection<Sensor>(_sensorService.GetSensorByStationId(station.Id));
            }
            else
            {
                Sensors = new ObservableCollection<Sensor>();
            }

            OnPropertyChanged(nameof(Sensors));
        }


        public override void OnNavigated(object? parameter = null)
        {
            if (parameter is Sensor sensor)
            {
                SelectedSensor = sensor;
                // Démarrer les lectures ou initialiser les données ici
            }
        }


        private void OpenCsvDialog()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Sélectionner un fichier de données sismiques"
            };

            if (dialog.ShowDialog() == true)
            {
                _csvFilePath = dialog.FileName;

                _donneesSismiques = CsvUtils.LireLecturesDepuisCsv(_csvFilePath, SelectedSensor, _sensorAdjustementService);

                MessageBox.Show("Fichier chargé avec succès.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void StartDetection()
        {
            if (SelectedSensor == null || string.IsNullOrWhiteSpace(_csvFilePath))
            {
                MessageBox.Show("Veuillez sélectionner un capteur et charger un fichier CSV.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ///_vibrationService.StartDetection(SelectedSensor, _csvFilePath);
            MessageBox.Show("StartDetection.", "Info", MessageBoxButton.OK, MessageBoxImage.Warning);
        }


        public ObservableCollection<ISeries> SensorSeries { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

        private void SetupSensorChart()
        {
            // On suppose que chaque capteur est lu à intervalle régulier selon sa fréquence.
            // Tu peux adapter cette logique pour refléter ton vrai scénario d’analyse en temps réel.

            var lectureIntervalle = 5; // par exemple, 5 secondes entre chaque lecture
            var temps = Sensors.Select((s, index) => $"{index * lectureIntervalle}s").ToArray();

            SensorSeries = new ObservableCollection<ISeries>
    {
        new ColumnSeries<double>
        {
            Values = Sensors.Select(s => s.Frequency).ToArray(),
            Name = "Fréquence (Hz)",
            Stroke = new SolidColorPaint(SKColors.Blue),
            Fill = new SolidColorPaint(SKColors.LightBlue)
        }
    };

            XAxes = new[]
            {
        new Axis
        {
            Labels = temps,
            Name = "Temps (s)",
            LabelsRotation = 15
        }
    };

            YAxes = new[]
            {
        new Axis
        {
            Name = "Fréquence (Hz)"
        }
    };
        }

    }
}
