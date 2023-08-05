using PropertyChanged;
using System.ComponentModel;
using System.Windows.Threading;

namespace PD_ScriptTemplate.ViewModels
{
    /// <summary>
    /// A base view model that fires Property Changed events as needed
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public  class BaseViewModel : INotifyPropertyChanged  
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
}
