#if UNITY_EDITOR

using UnityEditor;
using UnitySimplified.Serialization;

namespace UnitySimplifiedEditor.Serialization
{
    [InitializeOnLoad]
    class StorageInitializer
    {
        private const string editorStatus = "editorStatus_Storages";
        static StorageInitializer()
        {
            if (!SessionState.GetBool(editorStatus, false))
            {
                SessionState.SetBool(editorStatus, true);
                _ = PrefabStorage.Instance;
                //_ = AssetStorage.Instance;
            }
        }
    }
}

#endif