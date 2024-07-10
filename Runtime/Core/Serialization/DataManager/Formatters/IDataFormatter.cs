namespace UnitySimplified.Serialization.Formatters
{
    public interface IDataFormatter
    {
        public string FileExtension { get; }
        public void SerializeToFile<T>(string filePath, T instance);
        public void DeserializeFromFile<T>(string filePath, T instance);
        public void SerializeToString<T>(T instance, out string instanceData);
        public void DeserializeFromString<T>(T instance, string instanceData);
    }
}