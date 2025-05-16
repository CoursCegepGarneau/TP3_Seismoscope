using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seismoscope.Utils.Services.Interfaces
{
    public interface INavigationService
    {
        BaseViewModel? CurrentView { get; }
        void NavigateTo<T>() where T : BaseViewModel;

        void NavigateTo(BaseViewModel viewModel);

        //Surcharge pour que NavigateTo puisse passer un objet
        void NavigateTo<TViewModel>(object parameter);

    }
}
