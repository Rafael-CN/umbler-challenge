using Desafio.Umbler.Models;
using Desafio.Umbler.Repository;
using DnsClient;
using System;
using System.Linq;
using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Services
{
    public class DomainService
    {
        private readonly DomainRepository _domainRepository;

        public DomainService(DomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task<Domain> GetDomainAsync(string domainName)
        {
            var domain = await _domainRepository.GetDomainByNameAsync(domainName);
            if (domain == null)
            {
                domain = await LookupDomainAsync(domainName);
            } 
            else 
            {
                if (DateTime.Now.Subtract(domain.UpdatedAt).TotalMinutes > domain.Ttl)
                {
                    domain = await LookupDomainAsync(domainName);
                }
            }

            await _domainRepository.UpsertDomainAsync(domain);
            return domain;
        }

        private static async Task<Domain> LookupDomainAsync(string domainName)
        {
            var response = await WhoisClient.QueryAsync(domainName);

            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(domainName, QueryType.ANY);
            var record = result.Answers.ARecords().FirstOrDefault();
            var address = record?.Address;
            var ip = address?.ToString();

            var hostResponse = await WhoisClient.QueryAsync(ip);

            return new Domain
            {
                Name = domainName,
                Ip = ip,
                UpdatedAt = DateTime.Now,
                WhoIs = response.Raw,
                Ttl = record?.TimeToLive ?? 0,
                HostedAt = hostResponse.OrganizationName
            };
        }
    }
}
