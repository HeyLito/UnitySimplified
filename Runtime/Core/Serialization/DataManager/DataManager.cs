using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnitySimplified.Serialization.Formatters;

namespace UnitySimplified.Serialization
{
    public static partial class DataManager
    {
        #region FIELDS
        public static bool debug = false;
        public static bool autoCreateDatabaseDirectory = false;
        public static string persistentDirectoryPath = Application.persistentDataPath;
        public static string subDirectory = "User Data";

        private const string PathColor = "#80E6FF";
        private static DatabaseCache _databaseCache;
        #endregion

        #region PROPERTIES
        public static string DefaultPath { get { string path = Path.Combine(persistentDirectoryPath, subDirectory); if (!Directory.Exists(path)) Directory.CreateDirectory(path); return path; } }
        public static string TargetDataPath { get => PlayerPrefs.GetString("TargetDataPath", DefaultPath); set => PlayerPrefs.SetString("TargetDataPath", value); }
        #endregion

        #region METHODS
        /// <summary>
        /// Sets <see cref="TargetDataPath"/> back to default value.
        /// </summary>
        public static void ResetTargetDataPath() => TargetDataPath = DefaultPath;

        /// <summary>
        /// <inheritdoc cref="LoadDatabase"/>
        /// </summary>
        public static void LoadDatabase() => DoLoadDatabase();

        /// <typeparam name="T"><inheritdoc cref="DoCreateNewFile{T}"/></typeparam>
        /// <summary><inheritdoc cref="DoCreateNewFile{T}"/></summary>
        /// <returns><inheritdoc cref="DoCreateNewFile{T}"/></returns>
        /// <param name="fileIdentifier"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        /// <param name="fileName"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        /// <param name="fileFormatter"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        /// <param name="obj"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        public static bool CreateNewFile<T>(string fileIdentifier, string fileName, IDataFormatter fileFormatter, T obj) => DoCreateNewFile(fileIdentifier, fileName, null, fileFormatter, obj);

        /// <typeparam name="T"><inheritdoc cref="DoCreateNewFile{T}"/></typeparam>
        /// <summary><inheritdoc cref="DoCreateNewFile{T}"/></summary>
        /// <returns><inheritdoc cref="DoCreateNewFile{T}"/></returns>
        /// <param name="fileIdentifier"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        /// <param name="fileName"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        /// <param name="fileSubPath"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        /// <param name="fileFormatter"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        /// <param name="obj"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        public static bool CreateNewFile<T>(string fileIdentifier, string fileName, string fileSubPath, IDataFormatter fileFormatter, T obj) => DoCreateNewFile(fileIdentifier, fileName, fileSubPath, fileFormatter, obj);

        /// <typeparam name="T"><inheritdoc cref="DoSaveToFile{T}"/></typeparam>
        /// <summary><inheritdoc cref="DoSaveToFile{T}"/></summary>
        /// <returns><inheritdoc cref="DoSaveToFile{T}"/></returns>
        /// <param name="fileIdentifier"><inheritdoc cref="DoSaveToFile{T}"/></param>
        /// <param name="obj"><inheritdoc cref="DoSaveToFile{T}"/></param>
        public static bool SaveToFile<T>(string fileIdentifier, T obj) => DoSaveToFile(fileIdentifier, obj);

        /// <typeparam name="T"><inheritdoc cref="DoLoadFromFile{T}"/></typeparam>
        /// <summary><inheritdoc cref="DoLoadFromFile{T}"/></summary>
        /// <returns><inheritdoc cref="DoLoadFromFile{T}"/></returns>
        /// <param name="fileIdentifier"><inheritdoc cref="DoLoadFromFile{T}"/></param>
        /// <param name="obj"><inheritdoc cref="DoLoadFromFile{T}"/></param>
        public static bool LoadFromFile<T>(string fileIdentifier, T obj) => DoLoadFromFile(fileIdentifier, obj);

        /// <summary><inheritdoc cref="DoDeleteFile"/></summary>
        /// <returns><inheritdoc cref="DoDeleteFile"/></returns>
        /// <param name="fileIdentifier"><inheritdoc cref="DoDeleteFile"/></param>
        public static bool DeleteFile(string fileIdentifier) => DoDeleteFile(fileIdentifier);
        public static bool ContainsFile(string fileIdentifier) => DoContainsFile(fileIdentifier, out _, out _, out _);
        public static bool ContainsFile(string fileIdentifier, out string fileName, out string filePath, out IDataFormatter fileFormatter) => DoContainsFile(fileIdentifier, out fileName, out filePath, out fileFormatter);
        public static void SaveObjectAsString<T>(T instance, IDataFormatter fileFormatter, out string instanceData) => fileFormatter.SerializeToString(instance, out instanceData);
        public static void LoadObjectFromString<T>(T instance, IDataFormatter fileFormatter, string instanceData) => fileFormatter.DeserializeFromString(instance, instanceData);



