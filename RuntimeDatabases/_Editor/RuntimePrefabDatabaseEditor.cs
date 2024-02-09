#if UNITY_EDITOR

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnitySimplified.RuntimeDatabases;
using UnityObject = UnityEngine.Object;

namespace UnitySimplifiedEditor.RuntimeDatabases
{
    [CustomEditor(typeof(RuntimePrefabDatabase))]
    public class RuntimePrefabDatabaseEditor : Editor
    {
        private RuntimePrefabDatabase _runtimePrefabDatabase;
        private ReorderableList _itemsReorderableList;



        protected void OnEnable()
        {
            _runtimePrefabDatabase = target as RuntimePrefabDatabase;
            _itemsReorderableList = RuntimeDatabaseEditorUtility.ReorderableListTemplate(_runtimePrefabDatabase.Items, typeof(KeyValuePair<string, UnityObject>), "Identified Assets",
                actionOnClear: () =>
                {
                    Undo.RecordObject(_runtimePrefabDatabase, "Clear");
                    _runtimePrefabDatabase.Clear();
                    ((ISerializationCallbackReceiver)_runtimePrefabDatabase.Items)?.OnBeforeSerialize();
                    EditorUtility.SetDirty(_runtimePrefabDatabase);
                },
                actionOnRepopulate: () =>
                {
                    Undo.RecordObject(_runtimePrefabDatabase, "Repopulate");
                    Repopulate();
                    ((ISerializationCallbackReceiver)_runtimePrefabDatabase.Items)?.OnBeforeSerialize();
                    EditorUtility.SetDirty(_runtimePrefabDatabase);
                });
            _itemsReorderableList.elementHeightCallback = (index) => EditorGUIUtility.singleLineHeight + 10;
            _itemsReorderableList.drawElementCallback = (position, index, isActive, isFocused) =>
            {
                KeyValuePair<string, GameObject> entry = (KeyValuePair<string, GameObject>)_runtimePrefabDatabase.Items[index];

                Rect idRect = new(position) { x = position.x + 6, width = EditorGUIUtility.labelWidth + 30f };
                Rect assetRect = new(position) { xMin = idRect.x + idRect.width + 6 };

                if (position.Contains(Event.current.mousePosition) && Event.current.clickCount == 2)
                    Selection.activeObject = entry.Value;

                EditorGUI.LabelField(idRect, $"ID:<i>{entry.Key}</i>", RuntimeDatabaseEditorUtility.IDStyle);
                if (entry.Value != null)
                    EditorGUI.LabelField(assetRect, $"<b>{entry.Value.name}</b>", RuntimeDatabaseEditorUtility.UnityObjectStyle);
                else EditorGUI.LabelField(assetRect, "NULL", RuntimeDatabaseEditorUtility.UnityObjectErrorStyle);
            };
            _itemsReorderableList.onRemoveCallback = (list) =>
            {
                Undo.RecordObject(_runtimePrefabDatabase, "Remove");
                List<string> keysToRemove = new();
                for (int i = list.selectedIndices.Count - 1; i >= 0; i--)
                    keysToRemove.Add(((KeyValuePair<string, GameObject>)_runtimePrefabDatabase.Items[list.selectedIndices[i]]).Key);
                foreach (var key in keysToRemove)
                    _runtimePrefabDatabase.TryRemove(key);
                list.ClearSelection();
                ((ISerializationCallbackReceiver)_runtimePrefabDatabase.Items)?.OnBeforeSerialize();
                EditorUtility.SetDirty(_runtimePrefabDatabase);
            };
        }
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            _itemsReorderableList.DoLayoutList();
            if (!EditorGUI.EndChangeCheck())
                return;
            ((ISerializationCallbackReceiver)_runtimePrefabDatabase.Items)?.OnAfterDeserialize();
            serializedObject.ApplyModifiedProperties();
        }

        private void Repopulate()
        {
            var items = new List<KeyValuePair<string, GameObject>>();
            items.AddRange((IEnumerable<KeyValuePair<string, GameObject>>)_runtimePrefabDatabase.Items);
            foreach (var item in items.Where(item => item.Equals(default) || item.Value == null))
                _runtimePrefabDatabase.TryRemove(item.Key);
            foreach (var asset in RuntimeDatabaseEditorUtility.GetAssetsInDirectories(".prefab").Where(asset => asset != null))
                _runtimePrefabDatabase.TryAdd((GameObject)asset);
        }
    }
}

#endif