using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class Rigidbody2DDataConverter : IDataConverter
    {
        int IDataConverter.ConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(Rigidbody2D);
        void IDataConverter.Serialize(Type valueType, object value, IDictionary<string, object> output)
        {
            var obj = value as Rigidbody2D;
            if (obj == null)
                return;

            output[nameof(Rigidbody2D.bodyType)] = (int)obj.bodyType;
            output[nameof(Rigidbody2D.simulated)] = obj.simulated;
            output[nameof(Rigidbody2D.useAutoMass)] = obj.useAutoMass;
            output[nameof(Rigidbody2D.mass)] = obj.mass;
            output[nameof(Rigidbody2D.drag)] = obj.drag;
            output[nameof(Rigidbody2D.gravityScale)] = obj.gravityScale;
            output[nameof(Rigidbody2D.collisionDetectionMode)] = (int)obj.collisionDetectionMode;
            output[nameof(Rigidbody2D.sleepMode)] = (int)obj.sleepMode;
            output[nameof(Rigidbody2D.interpolation)] = (int)obj.interpolation;
            output[nameof(Rigidbody2D.constraints)] = (int)obj.constraints;

            DataConvertUtility.TrySerializeFieldAsset(nameof(Rigidbody2D.sharedMaterial), obj.sharedMaterial, output);

            output[nameof(Rigidbody2D.position)] = obj.position;
            output[nameof(Rigidbody2D.rotation)] = obj.rotation;
            output[nameof(Rigidbody2D.velocity)] = obj.velocity;
            output[nameof(Rigidbody2D.angularVelocity)] = obj.angularVelocity;
            output[nameof(Rigidbody2D.inertia)] = obj.inertia;
            output[nameof(Rigidbody2D.centerOfMass)] = obj.centerOfMass;
            output[nameof(Rigidbody2D.IsSleeping)] = obj.IsSleeping();
        }
        object IDataConverter.Deserialize(Type valueType, object existingValue, IDictionary<string, object> input)
        {
            var obj = existingValue as Rigidbody2D;
            if (obj == null)
                return null;

            if (input.TryGetValue(nameof(Rigidbody2D.bodyType), out object bodyType))
                obj.bodyType = (RigidbodyType2D)bodyType;
            if (input.TryGetValue(nameof(Rigidbody2D.simulated), out object simulated))
                obj.simulated = (bool)simulated;
            if (input.TryGetValue(nameof(Rigidbody2D.useAutoMass), out object useAutoMass))
                obj.useAutoMass = (bool)useAutoMass;
            if (input.TryGetValue(nameof(Rigidbody2D.mass), out object mass))
                obj.mass = (float)mass;
            if (input.TryGetValue(nameof(Rigidbody2D.drag), out object drag))
                obj.drag = (float)drag;
            if (input.TryGetValue(nameof(Rigidbody2D.gravityScale), out object gravityScale))
                obj.gravityScale = (float)gravityScale;
            if (input.TryGetValue(nameof(Rigidbody2D.collisionDetectionMode), out object collisionDetectionMode))
                obj.collisionDetectionMode = (CollisionDetectionMode2D)collisionDetectionMode;
            if (input.TryGetValue(nameof(Rigidbody2D.sleepMode), out object sleepingMode))
                obj.sleepMode = (RigidbodySleepMode2D)sleepingMode;
            if (input.TryGetValue(nameof(Rigidbody2D.interpolation), out object interpolation))
                obj.interpolation = (RigidbodyInterpolation2D)interpolation;
            if (input.TryGetValue(nameof(Rigidbody2D.constraints), out object constraints))
                obj.constraints = (RigidbodyConstraints2D)constraints;

            if (DataConvertUtility.TryDeserializeFieldAsset(nameof(Rigidbody2D.sharedMaterial), out object sharedMaterial, input))
                obj.sharedMaterial = (PhysicsMaterial2D)sharedMaterial;

            if (input.TryGetValue(nameof(Rigidbody2D.position), out object position))
                obj.position = (Vector2)position;
            if (input.TryGetValue(nameof(Rigidbody2D.rotation), out object rotation))
                obj.rotation = (float)rotation;
            if (input.TryGetValue(nameof(Rigidbody2D.velocity), out object velocity))
                obj.velocity = (Vector2)velocity;
            if (input.TryGetValue(nameof(Rigidbody2D.angularVelocity), out object angularVelocity))
                obj.angularVelocity = (float)angularVelocity;
            if (input.TryGetValue(nameof(Rigidbody2D.inertia), out object inertia))
                obj.inertia = (float)inertia;
            if (input.TryGetValue(nameof(Rigidbody2D.centerOfMass), out object localCenterOfMass))
                obj.centerOfMass = (Vector2)localCenterOfMass;
            if (input.TryGetValue(nameof(Rigidbody2D.IsSleeping), out object sleepState) && (bool)sleepState != obj.IsSleeping())
                if ((bool)sleepState)
                    obj.Sleep();
                else obj.WakeUp();

            return obj;
        }
    }
}