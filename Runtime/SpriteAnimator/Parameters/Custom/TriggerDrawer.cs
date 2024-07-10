#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace UnitySimplifiedEditor.SpriteAnimator.Parameters
{
    [CustomPropertyDrawer(typeof(Trigger))]
    public class TriggerDrawer : PropertyDrawer
    {
        private GUIStyle _circleStyle = null;
        private GUIStyle _circleDotStyle = null;


        private GUIStyle CircleStyle => _circleStyle ??= new GUIStyle("U2D.pivotDot");
        private GUIStyle CircleDotStyle => _circleDotStyle ??= new GUIStyle("U2D.dragDot");



        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_value"));
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Keyboard, position);
            bool missingLabel = label == null || label == GUIContent.none;
            SerializedProperty valueProp = property.FindPropertyRelative("_value");
            Rect previousRect = new Rect(position) { };
            Rect labelRect = previousRect = new Rect(previousRect) { x = previousRect.x, width = !missingLabel ? EditorGUIUtility.labelWidth : 0 };
            Rect fieldRect = previousRect = new Rect(previousRect) { x = previousRect.x + previousRect.width, width = (position.x + position.width) - (previousRect.x + previousRect.width) };
            Rect circleRect = new Rect(fieldRect) { x = fieldRect.x, width = 18 };
            Rect circleDotRect = new Rect(circleRect) { x = circleRect.x + 3f, y = circleRect.y + 5f, width = 10, height = 10 };
            circleRect.y += 2;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PrefixLabel(position, label);
            if (Event.current.type == EventType.Repaint)
            {
                CircleStyle.Draw(circleRect, GUIContent.none, controlID);
                if (valueProp.boolValue)
                    CircleDotStyle.Draw(circleDotRect, GUIContent.none, controlID);
            }
            valueProp.boolValue = EditorGUI.Toggle(fieldRect, GUIContent.none, valueProp.boolValue, GUIStyle.none);
            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif