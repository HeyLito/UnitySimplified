using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnitySimplified.Serialization
{
    [Serializable]
    public class AssetByKeysDictionary : SerializableDictionary<string, AssetInfo> { }
    
    [Serializable]
    public class KeysByAssetsDictionary : SerializableDictionary<UnityObject, string> { }

    [Serializable]
    public class TagsToIDsDictionary : SerializableDictionary<string, HashSet<string>> { }
    
    [Serializable]
    public struct AssetInfo
    {
        [SerializeField] private UnityObject asset;
        [SerializeField] private string category;
        [SerializeField] private string[] tags;

        public UnityObject Asset => asset;
        public string Category => category;
        public string[] Tags => tags;

        public AssetInfo(UnityObject asset, string category = "", params string[] tags)
        {
            this.asset = asset;
            this.category = category;
            this.tags = tags;
        }
    }

    public class AssetStorage : Storage<AssetStorage>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string[] assetExtensions = new[]
        {
            ".prefab",
            ".asset",
            ".fbx",
            ".mat",
            ".png",
            ".mp4",
            ".mixer",
            ".ogg",
            ".controller",
        };

        [SerializeField]
        private AssetByKeysDictionary _assetsByKeys = new AssetByKeysDictionary();
        [SerializeField]
        private KeysByAssetsDictionary _keysByAssets = new KeysByAssetsDictionary();
        [SerializeField]
        private TagsToIDsDictionary _tagsToIDsDict = new TagsToIDsDictionary();

        public string[] AssetExtensions => assetExtensions;
        public List<KeyValuePair<string, AssetInfo>> Values => _assetsByKeys.ToList();

        private readonly HashSet<Type> _supportedAssets = new HashSet<Type>()
        {
            typeof(Mesh),
            typeof(AudioClip),
            typeof(Material),
            typeof(PhysicMaterial),
            typeof(PhysicsMaterial2D),
            typeof(Flare),
            typeof(GUIStyle),
            typeof(Texture),
            typeof(RuntimeAnimatorController),
            //typeof(AnimatorController),
            typeof(AnimationClip)
        };

        protected override bool SaveAsFile => false;



        public bool Contains(string id) => TryGetAsset(id, out _);
        public bool Contains(UnityObject asset) => TryGetID(asset, out _);

        public bool TryGetAsset(string id, out UnityObject asset)
        {
            if (!_assetsByKeys.TryGetValue(id, out AssetInfo assetInfo))
            {
                asset = null;
                return false;
            }
            else
            {
                asset = assetInfo.Asset;
                return true;
            }
        }
        public bool TryGetID(UnityObject asset, out string id)
        {
            if (!_keysByAssets.TryGetValue(asset, out id))
            {
                if (_tagsToIDsDict.TryGetValue(asset.name, out HashSet<string> ids) && ids.Count > 0)
                {
                    HashSet<string>.Enumerator enumerator = ids.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        id = enumerator.Current;
                        return true;
                    }
                }
                
                id = null;
                return false;
            }
            else return true;
        }

        #if UNITY_EDITOR
        public void Clear()
        {
            if (Application.isPlaying)
                return;

            _assetsByKeys.Clear();
            _keysByAssets.Clear();
            _tagsToIDsDict.Clear();

        }
        public bool InsertAssetEntry(AssetInfo assetInfo)
        {
            if (Application.isPlaying)
                return false;

            if (Contains(assetInfo.Asset))
            {
                Debug.Log($"{assetInfo.Asset}");
                return false;
            }
            else
            {
                string id;
                do id = Guid.NewGuid().ToString();
                while (_assetsByKeys.ContainsKey(id));

                _assetsByKeys[id] = assetInfo;
                _keysByAssets[assetInfo.Asset] = id;

                return true;
            }
        }
        public bool RemoveAssetEntry(string id)
        {
            if (Application.isPlaying)
                return false;

            if (TryGetAsset(id, out UnityObject asset))
            {
                _assetsByKeys.Remove(id);
                _keysByAssets.Remove(asset);
                return true;
            }
            else return false;
        }
        public bool RemoveAssetEntry(UnityObject asset)
        {
            if (Application.isPlaying)
                return false;

            if (TryGetID(asset, out string id))
            {
                _assetsByKeys.Remove(id);
                _keysByAssets.Remove(asset);
                return true;
            }
            else return false;
        }
        #endif

        public bool SupportsType(Type type) => _supportedAssets.Contains(type) || type.IsSubclassOf(typeof(ScriptableObject));

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _tagsToIDsDict.Clear();
            foreach (var entry in _assetsByKeys)
                foreach (var tag in entry.Value.Tags)
                {
                    if (!_tagsToIDsDict.TryGetValue(tag, out HashSet<string> ids))
                        _tagsToIDsDict[tag] = ids = new HashSet<string>();
                    if (!ids.Contains(tag))
                        ids.Add(entry.Key);
                }
        }
    }
}