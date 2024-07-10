#if UNITY_EDITOR

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
        private static readonly Dictionary<Type, (Attribute drawerAttribute, Type drawerType, PropertyDrawer drawerInstance)> _customDrawersByParameterTypes = new();
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
            _customDrawersByParameterTypes.Clear();
            var assemblies = ApplicationUtility.GetAssemblies();
            foreach (var assembly in assemblies)
                foreach (var type in ApplicationUtility.GetTypesFromAssembly(assembly))
                    if (typeof(PropertyDrawer).IsAssignableFrom(type))
                    {
                        var attributes = type.GetCustomAttributes(true);
                        foreach (var attribute in attributes)
                            if (attribute is CustomControllerParameterDrawer customControllerParameterDrawer)
                                _customDrawersByParameterTypes[customControllerParameterDrawer.ParameterType] = (customControllerParameterDrawer, type, null);

                    }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty identifierProp = property?.FindPropertyRelative("_identifier");
            SerializedProperty nameKeywordProp = property?.FindPropertyRelative("_nameKeyword");
            SerializedProperty parameterReferenceProp = property?.FindPropertyRelative("_parameterReference");
            SerializedProperty parameterValueProp = parameterReferenceProp?.FindPropertyRelative("_value");

            if (identifierProp != null && nameKeywordProp != null && parameterReferenceProp != null && parameterValueProp != null && !string.IsNullOrEmpty(identifierProp.stringValue))
            {
                if (parameterReferenceProp.managedReferenceValue is ParameterReference parameterReference && parameterReference != null)
                {
                    if (_customDrawersByParameterTypes.TryGetValue(parameterReference.Type, out var tuple))
                    {
                        tuple.drawerInstance ??= Activator.CreateInstance(tuple.drawerType) as PropertyDrawer;
                        return (float)GetPropertyHeightSafeInfo.Invoke(tuple.drawerInstance, new object[] { property, label });
                    }
                    else return 2 + EditorGUI.GetPropertyHeight(nameKeywordProp) + EditorGUI.GetPropertyHeight(parameterValueProp);
                }
                else return EditorGUIUtility.singleLineHeight;
            }
            else return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty identifierProp = property?.FindPropertyRelative("_identifier");
            SerializedProperty nameKeywordProp = property?.FindPropertyRelative("_nameKeyword");
            SerializedProperty parameterReferenceProp = property?.FindPropertyRelative("_parameterReference");
            SerializedProperty parameterValueProp = parameterReferenceProp?.FindPropertyRelative("_value");

            string exitReason = null;
            if (property == null)
                exitReason = "Property is NULL!";
            else if (identifierProp == null)
                exitReason = "SerializedProperty \"_identifier\" is NULL!";
            else if (nameKeywordProp == null)
                exitReason = "SerializedProperty \"_nameKeyword\" is NULL!";
            else if (parameterReferenceProp == null)
                exitReason = "SerializedProperty \"_parameterReference\" is NULL!";
            else if (parameterValueProp == null)
                exitReason = "SerializedProperty \"_parameterReference.value\" is NULL!";
            else if (string.IsNullOrEmpty(identifierProp.stringValue))
                exitReason = "SerializedProperty \"_identifier\" is empty!";
            else if (parameterReferenceProp.managedReferenceValue is not ParameterReference parameterReference || parameterReference == null)
                exitReason = $"The type in \"_parameterReference\" is not \"{nameof(ParameterReference)}\"!";
            else
            {
                if (_customDrawersByParameterTypes.TryGetValue(parameterReference.Type, out var tuple))
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

#endif