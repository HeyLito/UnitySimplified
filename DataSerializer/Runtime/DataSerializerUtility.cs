using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnitySimplified.Serialization
{
    public static class DataSerializerUtility
    {
        internal enum SerializerModeType { Serializing, Deserializing }

        #region FIELDS
        private static readonly string[] IncompatibleAssemblies = new[] { "UnityEngine" };
        private static readonly List<(Type serializerClass, CustomSerializer serializerAttribute)> SerializersInAssemblies = new();
        private static readonly Dictionary<Type, IDataSerializable> SerializersByInspectedTypes = new();
        #endregion



        #region PROPERTIES
        internal static SerializerModeType SerializerMode { get; set; }
        internal static Action ActionOnInitializeReferences { get; set; }
        internal static Action ActionOnPostSerializer { get; set; }
        #endregion


        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        #else
        [RuntimeInitializeOnLoadMethod]
        #endif
        private static void LoadCustomSerializers()
        {
            foreach (var assembly in ApplicationUtility.GetAssemblies())
                foreach (var type in ApplicationUtility.GetTypesFromAssembly(assembly))
                {
                    if (!typeof(IDataSerializable).IsAssignableFrom(type) || type == typeof(IDataSerializable))
                        continue;

                    CustomSerializer serializerAttribute = (CustomSerializer)Attribute.GetCustomAttribute(type, typeof(CustomSerializer));
                    if (serializerAttribute != null)
                        SerializersInAssemblies.Add((type, serializerAttribute));
                }
            SerializersInAssemblies.Sort((x, y) => y.serializerAttribute.overridePriority.CompareTo(x.serializerAttribute.overridePriority));
        }

        #region FUNCTIONS_INTERNAL
        public static void AddInitializeReferenceAction(Action action) => ActionOnInitializeReferences += action;
        public static bool FindCustomSerializer(Type inspectedType, out IDataSerializable serializer)
        {
            if (SerializersByInspectedTypes.TryGetValue(inspectedType, out serializer))
                return serializer != null;

            Type targetType = null;

            foreach (var (serializerType, serializerAttribute) in SerializersInAssemblies)
            {
                var serializerInspectedType = serializerAttribute.inspectedType;
                var serializerUseForDerivedClasses = serializerAttribute.useForDerivedClasses;

                if (inspectedType.IsEquivalentTo(serializerInspectedType))
                    targetType = serializerType;
                else if (serializerUseForDerivedClasses && inspectedType.IsSubclassOf(serializerInspectedType))
                    targetType = serializerType;

                if (targetType != null)
                    break;
            }

            if (targetType != null)
                serializer = (IDataSerializable)Activator.CreateInstance(targetType);
            SerializersByInspectedTypes.Add(inspectedType, serializer);
            return serializer != null;
        }
        #endregion



        #region FUNCTIONS_UNITYOBJECT_SERIALIZER
        public static bool SerializeFieldAsset(string fieldName, object asset, IDictionary<string, object> fieldData)
        {
            if (!(asset as UnityObject))
                return false;

            if (asset is GameObject gameObjectAsset && PrefabStorage.Instance.ContainsPrefab(gameObjectAsset, out string key))
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
            return false;
        }
        public static bool DeserializeFieldAsset(string fieldName, out object asset, Type assetType, IDictionary<string, object> fieldData)
        {
            asset = null;
            if (!fieldData.TryGetValue(fieldName, out object fieldValue) ||
                fieldValue is not string fieldStringValue)
                return false;

            if (assetType.IsAssignableFrom(typeof(GameObject)))
            {
                PrefabStorage.Instance.ContainsID(fieldStringValue, out GameObject result);
                asset = result;
                return true;
            }
            if (AssetStorage.Instance.SupportsType(assetType))
            {
                AssetStorage.Instance.TryGetAsset(fieldStringValue, out UnityObject result);
                asset = result;
                return true;
            }
            return false;
        }
        #endregion



        #region REFERENCE_SERIALIZER
        public static bool SerializeFieldReference(string fieldName, object referenceObject, IDictionary<string, object> fieldData, Type typeIndexer)
        {
            if (!ReferenceStorage.Instance.TryGetIdentifier(referenceObject, typeIndexer, out string identifier))
                return false;
            fieldData[fieldName] = identifier;
            return true;
        }
        public static bool DeserializeFieldReference(string fieldName, out object referenceObject, IDictionary<string, object> fieldData, Type typeIndexer)
        {
            if (fieldData.TryGetValue(fieldName, out object fieldValue) && fieldValue is string stringValue)
                if (ReferenceStorage.Instance.TryGetObject(stringValue, typeIndexer, out referenceObject))
                    return true;
            referenceObject = null;
            return false;
        }
        #endregion


        private static readonly Dictionary<string, FieldInfo> TempFieldInfos = new();
        #region FUNCTIONS_SYSTEM.OBJECT_SERIALIZER
        public static void Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var callback = instance as IDataSerializerCallback;
            callback?.OnBeforeSerialization();

            TempFieldInfos.Clear();
            var currentType = instance.GetType();
            while (currentType != null && currentType != typeof(object))
            {
                foreach (var fieldInfo in currentType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
                    TempFieldInfos.Add(fieldInfo.Name, fieldInfo);
                currentType = currentType.BaseType;
            }

            foreach (var fieldPair in TempFieldInfos)
            {
                if (fieldPair.Value == null)
                    continue;

                var attributes = fieldPair.Value.GetCustomAttributes(true);
                if (attributes.Any(attribute => attribute is NonSerializedAttribute or DontSaveField or System.Runtime.CompilerServices.CompilerGeneratedAttribute))
                    continue;

                object fieldValue = fieldPair.Value.GetValue(instance);

                if (fieldValue != null && !FindCustomSerializer(fieldPair.Value.FieldType, out _))
                {
                    ObjectData objectData = new ObjectData(fieldPair.Value.FieldType, new ReferenceData(fieldValue, typeof(UnityObject)));
                    DataSerializer.SerializeIntoData(fieldValue, objectData.fieldData, flags);
                    fieldData[fieldPair.Key] = objectData;
                }
                else if (flags.HasFlag(SerializerFlags.AssetReference) && SerializeFieldAsset(fieldPair.Key, fieldValue, fieldData))
                { }
                else if (flags.HasFlag(SerializerFlags.RuntimeReference) && fieldValue is UnityObject)
                    AddInitializeReferenceAction(() => SerializeFieldReference(fieldPair.Key, fieldValue, fieldData, typeof(UnityObject)));
                else if (FieldIsSerializable(fieldPair.Value, flags))
                    fieldData[fieldPair.Key] = fieldValue;
            }

            callback?.OnAfterSerialization();
        }

        public static void Deserialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var callback = instance as IDataSerializerCallback;
            callback?.OnBeforeSerialization();

            TempFieldInfos.Clear();
            var currentType = instance.GetType();
            while (currentType != null && currentType != typeof(object))
            {
                foreach (var fieldInfo in currentType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
                    TempFieldInfos.Add(fieldInfo.Name, fieldInfo);
                currentType = currentType.BaseType;
            }

            foreach (var (dataKey, dataValue) in fieldData)
            {
                if (!TempFieldInfos.TryGetValue(dataKey, out FieldInfo field))
                    continue;

                if (dataValue is ObjectData objectData)
                {
                    if (!FindCustomSerializer(field.FieldType, out _))
                        continue;
                    var fieldValue = field.GetValue(instance) ?? Activator.CreateInstance(field.FieldType);
                    objectData.referenceData?.Update(fieldValue);
                    DataSerializer.DeserializeIntoInstance(ref fieldValue, objectData.fieldData, flags);
                    field.SetValue(instance, fieldValue);
                }
                else if (flags.HasFlag(SerializerFlags.AssetReference) && DeserializeFieldAsset(field.Name, out object asset, field.FieldType, fieldData))
                    field.SetValue(instance, asset);
                else if (flags.HasFlag(SerializerFlags.RuntimeReference) && field.FieldType != typeof(string) && dataValue is string)
                    AddInitializeReferenceAction(delegate { if (DeserializeFieldReference(field.Name, out object unityObject, fieldData, typeof(UnityObject))) field.SetValue(instance, unityObject); });
                else if (FieldIsSerializable(field, flags))
                    field.SetValue(instance, dataValue);
                //else if (dataValue is Newtonsoft.Json.Linq.JObject)
                //{
                //    Debug.Log($"{field}, {dataValue.GetType().AssemblyQualifiedName}");
                //    //var fieldValue = field.GetValue(instance) ?? Activator.CreateInstance(field.FieldType);
                //    //DataSerializer.DeserializeIntoInstance(ref fieldValue, (dataValue as ObjectData).fieldData, flags);
                //    //field.SetValue(instance, fieldValue);
                //}
            }

            callback?.OnAfterSerialization();
        }
        #endregion



        #region FUNCTIONS_HELPER
        public static bool FieldIsSerializable(FieldInfo fieldInfo, SerializerFlags flags)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));
            if (fieldInfo.FieldType == null)
                throw new NullReferenceException($"{nameof(fieldInfo)}.{nameof(fieldInfo.FieldType)}");
            if (fieldInfo.FieldType.FullName == null)
                throw new NullReferenceException($"{nameof(fieldInfo)}.{nameof(fieldInfo.FieldType)}.{nameof(fieldInfo.FieldType.FullName)}");

            bool canBeSerialized = fieldInfo.FieldType.IsSerializable;
            if (canBeSerialized)
            {
                if (fieldInfo.FieldType.IsSerializable)
                    foreach (var IncompatibleAssembly in IncompatibleAssemblies)
                        if (fieldInfo.FieldType.FullName.Contains(IncompatibleAssembly))
                            canBeSerialized = false;
            }

            if (canBeSerialized)
            {
                if (flags.HasFlag(SerializerFlags.SerializedVariable))
                    if (fieldInfo.IsPublic || (fieldInfo.IsPrivate && fieldInfo.GetCustomAttribute(typeof(SerializeField)) != null))
                        return true;
                if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
                    if (fieldInfo.IsPrivate)
                        return true;
            }
            return false;
        }
        #endregion
    }
}