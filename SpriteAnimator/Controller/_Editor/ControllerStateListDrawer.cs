#if UNITY_EDITOR

using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnitySimplified.SpriteAnimator.Controller;

namespace UnitySimplifiedEditor.SpriteAnimator.Controller
{
    [CustomPropertyDrawer(typeof(SpriteAnimatorController.ControllerStateList))]
    class ControllerStateListDrawer : PropertyDrawer
    {
        private readonly PropertyDrawableReorderableList _drawableLists = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty itemsProp = property.FindPropertyRelative("_items");
            return _drawableLists.GetList(itemsProp, () => InitializeControllerStatesList(itemsProp)).GetHeight();
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty itemsProp = property.FindPropertyRelative("_items");
            ReorderableList itemsList = _drawableLists.GetList(itemsProp, () => InitializeControllerStatesList(itemsProp));
            Rect itemsRect = new Rect(position) { height = itemsList.GetHeight() };

            EditorGUI.BeginChangeCheck();
            itemsList.DoList(itemsRect);
            if (EditorGUI.EndChangeCheck())
                itemsList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }



        private ReorderableList InitializeControllerStatesList(SerializedProperty serializedProperty)
        {
            ReorderableList list = new ReorderableList(serializedProperty.serializedObject, serializedProperty);
            list.onCanRemoveCallback = (list) =>
            {
                var controller = list.serializedProperty.serializedObject.targetObject as SpriteAnimatorController;
                if (controller != null)
                {
                    if (list.index > 0)
                        return !list.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative("_isGlobal").boolValue;
                    else return false;
                }
                else return false;
            };
            list.drawHeaderCallback = (rect) =>
            {
                EditorGUI.BeginProperty(rect, GUIContent.none, list.serializedProperty);
                EditorGUI.LabelField(rect, new GUIContent("States", list.serializedProperty.tooltip));
                EditorGUI.EndProperty();
            };
            list.drawElementCallback = (rect, index, _, _) =>
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rect, list.serializedProperty.GetArrayElementAtIndex(index));
                if (EditorGUI.EndChangeCheck())
                {
                    list.serializedProperty.serializedObject.ApplyModifiedProperties();
                    list.serializedProperty.serializedObject.Update();
                }
            };
            list.onAddCallback = (list) =>
            {
                var controller = list.serializedProperty.serializedObject.targetObject as SpriteAnimatorController;
                if (controller != null)
                {
                    int nextIndex = list.serializedProperty.arraySize;
                    list.serializedProperty.InsertArrayElementAtIndex(nextIndex);
                    list.serializedProperty.serializedObject.ApplyModifiedProperties();
                    list.serializedProperty.serializedObject.Update();
                    SerializedProperty nextProp = list.serializedProperty.GetArrayElementAtIndex(nextIndex);
                    (FieldInfo, object) exposedStateInfoPair = nextProp.ExposePropertyInfo(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, out _, true);
                    IList exposedStates = exposedStateInfoPair.Item1.GetValue(exposedStateInfoPair.Item2) as IList;
                    exposedStates[nextIndex] = new ControllerState(controller, GetEmptyStateName(controller));
                    exposedStateInfoPair.Item1.SetValue(exposedStateInfoPair.Item2, exposedStates);
                }
            };
            list.elementHeightCallback = (index) => EditorGUI.GetPropertyHeight(list.serializedProperty.GetArrayElementAtIndex(index));
            return list;
        }

        private string GetEmptyStateName(SpriteAnimatorController controller)
        {
            string name = "Empty State";
            string nameSuffix = "";
            int iteration = 0;
            while (controller.TryGetStateFromName(name + nameSuffix, out _)) { nameSuffix = " " + iteration++; }
            return name + nameSuffix;
        }
    }
}

#endif