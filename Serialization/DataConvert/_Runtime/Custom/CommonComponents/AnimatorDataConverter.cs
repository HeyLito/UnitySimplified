using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class AnimatorDataConverter : IDataConverter
    {
        int IDataConverter.GetConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(Animator);
        void IDataConverter.Serialize(object instance, IDictionary<string, object> output, SerializerFlags flags)
        {
            var obj = instance as Animator;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
            {
                output["currentStateHash"] = obj.GetCurrentAnimatorStateInfo(0).shortNameHash;
                output["currentStateNormalizedTime"] = obj.GetCurrentAnimatorStateInfo(0).normalizedTime;
                output["nextStateHash"] = obj.GetNextAnimatorStateInfo(0).shortNameHash;
                output["transitionDuration"] = obj.GetAnimatorTransitionInfo(0).duration;
                output["transitionNormalized"] = obj.GetAnimatorTransitionInfo(0).normalizedTime;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataConvertUtility.SerializeFieldAsset(nameof(Animator.runtimeAnimatorController), obj.runtimeAnimatorController, output);
            }
        }
        void IDataConverter.Deserialize(ref object instance, IDictionary<string, object> input, SerializerFlags flags)
        {
            var obj = instance as Animator;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
            {
                if (input.TryGetValue("currentStateHash", out object currentStateHash) && input.TryGetValue("currentStateNormalizedTime", out object currentStateNormalizedTime))
                    obj.Play((int)currentStateHash, 0, (float)currentStateNormalizedTime);
                obj.Update(0f);
                if (input.TryGetValue("nextStateHash", out object nextStateHash) && input.TryGetValue("transitionDuration", out object transitionDuration) && input.TryGetValue("transitionNormalized", out object transitionNormalized))
                    obj.CrossFadeInFixedTime((int)nextStateHash, (float)transitionDuration, 0, 0f, (float)transitionNormalized);

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataConvertUtility.DeserializeFieldAsset(nameof(Animator.runtimeAnimatorController), out object runtimeAnimatorController, typeof(RuntimeAnimatorController), input))
                        obj.runtimeAnimatorController = runtimeAnimatorController as RuntimeAnimatorController;
            }
        }
    }
}