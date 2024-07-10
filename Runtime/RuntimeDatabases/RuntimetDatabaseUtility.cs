using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnitySimplified.RuntimeDatabases
{
    public static class RuntimeDatabaseUtility
    {
        private static string[] _assetNames = Array.Empty<string>();
        public static IEnumerable<UnityObject> GetBuiltInAssets()
        {
#if UNITY_EDITOR
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
#else
            yield break;
#endif
        }
        public static IEnumerable<UnityObject> GetAssetsInDirectories(params string[] extensions)
        {
#if UNITY_EDITOR
            return Directory.EnumerateFiles("Assets", "*", SearchOption.AllDirectories)
                            .Where(file => extensions.Any(extension => extension.StartsWith(".") && file.ToLower().EndsWith(extension)))
                            .Select(AssetDatabase.LoadAssetAtPath<UnityObject>)
                            .Where(asset => asset != null);
#else
            yield break;
#endif
        }
    }
}