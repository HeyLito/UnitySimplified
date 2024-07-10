using System;
using UnityEngine;

namespace UnitySimplified.SpriteAnimator
{
    [Serializable]
    public class SpriteAnimation
    {
        [Serializable]
        public class EventTrigger
        {
            [SerializeField]
            private int _frame;
            [SerializeField]
            private string _identifier;

            public int Frame => _frame;
            public string Identifier => _identifier;

            public EventTrigger() { }
            public EventTrigger(int frame, string identifier) : this()
            {
                _frame = frame;
                _identifier = identifier;
            }
        }
        public class EventTriggerReceiver
        {
            public string Identifier { get; }
            public Action Callback { get; }

            public EventTriggerReceiver(string identifier, Action callback)
            {
                Identifier = identifier;
                Callback = callback;
            }
        }

        [SerializeField]
        private int _fps = 0;
        [SerializeField]
        private bool _loop = true;
        [SerializeField]
        private Sprite[] _frames = Array.Empty<Sprite>();
        [SerializeField]
        private EventTrigger[] _triggers = Array.Empty<EventTrigger>();

        public int Fps => _fps;
        public bool Loop => _loop;
        public Sprite[] Frames => _frames;
        public EventTrigger[] Triggers => _triggers;
    }
}