using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnitySimplified.Serialization.Formatters
{
    public class BinaryDataFormatter : IDataFormatter
    {
        public string FileExtension => ".SAV";

        public void SerializeToFile<T>(string filePath, T instance)
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            BinaryFormatter binaryFormatter = new();
            binaryFormatter.SurrogateSelector = DataManagerUtility.SurrogateSelector;
            binaryFormatter.Serialize(stream, instance);
            stream.Close();
        }
        public void DeserializeFromFile<T>(string filePath, T instance)
        {
            using var stream = new FileStream(filePath, FileMode.Open);
            BinaryFormatter binaryFormatter = new();
            binaryFormatter.SurrogateSelector = DataManagerUtility.SurrogateSelector;
            DataManagerUtility.OverwriteInstanceFromOther((T)binaryFormatter.Deserialize(stream), instance);
            stream.Close();
        }
        public void SerializeToString<T>(T instance, out string instanceData)
        {
            using var stream = new MemoryStream();
            BinaryFormatter binaryFormatter = new();
            binaryFormatter.SurrogateSelector = DataManagerUtility.SurrogateSelector;
            binaryFormatter.Serialize(stream, instance);
            instanceData = Convert.ToBase64String(stream.ToArray());
            stream.Close();
        }
        public void DeserializeFromString<T>(T instance, string instanceData)
        {
            using var stream = new MemoryStream(Convert.FromBase64String(instanceData));
            BinaryFormatter binaryFormatter = new();
            binaryFormatter.SurrogateSelector = DataManagerUtility.SurrogateSelector;
            DataManagerUtility.OverwriteInstanceFromOther((T)binaryFormatter.Deserialize(stream), instance);
            stream.Close();
        }
    }
}