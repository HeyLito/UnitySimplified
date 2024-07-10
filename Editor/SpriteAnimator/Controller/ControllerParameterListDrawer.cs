using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnitySimplified;
using UnitySimplified.SpriteAnimator.Controller;
using UnitySimplified.SpriteAnimator.Parameters;

namespace UnitySimplifiedEditor.SpriteAnimator.Controller
{
    [CustomPropertyDrawer(typeof(SpriteAnimatorController.ControllerParameterList))]
    internal class ControllerParameterListDrawer : PropertyDrawer
    {
        private static readonly List<Type> CustomParameterTypes = new();
        private static readonly PropertyDrawableReorderableList DrawableLists = new();



        [InitializeOnLoadMethod]
        private static void FindCustomParameters()
        {
            Type genericParameterType = typeof(Parameter<>);
            foreach (var assembly in ApplicationUtility.GetAssemblies())
                foreach (var type in ApplicationUtility.GetTypesFromAssembly(assembly))
                {
                    var baseType = type.BaseType;
                    if (baseType is not { IsGenericType: true })
                        continue;
                    var genericTypeDef = baseType.GetGenericTypeDefinition();
                    if (genericTypeDef != null && genericTypeDef.IsAssignableFrom(genericParameterType))
                        CustomParameterTypes.Add(type);
                }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty itemsProp = property.FindPropertyRelative("items");
            return DrawableLists.GetList(itemsProp, () => InitializeList(itemsProp)).GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty itemsProp = property.FindPropertyRelative("items");
            ReorderableList parametersList = DrawableLists.GetList(itemsProp, () => InitializeList(itemsProp));
            Rect parametersRect = new(position) { height = parametersList.GetHeight() };

            EditorGUI.BeginChangeCheck();
            parametersList.DoList(parametersRect);
            if (EditorGUI.EndChangeCheck())
                parametersList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private static ReorderableList InitializeList(SerializedProperty serializedProperty) => new(serializedProperty.serializedObject, serializedProperty)
        {
            drawHeaderCallback = (rect) =>
            {
                EditorGUI.BeginProperty(rect, GUIContent.none, serializedProperty);
                EditorGUI.LabelField(rect, new GUIContent("Parameters", serializedProperty.tooltip));
                EditorGUI.EndProperty();
            },
            drawElementCallback = (rect, index, _, _) =>
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rect, serializedProperty.GetArrayElementAtIndex(index));
                if (!EditorGUI.EndChangeCheck())
                    return;
                serializedProperty.serializedObject.ApplyModifiedProperties();
                serializedProperty.serializedObject.Update();
            },
            onAddDropdownCallback = (rect, list) =>
            {
                if (list.serializedProperty.serializedObject.targetObject is not SpriteAnimatorController controller)
                    return;

                GenericMenu genericMenu = new();
                foreach (var parameterType in CustomParameterTypes)
                {
                    string parameterName = parameterType.Name;
                    genericMenu.AddItem(new GUIContent(parameterName), false, () =>
                    {
                        Undo.RecordObject(controller, "Add Controller Parameter");
                        controller.InternalParameters.Add(new ControllerParameter(controller, parameterType));
                        ((ISerializationCallbackReceiver)controller).OnAfterDeserialize();
                        list.serializedProperty.serializedObject.ApplyModifiedProperties();
                        list.serializedProperty.serializedObject.Update();
                    });
                }
                genericMenu.DropDown(rect);
            },
            elementHeightCallback = index => EditorGUI.GetPropertyHeight(serializedProperty.GetArrayElementAtIndex(index))
        };
    }
}