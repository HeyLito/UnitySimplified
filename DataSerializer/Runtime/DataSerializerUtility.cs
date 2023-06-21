using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityObject = UnityEngine.Object;

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
                if (type == null)
                    continue;
                var attribute = (CustomSerializer)type.GetTypeInfo().GetCustomAttributes(typeof(CustomSerializer), false)
                                                      .FirstOrDefault();
                queue.Insert(attribute.priority, (type, attribute));
                //Debug.Log($"Enqueue: {attribute.inspectedType}, {type}, {attribute.priority}");
            }

            int count = queue.Count;
            for (int i = 0; i < count; i++)
            {
                var tuple = queue.Pop();
                if (tuple.Item2.allowsInheritance)
                    SerializerInfo.inheritableTypes.Add((tuple.Item2.inspectedType, tuple.Item1, tuple.Item1.GetInterface(typeof(IDataSerializable).Name).GetMethods()));
                else
                    SerializerInfo.uninheritableTypes.Add(tuple.Item2.inspectedType, (tuple.Item1, tuple.Item1.GetInterface(typeof(IDataSerializable).Name).GetMethods()));
                //Debug.Log($"Dequeue: {tuple.Item2.inspectedType}, {tuple.Item1}, {tuple.Item2.priority}");
            }
        }
        #endregion



        #region FUNCTIONS_UNITYOBJECT_SERIALIZER
        public static bool SerializeFieldAsset(string fieldName, object asset, Dictionary<string, object> fieldData)
        {
            if (asset as UnityObject)
            {
                if (asset is GameObject && PrefabStorage.Instance.ContainsPrefab(asset as GameObject, out string key))
                {
                    fieldData[fieldName] = key;
                    return true;
                }
                if (AssetStorage.Instance.SupportsType(asset.GetType()))
                {
                    AssetStorage.Instance.TryGetID(asset as UnityObject, out key);
                    fieldData[fieldName] = key;
                    return true;
                }
            }
            return false;
        }
        public static bool DeserializeFieldAsset(string fieldName, out object asset, Type assetType, Dictionary<string, object> fieldData)
        {
            if (fieldData.TryGetValue(fieldName, out object fieldValue) && fieldValue is string)
            {
                if (assetType.IsAssignableFrom(typeof(GameObject)))
                {
                    PrefabStorage.Instance.ContainsID(fieldValue as string, out GameObject result);
                    asset = result;
                    return true;
                }
                if (AssetStorage.Instance.SupportsType(assetType))
                {
                    AssetStorage.Instance.TryGetAsset(fieldValue as string, out UnityObject result);
                    asset = result;
                    return true;
                }
            }
            asset = null;
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
            if (instance is IDataSerializerCallback)
                (instance as IDataSerializerCallback).OnBeforeSerialization();

            FieldInfo[] fields = instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (field == null)
                    continue;

                bool next = false, forceCustomSerializer = false;
                var attributes = field.GetCustomAttributes(true);

                foreach (var attribute in attributes)
                {
                    if (attribute is NonSerializedAttribute || attribute is DontSaveField || attribute is System.Runtime.CompilerServices.CompilerGeneratedAttribute)
                    {
                        next = true;
                        break;
                    }
                    else if (attribute is FindCustomSerializer)
                        forceCustomSerializer = true;
                }
                if (next)
                    continue;

                object fieldValue = field.GetValue(instance);

                if (forceCustomSerializer)
                {
                    if (fieldValue != null)
                    {
                        ObjectData objectData = new ObjectData(field.FieldType, new ReferenceData(fieldValue, typeof(UnityObject)));
                        DataSerializer.SerializeIntoData(fieldValue, objectData.fieldData, flags);
                        fieldData[field.Name] = objectData;
                    }
                }
                else if (flags.HasFlag(SerializerFlags.AssetReference) && SerializeFieldAsset(field.Name, fieldValue, fieldData))
                { }
                else if (flags.HasFlag(SerializerFlags.RuntimeReference) && fieldValue is UnityObject)
                    AddInitializeReferenceAction(() => SerializeFieldReference(field.Name, fieldValue, fieldData, typeof(UnityObject)));
                else if ((flags.HasFlag(SerializerFlags.SerializedVariable) || flags.HasFlag(SerializerFlags.NonSerializedVariable)) && FieldIsSerializable(field, DataSerializer.SurrogateSelector))
                    fieldData[field.Name] = fieldValue;
            }

            if (instance is IDataSerializerCallback)
                (instance as IDataSerializerCallback).OnAfterSerialization();
        }

        public static void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (instance is IDataSerializerCallback)
                (instance as IDataSerializerCallback).OnBeforeDeserialization();

            foreach (var pair in fieldData)
            {
                FieldInfo field = instance.GetType().GetField(pair.Key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field == null)
                    continue;

                bool forceCustomSerializer = false;
                var attributes = field.GetCustomAttributes(true);
                object dataValue = pair.Value;

                foreach (var attribute in attributes)
                {
                    if (attribute is FindCustomSerializer)
                        forceCustomSerializer = true;
                }

                if (forceCustomSerializer)
                {
                    if (dataValue is ObjectData)
                    {
                        var fieldValue = field.GetValue(instance);
                        if (fieldValue == null)
                            fieldValue = Activator.CreateInstance(field.FieldType);
                        (dataValue as ObjectData).referenceData?.Update(fieldValue);
                        DataSerializer.DeserializeIntoInstance(fieldValue, (dataValue as ObjectData).fieldData, flags);
                        field.SetValue(instance, fieldValue);
                    }
                }
                else if (flags.HasFlag(SerializerFlags.AssetReference) && DeserializeFieldAsset(field.Name, out object asset, field.FieldType, fieldData))
                    field.SetValue(instance, asset);
                else if (flags.HasFlag(SerializerFlags.RuntimeReference) && field.FieldType != typeof(string) && dataValue is string)
                    AddInitializeReferenceAction(delegate { if (DeserializeFieldReference(field.Name, out object unityObject, fieldData, typeof(UnityObject))) field.SetValue(instance, unityObject); });
                else if ((flags.HasFlag(SerializerFlags.SerializedVariable) || flags.HasFlag(SerializerFlags.NonSerializedVariable)) && FieldIsSerializable(field, DataSerializer.SurrogateSelector))
                    field.SetValue(instance, dataValue);
            }

            if (instance is IDataSerializerCallback)
                (instance as IDataSerializerCallback).OnAfterDeserialization();
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
        #endregion
    }
}