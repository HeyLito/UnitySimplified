#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnitySimplified.SpriteAnimator.Parameters;

namespace UnitySimplifiedEditor.SpriteAnimator.Controller
{
    [CustomControllerConditionDrawer(typeof(TriggerParameter))]
    public class TriggerControllerConditionDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) => EditorGUI.LabelField(position, label);
    }
}

#endif