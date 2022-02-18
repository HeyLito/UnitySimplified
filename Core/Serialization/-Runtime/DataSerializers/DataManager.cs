using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;
using Newtonsoft.Json;

namespace UnitySimplified.Serialization
{
    public enum FileFormat { JSON = default, Binary, Custom }

    public static class DataManager
    {
        private class FileInfoContainer
        {
            public readonly FileFormat fileFormat;
            public readonly string fileExtension;
            public readonly List<FileInfo> files = new List<FileInfo>();

            private readonly string _jsonExtension = ".json";
            private readonly string _binaryExtension = ".sav";

            public FileInfoContainer(FileFormat fileFormat)
            {
                this.fileFormat = fileFormat;
                switch (fileFormat)
                {
                    case FileFormat.JSON:
                        fileExtension = _jsonExtension;
                        break;
                    case FileFormat.Binary:
                        fileExtension = _binaryExtension;
                        break;
                }
            }

            public bool Contains(string path) 
            {
                var filesHashSet = new HashSet<string>();
                for (int i = 0; i < files.Count; i++)
                    filesHashSet.Add(files[i].FullName);
                return filesHashSet.Contains(path);
            }
        }

        #region FIELDS
        public static bool debug = false;
        public static string persistentDirectoryPath = Application.persistentDataPath;
        public static string subDirectory = "User Data";

        private const string _pathColor = "#80E6FF";
        private static Dictionary<FileFormat, FileInfoContainer> _fileContainers = new Dictionary<FileFormat, FileInfoContainer>();
        private static SurrogateSelector _surrogateSelector;
        #endregion

