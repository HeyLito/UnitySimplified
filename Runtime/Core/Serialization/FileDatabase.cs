using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnitySimplified.Serialization.Formatters;

namespace UnitySimplified.Serialization
{
    [Serializable]
    [XmlRoot(Namespace = "")]
    [DataContract(Namespace = "")]
    public sealed partial class FileDatabase
    {
        [SerializeField] [DataMember(Name = "Entries")]
        private List<Entry> entries;

        private static readonly Dictionary<string, FileDatabase> OpenedDatabases = new();
        [NonSerialized]
        private readonly List<Entry> _tempEntries;
        [NonSerialized]
        private readonly Dictionary<string, Entry> _cachedEntriesByKeys;

        private FileDatabase()
        {
            entries = new List<Entry>();
            _tempEntries = new List<Entry>();
            _cachedEntriesByKeys = new Dictionary<string, Entry>();
        }
        public FileDatabase(string directory) : this() => Directory = directory;

        public static string FileName => "Database.xml";
        public string Directory { get; private set; }
        public string FullPath => Path.Combine(Directory, FileName);
        public bool HasData { get; private set; }



        #region DATABASE_METHODS
        /// <summary><inheritdoc cref="TryGetDatabase"/></summary>
        /// <param name="directoryPath"><inheritdoc cref="TryGetDatabase"/></param>
        /// 
        /// <returns></returns>
        /// 
        /// <exception cref="FileDatabaseInvalidPathException">
        /// </exception>
        public static FileDatabase GetDatabase(string directoryPath)
        {
            if (!TryGetDatabase(directoryPath, out FileDatabase fileDatabase))
                throw new FileDatabaseInvalidPathException($"Database unable to found at \"{directoryPath}\"");
            return fileDatabase;
        }
        /// <summary>
        /// </summary>
        ///
        /// <param name="directoryPath"> Path to retrieve <paramref name="fileDatabase"/> from. </param>
        /// <param name="fileDatabase"> <see cref="FileDatabase"/> retrieved from <paramref name="directoryPath"/>. </param>
        /// 
        /// <returns>
        /// </returns>
        public static bool TryGetDatabase(string directoryPath, out FileDatabase fileDatabase)
        {
            fileDatabase = null;
            string databasePath = Path.Combine(directoryPath, FileName);

            if (OpenedDatabases.TryGetValue(databasePath, out fileDatabase))
                return true;

            if (!File.Exists(databasePath))
                return false;

            fileDatabase = new FileDatabase(directoryPath);
            using (var fileStream = new FileStream(databasePath, FileMode.Open))
            {
                var xmlSerializer = new DataContractSerializer(typeof(FileDatabase));
                SerializationUtility.OverwriteInstanceFromOther((FileDatabase)xmlSerializer.ReadObject(fileStream), fileDatabase);
                fileDatabase.Directory = directoryPath;
                fileDatabase.HasData = true;
                fileDatabase.AfterDeserialization();
            }
            OpenedDatabases[databasePath] = fileDatabase;
            return true;
        }
        public void SaveDatabase()
        {
            if (OpenedDatabases.TryGetValue(FullPath, out FileDatabase openedFileDatabase))
            {
                if (openedFileDatabase != this)
                    throw new Exception("Contains");
            }
            else OpenedDatabases[FullPath] = this;

            if (!HasData)
            {
                if (!System.IO.Directory.Exists(Directory))
                    System.IO.Directory.CreateDirectory(Directory);
            }

            var fileStream = new FileStream(FullPath, FileMode.Create);
            var xmlSerializer = new DataContractSerializer(typeof(FileDatabase));
            var xmlSettings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };
            HasData = true;
            BeforeSerialization();
            using (var xmlWriter = XmlWriter.Create(fileStream, xmlSettings))
                xmlSerializer.WriteObject(xmlWriter, this);
            fileStream.Close();
        }
        #endregion

        #region FILE_METHODS
        /// <typeparam name="T"><inheritdoc cref="DoCreateNewFile{T}"/></typeparam>
        /// <summary><inheritdoc cref="DoCreateNewFile{T}"/></summary>
        /// <returns><inheritdoc cref="DoCreateNewFile{T}"/></returns>
        /// <param name="formatter"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        /// <param name="obj"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        /// <param name="key"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        /// <param name="fileName"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        /// <param name="fileSubPath"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        /// <param name="canCreateDirectory"><inheritdoc cref="DoCreateNewFile{T}"/></param>
        public bool CreateNewFile<T>(IDataFormatter formatter, T obj, string key, string fileName, string fileSubPath = "", bool canCreateDirectory = false) => DoCreateNewFile(formatter, obj, key, fileName, fileSubPath, canCreateDirectory);

