using Newtonsoft.Json;
using System;

namespace Mega.WebService.GraphQL.Tests.Models.Safety
{
    class SafeFieldJsonConverter<T> : JsonConverter<SafeField<T>>
    {
        public override SafeField<T> ReadJson(JsonReader reader, Type objectType, SafeField<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, SafeField<T> value, JsonSerializer serializer)
        {
            T fieldValue = value.Get();
            serializer.Serialize(writer, fieldValue);
        }
    }
}
