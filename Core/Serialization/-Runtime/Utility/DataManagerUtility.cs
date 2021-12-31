using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public static class DataManagerUtility
    {
        public static bool IsSerializable(Type type)
        {
            if (DataManager.SurrogateSelector != null && DataManager.SurrogateSelector.GetSurrogate(type, new StreamingContext(StreamingContextStates.All), out _) != null)
                return true;
            if (type.IsSerializable)
                return true;
            return false;
        }
    }
}