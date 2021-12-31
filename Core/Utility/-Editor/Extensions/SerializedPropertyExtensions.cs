#if UNITY_EDITOR

using System.Reflection;
using System.Collections;
using UnityEditor;

namespace UnitySimplifiedEditor
{
    public static class SerializedPropertyExtensions
    {
        public static object ExposeProperty(this SerializedProperty property, BindingFlags flags)
        {
            string[] pathSplit = property.propertyPath.Split('.');
            object currentObj = property.serializedObject.targetObject;
            FieldInfo currentInfo;
            for (int i = 0; i < pathSplit.Length; i++)
            {
                if (pathSplit[i] == "Array")
                {
                    if (i + 1 < pathSplit.Length && currentObj is IList)
                    {
                        string[] elementSplit = pathSplit[i + 1].Split('[', ']');
                        if (elementSplit.Length == 3)
                            if (int.TryParse(elementSplit[1], out int element))
                            {
                                currentObj = (currentObj as IList)[element];
                                i += 1;
                                continue;
                            }
                    }
                    return null;
                }
                else
                {
                    currentInfo = currentObj.GetType().GetField(pathSplit[i], flags);
                    if (currentInfo == null)
                        return null;
                    currentObj = currentInfo.GetValue(currentObj);
                }
            }
            return currentObj;
        }
    }
}

#endif