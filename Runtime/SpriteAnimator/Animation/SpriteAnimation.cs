using System;
using UnityEngine;
using UnityEngine.Serialization;

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
            private string identifier;

            public EventTrigger() { }
            public EventTrigger(int frame, string identifier) : this()
            {
                this.frame = frame;
                this.identifier = identifier;
            }

            public int Frame => frame;
            public string Identifier => identifier;
        }
        public class EventTriggerReceiver
        {
            public EventTriggerReceiver(string identifier, Action callback)
            {
                Identifier = identifier;
                Callback = callback;
            }

            public string Identifier { get; }
            public Action Callback { get; }
        }
    }
}