
using Newtonsoft.Json;

namespace THNETII.AzureAcs.Client.Metadata
{
    public class AcsInstanceJwkKeyValue
    {
        [JsonProperty("type")]
        public string Type { get; set; } = null!;
        [JsonProperty("value")]
        public string Base64Data { get; set; } = null!;
        [JsonProperty("keyInfo")]
        public AcsInstanceJwkKeyInfo Information { get; set; } = null!;
    }
}
