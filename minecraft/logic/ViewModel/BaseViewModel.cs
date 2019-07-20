using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged;
using nihilus.Annotations;

namespace nihilus.logic.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => {};
    }
}