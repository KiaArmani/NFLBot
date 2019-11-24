using System.Threading.Tasks;
using BungieNet.Destiny.Config;

namespace BungieNet.Api
{
    public interface IDestiny1Client
    {
        [Undocumented]
        DestinyManifest GetDestinyManifest();

        [Undocumented]
        Task<DestinyManifest> GetDestinyManifestAsync();
    }

    partial interface IBungieClient
    {
        [Undocumented] IDestiny1Client Destiny1 { get; }
    }

    partial class BungieClient : IDestiny1Client
    {
        [Undocumented] public IDestiny1Client Destiny1 => this;


        DestinyManifest IDestiny1Client.GetDestinyManifest()
        {
            return Destiny1.GetDestinyManifestAsync().GetAwaiter().GetResult();
        }

        Task<DestinyManifest> IDestiny1Client.GetDestinyManifestAsync()
        {
            string[] pathSegments = {"Destiny", "Manifest"};
            var uri = GetEndpointUri(pathSegments, true, null, true);
            return GetEntityAsync<DestinyManifest>(uri);
        }
    }
}