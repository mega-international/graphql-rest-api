using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hopex.Model.DataModel
{
    public class SearchAllResultRoot
    {
        [JsonProperty("results")]
        public SearchAllResults Results { get; set; }
    }

    public class SearchAllResults
    {
        public string SearchedString { get; set; }
        public string Language { get; set; }
        [JsonProperty("Occresults")]
        public OccResults OccResults { get; set; }
        public string Message { get; set; } 
        public string ParsedString { get; set; }
        public int ExhaustiveList { get; set; }
        public int FoundConfidentialResult { get; set; }
    }

    public class OccResults
    {
        public int MinRange { get; set; }
        public int MaxRange { get; set; }
        public List<Occ> OccList { get; set; }
        public int OccCount { get; set; }
    }

    public class Occ
    {
        public string ObjectId { get; set; }
        public string MetaclassId { get; set; }
        public string ObjectPath { get; set; }
        public string ObjectName { get; set; }
        public string ObjectIcon { get; set; }
        public string MetaclassName { get; set; }
        public int HitCount { get; set; }
        public int Ranking { get; set; }
        public List<Detail> Details { get; set; }
        public List<FoundWord> FoundWords { get; set; }
        public string ObjectComment { get; set; }
        public List<Location> Locations { get; set; }
    }

    public class Detail
    {
        public string AttributeId { get; set; }
        public string AttributeName { get; set; }
    }

    public class FoundWord
    {
        public string Word { get; set; }
    }

    public class Location
    {
        public string ObjectId { get; set; }
        public string ObjectName { get; set; }
        public string ObjectIcon { get; set; }
        public string MetaclassId { get; set; }
        public string MetaclassName { get; set; }
        public int HitCount { get; set; }
        public int Ranking { get; set; }
        public List<Detail> Details { get; set; }
        public List<FoundWord> FoundWords { get; set; }
    }
}
