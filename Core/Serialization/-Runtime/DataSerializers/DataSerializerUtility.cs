using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using System.Linq;

namespace UnitySimplified.Serialization
{
    public static class DataSerializerUtility
    {
        internal struct CustomSerializerInfo
        {
            internal readonly Dictionary<Type, ValueTuple<Type, MethodInfo[]>> uninheritableTypes;
            internal readonly List<ValueTuple<Type, Type, MethodInfo[]>> inheritableTypes;

            internal CustomSerializerInfo(Dictionary<Type, ValueTuple<Type, MethodInfo[]>> uninheritableTypes, List<ValueTuple<Type, Type, MethodInfo[]>> inheritableTypes)
            {
                this.uninheritableTypes = uninheritableTypes;
                this.inheritableTypes = inheritableTypes;
            }

            internal void Clear() 
            {
                uninheritableTypes.Clear();
                inheritableTypes.Clear();
            }
        }



        #region FIELDS
        private static readonly string[] _incompatibleAssemblies = new[] { "UnityEngine" };
        private static bool _modeToggle = true;
        private static Action _initializeReferencesAction;
        private static Action _deepSerializerAction;
        private static readonly Dictionary<Type, MethodInfo[]> _methodInfoByTypes = new Dictionary<Type, MethodInfo[]>();
        private static readonly CustomSerializerInfo _serializerInfo = new CustomSerializerInfo(new Dictionary<Type, ValueTuple<Type, MethodInfo[]>>(), new List<(Type, Type, MethodInfo[])>());
        #endregion



        #region PROPERTIES
        internal static bool ModeToggle { get => _modeToggle; set => _modeToggle = value; }
        internal static Action InitializeReferencesAction { get => _initializeReferencesAction; set => _initializeReferencesAction = value; }
        internal static Action PostSerializerAction { get => _deepSerializerAction; set => _deepSerializerAction = value; }
        internal static Dictionary<Type, MethodInfo[]> MethodInfoByTypes { get => _methodInfoByTypes; }
        internal static CustomSerializerInfo SerializerInfo => _serializerInfo;
        #endregion



        #region FUNCTIONS_INTERNAL
        public static void AddInitializeReferenceAction(Action action)
        {   _initializeReferencesAction += action;   }

        internal static void PopulateSerializerInfo()
        {
            SerializerInfo.Clear();

            var queue = new PriorityQueue<int, ValueTuple<Type, CustomSerializer>>();
            foreach (var type in SerializerStorage.Instance.Retrieve()) 
            {
                var attribute = (CustomSerializer)Attribute.GetCustomAttribute(type, typeof(CustomSerializer));
                queue.Insert(attribute.priority, (type, attribute));
                //Debug.Log($"Enqueue: {attribute.inspectedType}, {type}, {attribute.priority}");
            }

            int count = queue.Count();
            for (int i = 0; i < count; i++)
            {
                var tuple = queue.Pop();
                if (tuple.Item2.allowsInheritance)
                    SerializerInfo.inheritableTypes.Add((tuple.Item2.inspectedType, tuple.Item1, tuple.Item1.GetInterface(typeof(IConvertibleData).Name).GetMethods()));
                else
                    SerializerInfo.uninheritableTypes.Add(tuple.Item2.inspectedType, (tuple.Item1, tuple.Item1.GetInterface(typeof(IConvertibleData).Name).GetMethods()));
                //Debug.Log($"Dequeue: {tuple.Item2.inspectedType}, {tuple.Item1}, {tuple.Item2.priority}");
            }
        }
        #endregion



        #region FUNCTIONS_UNITYOBJECT_SERIALIZER
        public static bool SerializeFieldAsset(string fieldName, object asset, Dictionary<string, object> fieldData)
        {
            if (asset as UnityObject)
            {
                if (asset is GameObject && PrefabStorage.Instance.TryGetPrefabKey(asset as GameObject, out string key))
                {
                    fieldData[fieldName] = key;
                    return true;
                }
                if (AssetStorage.Instance.SupportsType(asset.GetType()))
                {
                    AssetStorage.Instance.StoreAsset(asset as UnityObject, out key);
                    fieldData[fieldName] = key;
                    return true;
                }
            }
            return false;
        }
        public static bool DeserializeFieldAsset(string fieldName, ref object asset, Dictionary<string, object> fieldData)
        {
            if (fieldData.TryGetValue(fieldName, out object fieldValue) && fieldValue is string)
            {
                if (asset is GameObject)
                {
                    asset = PrefabStorage.Instance.RetrieveGameObject(fieldValue as string);
                    return true;
                }
                if (AssetStorage.Instance.SupportsType(asset as Type))
                {
                    asset = AssetStorage.Instance.RetrieveAsset(fieldValue as string);
                    return true;
                }
            }
            return false;
        }
        #endregion



