using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityObject = UnityEngine.Object;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(MonoBehaviour), true, -5)]
    public sealed class MonobehaviourSerializer : IConvertibleData
    {
        public void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {   DataSerializerUtility.Serialize(instance, fieldData, flags);   }
        public void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {   DataSerializerUtility.Deserialize(instance, fieldData, flags);   }
    }

    [CustomSerializer(typeof(GameObject), -1)]
    public sealed class GameObjectSerializer : IConvertibleData
    {
        public void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as GameObject;

            if (flags.HasFlag(SerializerFlags.SerializedVariables))
            {
                fieldData[nameof(GameObject.name)] = obj.name;
                fieldData[nameof(GameObject.tag)] = obj.tag;
                fieldData[nameof(GameObject.layer)] = obj.layer;
                fieldData[nameof(GameObject.isStatic)] = obj.isStatic;
                fieldData[nameof(GameObject.activeSelf)] = obj.activeSelf;
            }
        }
        public void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as GameObject;

            if (flags.HasFlag(SerializerFlags.SerializedVariables))
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

    [CustomSerializer(typeof(Transform), -1)]
    public sealed class TransformSerializer : IConvertibleData
    {
        public void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as Transform;

            if (flags.HasFlag(SerializerFlags.SerializedVariables))
            {
                fieldData[nameof(Transform.position)] = obj.position;
                fieldData[nameof(Transform.rotation)] = obj.rotation;
                fieldData[nameof(Transform.localScale)] = obj.localScale;
            }
        }
        public void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as Transform;

            if (flags.HasFlag(SerializerFlags.SerializedVariables))
            {
                if (fieldData.TryGetValue(nameof(Transform.position), out object position))
                    obj.position = (Vector3)position;
                if (fieldData.TryGetValue(nameof(Transform.rotation), out object rotation))
                    obj.rotation = (Quaternion)rotation;
                if (fieldData.TryGetValue(nameof(Transform.localScale), out object scale))
                    obj.localScale = (Vector3)scale;
            }
        }
    }

    [CustomSerializer(typeof(MeshFilter), -1)]
    public sealed class MeshFilterSerializer : IConvertibleData
    {
        public void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as MeshFilter;

            if (flags.HasFlag(SerializerFlags.AssetReferences))
                DataSerializerUtility.SerializeFieldAsset(nameof(MeshFilter.sharedMesh), obj.sharedMesh, fieldData);
        }
        public void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as MeshFilter;

            if (flags.HasFlag(SerializerFlags.AssetReferences))
            {
                object sharedMesh = typeof(Mesh);
                if (DataSerializerUtility.DeserializeFieldAsset(nameof(MeshFilter.sharedMesh), ref sharedMesh, fieldData))
                    obj.mesh = sharedMesh as Mesh;
            }
        }
    }

    [CustomSerializer(typeof(MeshRenderer), -1)]
    public sealed class MeshRendererSerializer : IConvertibleData
    {
        public void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as MeshRenderer;

            if (flags.HasFlag(SerializerFlags.AssetReferences))
                for (int i = 0; i < obj.sharedMaterials.Length; i++)
                    DataSerializerUtility.SerializeFieldAsset(nameof(MeshRenderer.sharedMaterials) + i, obj.sharedMaterials[i], fieldData);
        }
        public void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as MeshRenderer;

            if (flags.HasFlag(SerializerFlags.AssetReferences))
            {
                List<Material> materials = new List<Material>();
                int i = -1;
                foreach (var pair in fieldData)
                {
                    i++;
                    object sharedMaterial = typeof(Material);
                    if (DataSerializerUtility.DeserializeFieldAsset(nameof(MeshRenderer.sharedMaterials) + i, ref sharedMaterial, fieldData))
                        materials.Add(sharedMaterial as Material);
                    else continue;
                }
                obj.materials = materials.ToArray();
            }
        }
    }

    [CustomSerializer(typeof(Animator), -1)]
    public sealed class AnimatorSerializer : IConvertibleData
    {
        public void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as Animator;

            if (flags.HasFlag(SerializerFlags.AssetReferences))
                DataSerializerUtility.SerializeFieldAsset(nameof(Animator.runtimeAnimatorController), obj.runtimeAnimatorController, fieldData);

            if (flags.HasFlag(SerializerFlags.SoftRuntimeVariables) || flags.HasFlag(SerializerFlags.DeepRuntimeVariables))
            {
                fieldData["currentStateHash"] = obj.GetCurrentAnimatorStateInfo(0).shortNameHash;
                fieldData["currentStateNormalizedTime"] = obj.GetCurrentAnimatorStateInfo(0).normalizedTime;
                fieldData["nextStateHash"] = obj.GetNextAnimatorStateInfo(0).shortNameHash;
                fieldData["transitionDuration"] = obj.GetAnimatorTransitionInfo(0).duration;
                fieldData["transitionNormalized"] = obj.GetAnimatorTransitionInfo(0).normalizedTime;
            }
        }
        public void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as Animator;

            if (flags.HasFlag(SerializerFlags.AssetReferences))
            {
                object runtimeAnimatorController = typeof(RuntimeAnimatorController);
                if (DataSerializerUtility.DeserializeFieldAsset(nameof(Animator.runtimeAnimatorController), ref runtimeAnimatorController, fieldData))
                    obj.runtimeAnimatorController = runtimeAnimatorController as RuntimeAnimatorController;
            }

            if (flags.HasFlag(SerializerFlags.SoftRuntimeVariables) || flags.HasFlag(SerializerFlags.DeepRuntimeVariables))
            {
                if (fieldData.TryGetValue("currentStateHash", out object currentStateHash) && fieldData.TryGetValue("currentStateNormalizedTime", out object currentStateNormalizedTime))
                    obj.Play((int)currentStateHash, 0, (float)currentStateNormalizedTime);
                obj.Update(0f);
                if (fieldData.TryGetValue("nextStateHash", out object nextStateHash) && fieldData.TryGetValue("transitionDuration", out object transitionDuration) && fieldData.TryGetValue("transitionNormalized", out object transitionNormalized))
                    obj.CrossFadeInFixedTime((int)nextStateHash, (float)transitionDuration, 0, 0f, (float)transitionNormalized);
            }
        }
    }

    [CustomSerializer(typeof(VideoPlayer), -1)]
    public sealed class VideoPlayerSerializer : IConvertibleData
    {
        public void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as VideoPlayer;

            if (flags.HasFlag(SerializerFlags.AssetReferences))
                DataSerializerUtility.SerializeFieldAsset("clip", obj.clip, fieldData);

            if (flags.HasFlag(SerializerFlags.RuntimeReferences))
                DataSerializerUtility.AddInitializeReferenceAction(() => DataSerializerUtility.SerializeFieldReference("camera", obj.targetCamera, fieldData, typeof(UnityObject)));

            fieldData["isPlaying"] = obj.isPlaying;
            fieldData["time"] = obj.time;
        }
        public void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as VideoPlayer;

            if (flags.HasFlag(SerializerFlags.AssetReferences))
            {
                object asset = typeof(VideoClip);
                if (DataSerializerUtility.DeserializeFieldAsset("clip", ref asset, fieldData))
                    obj.clip = asset as VideoClip;
            }

            if (flags.HasFlag(SerializerFlags.RuntimeReferences))
            {
                object reference = null;
                DataSerializerUtility.AddInitializeReferenceAction(() => DataSerializerUtility.DeserializeFieldReference("camera", out object reference, fieldData, typeof(UnityObject)));
                DataSerializer.AddPostSerializerAction(() => obj.targetCamera = (Camera)reference);
            }

            if (fieldData.TryGetValue("isPlaying", out object isPlaying) && (bool)isPlaying)
            {
                obj.Play();
                if (fieldData.TryGetValue("time", out object time))
                    obj.time = (double)time;
            }
        }
    }
}