using UnityEngine;

namespace UnitySimplified.Audio
{
	public abstract class AudioEvent : ScriptableObject
    {
        public struct AudioEventParams
        {
            public Transform Parent;
            public Vector3 Position;
        }

        protected abstract void OnPlay(AudioEventHelper audioEventHelper, AudioEventParams audioEventParams);
        public AudioEventHelper Play(AudioEventParams audioEventParams)
        {
            AudioEventHelper helper = CreateHelper();
            if (helper == null)
                return null;
            SetupHelper(helper, audioEventParams);
            OnPlay(helper, audioEventParams);
            return helper;
        }

        private static void SetupHelper(AudioEventHelper helper, AudioEventParams audioEventParams)
        {
            var helperRoot = helper.gameObject;
            if (audioEventParams.Parent)
                helperRoot.transform.SetParent(audioEventParams.Parent);
            helperRoot.transform.localPosition = audioEventParams.Position;
        }

        private AudioEventHelper CreateHelper()
        {
#if UNITY_EDITOR
            var root = Application.isPlaying ? new GameObject(name, typeof(AudioEventHelper)) : UnityEditor.EditorUtility.CreateGameObjectWithHideFlags("Audio Previewer", HideFlags.HideAndDontSave, typeof(AudioEventHelper));
#else
            var root = new GameObject(name, typeof(AudioEventHelper));
#endif
            return root.GetComponent<AudioEventHelper>();
        }
    }
}