        /// <typeparam name="T"><inheritdoc cref="DoSaveToFile{T}"/></typeparam>
        /// <summary><inheritdoc cref="DoSaveToFile{T}"/></summary>
        /// <returns><inheritdoc cref="DoSaveToFile{T}"/></returns>
        /// <param name="obj"><inheritdoc cref="DoSaveToFile{T}"/></param>
        /// <param name="key"><inheritdoc cref="DoSaveToFile{T}"/></param>
        public bool SaveToFile<T>(string key, T obj) => DoSaveToFile(key, obj);

        public bool LoadFromFile<T>(string key, T obj) => DoLoadFromFile(key, obj);

        public bool ContainsFile(string key) => DoContainsFile(key, out _, out _, out _);
        public bool ContainsFile(string key, out string fileName, out string fileDirectory, out IDataFormatter fileFormatter) => DoContainsFile(key, out fileName, out fileDirectory, out fileFormatter);

        public bool DeleteFile(string key) => DoDeleteFile(key);




        /// <typeparam name="T"></typeparam>
        /// 
        /// <summary> Creates a save file for an object and stores its disk location in this database. </summary>
        /// 
        /// <returns></returns>
        /// 
        /// <param name="formatter"></param>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <param name="fileName"></param>
        /// <param name="fileSubPath"></param>
        /// <param name="canCreateDirectory"></param>
        ///
        /// <exception cref="ArgumentNullException"></exception>
        private bool DoCreateNewFile<T>(IDataFormatter formatter, T obj, string key, string fileName, string fileSubPath, bool canCreateDirectory)
        {
            if (key == string.Empty)
                throw new ArgumentNullException($"{nameof(key)} is empty or NULL!");
            if (fileName == string.Empty)
                throw new ArgumentNullException($"{nameof(fileName)} is empty or NULL!");
            if (formatter.FileExtension == string.Empty)
                throw new ArgumentNullException($"{nameof(formatter)}.{nameof(formatter.FileExtension)} is empty or NULL!");

            var fileDirectory = !string.IsNullOrWhiteSpace(fileSubPath) ? Path.Combine(Directory, fileSubPath) : Directory;
            if (!System.IO.Directory.Exists(fileDirectory))
            {
                if (canCreateDirectory)
                    System.IO.Directory.CreateDirectory(fileDirectory);
                else
                {
                    //if (debug)
                    //    Debug.LogWarning($"[DataManagerError, FileCreation] Database path is is missing at path \"{directoryPath}\".");
                    return false;
                }
            }

            if (!fileDirectory.Equals(Directory))
            {
                if (!System.IO.Directory.Exists(fileDirectory))
                    System.IO.Directory.CreateDirectory(fileDirectory);
            }

            var entry = new Entry(key, fileName + formatter.FileExtension, fileSubPath, formatter);
            if (ContainsEntry(entry))
                return false;

            var fullFilePath = GetEntryPath(entry);
            entry.Formatter.SerializeToFile(fullFilePath, obj);
            InsertEntry(entry);
            SaveDatabase();
            //if (debug)
            //    Debug.Log($"[DataManagerMessage, FileCreation] Successfully created file under {nameof(fileIdentifier)}:\"{fileIdentifier}\" to path \"{databaseFile.FullPath}\".");
            return true;
        }

        /// <typeparam name="T"></typeparam>
        /// 
        /// <summary>
        /// Searches the loaded databases for an existing file with the identical name and overwrites the discovered file from the object data.
        /// </summary>
        /// 
        /// <returns></returns>
        /// 
        /// <param name="key"></param>
        /// <param name="obj"></param>
        private bool DoSaveToFile<T>(string key, T obj)
        {
            if (TryGetEntry(key, out Entry entry))
            {
                var fullFilePath = GetEntryPath(entry);
                if (!File.Exists(fullFilePath))
                {
                    //if (debug)
                    //    Debug.LogWarning($"[DataManagerError, FileSave] Failed to locate {nameof(fileIdentifier)}: \"{fileIdentifier}\" at {nameof(databaseFile.FullPath)}: \"{databaseFile.FullPath}\".");
                    return false;
                }

                entry.Formatter.SerializeToFile(fullFilePath, obj);
                //if (debug)
                //    Debug.Log($"[DataManagerMessage, FileSave] Successfully saved {nameof(fileIdentifier)}:\"{fileIdentifier}\" to file at path \"{databaseFile.FullPath}\".");
                return true;
            }

            //if (debug)
            //    Debug.LogWarning($"[DataManagerError, FileSave] {nameof(fileIdentifier)}:\"{fileIdentifier}\" does not exist in database.");
            return false;
        }

