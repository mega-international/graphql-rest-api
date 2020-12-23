using Newtonsoft.Json;

namespace Hopex.Common.JsonMessages
{
    public class DatasetArguments
    {
        [JsonProperty("regenerate")]
        public bool Regenerate { get; set; }
        public DatasetNullValues NullValues { get; set; } = DatasetNullValues.FirstLine;
    }

    public enum DatasetNullValues
    {
        Never = 0,
        FirstLine = 1,
        Always = 2
    }
}
