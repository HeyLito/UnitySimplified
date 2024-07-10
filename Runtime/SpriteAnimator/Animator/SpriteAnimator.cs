using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnitySimplified.SpriteAnimator.Controller;

namespace UnitySimplified.SpriteAnimator
{
    public class SpriteAnimator : AbstractSpriteAnimator
    {
        public enum PlayOn
        {
            None    = 0,
            Awake   = 1,
            Start   = 2,
            Enable  = 3,
        }
        [SerializeField]
        [FormerlySerializedAs("_spriteRenderer")]
        private SpriteRenderer spriteRenderer;
        [SerializeField]
        [FormerlySerializedAs("_controller")]
        private SpriteAnimatorController controller;
        [SerializeField]
        [FormerlySerializedAs("_playOn")]
        private PlayOn playOn = PlayOn.None;

        [NonSerialized]
        private bool _initializedController;


        public override Sprite Sprite
        {
            get => spriteRenderer != null ? spriteRenderer.sprite : null;
            protected set
            {
                if (spriteRenderer != null)
                    spriteRenderer.sprite = value;
            }
        }
        public SpriteAnimatorController Controller
        {
            get => controller;
            set
            {
                if (controller == value)
                    return;
                Stop();
                if (controller != null)
                    controller.DetachFromAnimator(this);
                controller = value;
                if (controller != null)
                    controller.AttachToAnimator(this);
                if (_initializedController == false)
                    _initializedController = true;
            }
        }



        private void Awake()
        {
            if (playOn == PlayOn.Awake)
                DoPlay();
        }
        private void Start()
        {
            if (playOn == PlayOn.Start)
                DoPlay();
        }
        private void OnEnable()
        {
            if (playOn == PlayOn.Enable)
                DoPlay();
        }
        protected override void OnInitialize()
        {
            if (controller == null || _initializedController)
                return;
            controller.AttachToAnimator(this);
            _initializedController = true;
        }
        private void DoPlay()
        {
            Play();

            if (AnimationStates.Count != 0)
                return;
            string message = $"Could not start <b>{this}</b> because it is missing animations!";
            if (controller == null)
                message += $" Consider adding an animator controller.";
            Debug.LogWarning(message);
        }
    }
}