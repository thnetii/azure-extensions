using System;
using System.Net.Http;

using Microsoft.IdentityModel.Protocols;

namespace THNETII.AzureAcs.Client.Metadata
{
    public class AcsInstanceConfigurationManager
        : ConfigurationManager<AcsInstanceMetadata>
    {
        private static readonly IConfigurationRetriever<AcsInstanceMetadata>
            defaultConfigRetriever = new AcsInstanceMetadataRetriever();

        public AcsInstanceConfigurationManager(
            string realm, HttpClient? httpClient)
            : this(realm, instance: default, httpClient)
        { }

        public AcsInstanceConfigurationManager(
            string realm, string? instance, HttpClient? httpClient)
            : this(realm, instance ?? AcsDefaults.Instance,
                  configRetriever: default,
                  httpClient is not null ? new HttpDocumentRetriever(httpClient) : default)
        { }

        public AcsInstanceConfigurationManager(
            string realm, string? instance = AcsDefaults.Instance,
            AcsInstanceMetadataRetriever? configRetriever = default,
            HttpDocumentRetriever? documentRetriever = default)
            : base(
                  GetMetadataAddress(realm, instance),
                  configRetriever ?? defaultConfigRetriever,
                  documentRetriever ?? new HttpDocumentRetriever())
        { }

        public static string GetMetadataAddress(string realm, string? instance)
        {
            instance ??= AcsDefaults.Instance;
            const string slash = "/";
            const string metadataPath = AcsDefaults.MetadataPath + "?realm=";
            realm = Uri.EscapeDataString(realm ?? string.Empty);
            if (instance.AsSpan().EndsWith(slash.AsSpan(), StringComparison.Ordinal))
                return instance + metadataPath + realm;
            else
                return instance + slash + metadataPath + realm;
        }
    }
}
