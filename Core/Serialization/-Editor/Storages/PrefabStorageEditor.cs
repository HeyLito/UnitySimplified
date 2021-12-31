#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified.Serialization;

namespace UnitySimplifiedEditor.Serialization
{
    [CustomEditor(typeof(PrefabStorage))]
    public class PrefabStorageEditor : StorageEditor<PrefabStorage>
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Repopulate"))
                Repopulate(Target.KeyedPrefabs);
            if (GUILayout.Button("Filter Nulls"))
                FilterNulls(Target.KeyedPrefabs);
            if (Target.KeyedPrefabs.Count > 0)
                DisplayUnityObjects(new List<string>(Target.KeyedPrefabs.Keys), new List<GameObject>(Target.KeyedPrefabs.Values));
            serializedObject.ApplyModifiedProperties();
        }

        private void Repopulate(KeyedPrefabsDictionary keyedPrefabs)
        {
            keyedPrefabs.Clear();
            string[] prefabPaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);

            for (int i = 0; i < prefabPaths.Length; i++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[i]);
                if (prefab)
                    keyedPrefabs.Add(PrefabStorage.FormatGameObjectToKey(prefab, i), prefab);
            }
            EditorUtility.SetDirty(target);
        }
        private void FilterNulls(KeyedPrefabsDictionary keyedPrefabs)
        {
            List<string> keys = new List<string>(keyedPrefabs.Keys);
            List<GameObject> values = new List<GameObject>(keyedPrefabs.Values);

            for (int i = 0; i < keys.Count && i < values.Count; i++)
                if (values[i] == null)
                    keyedPrefabs.Remove(keys[i]);
            EditorUtility.SetDirty(target);
        }
    }
}

#endif