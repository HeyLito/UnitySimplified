using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Newtonsoft.Json;

namespace UnitySimplified.Serialization
{
    public static class DataManagerUtility
    {
        private static SurrogateSelector _surrogateSelector;
        private static Assembly _newtonsoftAssembly;
        private static Dictionary<string, Type> _cachedTypesByNames;
        private static MethodInfo _serializeObjectInfo;
        private static MethodInfo _deserializeObjectInfo;
        private static object _formattingIndented;

        public static SurrogateSelector SurrogateSelector
        {
            get
            {
                if (_surrogateSelector == null)
                {
                    _surrogateSelector = new SurrogateSelector();
                    var colorSurrogate = new ColorSurrogate();
                    var vector2Surrogate = new Vector2Surrogate();
                    var vector3Surrogate = new Vector3Surrogate();
                    var quaternionSurrogate = new QuaternionSurrogate();

                    _surrogateSelector.AddSurrogate(typeof(Color), new StreamingContext(StreamingContextStates.All), colorSurrogate);
                    _surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2Surrogate);
                    _surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3Surrogate);
                    _surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quaternionSurrogate);
                }
                return _surrogateSelector;
            }
        }

        public static string SerializeJsonObject(Type type, object value)
        {
            if (!SearchNewtonsoftJsonAssembly(out Assembly newtonsoftAssembly))
                return JsonUtility.ToJson(value, true);

            _cachedTypesByNames ??= GetTypesByNamesFromAssembly(newtonsoftAssembly);
            _serializeObjectInfo ??= GetSerializeObjectMethod(_cachedTypesByNames["JsonConvert"]);
            if (_formattingIndented == null)
                if (_cachedTypesByNames.TryGetValue("Formatting", out var formattingType))
                    _formattingIndented = Enum.Parse(formattingType, "Indented");

            return (string)_serializeObjectInfo.Invoke(null, new[] { value, type, _formattingIndented, null });
        }
        public static void DeserializeJsonObject(Type type, string valueData, object value)
        {
            if (!SearchNewtonsoftJsonAssembly(out Assembly newtonsoftAssembly))
            {
                JsonUtility.FromJsonOverwrite(valueData, value);
                return;
            }

            _cachedTypesByNames ??= GetTypesByNamesFromAssembly(newtonsoftAssembly);
            _deserializeObjectInfo ??= GetDeserializeObjectMethod(_cachedTypesByNames["JsonConvert"]);

            OverwriteInstanceFromOther(_deserializeObjectInfo.Invoke(null, new object[] { valueData, type }), value);
        }
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
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetField;
            FieldInfo[] fromInstanceFields = fromInstance.GetType().GetFields(flags);
            FieldInfo[] toInstanceFields = toInstance.GetType().GetFields(flags);
            for (int i = 0; i < fromInstanceFields.Length; i++)
            {
                if (fromInstanceFields[i].Attributes.HasFlag(FieldAttributes.NotSerialized))
                    continue;
                object value = fromInstanceFields[i].GetValue(fromInstance);
                toInstanceFields[i].SetValue(toInstance, value);
            }
        }

        private static bool SearchNewtonsoftJsonAssembly(out Assembly newtonsoftAssembly)
        {
            if (_newtonsoftAssembly != null)
            {
                newtonsoftAssembly = _newtonsoftAssembly;
                return true;
            }

            foreach (var assembly in ApplicationUtility.GetAssemblies())
            {
                foreach (var type in ApplicationUtility.GetTypesFromAssembly(assembly))
                {
                    if (type.Namespace != "Newtonsoft.Json")
                        continue;

                    newtonsoftAssembly = _newtonsoftAssembly = assembly;
                    return true;
                }
            }

            newtonsoftAssembly = null;
            return false;
        }
        private static Dictionary<string, Type> GetTypesByNamesFromAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            var typesByNames = new Dictionary<string, Type>();
            foreach (var typeInAssembly in assembly.GetTypes())
                typesByNames[typeInAssembly.Name] = typeInAssembly;
            return typesByNames;
        }
        private static MethodInfo GetSerializeObjectMethod(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var methodInfo =
                type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                    .Where((x) => x.Name == "SerializeObject")
                    .Where((x) =>
                    {
                        ParameterInfo[] y = x.GetParameters();
                        return y.Length == 4 && y[0].ParameterType == typeof(object) && y[1].ParameterType == typeof(Type) && y[2].ParameterType.Name == "Formatting";
                    })
                    .FirstOrDefault();

            return methodInfo ?? throw new NullReferenceException($"Could not create {nameof(MethodInfo)} from {type.Name}");
        }
        private static MethodInfo GetDeserializeObjectMethod(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var methodInfo =
                type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                    .Where((x) => x.Name == "DeserializeObject")
                    .Where((x) =>
                    {
                        ParameterInfo[] y = x.GetParameters();
                        return y.Length == 2 && y[0].ParameterType == typeof(string) && y[1].ParameterType == typeof(Type);
                    })
                    .FirstOrDefault();

            return methodInfo ?? throw new NullReferenceException($"Could not create {nameof(MethodInfo)} from {type.Name}");
        }
    }
}