using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Seismoscope.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Seismoscope.View
{
    /// <summary>
    /// Logique d'interaction pour SensorReadingView.xaml
    /// </summary>
    public partial class SensorReadingView : UserControl
    {
        public SensorReadingView()
        {
            InitializeComponent();

            LiveCharts.Configure(config =>
            config
                .AddSkiaSharp()
                .AddDefaultMappers()
                .AddLightTheme()
                );
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //if (DataContext is SensorReadingViewModel vm)
            //{
            //    vm.RefreshSensors();
            //}
        }
    }
}
