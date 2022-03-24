#if UNITY_EDITOR

using System.Reflection;
using System.Collections;
using UnityEditor;
using UnityEngine;
using System;

namespace UnitySimplifiedEditor
{
    public static class SerializedPropertyExtensions
    {
        public static object ExposeProperty(this SerializedProperty property, BindingFlags flags)
        {
            (FieldInfo, object) exposedInfo = ExposePropertyInfo(property, flags, out int listIndex);
            if (exposedInfo != (null, null))
            {
                if (listIndex > -1)
                    return (exposedInfo.Item1.GetValue(exposedInfo.Item2) as IList)[listIndex];
                else return exposedInfo.Item1.GetValue(exposedInfo.Item2);
            }
            else return null;
        }

        public static bool ExposePropertyInfo(this SerializedProperty property, BindingFlags flags, out FieldInfo info, out object previousObj, out int listIndex)
        {
            (FieldInfo, object) tuple = ExposePropertyInfo(property, flags, out listIndex);
            info = tuple.Item1;
            previousObj = tuple.Item2;
            return tuple != (null, null);
        }
        public static (FieldInfo, object) ExposePropertyInfo(this SerializedProperty property, BindingFlags flags, out int listIndex)
        {
            string[] pathSplit = property.propertyPath.Split('.');
            FieldInfo currentInfo = null;
            object currentObj = null;
            object nextObj = property.serializedObject.targetObject;
            listIndex = -1;
            for (int i = 0; i < pathSplit.Length; i++)
            {
                if (pathSplit[i] == "Array")
                {
                    if (i + 1 < pathSplit.Length && nextObj is IList)
                    {
                        string[] elementSplit = pathSplit[i + 1].Split('[', ']');
                        if (elementSplit.Length == 3)
                            if (int.TryParse(elementSplit[1], out int element))
                            {
                                if (i + 2 == pathSplit.Length)
                                    listIndex = element;
                                nextObj = (nextObj as IList)[element];
                                i += 1;
                                continue;
                            }
                    }
                    return (null, null);
                }
                else
                {
                    currentObj = nextObj;
                    currentInfo = currentObj.GetType().GetField(pathSplit[i], flags);
                    if (currentInfo == null)
                        return (null, null);
                    nextObj = currentInfo.GetValue(currentObj);
                }
            }
            return (currentInfo, currentObj);
        }
    }
}

#endif