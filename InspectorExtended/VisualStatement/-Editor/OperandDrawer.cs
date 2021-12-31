#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified;
using UnityObject = UnityEngine.Object;

namespace UnitySimplifiedEditor 
{
    [CustomPropertyDrawer(typeof(VisualStatement.Operand))]
    public class OperandDrawer : PropertyDrawer
    {
        #region FIELDS
        private readonly int _spacing = 10;
        #endregion

        #region METHODS
        #region PROPERTY_DRAWER
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {   return base.GetPropertyHeight(property, label) * 2 + 4;   }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            VisualStatement.Operand operand = property.ExposeProperty(VisualStatementUtility.flags | BindingFlags.NonPublic) as VisualStatement.Operand;
            var referenceTypeProp = property.FindPropertyRelative("referenceType");
            var valueTypeProp = property.FindPropertyRelative("valueType");
            var valuePathProp = property.FindPropertyRelative("valuePath");
            var fieldObjectProp = property.FindPropertyRelative("fieldObject");
            var fieldSubObjectProp = property.FindPropertyRelative("fieldSubObject");
            float objFieldWidth = position.width * 0.3f;
            float height = EditorGUIUtility.singleLineHeight;

            Rect referenceTypeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect fieldSubObjectRect = new Rect(position.x, position.y + 2 + height, objFieldWidth, height);
            Rect genericMenuFieldRect = new Rect(position.x + objFieldWidth + _spacing * 0.5f, position.y + 2 + height, position.width - objFieldWidth - _spacing * 0.5f, height);
            Rect genericObjectFieldRect = new Rect(position.x, position.y + 2 + height, position.width, height);

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(referenceTypeRect, GUIContent.none, property);
            EditorGUI.PropertyField(referenceTypeRect, referenceTypeProp, GUIContent.none);
            EditorGUI.EndProperty();
            if (EditorGUI.EndChangeCheck())
            {
                if ((VisualStatement.Operand.ReferenceType)referenceTypeProp.enumValueIndex == VisualStatement.Operand.ReferenceType.Value)
                {
                    fieldObjectProp.objectReferenceValue = null;
                    fieldSubObjectProp.objectReferenceValue = null;
                    valuePathProp.stringValue = "";
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                    operand.Intialize(null, null);
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }

            if ((VisualStatement.Operand.ReferenceType)referenceTypeProp.enumValueIndex == VisualStatement.Operand.ReferenceType.Value)
            {
                Type valueType = operand.ValueType;
                if (valueType != null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.BeginProperty(genericMenuFieldRect, GUIContent.none, valueTypeProp);
                    var newValue = EditorGUIExtras.ObjectField(genericObjectFieldRect, operand.GetValueObject(), valueType);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();
                        operand.Intialize(newValue, valueType);
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }
                    EditorGUI.EndProperty();
                }
            }
            else if ((VisualStatement.Operand.ReferenceType)referenceTypeProp.enumValueIndex == VisualStatement.Operand.ReferenceType.Field)
            {
                #region OBJECT_FIELD
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(fieldSubObjectRect, fieldSubObjectProp, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    fieldObjectProp.objectReferenceValue = fieldSubObjectProp.objectReferenceValue;
                    valuePathProp.stringValue = "";
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                    operand.Intialize(null, null);
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
                #endregion

                #region GENERIC_MENU_SELECTION
                EditorGUI.BeginDisabledGroup(!fieldObjectProp.objectReferenceValue);
                EditorGUI.BeginChangeCheck();
                EditorGUI.BeginProperty(genericMenuFieldRect, GUIContent.none, fieldObjectProp);
                EditorGUI.BeginProperty(genericMenuFieldRect, GUIContent.none, valueTypeProp);
                EditorGUI.BeginProperty(genericMenuFieldRect, GUIContent.none, valuePathProp);
                GenericMenuSelection selection = HandleGenericMenu(genericMenuFieldRect, valuePathProp.stringValue, fieldObjectProp.objectReferenceValue, GUIStyle.none);
                if (EditorGUI.EndChangeCheck())
                {
                    if (selection.target != null)
                    {
                        fieldObjectProp.objectReferenceValue = selection.target;
                    }
                    var type = VisualStatementUtility.GetReturnTypeFromObjectPath(fieldObjectProp.objectReferenceValue, selection.memberPath);
                    if (type != null || string.IsNullOrEmpty(selection.memberPath))
                    {
                        valuePathProp.stringValue = selection.memberPath;
                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();
                        operand.Intialize(null, type);
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }
                }
                EditorGUI.EndProperty();
                EditorGUI.EndProperty();
                EditorGUI.EndProperty();
                EditorGUI.EndDisabledGroup();
                #endregion
            }
        }
        #endregion

