using System.Windows;
using Seismoscope.ViewModel;

namespace Seismoscope.View
{
    public partial class SensorDialogView : Window
    {
        public SensorDialogView()
        {
            InitializeComponent();
            Loaded += SensorDialogView_Loaded;
        }
        private void SensorDialogView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SensorDialogViewModel vm)
            {
                vm.CloseAction = result =>
                {
                    this.DialogResult = result;
                    this.Close();
                };
            }
        }
    }
}
