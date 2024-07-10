using System.Collections;
using System.Collections.Generic;

namespace UnitySimplified.Collections
{
    /// https://gmamaladze.wordpress.com/2013/07/25/hashset-that-preserves-insertion-order-or-net-implementation-of-linkedhashset/
    public class OrderedSet<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        private readonly IDictionary<T, LinkedListNode<T>> m_Dictionary;
        private readonly LinkedList<T> m_LinkedList;

        public int Count => m_Dictionary.Count;
        public virtual bool IsReadOnly => m_Dictionary.IsReadOnly;


        public OrderedSet() : this(EqualityComparer<T>.Default) { }
        public OrderedSet(IEqualityComparer<T> comparer)
        {
            m_Dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
            m_LinkedList = new LinkedList<T>();
        }



        public IEnumerator<T> GetEnumerator() => m_LinkedList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<T>.Add(T item) => Add(item);
        public bool Contains(T item) => m_Dictionary.ContainsKey(item);
        public void CopyTo(T[] array, int arrayIndex) => m_LinkedList.CopyTo(array, arrayIndex);
        public void Clear()
        {
            m_LinkedList.Clear();
            m_Dictionary.Clear();
        }
        public bool Add(T item)
        {
            if (m_Dictionary.ContainsKey(item))
                return false;
            LinkedListNode<T> node = m_LinkedList.AddLast(item);
            m_Dictionary.Add(item, node);
            return true;
        }
        public bool Remove(T item)
        {
            LinkedListNode<T> node;
            bool found = m_Dictionary.TryGetValue(item, out node);
            if (!found) return false;
            m_Dictionary.Remove(item);
            m_LinkedList.Remove(node);
            return true;
        }
    }
}
