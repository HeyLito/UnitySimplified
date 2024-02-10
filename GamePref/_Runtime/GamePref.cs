using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnitySimplified.RuntimeDatabases;
using UnitySimplified.Serialization.Formatters;
using Unity.VisualScripting;

namespace UnitySimplified.Serialization
{
    public interface IGamePrefEventListener
    {
        public void OnValueSet(object value);
    }

    [Serializable]
    public class GamePref
    {
        #region PUBLIC

        #region FIELDS
        [SerializeField]
        private string _identifier = "";
        #endregion

        #region PROPERTIES
        public string Identifier => _identifier;
        #endregion

        #region CONSTRUCTORS
        private GamePref() { }
        private GamePref(string identifier) => _identifier = identifier;
        #endregion

        #region METHODS
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
        public object GetValue()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return null;
#endif
            if (this == Empty)
                return null;

            if (!_loaded)
                Load();

            if (DoTryGetGamePrefFromIdentifier(_identifier, out var data))
                return DoGetValue<object>(data.Key, null);
            return null;
        }
        public void SetValue(object value)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            if (this == Empty)
                return;

            if (!_loaded)
                Load();

            if (DoTryGetGamePrefFromIdentifier(_identifier, out var data))
                DoSetValue(data.Key, value);
        }

        public void RegisterListener(IGamePrefEventListener listener)
        {
            if (listener == null)
                return;
            if (!_activeListeners.TryGetValue(Identifier, out HashSet<IGamePrefEventListener> listeners))
                _activeListeners[Identifier] = listeners = new HashSet<IGamePrefEventListener>();

            listeners.Add(listener);
        }
        public void UnregisterListener(IGamePrefEventListener listener)
        {
            if (listener == null)
                return;
            if (!_activeListeners.TryGetValue(Identifier, out HashSet<IGamePrefEventListener> listeners))
                return;

            if (!listeners.Remove(listener))
                return;
            if (listeners.Count == 0)
                _activeListeners.Remove(Identifier);
        }
        #endregion

        #endregion


        #region STATIC

        public static readonly GamePref Empty = new();
        private static readonly Dictionary<string, string> IdentifiersByKeys = new();
        private static readonly Dictionary<string, GamePrefData> GamePrefsByIdentifiers = new();
        private static readonly Dictionary<string, HashSet<IGamePrefEventListener>> _activeListeners = new();
        private static List<GamePrefData> _savedData = new();
        private static bool _loaded;

        public static event Action onGamePrefUpdated;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Application.quitting += Save;
            Load();
        }


        public static void Load()
        {
            _loaded = true;
            _savedData.Clear();
            IdentifiersByKeys.Clear();
            GamePrefsByIdentifiers.Clear();

            string previousPath = DataManager.TargetDataPath;
            DataManager.TargetDataPath = DataManager.DefaultPath;
            DataManager.LoadDatabase();
            DataManager.LoadFromFile("GamePrefs", _savedData);
            DataManager.TargetDataPath = previousPath;
            

            foreach (var item in _savedData)
            {
                IdentifiersByKeys.Add(item.Key, item.Identifier);
                GamePrefsByIdentifiers.Add(item.Identifier, item);
            }

            onGamePrefUpdated?.Invoke();
        }
        public static void Save()
        {
            _savedData.Clear();
            foreach (var item in GamePrefsByIdentifiers.Values)
                _savedData.Add(item);

            string previousPath = DataManager.TargetDataPath;
            DataManager.TargetDataPath = DataManager.DefaultPath;
            DataManager.LoadDatabase();
            if (!DataManager.SaveToFile("GamePrefs", _savedData))
                DataManager.CreateNewFile("GamePrefs", "GamePrefs", new BinaryDataFormatter(), _savedData);
            DataManager.TargetDataPath = previousPath;
            DataManager.LoadDatabase();

            onGamePrefUpdated?.Invoke();
        }
        public static void DeleteAll()
        {
            IdentifiersByKeys.Clear();
            GamePrefsByIdentifiers.Clear();
            onGamePrefUpdated?.Invoke();
        }
        public static bool DeleteKey(string key)
        {
            if (!IdentifiersByKeys.Remove(key, out string identifier)) return false;
            GamePrefsByIdentifiers.Remove(identifier);
            onGamePrefUpdated?.Invoke();
            return true;
        }

        public static bool HasGamePref(GamePref gamePref) => GamePrefsByIdentifiers.TryGetValue(gamePref.Identifier, out _);
        public static bool HasKey(string key) => IdentifiersByKeys.ContainsKey(key);

        public static T GetValue<T>(string key) => DoGetValue(key, default(T));
        public static T GetValue<T>(string key, T defaultValue) => DoGetValue(key, defaultValue);
        public static void SetValue<T>(string key, T value) => DoSetValue(key, value);

        internal static bool DoTryGetGamePrefFromIdentifier(string identifier, out GamePrefData value) => GamePrefsByIdentifiers.TryGetValue(identifier, out value);
        internal static bool DoTryGetGamePrefFromKey(string key, out GamePrefData value) => 
            IdentifiersByKeys.TryGetValue(key, out var identifier) ? DoTryGetGamePrefFromIdentifier(identifier, out value) : (value = null) != null;
        
        private static T DoGetValue<T>(string key, T defaultValue)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"Method parameter \'{nameof(key)}\' is empty.");

            GamePrefData data;
            if (IdentifiersByKeys.TryGetValue(key, out var identifier))
            {
                if (GamePrefsByIdentifiers.TryGetValue(identifier, out data))
                {
                    if (data.ValueType != typeof(T))
                        data = null;
                }
                else
                {
                    Debug.Log("This should be fixed");
                }
            }
            else if (GamePrefStorage.Instance.TryGetFromKey(key, out data))
            {
                data = data.ValueType == typeof(T) ? new GamePrefData(data) : null;
            }
            else
            {
                data = new GamePrefData(Create().Identifier, key, defaultValue);
                IdentifiersByKeys[data.Key] = data.Identifier;
                GamePrefsByIdentifiers[data.Identifier] = data;
                onGamePrefUpdated?.Invoke();
            }
            return (T)data?.Value;
        }
        private static void DoSetValue<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"Method parameter \'{nameof(key)}\' is empty.");
            if (value is null)
                return;

            GamePrefData data;
            if (IdentifiersByKeys.TryGetValue(key, out var identifier))
            {
                if (GamePrefsByIdentifiers.TryGetValue(identifier, out data))
                {
                    data = data != null && data.ValueType != typeof(T) ? data : null;
                }
                else
                {
                    Debug.Log("This should be fixed");
                }
            }
            else if (GamePrefStorage.Instance.TryGetFromKey(key, out data))
            {
                data = data != null && data.ValueType == typeof(T) ? new GamePrefData(data) : null;
            }
            else
            {
                data = new GamePrefData(Create().Identifier, key, value);
                IdentifiersByKeys[data.Key] = data.Identifier;
                GamePrefsByIdentifiers[data.Identifier] = data;
            }


            if (data == null)
                return;
            data.Value = value;
            if (_activeListeners.TryGetValue(data.Identifier, out HashSet<IGamePrefEventListener> listeners))
            {
                var listenersAsArray = listeners.ToArray();
                for (int i = listenersAsArray.Length - 1; i >= 0; i--)
                    listenersAsArray[i].OnValueSet(value);
            }
            onGamePrefUpdated?.Invoke();
        }

        private static GamePref Create()
        {
            string identifier;
            do identifier = Guid.NewGuid().ToString();
            while (GamePrefsByIdentifiers.ContainsKey(identifier));
            return new GamePref(identifier);
        }
        #endregion
    }
}