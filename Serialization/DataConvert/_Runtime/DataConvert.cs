using System;
using System.Collections.Generic;

namespace UnitySimplified.Serialization
{
    public static class DataConvert
    {
        /// <summary>
        /// Some serializer processes must yield until deep-level <em>(i.e. runtime references)</em> data conversions are complete. This function adds a post-serializer action to the list of processes that are invoked from <see cref="InvokePostSerializerActions"/>.
        /// </summary>
        /// <param name="action">
        /// </param>
        public static void AddPostSerializerAction(Action action) => DataConvertUtility.ActionOnPostSerializer += action;

        /// <summary>
        /// Clears all post-serializer processes.
        /// <br>
        /// <em>
        /// Actions are automatically wiped after invoking. Switching from serialization to deserialization mode, and vice-versa, will also clear actions.
        /// </em>
        /// </br>
        /// </summary>
        public static void ClearPostSerializerActions()
        {
            DataConvertUtility.ActionOnInitializeReferences = null;
            DataConvertUtility.ActionOnPostSerializer = null;
        }

        /// <summary>
        /// Some serializer processes must yield until deep-level <em>(i.e. runtime references)</em> data conversions are complete. This function invokes those post-serializer processes.
        /// <br>
        /// <em>
        /// Actions are automatically wiped after invoking.
        /// </em>
        /// </br>
        /// </summary>
        public static void InvokePostSerializerActions()
        {
            DataConvertUtility.ActionOnInitializeReferences?.Invoke();
            DataConvertUtility.ActionOnPostSerializer?.Invoke();
            ClearPostSerializerActions();
        }



        /// <summary>
        /// Uses the custom Serializer of instance's Type and transfers the data from instance into a Dictionary{string, object}.
        /// </summary>
        /// <param name="instance">
        /// </param>
        /// <param name="output"></param>
        /// <param name="flags"></param>
        public static void Serialize(object instance, IDictionary<string, object> output, SerializerFlags flags)
        {
            if (DataConvertUtility.SerializerMode != DataConvertUtility.SerializerModeType.Serializing)
            {
                DataConvertUtility.SerializerMode = DataConvertUtility.SerializerModeType.Serializing;
                ClearPostSerializerActions();
            }

            if (DataConvertUtility.FindCustomConverter(instance.GetType(), out var converter))
                converter.Serialize(instance, output, flags);
            else DataConvertUtility.Serialize(instance, output, flags);
        }
        public static void Deserialize(object instance, IDictionary<string, object> input, SerializerFlags flags) => Deserialize(ref instance, input, flags);
        public static void Deserialize(ref object instance, IDictionary<string, object> input, SerializerFlags flags)
        {
            if (DataConvertUtility.SerializerMode != DataConvertUtility.SerializerModeType.Deserializing)
            {
                DataConvertUtility.SerializerMode = DataConvertUtility.SerializerModeType.Deserializing;
                ClearPostSerializerActions();
            }

            if (DataConvertUtility.FindCustomConverter(instance.GetType(), out var converter))
                converter.Deserialize(ref instance, input, flags);
            else DataConvertUtility.Deserialize(instance, input, flags);
        }
    }
}