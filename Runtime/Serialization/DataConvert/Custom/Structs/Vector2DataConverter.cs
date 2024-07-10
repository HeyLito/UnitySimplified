using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class Vector2DataConverter : IDataConverter<Vector2>
    {
        public void Serialize(Vector2 value, IDictionary<string, object> output)
        {
            output[nameof(Vector2.x)] = value.x;
            output[nameof(Vector2.y)] = value.y;
        }
        public Vector2 Deserialize(Vector2 existingValue, IDictionary<string, object> input) => new()
        {
            x = input.TryGetValue(nameof(Vector3.x), out object xObject) ? xObject.CastDynamically<float>() : 0,
            y = input.TryGetValue(nameof(Vector3.y), out object yObject) ? yObject.CastDynamically<float>() : 0
        };
    }
}