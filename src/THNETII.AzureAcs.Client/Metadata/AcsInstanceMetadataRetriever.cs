using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;

using Newtonsoft.Json;

namespace THNETII.AzureAcs.Client.Metadata
{
    public class AcsInstanceMetadataRetriever
        : IConfigurationRetriever<AcsInstanceMetadata>
    {
        public static Task<AcsInstanceMetadata>
            GetAsync(string address, CancellationToken cancelToken = default) =>
            GetAsync(address,
                new HttpDocumentRetriever(),
                cancelToken);

        public static Task<AcsInstanceMetadata>
            GetAsync(string address, HttpClient httpClient,
                CancellationToken cancelToken = default) =>
            GetAsync(address,
                new HttpDocumentRetriever(httpClient ??
                    throw LogHelper.LogArgumentNullException(nameof(httpClient))),
                cancelToken);

        public static async Task<AcsInstanceMetadata>
            GetAsync(string address, IDocumentRetriever documentRetriever,
                CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(address))
                throw LogHelper.LogArgumentNullException(nameof(address));
            if (documentRetriever is null)
                throw LogHelper.LogArgumentNullException(nameof(documentRetriever));

            string metadataJson = await documentRetriever
                .GetDocumentAsync(address, cancelToken)
                .ConfigureAwait(continueOnCapturedContext: false);
            LogHelper.LogVerbose(LogMessages.ACS2001, metadataJson);
            return JsonConvert.DeserializeObject<AcsInstanceMetadata>(metadataJson);
        }

        Task<AcsInstanceMetadata> IConfigurationRetriever<AcsInstanceMetadata>
            .GetConfigurationAsync(string address,
                IDocumentRetriever retriever,
                CancellationToken cancelToken) =>
            GetAsync(address, retriever, cancelToken);
    }
}
