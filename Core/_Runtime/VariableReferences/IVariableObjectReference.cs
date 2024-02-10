namespace UnitySimplified.VariableReferences
{
    public interface IVariableObjectReference<T>
    {
        public T GetValue();
        public void SetValue(T value);
        public void SetValue(IVariableObjectReference<T> otherValue);
    }
}
