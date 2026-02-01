using DnsClient;
using System.Threading.Tasks;

namespace Desafio.Umbler.Services.Interfaces
{
    public interface IDnsLookupClient
    {
        Task<IDnsQueryResponse> QueryAsync(string domainName, QueryType queryType);
    }
}
