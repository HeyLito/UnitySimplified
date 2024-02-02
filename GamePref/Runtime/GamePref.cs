using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnitySimplified.Serialization.Formatters;

namespace UnitySimplified.Serialization
{
    public interface IGamePrefEventListener
    {
        public void OnValueSet(object value);
    }

    [Serializable]
    public class GamePref
    {
        #region FIELDS
        [SerializeField] private string persistentIdentifier = "";
        
        private static bool _loaded = false;
        private static readonly Dictionary<string, GamePrefData> _gamePrefsByIDs = new Dictionary<string, GamePrefData>();
        private static readonly Dictionary<string, GamePrefData> _gamePrefsByKeys = new Dictionary<string, GamePrefData>();
        private static readonly Dictionary<string, HashSet<IGamePrefEventListener>> _activeListeners = new Dictionary<string, HashSet<IGamePrefEventListener>>();

        public static event Action onGamePrefUpdated;
        #endregion

        #region PROPERTIES
        public string PersistentIdentifier => persistentIdentifier;
        #endregion

        #region CONSTRUCTORS
        private GamePref() { }
        private GamePref(string persistentIdentifier) => this.persistentIdentifier = persistentIdentifier;
        #endregion

        #region METHODS-BASE_OVERRIDE
        public static bool operator ==(GamePref lhs, GamePref rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;

            if (ReferenceEquals(lhs, null))
                return false;
            else if (string.IsNullOrEmpty(lhs.persistentIdentifier))
                return true;

            if (ReferenceEquals(rhs, null))
                return false;
            else if (string.IsNullOrEmpty(rhs.persistentIdentifier))
                return true;

            return lhs.Equals(rhs);
        }
        public static bool operator !=(GamePref lhs, GamePref rhs) => !(lhs == rhs);
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var rhs = obj as GamePref;
            return rhs != null && ReferenceEquals(this, rhs);
        }
        public override int GetHashCode() => 0;
        #endregion

        #region METHODS-MAIN
        public object GetValue()
        {
            if (this == null)
                return null;

            #if UNITY_EDITOR
            if (!_loaded)
                if (Application.isPlaying)
                    Load();
                else return null;
            #else
            if (!_loaded)
                Load();
            #endif

            return DoGetValue<object>(PersistentIdentifier, null, null);
        }
        public void SetValue(object value)
        {
            if (this == null)
                return;

            #if UNITY_EDITOR
            if (!_loaded)
                if (Application.isPlaying)
                    Load();
                else return;
            #else
            if (!_loaded)
                Load();
            #endif

            DoSetValue(PersistentIdentifier, null, value);
        }

        public void RegisterListener(IGamePrefEventListener listener)
        {
            if (listener == null)
                return;
            if (!_activeListeners.TryGetValue(PersistentIdentifier, out HashSet<IGamePrefEventListener> listeners))
                _activeListeners[PersistentIdentifier] = listeners = new HashSet<IGamePrefEventListener>();
            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }
        public void UnregisterListener(IGamePrefEventListener listener)
        {
            if (listener == null)
                return;
            if (_activeListeners.TryGetValue(PersistentIdentifier, out HashSet<IGamePrefEventListener> listeners))
                if (listeners.Contains(listener))
                {
                    listeners.Remove(listener);
                    if (listeners.Count == 0)
                        _activeListeners.Remove(PersistentIdentifier);

                }
        }
        #endregion

