using System;
using UnityEngine;

namespace UnitySimplified.Text
{
    [Serializable]
    public class TextCommandAssetReference : ITextCommand
    {
        [SerializeField]
        private TextCommandAsset asset;

        bool ITextCommand.TryParse(TextParser sender, object context, string command, string input, out string output) => asset.TryParse(sender, context, command, input, out output);
    }
}