        /// <summary>
        /// Loads the file database given from <see cref="TargetDataPath"/>.
        /// </summary>
        private static void DoLoadDatabase()
        {
            if (string.IsNullOrEmpty(TargetDataPath))
            {
                Debug.LogWarning($"[DataManagerError, DatabaseDirectory] Path from {nameof(TargetDataPath)} returned \"{TargetDataPath}\", which is invalid.");
                return;
            }
            if (debug)
                Debug.Log($"[DataManagerMessage, DatabaseDirectory] Loading {nameof(TargetDataPath)} database from directory path \"{TargetDataPath}\".");

            var modified = new List<Database.File>();
            var missing = new List<Database.File>();

            _databaseCache ??= new DatabaseCache();

            var databaseModified = false;
            var database = _databaseCache.GetDatabase(TargetDataPath);
            database.VerifyFiles(modified, missing);
            foreach (var item in missing)
            {
                database.Remove(item);
                databaseModified = true;
            }

            if (databaseModified)
            {
                _databaseCache.SaveDatabase(database);
                if (debug && missing.Count > 0)
                    Debug.Log($"[DataManagerMessage, DatabaseDirectory] Removed {missing.Count} missing file reference{(missing.Count > 0 ? "s" : "")}.");
            }
            if (debug)
                Debug.Log($"[DataManagerMessage, DatabaseDirectory] Successfully loaded {nameof(TargetDataPath)} database at path \"{database.FullPath}\".");
        }

