using UnityEngine;

namespace UnitySimplified.Text
{
    public interface ITextCommand
    {
        bool TryParse(TextParser sender, object context, string command, string input, out string output);
    }
}