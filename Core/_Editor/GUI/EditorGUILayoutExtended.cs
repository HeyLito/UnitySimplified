#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace UnitySimplifiedEditor 
{
    public class EditorGUILayoutExtended : MonoBehaviour
    {
        #region METHODS
        public static object ObjectField(object obj, Type type, params GUILayoutOption[] options) => DoObjectField(GUIContent.none, obj, type, options);
        public static object ObjectField(string name, object obj, Type type, params GUILayoutOption[] options) => DoObjectField(new GUIContent(name), obj, type, options);
        public static object ObjectField(GUIContent content, object obj, Type type, params GUILayoutOption[] options) => DoObjectField(content, obj, type, options);
        public static string EditableLabel(string text, params GUILayoutOption[] options) => EditorGUIExtended.DoEditableLabel(GUILayoutUtility.GetRect(new GUIContent(text), EditorStyles.textField, options), new GUIContent(text));
        public static string EditableLabel(GUIContent content, params GUILayoutOption[] options) => EditorGUIExtended.DoEditableLabel(GUILayoutUtility.GetRect(content, EditorStyles.textField, options), content);

        private static object DoObjectField(GUIContent content, object obj, Type type, GUILayoutOption[] options)
        {
            switch (type.Name)
            {
                case nameof(Boolean):
                    return EditorGUILayout.Toggle(content, (bool)obj, options);
                case nameof(Int32):
                    return EditorGUILayout.IntField(content, (int)obj, options);
                case nameof(Int64):
                    return EditorGUILayout.LongField(content, (long)obj, options);
                case nameof(Single):
                    return EditorGUILayout.FloatField(content, (float)obj, options);
                case nameof(Double):
                    return EditorGUILayout.DoubleField(content, (double)obj, options);
                case nameof(String):
                    return EditorGUILayout.TextField(content, (string)obj, options);

                case nameof(Rect):
                    return EditorGUILayout.RectField(content, (Rect)obj, options);
                case nameof(RectInt):
                    return EditorGUILayout.RectIntField(content, (RectInt)obj, options);
                case nameof(Bounds):
                    return EditorGUILayout.BoundsField(content, (Bounds)obj, options);
                case nameof(BoundsInt):
                    return EditorGUILayout.BoundsIntField(content, (BoundsInt)obj, options);
                case nameof(Color):
                    return EditorGUILayout.ColorField(content, (Color)obj, options);
                case nameof(Vector2):
                    return EditorGUILayout.Vector2Field(content, (Vector2)obj, options);
                case nameof(Vector2Int):
                    return EditorGUILayout.Vector2IntField(content, (Vector2Int)obj, options);
                case nameof(Vector3):
                    return EditorGUILayout.Vector3Field(content, (Vector3)obj, options);
                case nameof(Vector3Int):
                    return EditorGUILayout.Vector3IntField(content, (Vector3Int)obj, options);
                case nameof(Vector4):
                    return EditorGUILayout.Vector4Field(content, (Vector4)obj, options);

                case nameof(AnimationCurve):
                    return EditorGUILayout.CurveField(content, (AnimationCurve)obj, options);
                case nameof(Gradient):
                    return EditorGUILayout.GradientField(content, (Gradient)obj, options);

                default:
                    if (type.IsSubclassOf(typeof(UnityObject)) && (obj == null || obj.Equals(null) || !obj.Equals(null) && type.IsInstanceOfType(obj)))
                        return EditorGUILayout.ObjectField(content, (UnityObject)obj, type, true, options);
                    if (type.IsSubclassOf(typeof(Enum)))
                        return EditorGUILayout.EnumFlagsField(content, (Enum)obj, options);
                    break;
            }
            return obj;
        }
        #endregion
    }
}

#endif