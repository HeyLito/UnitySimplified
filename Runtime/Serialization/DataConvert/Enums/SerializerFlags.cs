using System;

namespace UnitySimplified.Serialization
{
    [Flags]
    public enum SerializerFlags
    {
        SerializedVariable = 1 << 1,
        NonSerializedVariable = 1 << 2,
        AssetReference = 1 << 3,
        RuntimeReference = 1 << 4,
    }
}