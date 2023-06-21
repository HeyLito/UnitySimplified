#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified.Serialization;
using System.Linq;

namespace UnitySimplifiedEditor.Serialization
{
    [CustomEditor(typeof(PrefabStorage))]
    public class PrefabStorageEditor : StorageEditor<PrefabStorage>
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Repopulate"))
                Repopulate(); //Repopulate(Target.KeyedPrefabs);
            //if (GUILayout.Button("Filter Nulls"))
            //    FilterNulls(Target.KeyedPrefabs);
            //if (Target.KeyedPrefabs.Count > 0)
            //    DisplayUnityObjects(new List<string>(Target.KeyedPrefabs.Keys), new List<GameObject>(Target.KeyedPrefabs.Values));
            if (Target.Values.Count > 0)
            {
                List<string> keys = new List<string>();
                List<GameObject> values = new List<GameObject>();
                foreach (var entry in Target.Values)
                {
                    keys.Add(entry.Key);
                    values.Add(entry.Value);
                }
                DisplayUnityObjects(keys, values);
            }
            serializedObject.ApplyModifiedProperties();
        }

        //private void Repopulate(KeyedPrefabsDictionary keyedPrefabs)
        //{
        //    keyedPrefabs.Clear();
        //    string[] prefabPaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);

        //    for (int i = 0; i < prefabPaths.Length; i++)
        //    {
        //        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[i]);
        //        if (prefab)
        //            keyedPrefabs.Add(PrefabStorage.FormatGameObjectToKey(prefab, i), prefab);
        //    }
        //    EditorUtility.SetDirty(target);
        //    Repopulate();
        //}

        private void Repopulate()
        {
            var prefabsFromDirectories = GetFiles("Assets", ".prefab");
            var prefabsFromStorage = Target.Values;

            foreach (var storedPrefab in prefabsFromStorage)
            {
                if (string.IsNullOrEmpty(storedPrefab.Key) || storedPrefab.Value == null)
                    Target.RemovePrefabEntry(storedPrefab.Value);
            }

            foreach (var path in prefabsFromDirectories)
            {
                Debug.Log(path);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab)
                        Target.InsertPrefabEntry(prefab);
            }
            //for (int i = 0; i < prefabs.Length; i++)
            //    Debug.Log(prefabs);
            EditorUtility.SetDirty(Target);
        }

        private string[] GetFiles(string path, params string[] extensions)
        {
            return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                            .Where(file => extensions.Any(extension => extension.StartsWith(".") && file.ToLower().EndsWith(extension)))
                            .ToArray();
        }

        //private void FilterNulls(KeyedPrefabsDictionary keyedPrefabs)
        //{
        //    List<string> keys = new List<string>(keyedPrefabs.Keys);
        //    List<GameObject> values = new List<GameObject>(keyedPrefabs.Values);

        //    for (int i = 0; i < keys.Count && i < values.Count; i++)
        //        if (values[i] == null)
        //            keyedPrefabs.Remove(keys[i]);
        //    EditorUtility.SetDirty(target);
        //}
    }
}

#endif