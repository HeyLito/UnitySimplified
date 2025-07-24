using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnitySimplified.Serialization.Formatters;
using UnitySimplified.Serialization;

namespace UnitySimplified.GamePrefs
{
    public sealed partial class GamePref
    {
        internal static readonly GamePref Empty = new();
        private static List<GamePrefData> _savedData = new();
        private static ILookup<string, GamePrefData> _savedDataIdentifierLookup;
        private static ILookup<string, GamePrefData> _savedDataKeyLookup;
        private static bool _loaded;



        public static event Action OnValuesChanged;



        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Application.quitting += Overwrite;
            Reload();
        }

        public static void Reload() => DoReload();
        public static void Overwrite() => DoOverwrite();

        public static void DeleteAll() => DoDeleteAll();
        public static bool Delete(GamePref gamePref) => DoDeleteWithIdentifier(gamePref.identifier);
        public static bool Delete(string key) => DoDeleteWithKey(key);

        public static bool HasData(GamePref gamePref) => _savedDataIdentifierLookup[gamePref.Identifier].FirstOrDefault() != null;
        public static bool HasData(string key) => _savedDataKeyLookup[key].FirstOrDefault() != null;

        public static T GetValue<T>(string key) => DoGetValueFromKey(key, default(T));
        public static T GetValue<T>(string key, T defaultValue) => DoGetValueFromKey(key, defaultValue);
        public static void SetValue<T>(string key, T value) => DoSetValueWithKey(key, value);



        internal static IEnumerable<GamePrefData> All => _savedData;
        internal static bool Delete(GamePrefData data) => DoDeleteWithIdentifier(data.identifier);
        internal static bool TryGetIdentifierFromKey(string identifier, out string key)
        {
            var lookupResult = _savedDataIdentifierLookup[identifier];
            foreach (var data in lookupResult)
            {
                if (data == null || !data.IsValid())
                    continue;
                key = data.key;
                return true;
            }
            key = string.Empty;
            return false;
        }

        private static void DoReload()
        {
            _loaded = true;
            _savedData.Clear();
            _savedDataIdentifierLookup = null;
            _savedDataKeyLookup = null;

            if (!FileDatabase.TryGetDatabase(DataManager.DefaultPath, out FileDatabase fileDatabase))
            {
                fileDatabase = new FileDatabase(DataManager.DefaultPath);
                fileDatabase.SaveDatabase();
            }
            fileDatabase.LoadFromFile("GamePrefs", _savedData);

            _savedDataIdentifierLookup = _savedData.ToLookup(x => x.identifier);
            _savedDataKeyLookup = _savedData.ToLookup(x => x.key);

            OnValuesChanged?.Invoke();
        }

        private static void DoOverwrite()
        {
            string key = "GamePrefs";
            FileDatabase fileDatabase = FileDatabase.GetDatabase(DataManager.DefaultPath);
            if (!fileDatabase.SaveToFile("GamePrefs", _savedData))
            {
                //Find an solution for when newtonsoft is enabled after having existing saved data using the Binary data formatter.
#if ENABLE_UNITYSIMPLIFIED_NEWTONSOFT
                fileDatabase.CreateNewFile(new JsonDataFormatter(), _savedData, key, key);
#else
                fileDatabase.CreateNewFile(new BinaryDataFormatter(), _savedData, key, key);
#endif
                fileDatabase.SaveDatabase();
            }
            OnValuesChanged?.Invoke();
        }

        private static void DoDeleteAll()
        {
            _savedDataIdentifierLookup = default;
            _savedDataKeyLookup = default;
            OnValuesChanged?.Invoke();
        }
        private static bool DoDeleteWithIdentifier(string identifier)
        {
            if (!_loaded)
                Reload();

            if (!_savedDataIdentifierLookup.Contains(identifier))
                return false;

            _savedData.RemoveAll(x => x.identifier == identifier);
            _savedDataIdentifierLookup = _savedData.ToLookup(x => x.identifier);
            _savedDataKeyLookup = _savedData.ToLookup(x => x.key);
            OnValuesChanged?.Invoke();
            return true;
        }
        private static bool DoDeleteWithKey(string key)
        {
            if (!_loaded)
                Reload();

            if (!_savedDataKeyLookup.Contains(key))
                return false;

            _savedData.RemoveAll(x => x.key == key);
            _savedDataIdentifierLookup = _savedData.ToLookup(x => x.identifier);
            _savedDataKeyLookup = _savedData.ToLookup(x => x.key);
            OnValuesChanged?.Invoke();
            return true;
        }

