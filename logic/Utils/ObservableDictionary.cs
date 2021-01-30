using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Fork.Logic.Utils
{
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged
    {
        private readonly Dictionary<TKey, TValue> internalDict;

        public ObservableDictionary()
        {
            internalDict = new Dictionary<TKey, TValue>();
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dict)
        {
            internalDict = new Dictionary<TKey, TValue>(dict);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return internalDict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            internalDict.Add(item.Key, item.Value);
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void Clear()
        {
            internalDict.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return internalDict.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool result = internalDict.Remove(item.Key);
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            return result;
        }

        public int Count => internalDict.Count;
        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            var pair = new KeyValuePair<TKey, TValue>(key, value);
            internalDict.Add(pair.Key, pair.Value);
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, pair));
        }

        public bool ContainsKey(TKey key)
        {
            return internalDict.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            bool result = internalDict.Remove(key, out TValue value);
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                    new KeyValuePair<TKey, TValue>(key, value)));
            return result;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return internalDict.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get => internalDict[key];
            set
            {
                if (internalDict.TryGetValue(key, out TValue origValue))
                {
                    if (origValue.Equals(value)) return;
                    internalDict[key] = value;
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                            new KeyValuePair<TKey, TValue>(key, value),
                            new KeyValuePair<TKey, TValue>(key, origValue)));
                }
                else
                {
                    internalDict[key] = value;
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                            new KeyValuePair<TKey, TValue>(key, value)));
                }
            }
        }

        public ICollection<TKey> Keys => internalDict.Keys;
        public ICollection<TValue> Values => internalDict.Values;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
    }
}