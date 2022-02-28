using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnitySimplified
{
    public static class VisualStatementUtility
    {
        public static readonly BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

        public static bool MemberisValid(MemberInfo member)
        {
            if (member.DeclaringType == typeof(object) || member.DeclaringType == typeof(UnityObject) || member.GetCustomAttribute(typeof(ObsoleteAttribute)) != null)
                return false;

            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    //var field = member as FieldInfo;
                    return true;
                case MemberTypes.Property:
                    //var property = member as PropertyInfo;
                    return true;
                case MemberTypes.Method:
                    var method = member as MethodInfo;
                    if (method.ReturnType == typeof(void) || method.IsSpecialName || method.ContainsGenericParameters || method.GetParameters().Length > 0 || method.ReturnType.GetInterfaces().Contains(typeof(IEnumerable)))
                        return false;
                    return true;

                default:
                    break;
            }
            return false;
        }

        public static MemberInfo GetValidMember(object obj, string name)
        {
            foreach (var member in obj.GetType().GetMember(name, flags))
            {
                if (MemberisValid(member))
                    return member;
            }
            return null;
        }

        public static Type GetReturnTypeFromMember(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return (member as FieldInfo).FieldType;
                case MemberTypes.Property:
                    return (member as PropertyInfo).PropertyType;
                case MemberTypes.Method:
                    return (member as MethodInfo).ReturnType;
                default:
                    return null;
            }
        }
        public static ValueTuple<object, Type> GetResultFromMember(object obj, MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((member as FieldInfo).GetValue(obj), (member as FieldInfo).FieldType);
                case MemberTypes.Property:
                    return ((member as PropertyInfo).GetValue(obj), (member as PropertyInfo).PropertyType);
                case MemberTypes.Method:
                    return ((member as MethodInfo).Invoke(obj, null), (member as MethodInfo).ReturnType);
                default:
                    return (null, null);
            }
        }

        public static Type GetReturnTypeFromObjectPath(object obj, string path)
        {
            string[] pathSplit = path.Split('.');
            object memberObject = null;
            MemberInfo memberInfo = null;
            object currentObj = obj;
            Type currentType = null;
            for (int i = 0; i < pathSplit.Length; i++)
            {
                if (pathSplit[i] == "MemberTypeField" || pathSplit[i] == "MemberTypeProperty" || pathSplit[i] == "MemberTypeMethod")
                    switch (pathSplit[i])
                    {
                        case "MemberTypeField":
                            break;
                        case "MemberTypeProperty":
                            break;
                        case "MemberTypeMethod":
                            break;
                    }
                else
                {
                    memberObject = currentObj;
                    memberInfo = GetValidMember(memberObject, pathSplit[i]);
                    if (memberInfo == null)
                        return null;

                    if (i + 1 < pathSplit.Length)
                    {
                        var tuple = GetResultFromMember(memberObject, memberInfo);
                        currentType = tuple.Item2;
                        currentObj = tuple.Item1;
                    }
                    else return GetReturnTypeFromMember(memberInfo); 
                }
            }
            return currentType;
        }
        public static ValueTuple<ValueTuple<object, MemberInfo>, object, Type> GetValueFromObjectPath(object obj, string path)
        {
            string[] pathSplit = path.Split('.');
            object memberObject = null;
            MemberInfo memberInfo = null;
            object currentObj = obj;
            Type currentType = null;
            for (int i = 0; i < pathSplit.Length; i++)
            {
                if (pathSplit[i] == "MemberTypeField" || pathSplit[i] == "MemberTypeProperty" || pathSplit[i] == "MemberTypeMethod")
                    switch (pathSplit[i])
                    {
                        case "MemberTypeField":
                            break;
                        case "MemberTypeProperty":
                            break;
                        case "MemberTypeMethod":
                            break;
                    }
                else
                {
                    memberObject = currentObj;
                    memberInfo = GetValidMember(memberObject, pathSplit[i]);
                    if (memberInfo == null)
                        return ((null, null), null, null);

                    var tuple = GetResultFromMember(memberObject, memberInfo);
                    currentType = tuple.Item2;
                    currentObj = tuple.Item1;
                }
            }
            return ((memberObject, memberInfo), currentObj, currentType);
        }
    }
}