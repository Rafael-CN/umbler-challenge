using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Services.Interfaces
{
    public interface IWhoIsClient
    {
        Task<WhoisResponse> QueryAsync(string query);
    }
}
