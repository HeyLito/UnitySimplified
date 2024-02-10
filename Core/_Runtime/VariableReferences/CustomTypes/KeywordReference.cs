using System;

namespace UnitySimplified.VariableReferences
{
    /// <summary>
    /// Referenceable <see cref="string"/>-like tags.
    /// </summary>
    [Serializable]
    public class KeywordReference : VariableReference<string, KeywordAsset>
    {
        public KeywordReference(string value) : base(value) { }
        public static implicit operator string(KeywordReference reference) => reference.Value;
    }
}