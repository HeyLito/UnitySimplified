#if UNITY_EDITOR

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
            SerializedProperty itemsProp = property.FindPropertyRelative("items");
            return _drawableLists.GetList(itemsProp, () => InitializeControllerStatesList(itemsProp)).GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty itemsProp = property.FindPropertyRelative("items");
            ReorderableList itemsList = _drawableLists.GetList(itemsProp, () => InitializeControllerStatesList(itemsProp));
            Rect itemsRect = new(position) { height = itemsList.GetHeight() };

            EditorGUI.BeginChangeCheck();
            itemsList.DoList(itemsRect);
            if (EditorGUI.EndChangeCheck())
                itemsList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private static ReorderableList InitializeControllerStatesList(SerializedProperty serializedProperty)
        {
            return new ReorderableList(serializedProperty.serializedObject, serializedProperty)
            {
                elementHeightCallback = index => EditorGUI.GetPropertyHeight(serializedProperty.GetArrayElementAtIndex(index)),
                drawHeaderCallback = rect =>
                {
                    EditorGUI.BeginProperty(rect, GUIContent.none, serializedProperty);
                    EditorGUI.LabelField(rect, new GUIContent("States", serializedProperty.tooltip));
                    EditorGUI.EndProperty();
                },
                onCanRemoveCallback = list =>
                {
                    var controller = list.serializedProperty.serializedObject.targetObject as SpriteAnimatorController;
                    if (controller == null)
                        return false;
                    if (list.index > 0)
                        return !list.serializedProperty.GetArrayElementAtIndex(list.index)
                            .FindPropertyRelative("isGlobal").boolValue;
                    return false;
                },
                onAddCallback = list =>
                {
                    if (list.serializedProperty.serializedObject.targetObject is not SpriteAnimatorController controller)
                        return;
                    Undo.RecordObject(controller, "Add Controller State");
                    controller.InternalStates.Add(new ControllerState(controller, GetEmptyStateName(controller)));
                    ((ISerializationCallbackReceiver)controller).OnAfterDeserialize();
                    list.serializedProperty.serializedObject.ApplyModifiedProperties();
                    list.serializedProperty.serializedObject.Update();
                },
                drawElementCallback = (rect, index, _, _) =>
                {
                    using var changeCheckScope = new EditorGUI.ChangeCheckScope();
                    EditorGUI.PropertyField(rect, serializedProperty.GetArrayElementAtIndex(index));
                    if (!changeCheckScope.changed)
                        return;
                    serializedProperty.serializedObject.ApplyModifiedProperties();
                    serializedProperty.serializedObject.Update();
                }
            };
        }

        private static string GetEmptyStateName(SpriteAnimatorController controller)
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