        /// <typeparam name="T"></typeparam>
        /// 
        /// <summary> Searches this database for a file with the identical <paramref name="key"/> and retrieves it from disk space. </summary>
        ///
        /// <returns></returns>
        ///
        /// <param name="key"></param>
        /// <param name="obj"></param>
        private bool DoLoadFromFile<T>(string key, T obj)
        {
            if (TryGetEntry(key, out Entry entry))
            {
                var fullFilePath = GetEntryPath(entry);
                if (!File.Exists(fullFilePath))
                {
                    //if (debug)
                    //    Debug.LogWarning($"[DataManagerError, FileLoad] Failed to locate {nameof(fileIdentifier)}: \"{fileIdentifier}\" at {nameof(databaseFile.FullPath)}: \"{databaseFile.FullPath}\".");
                    return false;
                }

                entry.Formatter.DeserializeFromFile(fullFilePath, obj);
                //if (debug)
                //    Debug.Log($"[DataManagerMessage, FileLoad] Successfully loaded {nameof(fileIdentifier)}:\"{fileIdentifier}\" from the file found at path \"{databaseFile.FullPath}\".");
                return true;
            }

            //if (debug)
            //    Debug.LogWarning($"[DataManagerError, FileLoad] {nameof(fileIdentifier)}:\"{fileIdentifier}\" does not exist in database.");
            return false;
        }

        private bool DoContainsFile(string key, out string fileName, out string fileDirectory, out IDataFormatter fileFormatter)
        {
            if (!TryGetEntry(key, out Entry entry))
            {
                fileName = string.Empty;
                fileDirectory = string.Empty;
                fileFormatter = null;
                return false;
            }

            fileName = entry.Name;
            fileDirectory = GetEntryPath(entry);
            fileFormatter = entry.Formatter;
            return true;
        }

        /// <summary> Searches for a save file with the identical <paramref name="key"/> and removes it from the database and disk space. </summary>
        /// <returns> True if the file was deleted. </returns>
        /// <param name="key"></param>
        private bool DoDeleteFile(string key)
        {
            if (TryGetEntry(key, out Entry entry))
            {
                var fullFilePath = GetEntryPath(entry);
                if (File.Exists(fullFilePath))
                {
                    File.Delete(fullFilePath);
                    RemoveEntry(entry);
                    SaveDatabase();
                    //if (debug)
                    //    Debug.Log($"[DataManagerMessage, FileDeletion] Successfully removed {nameof(fileIdentifier)}:\"{fileIdentifier}\" and the file found at path \"{databaseFile.FullPath}\".");
                }
                else
                {
                    //if (debug)
                    //    Debug.LogWarning($"[DataManagerError, FileDeletion] Found {nameof(fileIdentifier)}:\"{fileIdentifier}\" in database, but could not find file to delete at path \"{databaseFile.FullPath}\".");
                    return false;
                }
                return true;
            }

            //if (debug)
            //    Debug.LogWarning($"[DataManagerError, FileDeletion] {nameof(fileIdentifier)}\"{fileIdentifier}\" does not exist in database.");
            return false;
        }
        #endregion

        #region ENTRY_METHODS
        private void BeforeSerialization()
        {
            entries.Clear();
            foreach (var entry in _cachedEntriesByKeys.Values)
                entries.Add(entry);
        }
        private void AfterDeserialization()
        {
            _cachedEntriesByKeys.Clear();
            foreach (var entry in entries)
                _cachedEntriesByKeys.Add(entry.Key, entry);
        }
        internal bool TryGetEntry(string key, out Entry entry) => _cachedEntriesByKeys.TryGetValue(key, out entry);
        internal bool ContainsEntry(Entry entry) => _cachedEntriesByKeys.ContainsKey(entry.Key);
        internal void InsertEntry(Entry entry)
        {
            if (!_cachedEntriesByKeys.TryAdd(entry.Key, entry))
                throw new FileDatabaseKeyDuplicateException($"Database already contains {nameof(entry.Key)}: {entry.Key}");
        }
        internal void RemoveEntry(Entry entry)
        {
            if (!_cachedEntriesByKeys.Remove(entry.Key))
                throw new FileDatabaseKeyMissingException($"Database doesn't contain {nameof(entry.Key)}: {entry.Key}");
        }
        internal void VerifyEntries(List<Entry> modified, List<Entry> missing)
        {
            if (!HasData)
                return;

            _tempEntries.Clear();
            foreach (var databaseFile in _cachedEntriesByKeys.Values)
                _tempEntries.Add(databaseFile);

            if (missing != null)
            {
                missing.Clear();
                for (int i = _tempEntries.Count - 1; i >= 0; i--)
                {

                    if (File.Exists(GetEntryPath(_tempEntries[i])))
                        continue;

                    missing.Add(_tempEntries[i]);
                }
            }
            modified?.Clear();
        }

        internal string GetEntryPath(Entry entry) => !string.IsNullOrWhiteSpace(entry.SubDirectory) ? Path.Combine(Directory, entry.SubDirectory, entry.Name) : Path.Combine(Directory, entry.Name);

        #endregion
    }
}
