using System;
using System.IO;
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
using GMap.NET.MapProviders;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using Seismoscope.ViewModel;

namespace Seismoscope.View
{
    /// <summary>
    /// Logique d'interaction pour AdminDashboardView.xaml
    /// </summary>
    public partial class AdminDashboardView : UserControl
    {
        public AdminDashboardView()
        {
            InitializeComponent();

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            Loaded += AdminDashboardView_Loaded;
            




        }

        private void AdminDashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            MapControl.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            MapControl.Position = new GMap.NET.PointLatLng(46.8139, -71.2082); // Québec
            MapControl.MinZoom = 2;
            MapControl.MaxZoom = 18;
            MapControl.Zoom = 5; //

            MapControl.ShowCenter = false;
            MapControl.CanDragMap = true;
            MapControl.DragButton = MouseButton.Left;

            var marker = new GMapMarker(new PointLatLng(45.5, -73.6)) // Station A (Québec)
            {
                Shape = new System.Windows.Shapes.Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = System.Windows.Media.Brushes.Red,
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 2
                }
            };

            MapControl.Markers.Add(marker);

            AddStationMarkers();
        }

        private void AddStationMarkers()
        {
            if (DataContext is AdminDashboardViewModel vm && vm.Stations != null)
            {
                foreach (var station in vm.Stations)
                {
                    var ellipse = new System.Windows.Shapes.Ellipse
                    {
                        Width = 12,
                        Height = 12,
                        Fill = System.Windows.Media.Brushes.Red,
                        Stroke = System.Windows.Media.Brushes.White,
                        StrokeThickness = 1.5,
                        ToolTip = $"📍 {station.Nom}\n{station.Région}",
                        Cursor = Cursors.Hand
                    };

                    ellipse.MouseLeftButtonDown += (s, e) =>
                    {
                        vm.SelectedStation = station;
                    };

                    var marker = new GMapMarker(new GMap.NET.PointLatLng(station.Latitude, station.Longitude))
                    {
                        Shape = ellipse
                    };

                    MapControl.Markers.Add(marker);
                }
            }
        }
    }
}
