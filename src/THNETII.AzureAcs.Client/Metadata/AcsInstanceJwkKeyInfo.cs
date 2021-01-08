
using Newtonsoft.Json;

namespace THNETII.AzureAcs.Client.Metadata
{
    public class AcsInstanceJwkKeyInfo
    {
        [JsonProperty("x5t")]
        public string CertificateThumbprint { get; set; } = null!;
    }
}
