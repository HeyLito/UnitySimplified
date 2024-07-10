using System;

namespace UnitySimplified.VariableReferences
{
    /// <summary>
    /// Reference-able <see cref="string"/>-like tags.
    /// </summary>
    [Serializable]
    public class KeywordReference : VariableReference<string, KeywordAsset>
    {
        public KeywordReference(string value) : base(value) { }
        public static implicit operator string(KeywordReference reference) => reference.Value;

        public override string ToString() =>
            nameof(KeywordReference) + "{ " + $"{(ValueToggle ? $"Constant: {nameof(String)}({Constant})" : $"Reference: {nameof(KeywordAsset)}({Reference})")}" + " }";
    }
}