using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class Vector2IntDataConverter : IDataConverter<Vector2Int>
    {
        public void Serialize(Vector2Int value, IDictionary<string, object> output)
        {
            output[nameof(Vector2.x)] = value.x;
            output[nameof(Vector2.y)] = value.y;
        }
        public Vector2Int Deserialize(Vector2Int existingValue, IDictionary<string, object> input) => new()
        {
            x = input.TryGetValue(nameof(Vector2Int.x), out object xObject) ? xObject.CastDynamically<int>() : 0,
            y = input.TryGetValue(nameof(Vector2Int.y), out object yObject) ? yObject.CastDynamically<int>() : 0
        };
    }
}