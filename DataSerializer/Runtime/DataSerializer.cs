using System;
using System.Collections.Generic;

namespace UnitySimplified.Serialization
{
    public static class DataSerializer
    {
        /// <summary>
        /// Some serializer processes must yield until deep-level <em>(i.e. runtime references)</em> data conversions are complete. This function adds a post-serializer action to the list of processes that are invoked from <see cref="InvokePostSerializerActions"/>.
        /// </summary>
        /// <param name="action">
        /// </param>
        public static void AddPostSerializerAction(Action action) => DataSerializerUtility.ActionOnPostSerializer += action;

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
            DataSerializerUtility.ActionOnInitializeReferences = null;
            DataSerializerUtility.ActionOnPostSerializer = null;
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
            DataSerializerUtility.ActionOnInitializeReferences?.Invoke();
            DataSerializerUtility.ActionOnPostSerializer?.Invoke();
            ClearPostSerializerActions();
        }



        /// <summary>
        /// Uses the custom Serializer of instance's Type and transfers the data from instance into a Dictionary{string, object}.
        /// </summary>
        /// <param name="instance">
        /// </param>
        /// <param name="fieldData"></param>
        /// <param name="flags"></param>

        public static void SerializeIntoData(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (DataSerializerUtility.SerializerMode != DataSerializerUtility.SerializerModeType.Serializing)
            {
                DataSerializerUtility.SerializerMode = DataSerializerUtility.SerializerModeType.Serializing;
                ClearPostSerializerActions();
            }

            if (DataSerializerUtility.FindCustomSerializer(instance.GetType(), out var serializer))
                serializer.Serialize(instance, fieldData, flags);
            else DataSerializerUtility.Serialize(instance, fieldData, flags);
        }
        public static void DeserializeIntoInstance(object instance, Dictionary<string, object> fieldData, SerializerFlags flags) => DeserializeIntoInstance(ref instance, fieldData, flags);
        public static void DeserializeIntoInstance(ref object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (DataSerializerUtility.SerializerMode != DataSerializerUtility.SerializerModeType.Deserializing)
            {
                DataSerializerUtility.SerializerMode = DataSerializerUtility.SerializerModeType.Deserializing;
                ClearPostSerializerActions();
            }

            if (DataSerializerUtility.FindCustomSerializer(instance.GetType(), out var serializer))
                serializer.Deserialize(ref instance, fieldData, flags);
            else DataSerializerUtility.Deserialize(instance, fieldData, flags);
        }
    }
}