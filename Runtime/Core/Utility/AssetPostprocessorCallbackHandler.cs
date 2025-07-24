#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;

namespace UnitySimplifiedEditor
{
    public class AssetPostprocessorCallbackHandler
    {
        private class PostProcessor : AssetPostprocessor
        {
            private static readonly HashSet<PostprocessorCallback> Callbacks = new();

            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                Task.Run(async () =>
                {
                    while (EditorApplication.isCompiling)
                        await Task.Delay(5);

                    foreach (var callback in Callbacks.ToArray())
                        callback(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
                    Callbacks.Clear();
                });
            }
            public void Add(PostprocessorCallback callback) => Callbacks.Add(callback);
        }
        private class ModificationProcessor : AssetModificationProcessor
        {
            private static readonly HashSet<PostprocessorCallback> Callbacks = new();
            private static readonly string[] EmptyArray = Array.Empty<string>();
            private static readonly string[] SingleArray = new string[1];

            private static void OnWillCreateAsset(string assetName)
            {
                if (assetName.EndsWith(".meta"))
                    return;
                SingleArray[0] = assetName;

                foreach (var callback in Callbacks.ToArray())
                    callback(SingleArray, EmptyArray, EmptyArray, EmptyArray);
                Callbacks.Clear();
            }

            public void Add(PostprocessorCallback callback) => Callbacks.Add(callback);
        }

        public delegate void PostprocessorCallback(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths);

        private static readonly PostProcessor PostProcessorInstance = new();
        private static readonly ModificationProcessor ModificationProcessorInstance = new();

        public static void AddCallback(PostprocessorCallback callback)
        {
            PostProcessorInstance.Add(callback);
            ModificationProcessorInstance.Add(callback);
        }
    }
}

#endif