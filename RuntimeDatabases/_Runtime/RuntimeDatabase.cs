using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnitySimplified.RuntimeDatabases
{
    public abstract class RuntimeDatabase : ScriptableObject
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InitializeDatabase()
        {
            string keyName = $"HasInitialized.{nameof(RuntimeDatabase)}";
            if (SessionState.GetBool(keyName, false))
                return;
            SessionState.SetBool(keyName, true);

            foreach (var assembly in ApplicationUtility.GetAssemblies())
                foreach (var type in ApplicationUtility.GetTypesFromAssembly(assembly))
                {
                    if (!IsSubclassOfRawGeneric(typeof(RuntimeDatabase<>), type) || type == typeof(RuntimeDatabase<>))
                        continue;
                    var propertyField = type.GetProperty($"Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    propertyField?.GetValue(null);
                }
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                    return true;
                toCheck = toCheck.BaseType;
            }
            return false;
        }
#endif
    }

    /// <summary>
    /// A <see cref="ScriptableObject"/> singleton data container.
    /// </summary>
    /// 
    /// <remarks>
    /// <example> 
    /// Within classes inheriting from this, the asset is retrieved as if it were an actual singleton:
    ///     <code>
    ///     FooDatabase.<see cref="Instance">Instance</see>
    ///     <br/>or
    ///     GameSettingsExampleDatabase.<see cref="Instance">Instance</see>
    ///     </code>
    /// </example>
    /// </remarks>
    public abstract class RuntimeDatabase<T> : ScriptableObject where T : RuntimeDatabase<T>
    {
        [NonSerialized]
        private static T _instance;


        /// <summary>
        /// Returns the asset data container of <see cref="T"/>.
        /// </summary>
        public static T Instance => _instance ??= Get();



        protected virtual void OnGet() { }
        protected virtual void OnCreate() { }

        private static T Get()
        {
            T instance = Resources.Load<T>(typeof(T).Name);
            bool wasCreated = false;

            if (instance == null) 
            {
                instance = CreateInstance<T>();
                instance.OnCreate();
                wasCreated = true;
            }

            if (!wasCreated)
            {
                instance.OnGet();
                return instance;
            }

#if UNITY_EDITOR
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
            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
#endif
            instance.OnGet();
            return instance;
        }
    }
}