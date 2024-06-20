using System;
using UnityEngine;

namespace UnitySimplified.GamePrefs
{
    [Serializable]
    public sealed partial class GamePref
    {
        [SerializeField]
        private string identifier = "";



        private GamePref() { }
        private GamePref(string identifier) => this.identifier = identifier;



        public string Identifier => identifier;
        


        public override bool Equals(object obj) => obj switch
        {
            GamePref rhs => ReferenceEquals(this, rhs),
            null => false,
            _ => false
        };
        public static bool operator ==(GamePref lhs, GamePref rhs)
        {
            if (rhs is null)
                return lhs is null;
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (string.IsNullOrEmpty(lhs.Identifier))
                return false;
            if (string.IsNullOrEmpty(rhs.Identifier))
                return false;
            return lhs.Identifier.Equals(rhs.Identifier);
        }

        public static bool operator !=(GamePref lhs, GamePref rhs)
        {
            if (rhs is null)
                return lhs is not null;
            if (ReferenceEquals(lhs, rhs))
                return false;
            if (string.IsNullOrEmpty(lhs.Identifier))
                return false;
            if (string.IsNullOrEmpty(rhs.Identifier))
                return false;
            return !lhs.Identifier.Equals(rhs.Identifier);
        }

        public override int GetHashCode() => 0;
        public T GetValue<T>(T defaultValue = default)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return default;
#endif
            if (this == Empty)
                return default;

            if (!_loaded)
                Reload();

            return DoGetValueFromIdentifier(identifier, defaultValue);
        }
        public void SetValue<T>(T value)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            if (this == Empty)
                return;

            if (!_loaded)
                Reload();

            DoSetValueWithIdentifier(identifier, value);
            Overwrite();
        }
    }
}