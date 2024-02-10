using System;

namespace UnitySimplified.Serialization
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FindCustomSerializer : Attribute { }
}