#if UNITY_EDITOR

using System;

namespace UnitySimplifiedEditor.SpriteAnimator.Controller
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class CustomControllerParameterDrawer : Attribute
    {
        public Type ParameterType { get; }
        /// <summary>
        /// Not yet implemented
        /// </summary>
        public bool UseForChildren { get; }

        public CustomControllerParameterDrawer(Type parameterType) =>
            ParameterType = parameterType;
        public CustomControllerParameterDrawer(Type parameterType, bool useForChildren) : this(parameterType) =>
            UseForChildren = useForChildren;
    }
}

#endif