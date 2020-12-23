using Newtonsoft.Json;

namespace Hopex.Model.DataModel
{
    public class SearchAllRequestRoot
    {
        [JsonProperty("request")]
        public SearchAllRequest Request { get; set; }
    }

    public class SearchAllRequest
    {
       public string Value { get; set; }
       public string Language { get; set; }
       public int MinRange { get; set; }
       public int MaxRange { get; set; }
       public string SortColumn { get; set; }
       public string SortDirection { get; set; }
    }
}
