using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
        [NonSerialized] private static T _instance = null;
        
        /// <summary>
        /// Determine how to save the container's data. 
        /// By default, the asset will be serialized as a ScriptableObject and stored within the Resources folder. 
        /// Selecting this value as "true" will attempt to save the container a JSON file using custom data serialization (EasyVN.Serialization.DataManager).
        /// </summary>
        protected virtual bool SaveAsFile { get; } = false;

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

            if (storageWasInstanced && storage)
            {
                #if UNITY_EDITOR
                if (!storage.SaveAsFile)
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
                    AssetDatabase.SaveAssets();
                }
                #endif
                if (storage.SaveAsFile)
                {
                    DataManager.LoadFileDatabase(FileFormat.JSON);
                    if (!DataManager.LoadFromFile(storage.GetType().Name, storage))
                        DataManager.CreateNewFile(storage.GetType().Name, storage, FileFormat.JSON);

                    Application.quitting += storage.AttemptToSave;
                }
            }
            return storage;
        }
        public void AttemptToSave()
        {
            if (SaveAsFile)
            {
                #if UNITY_EDITOR
                if (EditorApplication.isCompiling)
                    return;
                #endif

                DataManager.LoadFileDatabase(FileFormat.JSON);
                DataManager.SaveToFile(GetType().Name, Instance);
            }
        }
    }
}