        #region METHODS-STATIC
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Application.quitting += Save;
            Load();
        }


        public static void Load()
        {
            _gamePrefsByIDs.Clear();
            _gamePrefsByKeys.Clear();

            List<GamePrefData> gamePrefDatas = new List<GamePrefData>();

            string previousPath = DataManager.TargetDataPath;
            DataManager.TargetDataPath = DataManager.DefaultPath;
            DataManager.LoadDatabase();
            DataManager.LoadFromFile("GamePrefs", gamePrefDatas);
            DataManager.TargetDataPath = previousPath;
            

            _loaded = true;
            foreach (var item in gamePrefDatas)
            {
                _gamePrefsByIDs.Add(item.PersistentIdentifier, item);
                _gamePrefsByKeys.Add(item.PrefKey, item);
            }

            onGamePrefUpdated?.Invoke();
        }
        public static void Save()
        {
            List<GamePrefData> gamePrefDatas = new List<GamePrefData>();
            foreach (var item in _gamePrefsByIDs.Values)
                gamePrefDatas.Add(item);

            string previousPath = DataManager.TargetDataPath;
            DataManager.TargetDataPath = DataManager.DefaultPath;
            DataManager.LoadDatabase();
            if (!DataManager.SaveToFile("GamePrefs", gamePrefDatas))
                DataManager.CreateNewFile("GamePrefs", "GamePrefs", new BinaryDataFormatter(), gamePrefDatas);
            DataManager.TargetDataPath = previousPath;
            DataManager.LoadDatabase();

            onGamePrefUpdated?.Invoke();
        }
        public static void DeleteAll()
        {
            _gamePrefsByIDs.Clear();
            _gamePrefsByKeys.Clear();

            onGamePrefUpdated?.Invoke();
        }
        public static void DeleteKey(string key)
        {
            if (_gamePrefsByKeys.TryGetValue(key, out GamePrefData data))
            {
                _gamePrefsByIDs.Remove(data.PersistentIdentifier);
                _gamePrefsByKeys.Remove(data.PrefKey);

                onGamePrefUpdated?.Invoke();
            }
        }

        public static bool HasGamePref(GamePref gamePref) => HasGamePref(gamePref, out _);
        internal static bool HasGamePref(GamePref gamePref, out GamePrefData data)
        {
            data = null;
            return gamePref != null && _gamePrefsByIDs.TryGetValue(gamePref.PersistentIdentifier, out data);
        }
        public static bool HasKey(string key) => _gamePrefsByKeys.ContainsKey(key);
        public static bool HasID(string id)
        {
            if (!_loaded)
                Load();
            return _gamePrefsByIDs.ContainsKey(id);
        }

        public static T GetValue<T>(string key) => (T)DoGetValue(null, key, default(T));
        public static T GetValue<T>(string key, T defaultValue) => (T)DoGetValue(null, key, defaultValue);
        public static void SetValue<T>(string key, T value) => DoSetValue(null, key, value);

        private static object DoGetValue<T>(string id, string key, T defaultValue)
        {
            GamePrefData data = null;

            if (!string.IsNullOrEmpty(id) && _gamePrefsByIDs.TryGetValue(id, out data) || !string.IsNullOrEmpty(key) && _gamePrefsByKeys.TryGetValue(key, out data))
            {
                if (typeof(T) != typeof(object) && typeof(T) != data.GetPrefType())
                    data = null;
            }
            else if (!string.IsNullOrEmpty(id) && GamePrefStorage.Instance.HasID(id, out data) || !string.IsNullOrEmpty(key) && GamePrefStorage.Instance.HasKey(key, out data))
            {
                if (typeof(T) != typeof(object) && typeof(T) != data.GetPrefType())
                    data = null;
                else data = new GamePrefData(data);
            }
            else if (!string.IsNullOrEmpty(key) && defaultValue != null)
            {
                data = new GamePrefData(GetNewPref().persistentIdentifier, key, defaultValue);
                _gamePrefsByKeys[data.PrefKey] = data;
                _gamePrefsByIDs[data.PersistentIdentifier] = data;
                
                onGamePrefUpdated?.Invoke();
            }

            return data?.PrefValue;
        }
        private static void DoSetValue<T>(string id, string key, T value)
        {
            GamePrefData data = null;

            if (value is not null)
            {
                if (!string.IsNullOrEmpty(id) && _gamePrefsByIDs.TryGetValue(id, out data) || !string.IsNullOrEmpty(key) && _gamePrefsByKeys.TryGetValue(key, out data))
                {
                    if (data.GetPrefType() != value.GetType())
                        data = null;
                }
                else if (!string.IsNullOrEmpty(id) && GamePrefStorage.Instance.HasID(id, out data) || !string.IsNullOrEmpty(key) && GamePrefStorage.Instance.HasKey(key, out data))
                {
                    if (data.GetPrefType() == value.GetType())
                        data = new GamePrefData(data);
                    else data = null;
                }
                else if (!string.IsNullOrEmpty(key) && value != null)
                    data = new GamePrefData(GetNewPref().persistentIdentifier, key, value);

                if (data != null)
                {
                    data.PrefValue = value;
                    _gamePrefsByIDs[data.PersistentIdentifier] = data;
                    _gamePrefsByKeys[data.PrefKey] = data;

                    if (_activeListeners.TryGetValue(data.PersistentIdentifier, out HashSet<IGamePrefEventListener> listeners))
                    {
                        var listenersAsArray = listeners.ToArray();
                        for (int i = listenersAsArray.Length - 1; i >= 0; i--)
                            listenersAsArray[i].OnValueSet(value);
                    }

                    onGamePrefUpdated?.Invoke();
                }

            }
        }

        private static GamePref GetNewPref()
        {
            string persistentIdentifier;
            do persistentIdentifier = Guid.NewGuid().ToString();
            while (HasID(persistentIdentifier));
            return new GamePref(persistentIdentifier);
        }
        #endregion
    }
}