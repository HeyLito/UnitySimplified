using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnitySimplified.VariableReferences;

namespace UnitySimplified.SpriteAnimator
{
    [Serializable]
    public sealed partial class SpriteAnimation
    {
        [SerializeField]
        [FormerlySerializedAs("_fps")]
        private int fps;
        [SerializeField]
        [FormerlySerializedAs("_loop")]
        private bool loop = true;
        [SerializeField]
        [FormerlySerializedAs("_frames")]
        private Sprite[] frames = Array.Empty<Sprite>();
        [SerializeField]
        [FormerlySerializedAs("_triggers")]
        private EventTrigger[] triggers = Array.Empty<EventTrigger>();

        public int Fps => fps;
        public bool Loop => loop;
        public Sprite[] Frames => frames;
        public EventTrigger[] Triggers => triggers;
    }

    public partial class SpriteAnimation
    {
        [Serializable]
        public class EventTrigger
        {
            [SerializeField]
            [FormerlySerializedAs("_frame")]
            private int frame;
            [SerializeField]
            [FormerlySerializedAs("_identifier")]
            [FormerlySerializedAs("identifier")]
            private KeywordReference key;

            public EventTrigger() { }
            public EventTrigger(int frame, KeywordReference key) : this()
            {
                this.frame = frame;
                this.key = key;
            }

            public int Frame => frame;
            public string Key => key;
        }
        public class EventTriggerReceiver
        {
            public EventTriggerReceiver(KeywordReference key, Action callback)
            {
                Key = key;
                Callback = callback;
            }

            public KeywordReference Key { get; }
            public Action Callback { get; }
        }
    }
}