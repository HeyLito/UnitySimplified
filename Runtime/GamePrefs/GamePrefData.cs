using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace UnitySimplified.GamePrefs
{
    [Serializable]
    [XmlRoot(Namespace = "")]
    [DataContract(Namespace = "")]
    public class GamePrefData
    {
        [DataMember(Name = nameof(identifier), IsRequired = true)]
        public string identifier;
        [DataMember(Name = nameof(key), IsRequired = true)]
        public string key;
        [DataMember(Name = nameof(value), IsRequired = true)]
        public object value;
        [DataMember(Name = nameof(valueTypeNamespace), IsRequired = true)]
        public string valueTypeNamespace;

        public GamePrefData() { }
        public GamePrefData(GamePrefData data) : this(data.identifier, data.key, data.value) { }
        public GamePrefData(string identifier, string key, object value)
        {
            this.identifier = identifier;
            this.key = key;
            this.value = value;
            valueTypeNamespace = value.GetType().AssemblyQualifiedName;
        }

        public bool IsValid() => !string.IsNullOrEmpty(identifier) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(valueTypeNamespace) && value is not null;
        public Type GetValueType() => Type.GetType(valueTypeNamespace);
        public void OnGet()
        {
            if (!IsValid())
                return;

            var valueType = GetValueType();
            if (valueType.IsNumericType())
                value = Convert.ChangeType(value, valueType);
        }
        public void OnSet()
        {
            if (!IsValid())
                return;

            var valueType = GetValueType();
            if (valueType.IsNumericType())
                value = Convert.ChangeType(value, valueType);
        }
    }
}