using System;
using UnityEngine;
using UnitySimplified.SpriteAnimator.Controller;

namespace UnitySimplified.SpriteAnimator
{
    public class SpriteAnimator : BaseSpriteAnimator
    {
        public enum PlayOn
        {
            None = 0,
            Awake = 1,
            Start = 2,
            Enable = 3,
        }
        [SerializeField]
        private SpriteRenderer _spriteRenderer = null;
        [SerializeField]
        private SpriteAnimatorController _controller;
        [SerializeField]
        private PlayOn _playOn = PlayOn.None;

        [NonSerialized]
        private bool _initializedController;


        public override Sprite Sprite
        {
            get => _spriteRenderer != null ? _spriteRenderer.sprite : null;
            protected set
            {
                if (_spriteRenderer != null)
                    _spriteRenderer.sprite = value;
            }
        }
        public SpriteAnimatorController Controller
        {
            get => _controller;
            set
            {
                if (_controller == value)
                    return;
                Stop();
                if (_controller != null)
                    _controller.DetachFromAnimator(this);
                _controller = value;
                if (_controller != null)
                    _controller.AttachToAnimator(this);
                if (_initializedController == false)
                    _initializedController = true;
            }
        }



        private void Awake()
        {
            if (_playOn == PlayOn.Awake)
                DoPlay();
        }
        private void Start()
        {
            if (_playOn == PlayOn.Start)
                DoPlay();
        }
        private void OnEnable()
        {
            if (_playOn == PlayOn.Enable)
                DoPlay();
        }
        private void DoPlay()
        {
            Play();

            if (AnimationStates.Count == 0)
            {
                string message = $"Could not start <b>{this}</b> because it is missing animations!";
                if (_controller == null)
                    message += $" Consider adding an animator controller.";
                Debug.LogWarning(message);
            }
        }

        protected override void OnInitialize()
        {
            if (_controller != null && !_initializedController)
            {
                _controller.AttachToAnimator(this);
                _initializedController = true;
            }
        }
    }
}