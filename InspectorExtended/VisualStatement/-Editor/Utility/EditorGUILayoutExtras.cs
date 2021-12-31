#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace UnitySimplifiedEditor 
{
    public class EditorGUILayoutExtras : MonoBehaviour
    {
        #region METHODS
        public static string EditableLabel(string name, bool beginActivated = false, params GUILayoutOption[] options)
        {   return EditableLabel(name, EditorStyles.textField, beginActivated, options);   }
        public static string EditableLabel(string name, GUIStyle style, bool beginActivated = false, params GUILayoutOption[] options)
        {
            GUIContent content = new GUIContent(name);
            Rect rect = GUILayoutUtility.GetRect(content, style, options);
            return EditorGUIExtras.DoEditableLabel(rect, content, style, beginActivated);
        }

        public static object ObjectField(object obj, params GUILayoutOption[] options)
        {   return DoObjectField(GUIContent.none, obj, options);   }
        public static object ObjectField(string name, object obj, params GUILayoutOption[] options)
        {   return DoObjectField(new GUIContent(name), obj, options);   }
        public static object ObjectField(GUIContent content, object obj, params GUILayoutOption[] options)
        {   return DoObjectField(content, obj, options);   }

        private static object DoObjectField(GUIContent content, object obj, GUILayoutOption[] options)
        {
            switch (obj)
            {
                case string value:
                    return EditorGUILayout.TextField(content, value, options);
                case bool value:
                    return EditorGUILayout.Toggle(content, value, options);
                case int value:
                    return EditorGUILayout.IntField(content, value, options);
                case float value:
                    return EditorGUILayout.FloatField(content, value, options);
                case double value:
                    return EditorGUILayout.DoubleField(content, value, options);
                case long value:
                    return EditorGUILayout.LongField(content, value, options);
                case Bounds value:
                    return EditorGUILayout.BoundsField(content, value, options);
                case BoundsInt value:
                    return EditorGUILayout.BoundsIntField(content, value, options);
                case Rect value:
                    return EditorGUILayout.RectField(content, value, options);
                case RectInt value:
                    return EditorGUILayout.RectIntField(content, value, options);
                case Color value:
                    return EditorGUILayout.ColorField(content, value, options);
                case Vector2 value:
                    return EditorGUILayout.Vector2Field(content, value, options);
                case Vector2Int value:
                    return EditorGUILayout.Vector2IntField(content, value, options);
                case Vector3 value:
                    return EditorGUILayout.Vector2Field(content, value, options);
                case Vector3Int value:
                    return EditorGUILayout.Vector3IntField(content, value, options);
                case Vector4 value:
                    return EditorGUILayout.Vector4Field(content, value, options);
                case UnityObject value:
                    return EditorGUILayout.ObjectField(content, value, value.GetType(), false, options);
                case AnimationCurve value:
                    return EditorGUILayout.CurveField(content, value, options);
                case Gradient value:
                    return EditorGUILayout.GradientField(content, value, options);
                case Enum value:
                    return EditorGUILayout.EnumFlagsField(content, value, options);
                default:
                    return null;
            }
        }
        #endregion
    }
}

#endif