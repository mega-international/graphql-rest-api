using Mega.WebService.GraphQL.Tests.Models.Interfaces.Safety;
using Newtonsoft.Json;

namespace Mega.WebService.GraphQL.Tests.Models.Safety
{
    [JsonConverter(typeof(SafeFieldJsonConverter<string>))]
    public class StringSafeField : SafeField<string>, IAddableSafeField<string>
    {
        public StringSafeField(ISafeClass safeClass) : base(safeClass) { }

        public override void Reset()
        {
            _value = string.Empty;
        }

        public void Add(string value)
        {
            _value += value;
        }
    }
}
