#if UNITY_EDITOR

using System;
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
            VisualStatement.Operand.ReferenceType referenceType = (VisualStatement.Operand.ReferenceType)referenceTypeProp.enumValueIndex;

            Rect referenceTypeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect fieldSubObjectRect = new Rect(position.x, position.y + 2 + height, objFieldWidth, height);
            Rect genericMenuFieldRect = new Rect(position.x + objFieldWidth + _spacing * 0.5f, position.y + 2 + height, position.width - objFieldWidth - _spacing * 0.5f, height);
            Rect genericObjectFieldRect = new Rect(position.x, position.y + 2 + height, position.width, height);
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(referenceTypeRect, GUIContent.none, property);
            EditorGUI.PropertyField(referenceTypeRect, referenceTypeProp, GUIContent.none);
            EditorGUI.EndProperty();
            if (EditorGUI.EndChangeCheck() && referenceType != (VisualStatement.Operand.ReferenceType)referenceTypeProp.enumValueIndex)
            {
                referenceType = (VisualStatement.Operand.ReferenceType)referenceTypeProp.enumValueIndex;
                fieldObjectProp.objectReferenceValue = null;
                fieldSubObjectProp.objectReferenceValue = null;
                valuePathProp.stringValue = "";
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                operand.Intialize(null, null);
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            switch (referenceType)
            {
                case VisualStatement.Operand.ReferenceType.Value:
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
                    break;

                case VisualStatement.Operand.ReferenceType.Field:
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
                    GenericMenuSelection selection = HandleGenericMenu(genericMenuFieldRect, operand.ValueType, valuePathProp.stringValue, fieldObjectProp.objectReferenceValue, GUIStyle.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (selection.target != null)
                            fieldObjectProp.objectReferenceValue = fieldSubObjectProp.objectReferenceValue = selection.target;
                        var type = VisualStatementUtility.GetReturnTypeFromObjectPath(fieldObjectProp.objectReferenceValue, selection.MemberPath);
                        if (type != null || string.IsNullOrEmpty(selection.path))
                        {
                            valuePathProp.stringValue = selection.MemberPath;
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
                    break;
            }
        }
        #endregion

        private PriorityQueue<int, ValueTuple<string, string, string>> GetGenericMenuOptions(UnityObject target, bool displayTarget)
        {
            PriorityQueue<int, ValueTuple<string, string, string>> optionInfos = new PriorityQueue<int, (string, string, string)>();
            foreach (var member in target.GetType().GetMembers(VisualStatementUtility.flags))
            {
                if (!VisualStatementUtility.MemberisValid(member))
                    continue;
                string memberPath; // THIRD
                string contextPath; // SECOND
                string indexedPath; // FIRST
                int priority;
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        var field = member as FieldInfo;
                        memberPath = field.Name;
                        contextPath = $"{(displayTarget ? $"{target.GetType().Name}/" : "")}{field.FieldType.Name} {memberPath}";
                        priority = 2;
                        break;
                    case MemberTypes.Property:
                        var property = member as PropertyInfo;
                        memberPath = property.Name;
                        contextPath = $"{(displayTarget ? $"{target.GetType().Name}/" : "")}{property.PropertyType.Name} {memberPath}";
                        priority = 1;
                        break;
                    case MemberTypes.Method:
                        var method = member as MethodInfo;
                        memberPath = method.Name;
                        contextPath = $"{(displayTarget ? $"{target.GetType().Name}/" : "")}{method.ReturnType.Name} {memberPath} ()";
                        priority = 0;
                        break;

                    default:
                        continue;
                }
                indexedPath = $"{target.GetType().Name}.{memberPath}";
                optionInfos.Insert(priority, (indexedPath, contextPath, memberPath));
            }
            return optionInfos;
        }
        private GenericMenuSelection HandleGenericMenu(Rect position, Type type, string path, UnityObject obj, GUIStyle style)
        {
            Event evt = Event.current;
            int currentIndex = -1;
            GUIContent content = new GUIContent("No Function");
            List<string> contextPathsList;
            Dictionary<int, ValueTuple<string, UnityObject>> tuplesByPaths = new Dictionary<int, ValueTuple<string, UnityObject>>();

            GameObject gObj = null;
            if (obj != null)
            {
                if (obj is GameObject)
                    gObj = obj as GameObject;
                else if (obj is Component)
                    gObj = (obj as Component).gameObject;
                
                content = !string.IsNullOrEmpty(path) ? gObj ? new GUIContent($"{obj.GetType().Name}.{path}") : new GUIContent(path) : content;
            }

            if (evt.type == EventType.ExecuteCommand && evt.commandName == PopupInfo.commandMessage || evt.type == EventType.MouseDown && evt.button == 0 && position.Contains(evt.mousePosition))
            {
                contextPathsList = new List<string>();
                if (gObj)
                {
                    List<UnityObject> candidates = new List<UnityObject> { gObj };
                    foreach (var component in gObj.GetComponents<Component>())
                        if (component != null)
                            candidates.Add(component);

                    for (int i = 0, j = 0; i < candidates.Count; i++)
                    {
                        var orderedOptions = GetGenericMenuOptions(candidates[i], true);
                        for (int v = 0, count = orderedOptions.Count(); v < count; v++)
                        {
                            var next = orderedOptions.Pop();
                            if (next.Item1 == $"{obj.GetType().Name}.{path}")
                                currentIndex = j;
                            contextPathsList.Add(next.Item2);
                            tuplesByPaths[j] = (next.Item3, candidates[i]);
                            j++;
                        }
                    }
                }
                else if (obj is ScriptableObject)
                {
                    int index = 0;
                    var orderedOptions = GetGenericMenuOptions(obj, true);
                    for (int i = 0, count = orderedOptions.Count(); i < count; i++)
                    {
                        var next = orderedOptions.Pop();
                        if (next.Item1 == $"{obj.GetType().Name}.{path}")
                            currentIndex = index;
                        contextPathsList.Add(next.Item2);
                        tuplesByPaths[index] = (next.Item3, obj);
                        index++;
                    }
                }
            }
            else contextPathsList = new List<string>();

            var selected = GenericMenuField(position, content, currentIndex, contextPathsList.ToArray(), style);
            if (evt.commandName == PopupInfo.commandMessage && selected > -1 && tuplesByPaths.TryGetValue(selected, out var value))
                return new GenericMenuSelection(value.Item2, value.Item1);
            else return new GenericMenuSelection("");
        }
        private int GenericMenuField(Rect position, GUIContent content, int selectedIndex, string[] paths, GUIStyle style)
        {
            if (style == null || style == GUIStyle.none)
                style = EditorStyles.popup;

            Event evt = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            var selected = PopupInfo.GetValueForControl(controlID, selectedIndex);

            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    if (evt.button == 0 && position.Contains(evt.mousePosition))
                    {
                        PopupInfo.instance = new PopupInfo(controlID);

                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("No Function"), selectedIndex == -1, () => PopupInfo.instance.SetValueDelegate(-1));
                        if (paths.Length > 0)
                        {
                            menu.AddSeparator("");
                            for (int i = 0; i < paths.Length; i++)
                            {
                                int current = i;
                                menu.AddItem(new GUIContent(paths[i]), i == selectedIndex, () => PopupInfo.instance.SetValueDelegate(current));
                            }
                        }
                        menu.DropDown(position);

                        GUIUtility.keyboardControl = controlID;
                        evt.Use();
                    }
                    break;
                case EventType.Repaint:
                    style.Draw(position, content, controlID, false, position.Contains(Event.current.mousePosition));
                    break;
            }
            return selected;
        }
        #endregion
    }

    sealed class GenericMenuSelection
    {
        public readonly UnityObject target;
        public readonly string path;
        public string MemberPath 
        {
            get
            {
                string[] splitPath = path.Split('/');
                return splitPath[splitPath.Length - 1];
            } 
        } 

        public GenericMenuSelection(UnityObject target, string path)
        {
            this.path = path;
            this.target = target;
        }
        public GenericMenuSelection(string path)
        {   this.path = path;   }
    }
}

#endif