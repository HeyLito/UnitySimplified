namespace UnitySimplified.Serialization
{
    public interface IDataManagerFormatter
    {
        public string FileExtension { get; }
        public void SerializeToFile<T>(string filePath, T instance);
        public void DeserializeFromFile<T>(string filePath, T instance);
        public void SerializeToString<T>(T instance, out string instanceData);
        public void DeserializeFromString<T>(T instance, string instanceData);
    }
}