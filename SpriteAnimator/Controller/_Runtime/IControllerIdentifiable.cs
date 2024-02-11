using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnitySimplified.SpriteAnimator.Controller
{
    public interface IControllerIdentifiable
    {
        public string GetIdentifier();
        internal static string GenerateLocalUniqueIdentifier(ISet<string> existingIdentifiers)
        {
            Regex regex = new("[/+=]");
            string identifier;
            do identifier = regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "");
            while (existingIdentifiers.Contains(identifier));
            return identifier;
        }
    }
}