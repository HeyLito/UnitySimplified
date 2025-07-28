#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace UnitySimplifiedEditor.RuntimeDatabases
{
    public static class RuntimeDatabaseUtility
    {
        private static readonly string[] BuiltinMeshNames =
        {
            "Cube.fbx",
            "Capsule.fbx",
            "Cylinder.fbx",
            "Plane.fbx",
            "Sphere.fbx",
            "Quad.fbx",
        };
        private static readonly string[] BuiltinMaterialNames =
        {
            // --- Missing from function getter? ---
            // "FrameDebuggerRenderTargetDisplay.mat"
            // "SpatialMappingOcclusion.mat"w
            // "SpatialMappingWireframe.mat"
            // -------------------------------------
            "Default-Diffuse.mat",
            "Default-Line.mat",
            "Default-Material.mat",
            "Default-Particle.mat",
            "Default-ParticleSystem.mat",
            "Default-Skybox.mat",
            "Default-Terrain-Diffuse.mat",
            "Default-Terrain-Specular.mat",
            "Default-Terrain-Standard.mat",
            "Sprites-Default.mat",
            "Sprites-Mask.mat",
        };
        private static readonly string[] BuiltinImageNames =
        {
            "UI/Skin/UISprite.psd",
            "UI/Skin/Background.psd",
            "UI/Skin/InputFieldBackground.psd",
            "UI/Skin/Knob.psd",
            "UI/Skin/Checkmark.psd",
            "UI/Skin/DropdownArrow.psd",
            "UI/Skin/UIMask.psd",
        };

        public static IEnumerable<UnityObject> GetBuiltInAssets()
        {
            foreach (var assetName in BuiltinMeshNames)
            {
                var asset = Resources.GetBuiltinResource<Mesh>(assetName);
                    yield return asset;
            }
            foreach (var assetName in BuiltinMaterialNames)
            {
                var asset = AssetDatabase.GetBuiltinExtraResource<Material>(assetName);
                if (asset != null)
                    yield return asset;
            }
            foreach (var assetName in BuiltinImageNames)
            {
                var asset = AssetDatabase.GetBuiltinExtraResource<Sprite>(assetName);
                if (asset != null)
                    yield return asset;
            }
        }
        public static IEnumerable<UnityObject> GetAssetsInDirectories(params string[] extensions)
        {
            return Directory.EnumerateFiles("Assets", "*", SearchOption.AllDirectories)
                            .Where(file => extensions.Any(extension => extension.StartsWith(".") && file.ToLower().EndsWith(extension)))
                            .Select(AssetDatabase.LoadAssetAtPath<UnityObject>)
                            .Where(asset => asset != null && asset.hideFlags != HideFlags.DontSave);
        }
    }
}

#endif