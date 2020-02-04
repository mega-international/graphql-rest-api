using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;

namespace Hopex.Common.JsonMessages
{
    public class DiagramExportArguments
    {
        [CLSCompliant(false)]
        [JsonProperty("format")]
        [JsonConverter(typeof(StringEnumConverter), converterParameters: typeof(CamelCaseNamingStrategy))]
        public ImageFormat Format { get; set; }
        [JsonProperty("quality")]
        public int Quality { get; set; } /* SaveAsPicture quality: 1 = best quality, 255 = max compression */
    }

    public enum ImageFormat
    {
        Png,
        Jpeg,
        Svg
    }
}
