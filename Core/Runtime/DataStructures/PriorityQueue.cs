using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitySimplified
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TElement">The type of the actual elements that are stored</typeparam>
    /// <typeparam name="TPriority">The type of the priority.  It probably makes sense to be an int or long, \
    /// but any type that can be the key of a SortedDictionary will do.</typeparam>
    public class PriorityQueue<TPriority, TElement> where TPriority : IComparable
    {
        private readonly SortedDictionary<TPriority, Queue<TElement>> _dictionary;
        private int _count;

        public int Count => _count;

        public PriorityQueue() => _dictionary = new SortedDictionary<TPriority, Queue<TElement>>(new DescendingComparer<TPriority>());
        public PriorityQueue(IComparer<TPriority> comparer) => _dictionary = new SortedDictionary<TPriority, Queue<TElement>>(comparer);
        public PriorityQueue(PriorityQueue<TPriority, TElement> other)
        {   
            _dictionary = new SortedDictionary<TPriority, Queue<TElement>>(new DescendingComparer<TPriority>());
            foreach (var pair in other)
            {
                Queue<TElement> queue = new Queue<TElement>();
                _dictionary.Add(pair.Key, queue);
                foreach (var item in pair.Value)
                {
                    _count++;
                    queue.Enqueue(item);
                }
            }
        }

        public void Insert(TPriority priority, TElement item)
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
        public KeyValuePair<TPriority, TElement> TopPair()
        {
            if (_dictionary.Count == 0)
                throw new Exception("No items to Dequeue:");

            var pair = _dictionary.First();
            var output = pair.Value.Peek();

            return new KeyValuePair<TPriority, TElement>(pair.Key, output);
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
        public KeyValuePair<TPriority, TElement> PopPair()
        {
            if (_dictionary.Count == 0)
                throw new Exception("No items to Dequeue:");

            var pair = _dictionary.First();
            var output = pair.Value.Dequeue();

            if (pair.Value.Count == 0)
                _dictionary.Remove(pair.Key);

            _count--;
            return new KeyValuePair<TPriority, TElement>(pair.Key, output);
        }
        public void Clear()
        {
            _dictionary.Clear();
            _count = 0;
        }

        public IEnumerator<KeyValuePair<TPriority, Queue<TElement>>> GetEnumerator() => ((IEnumerable<KeyValuePair<TPriority, Queue<TElement>>>)_dictionary).GetEnumerator();

        public class DescendingComparer<T> : IComparer<T> where T : IComparable
        {
            public int Compare(T x, T y) => y.CompareTo(x);
        }
    }
}