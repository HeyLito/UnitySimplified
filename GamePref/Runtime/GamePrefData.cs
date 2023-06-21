using System;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [Serializable]
    public class GamePrefData
    {
        [SerializeField]
        private string persistentIdentifier;
        [SerializeField]
        private string prefType;
        [SerializeField]
        private string prefKey;
        [SerializeField]
        private object prefValue;

        public string PersistentIdentifier => persistentIdentifier;
        public string PrefType => prefType;
        public string PrefKey { get => prefKey; set => prefKey = value; }
        public object PrefValue { get => prefValue; set => prefValue = value; }

        internal GamePrefData() { }
        internal GamePrefData(string persistentIdentifier, string prefKey, object prefValue)
        {
            this.persistentIdentifier = persistentIdentifier;
            this.prefKey = prefKey;
            this.prefValue = prefValue;
            prefType = prefValue.GetType().AssemblyQualifiedName;
        }
        internal GamePrefData(GamePrefData data) : this(data.persistentIdentifier, data.prefKey, data.prefValue) { }

        public Type GetPrefType()
        {
            if (this == null)
                return null;
            else return Type.GetType(prefType);
        }

        public static bool operator ==(GamePrefData lhs, GamePrefData rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;

            if (lhs is null)
                return false;
            else if (string.IsNullOrEmpty(lhs.persistentIdentifier) || string.IsNullOrEmpty(lhs.prefKey) || string.IsNullOrEmpty(lhs.prefType) || lhs.prefValue is null)
                return true;

            if (rhs is null)
                return false;
            else if (string.IsNullOrEmpty(rhs.persistentIdentifier) || string.IsNullOrEmpty(rhs.prefKey) || string.IsNullOrEmpty(rhs.prefType) || rhs.prefValue is null)
                return true;

            return lhs.Equals(rhs);
        }
        public static bool operator !=(GamePrefData lhs, GamePrefData rhs) => !(lhs == rhs);
        public override int GetHashCode() => base.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else return !(obj as GamePrefData is null) && ReferenceEquals(this, obj);
        }
    }
}