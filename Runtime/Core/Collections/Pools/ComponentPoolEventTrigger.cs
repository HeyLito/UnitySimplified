using System;
using UnityEngine;

namespace UnitySimplified.Collections.Pools
{
    [AddComponentMenu("")]
    internal class ComponentPoolEventTrigger : MonoBehaviour
    {
        private Action<ComponentPoolEventTrigger> _callbackOnDisable;
        private Action<ComponentPoolEventTrigger> _callbackOnDestroy;
        
        public void Initialize(Component target, Action<ComponentPoolEventTrigger> callbackOnDisable, Action<ComponentPoolEventTrigger> callbackOnDestroy)
        {
            Target = target;
            _callbackOnDisable = callbackOnDisable;
            _callbackOnDestroy = callbackOnDestroy;
        }

        public Component Target { get; private set; }

        private void OnDisable() => _callbackOnDisable?.Invoke(this);
        private void OnDestroy()
        {
            _callbackOnDestroy?.Invoke(this);
            _callbackOnDisable = null;
            _callbackOnDestroy = null;
        }
    }
}