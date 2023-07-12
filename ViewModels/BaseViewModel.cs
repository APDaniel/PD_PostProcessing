using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PD_ScriptTemplate.ViewModels
{
    /// <summary>
    /// A base view model that fires Property Changed events as needed
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public  class BaseViewModel : INotifyPropertyChanged  
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
    }
    
}
