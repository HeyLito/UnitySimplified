using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnitySimplified.Collections;
using UnitySimplified.RuntimeDatabases;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityObject = UnityEngine.Object;
// ReSharper disable RedundantIfElseBlock
// ReSharper disable InvertIf

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
                    if (!typeof(IDataConverter).IsAssignableFrom(type) || type == typeof(IDataConverter) || type == typeof(IDataConverter<>))
                        continue;
                    IDataConverter instancedConverter = (IDataConverter)Activator.CreateInstance(type);
                    if (instancedConverter != null)
                        Converters.Add(instancedConverter);
                }
        }

        #region FUNCTIONS_INTERNAL
        public static void AddInitializeReferenceAction(Action action) => ActionOnInitializeReferences += action;
        //ForceSubConverter
        public static bool FindCustomConverter(Type objectType, out IDataConverter converter)
        {
            if (ConverterByObjectTypes.TryGetValue(objectType, out converter))
                return converter != null;

            var lookup = Converters.ToLookup(x => x.CanConvert(objectType))[true];
            var lookupOrderedResult = lookup.OrderByDescending(x => x.ConversionPriority()).FirstOrDefault();
            ConverterByObjectTypes[objectType] = converter = lookupOrderedResult;
            return converter != null;
        }
        #endregion



        #region FUNCTIONS_UNITYOBJECT_SERIALIZER
        public static bool TrySerializeFieldAsset(string fieldName, object asset, IDictionary<string, object> outputData)
        {
            if (!(asset as UnityObject))
                return false;

            string inputDataKey = $"{fieldName}.Ref";
            if (asset is GameObject gameObjectAsset && RuntimePrefabDatabase.Instance.TryGet(gameObjectAsset, out string key))
            {
                outputData[inputDataKey] = key;
                return true;
            }
            if (RuntimeAssetDatabase.Instance.SupportsType(asset.GetType()))
            {
                RuntimeAssetDatabase.Instance.TryGet(asset as UnityObject, out key);
                outputData[inputDataKey] = key;
                return true;
            }
            return false;
        }
        public static bool TryDeserializeFieldAsset(string fieldName, out object asset, IDictionary<string, object> inputData)
        {
            asset = null;
            string outputDataKey = $"{fieldName}.Ref";
            if (!inputData.TryGetValue(outputDataKey, out object fieldValue) || fieldValue is not string fieldStringValue)
                return false;

            if (RuntimePrefabDatabase.Instance.TryGet(fieldStringValue, out GameObject projectPrefab))
                asset = projectPrefab;
            if (RuntimeAssetDatabase.Instance.TryGet(fieldStringValue, out UnityObject projectAsset))
                asset = projectAsset;
            return true;
        }
        #endregion



        #region SERIALIZERS
        public static void Serialize(object instance, IDictionary<string, object> output)
        {
            if (instance is ISerializationCallbackReceiver callbackReceiver)
                callbackReceiver.OnBeforeSerialize();

            foreach (var fieldInfo in instance.GetType().GetSerializedFields(typeof(DontSaveField), typeof(NonSerializedAttribute), typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)))
            {
                string fieldKey = fieldInfo.Name;
                object fieldValue = fieldInfo.GetValue(instance);

                if (TrySerializeFieldAsset(fieldKey, fieldValue, output)) { }
                else if (TrySerializeValue(fieldInfo.FieldType, fieldValue, out object serializedValue))
                    output[fieldInfo.Name] = serializedValue;
                else SerializeValueAsReference(fieldKey, fieldValue, output);
            }
        }
        public static object Deserialize(object instance, IDictionary<string, object> input)
        {
            foreach (var fieldInfo in instance.GetType().GetSerializedFields(typeof(DontSaveField), typeof(NonSerializedAttribute), typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)))
            {
                string fieldKey = fieldInfo.Name;

                if (!input.TryGetValue(fieldKey, out var inputValue))
                    continue;

                if (TryDeserializeFieldAsset(fieldKey, out object asset, input))
                    fieldInfo.SetValue(instance, asset);
                else if (TryDeserializeValue(fieldInfo.FieldType, inputValue, fieldInfo.GetValue(instance), out object deserializedValue))
                    fieldInfo.SetValue(instance, deserializedValue);
                else DeserializeValueFromReference(fieldInfo.Name, (x) => fieldInfo.SetValue(instance, x), input);
            }

            if (instance is ISerializationCallbackReceiver callbackReceiver)
                callbackReceiver.OnAfterDeserialize();
            return instance;
        }
        #endregion



        #region HELPER_FUNCTIONS

        public static bool TrySerializeValue<T>(T value, out object serializedValue) => TrySerializeValue(typeof(T), value, out serializedValue);
        public static bool TrySerializeValue(Type valueType, object value, out object serializedValue)
        {
            serializedValue = null;
            if (valueType == null || value == null)
                return false;

            if (value is ISerializationCallbackReceiver callbackReceiver)
                callbackReceiver.OnBeforeSerialize();

            if (FindCustomConverter(valueType, out IDataConverter customConverter))
            {
                serializedValue = new SerializableDictionary<string, object>();
                customConverter.Serialize(valueType, value, (IDictionary<string, object>)serializedValue);
                return true;
            }
            if (valueType.IsSerializable)
            {
                serializedValue = value;
                return true;
            }

            return false;
        }
        public static bool TryDeserializeValue<T>(object value, T existingValue, out T deserializedValue)
        {
            deserializedValue = default;
            if (!TryDeserializeValue(typeof(T), value, existingValue, out object deserializedValueObject))
                return false;
            deserializedValue = (T)deserializedValueObject;
            return true;
        }
        public static bool TryDeserializeValue(Type valueType, object value, object existingValue, out object deserializedValue)
        {
            deserializedValue = false;
            if (valueType == null || value == null)
                return false;

            if (FindCustomConverter(valueType, out IDataConverter customConverter))
            {
                if (value is not IDictionary<string, object> valueInput)
                    return false;
                deserializedValue = customConverter.Deserialize(valueType, existingValue, valueInput);
            }
            else if (valueType.IsSerializable)
                deserializedValue = valueType.IsNumericType() ? CastDynamically(valueType, value) : value;

            if (!valueType.IsInstanceOfType(deserializedValue))
                return false;

            if (deserializedValue is ISerializationCallbackReceiver callbackReceiver)
                callbackReceiver.OnAfterDeserialize();

            return true;
        }
        public static void SerializeValueAsReference(string name, object value, IDictionary<string, object> output)
        {
            AddInitializeReferenceAction(() =>
            {
                if (RuntimeReferenceDatabase.Instance.TryGet(value, out string referenceID))
                    output[name] = referenceID;
            });
        }
        public static void DeserializeValueFromReference(string name, Action<object> actionOnGetValue, IDictionary<string, object> input)
        {
            AddInitializeReferenceAction(() =>
            {
                if (!input.TryGetValue(name, out object inputValue) || inputValue is not string referenceID)
                    return;
                if (RuntimeReferenceDatabase.Instance.TryGet(referenceID, out var value))
                    actionOnGetValue.Invoke(value);
            });
        }
        public static bool FieldIsSerializable(FieldInfo fieldInfo)
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

            return true;
        }
        public static object CastDynamically(this Type type, object data)
        {
            var dataParam = Expression.Parameter(typeof(object), "data");
            var body = Expression.Block(Expression.Convert(Expression.Convert(dataParam, data.GetType()), type));

            var run = Expression.Lambda(body, dataParam).Compile();
            var ret = run.DynamicInvoke(data);
            return ret;
        }
        public static T CastDynamically<T>(this object data)
        {
            var dataParam = Expression.Parameter(typeof(object), "data");
            var body = Expression.Block(Expression.Convert(Expression.Convert(dataParam, data.GetType()), typeof(T)));

            var run = Expression.Lambda(body, dataParam).Compile();
            var ret = run.DynamicInvoke(data);
            return (T)ret;
        }
        #endregion
    }
}