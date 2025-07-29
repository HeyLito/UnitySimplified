using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnitySimplified.Text
{
    [Serializable]
    public class TextParser
    {
#if UNITY_EDITOR
#pragma warning disable CS0414
        [SerializeField]
        // ReSharper disable NotAccessedField.Local
        private string editorDescription = "";
        // ReSharper restore NotAccessedField.Local
#pragma warning restore
#endif
        // ReSharper disable InconsistentNaming
        [SerializeReference]
        [SerializeReferenceMenu]
        private ITextCommand[] textCommands = Array.Empty<ITextCommand>();
        // ReSharper restore InconsistentNaming

        private Regex _commandRegex;
        private readonly string _commandPattern = @"\[.*?\]";

        public bool TryParse(object context, string input, out string output)
        {
            _commandRegex ??= new Regex(_commandPattern);

            output = input;
            foreach (var textCommand in textCommands)
            {
                textCommand.TryParse(this, context, "REGEX", output, out output);
                output = _commandRegex.Replace(output, y => textCommand.TryParse(this, context, y.Value[1..^1], y.Value, out string innerOutput) ? innerOutput : y.Value);
            }

            return input != output;
        }
    }
}
