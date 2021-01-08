
using Newtonsoft.Json;

namespace THNETII.AzureAcs.Client.Metadata
{
    public class AcsInstanceJwkSetItem
    {
        [JsonProperty("usage")]
        public string Usage { get; set; } = null!;
        [JsonProperty("keyValue")]
        public AcsInstanceJwkKeyValue Value { get; set; } = null!;
    }
}
