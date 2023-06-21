using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(Rigidbody2D), -1)]
    public sealed class Rigidbody2DSerializer : IDataSerializable
    {
        public void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
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
        public void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as Rigidbody2D;

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


    [CustomSerializer(typeof(CircleCollider2D), -1)]
    public sealed class CircleCollider2DSerializer : IDataSerializable
    {
        public void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as CircleCollider2D;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                fieldData[nameof(CircleCollider2D.density)] = obj.density;
                fieldData[nameof(CircleCollider2D.isTrigger)] = obj.isTrigger;
                fieldData[nameof(CircleCollider2D.usedByEffector)] = obj.usedByEffector;
                fieldData[nameof(CircleCollider2D.usedByComposite)] = obj.usedByComposite;
                fieldData[nameof(CircleCollider2D.offset)] = obj.offset;
                fieldData[nameof(CircleCollider2D.radius)] = obj.radius;
                
                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataSerializerUtility.SerializeFieldAsset(nameof(CircleCollider2D.sharedMaterial), obj.sharedMaterial, fieldData);
            }
        }
        public void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as CircleCollider2D;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(CircleCollider2D.density), out object density))
                    obj.density = (float)density;
                if (fieldData.TryGetValue(nameof(CircleCollider2D.isTrigger), out object isTrigger))
                    obj.isTrigger = (bool)isTrigger;
                if (fieldData.TryGetValue(nameof(CircleCollider2D.usedByEffector), out object usedByEffector))
                    obj.usedByEffector = (bool)usedByEffector;
                if (fieldData.TryGetValue(nameof(CircleCollider2D.usedByComposite), out object usedByComposite))
                    obj.usedByComposite = (bool)usedByComposite;
                if (fieldData.TryGetValue(nameof(CircleCollider2D.offset), out object offset))
                    obj.offset = (Vector2)offset;
                if (fieldData.TryGetValue(nameof(CircleCollider2D.radius), out object radius))
                    obj.radius = (float)radius;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataSerializerUtility.DeserializeFieldAsset(nameof(CircleCollider2D.sharedMaterial), out object sharedMaterial, typeof(PhysicsMaterial2D), fieldData))
                        obj.sharedMaterial = (PhysicsMaterial2D)sharedMaterial;
            }
        }
    }


    [CustomSerializer(typeof(BoxCollider2D), -1)]
    public sealed class BoxCollider2DSerializer : IDataSerializable
    {
        public void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as BoxCollider2D;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                fieldData[nameof(BoxCollider2D.density)] = obj.density;
                fieldData[nameof(BoxCollider2D.isTrigger)] = obj.isTrigger;
                fieldData[nameof(BoxCollider2D.usedByEffector)] = obj.usedByEffector;
                fieldData[nameof(BoxCollider2D.usedByComposite)] = obj.usedByComposite;
                fieldData[nameof(BoxCollider2D.autoTiling)] = obj.autoTiling;
                fieldData[nameof(BoxCollider2D.offset)] = obj.offset;
                fieldData[nameof(BoxCollider2D.size)] = obj.size;
                fieldData[nameof(BoxCollider2D.edgeRadius)] = obj.edgeRadius;
                
                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataSerializerUtility.SerializeFieldAsset(nameof(BoxCollider2D.sharedMaterial), obj.sharedMaterial, fieldData);
            }
        }
        public void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as BoxCollider2D;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(BoxCollider2D.density), out object density))
                    obj.density = (float)density;
                if (fieldData.TryGetValue(nameof(BoxCollider2D.isTrigger), out object isTrigger))
                    obj.isTrigger = (bool)isTrigger;
                if (fieldData.TryGetValue(nameof(BoxCollider2D.usedByEffector), out object usedByEffector))
                    obj.usedByEffector = (bool)usedByEffector;
                if (fieldData.TryGetValue(nameof(BoxCollider2D.usedByComposite), out object usedByComposite))
                    obj.usedByComposite = (bool)usedByComposite;
                if (fieldData.TryGetValue(nameof(BoxCollider2D.autoTiling), out object autoTiling))
                    obj.autoTiling = (bool)autoTiling;
                if (fieldData.TryGetValue(nameof(BoxCollider2D.offset), out object offset))
                    obj.offset = (Vector2)offset;
                if (fieldData.TryGetValue(nameof(BoxCollider2D.size), out object size))
                    obj.size = (Vector2)size;
                if (fieldData.TryGetValue(nameof(BoxCollider2D.edgeRadius), out object edgeRadius))
                    obj.edgeRadius = (float)edgeRadius;
                
                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataSerializerUtility.DeserializeFieldAsset(nameof(BoxCollider2D.sharedMaterial), out object sharedMaterial, typeof(PhysicsMaterial2D), fieldData))
                        obj.sharedMaterial = (PhysicsMaterial2D)sharedMaterial;
            }
        }
    }


    [CustomSerializer(typeof(EdgeCollider2D), -1)]
    public sealed class EdgeCollider2DSerializer : IDataSerializable
    {
        public void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as EdgeCollider2D;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                fieldData[nameof(EdgeCollider2D.density)] = obj.density;
                fieldData[nameof(EdgeCollider2D.isTrigger)] = obj.isTrigger;
                fieldData[nameof(EdgeCollider2D.usedByEffector)] = obj.usedByEffector;
                fieldData[nameof(EdgeCollider2D.usedByComposite)] = obj.usedByComposite;
                fieldData[nameof(EdgeCollider2D.offset)] = obj.offset;
                fieldData[nameof(EdgeCollider2D.edgeRadius)] = obj.edgeRadius;
                fieldData[nameof(EdgeCollider2D.points)] = obj.points;
                
                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataSerializerUtility.SerializeFieldAsset(nameof(EdgeCollider2D.sharedMaterial), obj.sharedMaterial, fieldData);
            }
        }
        public void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as EdgeCollider2D;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(EdgeCollider2D.density), out object density))
                    obj.density = (float)density;
                if (fieldData.TryGetValue(nameof(EdgeCollider2D.isTrigger), out object isTrigger))
                    obj.isTrigger = (bool)isTrigger;
                if (fieldData.TryGetValue(nameof(EdgeCollider2D.usedByEffector), out object usedByEffector))
                    obj.usedByEffector = (bool)usedByEffector;
                if (fieldData.TryGetValue(nameof(EdgeCollider2D.usedByComposite), out object usedByComposite))
                    obj.usedByComposite = (bool)usedByComposite;
                if (fieldData.TryGetValue(nameof(EdgeCollider2D.offset), out object offset))
                    obj.offset = (Vector2)offset;
                if (fieldData.TryGetValue(nameof(EdgeCollider2D.edgeRadius), out object edgeRadius))
                    obj.edgeRadius = (float)edgeRadius;
                if (fieldData.TryGetValue(nameof(EdgeCollider2D.points), out object points))
                    obj.points = (Vector2[])points;
                
                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataSerializerUtility.DeserializeFieldAsset(nameof(EdgeCollider2D.sharedMaterial), out object sharedMaterial, typeof(PhysicsMaterial2D), fieldData))
                        obj.sharedMaterial = (PhysicsMaterial2D)sharedMaterial;
            }
        }
    }
}