using System;
using System.Linq;
using UnityEngine;

namespace UnitySimplified.Collections.Pools
{
    public struct ComponentPoolItem<T>
    {
        public T Item;
        public ILookup<Type, Component> ComponentCache;

        internal ComponentPoolItem(T item, ILookup<Type, Component> componentCache)
        {
            Item = item;
            ComponentCache = componentCache;
        }
    }
}