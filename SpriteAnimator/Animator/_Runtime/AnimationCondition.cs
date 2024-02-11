using System;

namespace UnitySimplified.SpriteAnimator
{
    public class AnimationCondition
    {
        public virtual string Name { get; private set; }
        public virtual string GetCurrentAsString => "";
        public virtual Func<bool> GetResult { get; private set; }

        public AnimationCondition(string name, Func<bool> resultGetter)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            GetResult = resultGetter;
        }



        public virtual void OnSuccessfulResult(AnimationTransition context) { }
    }
}