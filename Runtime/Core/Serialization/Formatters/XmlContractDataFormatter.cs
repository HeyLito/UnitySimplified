using System;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using UnityEngine;
using UnitySimplified.GamePrefs;
using System.Text;

namespace UnitySimplified.Serialization.Formatters
{
    public class XmlContractDataFormatter : IDataFormatter
    {
        public string FileExtension => ".xml";

        public void SerializeToFile<T>(string filePath, T instance)
        {
            using FileStream stream = new(filePath, FileMode.OpenOrCreate);
            var serializer = new DataContractSerializer(typeof(T));
            var xmlSettings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };
            using var xmlWriter = XmlWriter.Create(stream, xmlSettings);
            serializer.WriteObject(xmlWriter, instance);
        }
        public void DeserializeFromFile<T>(string filePath, T instance)
        {
            using FileStream stream = new(filePath, FileMode.Open);
            DataContractSerializer serializer = new DataContractSerializer(instance.GetType());
            SerializationUtility.OverwriteInstanceFromOther((T)serializer.ReadObject(stream), instance);
        }
        public void SerializeToString<T>(T instance, out string instanceData)
        {
            StringBuilder stringBuilder = new();
            var serializer = new DataContractSerializer(typeof(T));
            var xmlSettings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };
            using var xmlWriter = XmlWriter.Create(stringBuilder, xmlSettings);
            serializer.WriteObject(xmlWriter, instance);
            xmlWriter.Close();
            instanceData = stringBuilder.ToString();
        }

        public void DeserializeFromString<T>(T instance, string instanceData)
        {
            using StringReader stringReader = new(instanceData);
            using XmlReader xmlReader = XmlReader.Create(stringReader);
            DataContractSerializer serializer = new DataContractSerializer(instance.GetType());
            SerializationUtility.OverwriteInstanceFromOther((T)serializer.ReadObject(xmlReader), instance);
        }
    }
}