using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class GameObjectDataConverter : IDataConverter
    {
        int IDataConverter.GetConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(GameObject);
        void IDataConverter.Serialize(object instance, IDictionary<string, object> output, SerializerFlags flags)
        {
            var obj = instance as GameObject;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                output[nameof(GameObject.name)] = obj.name;
                output[nameof(GameObject.tag)] = obj.tag;
                output[nameof(GameObject.layer)] = obj.layer;
                output[nameof(GameObject.isStatic)] = obj.isStatic;
                output[nameof(GameObject.activeSelf)] = obj.activeSelf;
            }
        }
        void IDataConverter.Deserialize(ref object instance, IDictionary<string, object> input, SerializerFlags flags)
        {
            var obj = instance as GameObject;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (input.TryGetValue(nameof(GameObject.name), out object name))
                    obj.name = (string)name;
                if (input.TryGetValue(nameof(GameObject.tag), out object tag))
                    obj.tag = (string)tag;
                if (input.TryGetValue(nameof(GameObject.layer), out object layer))
                    obj.layer = (int)layer;
                if (input.TryGetValue(nameof(GameObject.isStatic), out object isStatic))
                    obj.isStatic = (bool)isStatic;
                if (input.TryGetValue(nameof(GameObject.activeSelf), out object active))
                    obj.SetActive((bool)active);
            }
        }
    }
}