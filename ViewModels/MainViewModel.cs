using PD_ScriptTemplate.Helpers;
using System.Windows;
using System.Windows.Threading;

namespace PD_ScriptTemplate.ViewModels
{
    /// <summary>
    /// This class is used for navigation purposes. 
    /// </summary>

    public class MainViewModel:BaseViewModel
    {
        private Dispatcher userInterface;
        public EsapiWorker esapiWorker = null;
        public BaseViewModel CurrentViewModel { get; }
        public MainViewModel(EsapiWorker _esapiWorker = null) 
        {
            esapiWorker = _esapiWorker;
            userInterface = Dispatcher.CurrentDispatcher;
            CurrentViewModel = new PrimaryMainViewModel(_esapiWorker); 
        }
        
    }
}
