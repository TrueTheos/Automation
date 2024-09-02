using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        [System.Serializable]
        public class OBJ
        {
            public TKey Key;
            public TValue Value;
        }

        public List<OBJ> dictionary = new List<OBJ>();

        public ICollection<TKey> Keys => dictionary.Select(x => x.Key).ToList();
        public ICollection<TValue> Values => dictionary.Select(x => x.Value).ToList();
        public int Count => dictionary.Count;
        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get
            {
                OBJ item = dictionary.Find(x => EqualityComparer<TKey>.Default.Equals(x.Key, key));
                if (item == null)
                    throw new KeyNotFoundException();
                return item.Value;
            }
            set
            {
                OBJ item = dictionary.Find(x => EqualityComparer<TKey>.Default.Equals(x.Key, key));
                if (item == null)
                    dictionary.Add(new OBJ { Key = key, Value = value });
                else
                    item.Value = value;
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            dictionary.Add(new OBJ { Key = key, Value = value });
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.Exists(x => EqualityComparer<TKey>.Default.Equals(x.Key, key));
        }

        public bool Remove(TKey key)
        {
            return dictionary.RemoveAll(x => EqualityComparer<TKey>.Default.Equals(x.Key, key)) > 0;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            OBJ item = dictionary.Find(x => EqualityComparer<TKey>.Default.Equals(x.Key, key));
            if (item != null)
            {
                value = item.Value;
                return true;
            }
            value = default;
            return false;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.Exists(x =>
                EqualityComparer<TKey>.Default.Equals(x.Key, item.Key) &&
                EqualityComparer<TValue>.Default.Equals(x.Value, item.Value));
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("The number of elements in the source dictionary is greater than the available space from arrayIndex to the end of the destination array.");

            foreach (var item in dictionary)
            {
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(item.Key, item.Value);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.RemoveAll(x =>
                EqualityComparer<TKey>.Default.Equals(x.Key, item.Key) &&
                EqualityComparer<TValue>.Default.Equals(x.Value, item.Value)) > 0;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}