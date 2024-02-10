using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnitySimplified.VariableReferences
{
    public abstract class VariableObjectReference : ScriptableObject
    {
#if UNITY_EDITOR
#pragma warning disable CS0414 // Field is assigned but its value is never used
        [SerializeField, TextArea(3, 10)]
        private string _editorDescription = "";
#pragma warning restore CS0414 // Field is assigned but its value is never used
#endif
    }

    public abstract class VariableObjectReference<T> : VariableObjectReference, IVariableObjectReference<T>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private T _value = default;
        [SerializeField]
        private T _runtimeValue;

        public T GetValue() => _runtimeValue;
        public void SetValue(T value) => _runtimeValue = value;
        public virtual void SetValue(IVariableObjectReference<T> otherValue) => _runtimeValue = otherValue.GetValue();

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                _runtimeValue = _value;
#endif
        }
    }
}