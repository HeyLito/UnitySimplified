using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnitySimplified.Serialization 
{
    public class SerializerStorage : Storage<SerializerStorage>
    {
        [Serializable]
        private class StringByStringDictionary : SerializableDictionary<string, string> { }

        [SerializeField] private List<string> serializerTypes = new List<string>();

        public void Clear()
        {   serializerTypes.Clear();   }

        public void Store(Type type)
        {   serializerTypes.Add(type.AssemblyQualifiedName);   }

        public Type[] Retrieve()
        {
            var toArray = new Type[serializerTypes.Count];
            for (int i = 0; i < toArray.Length; i++)
                toArray[i] = Type.GetType(serializerTypes[i]);
            return toArray;
        }

        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void LoadCustomSerializers()
        {
            Instance.Clear();
            foreach (var type in typeof(IConvertibleData).Assembly.GetTypes())
            {
                if (!typeof(IConvertibleData).IsAssignableFrom(type) || type == typeof(IConvertibleData))
                    continue;

                CustomSerializer attribute = (CustomSerializer)Attribute.GetCustomAttribute(type, typeof(CustomSerializer));
                if (attribute != null)
                    Instance.Store(type);
            }
        }
        #endif
    }
}