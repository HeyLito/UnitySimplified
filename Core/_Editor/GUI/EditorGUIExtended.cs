#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityObject = UnityEngine.Object;

namespace UnitySimplifiedEditor
{
    public static class EditorGUIExtended
    {
        #region FIELDS
        public static event Action OnPingBegan;

        private static GUIStyle _pingStyle;
        private static readonly EditorPingData PingData = new();
        private static int _activeEditableLabel = -1;
        #endregion

        #region PROPERTIES
        public static bool IsPinging => PingData.IsPinging;
        public static Rect PingPosition => PingData.Position;
        public static GUIContent PingLabel => PingData.Label;
        public static GUIStyle PingStyle => _pingStyle ??= new GUIStyle("OL Ping");
        #endregion

        #region METHODS
        public static void PingStart(Rect position, GUIContent label) => DoPingStart(position, label);
        public static void PingUpdate(Rect? newPosition = null) => DoPingUpdate(newPosition);
        public static void PingEnd() => DoPingEnd();
        public static object ObjectField(Rect rect, object obj, Type type) => DoObjectField(rect, GUIContent.none, obj, type);
        public static object ObjectField(Rect rect, string name, object obj, Type type) => DoObjectField(rect, new GUIContent(name), obj, type);
        public static object ObjectField(Rect rect, GUIContent content, object obj, Type type) => DoObjectField(rect, content, obj, type);
        public static string EditableLabel(Rect rect, string text) => DoEditableLabel(rect, new GUIContent(text));
        public static string EditableLabel(Rect rect, GUIContent content) => DoEditableLabel(rect, content);