        /// <typeparam name="T">
                /// </typeparam>
        /// 
        /// <summary>
        /// Creates a new file from the object's data and inserts it into current loaded databases.
        /// </summary>
        /// 
        /// <returns>
        /// </returns>
        /// 
        /// <param name="fileIdentifier">
        /// </param>
        /// 
        /// <param name="fileName">
        /// </param>
        /// 
        /// <param name="fileSubPath">
        /// </param>
        /// 
        /// <param name="fileFormatter">
        /// </param>
        /// 
        /// <param name="obj">
        /// </param>
        private static bool DoCreateNewFile<T>(string fileIdentifier, string fileName, string fileSubPath, IDataFormatter fileFormatter, T obj)
        {
            if (_databaseCache == null)
            {
                if (debug)
                    Debug.LogWarning($"[DataManagerError, FileDirectory] {nameof(DataManager)} file databases are not loaded or are missing.");
                return false;
            }

            if (fileIdentifier == string.Empty)
                throw new ArgumentException($"{nameof(fileIdentifier)} is empty or NULL!");
            if (fileName == string.Empty)
                throw new ArgumentException($"{nameof(fileName)} is empty or NULL!");
            if (fileFormatter.FileExtension == string.Empty)
                throw new ArgumentException($"{nameof(fileFormatter)}.{nameof(fileFormatter.FileExtension)} is empty or NULL!");

            Database database = _databaseCache.GetDatabase(TargetDataPath);
            if (database.TryGetFileEntry(fileIdentifier, out var existingFile))
            {
                if (debug)
                    Debug.LogWarning($"[DataManagerError, FileCreation] {nameof(fileIdentifier)}:\"{fileIdentifier}\" already exists at path \"{existingFile.FullPath}\".");
                return false;
            }

            string directoryPath = database.DirectoryPath;
            if (!Directory.Exists(directoryPath))
            {
                if (autoCreateDatabaseDirectory)
                    Directory.CreateDirectory(directoryPath);
                else
                {
                    if (debug)
                        Debug.LogWarning($"[DataManagerError, FileCreation] Database path is is missing at path \"{directoryPath}\".");
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(fileSubPath))
            {
                directoryPath = Path.Combine(directoryPath, fileSubPath);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
            }

            var databaseFile = new Database.File(fileIdentifier, fileName + fileFormatter.FileExtension, directoryPath, fileFormatter);
            databaseFile.Formatter.SerializeToFile(databaseFile.FullPath, obj);
            database.Add(databaseFile);
            _databaseCache.SaveDatabase(database);
            if (debug)
                Debug.Log($"[DataManagerMessage, FileCreation] Successfully created file under {nameof(fileIdentifier)}:\"{fileIdentifier}\" to path \"{databaseFile.FullPath}\".");
            return true;

        }

        /// <typeparam name="T">
                /// </typeparam>
        /// 
        /// <summary>
        /// Searches the loaded databases for an existing file with the identical name and overwrites the discovered file from the object data.
        /// </summary>
        /// 
        /// <returns>
        /// </returns>
        /// 
        /// <param name="fileIdentifier">
        /// </param>
        ///
        /// <param name="obj">
        /// </param>
        private static bool DoSaveToFile<T>(string fileIdentifier, T obj)
        {
            if (_databaseCache == null)
            {
                if (debug)
                    Debug.LogWarning($"[DataManagerError, DatabaseDirectory] {nameof(DataManager)} databases are not loaded or are missing.");
                return false;
            }
            Database database = _databaseCache.GetDatabase(TargetDataPath);
            if (database.TryGetFileEntry(fileIdentifier, out Database.File databaseFile))
            {
                databaseFile.Formatter.SerializeToFile(databaseFile.FullPath, obj);
                if (debug)
                    Debug.Log($"[DataManagerMessage, FileSave] Successfully saved {nameof(fileIdentifier)}:\"{fileIdentifier}\" to file at path \"{databaseFile.FullPath}\".");
                return true;
            }

            if (debug)
                Debug.LogWarning($"[DataManagerError, FileSave] {nameof(fileIdentifier)}:\"{fileIdentifier}\" does not exist in database.");
            return false;
        }

        /// <typeparam name="T">
        /// </typeparam>
        /// 
        /// <summary>
        /// Searches the loaded databases for an existing file with the identical name and overwrites the object from the discovered file.
        /// </summary>
        ///
        /// <returns>
        /// </returns>
        ///
        /// <param name="fileIdentifier">
        /// </param>
        ///
        /// <param name="obj">
        /// </param>
        private static bool DoLoadFromFile<T>(string fileIdentifier, T obj)
        {
            if (_databaseCache == null)
            {
                if (debug)
                    Debug.LogWarning($"[DataManagerError, DatabaseDirectory] {nameof(DataManager)} databases are not loaded or are missing.");
                return false;
            }
            Database database = _databaseCache.GetDatabase(TargetDataPath);
            if (database.TryGetFileEntry(fileIdentifier, out Database.File databaseFile))
            {
                databaseFile.Formatter.DeserializeFromFile(databaseFile.FullPath, obj);
                if (debug)
                    Debug.Log($"[DataManagerMessage, FileLoad] Successfully loaded {nameof(fileIdentifier)}:\"{fileIdentifier}\" from the file found at path \"{databaseFile.FullPath}\".");
                return true;
            }

            if (debug)
                Debug.LogWarning($"[DataManagerError, FileLoad] {nameof(fileIdentifier)}:\"{fileIdentifier}\" does not exist in database.");
            return false;
        }

        /// <summary>
        ///         Searches the loaded databases for an existing file with the identical identifier and removes it from the database and file-system.
        /// </summary>
        /// 
        /// <returns>
        /// True if the file was deleted.
        /// </returns>
        /// 
        /// <param name="fileIdentifier">
        /// </param>
        private static bool DoDeleteFile(string fileIdentifier)
        {
            if (_databaseCache == null)
            {
                if (debug)
                    Debug.LogWarning($"[DataManagerError, DatabaseDirectory] {nameof(DataManager)} databases are not loaded or are missing.");
                return false;
            }
            Database database = _databaseCache.GetDatabase(TargetDataPath);
            if (database.TryGetFileEntry(fileIdentifier, out Database.File databaseFile))
            {
                if (File.Exists(databaseFile.FullPath))
                {
                    File.Delete(databaseFile.FullPath);
                    database.Remove(databaseFile);
                    _databaseCache.SaveDatabase(database);
                    if (debug)
                        Debug.Log($"[DataManagerMessage, FileDeletion] Successfully removed {nameof(fileIdentifier)}:\"{fileIdentifier}\" and the file found at path \"{databaseFile.FullPath}\".");
                }
                else
                {
                    if (debug)
                        Debug.LogWarning($"[DataManagerError, FileDeletion] Found {nameof(fileIdentifier)}:\"{fileIdentifier}\" in database, but could not find file to delete at path \"{databaseFile.FullPath}\".");
                    return false;
                }
                return true;
            }

            if (debug)
                Debug.LogWarning($"[DataManagerError, FileDeletion] {nameof(fileIdentifier)}\"{fileIdentifier}\" does not exist in database.");
            return false;
        }

        private static bool DoContainsFile(string fileIdentifier, out string fileName, out string fileDirectoryPath, out IDataFormatter fileFormatter)
        {
            if (_databaseCache == null)
            {
                if (debug)
                    Debug.LogWarning($"[DataManagerError, DatabaseDirectory] {nameof(DataManager)} databases are not loaded or are missing.");
                fileName = "";
                fileDirectoryPath = "";
                fileFormatter = null;
                return false;
            }

            Database database = _databaseCache.GetDatabase(TargetDataPath);
            if (!database.TryGetFileEntry(fileIdentifier, out Database.File file))
            {
                fileName = string.Empty;
                fileDirectoryPath = string.Empty;
                fileFormatter = null;
                return false;
            }

            fileName = file.Name;
            fileDirectoryPath = file.DirectoryPath;
            fileFormatter = file.Formatter;
            return true;
        }

        private static string FormatStringPath(string path) => $"<color={PathColor}>{path}</color>";

        #endregion
    }
}