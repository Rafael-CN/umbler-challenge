#nullable enable

using Desafio.Umbler.Models;
using Desafio.Umbler.Repository;
using Desafio.Umbler.Services.Interfaces;
using DnsClient;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<Domain?> GetDomainAsync(string domainName)
        {
            var domain = await _domainRepository.GetDomainByNameAsync(domainName);
            if (domain == null || TtlExpired(domain))
            {
                domain = await LookupDomainAsync(domainName);

                if (domain != null && !string.IsNullOrEmpty(domain.Ip))
                {
                    await _domainRepository.UpsertDomainAsync(domain);
                }
            }

            return domain;
        }

        private async Task<Domain?> LookupDomainAsync(string domainName)
        {
            var result = await _lookupClient.QueryAsync(domainName, QueryType.ANY);

            var record = result.Answers.ARecords().FirstOrDefault();
            if (record == null) return null;

            var ip = record.Address.ToString();

            var response = await _whoIsClient.QueryAsync(domainName);
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

        private static bool TtlExpired(Domain domain)
        {
            var elapsedMinutes = DateTime.Now.Subtract(domain.UpdatedAt).TotalMinutes;
            return elapsedMinutes >= domain.Ttl;
        }
    }
}
