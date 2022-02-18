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
                    _gamePrefsByIDs.Add(tempPref.persistentIdentifier, tempPref);
                    _gamePrefsByKeys.Add(tempPref.prefKey, tempPref);
                    _indexesByIDs.Add(tempPref.persistentIdentifier, i);
                }
            }
            return _gamePrefsByIDs;
        }
        public void AddGamePrefData(GamePrefData data)
        {
            gamePrefs.Add(DataManager.SaveFileAsString(data, FileFormat.Binary));
            _gamePrefsByIDs.Add(data.persistentIdentifier, data);
            _gamePrefsByKeys.Add(data.prefKey, data);
            _indexesByIDs.Add(data.persistentIdentifier, gamePrefs.Count - 1);
        }
        public void OverwriteGamePref(GamePrefData gamePref)
        {
            if (_indexesByIDs.TryGetValue(gamePref.persistentIdentifier, out int index))
            {
                gamePrefs[index] = DataManager.SaveFileAsString(gamePref, FileFormat.Binary);
                _gamePrefsByIDs[gamePref.persistentIdentifier] = gamePref;
                _gamePrefsByKeys[gamePref.prefKey] = gamePref;
            }
        }
        public void Remove(string id)
        {
            if (_indexesByIDs.TryGetValue(id, out int index))
            {
                string prefKey = _gamePrefsByIDs[id].prefKey;
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

        public bool HasGamePref(GamePref gamePref)
        {   return HasGamePref(gamePref, out _);   }
        internal bool HasGamePref(GamePref gamePref, out GamePrefData data)
        {   data = null; return gamePref != null && HasID(gamePref.PersistentIdentifier, out data);   }

        public bool HasKey(string key)
        {   return HasKey(key, out _);   }
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

        public bool HasID(string id)
        {   return HasID(id, out _);   }
        internal bool HasID(string id, out GamePrefData data)
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