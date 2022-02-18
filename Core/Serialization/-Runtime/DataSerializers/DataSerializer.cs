using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UnitySimplified.Serialization
{
    public static class DataSerializer
    {
        public static SurrogateSelector SurrogateSelector { get; set; } = new SurrogateSelector();

        /// <summary>
        /// Some serializer processes must yield until deep-level <em>(i.e. runtime references)</em> data conversions are complete. This function adds a post-serializer action to the list of processes that are invoked from <see cref="InvokePostSerializerActions"/>.
        /// </summary>
        /// <param name="action">
        /// </param>
        public static void AddPostSerializerAction(Action action)
        {   DataSerializerUtility.PostSerializerAction += action;   }

        /// <summary>
        /// Clears all post-serializer processes.
        /// <br>
        /// <em>
        /// Actions are automatically wiped after invoking. Switching from serialization to deserialization mode, and vice-versa will also clear actions.
        /// </em>
        /// </br>
        /// </summary>
        public static void ClearPostSerializerActions()
        {   DataSerializerUtility.InitializeReferencesAction = null; DataSerializerUtility.PostSerializerAction = null;   }

        /// <summary>
        /// Some serializer processes must yield until deep-level <em>(i.e. runtime references)</em> data conversions are complete. This function invokes those post-serializer processes.
        /// <br>
        /// <em>
        /// Actions are automatically wiped after invoking.
        /// </em>
        /// </br>
        /// </summary>
        public static void InvokePostSerializerActions()
        {   DataSerializerUtility.InitializeReferencesAction?.Invoke(); DataSerializerUtility.PostSerializerAction?.Invoke(); ClearPostSerializerActions();   }



        /// <summary>
        /// Uses the custom Serializer of instance's Type and transfers the data from instance into a Dictionary{string, object}.
        /// </summary>
        /// <param name="instance">
        /// </param>
        /// <param name="fieldData"></param>
        /// <param name="patterns"></param>
        public static void SerializeIntoData(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (DataSerializerUtility.SerializerInfo.inheritableTypes.Count == 0)
                DataSerializerUtility.PopulateSerializerInfo();

            if (DataSerializerUtility.ModeToggle != true)
            {
                DataSerializerUtility.ModeToggle = !DataSerializerUtility.ModeToggle;
                ClearPostSerializerActions();
            }

            if (DataSerializerUtility.SerializerInfo.uninheritableTypes.Count > 0 && DataSerializerUtility.SerializerInfo.uninheritableTypes.TryGetValue(instance.GetType(), out (Type, MethodInfo[]) uninheritable))
                HandleSerializerInvoke(false, instance, null, uninheritable.Item1, nameof(IConvertibleData.Serialize), uninheritable.Item2, fieldData, flags);
            else foreach (var inheritable in DataSerializerUtility.SerializerInfo.inheritableTypes)
                if (HandleSerializerInvoke(true, instance, inheritable.Item1, inheritable.Item2, nameof(IConvertibleData.Serialize), inheritable.Item3, fieldData, flags))
                    break;
        }

        public static void DeserializeIntoInstance(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (DataSerializerUtility.SerializerInfo.inheritableTypes.Count == 0) 
                DataSerializerUtility.PopulateSerializerInfo();

            if (DataSerializerUtility.ModeToggle != false)
            {
                DataSerializerUtility.ModeToggle = !DataSerializerUtility.ModeToggle;
                ClearPostSerializerActions();
            }

            if (DataSerializerUtility.SerializerInfo.uninheritableTypes.Count > 0 && DataSerializerUtility.SerializerInfo.uninheritableTypes.TryGetValue(instance.GetType(), out (Type, MethodInfo[]) uninheritable))
                HandleSerializerInvoke(false, instance, null, uninheritable.Item1, nameof(IConvertibleData.Deserialize), uninheritable.Item2, fieldData, flags);
            else foreach (var inheritable in DataSerializerUtility.SerializerInfo.inheritableTypes)
                if (HandleSerializerInvoke(true, instance, inheritable.Item1, inheritable.Item2, nameof(IConvertibleData.Deserialize), inheritable.Item3, fieldData, flags))
                    break;
        }

        private static bool HandleSerializerInvoke(bool isInheritable, object invoker, Type comparedTo, Type serializer, string methodName, MethodInfo[] methods, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (!isInheritable)
            {
                foreach (var method in methods)
                    if (method.Name.Equals(methodName))
                    {
                        object[] parameters = new[] { invoker, fieldData, flags };
                        method.Invoke(Activator.CreateInstance(serializer), parameters);
                        return true;
                    }
            }
            else if (!invoker.GetType().IsEquivalentTo(comparedTo) && invoker.GetType().IsSubclassOf(comparedTo))
                foreach (var method in methods)
                    if (method.Name.Equals(methodName))
                    {
                        object[] parameters = new[] { invoker, fieldData, flags };
                        method.Invoke(Activator.CreateInstance(serializer), parameters);
                        return true;
                    }
            return false;
        }

    }
}