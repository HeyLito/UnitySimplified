using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnitySimplified.Collections
{
    [Serializable]
    public class ListWrapper<T> : AbstractListWrapper<T>
    {
        [SerializeField]
        [FormerlySerializedAs("_items")]
        private List<T> items = new();
        protected override List<T> Items => items;
    }
}