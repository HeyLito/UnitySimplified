using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class AnimatorDataConverter : IDataConverter
    {
        int IDataConverter.ConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(Animator);
        void IDataConverter.Serialize(Type valueType, object value, IDictionary<string, object> output)
        {
            var obj = value as Animator;
            if (obj == null)
                return;

            output["currentStateHash"] = obj.GetCurrentAnimatorStateInfo(0).shortNameHash;
            output["currentStateNormalizedTime"] = obj.GetCurrentAnimatorStateInfo(0).normalizedTime;
            output["nextStateHash"] = obj.GetNextAnimatorStateInfo(0).shortNameHash;
            output["transitionDuration"] = obj.GetAnimatorTransitionInfo(0).duration;
            output["transitionNormalized"] = obj.GetAnimatorTransitionInfo(0).normalizedTime;

            DataConvertUtility.TrySerializeFieldAsset(nameof(Animator.runtimeAnimatorController), obj.runtimeAnimatorController, output);
        }
        object IDataConverter.Deserialize(Type valueType, object existingValue, IDictionary<string, object> input)
        {
            var obj = existingValue as Animator;
            if (obj == null)
                return null;

            if (input.TryGetValue("currentStateHash", out object currentStateHash) && input.TryGetValue("currentStateNormalizedTime", out object currentStateNormalizedTime))
                obj.Play((int)currentStateHash, 0, (float)currentStateNormalizedTime);
            obj.Update(0f);
            if (input.TryGetValue("nextStateHash", out object nextStateHash) && input.TryGetValue("transitionDuration", out object transitionDuration) && input.TryGetValue("transitionNormalized", out object transitionNormalized))
                obj.CrossFadeInFixedTime((int)nextStateHash, (float)transitionDuration, 0, 0f, (float)transitionNormalized);

            if (DataConvertUtility.TryDeserializeFieldAsset(nameof(Animator.runtimeAnimatorController), out object runtimeAnimatorController, input))
                obj.runtimeAnimatorController = runtimeAnimatorController as RuntimeAnimatorController;

            return existingValue;
        }
    }
}