        private static void DoPingStart(Rect position, GUIContent label)
        {
            Rect pingPosition = new(position.position, PingStyle.CalcSize(label));
            PingData.Start(pingPosition, label);
            PingData.actionOnDraw = r =>
            {
                GUIStyle style = new(EditorStyles.label);
                style.alignment = TextAnchor.MiddleLeft;
                style.Draw(r, label.text, false, false, false, false);
            };

            OnPingBegan?.Invoke();
            GUIView.RepaintImmediately();
        }
        private static void DoPingUpdate(Rect? newPosition)
        {
            if (!PingData.IsPinging)
                return;

            PingData.Update(newPosition);
            GUIView.RepaintImmediately();
        }
        private static void DoPingEnd()
        {
            if (!PingData.IsPinging)
                return;

            PingData.End();
            PingData.actionOnDraw = null;
        }
        private static object DoObjectField(Rect rect, GUIContent content, object obj, Type type, GUIStyle style = null)
        {
            switch (type.Name)
            {
                case nameof(Boolean):
                    return style != null ? EditorGUI.Toggle(rect, content, (bool)obj, style) : EditorGUI.Toggle(rect, content, (bool)obj);
                case nameof(Int32):
                    return style != null ? EditorGUI.IntField(rect, content, (int)obj, style) : EditorGUI.IntField(rect, content, (int)obj);
                case nameof(Int64):
                    return style != null ? EditorGUI.LongField(rect, content, (long)obj, style) : EditorGUI.LongField(rect, content, (long)obj);
                case nameof(Single):
                    return style != null ? EditorGUI.FloatField(rect, content, (float)obj, style) : EditorGUI.FloatField(rect, content, (float)obj);
                case nameof(Double):
                    return style != null ? EditorGUI.DoubleField(rect, content, (double)obj, style) : EditorGUI.DoubleField(rect, content, (double)obj);
                case nameof(String):
                    return style != null ? EditorGUI.TextField(rect, content, (string)obj, style) : EditorGUI.TextField(rect, content, (string)obj);

                case nameof(Rect):
                    return EditorGUI.RectField(rect, content, (Rect)obj);
                case nameof(RectInt):
                    return EditorGUI.RectIntField(rect, content, (RectInt)obj);
                case nameof(Bounds):
                    return EditorGUI.BoundsField(rect, content, (Bounds)obj);
                case nameof(BoundsInt):
                    return EditorGUI.BoundsIntField(rect, content, (BoundsInt)obj);
                case nameof(Color):
                    return EditorGUI.ColorField(rect, content, (Color)obj);
                case nameof(Vector2):
                    return EditorGUI.Vector2Field(rect, content, (Vector2)obj);
                case nameof(Vector2Int):
                    return EditorGUI.Vector2IntField(rect, content, (Vector2Int)obj);
                case nameof(Vector3):
                    return EditorGUI.Vector3Field(rect, content, (Vector3)obj);
                case nameof(Vector3Int):
                    return EditorGUI.Vector3IntField(rect, content, (Vector3Int)obj);
                case nameof(Vector4):
                    return EditorGUI.Vector4Field(rect, content, (Vector4)obj);

                case nameof(AnimationCurve):
                    return EditorGUI.CurveField(rect, content, (AnimationCurve)obj);
                case nameof(Gradient):
                    return EditorGUI.GradientField(rect, content, (Gradient)obj);

                default:
                    if (type.IsSubclassOf(typeof(UnityObject)) && (obj == null || obj.Equals(null) || !obj.Equals(null) && type.IsInstanceOfType(obj)))
                        return EditorGUI.ObjectField(rect, content, (UnityObject)obj, type, true);
                    if (type.IsSubclassOf(typeof(Enum)))
                        return EditorGUI.EnumFlagsField(rect, content, (Enum)obj);
                    break;
            }
            return obj;
        }
        internal static string DoEditableLabel(Rect rect, GUIContent content)
        {
            var evt = Event.current;
            var control = GUIUtility.GetControlID(FocusType.Passive, rect);
            var textField = new GUIStyle(EditorStyles.textField);
            var labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.border = EditorStyles.textField.border;
            labelStyle.margin = EditorStyles.textField.margin;
            labelStyle.padding = EditorStyles.textField.padding;
            labelStyle.overflow = EditorStyles.textField.overflow;
            labelStyle.font = EditorStyles.textField.font;

            labelStyle.clipping = EditorStyles.textField.clipping;
            labelStyle.alignment = EditorStyles.textField.alignment;
            labelStyle.fontStyle = EditorStyles.textField.fontStyle;
            labelStyle.imagePosition = EditorStyles.textField.imagePosition;

            labelStyle.stretchHeight = EditorStyles.textField.stretchHeight;
            labelStyle.stretchWidth = EditorStyles.textField.stretchWidth;
            labelStyle.wordWrap = EditorStyles.textField.wordWrap;
            labelStyle.richText = EditorStyles.textField.richText;

            labelStyle.fontSize = EditorStyles.textField.fontSize;

            labelStyle.fixedWidth = EditorStyles.textField.fixedWidth;
            labelStyle.fixedHeight = EditorStyles.textField.fixedHeight;

            labelStyle.contentOffset = EditorStyles.textField.contentOffset;
            labelStyle.normal.textColor = EditorStyles.textField.normal.textColor;
            labelStyle.active.textColor = EditorStyles.textField.active.textColor;
            labelStyle.hover.textColor = EditorStyles.textField.hover.textColor;
            labelStyle.onNormal.textColor = EditorStyles.textField.onNormal.textColor;
            labelStyle.onActive.textColor = EditorStyles.textField.onActive.textColor;
            labelStyle.hover.textColor = EditorStyles.textField.onHover.textColor;
            labelStyle.focused.textColor = EditorStyles.textField.onFocused.textColor;

            if (_activeEditableLabel == control)
            {
                if (!rect.Contains(evt.mousePosition) && evt.type is EventType.MouseDown or EventType.ContextClick)
                {
                    _activeEditableLabel = -1;
                    GUI.changed = true;
                }
                else if (evt.type is EventType.KeyDown && evt.keyCode is KeyCode.Escape or KeyCode.Return or KeyCode.KeypadEnter)
                {
                    _activeEditableLabel = -1;
                    GUI.changed = true;
                }
            }

            if (_activeEditableLabel == control)
            {
                GUI.SetNextControlName("ActiveTextField");
                return GUI.TextField(rect, content.text, textField);
            }
            if (evt.clickCount == 2 && rect.Contains(evt.mousePosition))
            {
                _activeEditableLabel = control;
                EditorGUI.FocusTextInControl("ActiveTextField");
                GUI.changed = true;
            }

            GUI.Label(rect, content, labelStyle);
            return content.text;
        }
        #endregion
    }
}

#endif