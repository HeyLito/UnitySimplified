using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(GameObject), -1)]
    public sealed class GameObjectSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as GameObject;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                fieldData[nameof(GameObject.name)] = obj.name;
                fieldData[nameof(GameObject.tag)] = obj.tag;
                fieldData[nameof(GameObject.layer)] = obj.layer;
                fieldData[nameof(GameObject.isStatic)] = obj.isStatic;
                fieldData[nameof(GameObject.activeSelf)] = obj.activeSelf;
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as GameObject;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(GameObject.name), out object name))
                    obj.name = (string)name;
                if (fieldData.TryGetValue(nameof(GameObject.tag), out object tag))
                    obj.tag = (string)tag;
                if (fieldData.TryGetValue(nameof(GameObject.layer), out object layer))
                    obj.layer = (int)layer;
                if (fieldData.TryGetValue(nameof(GameObject.isStatic), out object isStatic))
                    obj.isStatic = (bool)isStatic;
                if (fieldData.TryGetValue(nameof(GameObject.activeSelf), out object active))
                    obj.SetActive((bool)active);
            }
        }
    }
}