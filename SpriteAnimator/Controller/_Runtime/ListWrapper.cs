using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Collections
{
    [Serializable]
    public class ListWrapper<T> : IReadOnlyList<T>, IList<T>, IList
    {
        [SerializeField]
        private List<T> _items = new();

        
        bool ICollection.IsSynchronized => ((ICollection)_items).IsSynchronized;
        object ICollection.SyncRoot => ((ICollection)_items).SyncRoot;
        bool IList.IsFixedSize => ((IList)_items).IsFixedSize;
        
        public int Count => ((ICollection<T>)_items).Count;
        public bool IsReadOnly => ((ICollection<T>)_items).IsReadOnly;

        

        object IList.this[int index] { get => ((IList)_items)[index]; set => ((IList)_items)[index] = value; }
        public T this[int index] { get => ((IList<T>)_items)[index]; set => ((IList<T>)_items)[index] = value; }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
        int IList.IndexOf(object value) => ((IList)_items).IndexOf(value);
        int IList.Add(object value) => ((IList)_items).Add(value);
        void IList.Insert(int index, object value) => ((IList)_items).Insert(index, value);
        void IList.Remove(object value) => ((IList)_items).Remove(value);
        bool IList.Contains(object value) => ((IList)_items).Contains(value);
        void ICollection.CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);
        
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();
        public void CopyTo(T[] array, int arrayIndex) => ((ICollection<T>)_items).CopyTo(array, arrayIndex);
        public void Clear() => ((ICollection<T>)_items).Clear();
        public int IndexOf(T item) => ((IList<T>)_items).IndexOf(item);
        public bool Contains(T item) => ((ICollection<T>)_items).Contains(item);
        public void Insert(int index, T item) => ((IList<T>)_items).Insert(index, item);
        public void Add(T item) => ((ICollection<T>)_items).Add(item);
        public bool Remove(T item) => ((ICollection<T>)_items).Remove(item);
        public void RemoveAt(int index) => ((IList<T>)_items).RemoveAt(index);
    }
}
