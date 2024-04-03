using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnitySimplified.Serialization.Formatters;

namespace UnitySimplified.Serialization
{
    internal sealed class DataManagerInternal
    {
        [Serializable]
        internal class File
        {
            [SerializeField]
            private string identifier;
            [SerializeField]
            private string name;
            [SerializeField]
            private string path;
            [SerializeField]
            private string formatter;

            [NonSerialized]
            private IDataFormatter _formatter;

            public string Identifier => identifier;
            public string Name => name;
            public string Path => path;
            public IDataFormatter Formatter
            {
                get
                {
                    if (_formatter == null)
                    {
                        if (string.IsNullOrEmpty(formatter))
                            return null;

                        Type fileFormatterType = Type.GetType(formatter) ?? throw new NullReferenceException();
                        _formatter = (IDataFormatter)Activator.CreateInstance(fileFormatterType);
                    }
                    return _formatter;
                }
            }
            public string FullPath => System.IO.Path.Combine(Path, Name);

            public File(string identifier, string name, string path, IDataFormatter formatter)
            {
                this.identifier = identifier;
                this.name = name;
                this.path = path;
                this.formatter = formatter.GetType().AssemblyQualifiedName;
                _formatter = formatter;
            }
        }
        [Serializable]
        internal class FileDatabase
        {
            [SerializeField]
            private string filePath;
            [SerializeField]
            private string fileName;
            [SerializeField]
            private List<File> fileEntries;

            [NonSerialized]
            private readonly List<File> _tempFiles;
            [NonSerialized]
            private readonly Dictionary<string, File> _cachedFileEntriesByIDs;

            public string FilePath => filePath;
            public string FileName => fileName;
            public string FullFilePath => Path.Combine(filePath, fileName);

            public FileDatabase(string filePath, string fileName)
            {
                this.filePath = filePath;
                this.fileName = fileName;
                fileEntries = new List<File>();
                _tempFiles = new List<File>();
                _cachedFileEntriesByIDs = new Dictionary<string, File>();
            }

            public void VerifyFiles(List<File> filesModified, List<File> filesAbsent)
            {
                _tempFiles.Clear();
                foreach (File file in _cachedFileEntriesByIDs.Values)
                    _tempFiles.Add(file);

                if (filesAbsent != null)
                {
                    filesAbsent.Clear();
                    for (int i = _tempFiles.Count - 1; i >= 0; i--)
                    {
                        if (System.IO.File.Exists(_tempFiles[i].FullPath))
                            continue;

                        filesAbsent.Add(_tempFiles[i]);
                        _tempFiles.RemoveAt(i);
                    }
                }
                filesModified?.Clear();
            }
            public void AddFileEntry(File file)
            {
                if (!_cachedFileEntriesByIDs.TryAdd(file.Identifier, file))
                    throw new Exception($"Already contains {nameof(file.Identifier)}: {file.Identifier}");
            }
            public void RemoveFileEntry(File file)
            {
                if (!_cachedFileEntriesByIDs.Remove(file.Identifier))
                    throw new Exception($"Does not contain {nameof(file.Identifier)}: {file.Identifier}");
            }

            public bool TryGetFileEntry(string fileIdentifier, out File file) => _cachedFileEntriesByIDs.TryGetValue(fileIdentifier, out file);

            public void OnBeforeSerialization()
            {
                fileEntries.Clear();
                foreach (var file in _cachedFileEntriesByIDs.Values)
                    fileEntries.Add(file);
            }
            public void OnAfterDeserialization()
            {
                _cachedFileEntriesByIDs.Clear();
                foreach (var file in fileEntries)
                    _cachedFileEntriesByIDs.Add(file.Identifier, file);
            }
        }
        internal class FileDirectory
        {
            [NonSerialized]
            private const string DatabaseName = "Database.DAT";
            [NonSerialized]
            private Dictionary<string, FileDatabase> _cachedDatabasesByFullPaths;

            internal bool Contains(string databaseDirectoryPath) => _cachedDatabasesByFullPaths != null && _cachedDatabasesByFullPaths.ContainsKey(Path.Combine(databaseDirectoryPath, DatabaseName));
            internal FileDatabase GetDatabase(string databaseDirectoryPath)
            {
                string fullDatabasePath = Path.Combine(databaseDirectoryPath, DatabaseName);
                _cachedDatabasesByFullPaths ??= new Dictionary<string, FileDatabase>();
                if (_cachedDatabasesByFullPaths.TryGetValue(fullDatabasePath, out FileDatabase fileDatabase))
                    return fileDatabase;
                else
                {
                    if (System.IO.File.Exists(fullDatabasePath))
                    {
                        fileDatabase = new FileDatabase("", "");
                        var fileStream = new FileStream(fullDatabasePath, FileMode.Open);
                        var binaryFormatter = new BinaryFormatter();
                        DataManagerUtility.OverwriteInstanceFromOther((FileDatabase)binaryFormatter.Deserialize(fileStream), fileDatabase);
                        fileDatabase.OnAfterDeserialization();
                        fileStream.Close();
                    }
                    else
                    {
                        if (!Directory.Exists(databaseDirectoryPath))
                            Directory.CreateDirectory(databaseDirectoryPath);
                        fileDatabase = new FileDatabase(databaseDirectoryPath, DatabaseName);
                        var fileStream = new FileStream(fileDatabase.FullFilePath, FileMode.Create);
                        var binaryFormatter = new BinaryFormatter();
                        fileDatabase.OnBeforeSerialization();
                        binaryFormatter.Serialize(fileStream, fileDatabase);
                        fileStream.Close();
                    }
                    return fileDatabase;
                }
            }
            internal void OverwriteDatabase(FileDatabase fileDatabase)
            {
                var fileStream = new FileStream(fileDatabase.FullFilePath, FileMode.Create);
                var binaryFormatter = new BinaryFormatter();
                fileDatabase.OnBeforeSerialization();
                binaryFormatter.Serialize(fileStream, fileDatabase);
                fileStream.Close();
            }
        }
    }
}