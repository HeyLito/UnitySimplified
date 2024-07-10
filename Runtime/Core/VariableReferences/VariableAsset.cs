using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
#pragma warning disable CS0414 // Field is assigned but its value is never used
// ReSharper disable ParameterHidesMember
// ReSharper disable NotAccessedField.Local
using UnityEditor;
#endif

namespace UnitySimplified.VariableReferences
{
    public abstract class VariableAsset : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, TextArea(3, 10)]
        [FormerlySerializedAs("_editorDescription")]
        private string editorDescription = "";
#endif
    }

    public abstract class VariableAsset<T> : VariableAsset, IVariableAsset<T>, ISerializationCallbackReceiver
    {
        [SerializeField]
        [FormerlySerializedAs("_value")]
        private T value;
        [SerializeField]
        [FormerlySerializedAs("_runtimeValue")]
        private T runtimeValue;

        public T GetValue() => runtimeValue;
        public void SetValue(T value) => runtimeValue = value;
        public virtual void SetValue(IVariableAsset<T> otherValue) => runtimeValue = otherValue.GetValue();

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                runtimeValue = value;
#endif
        }
    }
}