        #region PROPERTIES
        public static string DefaultPath { get { string path = Path.Combine(persistentDirectoryPath, subDirectory); if (!Directory.Exists(path)) Directory.CreateDirectory(path); return path; } }
        public static string TargetDataPath { get => PlayerPrefs.GetString("TargetDataPath", DefaultPath); set => PlayerPrefs.SetString("TargetDataPath", value); }
        public static SurrogateSelector SurrogateSelector
        {
            get
            {
                if (_surrogateSelector == null)
                {
                    _surrogateSelector = new SurrogateSelector();
                    ColorSurrogate colorSurrogate = new ColorSurrogate();
                    Vector2Surrogate vector2Surrogate = new Vector2Surrogate();
                    Vector3Surrogate vector3Surrogate = new Vector3Surrogate();
                    QuaternionSurrogate quaternionSurrogate = new QuaternionSurrogate();

                    _surrogateSelector.AddSurrogate(typeof(Color), new StreamingContext(StreamingContextStates.All), colorSurrogate);
                    _surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2Surrogate);
                    _surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3Surrogate);
                    _surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quaternionSurrogate);
                }
                return _surrogateSelector;
            }
        }
        #endregion

        #region METHODS
        private static void DoLoadFileDatabase(bool clearDatabases, params FileFormat[] fileFormats)
        {
            if (!Directory.Exists(TargetDataPath)) 
            {
                RunDebugger(00);
                return;
            }

            DirectoryInfo targetDirectory = new DirectoryInfo(TargetDataPath);
            List<string> allDirectories = new List<string>() { targetDirectory.FullName };
            List<FileInfo> newFiles = new List<FileInfo>();
            List<FileInfoContainer> openFileContainers = new List<FileInfoContainer>();
            HashSet<FileFormat> closedFileFormats = new HashSet<FileFormat>();


            allDirectories.AddRange(Directory.GetDirectories(targetDirectory.FullName, "*", SearchOption.AllDirectories));

            for (int i = 0; i < fileFormats.Length; i++)
            {
                if (closedFileFormats.Contains(fileFormats[i]))
                    continue;

                if (_fileContainers.TryGetValue(fileFormats[i], out FileInfoContainer targetContainer)) 
                {
                    if (clearDatabases)
                        targetContainer.files.Clear();
                }
                else 
                {
                    targetContainer = new FileInfoContainer(fileFormats[i]);
                    _fileContainers.Add(targetContainer.fileFormat, targetContainer);
                }
                openFileContainers.Add(targetContainer);
                closedFileFormats.Add(fileFormats[i]);
            }

            for (int i = 0; i < allDirectories.Count; i++)
                for (int j = 0; j < openFileContainers.Count; j++)
                {
                    if (string.IsNullOrEmpty(openFileContainers[j].fileExtension))
                        continue;

                    FileInfo[] filesfromIndexDirectory = new DirectoryInfo(allDirectories[i]).GetFiles($"*{openFileContainers[j].fileExtension}");
                    newFiles.AddRange(filesfromIndexDirectory);
                    openFileContainers[j].files.AddRange(filesfromIndexDirectory);
                }
            RunDebugger(01, newFiles, openFileContainers);
        }

        /// <summary>
        /// Sets TargetDataPath back to default value.
        /// </summary>
        public static void ResetTargetDataPath()
        {   TargetDataPath = DefaultPath;   }

        /// <summary>
        /// Loads all files of applicable <see cref="FileFormat"/> within the <see cref="TargetDataPath"/> directory.
        /// </summary>
        /// <param name="clearDatabases">
        /// </param>
        public static void LoadAllFileDatabases(bool clearDatabases = false)
        {   DoLoadFileDatabase(clearDatabases, FileFormat.JSON, FileFormat.Binary, FileFormat.Custom);   }

        /// <summary>
        /// Loads all files of a target FileFormat within the TargetDataPath directory.
        /// </summary>
        /// <param name="fileFormat">
        /// </param>
        /// <param name="clearDatabases">
        /// </param>
        public static void LoadFileDatabase(FileFormat fileFormat, bool clearDatabases = false)
        {   DoLoadFileDatabase(clearDatabases, fileFormat);   }

        /// <summary>
        /// Creates a new file from the object's data and inserts it into current loaded databases.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="obj"></param>
        /// <param name="fileFormat"></param>
        /// <returns></returns>
        public static bool CreateNewFile<T>(string fileName, T obj, FileFormat fileFormat)
        {   return CreateNewFile(fileName, null, obj, fileFormat);   }

        /// <summary>
        /// Creates a new file from the object's data and inserts it into current loaded databases.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="obj"></param>
        /// <param name="fileFormat"></param>
        /// <param name="subPath"></param>
        /// <returns></returns>
        public static bool CreateNewFile<T>(string fileName, string subPath, T obj, FileFormat fileFormat)
        {
            if (!_fileContainers.TryGetValue(fileFormat, out FileInfoContainer container))
            {
                RunDebugger(10, fileName, fileFormat);
                return false;
            }

            FileInfo fileInfo = new FileInfo(Path.Combine(TargetDataPath, fileName + container.fileExtension));
            if (Directory.Exists(fileInfo.Directory.FullName))
            {
                if (!string.IsNullOrEmpty(subPath))
                {
                    fileInfo = new FileInfo(Path.Combine(TargetDataPath, subPath, fileName + container.fileExtension));
                    if (!Directory.Exists(fileInfo.Directory.FullName))
                        Directory.CreateDirectory(fileInfo.Directory.FullName);
                }
                if (!File.Exists(fileInfo.FullName))
                {
                    switch (fileFormat)
                    {
                        case FileFormat.JSON:
                            #region IF USING NEWTONSOFT
                            string dataAsJson = JsonConvert.SerializeObject(obj, typeof(T), Formatting.Indented, null);
                            #endregion
                            #region IF NOT
                            //string dataAsJson = JsonUtility.ToJson(obj, true);
                            #endregion
                            File.WriteAllText(fileInfo.FullName, dataAsJson);
                            container.files.Add(fileInfo);
                            break;

                        case FileFormat.Binary:
                            FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Create);
                            BinaryFormatter binaryFormatter = new BinaryFormatter();
                            binaryFormatter.SurrogateSelector = SurrogateSelector;
                            binaryFormatter.Serialize(fileStream, obj);
                            fileStream.Close();
                            container.files.Add(fileInfo);
                            break;
                    }
                    RunDebugger(13, fileName, fileInfo);
                    return true;
                }
                else
                {
                    RunDebugger(12, fileName, fileInfo);
                    return false;
                }

            }
            else
            {
                RunDebugger(11, fileName, fileInfo);
                return false;
            }
        }

        /// <summary>
        /// Searches the loaded databases for an existing file with the identical name and overwrites the discovered file from the object data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool SaveToFile<T>(string fileName, T obj)
        {
            if (FindFileFromDatabase(fileName, out FileInfoContainer container, out FileInfo fileInfo))
            {
                switch (container.fileFormat)
                {
                    case FileFormat.JSON:
                        #region IF USING NEWTONSOFT
                        string dataAsJson = JsonConvert.SerializeObject(obj, typeof(T), Formatting.Indented, null);
                        #endregion
                        #region IF NOT
                        //string dataAsJson = JsonUtility.ToJson(obj, true);
                        #endregion
                        File.WriteAllText(fileInfo.FullName, dataAsJson);
                        break;

                    case FileFormat.Binary:
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Create);
                        binaryFormatter.SurrogateSelector = SurrogateSelector;
                        binaryFormatter.Serialize(fileStream, obj);
                        fileStream.Close();
                        break;
                }

                RunDebugger(21, fileName, fileInfo);
                return true;
            }

            RunDebugger(20, fileName);
            return false;
        }

        /// <summary>
        /// Searches the loaded databases for an existing file with the identical name and overwrites the object from the discovered file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool LoadFromFile<T>(string fileName, T obj)
        {
            if (FindFileFromDatabase(fileName, out FileInfoContainer container, out FileInfo fileInfo))
            {
                switch (container.fileFormat)
                {
                    case FileFormat.JSON:
                        string dataFromJson = File.ReadAllText(fileInfo.FullName);
                        #region IF USING NEWTONSOFT
                        FromObjectOverwrite(JsonConvert.DeserializeObject(dataFromJson, typeof(T)), obj);
                        #endregion
                        #region IF NOT
                        //JsonUtility.FromJsonOverwrite(dataFromJson, obj);
                        #endregion
                        break;

                    case FileFormat.Binary:
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open);
                        binaryFormatter.SurrogateSelector = SurrogateSelector;
                        FromObjectOverwrite((T)binaryFormatter.Deserialize(fileStream), obj);
                        fileStream.Close();
                        break;
                }

                RunDebugger(31, fileName, fileInfo);
                return true;
            }

            RunDebugger(30, fileName);
            return false;
        }

        /// <summary>
        /// Searches the loaded databases for an existing file with the identical name and removes it from the filesystem.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool DeleteFile(string fileName)
        {
            if (FindFileFromDatabase(fileName, out FileInfoContainer container, out FileInfo fileInfo))
            {
                for (int i = container.files.Count - 1; i >= 0; i--)
                    if (container.files[i] == fileInfo) 
                    {
                        container.files.RemoveAt(i);
                        i = 0;
                    }
                File.Delete(fileInfo.FullName);
                RunDebugger(41, fileName, fileInfo);
                return true;
            }

            RunDebugger(40, fileName);
            return false;
        }

        public static string SaveFileAsString<T>(T file, FileFormat fileFormat)
        {
            string data = "";
            switch (fileFormat) 
            {
                case FileFormat.JSON:
                    data = JsonUtility.ToJson(file, false);
                    return data;

                case FileFormat.Binary:
                    using (MemoryStream stream = new MemoryStream()) 
                    {
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        binaryFormatter.SurrogateSelector = SurrogateSelector;
                        binaryFormatter.Serialize(stream, file);
                        data = Convert.ToBase64String(stream.ToArray());
                        stream.Close();
                        return data;
                    }
                default:
                    return data;
            }
        }
        public static void LoadFileFromString<T>(T file, string data, FileFormat fileFormat)
        {
            switch (fileFormat)
            {
                case FileFormat.JSON:
                    JsonUtility.FromJsonOverwrite(data, file);
                    return;

                case FileFormat.Binary:
                    if (!string.IsNullOrEmpty(data))
                    {
                        var buffer = Convert.FromBase64String(data);
                        if (buffer != null)
                            using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(data)))
                            {
                                BinaryFormatter binaryFormatter = new BinaryFormatter();
                                binaryFormatter.SurrogateSelector = SurrogateSelector;
                                FromObjectOverwrite((T)binaryFormatter.Deserialize(stream), file);
                                stream.Close();
                            }
                    }
                    return;
            }
        }

        #region UTILITY
        public static bool ContainsFile(string fileName, out string filePath)
        {
            filePath = "";
            if (FindFileFromDatabase(fileName, out _, out FileInfo fileInfo) && fileInfo.Exists)
            {
                filePath = fileInfo.FullName;
                return true;
            }
            else return false;
        }
        private static bool FindFileFromDatabase(string fileName, out FileInfoContainer container, out FileInfo fileInfo)
        {
            foreach (var pair in _fileContainers)
                for (int i = 0; i < pair.Value.files.Count; i++)
                    if (pair.Value.files[i].Name == $"{fileName}{pair.Value.fileExtension}")
                    {
                        container = pair.Value;
                        fileInfo = pair.Value.files[i];
                        return true;
                    }

            container = null;
            fileInfo = null;
            return false;
        }
        private static void FromObjectOverwrite<T>(T loadedObject, T objectToOverwrite)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetField;
            FieldInfo[] loadedFields = loadedObject.GetType().GetFields(flags);
            FieldInfo[] overwriteFields = objectToOverwrite.GetType().GetFields(flags);
            for (int i = 0; i < loadedFields.Length; i++)
            {
                if (loadedFields[i].Attributes.HasFlag(FieldAttributes.NotSerialized)) 
                    continue;
                object value = loadedFields[i].GetValue(loadedObject);
                overwriteFields[i].SetValue(objectToOverwrite, value);
            }
        }
        private static void RunDebugger(int code, params object[] parameters) 
        {
            if (!debug)
                return;

            string output = "";
            switch (code)
            {
                #region CASE_00_LOADFILEDATABASES_DEBUGGER
                case 00:
                    output = $"Attempt to load file database from <b>TargetDataPath</b>," +
                        $"   <color=yellow><b><i>FAILED</i></b></color>" +
                        $"   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n" +
                        $"   Target path does not exist!\n" +
                        $"   Path: <color={_pathColor}>{TargetDataPath}</color>\n";
                    break;
                #endregion

                #region CASE_01_LOADFILEDATABASES_DEBUGGER
                case 01:
                    var newFiles = (List<FileInfo>)parameters[0];
                    if (newFiles.Count == 0)
                        output = $"Attempt to load file database from <b>TargetDataPath</b>," +
                        $"   <color=green><b><i>SUCCEEDED</i></b></color>" +
                        $"   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n" +
                        $"   Found 0 new files from path.\n" +
                        $"   Path: <color={_pathColor}>{TargetDataPath}</color>\n";
                    else
                    {
                        List<FileInfoContainer> fileContainers = (List<FileInfoContainer>)parameters[1];
                        HashSet<FileFormat> fileFormats = new HashSet<FileFormat>();
                        int index = 0;
                        output = $"Attempt to load file database from <b>TargetDataPath</b>," +
                            $"   <color=green><b><i>SUCCEEDED</i></b></color>" +
                            $"   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n" +
                            $"   Found new files of format: ";

                        for (int i = 0; i < fileContainers.Count; i++) 
                        {
                            if (fileFormats.Contains(fileContainers[i].fileFormat))
                                continue;
                            for (int j = 0; j < newFiles.Count; j++)
                                if (fileContainers[i].Contains(newFiles[j].FullName))
                                {
                                    fileFormats.Add(fileContainers[i].fileFormat);
                                    break;
                                }
                        }

                        foreach (var format in fileFormats)
                        {
                            output += $"<b>{Enum.GetName(typeof(FileFormat), format)}</b>";
                            if (fileFormats.Count > 1 && index + 1 < fileFormats.Count) 
                                if (index + 1 == fileFormats.Count - 1)
                                    output += " and ";
                                else 
                                    output += ", ";
                            index++;
                        }

                        output += $" within the directories under <b>TargetDataPath</b>.\n";
                        for(int i =0; i < newFiles.Count; i++)
                            output += $"      <color={_pathColor}>*{Path.DirectorySeparatorChar}{newFiles[i].Directory.Name}{newFiles[i].FullName.Remove(0, TargetDataPath.Length)}</color>\n";
                        output += $"   Path: <color={_pathColor}>{TargetDataPath}</color>\n"; ;
                    }
                    break;
                #endregion

                    

                #region CASE_10_CREATENEWFILE_DEBUGGER
                case 10:
                    output = $"Attempt to create file: <b>{parameters[0]}</b>," +
                        $"   <color=yellow><b><i>FAILED</i></b></color>" +
                        $"   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n" +
                        $"   Failed to locate an existing <b>{Enum.GetName(typeof(FileFormat), parameters[1]).ToLower()}</b> formated file database!\n";
                    break;
                #endregion

                #region CASE_11_CREATENEWFILE_DEBUGGER
                case 11:
                    output = $"Attempt to create file: <b>{parameters[0]}</b>," +
                        $"   <color=yellow><b><i>FAILED</i></b></color>" +
                        $"   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n" +
                        $"   Current target path does not exist!\n" +
                        $"   Path: <color={_pathColor}>{(parameters[1] as FileInfo).Directory.FullName}</color>\n";
                    break;
                #endregion

                #region CASE_12_CREATENEWFILE_DEBUGGER
                case 12:
                    output = $"Attempt to create file: <b>{parameters[0]}</b>," +
                        $"   <color=yellow><b><i>FAILED</i></b></color>" +
                        $"   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n" +
                        $"   A file already exists at path!\n" +
                        $"   Path: <color={_pathColor}>{(parameters[1] as FileInfo).FullName}</color>\n";
                    break;
                #endregion

                #region CASE_13_CREATENEWFILE_DEBUGGER
                case 13:
                    output = $"Attempt to create file: <b>{parameters[0]}</b>," +
                        $"   <color=green><b><i>SUCCEEDED</i></b></color>" +
                        $"   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n" +
                        $"   Path: <color={_pathColor}>{(parameters[1] as FileInfo).FullName}</color>\n";
                    break;
                #endregion



                #region CASE_20_SAVEFILE_DEBUGGER
                case 20:
                    output = $"Attempt to save file: <b>{parameters[0]}</b>," +
                        $"   <color=yellow><b><i>FAILED</i></b></color>" +
                        $"   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n" +
                        $"   File <b>{parameters[0]}</b> was not found within current file databases!\n";
                    break;
                #endregion

                #region CASE_21_SAVEFILE_DEBUGGER
                case 21:
                    output = $"Attempt to save file: <b>{parameters[0]}</b>," +
                        $"   <color=green><b><i>SUCCEEDED</i></b></color>" +
                        $"   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n" +
                        $"   Path: <color={_pathColor}>{(parameters[1] as FileInfo).FullName}</color>\n";
                    break;
                #endregion



                #region CASE_30_LOADFILE_DEBUGGER
                case 30:
                    output = $"Attempt to load file: <b>{parameters[0]}</b>," +
                        $"   <color=yellow><b><i>FAILED</i></b></color>" +
                        $"   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n" +
                        $"   File <b>{parameters[0]}</b> was not found within current file databases!\n";
                    break;
                #endregion

                #region CASE_31_LOADFILE_DEBUGGER
                case 31:
                    output = $"Attempt to load file: <b>{parameters[0]}</b>," +
                        $"   <color=green><b><i>SUCCEEDED</i></b></color>" +
                        $"   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n" +
                        $"   Path: <color={_pathColor}>{(parameters[1] as FileInfo).FullName}</color>\n";
                    break;
                #endregion



                #region CASE_40_DELETEFILE_DEBUGGER
                case 40:
                    output = $"Attempt to delete file: <b>{parameters[0]}</b>," +
                        $"   <color=yellow><b><i>FAILED</i></b></color>" +
                        $"   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n" +
                        $"   File <b>{parameters[0]}</b> was not found within current file databases!\n";
                    break;
                #endregion

                #region CASE_41_DELETEFILE_DEBUGGER
                case 41:
                    output = $"Attempt to delete file: <b>{parameters[0]}</b>," +
                        $"   <color=green><b><i>SUCCEEDED</i></b></color>" +
                        $"   <color=#ba95d0>Frame Count: {Time.frameCount}</color>\n" +
                        $"   Path: <color={_pathColor}>{(parameters[1] as FileInfo).FullName}</color>\n";
                    break;
                #endregion

                default:
                    break;
            }
            Debug.Log(output);
        }
        #endregion
        #endregion
    }
}