        internal static bool DoTryGetGamePrefFromIdentifier(string identifier, out GamePrefData value)
        {
            if (!_loaded)
                Reload();

            value = _savedDataIdentifierLookup[identifier].FirstOrDefault();
            return value != null;
        }
        internal static bool DoTryGetGamePrefFromKey(string key, out GamePrefData value)
        {
            if (!_loaded)
                Reload();

            if (_savedDataKeyLookup == null)
            {
                value = null;
                return false;
            }
            value = _savedDataKeyLookup[key].FirstOrDefault();
            return value != null;
        }

        private static T DoGetValueFromIdentifier<T>(string identifier, T defaultValue)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentException($"Method parameter \'{nameof(identifier)}\' is empty.");

            if (!_loaded)
                Reload();

            GamePrefData data = _savedDataIdentifierLookup[identifier].FirstOrDefault();
            if (data == null)
                GamePrefLocalDatabase.Instance.TryGetFromIdentifier(identifier, out data);
            if (data != null && data.IsValid() && typeof(T).IsAssignableFrom(data.GetValueType()))
            {
                data.OnGet();
                return (T)data.value;
            }
            return defaultValue;
        }
        private static T DoGetValueFromKey<T>(string key, T defaultValue)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"Method parameter \'{nameof(identifier)}\' is empty.");

            if (!_loaded)
                Reload();

            GamePrefData data = _savedDataKeyLookup[key].FirstOrDefault();
            if (data == null)
                GamePrefLocalDatabase.Instance.TryGetFromIdentifier(key, out data);

            if (data != null && data.IsValid() && typeof(T).IsAssignableFrom(data.GetValueType()))
            {
                data.OnGet();
                return (T)data.value;
            }
            return defaultValue;
        }

        private static void DoSetValueWithIdentifier<T>(string identifier, T value)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentException($"Method parameter \'{nameof(identifier)}\' is empty.");

            if (!_loaded)
                Reload();

            GamePrefData data = _savedDataIdentifierLookup[identifier].FirstOrDefault();
            if (data != null)
            {
                if (data.IsValid() == false || !typeof(T).IsAssignableFrom(data.GetValueType()))
                    data = null;
            }
            else if (GamePrefLocalDatabase.Instance.TryGetFromIdentifier(identifier, out data))
            {
                if (data.IsValid() && typeof(T).IsAssignableFrom(data.GetValueType()))
                {
                    data = new GamePrefData(data);
                    _savedData.Add(data);
                    _savedDataIdentifierLookup = _savedData.ToLookup(x => x.identifier);
                    _savedDataKeyLookup = _savedData.ToLookup(x => x.key);
                }
                else data = null;
            }

            if (data == null)
                return;

            data.value = value;
            OnValuesChanged?.Invoke();
        }
        private static void DoSetValueWithKey<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"Method parameter \'{nameof(identifier)}\' is empty.");
            if (value is null)
                return;

            if (!_loaded)
                Reload();


            GamePrefData data = _savedDataKeyLookup[key].FirstOrDefault();
            if (data != null)
            {
                if (data.IsValid() == false || !typeof(T).IsAssignableFrom(data.GetValueType()))
                    data = null;
            }
            else
            {
                if(GamePrefLocalDatabase.Instance.TryGetFromIdentifier(key, out data))
                    data = data != null && data.IsValid() && data.GetValueType() == typeof(T) ? new GamePrefData(data) : null;
                else data = new GamePrefData(Create().Identifier, key, value);
                _savedData.Add(data);
                _savedDataIdentifierLookup = _savedData.ToLookup(x => x.identifier);
                _savedDataKeyLookup = _savedData.ToLookup(x => x.key);
            }

            if (data == null)
                return;
            data.value = value;
            OnValuesChanged?.Invoke();
        }

        internal static GamePref Create()
        {
            string identifier;
            do identifier = Guid.NewGuid().ToString();
            while (_savedDataIdentifierLookup != null && _savedDataIdentifierLookup.Contains(identifier));
            return new GamePref(identifier);
        }
    }
}