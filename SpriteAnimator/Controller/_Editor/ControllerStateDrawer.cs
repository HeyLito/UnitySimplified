#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnitySimplified.SpriteAnimator.Controller;

namespace UnitySimplifiedEditor.SpriteAnimator.Controller
{
    [CustomPropertyDrawer(typeof(ControllerState), true)]
    public class ControllerStateDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty nameProp = property.FindPropertyRelative("_name");
            SerializedProperty motionProp = property.FindPropertyRelative("_motion");
            SerializedProperty interruptionSourceProp = property.FindPropertyRelative("_interruptionSource");
            SerializedProperty isReadOnlyProp = property.FindPropertyRelative("_isReadOnly");
            if (nameProp != null && motionProp != null && interruptionSourceProp != null && isReadOnlyProp != null)
            {
                float height = 0;
                height += 2;
                height += EditorGUI.GetPropertyHeight(nameProp);
                if (!isReadOnlyProp.boolValue)
                {
                    height += 2;
                    height += EditorGUI.GetPropertyHeight(motionProp);
                    height += 2;
                    height += EditorGUI.GetPropertyHeight(interruptionSourceProp);
                }

                return height;
            }
            else return EditorGUIUtility.singleLineHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty nameProp = property.FindPropertyRelative("_name");
            SerializedProperty motionProp = property.FindPropertyRelative("_motion");
            SerializedProperty interruptionSourceProp = property.FindPropertyRelative("_interruptionSource");
            SerializedProperty isReadOnlyProp = property.FindPropertyRelative("_isReadOnly");

            if (nameProp != null && motionProp != null && interruptionSourceProp != null && isReadOnlyProp != null)
            {
                EditorGUI.BeginChangeCheck();
                Rect previousRect = new(position) { height = 0 };

                Rect nameRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height, height = EditorGUI.GetPropertyHeight(nameProp) };
                EditorGUI.BeginDisabledGroup(isReadOnlyProp.boolValue);
                if (isReadOnlyProp.boolValue)
                    EditorGUI.LabelField(nameRect, nameProp.stringValue);
                else
                {
                    string previousName = nameProp.stringValue;
                    EditorGUI.DelayedTextField(nameRect, nameProp, GUIContent.none);
                    if (((SpriteAnimatorController)property.serializedObject.targetObject).TryGetStateFromName(nameProp.stringValue, out _))
                        nameProp.stringValue = previousName;
                }
                EditorGUI.EndDisabledGroup();
                
                if (!isReadOnlyProp.boolValue)
                {
                    previousRect.y += 2;
                    Rect motionRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height, height = EditorGUI.GetPropertyHeight(motionProp) };
                    EditorGUI.PropertyField(motionRect, motionProp);
                    previousRect.y += 2;
                    Rect interruptionSourceRect = previousRect = new(previousRect) { y = previousRect.y + previousRect.height, height = EditorGUI.GetPropertyHeight(interruptionSourceProp) };
                    EditorGUI.PropertyField(interruptionSourceRect, interruptionSourceProp);
                }
                if (EditorGUI.EndChangeCheck())
                    property.serializedObject.ApplyModifiedProperties();
            }
            else
            {

            }
        }
    }
}

#endif