#if UNITY_EDITOR

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
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
            var parameterAssembly = typeof(Parameter<>).Assembly;
            Type genericParameterType = typeof(Parameter<>);
            foreach (var type in parameterAssembly.GetTypes())
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
            SerializedProperty itemsProp = property.FindPropertyRelative("_items");
            return DrawableLists.GetList(itemsProp, () => InitializeList(itemsProp)).GetHeight();
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty itemsProp = property.FindPropertyRelative("_items");
            ReorderableList parametersList = DrawableLists.GetList(itemsProp, () => InitializeList(itemsProp));
            Rect parametersRect = new(position) { height = parametersList.GetHeight() };
            
            EditorGUI.BeginChangeCheck();
            parametersList.DoList(parametersRect);
            if (EditorGUI.EndChangeCheck())
                parametersList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }



        private ReorderableList InitializeList(SerializedProperty serializedProperty) => new(serializedProperty.serializedObject, serializedProperty)
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
                GenericMenu genericMenu = new();
                foreach (var type in CustomParameterTypes)
                {
                    var parameterType = type;
                    string parameterName = type.Name;
                    genericMenu.AddItem(new GUIContent(parameterName), false, () =>
                    {
                        const BindingFlags bindingFlags = BindingFlags.Instance | 
                                                          BindingFlags.NonPublic | 
                                                          BindingFlags.Public | 
                                                          BindingFlags.DeclaredOnly;

                        if (list.serializedProperty.ExposePropertyInfo(bindingFlags, out FieldInfo listInfo, out object previousObj, out _, true))
                        {
                            var exposedList = (IList)listInfo.GetValue(previousObj);
                            if (exposedList != null)
                            {
                                var controller =
                                    list.serializedProperty.serializedObject.targetObject as SpriteAnimatorController;
                                Undo.RecordObject(controller, "Add Controller Parameter");
                                exposedList.Add(new ControllerParameter(controller, parameterType));
                                list.serializedProperty.serializedObject.ApplyModifiedProperties();
                                list.serializedProperty.serializedObject.Update();
                            }
                            else throw new NullReferenceException();
                        }
                        else throw new NotSupportedException();
                    });
                }

                genericMenu.DropDown(rect);
            },
            elementHeightCallback = (index) => EditorGUI.GetPropertyHeight(serializedProperty.GetArrayElementAtIndex(index))
        };
    }
}

#endif