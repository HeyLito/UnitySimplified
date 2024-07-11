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
        private static string[] _assetNames = Array.Empty<string>();
        public static IEnumerable<UnityObject> GetBuiltInAssets()
        {
            _assetNames = new[]
            {
                "Cube.fbx",
                "Capsule.fbx",
                "Cylinder.fbx",
                "Plane.fbx",
                "Sphere.fbx",
                "Quad.fbx",
            };
            foreach (var assetName in _assetNames)
            {
                var asset = Resources.GetBuiltinResource<Mesh>(assetName);
                if (asset != null)
                    yield return asset;
            }

            _assetNames = new[]
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
            foreach (var assetName in _assetNames)
            {
                var asset = AssetDatabase.GetBuiltinExtraResource<Material>(assetName);
                if (asset != null)
                    yield return asset;
            }

            _assetNames = new[]
            {
                "UI/Skin/UISprite.psd",
                "UI/Skin/Background.psd",
                "UI/Skin/InputFieldBackground.psd",
                "UI/Skin/Knob.psd",
                "UI/Skin/Checkmark.psd",
                "UI/Skin/DropdownArrow.psd",
                "UI/Skin/UIMask.psd",
            };
            foreach (var assetName in _assetNames)
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
                            .Where(asset => asset != null);
        }
    }
}

#endif