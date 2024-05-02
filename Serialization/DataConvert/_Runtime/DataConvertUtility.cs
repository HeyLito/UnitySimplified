using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnitySimplified.RuntimeDatabases;
using UnitySimplified.Serialization.Containers;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityObject = UnityEngine.Object;

namespace UnitySimplified.Serialization
{
    public static class DataConvertUtility
    {
        internal enum SerializerModeType { Serializing, Deserializing }

        #region FIELDS
        private static readonly string[] IncompatibleAssemblies = new[] { "UnityEngine" };
        private static readonly List<IDataConverter> Converters = new();
        private static readonly Dictionary<Type, IDataConverter> ConverterByObjectTypes = new();
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
                    if (!typeof(IDataConverter).IsAssignableFrom(type) || type == typeof(IDataConverter))
                        continue;
                    var instancedConverter = (IDataConverter)Activator.CreateInstance(type);
                    if (instancedConverter != null)
                        Converters.Add(instancedConverter);
                }
        }

        #region FUNCTIONS_INTERNAL
        public static void AddInitializeReferenceAction(Action action) => ActionOnInitializeReferences += action;
        //ForceSubConverter
        public static bool FindCustomConverter(Type objectType, out IDataConverter converter)
        {
            converter = null;
            if (ConverterByObjectTypes.TryGetValue(objectType, out converter))
                return true;

            var lookup = Converters.ToLookup(x => x.CanConvert(objectType))[true];
            var lookupOrderedResult = lookup.OrderByDescending(x => x.GetConversionPriority()).FirstOrDefault();
            if (lookupOrderedResult == null)
                return false;
            ConverterByObjectTypes[objectType] = converter = lookupOrderedResult;
            return true;

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



        #region SERIALIZERS
        public static void Serialize(object instance, IDictionary<string, object> output, SerializerFlags flags)
        {
            if (instance is ISerializationCallbackReceiver callbackReceiver)
                callbackReceiver.OnBeforeSerialize();

            foreach (var fieldInfo in instance.GetType().GetSerializedFields(typeof(DontSaveField), typeof(NonSerializedAttribute), typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)))
            {
                string fieldKey = fieldInfo.Name;
                object fieldValue = fieldInfo.GetValue(instance);

                if (TrySerializeFieldWithConverter((instance, fieldInfo), fieldKey, fieldValue, output, flags)) { }
                else if (flags.HasFlag(SerializerFlags.AssetReference) && SerializeFieldAsset(fieldKey, fieldValue, output)) { }
                else if (FieldIsSerializable(fieldInfo, flags))
                    output[fieldInfo.Name] = fieldValue;
                else if (flags.HasFlag(SerializerFlags.RuntimeReference))
                    SerializeValueAsReference(fieldKey, fieldValue, output);
            }
        }
        public static void Deserialize(object instance, IDictionary<string, object> input, SerializerFlags flags)
        {
            foreach (var fieldInfo in instance.GetType().GetSerializedFields(typeof(DontSaveField), typeof(NonSerializedAttribute), typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)))
            {
                string fieldKey = fieldInfo.Name;

                if (!input.TryGetValue(fieldKey, out var accessorValue))
                    continue;

                if (TryDeserializeFieldWithConverter((instance, fieldInfo), fieldKey, accessorValue, input, flags)) { }
                else if (flags.HasFlag(SerializerFlags.AssetReference) && DeserializeFieldAsset(fieldKey, out object asset, fieldInfo.FieldType, input))
                    fieldInfo.SetValue(instance, asset);
                else if (FieldIsSerializable(fieldInfo, flags))
                    fieldInfo.SetValue(instance, accessorValue);
                else if (flags.HasFlag(SerializerFlags.RuntimeReference))
                    DeserializeValueFromReference(fieldInfo.Name, (x) => fieldInfo.SetValue(instance, x), input);
            }

            if (instance is ISerializationCallbackReceiver callbackReceiver)
                callbackReceiver.OnAfterDeserialize();
        }
        #endregion



        #region HELPER_FUNCTIONS
        public static bool TrySerializeFieldWithConverter((object instance, FieldInfo info) context, string name, object value, IDictionary<string, object> output, SerializerFlags flags)
        {
            if (value == null)
                return false;

            var fieldType = context.info.FieldType;
            if (!FindCustomConverter(fieldType, out IDataConverter customConverter))
            {
                if (fieldType.IsGenericType)
                {
                    fieldType = fieldType.GetGenericTypeDefinition();
                    if (!FindCustomConverter(fieldType, out customConverter))
                        return false;
                }
                else return false;
            }

            if (value is ISerializationCallbackReceiver callbackReceiver)
                callbackReceiver.OnBeforeSerialize();

            ObjectData objectData = new(fieldType, new ReferenceData(value));
            customConverter.Serialize(value, objectData.accessors, flags);
            output[name] = objectData;
            return true;
        }
        public static bool TryDeserializeFieldWithConverter((object instance, FieldInfo info) context, string name, object value, IDictionary<string, object> input, SerializerFlags flags)
        {
            if (value is not ObjectData objectData)
                return false;

            var fieldType = context.info.FieldType;
            if (!FindCustomConverter(fieldType, out IDataConverter customConverter))
            {
                if (fieldType.IsGenericType)
                {
                    fieldType = fieldType.GetGenericTypeDefinition();
                    if (!FindCustomConverter(fieldType, out customConverter))
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
            customConverter.Deserialize(ref objectDataValue, objectData.accessors, flags);
            context.info.SetValue(context.instance, objectDataValue);

            if (objectDataValue is ISerializationCallbackReceiver callbackReceiver)
                callbackReceiver.OnBeforeSerialize();
            return true;
        }
        public static void SerializeValueAsReference(string valueName, object value, IDictionary<string, object> output)
        {
            AddInitializeReferenceAction(() =>
            {
                if (RuntimeReferenceDatabase.Instance.TryGet(value, out string referenceID))
                    output[$"{valueName}Ref"] = referenceID;
            });
        }
        public static void DeserializeValueFromReference(string valueName, Action<object> actionOnGetValue, IDictionary<string, object> input)
        {
            AddInitializeReferenceAction(() =>
            {
                if (!input.TryGetValue($"{valueName}Ref", out object inputValue) || inputValue is not string referenceID)
                    return;
                if (RuntimeReferenceDatabase.Instance.TryGet(referenceID, out var value))
                    actionOnGetValue.Invoke(value);
            });
        }
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