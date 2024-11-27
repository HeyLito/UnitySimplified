using System;

namespace UnitySimplified.Collections.Pools
{
    /// <summary>
    /// A modified version of <see cref="UnityEngine.Pool.PooledObject{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ObjectPoolItem<T> : IDisposable where T : class
    {
        private readonly T _toReturn;

        private readonly IObjectPool<T> _pool;

        public ObjectPoolItem(T value, IObjectPool<T> pool)
        {
            _toReturn = value;
            _pool = pool;
        }

        void IDisposable.Dispose() => _pool.Sleep(_toReturn);
    }
}