using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Fork.logic.Utils;

public class ObservableRangeCollection<T> : ObservableCollection<T>
{
    public void AddRange(IEnumerable<T> toAdd)
    {
        IEnumerable<T> enumerable = toAdd.ToList();
        if (!enumerable.Any())
        {
            return;
        }

        CheckReentrancy();
        foreach (T i in enumerable) Items.Add(i);

        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, enumerable.Last()));
    }
}