#if UNITY_EDITOR

using System;

namespace UnitySimplifiedEditor.SpriteAnimator.Controller
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class CustomControllerConditionDrawer : Attribute
    {
        public Type ParameterType { get; }
        /// <summary>
        /// Not yet implemented
        /// </summary>
        public bool UseForChildren { get; }

        public CustomControllerConditionDrawer(Type parameterType) =>
            ParameterType = parameterType;
        public CustomControllerConditionDrawer(Type parameterType, bool useForChildren) : this(parameterType) =>
            UseForChildren = useForChildren;
    }
}

#endif