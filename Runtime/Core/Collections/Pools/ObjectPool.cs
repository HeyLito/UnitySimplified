using System;
using System.Collections.Generic;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ForCanBeConvertedToForeach
namespace UnitySimplified.Collections.Pools
{
    /// <summary>
    /// A modified version of <see cref="UnityEngine.Pool.ObjectPool{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : IDisposable, IObjectPool<T> where T : class
    {
        private readonly bool _collectionCheck;
        private readonly List<T> _list;
        private readonly Func<T> _funcOnCreate;
        private readonly Action<T> _actionOnGet;
        private readonly Action<T> _actionOnSleep;
        private readonly Action<T> _actionOnDestroy;

        public ObjectPool(Func<T> funcOnCreate, Action<T> actionOnGet = null, Action<T> actionOnSleep = null, Action<T> actionOnDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (maxSize <= 0)
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));

            _list = new List<T>(defaultCapacity);
            _funcOnCreate = funcOnCreate ?? throw new ArgumentNullException(nameof(funcOnCreate));
            _actionOnGet = actionOnGet;
            _actionOnSleep = actionOnSleep;
            _actionOnDestroy = actionOnDestroy;
            _collectionCheck = collectionCheck;
            MaxSize = maxSize;
        }

        public int CountAll { get; private set; }
        public int CountActive => CountAll - CountInactive;
        public int CountInactive => _list.Count;
        public int MaxSize { get; }
        public IReadOnlyList<T> List => _list;

        public ObjectPoolItem<T> Get(out T v) => new(v = Get(), this);
        public T Get()
        {
            T element;
            if (_list.Count == 0)
            {
                element = _funcOnCreate();
                CountAll++;
            }
            else
            {
                int index = _list.Count - 1;
                element = _list[index];
                _list.RemoveAt(index);
            }

            _actionOnGet?.Invoke(element);
            return element;
        }

        /// <summary>
        /// Attempts to cache <paramref name="element"/> into a collection.
        /// <br/>
        /// ⠀⠀⠀If <see cref="CountInactive">CountInactive</see> is less than <see cref="MaxSize">MaxSize</see>, this will continue to cache <paramref name="element"/> and invoke the callback <see cref="_actionOnSleep">actionOnSleep</see>.
        /// <br/>
        /// ⠀⠀⠀Otherwise, this will instead destroy <paramref name="element"/> and invoke the <see cref="_actionOnDestroy">actionOnDestroy</see> callback.
        /// </summary>
        /// <param name="element"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Sleep(T element)
        {
            if (_collectionCheck && _list.Count > 0)
                for (int i = 0; i < _list.Count; i++)
                    if (element == _list[i])
                        throw new InvalidOperationException("Trying to sleep an object that has already been cached to the pool.");

            if (CountInactive < MaxSize)
            {
                _list.Add(element);
                _actionOnSleep?.Invoke(element);
            }
            else _actionOnDestroy?.Invoke(element);
        }

        public void Delete(T element)
        {
            if (_list.Count <= 0)
                return;
            for (int i = 0; i < _list.Count; i++)
                if (element == _list[i])
                {
                    CountAll--;
                    _list.RemoveAt(i);
                    _actionOnDestroy?.Invoke(element);
                    break;
                }
        }

        public void Dispose() => Clear();
        public void Clear()
        {
            if (_actionOnDestroy != null)
                foreach (T element in _list)
                    _actionOnDestroy(element);

            CountAll = 0;
            _list.Clear();
        }
    }
}