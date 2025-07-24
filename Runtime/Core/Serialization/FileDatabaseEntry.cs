using System;
using System.Runtime.Serialization;
using UnityEngine;
using UnitySimplified.Serialization.Formatters;

namespace UnitySimplified.Serialization
{
    public partial class FileDatabase
    {
        [Serializable]
        [DataContract(Name = "Entry", Namespace = "")]
        internal class Entry
        {
            [SerializeField] [DataMember(Name = "Key")]
            private string key;
            [SerializeField] [DataMember(Name = "Name")]
            private string name;
            [SerializeField] [DataMember(Name = "SubDirectory")]
            private string subDirectory;
            [SerializeField] [DataMember(Name = "Formatter")]
            private string formatter;

            [NonSerialized]
            private IDataFormatter _formatter;

            public Entry(string key, string name, string subDirectory, IDataFormatter formatter)
            {
                this.key = key;
                this.name = name;
                this.subDirectory = subDirectory;
                this.formatter = formatter.GetType().AssemblyQualifiedName;
                _formatter = formatter;
            }

            public string Key => key;
            public string Name => name;
            public string SubDirectory => subDirectory;
            public IDataFormatter Formatter
            {
                get
                {
                    if (_formatter != null)
                        return _formatter;
                    if (string.IsNullOrEmpty(formatter))
                        return null;

                    return _formatter = (IDataFormatter)Activator.CreateInstance(Type.GetType(formatter) ?? throw new NullReferenceException());
                }
            }
        }
    }
}