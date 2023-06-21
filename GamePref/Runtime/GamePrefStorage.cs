using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public class GamePrefStorage : Storage<GamePrefStorage>
    {
        [SerializeField] private List<string> gamePrefs = new List<string>();
        
        private Dictionary<string, GamePrefData> _gamePrefsByIDs = null;
        private Dictionary<string, GamePrefData> _gamePrefsByKeys = null;
        private Dictionary<string, int> _indexesByIDs = null;
        
        public Dictionary<string, GamePrefData> GetGamePrefs()
        {
            if (_gamePrefsByIDs == null)
            {
                _gamePrefsByIDs = new Dictionary<string, GamePrefData>();
                _gamePrefsByKeys = new Dictionary<string, GamePrefData>();
                _indexesByIDs = new Dictionary<string, int>();
                for (int i = 0; i < gamePrefs.Count; i++)
                {
                    GamePrefData tempPref = new GamePrefData();
                    DataManager.LoadFileFromString(tempPref, gamePrefs[i], FileFormat.Binary);
                    _gamePrefsByIDs.Add(tempPref.PersistentIdentifier, tempPref);
                    _gamePrefsByKeys.Add(tempPref.PrefKey, tempPref);
                    _indexesByIDs.Add(tempPref.PersistentIdentifier, i);
                }
            }
            return _gamePrefsByIDs;
        }
        public void AddGamePrefData(GamePrefData data)
        {
            gamePrefs.Add(DataManager.SaveFileAsString(data, FileFormat.Binary));
            _gamePrefsByIDs.Add(data.PersistentIdentifier, data);
            _gamePrefsByKeys.Add(data.PrefKey, data);
            _indexesByIDs.Add(data.PersistentIdentifier, gamePrefs.Count - 1);
        }
        public void OverwriteGamePref(GamePrefData gamePref)
        {
            if (_indexesByIDs.TryGetValue(gamePref.PersistentIdentifier, out int index))
            {
                gamePrefs[index] = DataManager.SaveFileAsString(gamePref, FileFormat.Binary);
                _gamePrefsByIDs[gamePref.PersistentIdentifier] = gamePref;
                _gamePrefsByKeys[gamePref.PrefKey] = gamePref;
            }
        }
        public void Remove(string id)
        {
            if (_indexesByIDs.TryGetValue(id, out int index))
            {
                string prefKey = _gamePrefsByIDs[id].PrefKey;
                gamePrefs.RemoveAt(index);
                _gamePrefsByIDs.Remove(id);
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
            gamePrefs.Clear();
            _gamePrefsByIDs.Clear();
            _gamePrefsByKeys.Clear();
            _indexesByIDs.Clear();
        }

        public bool HasGamePref(GamePref gamePref) => HasGamePref(gamePref, out _);
        internal bool HasGamePref(GamePref gamePref, out GamePrefData data)
        {
            data = null;
            return gamePref != null && HasID(gamePref.PersistentIdentifier, out data);
        }

        public bool HasKey(string key) => HasKey(key, out _);
        internal bool HasKey(string key, out GamePrefData data)
        {
            data = null;
            if (string.IsNullOrEmpty(key))
                return false;
            else
            {
                GetGamePrefs();
                if (_gamePrefsByKeys.TryGetValue(key, out data))
                    return true;
            }
            return false;
        }

        public bool HasID(string id) => HasID(id, out _);
        public bool HasID(string id, out GamePrefData data)
        {
            data = null;
            if (string.IsNullOrEmpty(id))
                return false;
            else if (GetGamePrefs().TryGetValue(id, out data))
                return true;
            return false;
        }
    }
}