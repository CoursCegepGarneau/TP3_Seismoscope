using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Seismoscope.Utils
{
    public abstract partial class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void OnNavigated(object? parameter = null)
        {
            // Méthode par défaut vide — peut être surchargée dans les ViewModels
        }
    }
}
