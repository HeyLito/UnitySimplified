using System;

namespace UnitySimplified.Serialization 
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DontSaveField : Attribute { }
}