#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace UnitySimplifiedEditor
{
    public static class UnityObjectUtility
    {
        public static bool IsObjectInPrefabEdit(UnityObject obj)
        {   return PrefabStageUtility.GetCurrentPrefabStage() != null || PrefabUtility.IsPartOfPrefabAsset(obj);   }

        public static void ApplyPropertyOverridesToPrefab(UnityObject prefabInstance, SerializedProperty[] properties) 
        {
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabInstance);
            GameObject prefab = !string.IsNullOrEmpty(prefabPath) ? (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) : null;

            if (!prefab)
                return;

            List<SerializedProperty> propList = new List<SerializedProperty>(properties);

            foreach (var propertyChange in PrefabUtility.GetPropertyModifications(prefabInstance))
            {
                for (int i = propList.Count - 1; i >= 0; i--)
                    if (propertyChange.target)
                    {
                        //Debug.Log($"{propertyChange.propertyPath} || {propList[i].propertyPath}");
                        if (propertyChange.target.GetType() == propList[i].serializedObject.targetObject.GetType() &&
                            propertyChange.propertyPath == propList[i].propertyPath) 
                        {
                            //Debug.Log($"{propertyChange.propertyPath} || {propertyChange.value}");
                            PrefabUtility.ApplyPropertyOverride(propList[i], prefabPath, InteractionMode.AutomatedAction);
                            propList.RemoveAt(i);
                        }
                    }
                if (propList.Count == 0)
                    break;
            }
        }
    }
}

#endif