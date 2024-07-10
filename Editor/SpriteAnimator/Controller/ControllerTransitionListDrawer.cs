#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnitySimplified.SpriteAnimator.Controller;

namespace UnitySimplifiedEditor.SpriteAnimator.Controller
{
    [CustomPropertyDrawer(typeof(SpriteAnimatorController.ControllerTransitionList))]
    internal class ControllerTransitionListDrawer : PropertyDrawer
    {
        private readonly PropertyDrawableReorderableList _drawableLists = new();



        #region METHODS_UNITY_CALLBACKS
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty itemsProp = property.FindPropertyRelative("_items");
            return _drawableLists.GetList(itemsProp, () => InitializeReorderableList(itemsProp)).GetHeight();
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty itemsProp = property.FindPropertyRelative("_items");
            ReorderableList itemsList = _drawableLists.GetList(itemsProp, () => InitializeReorderableList(itemsProp));
            Rect itemsRect = new(position) { height = itemsList.GetHeight() };

            EditorGUI.BeginChangeCheck();
            itemsList.DoList(itemsRect);
            if (EditorGUI.EndChangeCheck())
                itemsList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }
        #endregion

        private static ReorderableList InitializeReorderableList(SerializedProperty serializedProperty)
        {
            ReorderableList list = new(serializedProperty.serializedObject, serializedProperty) { displayAdd = false };
            list.drawHeaderCallback = (rect) =>
            {
                EditorGUI.BeginProperty(rect, GUIContent.none, list.serializedProperty);
                EditorGUI.LabelField(rect, new GUIContent("Transitions", list.serializedProperty.tooltip));
                EditorGUI.EndProperty();
            };
            list.drawElementCallback = (rect, index, _, _) =>
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rect, list.serializedProperty.GetArrayElementAtIndex(index));
                if (!EditorGUI.EndChangeCheck())
                    return;
                list.serializedProperty.serializedObject.ApplyModifiedProperties();
                list.serializedProperty.serializedObject.Update();
            };
            list.elementHeightCallback = (index) => EditorGUI.GetPropertyHeight(list.serializedProperty.GetArrayElementAtIndex(index));
            return list;
        }
    }
}

#endif