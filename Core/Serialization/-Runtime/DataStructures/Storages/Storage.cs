﻿using UnityEditor;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    /// <summary>
    ///     Storage is generic data-container for holding editor and runtime data.
    /// </summary>
    /// <remarks>
    /// <example> 
    ///     Upon inheriting from this class, the container is retrieved as if it were a singleton. e.g.
    ///     <code>
    ///         GameManagerStorage.Instance
    ///     </code>
    /// </example>
    /// </remarks>
    public abstract class Storage<T> : ScriptableObject where T : Storage<T>
    {
        /// <summary>
        /// Determine how to save the container's data. 
        /// By default, the asset will be serialized as a ScriptableObject and stored within the Resources folder. 
        /// Selecting this value as "true" will attempt to save the container a JSON file using custom data serialization (EasyVN.Serialization.DataManager).
        /// </summary>
        protected virtual bool SaveAsFile { get; } = false;

        private static T _instance = null;
        /// <summary>
        /// Returns the container of Storage generic T type.
        /// </summary>
        public static T Instance => _instance == null ? _instance = GetStorage() : _instance;



        private static T GetStorage()
        {
            T storage = Resources.Load<T>(typeof(T).Name);
            bool storageWasInstanced = false;

            if (!storage) 
            {
                storage = CreateInstance<T>();
                storageWasInstanced = true;
            }

            #if UNITY_EDITOR
            if (storageWasInstanced && !storage.SaveAsFile)
            {
                string[] resourcePaths = System.IO.Directory.GetDirectories("Assets", "Resources", System.IO.SearchOption.AllDirectories);
                string resourcePath;
                if (resourcePaths.Length > 0)
                    resourcePath = resourcePaths[0];
                else
                {
                    resourcePath = System.IO.Path.Combine("Assets", "Resources");
                    System.IO.Directory.CreateDirectory(resourcePath);
                }

                string path = $"{System.IO.Path.Combine(resourcePath, typeof(T).Name)}.asset";
                AssetDatabase.CreateAsset(storage, path);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();

                //string name = "Standard Graph.asset";
                //string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
                //string assetPath;
                //if (folderPath.Contains("."))
                //    folderPath = folderPath.Remove(folderPath.LastIndexOf('/'));
                //assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folderPath, name));

                //ProjectWindowUtil.CreateAsset(storage, assetPath);
            }
            #endif
            if (storage && storage.SaveAsFile)
            {
                DataManager.LoadFileDatabase(FileFormat.JSON);
                if (!DataManager.LoadFromFile(storage.GetType().Name, storage)) 
                    DataManager.CreateNewFile(storage.GetType().Name, storage, FileFormat.JSON);

                Application.quitting += storage.AttemptToSave;
            }
            return storage;
        }
        public void AttemptToSave()
        {
            if (SaveAsFile) 
            {
                DataManager.LoadFileDatabase(FileFormat.JSON);
                DataManager.SaveToFile(GetType().Name, this);
            }
        }
    }
}