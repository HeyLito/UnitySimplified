using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

namespace UnitySimplified.Serialization
{
    public class SerializationUtility : MonoBehaviour
    {
        private static SurrogateSelector _surrogateSelector;

        private static readonly Dictionary<Type, IEnumerable<FieldInfo>> FieldInfoEntriesByTypes = new();

        public static SurrogateSelector SurrogateSelector => _surrogateSelector ??= CreateSurrogateSelector();

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
            if (fromInstance == null)
                throw new ArgumentNullException(nameof(fromInstance));
            if (toInstance == null)
                throw new ArgumentNullException(nameof(toInstance));
            var type = typeof(T);

            if (!FieldInfoEntriesByTypes.TryGetValue(type, out IEnumerable<FieldInfo> fieldInfos))
            {
                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                FieldInfoEntriesByTypes[type] = fieldInfos = type.GetFields(flags);
            }

            foreach (var fieldInfo in fieldInfos)
            {
                object value = fieldInfo.GetValue(fromInstance);
                if (fieldInfo.Attributes.HasFlag(FieldAttributes.NotSerialized))
                    continue;
                fieldInfo.SetValue(toInstance, value);
            }
        }

        private static SurrogateSelector CreateSurrogateSelector()
        {
            var selector = new SurrogateSelector();
            selector.AddSurrogate(typeof(Vector2), default, new Vector2Surrogate());
            selector.AddSurrogate(typeof(Vector3), default, new Vector3Surrogate());
            selector.AddSurrogate(typeof(Vector4), default, new Vector4Surrogate());
            selector.AddSurrogate(typeof(Quaternion), default, new QuaternionSurrogate());
            selector.AddSurrogate(typeof(Color), default, new ColorSurrogate());
            return selector;
        }
    }
}