using UnityEngine;
using UnityEngine.Serialization;
using UnitySimplified.VariableReferences;

namespace UnitySimplified.SpriteAnimator
{
    [CreateAssetMenu(fileName = "New SpriteAnimationClip", menuName = "Unity Simplified/Sprite Animation Clip")]
    public class SpriteAnimationClip : ScriptableObject, IVariableAsset<SpriteAnimation>
    {
        [SerializeField]
        [FormerlySerializedAs("_animation")]
        private SpriteAnimation animation = new();

        public SpriteAnimation Animation => animation;

        public static implicit operator SpriteAnimation(SpriteAnimationClip clip) => clip.animation;

        SpriteAnimation IVariableAsset<SpriteAnimation>.GetValue() => this;
        void IVariableAsset<SpriteAnimation>.SetValue(SpriteAnimation value) => animation  = value;
        void IVariableAsset<SpriteAnimation>.SetValue(IVariableAsset<SpriteAnimation> variableAsset) => animation = variableAsset.GetValue();
    }
}