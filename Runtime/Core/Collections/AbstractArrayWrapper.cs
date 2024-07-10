using System;
using System.Collections;

namespace UnitySimplified.Collections
{
    [Serializable]
    public abstract class AbstractArrayWrapper<T> : IList, IStructuralComparable, IStructuralEquatable, ICloneable
    {
        protected abstract T[] Items { get; }

        public int Length => Items.Length;
        public bool IsFixedSize => Items.IsFixedSize;
        public T this[int index] { get => Items[index]; set => Items[index] = value; }

        object IList.this[int index] { get => ((IList)Items)[index]; set => ((IList)Items)[index] = value; }
        bool IList.IsReadOnly => Items.IsReadOnly;
        int ICollection.Count => ((ICollection)Items).Count;
        bool ICollection.IsSynchronized => Items.IsSynchronized;
        object ICollection.SyncRoot => Items.SyncRoot;

        public IEnumerator GetEnumerator() => Items.GetEnumerator();
        void IList.Clear() => ((IList)Items).Clear();
        bool IList.Contains(object value) => ((IList)Items).Contains(value);
        void IList.Insert(int index, object value) => ((IList)Items).Insert(index, value);
        int IList.IndexOf(object value) => ((IList)Items).IndexOf(value);
        int IList.Add(object value) => ((IList)Items).Add(value);
        void IList.Remove(object value) => ((IList)Items).Remove(value);
        void IList.RemoveAt(int index) => ((IList)Items).RemoveAt(index);
        void ICollection.CopyTo(Array array, int index) => Items.CopyTo(array, index);
        int IStructuralComparable.CompareTo(object other, IComparer comparer) => ((IStructuralComparable)Items).CompareTo(other, comparer);
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)Items).GetHashCode(comparer);
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) => ((IStructuralEquatable)Items).Equals(other, comparer);
        object ICloneable.Clone() => Items.Clone();
    }
}