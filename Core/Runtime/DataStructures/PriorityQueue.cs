using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnitySimplified
{
    /// <summary>
    /// A <see cref="KeyValuePair{TKey,TValue}"/> queue collection that sorts values accordingly to their priority value.
    /// </summary>
    /// 
    /// <typeparam name="TPriority">
    /// The type to be compared to other <see cref="TPriority"/> values.
    /// <br/>
    /// Intended to be used with numerical values, but should work with any value that inherits <see cref="IComparable"/>.
    /// </typeparam>
    /// 
    /// <typeparam name="TValue">
    /// The value type that is to be stored and retrieved.
    /// </typeparam>
    public class PriorityQueue<TPriority, TValue> : IEnumerable<KeyValuePair<TPriority, TValue>>
        where TPriority : IComparable
    {
        private readonly SortedDictionary<TPriority, LinkedList<TValue>> _dictionary;
        private readonly List<TPriority> _keysToBeRemoved = new();

        public int Count { get; private set; }


        public PriorityQueue() => _dictionary =
            new SortedDictionary<TPriority, LinkedList<TValue>>(new DescendingComparer<TPriority>());

        public PriorityQueue(IComparer<TPriority> comparer) =>
            _dictionary = new SortedDictionary<TPriority, LinkedList<TValue>>(comparer);

        public PriorityQueue(PriorityQueue<TPriority, TValue> other) : this(other._dictionary.Comparer)
        {
            using var enumerator =
                ((IEnumerable<KeyValuePair<TPriority, LinkedList<TValue>>>)_dictionary).GetEnumerator();
            while (enumerator.MoveNext())
            {
                LinkedList<TValue> elements = new();
                _dictionary.Add(enumerator.Current.Key, elements);
                foreach (var item in enumerator.Current.Value)
                {
                    Count++;
                    elements.AddLast(item);
                }
            }
        }



        IEnumerator IEnumerable.GetEnumerator() =>
            (from pair in _dictionary where pair.Value != null from value in pair.Value select (pair.Key, value))
            .GetEnumerator();

        IEnumerator<KeyValuePair<TPriority, TValue>> IEnumerable<KeyValuePair<TPriority, TValue>>.GetEnumerator() =>
            (from pair in _dictionary
                where pair.Value != null
                from value in pair.Value
                select new KeyValuePair<TPriority, TValue>(pair.Key, value)).GetEnumerator();

        public void Clear() => DoClear();
        public bool Contains(TPriority priority) => DoContains(priority);
        public bool Contains(TValue value) => DoContains(value);
        public void Add(TPriority priority, TValue value) => DoAdd(priority, value);
        public bool Remove(TValue value) => DoRemove(value);
        public bool Pop(out KeyValuePair<TPriority, TValue> pair) => DoPop(out pair);

        public bool Pop(out TValue value)
        {
            bool popResult = DoPop(out var pair);
            value = pair.Value;
            return popResult;
        }

        public bool Peek(out KeyValuePair<TPriority, TValue> pair) => DoPeek(out pair);

        public bool Peek(out TValue value)
        {
            bool peekResult = DoPeek(out var pair);
            value = pair.Value;
            return peekResult;
        }



        private void DoClear()
        {
            _dictionary.Clear();
            Count = 0;
        }

        private bool DoContains(TPriority priority) => _dictionary.ContainsKey(priority);
        private bool DoContains(TValue value) => _dictionary.Any(pair => pair.Value.Contains(value));

        private void DoAdd(TPriority priority, TValue value)
        {
            if (!_dictionary.TryGetValue(priority, out LinkedList<TValue> items))
                _dictionary.Add(priority, items = new LinkedList<TValue>());

            Count++;
            items.AddLast(value);
        }

        private bool DoRemove(TValue value)
        {
            bool result = false;
            foreach (var pair in _dictionary.Where(pair => pair.Value.Remove(value)))
            {
                if (pair.Value.Count == 0)
                    _keysToBeRemoved.Add(pair.Key);
                Count--;
                result = true;
                break;
            }

            foreach (var key in _keysToBeRemoved)
                _dictionary.Remove(key);
            _keysToBeRemoved.Clear();

            return result;
        }

        private bool DoPeek(out KeyValuePair<TPriority, TValue> value)
        {
            value = default;
            if (Count == 0)
                return false;

            var pair = _dictionary.First();
            var output = pair.Value.First();

            value = new KeyValuePair<TPriority, TValue>(pair.Key, output);
            return true;
        }

        private bool DoPop(out KeyValuePair<TPriority, TValue> value)
        {
            value = default;
            if (Count == 0)
                return false;

            var pair = _dictionary.First();
            var output = pair.Value.First();
            pair.Value.RemoveFirst();

            if (pair.Value.Count == 0)
                _dictionary.Remove(pair.Key);

            Count--;
            value = new KeyValuePair<TPriority, TValue>(pair.Key, output);
            return true;
        }


        public class DescendingComparer<T> : IComparer<T> where T : IComparable
        {
            public int Compare(T x, T y) => y.CompareTo(x);
        }

        public class AscendingComparer<T> : IComparer<T> where T : IComparable
        {
            public int Compare(T x, T y) => x.CompareTo(y);
        }
    }
}