using Microsoft.Win32;
using Seismoscope.Services.Interfaces;
using Seismoscope.View;

namespace Seismoscope.Services
{
    public class DialogService : IDialogService
    {
        public bool? ShowDialog(object viewModel)
        {
            var dialog = new SensorDialogView
            {
                DataContext = viewModel
            };
            return dialog.ShowDialog();
        }

        public string? OpenFile(string filter)
        {
            var dlg = new OpenFileDialog { Filter = filter };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }
    }

}

