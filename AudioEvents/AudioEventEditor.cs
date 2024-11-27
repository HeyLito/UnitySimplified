#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnitySimplified.Audio;

namespace UnitySimplifiedEditor.Audio
{
    [CustomEditor(typeof(AudioEvent), true)]
    public class AudioEventEditor : Editor
    {
        private AudioEventHelper _previewer;

        public void OnDisable()
        {
            if (_previewer)
            {
                _previewer.StopImmediately();
                DestroyImmediate(_previewer.gameObject);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            bool hasPreviewer = _previewer;
            if (hasPreviewer && !_previewer.IsPlaying && !_previewer.IsStopping)
            {
                DestroyImmediate(_previewer.gameObject);
                _previewer = null;
                hasPreviewer = false;
            }

            using (new EditorGUI.DisabledGroupScope(serializedObject.isEditingMultipleObjects))
            {
                using (new EditorGUI.DisabledGroupScope(hasPreviewer))
                {
                    if (GUILayout.Button("Preview"))
                    {
                        _previewer = ((AudioEvent)target).Play(default);
                        hasPreviewer = true;
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!hasPreviewer || !_previewer.IsPlaying || _previewer.IsStopping))
                {
                    if (GUILayout.Button("Stop"))
                    {
                        if (hasPreviewer)
                            _previewer.Stop();
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!hasPreviewer || !_previewer.IsPlaying))
                {
                    if (GUILayout.Button("Stop Immediately"))
                    {
                        if (hasPreviewer)
                            _previewer.StopImmediately();
                    }
                }
            }
        }
    }
}
#endif