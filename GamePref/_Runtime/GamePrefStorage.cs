using System.Collections.Generic;
using UnityEngine;
using UnitySimplified.Serialization;
using UnitySimplified.Serialization.Formatters;

namespace UnitySimplified.RuntimeDatabases
{
    public class GamePrefStorage : RuntimeDatabase<GamePrefStorage>
    {
        [SerializeField]
        private List<string> _savedGamePrefs = new();
        
        private Dictionary<string, GamePrefData> _gamePrefsByKeys;
        private Dictionary<string, GamePrefData> _gamePrefsByIdentifiers;
        private Dictionary<string, int> _indexesByIDs = null;
        private readonly BinaryDataFormatter _binaryDataFormatter = new BinaryDataFormatter();

        public void AddGamePrefData(GamePrefData data)
        {
            DataManager.SaveObjectAsString(data, _binaryDataFormatter, out string dataAsString);
            _savedGamePrefs.Add(dataAsString);
            _gamePrefsByIdentifiers.Add(data.Identifier, data);
            _gamePrefsByKeys.Add(data.Key, data);
            _indexesByIDs.Add(data.Identifier, _savedGamePrefs.Count - 1);
        }
        public void OverwriteGamePref(GamePrefData gamePref)
        {
            if (_indexesByIDs.TryGetValue(gamePref.Identifier, out int index))
            {
                DataManager.SaveObjectAsString(gamePref, _binaryDataFormatter, out string dataAsString);
                _savedGamePrefs[index] = dataAsString;
                _gamePrefsByIdentifiers[gamePref.Identifier] = gamePref;
                _gamePrefsByKeys[gamePref.Key] = gamePref;
            }
        }
        public void Remove(string id)
        {
            if (_indexesByIDs.TryGetValue(id, out int index))
            {
                string prefKey = _gamePrefsByIdentifiers[id].Key;
                _savedGamePrefs.RemoveAt(index);
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
            _savedGamePrefs.Clear();
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

        public Dictionary<string, GamePrefData> GetGamePrefs()
        {
            if (_gamePrefsByIdentifiers != null)
                return _gamePrefsByIdentifiers;

            _gamePrefsByIdentifiers = new Dictionary<string, GamePrefData>();
            _gamePrefsByKeys = new Dictionary<string, GamePrefData>();
            _indexesByIDs = new Dictionary<string, int>();
            for (int i = 0; i < _savedGamePrefs.Count; i++)
            {
                GamePrefData tempPref = new GamePrefData();
                DataManager.LoadObjectFromString(tempPref, _binaryDataFormatter, _savedGamePrefs[i]);
                _gamePrefsByIdentifiers.Add(tempPref.Identifier, tempPref);
                _gamePrefsByKeys.Add(tempPref.Key, tempPref);
                _indexesByIDs.Add(tempPref.Identifier, i);
            }
            return _gamePrefsByIdentifiers;
        }
    }
}