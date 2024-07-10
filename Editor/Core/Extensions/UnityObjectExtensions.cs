#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnitySimplifiedEditor
{
    public static class UnityObjectExtensions
    {
        private static readonly Dictionary<string, SerializedProperty> TempPropsByPaths = new();

        public static bool IsInPrefabEditMode(this Object obj) => PrefabStageUtility.GetCurrentPrefabStage() != null || PrefabUtility.IsPartOfPrefabAsset(obj);
        public static void ApplyPropertyOverridesToPrefab(this Object obj, SerializedProperty[] properties) 
        {
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
            GameObject prefab = !string.IsNullOrEmpty(prefabPath) ? (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) : null;

            if (!prefab)
                return;

            TempPropsByPaths.Clear();
            foreach (var property in properties)
                TempPropsByPaths.Add(property.propertyPath, property);

            foreach (var propertyChange in PrefabUtility.GetPropertyModifications(obj))
            {
                if (TempPropsByPaths.Count == 0)
                    break;
                if (!propertyChange.target)
                    continue;

                if (TempPropsByPaths.TryGetValue(propertyChange.propertyPath, out var cachedProperty))
                    if (propertyChange.target.GetType() == cachedProperty.serializedObject.targetObject.GetType())
                    {
                        PrefabUtility.ApplyPropertyOverride(cachedProperty, prefabPath, InteractionMode.AutomatedAction);
                        TempPropsByPaths.Remove(propertyChange.propertyPath);
                    }
            }
        }
    }
}

#endif