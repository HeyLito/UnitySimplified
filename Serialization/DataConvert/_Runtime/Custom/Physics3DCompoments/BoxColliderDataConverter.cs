using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class BoxColliderDataConverter : IDataConverter
    {
        int IDataConverter.GetConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(BoxCollider);
        void IDataConverter.Serialize(object target, IDictionary<string, object> output, SerializerFlags flags)
        {
            var obj = target as BoxCollider;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                output[nameof(BoxCollider.isTrigger)] = obj.isTrigger;
                output[nameof(BoxCollider.center)] = obj.center;
                output[nameof(BoxCollider.size)] = obj.size;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataConvertUtility.SerializeFieldAsset(nameof(BoxCollider.sharedMaterial), obj.sharedMaterial, output);
            }

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
                output[nameof(BoxCollider.contactOffset)] = obj.contactOffset;
        }
        void IDataConverter.Deserialize(ref object target, IDictionary<string, object> input, SerializerFlags flags)
        {
            var obj = target as BoxCollider;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (input.TryGetValue(nameof(BoxCollider.isTrigger), out object isTrigger))
                    obj.isTrigger = (bool)isTrigger;
                if (input.TryGetValue(nameof(BoxCollider.center), out object center))
                    obj.center = (Vector3)center;
                if (input.TryGetValue(nameof(BoxCollider.size), out object size))
                    obj.size = (Vector3)size;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataConvertUtility.DeserializeFieldAsset(nameof(BoxCollider.sharedMaterial), out object sharedMaterial, typeof(PhysicMaterial), input))
                        obj.sharedMaterial = (PhysicMaterial)sharedMaterial;
            }

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
                if (input.TryGetValue(nameof(BoxCollider.contactOffset), out object contactOffset))
                    obj.contactOffset = (float)contactOffset;
        }
    }
}