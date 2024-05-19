using System.ComponentModel;

namespace Fork.ViewModel;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void RaisePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(sender, e);
    }

    protected void RaisePropertyChanged(object sender, string propertyName)
    {
        PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
    }
}