using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnitySimplified.RuntimeDatabases;
using UnitySimplified.Serialization;
using UnitySimplified.Serialization.Formatters;

namespace UnitySimplified.GamePrefs
{
    public class GamePrefLocalDatabase : RuntimeDatabase<GamePrefLocalDatabase>, IEnumerable<GamePrefData>
    {
        [SerializeField]
        [FormerlySerializedAs("_savedGamePrefs")]
        private List<string> savedGamePrefs = new();
        
        private Dictionary<string, GamePrefData> _gamePrefsByKeys;
        private Dictionary<string, GamePrefData> _gamePrefsByIdentifiers;
        private Dictionary<string, int> _indexesByIDs;
        private readonly IDataFormatter _dataFormatter = new JsonDataFormatter();
        public event Action OnValuesChanged;

        internal void AddGamePrefData(GamePrefData data)
        {
            DataManager.SaveObjectAsString(data, _dataFormatter, out string dataAsString);
            savedGamePrefs.Add(dataAsString);
            _gamePrefsByIdentifiers.Add(data.identifier, data);
            _gamePrefsByKeys.Add(data.key, data);
            _indexesByIDs.Add(data.identifier, savedGamePrefs.Count - 1);
            OnValuesChanged?.Invoke();
        }
        internal void OverwriteGamePref(GamePrefData data)
        {
            if (!_indexesByIDs.TryGetValue(data.identifier, out int index))
                return;

            DataManager.SaveObjectAsString(data, _dataFormatter, out string dataAsString);
            savedGamePrefs[index] = dataAsString;
            _gamePrefsByIdentifiers[data.identifier] = data;
            _gamePrefsByKeys[data.key] = data;
            OnValuesChanged?.Invoke();
        }
        public void Remove(string id)
        {
            if (_indexesByIDs.TryGetValue(id, out int index))
            {
                string prefKey = _gamePrefsByIdentifiers[id].key;
                savedGamePrefs.RemoveAt(index);
                _gamePrefsByIdentifiers.Remove(id);
                _gamePrefsByKeys.Remove(prefKey);
                _indexesByIDs.Remove(id);
                List<KeyValuePair<string, int>> newIndexesByIDs = new List<KeyValuePair<string, int>>(_indexesByIDs);
                for (int i = 0; i < newIndexesByIDs.Count; i++)
                    if (newIndexesByIDs[i].Value > index)
                        _indexesByIDs[newIndexesByIDs[i].Key] = newIndexesByIDs[i].Value - 1;
            }
        }
        public void RemoveAll()
        {
            savedGamePrefs.Clear();
            _gamePrefsByIdentifiers.Clear();
            _gamePrefsByKeys.Clear();
            _indexesByIDs.Clear();
        }

        public bool HasIdentifier(string identifier) => TryGetFromIdentifier(identifier, out _);
        public bool HasKey(string key) => TryGetFromKey(key, out _);

        internal bool TryGetFromIdentifier(string id, out GamePrefData data)
        {
            data = null;
            if (string.IsNullOrEmpty(id))
                return false;
            
            return GetGamePrefs().TryGetValue(id, out data);
        }
        internal bool TryGetFromKey(string key, out GamePrefData data)
        {
            data = null;
            if (string.IsNullOrEmpty(key))
                return false;
            
            GetGamePrefs();
            return _gamePrefsByKeys.TryGetValue(key, out data);
        }

        internal Dictionary<string, GamePrefData> GetGamePrefs()
        {
            if (_gamePrefsByIdentifiers != null)
                return _gamePrefsByIdentifiers;

            _gamePrefsByIdentifiers = new Dictionary<string, GamePrefData>();
            _gamePrefsByKeys = new Dictionary<string, GamePrefData>();
            _indexesByIDs = new Dictionary<string, int>();
            for (int i = 0; i < savedGamePrefs.Count; i++)
            {
                GamePrefData tempPref = new GamePrefData();
                DataManager.LoadObjectFromString(tempPref, _dataFormatter, savedGamePrefs[i]);
                _gamePrefsByIdentifiers.Add(tempPref.identifier, tempPref);
                _gamePrefsByKeys.Add(tempPref.key, tempPref);
                _indexesByIDs.Add(tempPref.identifier, i);
            }
            return _gamePrefsByIdentifiers;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetGamePrefs().Values.GetEnumerator();
        IEnumerator<GamePrefData> IEnumerable<GamePrefData>.GetEnumerator() => GetGamePrefs().Values.GetEnumerator();
    }
}