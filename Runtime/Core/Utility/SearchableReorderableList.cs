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
        private FieldInfo _propertyCacheField;
        private readonly Dictionary<int, bool> _canDisplay = new();

        protected SearchableReorderableList(Type elementType, IList list, string label)
        {
            List = list;
            _reorderableList = CreateList(elementType, label);
        }

        protected IList List { get; }
        public bool MultiSelect { get => _reorderableList.multiSelect; set => _reorderableList.multiSelect = value; }

        public void LayoutList() => _reorderableList.DoLayoutList();
        protected virtual string OnGetSearchValue(ReorderableList list, int index) => list.list[index].ToString();
        protected virtual void OnDrawElement(ReorderableList list, Rect rect, int index, bool active, bool focused) => EditorGUI.LabelField(rect, list.list[index].ToString());
        protected virtual void OnContextMenu(OptionAddItemCallbackDelegate optionAddItem, OptionAddItemDisabledCallbackDelegate optionAddItemDisabled, OptionAddSeparatorCallbackDelegate optionAddSeparator) { }
        protected virtual void OnContextMenuElement(ReorderableList list, int[] indices, OptionAddItemCallbackDelegate optionAddItem, OptionAddItemDisabledCallbackDelegate optionAddItemDisabled, OptionAddSeparatorCallbackDelegate optionAddSeparator)
        {
            optionAddItem(new GUIContent("Remove"), false, () =>
            {
                foreach (var index in indices)
                    list.list.RemoveAt(index);
            });
        }
        protected virtual void OnRemove(ReorderableList list)
        {
            for (int i = list.selectedIndices.Count - 1; i >= 0; i--)
                list.list.RemoveAt(list.selectedIndices[i]);
        }
        protected ReorderableList CreateList(Type elementType, string label)
        {
            ReorderableList list = null;

            GUIContent labelContent = new GUIContent(label);
            GUIContent optionsButtonContent = EditorGUIUtility.IconContent("_Popup@2x");

            Action contextMenuElement = null;

            string searchFieldText = "";

            // ReSharper disable AccessToModifiedClosure
            // ReSharper disable PossibleNullReferenceException
            list = new ReorderableList(List, elementType)
            {
                drawHeaderCallback = x =>
                {
                    _labelStyle ??= EditorStyles.label;
                    float totalWidth = x.width;
                    float labelWidth = _labelStyle.CalcSize(labelContent).x + 8;
                    float optionsButtonWidth = 24;

                    Rect previousRect = x;
                    Rect labelRect = previousRect = new Rect(previousRect) { x = previousRect.x, width = labelWidth };
                    totalWidth -= previousRect.width;
                    Rect optionsButtonRect = previousRect = new Rect(x)
                        { x = x.x + x.width - optionsButtonWidth, width = optionsButtonWidth };
                    totalWidth -= previousRect.width;
                    float searchFieldWidth = Mathf.Clamp(totalWidth, 16, 120 + x.width * 0.16f);
                    Rect searchFieldRect = new Rect(x)
                        { x = previousRect.x - searchFieldWidth, width = searchFieldWidth };
                    searchFieldRect.yMin += 1;
                    searchFieldRect.xMax -= 1;

                    EditorGUI.LabelField(labelRect, labelContent);
                    DrawMenuButton(list, optionsButtonRect, optionsButtonContent);
                    searchFieldText = DrawSearchBar(list, searchFieldRect, searchFieldText);
                },
                elementHeightCallback = x =>
                {
                    if (!_searchEnabled)
                        return EditorGUIUtility.singleLineHeight;

                    bool canDisplay = _canDisplay[x] = _searchRegex.IsMatch(OnGetSearchValue(list, x));
                    if (canDisplay)
                        return EditorGUIUtility.singleLineHeight;

                    return 0;
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    if ((!_searchEnabled || !_canDisplay[index]) && _searchEnabled)
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
                        contextMenuElement = () =>
                        {
                            genericMenu.DropDown(rect);
                            contextMenuElement = null;
                        };
                    }

                    if (Event.current.type == EventType.Repaint)
                        contextMenuElement?.Invoke();
                },
                onRemoveCallback = OnRemove,
                multiSelect = true
            };
            return list;
            // ReSharper restore PossibleNullReferenceException
            // ReSharper restore AccessToModifiedClosure
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
            if (newSearchText.Equals(text) == false)
            {
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
            }
            // ReSharper restore PossibleNullReferenceException
            return text;
        }
    }
    public class SearchableReorderableList<T> : SearchableReorderableList
    {
        public SearchableReorderableList(IList<T> collection, string label) : base(typeof(T), (IList)collection, label) { }

        public new IList<T> List => (IList<T>)base.List;
    }
}
#endif