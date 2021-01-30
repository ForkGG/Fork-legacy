using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;

namespace Fork.Logic.Utils
{
    public class CollectionUtils
    {
        public static int Search<T>(ObservableCollection<T> collection, int startIndex, T other)
        {
            for (int i = startIndex; i < collection.Count; i++)
            {
                if (other.Equals(collection[i]))
                    return i;
            }

            throw new Exception("Could not find element in List");
        }
    }
}