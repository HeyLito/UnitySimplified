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
    [CustomEditor(typeof(RuntimeAssetDatabase), true)]
    public class RuntimeAssetDatabaseEditor : Editor
    {
        private RuntimeAssetDatabase _runtimeAssetDatabase;
        private ReorderableList _itemsReorderableList;



        [InitializeOnLoadMethod]
        private static void AddAssetsOnImporter()
        {
            AssetPostprocessorCallbackHandler.AddCallback((importedAssets, _, _, _) =>
            {
                foreach (var importedAssetPath in importedAssets)
                {
                    var asset = AssetDatabase.LoadAssetAtPath(importedAssetPath, typeof(UnityObject));
                    if (asset == null)
                        continue;

                    if (RuntimeAssetDatabase.Instance.SupportsType(asset.GetType()))
                        if (RuntimeAssetDatabase.Instance.TryAdd(asset))
                            EditorUtility.SetDirty(RuntimeAssetDatabase.Instance);
                }
            });
        }

        protected void OnEnable()
        {
            _runtimeAssetDatabase = target as RuntimeAssetDatabase;
            if (_runtimeAssetDatabase == null || _runtimeAssetDatabase.Items == null)
                return;
            _itemsReorderableList = RuntimeDatabaseEditorUtility.ReorderableListTemplate(_runtimeAssetDatabase.Items, typeof(KeyValuePair<string, UnityObject>), "Identified Assets",
                actionOnClear: () =>
                {
                    Undo.RecordObject(_runtimeAssetDatabase, "Clear");
                    _runtimeAssetDatabase.Clear();
                    ((ISerializationCallbackReceiver)_runtimeAssetDatabase.Items)?.OnBeforeSerialize();
                    EditorUtility.SetDirty(_runtimeAssetDatabase);
                },
                actionOnRepopulate: () =>
                {
                    Undo.RecordObject(_runtimeAssetDatabase, "Repopulate");
                    Repopulate();
                    ((ISerializationCallbackReceiver)_runtimeAssetDatabase.Items)?.OnBeforeSerialize();
                    EditorUtility.SetDirty(_runtimeAssetDatabase);
                });
            _itemsReorderableList.elementHeightCallback = (_) => EditorGUIUtility.singleLineHeight + 10;
            _itemsReorderableList.drawElementCallback = (position, index, _, _) =>
            {
                KeyValuePair<string, UnityObject> entry = (KeyValuePair<string, UnityObject>)_runtimeAssetDatabase.Items[index];

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
                Undo.RecordObject(_runtimeAssetDatabase, "Remove");
                List<string> keysToRemove = new();
                for (int i = list.selectedIndices.Count - 1; i >= 0; i--)
                    keysToRemove.Add(((KeyValuePair<string, UnityObject>)_runtimeAssetDatabase.Items[list.selectedIndices[i]]).Key);
                foreach (var key in keysToRemove)
                    _runtimeAssetDatabase.TryRemove(key);
                list.ClearSelection();
                ((ISerializationCallbackReceiver)_runtimeAssetDatabase.Items)?.OnBeforeSerialize();
                EditorUtility.SetDirty(_runtimeAssetDatabase);
            };
        }
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            _itemsReorderableList?.DoLayoutList();
            if (!EditorGUI.EndChangeCheck())
                return;
            ((ISerializationCallbackReceiver)_runtimeAssetDatabase.Items)?.OnAfterDeserialize();
            serializedObject.ApplyModifiedProperties();
        }

        private void Repopulate()
        {
            var items = new List<KeyValuePair<string, UnityObject>>();
            items.AddRange((IEnumerable<KeyValuePair<string, UnityObject>>)_runtimeAssetDatabase.Items);
            foreach (var item in items.Where(item => item.Equals(default) || item.Value == null))
                _runtimeAssetDatabase.TryRemove(item.Key);
            foreach (var asset in RuntimeDatabaseUtility.GetBuiltInAssets().Where(asset => asset != null))
                _runtimeAssetDatabase.TryAdd(asset);
            foreach (var asset in RuntimeDatabaseUtility.GetAssetsInDirectories(_runtimeAssetDatabase.ValidatedAssetExtensions).Where(asset => asset != null))
                _runtimeAssetDatabase.TryAdd(asset);
        }
    }
}

#endif