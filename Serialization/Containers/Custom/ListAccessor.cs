using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySimplified.Serialization.Containers;

[Serializable]
public class ListAccessor : Accessor
{
    [SerializeField]
    private string listType;
    [SerializeField]
    private List<object> values = new();

    public override bool CanAccess(Type valueType) => typeof(IList).IsAssignableFrom(valueType);

    public override void Set(object value)
    {
        listType = value.GetType().AssemblyQualifiedName;
        foreach (var item in (IList)value)
            values.Add(item);
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

        foreach (var item in values)
        {
            Debug.Log(item.GetType());
            valueList.Add(item);
        }
    }
}
