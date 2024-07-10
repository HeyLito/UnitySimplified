using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified;
using UnitySimplified.SpriteAnimator.Parameters;
using UnitySimplified.SpriteAnimator.Controller;

namespace UnitySimplifiedEditor.SpriteAnimator.Controller
{
    [CustomPropertyDrawer(typeof(ControllerParameter))]
    internal class ControllerParameterDrawer : PropertyDrawer
    {
        private static readonly Dictionary<Type, (Attribute drawerAttribute, Type drawerType, PropertyDrawer drawerInstance)> CustomDrawersByParameterTypes = new();
        private static MethodInfo _getPropertyHeightSafeInfo;
        private static MethodInfo _onGUISafeInfo;


        public static MethodInfo GetPropertyHeightSafeInfo
        {
            get
            {
                if (_getPropertyHeightSafeInfo == null)
                    _getPropertyHeightSafeInfo = typeof(PropertyDrawer).GetMethod("GetPropertyHeightSafe", BindingFlags.Instance | BindingFlags.NonPublic);
                return _getPropertyHeightSafeInfo;
            }
        }
        public static MethodInfo OnGUISafeInfo
        {
            get
            {
                if (_onGUISafeInfo == null)
                    _onGUISafeInfo = typeof(PropertyDrawer).GetMethod("OnGUISafe", BindingFlags.Instance | BindingFlags.NonPublic);
                return _onGUISafeInfo;
            }
        }



        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            CustomDrawersByParameterTypes.Clear();
            var assemblies = ApplicationUtility.GetAssemblies();
            foreach (var assembly in assemblies)
                foreach (var type in ApplicationUtility.GetTypesFromAssembly(assembly))
                    if (typeof(PropertyDrawer).IsAssignableFrom(type))
                    {
                        var attributes = type.GetCustomAttributes(true);
                        foreach (var attribute in attributes)
                            if (attribute is CustomControllerParameterDrawer customControllerParameterDrawer)
                                CustomDrawersByParameterTypes[customControllerParameterDrawer.ParameterType] = (customControllerParameterDrawer, type, null);

                    }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty identifierProp = property?.FindPropertyRelative("identifier");
            SerializedProperty nameKeywordProp = property?.FindPropertyRelative("nameKeyword");
            SerializedProperty parameterReferenceProp = property?.FindPropertyRelative("parameterReference");
            SerializedProperty parameterValueProp = parameterReferenceProp?.FindPropertyRelative("value");

            if (identifierProp == null || nameKeywordProp == null || parameterReferenceProp == null || parameterValueProp == null || string.IsNullOrEmpty(identifierProp.stringValue))
                return EditorGUIUtility.singleLineHeight;
            if (parameterReferenceProp.managedReferenceValue is not ParameterReference parameterReference)
                return EditorGUIUtility.singleLineHeight;
            if (!CustomDrawersByParameterTypes.TryGetValue(parameterReference.Type, out var tuple))
                return 2 + EditorGUI.GetPropertyHeight(nameKeywordProp) + EditorGUI.GetPropertyHeight(parameterValueProp);

            tuple.drawerInstance ??= Activator.CreateInstance(tuple.drawerType) as PropertyDrawer;
            return (float)GetPropertyHeightSafeInfo.Invoke(tuple.drawerInstance, new object[] { property, label });
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty identifierProp = property?.FindPropertyRelative("identifier");
            SerializedProperty nameKeywordProp = property?.FindPropertyRelative("nameKeyword");
            SerializedProperty parameterReferenceProp = property?.FindPropertyRelative("parameterReference");
            SerializedProperty parameterValueProp = parameterReferenceProp?.FindPropertyRelative("value");

            string exitReason = null;
            if (property == null)
                exitReason = "Property is NULL!";
            else if (identifierProp == null)
                exitReason = "SerializedProperty \"identifier\" is NULL!";
            else if (nameKeywordProp == null)
                exitReason = "SerializedProperty \"nameKeyword\" is NULL!";
            else if (parameterReferenceProp == null)
                exitReason = "SerializedProperty \"parameterReference\" is NULL!";
            else if (parameterValueProp == null)
                exitReason = "SerializedProperty \"parameterReference.value\" is NULL!";
            else if (string.IsNullOrEmpty(identifierProp.stringValue))
                exitReason = "SerializedProperty \"identifier\" is empty!";
            else if (parameterReferenceProp.managedReferenceValue is not ParameterReference parameterReference)
                exitReason = $"The type in \"parameterReference\" is not \"{nameof(ParameterReference)}\"!";
            else
            {
                if (CustomDrawersByParameterTypes.TryGetValue(parameterReference.Type, out var tuple))
                {
                    tuple.drawerInstance ??= Activator.CreateInstance(tuple.drawerType) as PropertyDrawer;
                    OnGUISafeInfo.Invoke(tuple.drawerInstance, new object[] { position, property, label });
                }
                else
                {
                    EditorGUI.BeginChangeCheck();

                    Rect previousRect = new Rect(position) { height = 0 };
                    previousRect.y += 2;
                    Rect nameKeywordRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height, height = EditorGUI.GetPropertyHeight(nameKeywordProp) };
                    previousRect.y += 2;
                    Rect parameterValueRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height, height = EditorGUI.GetPropertyHeight(parameterValueProp) };

                    EditorGUI.PropertyField(nameKeywordRect, nameKeywordProp, new GUIContent("Name"));
                    EditorGUI.PropertyField(parameterValueRect, parameterValueProp, new GUIContent("Value"));
                    if (EditorGUI.EndChangeCheck())
                        property.serializedObject.ApplyModifiedProperties();
                }
            }

            if (exitReason != null)
            {
                Color contentColor = GUI.contentColor;
                GUI.contentColor = Color.red;
                EditorGUI.LabelField(position, exitReason);
                GUI.contentColor = contentColor;
                return;
            }
        }
    }
}