using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnitySimplified;
using UnitySimplified.Collections;
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
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => base.GetPropertyHeight(property, label) * 2 + 4;
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
                        var newValue = EditorGUIExtended.ObjectField(genericObjectFieldRect, operand.GetValueObject(), valueType);
                        //Debug.Log($"{operand.GetValueObject()}, {valueType}");
                        //EditorGUI.DrawRect(genericObjectFieldRect, Color.cyan);
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

        private static PriorityQueue<int, (string indexedPath, string contextPath, string memberPath)> GetGenericMenuOptions(UnityObject target, bool displayTarget)
        {
            var optionInfos = new PriorityQueue<int, (string, string, string)>();
            foreach (var member in target.GetType().GetMembers(VisualStatementUtility.flags))
            {
                if (!VisualStatementUtility.MemberisValid(member))
                    continue;

                int priority;
                (string indexedPath, string contextPath, string memberPath) result = (default, default, default);
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        if (member is not FieldInfo fieldInfo)
                            continue;
                        result.memberPath = fieldInfo.Name;
                        result.contextPath = $"{(displayTarget ? $"{target.GetType().Name}/" : "")}{fieldInfo.FieldType.Name} {result.memberPath}";
                        priority = 2;
                        break;

                    case MemberTypes.Property:
                        if (member is not PropertyInfo propertyInfo)
                            continue;
                        result.memberPath = propertyInfo.Name;
                        result.contextPath = $"{(displayTarget ? $"{target.GetType().Name}/" : "")}{propertyInfo.PropertyType.Name} {result.memberPath}";
                        priority = 1;
                        break;

                    case MemberTypes.Method:
                        if (member is not MethodInfo methodInfo)
                            continue;
                        result.memberPath = methodInfo.Name;
                        result.contextPath = $"{(displayTarget ? $"{target.GetType().Name}/" : "")}{methodInfo.ReturnType.Name} {result.memberPath} ()";
                        priority = 0;
                        break;

                    default:
                        continue;
                }
                result.indexedPath = $"{target.GetType().Name}.{result.memberPath}";
                optionInfos.Add(priority, result);
            }
            return optionInfos;
        }
        private GenericMenuSelection HandleGenericMenu(Rect position, Type type, string path, UnityObject obj, GUIStyle style)
        {
            Event evt = Event.current;
            int selectedIndex = -1;
            GUIContent content = new("No Function");
            List<string> contextPathsList;
            Dictionary<int, ValueTuple<string, UnityObject>> tuplesByPaths = new();

            if (evt.type is EventType.Repaint)
            {
                if (obj != null)
                {
                    if (obj is GameObject or Component)
                        content = !string.IsNullOrEmpty(path) ? new GUIContent($"{obj.GetType().Name}.{path}") : content;
                    else content = new GUIContent(path);
                }
            }
            if (evt.type == EventType.ExecuteCommand && evt.commandName == PopupInfo.commandMessage || evt.type == EventType.MouseDown && evt.button == 0 && position.Contains(evt.mousePosition))
            {
                contextPathsList = new List<string>();
                var candidates = new List<UnityObject> { };

                switch (obj)
                {
                    case GameObject or Component:
                    {
                        GameObject gObj;
                        if (obj is GameObject gameObject)
                            gObj = gameObject;
                        else gObj = ((Component)obj).gameObject;
                        candidates.Add(gObj);
                        candidates.AddRange(gObj.GetComponents<Component>().Where(component => component != null));
                        break;
                    }
                    case ScriptableObject:
                        candidates.Add(obj);
                        break;
                }

                for (int i = 0, j = 0; i < candidates.Count; i++)
                {
                    var orderedOptions = GetGenericMenuOptions(candidates[i], true);
                    while (orderedOptions.Pop(out (string indexedPath, string contextPath, string memberPath) pop))
                    {
                        if (pop.indexedPath == $"{obj.GetType().Name}.{path}")
                            selectedIndex = j;
                        contextPathsList.Add(pop.contextPath);
                        tuplesByPaths[j] = (pop.memberPath, candidates[i]);
                        j++;
                    }
                }
            }
            else contextPathsList = new List<string>();

            selectedIndex = MemberSelectionField(position, content, selectedIndex, contextPathsList.ToArray(), style);
            if (evt.commandName == PopupInfo.commandMessage && selectedIndex > -1 && tuplesByPaths.TryGetValue(selectedIndex, out var value))
                return new GenericMenuSelection(value.Item2, value.Item1);
            return new GenericMenuSelection("");
        }

        private static int MemberSelectionField(Rect position, GUIContent content, int selectedIndex, IReadOnlyList<string> paths, GUIStyle style)
        {
            if (style == null || style == GUIStyle.none)
                style = EditorStyles.popup;

            Event evt = Event.current;
            int control = GUIUtility.GetControlID(FocusType.Keyboard);
            var selected = PopupInfo.GetValueForControl(control, selectedIndex);

            EditorGUI.DrawRect(position, Color.cyan);

            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    if (evt.button == 0 && position.Contains(evt.mousePosition))
                    {
                        PopupInfo.instance = new PopupInfo(control);

                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("No Function"), selectedIndex == -1, () => PopupInfo.instance.SetValueDelegate(-1));
                        if (paths.Count > 0)
                        {
                            menu.AddSeparator("");
                            for (int i = 0; i < paths.Count; i++)
                            {
                                int current = i;
                                menu.AddItem(new GUIContent(paths[i]), i == selectedIndex, () => PopupInfo.instance.SetValueDelegate(current));
                            }
                        }
                        menu.DropDown(position);

                        GUIUtility.keyboardControl = control;
                        evt.Use();
                    }
                    break;
                case EventType.Repaint:
                    style.Draw(position, content, control, false, position.Contains(Event.current.mousePosition));
                    break;
            }
            return selected;
        }
        #endregion
    }

    internal sealed class GenericMenuSelection
    {
        public readonly UnityObject target;
        public readonly string path;
        public string MemberPath 
        {
            get
            {
                string[] splitPath = path.Split('/');
                return splitPath[^1];
            } 
        } 

        public GenericMenuSelection(string path) => this.path = path;
        public GenericMenuSelection(UnityObject target, string path) : this(path) => this.target = target;
    }
}