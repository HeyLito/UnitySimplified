#if UNITY_EDITOR

using System.Linq;
using UnityEditor;
using UnityEngine;
using UnitySimplified.RuntimeDatabases;
using UnityObject = UnityEngine.Object;

namespace UnitySimplifiedEditor.RuntimeDatabases
{
    [CustomEditor(typeof(RuntimeAssetDatabase), true)]
    public class RuntimeAssetDatabaseEditor : Editor
    {
        private class EntryReorderableList : RuntimeDatabaseEditorUtility.EntryReorderableList<UnityObject>
        {
            private readonly RuntimeAssetDatabase _runtimeDatabase;

            public EntryReorderableList(RuntimeAssetDatabase runtimeDatabase, string label) : base(runtimeDatabase, label) => _runtimeDatabase = runtimeDatabase;

            protected override void OnClear()
            {
                Undo.RecordObject(_runtimeDatabase, "Clear");
                _runtimeDatabase.Clear();
            }
            protected override void OnRepopulate()
            {
                Undo.RecordObject(_runtimeDatabase, "Repopulate");
                foreach (var entry in _runtimeDatabase.Where(x => x.Equals(default(IRuntimeValueDatabase<UnityObject>.Entry)) || string.IsNullOrWhiteSpace(x.Identifier) || x.Value == null).ToArray())
                    _runtimeDatabase.TryRemove(entry.Identifier);
                foreach (var asset in RuntimeDatabaseUtility.GetBuiltInAssets().Where(asset => asset != null))
                    _runtimeDatabase.TryAdd(asset);
                foreach (var asset in RuntimeDatabaseUtility.GetAssetsInDirectories(_runtimeDatabase.ValidatedAssetExtensions).Where(asset => asset != null))
                    _runtimeDatabase.TryAdd(asset);
            }
            protected override void OnRemoveIdentifier(string identifier) => _runtimeDatabase.TryRemove(identifier);
        }

        private EntryReorderableList _list;

        protected void OnEnable() => _list = new EntryReorderableList((RuntimeAssetDatabase)target, "Entries");
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            _list.LayoutList();
            if (!EditorGUI.EndChangeCheck())
                return;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
        }

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
    }
}

#endif