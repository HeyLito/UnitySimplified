namespace UnitySimplified.VariableReferences
{
    public interface IVariableAsset<T>
    {
        public T GetValue();
        public void SetValue(T value);
        public void SetValue(IVariableAsset<T> variableAsset);
    }
}
