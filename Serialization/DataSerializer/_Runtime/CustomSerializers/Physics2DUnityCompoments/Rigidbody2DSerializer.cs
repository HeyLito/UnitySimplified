using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(Rigidbody2D), -1)]
    public sealed class Rigidbody2DSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as Rigidbody2D;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                fieldData[nameof(Rigidbody2D.bodyType)] = (int)obj.bodyType;
                fieldData[nameof(Rigidbody2D.simulated)] = obj.simulated;
                fieldData[nameof(Rigidbody2D.useAutoMass)] = obj.useAutoMass;
                fieldData[nameof(Rigidbody2D.mass)] = obj.mass;
                fieldData[nameof(Rigidbody2D.drag)] = obj.drag;
                fieldData[nameof(Rigidbody2D.gravityScale)] = obj.gravityScale;
                fieldData[nameof(Rigidbody2D.collisionDetectionMode)] = (int)obj.collisionDetectionMode;
                fieldData[nameof(Rigidbody2D.sleepMode)] = (int)obj.sleepMode;
                fieldData[nameof(Rigidbody2D.interpolation)] = (int)obj.interpolation;
                fieldData[nameof(Rigidbody2D.constraints)] = (int)obj.constraints;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataSerializerUtility.SerializeFieldAsset(nameof(Rigidbody2D.sharedMaterial), obj.sharedMaterial, fieldData);
            }

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
            {
                fieldData[nameof(Rigidbody2D.position)] = obj.position;
                fieldData[nameof(Rigidbody2D.rotation)] = obj.rotation;
                fieldData[nameof(Rigidbody2D.velocity)] = obj.velocity;
                fieldData[nameof(Rigidbody2D.angularVelocity)] = obj.angularVelocity;
                fieldData[nameof(Rigidbody2D.inertia)] = obj.inertia;
                fieldData[nameof(Rigidbody2D.centerOfMass)] = obj.centerOfMass;
                fieldData[nameof(Rigidbody2D.IsSleeping)] = obj.IsSleeping();
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as Rigidbody2D;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(Rigidbody2D.bodyType), out object bodyType))
                    obj.bodyType = (RigidbodyType2D)bodyType;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.simulated), out object simulated))
                    obj.simulated = (bool)simulated;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.useAutoMass), out object useAutoMass))
                    obj.useAutoMass = (bool)useAutoMass;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.mass), out object mass))
                    obj.mass = (float)mass;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.drag), out object drag))
                    obj.drag = (float)drag;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.gravityScale), out object gravityScale))
                    obj.gravityScale = (float)gravityScale;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.collisionDetectionMode), out object collisionDetectionMode))
                    obj.collisionDetectionMode = (CollisionDetectionMode2D)collisionDetectionMode;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.sleepMode), out object sleepingMode))
                    obj.sleepMode = (RigidbodySleepMode2D)sleepingMode;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.interpolation), out object interpolation))
                    obj.interpolation = (RigidbodyInterpolation2D)interpolation;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.constraints), out object constraints))
                    obj.constraints = (RigidbodyConstraints2D)constraints;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataSerializerUtility.DeserializeFieldAsset(nameof(Rigidbody2D.sharedMaterial), out object sharedMaterial, typeof(PhysicsMaterial2D), fieldData))
                        obj.sharedMaterial = (PhysicsMaterial2D)sharedMaterial;
            }

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(Rigidbody2D.position), out object position))
                    obj.position = (Vector2)position;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.rotation), out object rotation))
                    obj.rotation = (float)rotation;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.velocity), out object velocity))
                    obj.velocity = (Vector2)velocity;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.angularVelocity), out object angularVelocity))
                    obj.angularVelocity = (float)angularVelocity;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.inertia), out object inertia))
                    obj.inertia = (float)inertia;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.centerOfMass), out object localCenterOfMass))
                    obj.centerOfMass = (Vector2)localCenterOfMass;
                if (fieldData.TryGetValue(nameof(Rigidbody2D.IsSleeping), out object sleepState) && (bool)sleepState != obj.IsSleeping())
                    if ((bool)sleepState)
                        obj.Sleep();
                    else obj.WakeUp();
            }
        }
    }
}