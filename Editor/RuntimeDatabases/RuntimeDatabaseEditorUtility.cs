#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;
using UnitySimplified.RuntimeDatabases;
using UnityObject = UnityEngine.Object;

namespace UnitySimplifiedEditor.RuntimeDatabases
{
    public static class RuntimeDatabaseEditorUtility
    {
        private static GUIStyle _boxStyle;
        private static GUIStyle _idStyle;
        private static GUIStyle _unityObjectStyle;
        private static GUIStyle _unityObjectErrorStyle;

        public static GUIStyle BoxStyle
        {
            get
            {
                if (_boxStyle != null)
                    return _boxStyle;
                _boxStyle = new GUIStyle(GUI.skin.textArea);
                _boxStyle.fontSize = 10;
                _boxStyle.normal.textColor = new Color(1, 1f, 0.5f, 1);
                _boxStyle.onNormal.textColor = _boxStyle.normal.textColor;
                _boxStyle.focused.textColor = _boxStyle.normal.textColor;
                _boxStyle.onFocused.textColor = _boxStyle.normal.textColor;
                _boxStyle.hover.textColor = _boxStyle.normal.textColor;
                _boxStyle.onHover.textColor = _boxStyle.normal.textColor;
                _boxStyle.active.textColor = new Color(_boxStyle.normal.textColor.r - 0.2f, _boxStyle.normal.textColor.g - 0.2f, _boxStyle.normal.textColor.b - 0.2f, _boxStyle.normal.textColor.a);
                _boxStyle.onActive.textColor = _boxStyle.active.textColor;
                _boxStyle.alignment = TextAnchor.UpperLeft;
                return _boxStyle;
            }
        }
        public static GUIStyle IDStyle
        {
            get
            {
                if (_idStyle != null)
                    return _idStyle;
                _idStyle = new GUIStyle();
                _idStyle.fontSize = 10;
                _idStyle.normal.textColor = Color.white;
                _idStyle.richText = true;
                _idStyle.alignment = TextAnchor.MiddleLeft;
                _idStyle.wordWrap = true;
                return _idStyle;
            }
        }
        public static GUIStyle UnityObjectStyle
        {
            get
            {
                if (_unityObjectStyle != null)
                    return _unityObjectStyle;
                _unityObjectStyle = new GUIStyle();
                _unityObjectStyle.fontSize = 10;
                _unityObjectStyle.normal.textColor = new Color(0.25f, 0.65f, 1f, 1);
                _unityObjectStyle.richText = true;
                _unityObjectStyle.alignment = TextAnchor.MiddleLeft;
                _unityObjectStyle.wordWrap = true;
                return _unityObjectStyle;
            }
        }
        public static GUIStyle UnityObjectErrorStyle
        {
            get
            {
                if (_unityObjectErrorStyle != null)
                    return _unityObjectErrorStyle;
                _unityObjectErrorStyle = new GUIStyle();
                _unityObjectErrorStyle.fontSize = 10;
                _unityObjectErrorStyle.normal.textColor = new Color(1, 0.65f, 0.25f, 1);
                _unityObjectErrorStyle.richText = true;
                _unityObjectErrorStyle.alignment = TextAnchor.MiddleLeft;
                return _unityObjectErrorStyle;
            }
        }

        public abstract class EntryReorderableList<T> : SearchableReorderableList<IRuntimeValueDatabase<T>.Entry> where T : UnityObject
        {
            private readonly IRuntimeValueDatabase<T> _runtimeValueDatabase;
            private bool _showIdentifiers;

            protected EntryReorderableList(IRuntimeValueDatabase<T> runtimeValueDatabase, string label) : base(runtimeValueDatabase.Entries, label) => _runtimeValueDatabase = runtimeValueDatabase;

            protected override void OnDrawElement(ReorderableList list, Rect rect, int index, bool active, bool focused)
            {
                IRuntimeValueDatabase<T>.Entry entry = List[index];

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 2 && rect.Contains(Event.current.mousePosition))
                {
                    Undo.RecordObject(Selection.activeObject, "Select Object");
                    Selection.activeObject = entry.Value;
                }

