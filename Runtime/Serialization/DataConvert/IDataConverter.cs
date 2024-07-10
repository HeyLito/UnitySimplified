using System;
using System.Collections.Generic;

namespace UnitySimplified.Serialization
{
    public interface IDataConverter
    {
        int ConversionPriority() => 0;
        bool CanConvert(Type objectType);

        void Serialize(Type objectType, object value, IDictionary<string, object> output);
        object Deserialize(Type objectType, object existingValue, IDictionary<string, object> input);
    }

    public interface IDataConverter<T> : IDataConverter
    {
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(T);
        void IDataConverter.Serialize(Type objectType, object value, IDictionary<string, object> output) => Serialize((T)value, output);
        object IDataConverter.Deserialize(Type objectType, object existingValue, IDictionary<string, object> input) => Deserialize((T)existingValue, input);

        void Serialize(T value, IDictionary<string, object> output);
        T Deserialize(T existingValue, IDictionary<string, object> input);
    }
}