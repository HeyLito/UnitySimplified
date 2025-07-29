using System;
using UnityEngine;

namespace UnitySimplified.Text
{
    [CreateAssetMenu(fileName = "New TextCommand", menuName = "Unity Simplified/Text Command")]
    public sealed class TextCommandAsset : ScriptableObject
    {
#if UNITY_EDITOR
#pragma warning disable CS0414 
        // ReSharper disable NotAccessedField.Local
        [SerializeField]
        [TextArea]
        private string editorDescription = "";
        // ReSharper restore NotAccessedField.Local
#pragma warning restore
#endif
        // ReSharper disable InconsistentNaming
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        [Space]
        [SerializeReference]
        [SerializeReferenceMenu]
        private ITextCommand[] textCommands = Array.Empty<ITextCommand>();
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        // ReSharper restore InconsistentNaming

        public bool TryParse(TextParser sender, object context, string command, string input, out string output)
        {
            output = input;
            foreach (var textCommand in textCommands)
                textCommand.TryParse(sender, context, command, output, out output);
            return input != output;
        }
    }
}