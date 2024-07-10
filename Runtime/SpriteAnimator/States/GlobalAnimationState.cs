using System;
using System.Collections.Generic;
#if !UNITY_EDITOR
using UnityEngine;
#endif

namespace UnitySimplified.SpriteAnimator
{
    public abstract class GlobalAnimationState : AnimationState
    {
        private class DescendingOrderSorter : IComparer<GlobalAnimationState>
        {
            int IComparer<GlobalAnimationState>.Compare(GlobalAnimationState x, GlobalAnimationState y) => y.PrioritySort.CompareTo(x.PrioritySort);
        }

        private static List<GlobalAnimationState> _states;

        internal override bool IsGlobal => true;
        public virtual int PrioritySort => 0;
        public override int Priority => -1;
        public static IReadOnlyList<GlobalAnimationState> States
        {
            get
            {
                if (_states == null)
                    LoadGlobalAnimationStates();
                return _states;
            }
        }


#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        private static void LoadGlobalAnimationStates()
        {
            _states ??= new();    
            _states.Clear();
            foreach (var assembly in ApplicationUtility.GetAssemblies())
                foreach (var type in ApplicationUtility.GetTypesFromAssembly(assembly))
                    if (type.IsSubclassOf(typeof(GlobalAnimationState)))
                        _states.Add(Activator.CreateInstance(type) as GlobalAnimationState);
            _states.Sort(new DescendingOrderSorter());
        }

        public virtual void OnPlaying(AbstractSpriteAnimator animator) { }
        public virtual bool OnTryPlay(AbstractSpriteAnimator animator) => false;
    }
}