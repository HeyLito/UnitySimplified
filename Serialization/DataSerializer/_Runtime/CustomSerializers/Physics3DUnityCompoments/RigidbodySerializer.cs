using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(Rigidbody), -1)]
    public sealed class RigidbodySerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object target, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = target as Rigidbody;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                fieldData[nameof(Rigidbody.mass)] = obj.mass;
                fieldData[nameof(Rigidbody.drag)] = obj.drag;
                fieldData[nameof(Rigidbody.angularDrag)] = obj.angularDrag;
                fieldData[nameof(Rigidbody.useGravity)] = obj.useGravity;
                fieldData[nameof(Rigidbody.isKinematic)] = obj.isKinematic;
                fieldData[nameof(Rigidbody.interpolation)] = (int)obj.interpolation;
                fieldData[nameof(Rigidbody.collisionDetectionMode)] = (int)obj.collisionDetectionMode;
                fieldData[nameof(Rigidbody.constraints)] = (int)obj.constraints;
            }
            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
            {
                fieldData[nameof(Rigidbody.position)] = obj.position;
                fieldData[nameof(Rigidbody.rotation)] = obj.rotation;
                fieldData[nameof(Rigidbody.centerOfMass)] = obj.centerOfMass;
                fieldData[nameof(Rigidbody.velocity)] = obj.velocity;
                fieldData[nameof(Rigidbody.angularVelocity)] = obj.angularVelocity;
                fieldData[nameof(Rigidbody.maxAngularVelocity)] = obj.maxAngularVelocity;
                fieldData[nameof(Rigidbody.maxDepenetrationVelocity)] = obj.maxDepenetrationVelocity;
                fieldData[nameof(Rigidbody.detectCollisions)] = obj.detectCollisions;
                fieldData[nameof(Rigidbody.freezeRotation)] = obj.freezeRotation;
                fieldData[nameof(Rigidbody.inertiaTensor)] = obj.inertiaTensor;
                fieldData[nameof(Rigidbody.inertiaTensorRotation)] = obj.inertiaTensorRotation;
                fieldData[nameof(Rigidbody.sleepThreshold)] = obj.sleepThreshold;
                fieldData[nameof(Rigidbody.solverIterations)] = obj.solverIterations;
                fieldData[nameof(Rigidbody.solverVelocityIterations)] = obj.solverVelocityIterations;
                fieldData[nameof(Rigidbody.IsSleeping)] = obj.IsSleeping();
            }
        }
        void IDataSerializable.Deserialize(ref object target, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = target as Rigidbody;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(Rigidbody.mass), out object mass))
                    obj.mass = (float)mass;
                if (fieldData.TryGetValue(nameof(Rigidbody.drag), out object drag))
                    obj.drag = (float)drag;
                if (fieldData.TryGetValue(nameof(Rigidbody.angularDrag), out object angularDrag))
                    obj.angularDrag = (float)angularDrag;
                if (fieldData.TryGetValue(nameof(Rigidbody.useGravity), out object useGravity))
                    obj.useGravity = (bool)useGravity;
                if (fieldData.TryGetValue(nameof(Rigidbody.isKinematic), out object isKinematic))
                    obj.isKinematic = (bool)isKinematic;
                if (fieldData.TryGetValue(nameof(Rigidbody.interpolation), out object interpolation))
                    obj.interpolation = (RigidbodyInterpolation)interpolation;
                if (fieldData.TryGetValue(nameof(Rigidbody.collisionDetectionMode), out object collisionDetectionMode))
                    obj.collisionDetectionMode = (CollisionDetectionMode)collisionDetectionMode;
                if (fieldData.TryGetValue(nameof(Rigidbody.constraints), out object constraints))
                    obj.constraints = (RigidbodyConstraints)constraints;
            }
            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(Rigidbody.position), out object position))
                    obj.position = (Vector3)position;
                if (fieldData.TryGetValue(nameof(Rigidbody.rotation), out object rotation))
                    obj.rotation = (Quaternion)rotation;
                if (fieldData.TryGetValue(nameof(Rigidbody.centerOfMass), out object centerOfMass))
                    obj.centerOfMass = (Vector3)centerOfMass;
                if (fieldData.TryGetValue(nameof(Rigidbody.velocity), out object velocity))
                    obj.velocity = (Vector3)velocity;
                if (fieldData.TryGetValue(nameof(Rigidbody.angularVelocity), out object angularVelocity))
                    obj.angularVelocity = (Vector3)angularVelocity;
                if (fieldData.TryGetValue(nameof(Rigidbody.maxAngularVelocity), out object maxAngularVelocity))
                    obj.maxAngularVelocity = (float)maxAngularVelocity;
                if (fieldData.TryGetValue(nameof(Rigidbody.maxDepenetrationVelocity), out object maxDepenetrationVelocity))
                    obj.maxDepenetrationVelocity = (float)maxDepenetrationVelocity;
                if (fieldData.TryGetValue(nameof(Rigidbody.detectCollisions), out object detectCollisions))
                    obj.detectCollisions = (bool)detectCollisions;
                if (fieldData.TryGetValue(nameof(Rigidbody.freezeRotation), out object freezeRotation))
                    obj.freezeRotation = (bool)freezeRotation;
                if (fieldData.TryGetValue(nameof(Rigidbody.inertiaTensor), out object inertiaTensor))
                    obj.inertiaTensor = (Vector3)inertiaTensor;
                if (fieldData.TryGetValue(nameof(Rigidbody.inertiaTensorRotation), out object inertiaTensorRotation))
                    obj.inertiaTensorRotation = (Quaternion)inertiaTensorRotation;
                if (fieldData.TryGetValue(nameof(Rigidbody.sleepThreshold), out object sleepThreshold))
                    obj.sleepThreshold = (float)sleepThreshold;
                if (fieldData.TryGetValue(nameof(Rigidbody.solverIterations), out object solverIterations))
                    obj.solverIterations = (int)solverIterations;
                if (fieldData.TryGetValue(nameof(Rigidbody.solverVelocityIterations), out object solverVelocityIterations))
                    obj.solverVelocityIterations = (int)solverVelocityIterations;
                if (fieldData.TryGetValue(nameof(Rigidbody.IsSleeping), out object sleepState))
                    if ((bool)sleepState)
                        obj.Sleep();
                    else obj.WakeUp();
            }
        }
    }
}