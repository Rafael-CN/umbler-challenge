using Desafio.Umbler.Services.Interfaces;
using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Services.Wrappers
{
    public class WhoIsClientWrapper: IWhoIsClient
    {
        public async Task<WhoisResponse> QueryAsync(string query)
        {
            return await WhoisClient.QueryAsync(query);
        }
    }
}
