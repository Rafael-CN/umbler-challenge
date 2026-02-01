using Desafio.Umbler.Models;
using Desafio.Umbler.Repository;
using Desafio.Umbler.Services.Interfaces;
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
        private readonly IWhoIsClient _whoIsClient;
        private readonly IDnsLookupClient _lookupClient;

        public DomainService(DomainRepository domainRepository, IWhoIsClient whoIsClient, IDnsLookupClient lookupClient)
        {
            _domainRepository = domainRepository;
            _whoIsClient = whoIsClient;
            _lookupClient = lookupClient;
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

        private async Task<Domain> LookupDomainAsync(string domainName)
        {
            var response = await _whoIsClient.QueryAsync(domainName);
            var result = await _lookupClient.QueryAsync(domainName, QueryType.ANY);

            var record = result.Answers.ARecords().FirstOrDefault();
            var address = record?.Address;
            var ip = address?.ToString();

            var hostResponse = await _whoIsClient.QueryAsync(ip);

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
