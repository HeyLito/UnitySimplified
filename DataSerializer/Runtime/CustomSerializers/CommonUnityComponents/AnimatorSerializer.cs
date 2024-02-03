using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(Animator), -1)]
    public sealed class AnimatorSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as Animator;

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
            {
                fieldData["currentStateHash"] = obj.GetCurrentAnimatorStateInfo(0).shortNameHash;
                fieldData["currentStateNormalizedTime"] = obj.GetCurrentAnimatorStateInfo(0).normalizedTime;
                fieldData["nextStateHash"] = obj.GetNextAnimatorStateInfo(0).shortNameHash;
                fieldData["transitionDuration"] = obj.GetAnimatorTransitionInfo(0).duration;
                fieldData["transitionNormalized"] = obj.GetAnimatorTransitionInfo(0).normalizedTime;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataSerializerUtility.SerializeFieldAsset(nameof(Animator.runtimeAnimatorController), obj.runtimeAnimatorController, fieldData);
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as Animator;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
            {
                if (fieldData.TryGetValue("currentStateHash", out object currentStateHash) && fieldData.TryGetValue("currentStateNormalizedTime", out object currentStateNormalizedTime))
                    obj.Play((int)currentStateHash, 0, (float)currentStateNormalizedTime);
                obj.Update(0f);
                if (fieldData.TryGetValue("nextStateHash", out object nextStateHash) && fieldData.TryGetValue("transitionDuration", out object transitionDuration) && fieldData.TryGetValue("transitionNormalized", out object transitionNormalized))
                    obj.CrossFadeInFixedTime((int)nextStateHash, (float)transitionDuration, 0, 0f, (float)transitionNormalized);

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataSerializerUtility.DeserializeFieldAsset(nameof(Animator.runtimeAnimatorController), out object runtimeAnimatorController, typeof(RuntimeAnimatorController), fieldData))
                        obj.runtimeAnimatorController = runtimeAnimatorController as RuntimeAnimatorController;
            }
        }
    }
}