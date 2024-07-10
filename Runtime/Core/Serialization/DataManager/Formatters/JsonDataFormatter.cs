using System.IO;
#if ENABLE_UNITYSIMPLIFIED_NEWTONSOFT
using Newtonsoft.Json;
#else
using UnityEngine;
#endif

namespace UnitySimplified.Serialization.Formatters
{
    public class JsonDataFormatter : IDataFormatter
    {
        public string FileExtension => ".JSON";


        public void SerializeToFile<T>(string filePath, T instance)
        {
#if ENABLE_UNITYSIMPLIFIED_NEWTONSOFT
            string instanceData = JsonConvert.SerializeObject(instance, Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, CheckAdditionalContent = true });
#else
            string instanceData = JsonUtility.ToJson(instance, true);
#endif
            File.WriteAllText(filePath, instanceData);
        }
        public void DeserializeFromFile<T>(string filePath, T instance)
        {
            string instanceData = File.ReadAllText(filePath);

#if ENABLE_UNITYSIMPLIFIED_NEWTONSOFT
            DataManagerUtility.OverwriteInstanceFromOther(JsonConvert.DeserializeObject<T>(instanceData, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, CheckAdditionalContent = true }), instance);
#else
            JsonUtility.FromJsonOverwrite(instanceData, instance);
#endif
        }

        public void SerializeToString<T>(T instance, out string instanceData)
        {
#if ENABLE_UNITYSIMPLIFIED_NEWTONSOFT
            instanceData = JsonConvert.SerializeObject(instance, Formatting.Indented);
#else
            instanceData = JsonUtility.ToJson(instance, true);
#endif
        }
        public void DeserializeFromString<T>(T instance, string instanceData)
        {
#if ENABLE_UNITYSIMPLIFIED_NEWTONSOFT
            DataManagerUtility.OverwriteInstanceFromOther(JsonConvert.DeserializeObject<T>(instanceData), instance);
#else
            JsonUtility.FromJsonOverwrite(instanceData, instance);
#endif
        }
    }
}