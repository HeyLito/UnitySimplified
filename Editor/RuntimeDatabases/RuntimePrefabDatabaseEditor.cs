#if UNITY_EDITOR

using System.Linq;
using UnityEngine;
using UnityEditor;
using UnitySimplified.RuntimeDatabases;

namespace UnitySimplifiedEditor.RuntimeDatabases
{
    [CustomEditor(typeof(RuntimePrefabDatabase))]
    public class RuntimePrefabDatabaseEditor : Editor
    {
        private class EntryReorderableList : RuntimeDatabaseEditorUtility.EntryReorderableList<GameObject>
        {
            private readonly RuntimePrefabDatabase _runtimeDatabase;

            public EntryReorderableList(RuntimePrefabDatabase runtimeDatabase, string label) : base(runtimeDatabase, label) => _runtimeDatabase = runtimeDatabase;

            protected override void OnClear()
            {
                Undo.RecordObject(_runtimeDatabase, "Clear");
                _runtimeDatabase.Clear();
            }
            protected override void OnRepopulate()
            {
                Undo.RecordObject(_runtimeDatabase, "Repopulate");
                foreach (var entry in _runtimeDatabase.Where(x => x.Equals(default(IRuntimeValueDatabase<GameObject>.Entry)) || string.IsNullOrWhiteSpace(x.Identifier) || x.Value == null).ToArray())
                    _runtimeDatabase.TryRemove(entry.Identifier);
                foreach (var asset in RuntimeDatabaseUtility.GetAssetsInDirectories(".prefab"))
                    _runtimeDatabase.TryAdd((GameObject)asset);
            }
            protected override void OnRemoveIdentifier(string identifier) => _runtimeDatabase.TryRemove(identifier);
        }

        private EntryReorderableList _list;

        protected void OnEnable() => _list = new EntryReorderableList((RuntimePrefabDatabase)target, "Entries");
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            _list.LayoutList();
            if (!EditorGUI.EndChangeCheck())
                return;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
        }
    }
}

#endif