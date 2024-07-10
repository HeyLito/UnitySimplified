using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified;
using UnitySimplified.SpriteAnimator.Controller;
using UnitySimplified.SpriteAnimator.Parameters;

namespace UnitySimplifiedEditor.SpriteAnimator.Controller
{
    [CustomPropertyDrawer(typeof(ControllerCondition))]
    internal class ControllerConditionDrawer : PropertyDrawer
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
                            if (attribute is CustomControllerConditionDrawer customControllerConditionDrawer)
                                CustomDrawersByParameterTypes[customControllerConditionDrawer.ParameterType] = (customControllerConditionDrawer, type, null);

                    }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty parameterIdentifierProp = property?.FindPropertyRelative("parameterIdentifier");
            SerializedProperty parameterReferenceProp = property?.FindPropertyRelative("parameterReference");
            SerializedProperty parameterComparerProp = property?.FindPropertyRelative("parameterComparer");
            SerializedProperty parameterValueProp = parameterReferenceProp?.FindPropertyRelative("value");

            if (parameterIdentifierProp != null && parameterReferenceProp != null && parameterComparerProp != null && parameterValueProp != null)
            {
                if (parameterReferenceProp.managedReferenceValue is ParameterReference parameterReference)
                {
                    if (CustomDrawersByParameterTypes.TryGetValue(parameterReference.Type, out var tuple))
                    {
                        tuple.drawerInstance ??= Activator.CreateInstance(tuple.drawerType) as PropertyDrawer;
                        return (float)GetPropertyHeightSafeInfo.Invoke(tuple.drawerInstance, new object[] { property, label });
                    }
                    else
                    {
                        float height = 0;
                        height += 2;
                        height += Mathf.Max(EditorGUIUtility.singleLineHeight, EditorGUI.GetPropertyHeight(parameterReferenceProp), EditorGUI.GetPropertyHeight(parameterComparerProp));
                        return height;
                    }
                }
                else return EditorGUIUtility.singleLineHeight;
            }
            else return EditorGUIUtility.singleLineHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty parameterIdentifierProp = property?.FindPropertyRelative("parameterIdentifier");
            SerializedProperty parameterReferenceProp = property?.FindPropertyRelative("parameterReference");
            SerializedProperty parameterComparerProp = property?.FindPropertyRelative("parameterComparer");
            SerializedProperty parameterValueProp = parameterReferenceProp?.FindPropertyRelative("value");

            string exitReason = null;
            if (property == null)
                exitReason = "Property is NULL!";
            else if (parameterIdentifierProp == null)
                exitReason = "SerializedProperty \"parameterIdentifier\" is NULL!";
            else if (parameterReferenceProp == null)
                exitReason = "SerializedProperty \"parameterReference\" is NULL!";
            else if (parameterComparerProp == null)
                exitReason = "SerializedProperty \"parameterComparer\" is NULL!";
            else if (parameterValueProp == null)
                exitReason = "SerializedProperty \"parameterReference.value\" is NULL!";
            else if (property.serializedObject.targetObject is not SpriteAnimatorController controller)
                exitReason = "The \"targetObject\" in the serialized property does not inherit from \"SpriteAnimatorController\"!";
            else if (!controller.TryGetParameterFromIdentifier(parameterIdentifierProp.stringValue, out var controllerParameter))
                exitReason = $"The controller does not contain a parameter with the identifier of: {parameterIdentifierProp.stringValue}!";
            else if (parameterReferenceProp.managedReferenceValue is not ParameterReference parameterReference)
                exitReason = $"The type in \"parameterReference\" is not \"{nameof(ParameterReference)}\"!";
            else
            {
                label.text = controllerParameter.Name;
                if (CustomDrawersByParameterTypes.TryGetValue(parameterReference.Type, out var tuple))
                {
                    tuple.drawerInstance ??= Activator.CreateInstance(tuple.drawerType) as PropertyDrawer;
                    OnGUISafeInfo.Invoke(tuple.drawerInstance, new object[] { position, property, label });
                }
                else
                {
                    float propSpacing = 10;
                    float propDoubleSpacing = propSpacing * 2;
                    float labelWidth = position.width * 0.30f;
                    float targetValueWidth = position.width * 0.30f;
                    float parameterComparerWidth = position.width - (labelWidth + targetValueWidth + propDoubleSpacing);
                    float parameterComparerHeight = EditorGUI.GetPropertyHeight(parameterComparerProp, GUIContent.none);

                    Rect previousRect = new Rect(position) { width = 0 };
                    Rect labelRect = previousRect = new Rect(previousRect) { x = previousRect.x + previousRect.width, width = labelWidth };
                    previousRect.x += propSpacing;
                    Rect parameterComparerRect = previousRect = new Rect(previousRect) { x = previousRect.x + previousRect.width, width = parameterComparerWidth, height = parameterComparerHeight, y = position.y + (position.height / 2) - (parameterComparerHeight / 2) };
                    previousRect.x += propSpacing;
                    Rect parameterValueRect = previousRect = new Rect(previousRect) { x = previousRect.x + previousRect.width, width = targetValueWidth, y = position.y };

                    EditorGUI.LabelField(labelRect, label);
                    EditorGUI.PropertyField(parameterComparerRect, parameterComparerProp, GUIContent.none);
                    EditorGUI.PropertyField(parameterValueRect, parameterValueProp, GUIContent.none);
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