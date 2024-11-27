using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

// ReSharper disable once StaticMemberInGenericType
namespace UnitySimplified.Collections.Pools
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ComponentPool<T> : IEnumerable<T>, IObjectPool<T> where T : Component
    {
        public Action<T> ActionOnPreGet;
        public Action<T> ActionOnPostGet;
        public Action<T> ActionOnPreSleep;
        public Action<T> ActionOnPostSleep;
        public Action<T> ActionOnPreDestroy;

        private readonly ObjectPool<T> _objectPool;
        private readonly LinkedList<T> _actives = new();
        private readonly Dictionary<T, ComponentPoolItem<T>> _caches = new();
        private static Transform _poolContainer;

        public ComponentPool(Func<T> funcOnCreate, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (funcOnCreate == null)
                throw new ArgumentNullException(nameof(funcOnCreate));

            T CreateCallback()
            {
                var item = funcOnCreate.Invoke();
                _caches.Add(item, new ComponentPoolItem<T>(item, null));
                DoInitialize(item);
                return item;
            }
            _objectPool = new ObjectPool<T>(CreateCallback, DoGet, DoSleep, DoDestroy, collectionCheck, defaultCapacity, maxSize);
        }
        public ComponentPool(Func<(T item, ILookup<Type, Component> cache)> funcOnCreate, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (funcOnCreate == null)
                throw new ArgumentNullException(nameof(funcOnCreate));

            T CreateCallback()
            {
                var (item, componentCache) = funcOnCreate.Invoke();
                _caches.Add(item, new ComponentPoolItem<T>(item, componentCache));
                DoInitialize(item);
                return item;
            }
            _objectPool = new ObjectPool<T>(CreateCallback, DoGet, DoSleep, DoDestroy, collectionCheck, defaultCapacity, maxSize);
        }

        public int CountActive => _objectPool.CountActive;
        public int CountInactive => _objectPool.CountInactive;
        public int CountAll => _objectPool.CountAll;
        private static Transform PoolContainer
        {
            get
            {
                if (_poolContainer == null)
                    _poolContainer = GameObject.Find("ComponentPoolContainer")?.transform;
                if (_poolContainer == null)
                    _poolContainer = new GameObject("ComponentPoolContainer").transform;
                return _poolContainer;
            }
        }

        ObjectPoolItem<T> IObjectPool<T>.Get(out T v) => _objectPool.Get(out v);
        IEnumerator IEnumerable.GetEnumerator() => _actives.GetEnumerator();
        public IEnumerator<T> GetEnumerator() => _actives.GetEnumerator();
        public IEnumerator<T> GetEnumerator(bool includeInactive) => includeInactive ? _objectPool.List.GetEnumerator() : _actives.GetEnumerator();
        public IEnumerator<ComponentPoolItem<T>> GetItemizedEnumerator()
        {
            using var iterator = GetEnumerator();
            while (iterator.MoveNext())
            {
                var current = iterator.Current;
                if (current == null)
                    continue;
                if (!_caches.TryGetValue(current, out ComponentPoolItem<T> pooledComponent))
                    continue;
                yield return pooledComponent;
            }
        }
        public T Get() => _objectPool.Get();
        public T Get(out ComponentPoolItem<T> item)
        {
            var element = _objectPool.Get();
            item = _caches.GetValueOrDefault(element);
            return element;
        }
        public void Sleep(T element) => _objectPool.Sleep(element);
        public void Clear() => _objectPool.Clear();


        private void DoTriggerDisable(ComponentPoolEventTrigger eventTrigger)
        {
            if (eventTrigger.Target == null || eventTrigger.Target is not T target)
                return;
            if (_actives.Contains(target))
                _objectPool.Sleep(target);
        }
        private void DoTriggerDestroy(ComponentPoolEventTrigger listener)
        {
            if (listener.Target == null || listener.Target is not T target)
                return;
            _objectPool.Delete(target);
        }
        private void DoInitialize(T element)
        {
            if (element.transform.parent == null)
                element.transform.SetParent(PoolContainer, true);
            if (!element.TryGetComponent(out ComponentPoolEventTrigger eventTrigger))
                eventTrigger = element.gameObject.AddComponent<ComponentPoolEventTrigger>();
            eventTrigger.Initialize(element, DoTriggerDisable, DoTriggerDestroy);
        }
        private void DoGet(T element)
        {
            ActionOnPreGet?.Invoke(element);
            _actives.AddLast(element);
            element.gameObject.SetActive(true);
            ActionOnPostGet?.Invoke(element);
        }
        private void DoSleep(T element)
        {
            ActionOnPreSleep?.Invoke(element);
            _actives.Remove(element);
            element.gameObject.SetActive(false);
            ActionOnPostSleep?.Invoke(element);
        }
        private void DoDestroy(T element)
        {
            ActionOnPreDestroy?.Invoke(element);
            _actives.Remove(element);
            UnityObject.Destroy(element.gameObject);
        }
    }
}