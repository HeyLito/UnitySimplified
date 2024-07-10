using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Collections
{
    [Serializable]
    public class ListWrapper<T> : AbstractListWrapper<T>
    {
        [SerializeField]
        private List<T> items = new();
        protected override List<T> Items => items;
    }
}