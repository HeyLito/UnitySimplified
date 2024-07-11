namespace UnitySimplified.VariableReferences
{
    public interface IVariableReference<out TValue, out TReference> where TReference : IVariableAsset<TValue>
    {
        public TValue Constant { get; }
        public TValue Value { get; }
        public TReference Reference { get; }
    }
}