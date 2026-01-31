#nullable enable
using Desafio.Umbler.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Desafio.Umbler.Repository
{
    public class DomainRepository
    {
        private readonly DatabaseContext _db;

        public DomainRepository(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<Domain?> GetDomainByNameAsync(string domainName) => await _db.Domains.FirstOrDefaultAsync(d => d.Name == domainName);

        public async Task<int> UpsertDomainAsync(Domain domain)
        {
            var existingDomain = await GetDomainByNameAsync(domain.Name);
            if (existingDomain == null)
            {
                _db.Domains.Add(domain);
            }
            else
            {
                existingDomain.Ip = domain.Ip;
                existingDomain.UpdatedAt = domain.UpdatedAt;
                existingDomain.WhoIs = domain.WhoIs;
                existingDomain.Ttl = domain.Ttl;
                existingDomain.HostedAt = domain.HostedAt;
            }
            return await _db.SaveChangesAsync();
        }
    }
}
