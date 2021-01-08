using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;

namespace THNETII.AzureAcs.Client.Metadata
{
    public class AcsInstanceMetadata
    {
        [JsonProperty("keys")]
        public ICollection<AcsInstanceJwkSetItem> JwkSet { get; } =
            new List<AcsInstanceJwkSetItem>();
        [JsonProperty("endpoints")]
        public ICollection<AcsInstanceEndpointDescriptor> Endpoints { get; } =
            new List<AcsInstanceEndpointDescriptor>(capacity: 3);
        [JsonProperty("version")]
        public string Version { get; set; } = null!;
        [JsonProperty("realm")]
        public string Realm { get; set; } = null!;
        [JsonProperty("name")]
        public string Name { get; set; } = null!;
        [JsonProperty("allowedAudiences")]
        public ICollection<string> AllowedAudiences { get; } =
            new List<string>();
        [JsonProperty("issuer")]
        public string Issuer { get; set; } = null!;

        public IEnumerable<SecurityKey> GetSecurityKeys()
        {
            return JwkSet.Select(entry =>
            {
                var keyMeta = entry.Value;
                byte[] keyData;
                switch (keyMeta.Type)
                {
                    case string t when t.Equals("x509Certificate", StringComparison.Ordinal):
                        keyData = Convert.FromBase64String(keyMeta.Base64Data);
                        var cert = new X509Certificate2(keyData);
                        var key = new X509SecurityKey(cert);
                        return key;
                    default:
                        throw new InvalidOperationException("Unknown security key type: " + keyMeta.Type);
                }
            });
        }

        public Uri GetOAuth2TokenEndpoint()
        {
            return new Uri(Endpoints.First(e =>
                e.Protocol.Equals("OAuth2", StringComparison.OrdinalIgnoreCase) &&
                e.Usage.Equals("issuance", StringComparison.OrdinalIgnoreCase))
                .Location);
        }

        public void ConfigureClientAssertionValidation(
            TokenValidationParameters parameters)
        {
            if (parameters is null)
                return;

            parameters.ValidateAudience = true;
            parameters.ValidAudiences = AllowedAudiences;
            parameters.IgnoreTrailingSlashWhenValidatingAudience = true;
        }

        public void ConfigureAccessTokenValidation(
            TokenValidationParameters parameters)
        {
            if (parameters is null)
                return;

            parameters.ValidateIssuerSigningKey = true;
            parameters.IssuerSigningKey = null;
            parameters.IssuerSigningKeys = GetSecurityKeys().ToList();
        }
    }
}
