using System;
using System.IO;
using System.Xml.Serialization;

namespace UnitySimplified.Serialization.Formatters
{
    /// <summary>
    /// Needs to be worked on
    /// </summary>
    public class XmlDataFormatter : IDataFormatter
    {
        public string FileExtension => ".xml";

        public void SerializeToFile<T>(string filePath, T instance)
        {
            using FileStream stream = new(filePath, FileMode.OpenOrCreate);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            xmlSerializer.Serialize(stream, instance);
        }
        public void DeserializeFromFile<T>(string filePath, T instance)
        {
            using FileStream stream = new(filePath, FileMode.Open);
            XmlSerializer xmlSerializer = new XmlSerializer(instance.GetType());
            SerializationUtility.OverwriteInstanceFromOther((T)xmlSerializer.Deserialize(stream), instance);
        }
        public void SerializeToString<T>(T instance, out string instanceData)
        {
            using MemoryStream stream = new();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            xmlSerializer.Serialize(stream, instance);
            instanceData = Convert.ToBase64String(stream.ToArray());
        }

        public void DeserializeFromString<T>(T instance, string instanceData)
        {
            using MemoryStream stream = new(Convert.FromBase64String(instanceData));
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            SerializationUtility.OverwriteInstanceFromOther((T)xmlSerializer.Deserialize(stream), instance);
        }
    }
}