        #region REFERENCE_SERIALIZER
        public static bool SerializeFieldReference(string fieldName, object referenceObject, Dictionary<string, object> fieldData, Type typeIndexer)
        {
            if (ReferenceStorage.Instance.TryGetIdentifier(referenceObject, typeIndexer, out string identifier))
            {
                fieldData[fieldName] = identifier;
                return true;
            }
            return false;
        }
        public static bool DeserializeFieldReference(string fieldName, out object referenceObject, Dictionary<string, object> fieldData, Type typeIndexer)
        {
            if (fieldData.TryGetValue(fieldName, out object fieldValue) && fieldValue is string stringValue)
                if (ReferenceStorage.Instance.TryGetObject(stringValue, typeIndexer, out referenceObject))
                    return true;
            referenceObject = null;
            return false;
        }
        #endregion



        #region FUNCTIONS_SYSTEM.OBJECT_SERIALIZER
        public static void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            FieldInfo[] fields = instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (field == null || field.GetCustomAttributes(typeof(NonSerializedAttribute), true).Length > 0 || field.FieldType.GetCustomAttributes(typeof(DontSaveField), true).Length > 0)
                    continue;

                object fieldValue = field.GetValue(instance);

                if (flags.HasFlag(SerializerFlags.AssetReferences) && SerializeFieldAsset(field.Name, fieldValue, fieldData)) { }
                else if (flags.HasFlag(SerializerFlags.RuntimeReferences) && fieldValue is UnityObject)
                    AddInitializeReferenceAction(() => SerializeFieldReference(field.Name, fieldValue, fieldData, typeof(UnityObject)));
                else if (flags.HasFlag(SerializerFlags.GenericVariables) && FieldIsSerializable(field, DataSerializer.SurrogateSelector))
                    fieldData[field.Name] = fieldValue;
            }
        }

        public static void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            foreach (var pair in fieldData)
            {
                FieldInfo field = instance.GetType().GetField(pair.Key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field == null)
                    continue;

                object asset = field.FieldType;
                object dataValue = pair.Value;

                if (flags.HasFlag(SerializerFlags.AssetReferences) && DeserializeFieldAsset(field.Name, ref asset, fieldData))
                    field.SetValue(instance, asset);
                else if (flags.HasFlag(SerializerFlags.RuntimeReferences) && field.FieldType != typeof(string) && dataValue is string)
                    AddInitializeReferenceAction(delegate { if (DeserializeFieldReference(field.Name, out object unityObject, fieldData, typeof(UnityObject))) field.SetValue(instance, unityObject); });
                else if (flags.HasFlag(SerializerFlags.GenericVariables) && FieldIsSerializable(field, DataSerializer.SurrogateSelector))
                    field.SetValue(instance, dataValue);
            }
        }
        #endregion



        #region FUNCTIONS_HELPER
        public static bool FieldIsSerializable(FieldInfo fieldInfo, SurrogateSelector surrogateSelector)
        {
            if (fieldInfo.IsNotSerialized)
                return false;
            else if (surrogateSelector != null && surrogateSelector.GetSurrogate(fieldInfo.FieldType, new StreamingContext(StreamingContextStates.All), out _) != null)
                return true;
            else if (fieldInfo.FieldType.IsSerializable)
            {
                for (int j = 0; j < _incompatibleAssemblies.Length; j++)
                    if (fieldInfo.FieldType.FullName.Contains(_incompatibleAssemblies[j]))
                        return false;
                return true;
            }
            return false;
        }

        //[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
        //public class MethodSort : PropertyAttribute
        //{
        //    public readonly int priority = 0;

        //    public MethodSort(int priority)
        //    {
        //        this.priority = priority;
        //    }
        //}
        //public static MethodInfo[] GetSortedMethods<T>(BindingFlags bindingFlags)
        //{
        //    MethodInfo[] methods = typeof(T).GetMethods(bindingFlags);
        //    Dictionary<int, List<MethodInfo>> methodsByPriority = new Dictionary<int, List<MethodInfo>>();

        //    List<object> customAttributes = new List<object>();
        //    List<Type> customAttributeTypes = new List<Type>();
        //    foreach (var method in methods)
        //    {
        //        customAttributes = new List<object>(method.GetCustomAttributes(true));
        //        customAttributeTypes = new List<Type>();
        //        int attributeIndexOf;
        //        int priorityIndex = 0;
        //        foreach (var attribute in customAttributes)
        //            customAttributeTypes.Add(attribute.GetType());

        //        if ((attributeIndexOf = customAttributeTypes.IndexOf(typeof(MethodSort))) >= 0)
        //            priorityIndex = (customAttributes[attributeIndexOf] as MethodSort).priority;

        //        if (!methodsByPriority.ContainsKey(priorityIndex))
        //            methodsByPriority[priorityIndex] = new List<MethodInfo>();
        //        methodsByPriority[priorityIndex].Add(method);
        //    }

        //    List<MethodInfo> sortedMethods = new List<MethodInfo>();
        //    List<int> prioritiesSorted = new List<int>(methodsByPriority.Keys.OrderBy(x => x).Reverse());

        //    for (int i = 0; i < prioritiesSorted.Count; i++)
        //        foreach (var methodInList in methodsByPriority[prioritiesSorted[i]])
        //            sortedMethods.Add(methodInList);
        //    return sortedMethods.ToArray();
        //}
        #endregion
    }
}