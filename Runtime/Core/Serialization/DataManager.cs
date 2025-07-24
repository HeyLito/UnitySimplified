using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnitySimplified.Serialization.Formatters;

namespace UnitySimplified.Serialization
{
    public static class DataManager
    {
        #region FIELDS
        public static bool Debug = false;
        public static bool AutoCreateDatabaseDirectory = false;
        public static string PersistentDirectoryPath = Application.persistentDataPath;
        public static string SubDirectory = "User Data";

        private static Dictionary<string, FileDatabase> _cachedFileDatabasesByFullPaths;
        #endregion

        #region PROPERTIES
        public static string DefaultPath { get { string path = Path.Combine(PersistentDirectoryPath, SubDirectory); if (!Directory.Exists(path)) Directory.CreateDirectory(path); return path; } }
        public static string TargetDataPath { get => PlayerPrefs.GetString("TargetDataPath", DefaultPath); set => PlayerPrefs.SetString("TargetDataPath", value); }
        #endregion

        #region METHODS
        /// <summary>
        /// Sets <see cref="TargetDataPath"/> back to default value.
        /// </summary>
        public static void ResetTargetDataPath() => TargetDataPath = DefaultPath;

        /// <summary>
        /// <inheritdoc cref="DoLoadDatabase"/>
        /// </summary>
        public static void LoadDatabase() => DoLoadDatabase();

        /// <typeparam name="T"><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></typeparam>
        /// <summary><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></summary>
        /// <returns><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></returns>
        /// <param name="formatter"><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></param>
        /// <param name="obj"><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></param>
        /// <param name="key"><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></param>
        /// <param name="fileName"><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></param>
        public static bool CreateNewFile<T>(IDataFormatter formatter, T obj, string key, string fileName) => GetActiveDatabase().CreateNewFile(formatter, obj, key, fileName, null, AutoCreateDatabaseDirectory);

        /// <typeparam name="T"><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></typeparam>
        /// <summary><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></summary>
        /// <returns><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></returns>
        /// <param name="formatter"><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></param>
        /// <param name="obj"><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></param>
        /// <param name="key"><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></param>
        /// <param name="fileName"><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></param>
        /// <param name="fileSubPath"><inheritdoc cref="FileDatabase.DoCreateNewFile{T}"/></param>
        public static bool CreateNewFile<T>(IDataFormatter formatter, T obj, string key, string fileName, string fileSubPath) => GetActiveDatabase().CreateNewFile(formatter, obj, key, fileName, fileSubPath, AutoCreateDatabaseDirectory);

        /// <typeparam name="T"><inheritdoc cref="FileDatabase.DoSaveToFile{T}"/></typeparam>
        /// <summary><inheritdoc cref="FileDatabase.DoSaveToFile{T}"/></summary>
        /// <returns><inheritdoc cref="FileDatabase.DoSaveToFile{T}"/></returns>
        /// <param name="key"><inheritdoc cref="FileDatabase.DoSaveToFile{T}"/></param>
        /// <param name="obj"><inheritdoc cref="FileDatabase.DoSaveToFile{T}"/></param>
        public static bool SaveToFile<T>(string key, T obj) => GetActiveDatabase().SaveToFile(key, obj);

        /// <typeparam name="T"><inheritdoc cref="FileDatabase.DoLoadFromFile{T}"/></typeparam>
        /// <summary><inheritdoc cref="FileDatabase.DoLoadFromFile{T}"/></summary>
        /// <returns><inheritdoc cref="FileDatabase.DoLoadFromFile{T}"/></returns>
        /// <param name="key"><inheritdoc cref="FileDatabase.DoLoadFromFile{T}"/></param>
        /// <param name="obj"><inheritdoc cref="FileDatabase.DoLoadFromFile{T}"/></param>
        public static bool LoadFromFile<T>(string key, T obj) => GetActiveDatabase().LoadFromFile(key, obj);

        /// <summary><inheritdoc cref="FileDatabase.DoDeleteFile"/></summary>
        /// <returns><inheritdoc cref="FileDatabase.DoDeleteFile"/></returns>
        /// <param name="key"><inheritdoc cref="FileDatabase.DoDeleteFile"/></param>
        public static bool DeleteFile(string key) => GetActiveDatabase().DeleteFile(key);