        private GenericMenuSelection HandleGenericMenu(Rect position, string path, UnityObject obj, GUIStyle style)
        {
            Event evt = Event.current;
            Dictionary<string, ValueTuple<string, UnityObject>> tuplesByPaths = new Dictionary<string, ValueTuple<string, UnityObject>>();
            var contentPath = path;

            GameObject gObj = null;
            if (obj != null)
            {
                if (obj is GameObject)
                    gObj = obj as GameObject;
                else if (obj is Component)
                    gObj = (obj as Component).gameObject;
                if (gObj)
                    contentPath = !string.IsNullOrEmpty(path) ? $"{gObj.GetType().Name}/{contentPath}" : contentPath;
            }

            if (evt.type == EventType.ExecuteCommand && evt.commandName == PopupInfo.commandMessage || evt.type == EventType.MouseDown && evt.button == 0 && position.Contains(evt.mousePosition))
            {
                if (gObj)
                {
                    List<UnityObject> candidates = new List<UnityObject> { gObj };
                    foreach (var component in gObj.GetComponents<Component>())
                        if (component != null)
                            candidates.Add(component);

                    for (int i = 0; i < candidates.Count; i++)
                        foreach (var member in candidates[i].GetType().GetMembers(VisualStatementUtility.flags))
                        {
                            if (!VisualStatementUtility.MemberisValid(member))
                                continue;

                            string memberPath = $"{member.Name}";
                            string contextPath = $"{candidates[i].GetType().Name}/{memberPath}";
                            tuplesByPaths[contextPath] = (memberPath, candidates[i]);
                        }
                }
                else if (obj is ScriptableObject)
                    foreach (var member in obj.GetType().GetMembers(VisualStatementUtility.flags))
                        if (VisualStatementUtility.MemberisValid(member))
                            tuplesByPaths[member.Name] = (member.Name, obj);
            }

            var selected = GenericMenuField(position, contentPath, tuplesByPaths.Keys.ToArray(), style);
            if (evt.commandName == PopupInfo.commandMessage && !string.IsNullOrEmpty(selected) && tuplesByPaths.TryGetValue(selected, out var value))
                return new GenericMenuSelection(value.Item2, value.Item1);
            else return new GenericMenuSelection("");
        }
        private string GenericMenuField(Rect position, string path, string[] paths, GUIStyle style)
        {
            if (style == null || style == GUIStyle.none)
                style = EditorStyles.popup;

            Event evt = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            var selected = PopupInfo.GetValueForControl(controlID, path);
            var contentPath = $"{(!string.IsNullOrEmpty(path) ? $"{path.Replace('/', '.')}" : "No Function")}";

            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    if (evt.button == 0 && position.Contains(evt.mousePosition))
                    {
                        PopupInfo.instance = new PopupInfo(controlID);

                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("No Function"), string.IsNullOrEmpty(path), () => PopupInfo.instance.SetValueDelegate(""));
                        if (paths.Length > 0)
                        {
                            menu.AddSeparator("");
                            for (int i = 0; i < paths.Length; i++)
                            {
                                var indexedPath = paths[i];
                                menu.AddItem(new GUIContent(indexedPath), contentPath == indexedPath.Replace('/', '.'), () => PopupInfo.instance.SetValueDelegate(indexedPath));
                            }
                        }
                        menu.ShowAsContext();

                        GUIUtility.keyboardControl = controlID;
                        evt.Use();
                    }
                    break;
                case EventType.Repaint:
                    style.Draw(position, new GUIContent(contentPath), controlID, false, position.Contains(Event.current.mousePosition));
                    break;
            }
            return selected;
        }
        #endregion
    }

    sealed class GenericMenuSelection
    {
        public readonly UnityObject target;
        public readonly string memberPath;

        public GenericMenuSelection(UnityObject target, string memberPath)
        {
            this.memberPath = memberPath;
            this.target = target;
        }
        public GenericMenuSelection(string memberPath)
        {   this.memberPath = memberPath;   }
    }
}

#endif