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
        public string FileExtension => ".XML";

        public void SerializeToFile<T>(string filePath, T instance)
        {
            throw new NotImplementedException();
            using (FileStream stream = new(filePath, FileMode.OpenOrCreate))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(stream, instance);
                stream.Close();
            }
        }
        public void DeserializeFromFile<T>(string filePath, T instance)
        {
            throw new NotImplementedException();
            using (FileStream stream = new(filePath, FileMode.Open))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(instance.GetType());
                DataManagerUtility.OverwriteInstanceFromOther((T)xmlSerializer.Deserialize(stream), instance);
                stream.Close();
            }
        }
        public void SerializeToString<T>(T instance, out string instanceData)
        {
            throw new NotImplementedException();
            using (MemoryStream stream = new())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(stream, instance);
                instanceData = Convert.ToBase64String(stream.ToArray());
                stream.Close();
            }
        }

        public void DeserializeFromString<T>(T instance, string instanceData)
        {
            throw new NotImplementedException();
            using (MemoryStream stream = new(Convert.FromBase64String(instanceData)))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                DataManagerUtility.OverwriteInstanceFromOther((T)xmlSerializer.Deserialize(stream), instance);
                stream.Close();
            }
            throw new System.NotImplementedException();
        }
    }
}