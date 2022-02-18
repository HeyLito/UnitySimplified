using System;

namespace UnitySimplified.Serialization
{
    [Serializable]
    public class GamePrefData
    {
        public string persistentIdentifier;
        public string prefType;
        public string prefKey;
        public object prefValue;

        internal GamePrefData()
        { }
        internal GamePrefData(string persistentIdentifier, string key, object value)
        {
            this.persistentIdentifier = persistentIdentifier;
            prefKey = key;
            prefValue = value;
            prefType = prefValue.GetType().AssemblyQualifiedName;
        }
        internal GamePrefData(GamePref pref, string key, object value)
        {
            persistentIdentifier = pref.PersistentIdentifier;
            prefKey = key;
            prefValue = value;
            prefType = prefValue.GetType().AssemblyQualifiedName;
        }
        internal GamePrefData(GamePrefData data)
        {
            persistentIdentifier = data.persistentIdentifier;
            prefKey = data.prefKey;
            prefValue = data.prefValue;
            prefType = data.prefType;
        }

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

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else return !(obj as GamePrefData is null) && ReferenceEquals(this, obj);
        }
        public override int GetHashCode()
        {   return base.GetHashCode();   }
    }
}