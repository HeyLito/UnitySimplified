using System;

namespace UnitySimplified.GamePrefs
{
    [Serializable]
    internal class GamePrefData
    {
        public string identifier;
        public string key;
        public object value;
        public string valueTypeNamespace;

        internal GamePrefData() { }
        internal GamePrefData(GamePrefData data) : this(data.identifier, data.key, data.value) { }
        internal GamePrefData(string identifier, string key, object value)
        {
            this.identifier = identifier;
            this.key = key;
            this.value = value;
            valueTypeNamespace = value.GetType().AssemblyQualifiedName;
        }

        public bool IsValid() => !string.IsNullOrEmpty(identifier) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(valueTypeNamespace) && value is not null;
        public Type GetValueType() => IsValid() ? Type.GetType(valueTypeNamespace) : null;
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