        /// <summary><inheritdoc cref="FileDatabase.DoContainsFile"/></summary>
        /// <returns><inheritdoc cref="FileDatabase.DoContainsFile"/></returns>
        /// <param name="key"><inheritdoc cref="FileDatabase.DoContainsFile"/></param>
        public static bool ContainsFile(string key) => GetActiveDatabase().ContainsFile(key, out _, out _, out _);

        /// <summary><inheritdoc cref="FileDatabase.DoContainsFile"/></summary>
        /// <returns><inheritdoc cref="FileDatabase.DoContainsFile"/></returns>
        /// <param name="key"><inheritdoc cref="FileDatabase.DoContainsFile"/></param>
        /// <param name="fileName"><inheritdoc cref="FileDatabase.DoContainsFile"/></param>
        /// <param name="fileDirectoryPath"><inheritdoc cref="FileDatabase.DoContainsFile"/></param>
        /// <param name="fileFormatter"><inheritdoc cref="FileDatabase.DoContainsFile"/></param>
        public static bool ContainsFile(string key, out string fileName, out string fileDirectoryPath, out IDataFormatter fileFormatter) => GetActiveDatabase().ContainsFile(key, out fileName, out fileDirectoryPath, out fileFormatter);
        public static void SaveObjectAsString<T>(IDataFormatter formatter, T instance, out string instanceData) => formatter.SerializeToString(instance, out instanceData);
        public static void LoadObjectFromString<T>(IDataFormatter formatter, T instance, string instanceData) => formatter.DeserializeFromString(instance, instanceData);



        /// <summary>
        /// Loads the file database given from <see cref="TargetDataPath"/>.
        /// </summary>
        private static void DoLoadDatabase()
        {
            if (string.IsNullOrWhiteSpace(TargetDataPath))
                throw new DataManagerInvalidPathException($"{nameof(TargetDataPath)} can not be empty.");
            if (Debug)
                UnityEngine.Debug.Log($"[DataManagerMessage, DatabaseDirectory] Loading {nameof(TargetDataPath)} database from directory path \"{TargetDataPath}\".");

            var modified = new List<FileDatabase.Entry>();
            var missing = new List<FileDatabase.Entry>();

            _cachedFileDatabasesByFullPaths ??= new Dictionary<string, FileDatabase>();

            var databaseModified = false;
            var database = GetActiveDatabase();
            database.VerifyEntries(modified, missing);
            foreach (var item in missing)
            {
                if (Debug)
                    UnityEngine.Debug.Log($"Missing: {item}");
                database.RemoveEntry(item);
                databaseModified = true;
            }

            if (databaseModified)
            {
                database.SaveDatabase();
                if (Debug && missing.Count > 0)
                    UnityEngine.Debug.Log($"[DataManagerMessage, DatabaseDirectory] Removed {missing.Count} missing file reference{(missing.Count > 0 ? "s" : "")}.");
            }
            if (Debug)
                UnityEngine.Debug.Log($"[DataManagerMessage, DatabaseDirectory] Successfully loaded {nameof(TargetDataPath)} database at path \"{database.FullPath}\".");
        }

        /// <summary></summary>
        /// <returns></returns>
        /// <exception cref="DataManagerFileDatabaseException"></exception>
        private static FileDatabase GetActiveDatabase()
        {
            var directoryPath = TargetDataPath;
            if (_cachedFileDatabasesByFullPaths == null)
                throw new DataManagerFileDatabaseException($"File databases are not loaded or are missing. Invoke {nameof(LoadDatabase)}.");

            string databasePath = Path.Combine(directoryPath, FileDatabase.FileName);

            if (!_cachedFileDatabasesByFullPaths.TryGetValue(databasePath, out var fileDatabase))
                if (!FileDatabase.TryGetDatabase(directoryPath, out fileDatabase))
                {
                    fileDatabase = new FileDatabase(directoryPath);
                    _cachedFileDatabasesByFullPaths.Add(fileDatabase.FullPath, fileDatabase);
                }
            return fileDatabase;
        }
        #endregion
    }
}