using Desafio.Umbler.Services.Interfaces;
using DnsClient;
using System.Threading.Tasks;

namespace Desafio.Umbler.Services.Wrappers
{
    public class DnsLookupClientWrapper: IDnsLookupClient
    {
        private readonly LookupClient _lookupClient = new();

        public async Task<IDnsQueryResponse> QueryAsync(string domainName, QueryType queryType)
        {
            return await _lookupClient.QueryAsync(domainName, queryType);
        }
    }
}
