using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnitySimplifiedEditor.Serialization;
#endif

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
        #endregion

        #region PROPERTIES
        public string PersistentIdentifier => persistentIdentifier;
        #endregion

        #region CONSTRUCTORS
        private GamePref()
        { }
        private GamePref(string persistentIdentifier)
        {   this.persistentIdentifier = persistentIdentifier;   }
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
        public override int GetHashCode()
        {   return 0;   }
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

            bool previousSelection = DataManagerUtility.UsingNewtonsoftJson;
            string previousPath = DataManager.TargetDataPath;
            DataManager.TargetDataPath = DataManager.DefaultPath;
            DataManagerUtility.UsingNewtonsoftJson = DataManagerUtility.DoesNewtonsoftJsonExist;
            DataManager.LoadFileDatabase(DataManagerUtility.UsingNewtonsoftJson ? FileFormat.JSON : FileFormat.Binary, true);
            DataManager.LoadFromFile("GamePrefs", _gamePrefsByIDs);
            DataManager.TargetDataPath = previousPath;
            DataManagerUtility.UsingNewtonsoftJson = previousSelection;
            

            _loaded = true;
            foreach (var pair in _gamePrefsByIDs)
                _gamePrefsByKeys.Add(pair.Value.prefKey, pair.Value);


            #if UNITY_EDITOR
            GamePrefWindow.onGamePrefsUpdated?.Invoke();
            #endif
        }
        public static void Save()
        {
            bool previousSelection = DataManagerUtility.UsingNewtonsoftJson;
            DataManagerUtility.UsingNewtonsoftJson = DataManagerUtility.DoesNewtonsoftJsonExist;
            if (!DataManager.SaveToFile("GamePrefs", _gamePrefsByIDs))
            {
                string previousPath = DataManager.TargetDataPath;
                DataManager.TargetDataPath = DataManager.DefaultPath;
                DataManager.CreateNewFile("GamePrefs", _gamePrefsByIDs, DataManagerUtility.UsingNewtonsoftJson ? FileFormat.JSON : FileFormat.Binary);
                DataManager.TargetDataPath = previousPath;
            }
            DataManagerUtility.UsingNewtonsoftJson = previousSelection;


            #if UNITY_EDITOR
            GamePrefWindow.onGamePrefsUpdated?.Invoke();
            #endif
        }
        public static void DeleteAll()
        {
            _gamePrefsByIDs.Clear();
            _gamePrefsByKeys.Clear();

            #if UNITY_EDITOR
            GamePrefWindow.onGamePrefsUpdated?.Invoke();
            #endif
        }
        public static void DeleteKey(string key)
        {
            if (_gamePrefsByKeys.TryGetValue(key, out GamePrefData data))
            {
                _gamePrefsByIDs.Remove(data.persistentIdentifier);
                _gamePrefsByKeys.Remove(data.prefKey);

                #if UNITY_EDITOR
                GamePrefWindow.onGamePrefsUpdated?.Invoke();
                #endif
            }
        }

        public static bool HasGamePref(GamePref gamePref)
        {   return HasGamePref(gamePref, out _);   }
        internal static bool HasGamePref(GamePref gamePref, out GamePrefData data)
        {   data = null; return gamePref != null && _gamePrefsByIDs.TryGetValue(gamePref.PersistentIdentifier, out data);   }
        public static bool HasKey(string key)
        {   return _gamePrefsByKeys.ContainsKey(key);   }
        public static bool HasID(string id)
        {
            if (!_loaded)
                Load();
            return _gamePrefsByIDs.ContainsKey(id);
        }

        public static T GetValue<T>(string key)
        {   return (T)DoGetValue(null, key, default(T));   }
        public static T GetValue<T>(string key, T defaultValue)
        {   return (T)DoGetValue(null, key, defaultValue);   }
        public static void SetValue<T>(string key, T value)
        {   DoSetValue(null, key, value);   }

        private static object DoGetValue<T>(string id, string key, T defaultValue)
        {
            #if UNITY_EDITOR
            bool updateEditorWindow = false;
            #endif
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
                data = new GamePrefData(GetNewPref(), key, defaultValue);
                _gamePrefsByKeys[data.prefKey] = data;
                _gamePrefsByIDs[data.persistentIdentifier] = data;
                #if UNITY_EDITOR
                updateEditorWindow = true;
                #endif
            }

            #if UNITY_EDITOR
            if (updateEditorWindow)
                GamePrefWindow.onGamePrefsUpdated?.Invoke();
            #endif

            if (data != null)
                return data.prefValue;
            else return null;
        }
        private static void DoSetValue<T>(string id, string key, T value)
        {
            #if UNITY_EDITOR
            bool updateEditorWindow = false;
            #endif
            GamePrefData data = null;

            if (!(value is null))
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
                }
                else if (!string.IsNullOrEmpty(key) && value != null)
                    data = new GamePrefData(GetNewPref(), key, value);

                if (data != null)
                {
                    data.prefValue = value;
                    _gamePrefsByIDs[data.persistentIdentifier] = data;
                    _gamePrefsByKeys[data.prefKey] = data;
                    #if UNITY_EDITOR
                    updateEditorWindow = true;
                    #endif

                    if (_activeListeners.TryGetValue(data.persistentIdentifier, out HashSet<IGamePrefEventListener> listeners))
                    {
                        var listenersAsArray = listeners.ToArray();
                        for (int i = listenersAsArray.Length - 1; i >= 0; i--)
                            listenersAsArray[i].OnValueSet(value);
                    }
                }

                #if UNITY_EDITOR
                if (updateEditorWindow)
                    GamePrefWindow.onGamePrefsUpdated?.Invoke();
                #endif
            }
        }

        internal static GamePref GetNewPref()
        {
            string persistentIdentifier;
            do persistentIdentifier = Guid.NewGuid().ToString();
            while (HasID(persistentIdentifier));
            return new GamePref(persistentIdentifier);
        }
        #endregion
    }
}