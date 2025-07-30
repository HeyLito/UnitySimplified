#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace UnitySimplifiedEditor
{
    public class SearchableReorderableList
    {
        protected delegate void OptionAddItemCallbackDelegate(GUIContent content, bool isOn, GenericMenu.MenuFunction function);
        protected delegate void OptionAddItemDisabledCallbackDelegate(GUIContent content, bool isOn);
        protected delegate void OptionAddSeparatorCallbackDelegate(string path);

        private readonly ReorderableList _reorderableList;
        private bool _searchEnabled;
        private Regex _searchRegex = new("");
        private GUIStyle _labelStyle;
        private GUIStyle _optionsButtonStyle;
        private GUIStyle _searchFieldStyle;
        private GUIContent _optionsButtonContent;
        private FieldInfo _propertyCacheField;
        private readonly Dictionary<int, bool> _canDisplay = new();

        public SearchableReorderableList(SerializedProperty serializedProperty) => _reorderableList = CreateList(serializedProperty);
        public SearchableReorderableList(Type elementType, IList list, string label) => _reorderableList = CreateList(elementType, list, label);

        protected IList List => _reorderableList.list;
        protected SerializedProperty SerializedProperty => _reorderableList.serializedProperty;
        public bool MultiSelect { get => _reorderableList.multiSelect; set => _reorderableList.multiSelect = value; }

        public void LayoutList() => _reorderableList.DoLayoutList();
        protected virtual string OnGetSearchValue(ReorderableList list, int index) => list.serializedProperty != null ? GetSearchableTextFromProperty(list.serializedProperty.GetArrayElementAtIndex(index)) : list.list != null ? list.list[index].ToString() : string.Empty;

        protected virtual void OnDrawElement(ReorderableList list, Rect rect, int index, bool active, bool focused)
        {
            if (list.serializedProperty != null)
                EditorGUI.PropertyField(rect, list.serializedProperty.GetArrayElementAtIndex(index), true);
            else if (list.list != null)
                EditorGUI.LabelField(rect, list.list[index] != null ? list.list[index].ToString() : null);
            else throw new NullReferenceException();
        }
        protected virtual void OnContextMenu(OptionAddItemCallbackDelegate optionAddItem, OptionAddItemDisabledCallbackDelegate optionAddItemDisabled, OptionAddSeparatorCallbackDelegate optionAddSeparator) { }
        protected virtual void OnContextMenuElement(ReorderableList list, int[] indices, OptionAddItemCallbackDelegate optionAddItem, OptionAddItemDisabledCallbackDelegate optionAddItemDisabled, OptionAddSeparatorCallbackDelegate optionAddSeparator)
        {
            optionAddItem(new GUIContent("Remove"), false, () =>
            {
                if (list.serializedProperty != null)
                    for (int i = indices.Length - 1; i >= 0; i--)
                        list.serializedProperty.DeleteArrayElementAtIndex(indices[i]);
                else if (list.list != null)
                    for (int i = indices.Length - 1; i >= 0; i--)
                        list.list.RemoveAt(indices[i]);
                else throw new NullReferenceException();
            });
        }
        protected virtual void OnRemove(ReorderableList list)
        {
            if (list.serializedProperty != null)
                for (int i = list.selectedIndices.Count - 1; i >= 0; i--)
                    list.serializedProperty.DeleteArrayElementAtIndex(list.selectedIndices[i]);
            else if (list.list != null)
                for (int i = list.selectedIndices.Count - 1; i >= 0; i--)
                    list.list.RemoveAt(list.selectedIndices[i]);
            else throw new NullReferenceException();
        }

        protected ReorderableList CreateList(SerializedProperty serializedProperty, string label = default)
        {
            ReorderableList reorderableList = null;

            GUIContent labelContent = new GUIContent(!string.IsNullOrEmpty(label) ? label : serializedProperty.displayName);
            Action actionOnContextMenuElement = null;

            string searchText = "";

            // ReSharper disable AccessToModifiedClosure
            reorderableList = new ReorderableList(serializedProperty.serializedObject, serializedProperty)
            {
                drawHeaderCallback = rect => DrawHeader(reorderableList, rect, labelContent, ref searchText),
                elementHeightCallback = index => GetElementHeight(reorderableList, index),
                drawElementCallback = (rect, index, active, focused) => DrawElement(reorderableList, rect, index, active, focused, ref actionOnContextMenuElement),
                onRemoveCallback = OnRemove,
                multiSelect = true
            };
            return reorderableList;
            // ReSharper restore AccessToModifiedClosure
        }
        protected ReorderableList CreateList(Type elementType, IList list, string label)
        {
            ReorderableList reorderableList = null;

            GUIContent labelContent = new GUIContent(label);
            Action actionOnContextMenuElement = null;

            string searchText = "";

            // ReSharper disable AccessToModifiedClosure
            reorderableList = new ReorderableList(list, elementType)
            {
                drawHeaderCallback = rect => DrawHeader(reorderableList, rect, labelContent, ref searchText),
                elementHeightCallback = index => GetElementHeight(reorderableList, index),
                drawElementCallback = (rect, index, active, focused) => DrawElement(reorderableList, rect, index, active, focused, ref actionOnContextMenuElement),
                onRemoveCallback = OnRemove,
                multiSelect = true
            };
            return reorderableList;
            // ReSharper restore AccessToModifiedClosure
        }

        private void DrawHeader(ReorderableList list, Rect rect, GUIContent content, ref string searchText)
        {
            _labelStyle ??= EditorStyles.label;
            _optionsButtonContent ??= EditorGUIUtility.IconContent("_Popup@2x");
            float totalWidth = rect.width;
            float labelWidth = _labelStyle.CalcSize(content).x + 8;
            float optionsButtonWidth = 22;

            Rect previousRect = rect;
            Rect labelRect = previousRect = new Rect(previousRect) { x = previousRect.x, width = labelWidth };
            totalWidth -= previousRect.width;
            Rect optionsButtonRect = previousRect = new Rect(rect) { x = rect.x + rect.width - optionsButtonWidth, width = optionsButtonWidth };
            totalWidth -= previousRect.width;
            float searchFieldWidth = Mathf.Clamp(totalWidth, 16, 120 + rect.width * 0.16f);
            Rect searchFieldRect = new Rect(rect) { x = previousRect.x - searchFieldWidth, width = searchFieldWidth };
            searchFieldRect.yMin += 1;
            searchFieldRect.xMax -= 1;

            EditorGUI.LabelField(labelRect, content);
            DrawMenuButton(list, optionsButtonRect, _optionsButtonContent);
            searchText = DrawSearchBar(list, searchFieldRect, searchText);
        }

        private float GetElementHeight(ReorderableList list, int index)
        {
            if (!_searchEnabled)
                return EditorGUIUtility.singleLineHeight;

            bool canDisplay = _canDisplay[index] = _searchRegex.IsMatch(OnGetSearchValue(list, index));
            if (canDisplay)
                return EditorGUIUtility.singleLineHeight;

            return 0;
        }

        private void DrawElement(ReorderableList list, Rect rect, int index, bool active, bool focused, ref Action actionOnContextMenu)
        {
            if ((!_searchEnabled || !_canDisplay.TryGetValue(index, out bool canDisplayAtIndex) || !canDisplayAtIndex) && _searchEnabled)
                return;

            OnDrawElement(list, rect, index, active, focused);
            if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition) &&
                Event.current.button == 1)
            {
                GUI.changed = true;
                Event.current.Use();
                list.GrabKeyboardFocus();

                int[] indices = list.selectedIndices.Select(x => x).ToArray();
                if (!indices.Contains(index))
                {
                    list.Select(index);
                    indices = new[] { index };
                }

                GenericMenu genericMenu = new GenericMenu();
                void OptionAddItem(GUIContent content, bool isOn, GenericMenu.MenuFunction function) => genericMenu.AddItem(content, isOn, function);
                void OptionAddItemDisabled(GUIContent content, bool isOn) => genericMenu.AddDisabledItem(content, isOn);
                void OptionAddSeparator(string path) => genericMenu.AddSeparator(path);
                OnContextMenuElement(list, indices, OptionAddItem, OptionAddItemDisabled, OptionAddSeparator);
                actionOnContextMenu = () => genericMenu.DropDown(rect);
            }

            if (Event.current.type != EventType.Repaint)
                return;
            actionOnContextMenu?.Invoke();
            actionOnContextMenu = null;
        }
        private bool DrawMenuButton(ReorderableList list, Rect rect, GUIContent label)
        {
            _optionsButtonStyle ??= EditorStyles.toolbarButton;
            if (!GUI.Button(rect, label, _optionsButtonStyle))
                return false;
            GUI.changed = true;
            Event.current.Use();
            list.GrabKeyboardFocus();

            GenericMenu genericMenu = new GenericMenu();
            void OptionAddItem(GUIContent content, bool isOn, GenericMenu.MenuFunction function) => genericMenu.AddItem(content, isOn, function);
            void OptionAddItemDisabled(GUIContent content, bool isOn) => genericMenu.AddDisabledItem(content, isOn);
            void OptionAddSeparator(string path) => genericMenu.AddSeparator(path);
            OnContextMenu(OptionAddItem, OptionAddItemDisabled, OptionAddSeparator);
            genericMenu.DropDown(rect);
            return true;
        }
        private string DrawSearchBar(ReorderableList list, Rect rect, string text)
        {
            _propertyCacheField ??= typeof(ReorderableList).GetField("m_PropertyCacheValid", BindingFlags.Instance | BindingFlags.NonPublic);
            _searchFieldStyle ??= EditorStyles.toolbarSearchField;

            // ReSharper disable PossibleNullReferenceException
            var newSearchText = EditorGUI.TextField(rect, text, _searchFieldStyle);
            if (newSearchText.Equals(text))
                return text;
            if (string.IsNullOrEmpty(newSearchText))
            {
                _searchEnabled = false;
                list.draggable = true;
                _propertyCacheField.SetValue(list, false);
            }
            try
            {
                _searchRegex = new Regex(text = newSearchText);
                _searchEnabled = !string.IsNullOrEmpty(newSearchText);
                list.draggable = !_searchEnabled;
                _propertyCacheField.SetValue(list, false);
            }
            catch (Exception ex) when (ex is ArgumentException or ArgumentNullException)
            {
                _searchEnabled = false;
                list.draggable = true;
                _propertyCacheField.SetValue(list, false);
            }
            return text;
            // ReSharper restore PossibleNullReferenceException
        }
        private static string GetSearchableTextFromProperty(SerializedProperty prop) => prop.propertyType switch
        {
            SerializedPropertyType.ManagedReference => $"NAME={prop.displayName}, VALUE={(prop.managedReferenceValue != null ? prop.managedReferenceValue.ToString() : "Empty")}",
            SerializedPropertyType.ObjectReference => $"NAME={prop.displayName}, VALUE={(prop.objectReferenceValue ? prop.objectReferenceValue.ToString() : "NULL")}",
            SerializedPropertyType.Boolean => $"NAME={prop.displayName}, VALUE={prop.boolValue}",
            SerializedPropertyType.Integer => $"NAME={prop.displayName}, VALUE={prop.intValue}",
            SerializedPropertyType.Float => $"NAME={prop.displayName}, VALUE={prop.floatValue}",
            SerializedPropertyType.Vector2 => $"NAME={prop.displayName}, VALUE=(X={prop.vector2Value.x}, Y={prop.vector2Value.y})",
            SerializedPropertyType.Vector2Int => $"NAME={prop.displayName}, VALUE=(X={prop.vector2IntValue.x}, Y={prop.vector2IntValue.y})",
            SerializedPropertyType.Vector3 => $"NAME={prop.displayName}, VALUE=(X={prop.vector3Value.x}, Y={prop.vector3Value.y}, {prop.vector3Value.z})",
            SerializedPropertyType.Vector3Int => $"NAME={prop.displayName}, VALUE=(X={prop.vector3IntValue.x}, Y={prop.vector3IntValue.y}, Z={prop.vector3IntValue.z})",
            SerializedPropertyType.Vector4 => $"NAME={prop.displayName}, VALUE=(X={prop.vector4Value.x}, Y={prop.vector4Value.y}, Z={prop.vector4Value.z}, W={prop.vector4Value.w})",
            SerializedPropertyType.Quaternion => $"NAME={prop.displayName}, VALUE=(X={prop.quaternionValue.x}, Y={prop.quaternionValue.y}, Z={prop.quaternionValue.z}, W={prop.quaternionValue.w})",
            //SerializedPropertyType.Enum => $"NAME={prop.displayName}, VALUE=({prop.enumNames[prop.enumValueIndex]})",
            _ => prop.displayName
        };
    }
    public class SearchableReorderableList<T> : SearchableReorderableList
    {
        public SearchableReorderableList(IList<T> collection, string label) : base(typeof(T), (IList)collection, label) { }

        public new IList<T> List => (IList<T>)base.List;
    }
}
#endif