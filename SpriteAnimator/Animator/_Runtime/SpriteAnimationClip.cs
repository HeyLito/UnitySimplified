using UnityEngine;
using UnitySimplified.VariableReferences;

namespace UnitySimplified.SpriteAnimator
{
    [CreateAssetMenu(fileName = "New SpriteAnimationClip", menuName = "Unity Simplified/Sprite Animation Clip")]
    public class SpriteAnimationClip : ScriptableObject, IVariableAsset<SpriteAnimation>
    {
        [SerializeField]
        private SpriteAnimation _animation = new();


        public SpriteAnimation Animation => _animation;



        SpriteAnimation IVariableAsset<SpriteAnimation>.GetValue() => this;
        void IVariableAsset<SpriteAnimation>.SetValue(SpriteAnimation value) => throw new System.NotImplementedException();
        void IVariableAsset<SpriteAnimation>.SetValue(IVariableAsset<SpriteAnimation> otherValue) => throw new System.NotImplementedException();


        public static implicit operator SpriteAnimation(SpriteAnimationClip clip) => clip._animation;
    }
}