using System;

namespace UnitySimplified.Serialization
{
    [Flags]
    public enum SerializerFlags
    {
        AssetReferences = 1 << 1,
        RuntimeReferences = 1 << 2,
        GenericVariables = 1 << 3,
        SerializedVariables = 1 << 4,
        SoftRuntimeVariables = 1 << 5,
        DeepRuntimeVariables = 1 << 6,
    }
}