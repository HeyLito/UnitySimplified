#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace UnitySimplifiedEditor
{
    public static class EditorGUIExtras
    {
        #region FIELDS
        private static int _activeControl = -1;
        private static bool _intializedFocus = false;
        private static HashSet<int> _activeLabels = new HashSet<int>();
        #endregion

        #region METHODS
        public static string EditableLabel(Rect rect, string name, bool beginActivated = false)
        {
            GUIContent content = new GUIContent(name);
            return DoEditableLabel(rect, content, EditorStyles.textField, beginActivated);
        }
        public static string EditableLabel(Rect rect, string name, GUIStyle style, bool beginActivated = false)
        {
            GUIContent content = new GUIContent(name);
            return DoEditableLabel(rect, content, style, beginActivated);
        }
        internal static string DoEditableLabel(Rect rect, GUIContent content, GUIStyle style, bool beginActivated)
        {
            Event evt = Event.current;
            int current = GUILayoutUtilityExtras.GetEventIndexer();

            if (beginActivated) 
                _activeLabels.Add(current);

            if (_activeControl == current)
            {
                if (!rect.Contains(evt.mousePosition) && (evt.type == EventType.MouseDown || evt.type == EventType.ContextClick))
                    _activeControl = -1;
                else if (evt.type == EventType.KeyDown && (evt.keyCode == KeyCode.Escape || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter))
                    _activeControl = -1;

                string stringFromText;
                if (_intializedFocus) 
                {
                    GUI.SetNextControlName("ActiveTextField");
                    stringFromText = GUI.TextField(rect, content.text);
                    EditorGUI.FocusTextInControl("ActiveTextField");
                    _intializedFocus = !_intializedFocus;
                }
                else stringFromText = GUI.TextField(rect, content.text, style);
                return stringFromText;
            }
            else
            {
                if (evt.type != EventType.Used && _activeLabels.Contains(current))
                {
                    _activeLabels.Remove(current);
                    _activeControl = current;
                    _intializedFocus = true;
                }
                else if (evt.clickCount == 2 && rect.Contains(evt.mousePosition)) 
                {
                    _activeControl = current;
                    _intializedFocus = true;
                }

                GUIStyle label = new GUIStyle(EditorStyles.label);

                label.border = style.border;
                label.margin = style.margin;
                label.padding = style.padding;
                label.overflow = style.overflow;
                label.font = style.font;

                label.clipping = style.clipping;
                label.alignment = style.alignment;
                label.fontStyle = style.fontStyle;
                label.imagePosition = style.imagePosition;

                label.stretchHeight = style.stretchHeight;
                label.stretchWidth = style.stretchWidth;
                label.wordWrap = style.wordWrap;
                label.richText = style.richText;

                label.fontSize = style.fontSize;

                label.fixedWidth = style.fixedWidth;
                label.fixedHeight = style.fixedHeight;

                label.contentOffset = style.contentOffset;
                label.normal.textColor = style.normal.textColor;
                label.active.textColor = style.active.textColor;
                label.hover.textColor = style.hover.textColor;
                label.focused.textColor = style.focused.textColor;
                label.onNormal.textColor = style.onNormal.textColor;
                label.onActive.textColor = style.onActive.textColor;
                label.hover.textColor = style.onHover.textColor;
                label.focused.textColor = style.onFocused.textColor;

                GUI.Label(rect, content, label);
            }
            return content.text;
        }

        public static object ObjectField(Rect rect, object obj, Type type)
        {   return DoObjectField(rect, GUIContent.none, obj, type);   }
        public static object ObjectField(Rect rect, string name, object obj, Type type)
        {   return DoObjectField(rect, new GUIContent(name), obj, type);   }
        public static object ObjectField(Rect rect, GUIContent content, object obj, Type type)
        {   return DoObjectField(rect, content, obj, type);   }

        private static object DoObjectField(Rect rect, GUIContent content, object obj, Type type)
        {
            if (type == null || !type.IsSubclassOf(typeof(UnityObject)) && (obj == null || obj.Equals(null) || (obj != null && obj.GetType() != type)))
                return obj;

            if (type == typeof(bool))
                return EditorGUI.Toggle(rect, content, (bool)obj);
            
            if (type == typeof(string))
                return EditorGUI.TextField(rect, content, (string)obj);
            
            if (type == typeof(int))
                return EditorGUI.IntField(rect, content, (int)obj);
            
            if (type == typeof(float))
                return EditorGUI.FloatField(rect, content, (float)obj);
            
            if (type == typeof(double))
                return EditorGUI.DoubleField(rect, content, (double)obj);
            
            if (type == typeof(long))
                return EditorGUI.LongField(rect, content, (long)obj);
            
            if (type == typeof(Bounds))
                return EditorGUI.BoundsField(rect, content, (Bounds)obj);
            
            if (type == typeof(BoundsInt))
                return EditorGUI.BoundsIntField(rect, content, (BoundsInt)obj);
            
            if (type == typeof(Color))
                return EditorGUI.ColorField(rect, content, (Color)obj);
            
            if (type == typeof(Vector2))
                return EditorGUI.Vector2Field(rect, content, (Vector2)obj);
            
            if (type == typeof(Vector2Int))
                return EditorGUI.Vector2IntField(rect, content, (Vector2Int)obj);

            if (type == typeof(Vector3)) 
                return EditorGUI.Vector3Field(rect, content, (Vector3)obj);
            
            if (type == typeof(Vector3Int))
                return EditorGUI.Vector3IntField(rect, content, (Vector3Int)obj);
            
            if (type == typeof(Vector4))
                return EditorGUI.Vector4Field(rect, content, (Vector4)obj);

            if (type.IsSubclassOf(typeof(UnityObject)) && (obj == null || obj.Equals(null) || !obj.Equals(null) && type.IsAssignableFrom(obj.GetType()))) 
                return EditorGUI.ObjectField(rect, content, (UnityObject)obj, type, true);
            
            if (type == typeof(AnimationCurve))
                return EditorGUI.CurveField(rect, content, (AnimationCurve)obj);
            
            if (type == typeof(Gradient))
                return EditorGUI.GradientField(rect, content, (Gradient)obj);
            
            if (type == typeof(Enum))
                return EditorGUI.EnumFlagsField(rect, content, (Enum)obj);
            return obj;
        }
        #endregion
    }
}

#endif