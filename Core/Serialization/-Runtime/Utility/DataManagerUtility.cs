using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public static class DataManagerUtility
    {
        private static bool _usingNewtonsoftJson = false;
        private static (int, Assembly, Dictionary<string, Type>) _newtonsoftJsonInfo = (-1, null, new Dictionary<string, Type>());
        private static MethodInfo _serializeObjectMethod = null;
        private static MethodInfo _deserializeObjectMethod = null;



        internal static MethodInfo SerializeObjectMethod
        {
            get
            {
                if (_serializeObjectMethod == null && DoesNewtonsoftJsonExist)
                {
                    Type type = GetNewtonSoftType("JsonConvert");
                    if (type != null)
                    {
                        _serializeObjectMethod = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                                                     .Where((x) => x.Name == "SerializeObject")
                                                     .Where((x) =>
                                                     {
                                                         ParameterInfo[] y = x.GetParameters();
                                                         return y.Length == 4
                                                         && y[0].ParameterType == typeof(object)
                                                         && y[1].ParameterType == typeof(Type)
                                                         && y[2].ParameterType.Name == "Formatting"
                                                         && y[3].ParameterType.Name == "JsonSerializerSettings";
                                                     })
                                                     .FirstOrDefault();
                    }
                }
                return _serializeObjectMethod;
            }
        }
        internal static MethodInfo DeserializeObjectMethod
        {
            get
            {
                if (_deserializeObjectMethod == null && DoesNewtonsoftJsonExist)
                {
                    Type type = GetNewtonSoftType("JsonConvert");
                    if (type != null)
                    {
                        _deserializeObjectMethod = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                                                        .Where((x) => x.Name == "DeserializeObject")
                                                        .Where((x) =>
                                                        {
                                                            ParameterInfo[] y = x.GetParameters();
                                                            return y.Length == 2
                                                            && y[0].ParameterType == typeof(string)
                                                            && y[1].ParameterType == typeof(Type);
                                                        })
                                                        .FirstOrDefault();
                    }
                }
                return _deserializeObjectMethod;
            }
        }
        
        public static bool DoesNewtonsoftJsonExist
        {
            get
            {
                if (_newtonsoftJsonInfo.Item1 == -1)
                {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                        foreach (Type type in assembly.GetTypes())
                        {
                            if (type.Namespace == "Newtonsoft.Json")
                            {
                                _newtonsoftJsonInfo.Item1 = Convert.ToInt32(true);
                                _newtonsoftJsonInfo.Item2 = assembly;
                                return true;
                            }
                        }
                    _newtonsoftJsonInfo.Item1 = Convert.ToInt32(false);
                }
                return _newtonsoftJsonInfo.Item1 != -1 && Convert.ToBoolean(_newtonsoftJsonInfo.Item1);
            }
        }
        public static bool UsingNewtonsoftJson { get => _usingNewtonsoftJson; set { if (value == true && !DoesNewtonsoftJsonExist) throw new Exception("Newtonsoft.Json Package is missing!"); _usingNewtonsoftJson = value; } }



        internal static Type GetNewtonSoftType(string typeName)
        {
            Type type = null;
            if (DoesNewtonsoftJsonExist)
                if (!_newtonsoftJsonInfo.Item3.TryGetValue(typeName, out type))
                    foreach (var typeInAssembly in _newtonsoftJsonInfo.Item2.GetTypes())
                    {
                        _newtonsoftJsonInfo.Item3[typeInAssembly.Name] = typeInAssembly;
                        if (typeInAssembly.Name == typeName)
                        {
                            _newtonsoftJsonInfo.Item3[typeName] = type = typeInAssembly;
                            break;
                        }
                    }
            return type;
        }

        public static bool IsSerializable(Type type)
        {
            if (DataManager.SurrogateSelector != null && DataManager.SurrogateSelector.GetSurrogate(type, new StreamingContext(StreamingContextStates.All), out _) != null)
                return true;
            if (type.IsSerializable)
                return true;
            return false;
        }
    }
}