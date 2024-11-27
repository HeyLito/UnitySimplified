namespace UnitySimplified.Collections.Pools
{
    /// <summary>
    /// A modified version of <see cref="UnityEngine.Pool.IObjectPool{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectPool<T> where T : class
    {
        int CountInactive { get; }

        ObjectPoolItem<T> Get(out T v);
        T Get();
        void Clear();
        void Sleep(T element);
    }
}