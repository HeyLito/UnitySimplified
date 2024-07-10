using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class GameObjectDataConverter : IDataConverter<GameObject>
    {
        int IDataConverter.ConversionPriority() => -1;

        void IDataConverter<GameObject>.Serialize(GameObject value, IDictionary<string, object> output)
        {
            output[nameof(GameObject.name)] = value.name;
            output[nameof(GameObject.tag)] = value.tag;
            output[nameof(GameObject.layer)] = value.layer;
            output[nameof(GameObject.isStatic)] = value.isStatic;
            output[nameof(GameObject.activeSelf)] = value.activeSelf;
        }
        GameObject IDataConverter<GameObject>.Deserialize(GameObject existingValue, IDictionary<string, object> input)
        {
            if (input.TryGetValue(nameof(GameObject.name), out object name))
                existingValue.name = (string)name;
            if (input.TryGetValue(nameof(GameObject.tag), out object tag))
                existingValue.tag = (string)tag;
            if (input.TryGetValue(nameof(GameObject.layer), out object layer))
                existingValue.layer = (int)layer;
            if (input.TryGetValue(nameof(GameObject.isStatic), out object isStatic))
                existingValue.isStatic = (bool)isStatic;
            if (input.TryGetValue(nameof(GameObject.activeSelf), out object active))
                existingValue.SetActive((bool)active);
            return existingValue;
        }
    }
}