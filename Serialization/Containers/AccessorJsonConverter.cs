#if ENABLE_UNITYSIMPLIFIED_NEWTONSOFT

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnitySimplified.Serialization.Containers
{
    public class AccessorJsonConverter : JsonConverter
    {
        private readonly string _nameofType = "$accessorType";

        public override bool CanConvert(Type objectType) => throw new NotImplementedException();
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jObject = new();

            var valueType = value.GetType();
            if (!valueType.IsSerializable || !Accessor.TypeToName(valueType, out string valueTypeName))
            {
                jObject.WriteTo(writer);
                return;
            }
            jObject.Add(new JProperty(_nameofType, JToken.FromObject(valueTypeName, serializer)));

            foreach (var fieldInfo in valueType.GetSerializedFields(typeof(JsonIgnoreAttribute)))
                jObject.Add(fieldInfo.Name, JToken.FromObject(fieldInfo.GetValue(value), serializer));
            jObject.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            if (!jsonObject.TryGetValue(_nameofType, out var valueTypeNameToken))
                return null;
            var valueTypeName = (string)valueTypeNameToken.ToObject(typeof(string), serializer);
            if (valueTypeName == null)
                return null;
            if (!Accessor.NameToType(valueTypeName, out Type valueType))
                return null;
            var value = (Accessor)Activator.CreateInstance(valueType);
            if (value == null)
                return null;

            foreach (var fieldInfo in valueType.GetSerializedFields())
            {
                var tokenValue = jsonObject[fieldInfo.Name];
                if (tokenValue == null || tokenValue.Type == JTokenType.Null)
                    continue;
                var fieldValue = tokenValue.ToObject(fieldInfo.FieldType);
                fieldInfo.SetValue(value, fieldValue);
            }
            return value;
        }
    }
}

#endif