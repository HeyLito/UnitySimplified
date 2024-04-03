using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnitySimplified.RuntimeDatabases;
using UnitySimplified.Serialization.Containers;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityObject = UnityEngine.Object;

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

            if (asset is GameObject gameObjectAsset && RuntimePrefabDatabase.Instance.TryGet(gameObjectAsset, out string key))
            {
                fieldData[fieldName] = key;
                return true;
            }
            if (RuntimeAssetDatabase.Instance.SupportsType(asset.GetType()))
            {
                RuntimeAssetDatabase.Instance.TryGet(asset as UnityObject, out key);
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
                RuntimePrefabDatabase.Instance.TryGet(fieldStringValue, out GameObject result);
                asset = result;
                return true;
            }
            if (RuntimeAssetDatabase.Instance.SupportsType(assetType))
            {
                RuntimeAssetDatabase.Instance.TryGet(fieldStringValue, out UnityObject result);
                asset = result;
                return true;
            }
            return false;
        }
        #endregion



        #region REFERENCE_SERIALIZER
        public static bool SerializeFieldReference(string fieldName, object referenceObject, IDictionary<string, object> fieldData, Type typeIndexer)
        {
            if (!RuntimeReferenceDatabase.Instance.TryGet(referenceObject, out string identifier))
                return false;
            fieldData[fieldName] = identifier;
            return true;
        }
        public static bool DeserializeFieldReference(string fieldName, out object referenceObject, IDictionary<string, object> fieldData)
        {
            if (fieldData.TryGetValue(fieldName, out object fieldValue) && fieldValue is string stringValue)
                if (RuntimeReferenceDatabase.Instance.TryGet(stringValue, out referenceObject))
                    return true;
            referenceObject = null;
            return false;
        }
        #endregion


        public static bool OnTrySerializeField((object instance, FieldInfo info) context, string name, object value, IDictionary<string, object> output, SerializerFlags flags)
        {
            if (value == null)
                return false;

            var fieldType = context.info.FieldType;
            if (!FindCustomSerializer(fieldType, out IDataSerializable customSerializer))
            {
                if (fieldType.IsGenericType)
                {
                    fieldType = fieldType.GetGenericTypeDefinition();
                    if (!FindCustomSerializer(fieldType, out customSerializer))
                        return false;
                }
                else return false;
            }

            ObjectData objectData = new(fieldType, new ReferenceData(value));
            customSerializer.Serialize(value, objectData.accessors, flags);
            output[name] = objectData;
            return true;

        }

        public static bool OnTryDeserializeField((object instance, FieldInfo info) context, string name, object value, IDictionary<string, object> input, SerializerFlags flags)
        {
            if (value is not ObjectData objectData)
                return false;

            var fieldType = context.info.FieldType;
            if (!FindCustomSerializer(fieldType, out IDataSerializable customSerializer))
            {
                if (fieldType.IsGenericType)
                {
                    fieldType = fieldType.GetGenericTypeDefinition();
                    if (!FindCustomSerializer(fieldType, out customSerializer))
                        return false;
                }
                else return false;
            }

            if (!objectData.reference.TryGet(out object objectDataValue))
            {
                objectDataValue = context.info.GetValue(context.instance);
                if (objectDataValue == null)
                {
                    if (typeof(UnityObject).IsAssignableFrom(context.info.FieldType))
                        return false;
                    objectDataValue = Activator.CreateInstance(context.info.FieldType);
                }
                objectData.reference?.Update(objectDataValue);
            }
            customSerializer.Deserialize(ref objectDataValue, objectData.accessors, flags);
            context.info.SetValue(context.instance, objectDataValue);
            return true;
        }

        #region FUNCTIONS_SYSTEM.OBJECT_SERIALIZERs
        public static void Serialize(object instance, IDictionary<string, object> output, SerializerFlags flags)
        {
            var callback = instance as IDataSerializerCallback;
            callback?.OnBeforeSerialization();

            foreach (var fieldInfo in instance.GetType().GetSerializedFields(typeof(DontSaveField), typeof(NonSerializedAttribute), typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)))
            {
                string fieldKey = fieldInfo.Name;
                object fieldValue = fieldInfo.GetValue(instance);

                if (OnTrySerializeField((instance, fieldInfo), fieldKey, fieldValue, output, flags)) ;
                else if (flags.HasFlag(SerializerFlags.AssetReference) && SerializeFieldAsset(fieldKey, fieldValue, output));
                else if (flags.HasFlag(SerializerFlags.RuntimeReference) && fieldValue is UnityObject)
                    AddInitializeReferenceAction(() => SerializeFieldReference(fieldKey, fieldValue, output, typeof(UnityObject)));
                else if (FieldIsSerializable(fieldInfo, flags))
                    output[fieldInfo.Name] = fieldValue;
            }

            callback?.OnAfterSerialization();
        }

        public static void Deserialize(object instance, IDictionary<string, object> input, SerializerFlags flags)
        {
            var callback = instance as IDataSerializerCallback;
            callback?.OnBeforeSerialization();

            foreach (var fieldInfo in instance.GetType().GetSerializedFields(typeof(DontSaveField), typeof(NonSerializedAttribute), typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)))
            {
                string fieldKey = fieldInfo.Name;

                if (!input.TryGetValue(fieldKey, out var accessorValue))
                    continue;

                if (OnTryDeserializeField((instance, fieldInfo), fieldKey, accessorValue, input, flags)) ;
                else if (flags.HasFlag(SerializerFlags.AssetReference) && DeserializeFieldAsset(fieldKey, out object asset, fieldInfo.FieldType, input))
                    fieldInfo.SetValue(instance, asset);
                else if (flags.HasFlag(SerializerFlags.RuntimeReference) && fieldInfo.FieldType != typeof(string) && accessorValue is string)
                    AddInitializeReferenceAction(delegate { if (DeserializeFieldReference(fieldInfo.Name, out object unityObject, input)) fieldInfo.SetValue(instance, unityObject); });
                else if (FieldIsSerializable(fieldInfo, flags))
                    fieldInfo.SetValue(instance, accessorValue);
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

            if (!fieldInfo.FieldType.IsSerializable)
                return false;

            foreach (var incompatibleAssembly in IncompatibleAssemblies)
                if (fieldInfo.FieldType.Assembly.GetName().Name.Contains(incompatibleAssembly))
                    return false;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
                if (fieldInfo.IsPublic || (fieldInfo.IsPrivate && fieldInfo.GetCustomAttribute(typeof(SerializeField)) != null))
                    return true;
            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
                if (fieldInfo.IsPrivate)
                    return true;

            return false;
        }
        #endregion
    }
}