                if (_showIdentifiers)
                {
                    Rect idRect = new(rect) { x = rect.x + 6, width = EditorGUIUtility.labelWidth + 30f };
                    Rect assetRect = new(rect) { xMin = idRect.x + idRect.width + 6 };

                    EditorGUI.LabelField(idRect, $"ID:<i>{entry.Identifier}</i>", IDStyle);
                    if (entry.Value != null)
                        EditorGUI.LabelField(assetRect, $"<b>{entry.Value.name}</b>", UnityObjectStyle);
                    else EditorGUI.LabelField(assetRect, "NULL", UnityObjectErrorStyle);
                }
                else
                {
                    Rect labelRect = new(rect) { xMin = rect.x + 6 };

                    if (entry.Value != null)
                        EditorGUI.LabelField(labelRect, $"<b>{entry.Value.name}</b>", UnityObjectStyle);
                    else EditorGUI.LabelField(labelRect, "NULL", UnityObjectErrorStyle);
                }
            }
            protected override string OnGetSearchValue(ReorderableList list, int index)
            {
                var entry = _runtimeValueDatabase.Entries[index];
                return $"ID:{entry.Identifier}, PREFAB:{entry.Value}";
            }
            protected override void OnContextMenu(OptionAddItemCallbackDelegate optionAddItem, OptionAddItemDisabledCallbackDelegate optionAddItemDisabled, OptionAddSeparatorCallbackDelegate optionAddSeparator)
            {
                optionAddItem(new GUIContent("Clear"), false, OnClear);
                optionAddItem(new GUIContent("Repopulate"), false, OnRepopulate);
                optionAddItem(new GUIContent("Sort/Sort By Ascending"), false, SortByAscending);
                optionAddItem(new GUIContent("Sort/Sort By Descending"), false, SortByDescending);
                optionAddItem(new GUIContent("Show Identifiers"), _showIdentifiers, () => _showIdentifiers = !_showIdentifiers);
            }
            protected override void OnContextMenuElement(ReorderableList list, int[] indices, OptionAddItemCallbackDelegate optionAddItem, OptionAddItemDisabledCallbackDelegate optionAddItemDisabled, OptionAddSeparatorCallbackDelegate optionAddSeparator)
            {
                optionAddItem(new GUIContent("Remove"), false, () =>
                {
                    Undo.RecordObject((UnityObject)_runtimeValueDatabase, "Remove");
                    foreach (var index in indices)
                        OnRemoveIdentifier(List[index].Identifier);
                    list.ClearSelection();
                });
                switch (indices.Length)
                {
                    case 0:
                        return;
                    case 1:
                        optionAddItem(new GUIContent("Copy ID"), false, () => GUIUtility.systemCopyBuffer = List[indices[0]].Identifier);
                        break;
                    case > 1:
                        {
                            optionAddItem(new GUIContent("Copy IDs"), false, () =>
                            {
                                string value = indices.Aggregate("", (current, t) => current + List[t].Identifier + "\n");
                                value = !string.IsNullOrWhiteSpace(value) ? value[..^1] : value;
                                GUIUtility.systemCopyBuffer = value;
                            });
                            break;
                        }
                }
            }
            protected override void OnRemove(ReorderableList list)
            {
                Undo.RecordObject((UnityObject)_runtimeValueDatabase, "Remove");
                foreach (var identifier in List.Where((_, index) => list.selectedIndices.ToList().Exists(x => x == index)).Select(x => x.Identifier).ToList())
                    OnRemoveIdentifier(identifier);
                list.ClearSelection();
            }

            private void SortByAscending()
            {
                int Sort(IRuntimeValueDatabase<T>.Entry lhs, IRuntimeValueDatabase<T>.Entry rhs)
                {
                    bool hasLhs = lhs.Value != null;
                    bool hasRhs = rhs.Value != null;

                    if (!hasLhs && !hasRhs)
                        return 0;
                    if (!hasLhs)
                        return -1;
                    if (!hasRhs)
                        return 1;
                    return string.Compare(lhs.Value.name, rhs.Value.name, StringComparison.Ordinal);
                }

                Undo.RecordObject((UnityObject)_runtimeValueDatabase, "Sort");
                _runtimeValueDatabase.Entries.Sort(Comparer<IRuntimeValueDatabase<T>.Entry>.Create(Sort));
            }
            private void SortByDescending()
            {
                int Sort(IRuntimeValueDatabase<T>.Entry lhs, IRuntimeValueDatabase<T>.Entry rhs)
                {
                    bool hasLhs = lhs.Value != null;
                    bool hasRhs = rhs.Value != null;

                    if (!hasLhs && !hasRhs)
                        return 0;
                    if (!hasLhs)
                        return 1;
                    if (!hasRhs)
                        return -1;
                    return string.Compare(rhs.Value.name, lhs.Value.name, StringComparison.Ordinal);
                }

                Undo.RecordObject((UnityObject)_runtimeValueDatabase, "Sort");
                _runtimeValueDatabase.Entries.Sort(Comparer<IRuntimeValueDatabase<T>.Entry>.Create(Sort));
            }

            protected abstract void OnClear();
            protected abstract void OnRepopulate();
            protected abstract void OnRemoveIdentifier(string identifier);
        }
    }
}

#endif