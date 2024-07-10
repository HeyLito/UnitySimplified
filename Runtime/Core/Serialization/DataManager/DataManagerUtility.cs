using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UnitySimplified.Serialization
{
    public static class DataManagerUtility
    {
        private static readonly Dictionary<Type, IEnumerable<FieldInfo>> FieldInfoEntriesByTypes = new();
        private static SurrogateSelector _surrogateSelector;

        public static SurrogateSelector SurrogateSelector  => _surrogateSelector ??= new SurrogateSelector();



        public static bool IsSerializable(Type type)
        {
            if (SurrogateSelector != null && SurrogateSelector.GetSurrogate(type, new StreamingContext(StreamingContextStates.All), out _) != null)
                return true;
            if (type.IsSerializable)
                return true;
            return false;
        }
        public static void OverwriteInstanceFromOther<T>(T fromInstance, T toInstance)
        {
            if (!FieldInfoEntriesByTypes.TryGetValue(typeof(T), out IEnumerable<FieldInfo> fieldInfos))
            {
                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetField;
                FieldInfoEntriesByTypes[typeof(T)] = fieldInfos = typeof(T).GetFields(flags);
            }

            foreach (var fieldInfo in fieldInfos)
            {
                object value = fieldInfo.GetValue(fromInstance);
                if (fieldInfo.Attributes.HasFlag(FieldAttributes.NotSerialized))
                    continue;
                fieldInfo.SetValue(toInstance, value);
            }
        }
    }
}