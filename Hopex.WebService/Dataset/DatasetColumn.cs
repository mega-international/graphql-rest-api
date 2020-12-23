using Hopex.Model.Abstractions;
using Newtonsoft.Json;

namespace Hopex.Modules.GraphQL.Dataset
{
    public class DatasetColumn
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }
        [JsonProperty("label")]
        public string Label { get; internal set; }

        internal PropertyFormatter Formatter { get; private set; }

        internal PropertyFormatter GetFormatter(IMegaObject line)
        {
            if (Formatter == null)
            {
                var metaProperty = line.GetTypeObject()
                    .GetCollection("~7fs9P58ig1fC[Properties]")
                    .Item("~" + Id);
                var metaAttributeFormat = metaProperty.GetPropertyValue("~rhs9P5uNf1fC[Tabulated]");
                var metaAttributeType = metaProperty.GetPropertyValue("~mhs9P5eMf1fC[Format]");
                Formatter = PropertyFormatter.Create(Id, metaAttributeFormat, metaAttributeType);
            }
            return Formatter;
        }
    }

}
