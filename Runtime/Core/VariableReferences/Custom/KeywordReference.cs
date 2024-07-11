using System;

namespace UnitySimplified.VariableReferences
{
    /// <summary>
    /// Reference-able <see cref="string"/>-like tags.
    /// </summary>
    [Serializable]
    public class KeywordReference : VariableReference<string, KeywordAsset>, IVariableReference<string, IVariableAsset<string>>
    {
        public KeywordReference(string value) : base(value) { }

        IVariableAsset<string> IVariableReference<string, IVariableAsset<string>>.Reference => Reference;


        public static implicit operator string(KeywordReference reference) => reference.Value;
        public static implicit operator KeywordReference(string value) => new(value);
        public override string ToString() => nameof(KeywordReference) + "{ " + $"{(ValueToggle ? $"Constant: {nameof(String)}({Constant})" : $"Reference: {nameof(KeywordAsset)}({Reference})")}" + " }";
    }
}