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
            {
                if (PathIsUsableAsset(importedAssets[i], out string fileName, out string fileExtension, out FileType fileType, out UnityObject asset))
                {
                    Debug.Log($"{fileName}, {fileExtension}");
                    AssetStorage.Instance.InsertAssetEntry(new AssetInfo(asset));
                    //PrefabStorage.Instance.InsertPrefabEntry(PrefabStorage.FormatGameObjectToKey(asset as GameObject, PrefabStorage.Instance.KeyedPrefabs.Count - 1), asset as GameObject);
                }
                //if (PathIsUsableAsset(importedAssets[i], out _, out FileType fileType, out UnityObject asset) && fileType == FileType.Prefab)
                //    if (!PrefabStorage.Instance.ContainsPrefab(asset as GameObject))
                //    {
                //        PrefabStorage.Instance.InsertPrefabEntry(asset as GameObject);
                //        EditorUtility.SetDirty(PrefabStorage.Instance);
                //    }
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
                    PathIsUsableAsset(movedAssets[i], out string newAssetName, out _, out FileType fileType, out UnityObject asset))
                {
                    string oldKey = null;

                    switch (fileType)
                    {
                        case FileType.Asset:
                            //oldKey = AssetStorage.FormatAssetObjectToKey(asset, oldAssetName);
                            //if (AssetStorage.Instance.KeyedAssets.ContainsKey(oldKey))
                            //{
                            //    string newKey = AssetStorage.FormatAssetObjectToKey(asset, newAssetName);
                            //    AssetStorage.Instance.KeyedAssets.Remove(oldKey);
                            //    if (AssetStorage.Instance.KeyedAssets.ContainsKey(newKey))
                            //        AssetStorage.Instance.KeyedAssets[newKey] = asset;
                            //    else AssetStorage.Instance.KeyedAssets.Add(newKey, asset);
                            //    AssetStorage.Instance.AttemptToSave();
                            //    Debug.Log($"Changed from: <color=orange>{oldAssetName}</color> To: <color=lime>{newAssetName}</color>");
                            //}
                            break;

                        case FileType.Prefab:
                            //int index = 0;
                            //foreach (var pair in PrefabStorage.Instance.KeyedPrefabs)
                            //    if (pair.Value == asset)
                            //    {
                            //        oldKey = pair.Key;
                            //        break;
                            //    }
                            //    else index++;
                            //if (!string.IsNullOrEmpty(oldKey))
                            //{
                            //    string newKey = PrefabStorage.FormatGameObjectToKey(asset as GameObject, index);
                            //    PrefabStorage.Instance.KeyedPrefabs.Remove(oldKey);
                            //    PrefabStorage.Instance.KeyedPrefabs.Add(newKey, asset as GameObject);
                            //    Debug.Log($"Changed from: <color=orange>{oldAssetName}</color> To: <color=lime>{newAssetName}</color>");
                            //}
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
        private static bool PathIsUsableAsset(string path, out string fileName, out string fileExtension, out FileType fileType, out UnityObject asset)
        {
            fileType = FileType.Null;
            asset = null;

            string[] pathSplit = path.Split('/', '.');
            fileName = pathSplit.Length > 2 ? pathSplit[pathSplit.Length - 2] : "";
            fileExtension = pathSplit.Length > 2 ? pathSplit[pathSplit.Length - 1] : "";

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

        public static List<AssetInfo> GetAssets(bool loadDefaultAssets, params string[] extensions)
        {
            var assetsFromDirectories = GetFiles("Assets", extensions);
            var results = new List<AssetInfo>();

            if (loadDefaultAssets)
            {
                bool defaultMeshes = false, defaultMaterials = false, defaultImages = false;
                foreach (var extension in extensions)
                {
                    switch (extension)
                    {
                        case ".fbx":
                            defaultMeshes = true;
                            break;
                        case ".mat":
                            defaultMaterials = true;
                            break;
                        case ".png":
                            defaultImages = true;
                            break;
                    }
                }

                if (defaultMeshes)
                {
                    UnityObject resource;

                    if (resource = Resources.GetBuiltinResource<Mesh>("Cube.fbx"))
                        results.Add(new AssetInfo(resource, "Meshes", "Cube"));
                    
                    if (resource = Resources.GetBuiltinResource<Mesh>("Capsule.fbx"))
                        results.Add(new AssetInfo(resource, "Meshes", "Capsule"));
                    
                    if (resource = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx"))
                        results.Add(new AssetInfo(resource, "Meshes", "Cylinder"));
                    
                    if (resource = Resources.GetBuiltinResource<Mesh>("Plane.fbx"))
                        results.Add(new AssetInfo(resource, "Meshes", "Plane"));
                    
                    if (resource = Resources.GetBuiltinResource<Mesh>("Sphere.fbx"))
                        results.Add(new AssetInfo(resource, "Meshes", "Sphere"));

                    if (resource = Resources.GetBuiltinResource<Mesh>("Quad.fbx"))
                        results.Add(new AssetInfo(resource, "Meshes", "Quad"));
                }
                if (defaultMaterials)
                {
                    UnityObject resource;

                    // --- Missing from function getter? ---
                    //if (resource = AssetDatabase.GetBuiltinExtraResource<Material>("FrameDebuggerRenderTargetDisplay.mat"))
                    //    results.Add(new AssetInfo(resource, "Materials", "FrameDebuggerRenderTargetDisplay"));

                    if (resource = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat"))
                        results.Add(new AssetInfo(resource, "Materials", "Default-Diffuse"));

                    if (resource = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat"))
                        results.Add(new AssetInfo(resource, "Materials", "Default-Line"));

                    if (resource = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat"))
                        results.Add(new AssetInfo(resource, "Materials", "Default-Material"));

                    if (resource = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat"))
                        results.Add(new AssetInfo(resource, "Materials", "Default-Particle"));

                    if (resource = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat"))
                        results.Add(new AssetInfo(resource, "Materials", "Default-ParticleSystem"));

                    if (resource = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat"))
                        results.Add(new AssetInfo(resource, "Materials", "Default-Skybox"));

                    if (resource = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Terrain-Diffuse.mat"))
                        results.Add(new AssetInfo(resource, "Materials", "Default-Terrain-Diffuse"));

                    if (resource = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Terrain-Specular.mat"))
                        results.Add(new AssetInfo(resource, "Materials", "Default-Terrain-Specular"));

                    if (resource = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Terrain-Standard.mat"))
                        results.Add(new AssetInfo(resource, "Materials", "Default-Terrain-Standard"));

                    if (resource = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat"))
                        results.Add(new AssetInfo(resource, "Materials", "Sprites-Default"));

                    if (resource = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Mask.mat"))
                        results.Add(new AssetInfo(resource, "Materials", "Default-Mask"));

                    // --- Missing from function getter? ---
                    //if (resource = AssetDatabase.GetBuiltinExtraResource<Material>("SpatialMappingOcclusion.mat"))
                    //    results.Add(new AssetInfo(resource, "Materials", "SpatialMappingOcclusion"));

                    // --- Missing from function getter? ---
                    //if (resource = AssetDatabase.GetBuiltinExtraResource<Material>("SpatialMappingWireframe.mat"))
                    //    results.Add(new AssetInfo(resource, "Materials", "SpatialMappingWireframe"));
                }
            }

            foreach (var path in assetsFromDirectories)
            {
                UnityObject asset = AssetDatabase.LoadAssetAtPath<UnityObject>(path);
                if (asset)
                    results.Add(new AssetInfo(asset));
            }

            return results;
            //EditorUtility.SetDirty(Target);
        }

        public static string[] GetFiles(string path, params string[] extensions)
        {
            return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                            .Where(file => extensions.Any(extension => extension.StartsWith(".") && file.ToLower().EndsWith(extension)))
                            .ToArray();
        }
    }
}

#endif