using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Desafio.Umbler.Models;
using Desafio.Umbler.Services.Interfaces;

using DnsClient;
using DnsClient.Protocol;
using Whois.NET;
using Moq;

namespace Desafio.Umbler.Test.Helpers
{
    public static class TestHelper
    {
        public static DbContextOptions<DatabaseContext> CreateInMemoryDatabaseOptions()
        {
            return new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        public static Mock<IDnsQueryResponse> CreateMockDnsResponse(string ip, int ttl)
        {
            var mockDnsResponse = new Mock<IDnsQueryResponse>();

            var aRecord = new ARecord(
                new ResourceRecordInfo("test.com", ResourceRecordType.A, QueryClass.IN, ttl, 4),
                IPAddress.Parse(ip)
            );

            var answers = new List<DnsResourceRecord> { aRecord };
            mockDnsResponse.Setup(r => r.Answers).Returns(answers);

            return mockDnsResponse;
        }

        public static Mock<IDnsQueryResponse> CreateEmptyDnsResponse()
        {
            var mockDnsResponse = new Mock<IDnsQueryResponse>();
            mockDnsResponse.Setup(r => r.Answers).Returns(new List<DnsResourceRecord>());
            return mockDnsResponse;
        }

        public static Mock<IWhoIsClient> CreateMockWhoIsClientResponse(string domain, string ip, string organization = "Example corp")
        {
            var mockWhoIs = new Mock<IWhoIsClient>();
            mockWhoIs.Setup(m => m.QueryAsync(domain))
                     .ReturnsAsync(new WhoisResponse { Raw = "WhoIs Raw Data" });
            mockWhoIs.Setup(m => m.QueryAsync(ip))
                     .ReturnsAsync(new WhoisResponse { OrganizationName = organization });
            return mockWhoIs;
        }

        public static Domain CreateDomain(
             string name = "test.com",
             string ip = "192.168.0.1",
             string hostedAt = "Test Host",
             int ttl = 60,
             int minutesAgo = 0)
        {
            return new Domain
            {
                Id = 1,
                Name = name,
                Ip = ip,
                HostedAt = hostedAt,
                Ttl = ttl,
                UpdatedAt = DateTime.Now.AddMinutes(-minutesAgo),
                WhoIs = "whois data"
            };
        }
    }
}
