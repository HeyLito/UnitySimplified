#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified.Serialization;

namespace UnitySimplifiedEditor.Serialization
{
    [CustomEditor(typeof(AssetStorage))]
    public class AssetStorageEditor : StorageEditor<AssetStorage>
    {
        public override void OnInspectorGUI()
        {
            if (Target.KeyedAssets.Count > 0) 
                DisplayUnityObjects(new List<string>(Target.KeyedAssets.Keys), new List<Object>(Target.KeyedAssets.Values));
        }
    }
}

#endif