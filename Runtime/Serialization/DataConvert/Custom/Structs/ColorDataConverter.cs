using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class ColorDataConverter : IDataConverter<Color>
    {
        public void Serialize(Color value, IDictionary<string, object> output)
        {
            output[nameof(Color.r)] = value.r;
            output[nameof(Color.g)] = value.g;
            output[nameof(Color.b)] = value.b;
            output[nameof(Color.a)] = value.a;
        }
        public Color Deserialize(Color existingValue, IDictionary<string, object> input) => new()
        {
            r = input.TryGetValue(nameof(Color.r), out object rObject) ? rObject.CastDynamically<float>() : 0,
            g = input.TryGetValue(nameof(Color.g), out object gObject) ? gObject.CastDynamically<float>() : 0,
            b = input.TryGetValue(nameof(Color.b), out object bObject) ? bObject.CastDynamically<float>() : 0,
            a = input.TryGetValue(nameof(Color.a), out object aObject) ? aObject.CastDynamically<float>() : 0
        };
    }
}