using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitySimplified.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TElement">The type of the actual elements that are stored</typeparam>
    /// <typeparam name="TPriority">The type of the priority.  It probably makes sense to be an int or long, \
    /// but any type that can be the key of a SortedDictionary will do.</typeparam>
    public class PriorityQueue<TElement, TPriority> where TPriority : IComparable
    {
        private readonly SortedDictionary<TPriority, Queue<TElement>> _dictionary;
        private int _count;

        public PriorityQueue()
        {   _dictionary = new SortedDictionary<TPriority, Queue<TElement>>(new DescendingComparer<TPriority>());   }

        public int Count()
        {   return _count;   }

        public void Insert(TElement item, TPriority priority)
        {
            Queue<TElement> queue;
            if (!_dictionary.TryGetValue(priority, out queue))
            {
                queue = new Queue<TElement>();
                _dictionary.Add(priority, queue);
            }

            _count++;
            queue.Enqueue(item);
        }

        public TElement Top()
        {
            if (_dictionary.Count == 0)
                throw new Exception("No items to Dequeue:");

            var pair = _dictionary.First();
            var output = pair.Value.Peek();

            return output;
        }

        public TElement Pop()
        {
            if (_dictionary.Count == 0)
                throw new Exception("No items to Dequeue:");

            var pair = _dictionary.First();
            var output = pair.Value.Dequeue();

            if (pair.Value.Count == 0)
                _dictionary.Remove(pair.Key);

            _count--;
            return output;
        }

        private class DescendingComparer<T> : IComparer<T> where T : IComparable
        {
            public int Compare(T x, T y)
            {   return y.CompareTo(x);   }
        }
    }
}