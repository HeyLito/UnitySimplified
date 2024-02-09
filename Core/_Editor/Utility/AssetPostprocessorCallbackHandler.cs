#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;

namespace UnitySimplifiedEditor
{
    public class AssetPostprocessorCallbackHandler : AssetPostprocessor
    {
        public delegate void PostprocessorCallback(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths);

        private static readonly HashSet<PostprocessorCallback> Callbacks = new();



        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var callback in Callbacks)
                callback(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Callbacks.Clear();
        }
        public static void AddCallback(PostprocessorCallback callback) => Callbacks.Add(callback);
    }
}

#endif