using System;
using System.Collections;
using System.Collections.Generic;

namespace UnitySimplified.Collections
{
    [Serializable]
    public abstract class AbstractListWrapper<T> : IList, IList<T>, IReadOnlyList<T>
    {
        protected abstract List<T> Items { get; }

        public int Count => ((ICollection<T>)Items).Count;
        public bool IsReadOnly => ((ICollection<T>)Items).IsReadOnly;
        public T this[int index] { get => ((IList<T>)Items)[index]; set => ((IList<T>)Items)[index] = value; }

        bool IList.IsFixedSize => ((IList)Items).IsFixedSize;
        bool ICollection.IsSynchronized => ((ICollection)Items).IsSynchronized;
        object ICollection.SyncRoot => ((ICollection)Items).SyncRoot;
        object IList.this[int index] { get => ((IList)Items)[index]; set => ((IList)Items)[index] = value; }

        public void Clear() => ((ICollection<T>)Items).Clear();
        public bool Contains(T item) => ((ICollection<T>)Items).Contains(item);
        public void Insert(int index, T item) => ((IList<T>)Items).Insert(index, item);
        public int IndexOf(T item) => ((IList<T>)Items).IndexOf(item);
        public void Add(T item) => ((ICollection<T>)Items).Add(item);
        public bool Remove(T item) => ((ICollection<T>)Items).Remove(item);
        public void RemoveAt(int index) => ((IList<T>)Items).RemoveAt(index);
        public void CopyTo(T[] array, int arrayIndex) => ((ICollection<T>)Items).CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Items).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Items).GetEnumerator();
        bool IList.Contains(object value) => ((IList)Items).Contains(value);
        void IList.Insert(int index, object value) => ((IList)Items).Insert(index, value);
        int IList.IndexOf(object value) => ((IList)Items).IndexOf(value);
        int IList.Add(object value) => ((IList)Items).Add(value);
        void IList.Remove(object value) => ((IList)Items).Remove(value);
        public void CopyTo(Array array, int index) => ((ICollection)Items).CopyTo(array, index);
    }
}