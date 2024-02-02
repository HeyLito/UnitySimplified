using System.IO;

namespace UnitySimplified.Serialization.Formatters
{
    public class JsonDataFormatter : IDataFormatter
    {
        public string FileExtension => ".JSON";

        public void SerializeToFile<T>(string filePath, T instance)
        {
            string instanceData = DataManagerUtility.SerializeJsonObject(typeof(T), instance);
            File.WriteAllText(filePath, instanceData);
        }
        public void DeserializeFromFile<T>(string filePath, T instance)
        {
            string instanceData = File.ReadAllText(filePath);
            DataManagerUtility.DeserializeJsonObject(typeof(T), instanceData, instance);
        }
        public void SerializeToString<T>(T instance, out string instanceData) => instanceData = DataManagerUtility.SerializeJsonObject(typeof(T), instance);
        public void DeserializeFromString<T>(T instance, string instanceData) => DataManagerUtility.DeserializeJsonObject(typeof(T), instanceData, instance);
    }
}