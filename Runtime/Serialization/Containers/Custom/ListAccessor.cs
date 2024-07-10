using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization.Containers
{
    [Serializable]
    public class ListAccessor : Accessor
    {
        [SerializeField]
        private string listType;
        [SerializeReference]
        private List<Accessor> values = new();

        public override bool CanConvert(Type valueType) => typeof(IList).IsAssignableFrom(valueType);

        public override void Set(object value)
        {
            listType = value.GetType().FullName;
            foreach (var item in (IList)value)
                if (TryCreate(item.GetType(), out Accessor accessor))
                {
                    accessor.Set(item);
                    values.Add(accessor);
                }

        }
        public override void Get(out object value)
        {
            value = null;
            Type valueType = Type.GetType(listType);
            if (valueType == null)
                return;
            value = Activator.CreateInstance(valueType);
            if (value is not IList valueList)
                return;

            foreach (var accessor in values)
            {
                if (accessor is null)
                    continue;

                accessor.Get(out object accessorValue);
                valueList.Add(accessorValue);
            }
        }
    }
}