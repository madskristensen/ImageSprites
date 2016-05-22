using System.Collections.Generic;
using Newtonsoft.Json;

namespace ImageSprites
{
    public class Stylesheet
    {
        [JsonProperty("root", NullValueHandling = NullValueHandling.Ignore)]
        public string Root { get; set; }

        [JsonProperty("formats")]
        public IEnumerable<ExportFormat> Formats { get; set; } = new[] { ExportFormat.Less };
    }
}
