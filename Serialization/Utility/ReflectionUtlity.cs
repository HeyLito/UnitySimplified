using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public static class ReflectionUtility
    {
        [NonSerialized]
        private static readonly HashSet<Type> TempCustomAttributes = new();
        [NonSerialized]
        private static readonly Stack<IEnumerable<FieldInfo>> TempStackedFieldInfos = new();
        [NonSerialized]
        private static readonly Dictionary<Type, IEnumerable<FieldInfo>> SerializedFields = new();

        public static IEnumerable<FieldInfo> GetSerializedFields(this Type type, params Type[] ignoreAttributeTypes)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (SerializedFields.TryGetValue(type, out IEnumerable<FieldInfo> entries))
                return entries;

            TempStackedFieldInfos.Clear();
            Type currentType = type;
            while (currentType != null && currentType != typeof(object))
            {
                if (!currentType.IsSerializable)
                {
                    Debug.Log(currentType);
                    currentType = currentType.BaseType;
                    continue;
                }
                
                List<FieldInfo> serializedFieldInfos = new();
                foreach (var fieldInfo in currentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
                {
                    if (fieldInfo.IsInitOnly || fieldInfo.IsNotSerialized)
                        continue;

                    bool exit = false;
                    TempCustomAttributes.Clear();
                    foreach (var attribute in fieldInfo.GetCustomAttributes())
                    {
                        Type attributeType = attribute.GetType();
                        if (attributeType == typeof(NonSerializedAttribute))
                        {
                            exit = true;
                            break;
                        }
                        TempCustomAttributes.Add(attribute.GetType());
                    }
                    if (exit)
                        continue;

                    if (ignoreAttributeTypes.Any(ignoreAttributeType => TempCustomAttributes.Contains(ignoreAttributeType)))
                        continue;
                    if (fieldInfo.IsPublic || (fieldInfo.IsPrivate && (TempCustomAttributes.Contains(typeof(SerializeField)) || TempCustomAttributes.Contains(typeof(SerializeReference)))))
                        serializedFieldInfos.Add(fieldInfo);
                }
                TempStackedFieldInfos.Push(serializedFieldInfos);
                currentType = currentType.BaseType;
            }

            List<FieldInfo> fieldInfos = new();
            while (TempStackedFieldInfos.TryPop(out var stackResult))
                fieldInfos.AddRange(stackResult);
            SerializedFields[type] = entries = fieldInfos;
            return entries;
        }
    }
}