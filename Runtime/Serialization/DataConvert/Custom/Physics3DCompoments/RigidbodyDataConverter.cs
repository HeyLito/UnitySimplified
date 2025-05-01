using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class RigidbodyDataConverter : IDataConverter
    {
        int IDataConverter.ConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(Rigidbody);
        void IDataConverter.Serialize(Type valueType, object value, IDictionary<string, object> output)
        {
            var obj = value as Rigidbody;
            if (obj == null)
                return;

            output[nameof(Rigidbody.mass)] = obj.mass;
            output[nameof(Rigidbody.useGravity)] = obj.useGravity;
            output[nameof(Rigidbody.isKinematic)] = obj.isKinematic;
            output[nameof(Rigidbody.interpolation)] = (int)obj.interpolation;
            output[nameof(Rigidbody.collisionDetectionMode)] = (int)obj.collisionDetectionMode;
            output[nameof(Rigidbody.constraints)] = (int)obj.constraints;

            output[nameof(Rigidbody.position)] = obj.position;
            output[nameof(Rigidbody.rotation)] = obj.rotation;
            output[nameof(Rigidbody.centerOfMass)] = obj.centerOfMass;
            output[nameof(Rigidbody.angularVelocity)] = obj.angularVelocity;
            output[nameof(Rigidbody.maxAngularVelocity)] = obj.maxAngularVelocity;
            output[nameof(Rigidbody.maxDepenetrationVelocity)] = obj.maxDepenetrationVelocity;
            output[nameof(Rigidbody.detectCollisions)] = obj.detectCollisions;
            output[nameof(Rigidbody.freezeRotation)] = obj.freezeRotation;
            output[nameof(Rigidbody.inertiaTensor)] = obj.inertiaTensor;
            output[nameof(Rigidbody.inertiaTensorRotation)] = obj.inertiaTensorRotation;
            output[nameof(Rigidbody.sleepThreshold)] = obj.sleepThreshold;
            output[nameof(Rigidbody.solverIterations)] = obj.solverIterations;
            output[nameof(Rigidbody.solverVelocityIterations)] = obj.solverVelocityIterations;
            output[nameof(Rigidbody.IsSleeping)] = obj.IsSleeping();

#if UNITY_6000_0_OR_NEWER
            output[nameof(Rigidbody.linearDamping)] = obj.linearDamping;
            output[nameof(Rigidbody.linearVelocity)] = obj.linearVelocity;
            output[nameof(Rigidbody.angularDamping)] = obj.angularDamping;
#else
            output[nameof(Rigidbody.drag)] = obj.drag;
            output[nameof(Rigidbody.velocity)] = obj.velocity;
            output[nameof(Rigidbody.angularDrag)] = obj.angularDrag;
#endif
        }
        object IDataConverter.Deserialize(Type valueType, object existingValue, IDictionary<string, object> input)
        {
            var obj = existingValue as Rigidbody;
            if (obj == null)
                return null;

            if (input.TryGetValue(nameof(Rigidbody.mass), out object mass))
                obj.mass = (float)mass;
            if (input.TryGetValue(nameof(Rigidbody.useGravity), out object useGravity))
                obj.useGravity = (bool)useGravity;
            if (input.TryGetValue(nameof(Rigidbody.isKinematic), out object isKinematic))
                obj.isKinematic = (bool)isKinematic;
            if (input.TryGetValue(nameof(Rigidbody.interpolation), out object interpolation))
                obj.interpolation = (RigidbodyInterpolation)interpolation;
            if (input.TryGetValue(nameof(Rigidbody.collisionDetectionMode), out object collisionDetectionMode))
                obj.collisionDetectionMode = (CollisionDetectionMode)collisionDetectionMode;
            if (input.TryGetValue(nameof(Rigidbody.constraints), out object constraints))
                obj.constraints = (RigidbodyConstraints)constraints;

            if (input.TryGetValue(nameof(Rigidbody.position), out object position))
                obj.position = (Vector3)position;
            if (input.TryGetValue(nameof(Rigidbody.rotation), out object rotation))
                obj.rotation = (Quaternion)rotation;
            if (input.TryGetValue(nameof(Rigidbody.centerOfMass), out object centerOfMass))
                obj.centerOfMass = (Vector3)centerOfMass;
            if (input.TryGetValue(nameof(Rigidbody.angularVelocity), out object angularVelocity))
                obj.angularVelocity = (Vector3)angularVelocity;
            if (input.TryGetValue(nameof(Rigidbody.maxAngularVelocity), out object maxAngularVelocity))
                obj.maxAngularVelocity = (float)maxAngularVelocity;
            if (input.TryGetValue(nameof(Rigidbody.maxDepenetrationVelocity), out object maxDepenetrationVelocity))
                obj.maxDepenetrationVelocity = (float)maxDepenetrationVelocity;
            if (input.TryGetValue(nameof(Rigidbody.detectCollisions), out object detectCollisions))
                obj.detectCollisions = (bool)detectCollisions;
            if (input.TryGetValue(nameof(Rigidbody.freezeRotation), out object freezeRotation))
                obj.freezeRotation = (bool)freezeRotation;
            if (input.TryGetValue(nameof(Rigidbody.inertiaTensor), out object inertiaTensor))
                obj.inertiaTensor = (Vector3)inertiaTensor;
            if (input.TryGetValue(nameof(Rigidbody.inertiaTensorRotation), out object inertiaTensorRotation))
                obj.inertiaTensorRotation = (Quaternion)inertiaTensorRotation;
            if (input.TryGetValue(nameof(Rigidbody.sleepThreshold), out object sleepThreshold))
                obj.sleepThreshold = (float)sleepThreshold;
            if (input.TryGetValue(nameof(Rigidbody.solverIterations), out object solverIterations))
                obj.solverIterations = (int)solverIterations;
            if (input.TryGetValue(nameof(Rigidbody.solverVelocityIterations), out object solverVelocityIterations))
                obj.solverVelocityIterations = (int)solverVelocityIterations;
            if (input.TryGetValue(nameof(Rigidbody.IsSleeping), out object sleepState))
                if ((bool)sleepState)
                    obj.Sleep();
                else obj.WakeUp();

#if UNITY_6000_0_OR_NEWER
            if (input.TryGetValue(nameof(Rigidbody.linearDamping), out object damping))
                obj.linearDamping = (float)damping;
            if (input.TryGetValue(nameof(Rigidbody.linearVelocity), out object linearVelocity))
                obj.linearVelocity = (Vector3)linearVelocity;
            if (input.TryGetValue(nameof(Rigidbody.angularDamping), out object angularDamping))
                obj.angularDamping = (float)angularDamping;
#else
            if (input.TryGetValue(nameof(Rigidbody2D.drag), out object drag))
                obj.drag = (float)drag;
            if (input.TryGetValue(nameof(Rigidbody2D.velocity), out object velocity))
                obj.velocity = (Vector2)velocity;
            if (input.TryGetValue(nameof(Rigidbody2D.angularDrag), out object angularDrag))
                obj.angularDrag = (float)angularDrag;
#endif

            return obj;
        }
    }
}