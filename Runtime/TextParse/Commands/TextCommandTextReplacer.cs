using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnitySimplified.Text
{
    [Serializable]
    public class TextCommandTextReplacer : ITextCommand
    {
        [Serializable]
        private struct MatchManipulation
        {
            [SerializeField]
            private string pattern;
            [SerializeField]
            private string replacement;

            private Regex _regex;

            public Regex Regex => _regex ??= new Regex(pattern);
            public string Replacement => replacement;
        }
#if UNITY_EDITOR
#pragma warning disable CS0414
        // ReSharper disable NotAccessedField.Local
        [SerializeField]
        private string editorDescription = "";
        // ReSharper restore NotAccessedField.Local
#pragma warning restore
#endif
        [SerializeField]
        private string pattern;
        [SerializeField, TextArea]
        private string replacement;
        [SerializeField, Tooltip("Used to change the return value from pattern. Use <match> in first replacement string to access the value.")]
        private MatchManipulation match;

        private Regex _patternRegex;
        private Regex _resultRegex;
        
        public bool TryParse(TextParser sender, object context, string command, string input, out string output)
        {
            _patternRegex ??= new Regex(pattern);
            _resultRegex ??= new Regex(@"\B<match>\B");
            output = _patternRegex.Replace(input, x =>
            {
                var result = match.Regex.Replace(x.Value, match.Replacement);
                return _resultRegex.Replace(replacement, result);
            });
            return !output.Equals(input);
        }
    }
}