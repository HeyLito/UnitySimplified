using UnityEngine;
using UnitySimplified.VariableReferences;

namespace UnitySimplified.SpriteAnimator
{
    [CreateAssetMenu(fileName = "New SpriteAnimationClip", menuName = "Unity Simplified/Sprite Animation Clip")]
    public class SpriteAnimationClip : ScriptableObject, IVariableObjectReference<SpriteAnimation>
    {
        [SerializeField]
        private SpriteAnimation _animation = new();


        public SpriteAnimation Animation => _animation;



        SpriteAnimation IVariableObjectReference<SpriteAnimation>.GetValue() => this;
        void IVariableObjectReference<SpriteAnimation>.SetValue(SpriteAnimation value) => throw new System.NotImplementedException();
        void IVariableObjectReference<SpriteAnimation>.SetValue(IVariableObjectReference<SpriteAnimation> otherValue) => throw new System.NotImplementedException();


        public static implicit operator SpriteAnimation(SpriteAnimationClip clip) => clip._animation;
    }
}