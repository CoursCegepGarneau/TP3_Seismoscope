using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seismoscope.Services.Interfaces
{
    public interface IDialogService
    {
        bool? ShowDialog(object viewModel);
        string? OpenFile(string filter);
    }
}

