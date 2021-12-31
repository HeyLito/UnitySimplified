#if UNITY_EDITOR

using System.IO;
using UnityEngine;
using UnityEditor;
using UnitySimplified.Serialization;
using UnityObject = UnityEngine.Object;

namespace UnitySimplifiedEditor.Serialization
{
    public class AssetLocater : AssetPostprocessor
    {
        enum FileType { Null, Asset, Prefab }
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            HandleImportedAssets(importedAssets);
            HandleMovedAssets(movedFromAssetPaths, movedAssets);
        }

        private static void HandleImportedAssets(string[] importedAssets)
        {
            for (int i = 0; i < importedAssets.Length; i++)
                if (PathIsUsableAsset(importedAssets[i], out _, out FileType fileType, out UnityObject asset) && fileType == FileType.Prefab)
                    if (!PrefabStorage.Instance.TryGetPrefabKey(asset as GameObject, out _))
                    {
                        PrefabStorage.Instance.KeyedPrefabs.Add(PrefabStorage.FormatGameObjectToKey(asset as GameObject, PrefabStorage.Instance.KeyedPrefabs.Count - 1), asset as GameObject);
                        EditorUtility.SetDirty(PrefabStorage.Instance);
                    }
        }
        private static void HandleMovedAssets(string[] movedFromAssetPaths, string[] movedAssets)
        {
            for (int i = 0; i < movedAssets.Length; i++)
            {
                string directoryOfMovedFromAssetPaths = movedFromAssetPaths[i].Substring(0, movedFromAssetPaths[i].LastIndexOf(Path.AltDirectorySeparatorChar));
                string directoryOfMovedAsset = movedAssets[i].Substring(0, movedAssets[i].LastIndexOf(Path.AltDirectorySeparatorChar));
                if (directoryOfMovedFromAssetPaths != directoryOfMovedAsset)
                    continue;

                if (PathIsAsset(movedFromAssetPaths[i], out string oldAssetName) &&
                    PathIsUsableAsset(movedAssets[i], out string newAssetName, out FileType fileType, out UnityObject asset))
                {
                    string oldKey = null;
                    switch (fileType)
                    {
                        case FileType.Asset:
                            oldKey = AssetStorage.FormatAssetObjectToKey(asset, oldAssetName);
                            if (AssetStorage.Instance.KeyedAssets.ContainsKey(oldKey))
                            {
                                string newKey = AssetStorage.FormatAssetObjectToKey(asset, newAssetName);
                                AssetStorage.Instance.KeyedAssets.Remove(oldKey);
                                if (AssetStorage.Instance.KeyedAssets.ContainsKey(newKey))
                                    AssetStorage.Instance.KeyedAssets[newKey] = asset;
                                else AssetStorage.Instance.KeyedAssets.Add(newKey, asset);
                                AssetStorage.Instance.AttemptToSave();
                                Debug.Log($"Changed from: <color=orange>{oldAssetName}</color> To: <color=lime>{newAssetName}</color>");
                            }
                            break;

                        case FileType.Prefab:
                            int index = 0;
                            foreach (var pair in PrefabStorage.Instance.KeyedPrefabs)
                                if (pair.Value == asset)
                                {
                                    oldKey = pair.Key;
                                    break;
                                }
                                else index++;
                            if (!string.IsNullOrEmpty(oldKey))
                            {
                                string newKey = PrefabStorage.FormatGameObjectToKey(asset as GameObject, index);
                                PrefabStorage.Instance.KeyedPrefabs.Remove(oldKey);
                                PrefabStorage.Instance.KeyedPrefabs.Add(newKey, asset as GameObject);
                                Debug.Log($"Changed from: <color=orange>{oldAssetName}</color> To: <color=lime>{newAssetName}</color>");
                            }
                            break;
                    }
                }
            }
        }

        private static bool PathIsAsset(string path, out string fileName)
        {
            string[] pathSplit = path.Split('/', '.');
            fileName = pathSplit.Length > 2 ? pathSplit[pathSplit.Length - 2] : "";

            if (string.IsNullOrEmpty(fileName))
                return false;

            return true;
        }
        private static bool PathIsUsableAsset(string path, out string fileName, out FileType fileType, out UnityObject asset)
        {
            fileType = FileType.Null;
            asset = null;

            string[] pathSplit = path.Split('/', '.');
            fileName = pathSplit.Length > 2 ? pathSplit[pathSplit.Length - 2] : "";

            if (string.IsNullOrEmpty(fileName))
                return false;

            asset = AssetDatabase.LoadAssetAtPath(path, typeof(UnityObject));
            if (asset)
            {
                if (asset is GameObject)
                    fileType = FileType.Prefab;
                else
                    fileType = FileType.Asset;
                return true;
            }
            else return false;
        }
    }
}

#endif