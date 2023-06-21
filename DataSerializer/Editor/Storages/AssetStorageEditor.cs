#if UNITY_EDITOR

using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified.Serialization;

using UnityObject = UnityEngine.Object;

namespace UnitySimplifiedEditor.Serialization
{
    [CustomEditor(typeof(AssetStorage))]
    public class AssetStorageEditor : StorageEditor<AssetStorage>
    {
        private SerializedProperty _assetExtensions;

        protected override void OnEnable()
        {
            base.OnEnable();
            _assetExtensions = serializedObject.FindProperty("assetExtensions");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_assetExtensions);
            //if (Target.KeyedAssets.Count > 0) 
            //    DisplayUnityObjects(new List<string>(Target.KeyedAssets.Keys), new List<Object>(Target.KeyedAssets.Values));

            if (GUILayout.Button("Clear"))
            {
                Target.Clear();
                Undo.RecordObject(Target, "Clear");
                EditorUtility.SetDirty(Target);
            }
            if (GUILayout.Button("Repopulate"))
            {
                Repopulate();
                Undo.RecordObject(Target, "Repopulate");
            }
            //if (GUILayout.Button("Filter Nulls"))
            //    FilterNulls(Target.KeyedPrefabs);
            //if (Target.KeyedPrefabs.Count > 0)
            //    DisplayUnityObjects(new List<string>(Target.KeyedPrefabs.Keys), new List<GameObject>(Target.KeyedPrefabs.Values));
            if (Target.Values.Count > 0)
            {
                List<string> keys = new List<string>();
                List<UnityObject> values = new List<UnityObject>();
                foreach (var entry in Target.Values)
                {
                    keys.Add(entry.Key);
                    values.Add(entry.Value.Asset);
                }
                DisplayUnityObjects(keys, values);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void Repopulate()
        {
            var assetsFromDirectories = AssetLocater.GetAssets(true, Target.AssetExtensions);
            var assetsFromStorage = Target.Values;

            foreach (var entryPair in assetsFromStorage)
            {
                if (entryPair.Value.Asset == null)
                {
                    Debug.Log($"{entryPair.Key}, {entryPair.Value.Asset}");
                    Target.RemoveAssetEntry(entryPair.Key);
                }
            }

            foreach (var assetInfo in assetsFromDirectories)
            {
                if (assetInfo.Asset)
                    Target.InsertAssetEntry(assetInfo);
            }
            //foreach (var assetInfo in assetInfos)
            //    Target.InsertAssetEntry(assetInfo);
        }

        private string[] GetFiles(string path, params string[] extensions)
        {
            return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                            .Where(file => extensions.Any(extension => extension.StartsWith(".") && file.ToLower().EndsWith(extension)))
                            .ToArray();
        }
    }
}

#endif