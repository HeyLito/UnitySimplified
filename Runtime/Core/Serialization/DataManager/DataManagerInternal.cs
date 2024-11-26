using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnitySimplified.Serialization.Formatters;

namespace UnitySimplified.Serialization
{
    public static partial class DataManager
    {
        [Serializable]
        internal partial class Database
        {
            [SerializeField]
            private string directoryPath;
            [SerializeField]
            private List<File> files;

            [NonSerialized]
            private readonly List<File> _tempFiles;
            [NonSerialized]
            private readonly Dictionary<string, File> _cachedFilesByIDs;

            public Database(string directoryPath)
            {
                this.directoryPath = directoryPath;
                files = new List<File>();
                _tempFiles = new List<File>();
                _cachedFilesByIDs = new Dictionary<string, File>();
                IsTemporary = true;
            }

            public static string FileName => "Database.dat";
            public string DirectoryPath => directoryPath;
            public string FullPath => Path.Combine(DirectoryPath, FileName);
            public bool IsTemporary { get; set; }

            public void VerifyFiles(List<File> modified, List<File> missing)
            {
                if (!IsTemporary)
                    return;

                _tempFiles.Clear();
                foreach (var databaseFile in _cachedFilesByIDs.Values)
                    _tempFiles.Add(databaseFile);

                if (missing != null)
                {
                    missing.Clear();
                    for (int i = _tempFiles.Count - 1; i >= 0; i--)
                    {
                        if (System.IO.File.Exists(_tempFiles[i].FullPath))
                            continue;

                        missing.Add(_tempFiles[i]);
                        _tempFiles.RemoveAt(i);
                    }
                }
                modified?.Clear();
            }
            public bool TryGetFileEntry(string fileIdentifier, out File databaseFile) => _cachedFilesByIDs.TryGetValue(fileIdentifier, out databaseFile);
            public void Add(File databaseFile)
            {
                if (!_cachedFilesByIDs.TryAdd(databaseFile.Identifier, databaseFile))
                    throw new Exception($"Already contains {nameof(databaseFile.Identifier)}: {databaseFile.Identifier}");
            }
            public void Remove(File databaseFile)
            {
                if (!_cachedFilesByIDs.Remove(databaseFile.Identifier))
                    throw new Exception($"Does not contain {nameof(databaseFile.Identifier)}: {databaseFile.Identifier}");
            }
            public void OnBeforeSerialization()
            {
                files.Clear();
                foreach (var file in _cachedFilesByIDs.Values)
                    files.Add(file);
            }
            public void OnAfterDeserialization()
            {
                _cachedFilesByIDs.Clear();
                foreach (var file in files)
                    _cachedFilesByIDs.Add(file.Identifier, file);
            }
        }

        internal partial class Database
        {
            [Serializable]
            internal class File
            {
                [SerializeField]
                private string identifier;
                [SerializeField]
                private string name;
                [SerializeField]
                private string directoryPath;
                [SerializeField]
                private string formatter;

                [NonSerialized]
                private IDataFormatter _formatter;

                public File(string identifier, string name, string directoryPath, IDataFormatter formatter)
                {
                    this.identifier = identifier;
                    this.name = name;
                    this.directoryPath = directoryPath;
                    this.formatter = formatter.GetType().AssemblyQualifiedName;
                    _formatter = formatter;
                }

                public string Identifier => identifier;
                public string Name => name;
                public string DirectoryPath => directoryPath;
                public string FullPath => Path.Combine(DirectoryPath, Name);
                public IDataFormatter Formatter
                {
                    get
                    {
                        if (_formatter != null)
                            return _formatter;
                        if (string.IsNullOrEmpty(formatter))
                            return null;

                        return _formatter = (IDataFormatter)Activator.CreateInstance(Type.GetType(formatter) ?? throw new NullReferenceException());
                    }
                }
            }
        }

        internal class DatabaseCache
        {
            [NonSerialized]
            private Dictionary<string, Database> _cachedDatabasesByFullPaths;

            internal bool IsDatabaseLoaded(string directoryPath) => _cachedDatabasesByFullPaths != null && _cachedDatabasesByFullPaths.ContainsKey(Path.Combine(directoryPath, Database.FileName));
            internal Database GetDatabase(string directoryPath)
            {
                string databasePath = Path.Combine(directoryPath, Database.FileName);
                _cachedDatabasesByFullPaths ??= new Dictionary<string, Database>();
                if (_cachedDatabasesByFullPaths.TryGetValue(databasePath, out Database database))
                    return database;

                database = new Database(directoryPath);
                if (File.Exists(databasePath))
                {
                    database = new Database("");
                    var fileStream = new FileStream(databasePath, FileMode.Open);
                    var binaryFormatter = new BinaryFormatter();
                    DataManagerUtility.OverwriteInstanceFromOther((Database)binaryFormatter.Deserialize(fileStream), database);
                    database.OnAfterDeserialization();
                    database.IsTemporary = false;
                    fileStream.Close();
                }
                _cachedDatabasesByFullPaths.Add(database.FullPath, database);
                return database;
            }
            internal void SaveDatabase(Database database)
            {
                if (database.IsTemporary)
                {
                    if (!Directory.Exists(database.DirectoryPath))
                        Directory.CreateDirectory(database.DirectoryPath);
                }

                var fileStream = new FileStream(database.FullPath, FileMode.Create);
                var binaryFormatter = new BinaryFormatter();
                database.IsTemporary = false;
                database.OnBeforeSerialization();
                binaryFormatter.Serialize(fileStream, database);
                fileStream.Close();
            }
        }
    }
}