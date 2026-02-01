using Desafio.Umbler.Controllers;
using Desafio.Umbler.Controllers.DTOs;
using Desafio.Umbler.Models;
using Desafio.Umbler.Repository;
using Desafio.Umbler.Services;
using Desafio.Umbler.Services.Interfaces;
using DnsClient;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class DomainControllerTests
    {
        private DbContextOptions<DatabaseContext> CreateInMemoryDatabaseOptions()
        {
            return new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private Mock<IDnsQueryResponse> CreateMockDnsResponse(string ip, int ttl)
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

        [TestMethod]
        public async Task Get_DomainInCache_ReturnsFromDatabase()
        {
            // Arrange
            var options = CreateInMemoryDatabaseOptions();
            var domain = new Domain
            {
                Id = 1,
                Name = "test.com",
                Ip = "192.168.0.1",
                UpdatedAt = DateTime.Now,
                HostedAt = "umbler.corp",
                Ttl = 60,
                WhoIs = "Ns.umbler.com"
            };

            using (var db = new DatabaseContext(options))
            {
                db.Domains.Add(domain);
                await db.SaveChangesAsync();
            }

            var mockWhoIs = new Mock<IWhoIsClient>();
            var mockDns = new Mock<IDnsLookupClient>();

            using (var db = new DatabaseContext(options))
            {
                var repository = new DomainRepository(db);
                var service = new DomainService(repository, mockWhoIs.Object, mockDns.Object);
                var controller = new DomainController(service);

                // Act
                var response = await controller.Get("test.com");
                var result = response as OkObjectResult;
                var dto = result?.Value as DomainDTO;

                // Assert
                Assert.IsNotNull(dto);
                Assert.AreEqual("test.com", dto.Name);
                Assert.AreEqual("192.168.0.1", dto.Ip);
                Assert.AreEqual("umbler.corp", dto.HostedAt);

                mockWhoIs.Verify(m => m.QueryAsync(It.IsAny<string>()), Times.Never);
                mockDns.Verify(m => m.QueryAsync(It.IsAny<string>(), It.IsAny<QueryType>()), Times.Never);
            }
        }

        [TestMethod]
        public async Task Get_DomainNotInCache_FetchesFromExternalServices()
        {
            // Arrange
            var options = CreateInMemoryDatabaseOptions();

            var mockWhoIs = new Mock<IWhoIsClient>();
            mockWhoIs.Setup(m => m.QueryAsync("test.com"))
                     .ReturnsAsync(new WhoisResponse { Raw = "WhoIs Raw Data" });
            mockWhoIs.Setup(m => m.QueryAsync("93.184.216.34"))
                     .ReturnsAsync(new WhoisResponse { OrganizationName = "Example Corp" });

            var mockDns = new Mock<IDnsLookupClient>();
            var mockDnsResponse = CreateMockDnsResponse("93.184.216.34", 300);
            mockDns.Setup(m => m.QueryAsync("test.com", QueryType.ANY))
                   .ReturnsAsync(mockDnsResponse.Object);

            using (var db = new DatabaseContext(options))
            {
                var repository = new DomainRepository(db);
                var service = new DomainService(repository, mockWhoIs.Object, mockDns.Object);
                var controller = new DomainController(service);

                // Act
                var response = await controller.Get("test.com");
                var result = response as OkObjectResult;
                var dto = result?.Value as DomainDTO;

                // Assert
                Assert.IsNotNull(dto);
                Assert.AreEqual("test.com", dto.Name);
                Assert.AreEqual("93.184.216.34", dto.Ip);
                Assert.AreEqual("Example Corp", dto.HostedAt);

                mockWhoIs.Verify(m => m.QueryAsync("test.com"), Times.Once);
                mockWhoIs.Verify(m => m.QueryAsync("93.184.216.34"), Times.Once);
                mockDns.Verify(m => m.QueryAsync("test.com", QueryType.ANY), Times.Once);
            }
        }

        [TestMethod]
        public async Task Get_DomainWithExpiredTTL_RefreshesCacheFromExternalServices()
        {
            // Arrange
            var options = CreateInMemoryDatabaseOptions();
            var expiredDomain = new Domain
            {
                Id = 1,
                Name = "test.com",
                Ip = "1.1.1.1",
                UpdatedAt = DateTime.Now.AddMinutes(-120),
                HostedAt = "old-host",
                Ttl = 60,
                WhoIs = "old whois"
            };

            using (var db = new DatabaseContext(options))
            {
                db.Domains.Add(expiredDomain);
                await db.SaveChangesAsync();
            }

            var mockWhoIs = new Mock<IWhoIsClient>();
            mockWhoIs.Setup(m => m.QueryAsync("test.com"))
                     .ReturnsAsync(new WhoisResponse { Raw = "Fresh WhoIs Data" });
            mockWhoIs.Setup(m => m.QueryAsync("8.8.8.8"))
                     .ReturnsAsync(new WhoisResponse { OrganizationName = "Google LLC" });

            var mockDns = new Mock<IDnsLookupClient>();
            var mockDnsResponse = CreateMockDnsResponse("8.8.8.8", 300);
            mockDns.Setup(m => m.QueryAsync("test.com", QueryType.ANY))
                   .ReturnsAsync(mockDnsResponse.Object);

            using (var db = new DatabaseContext(options))
            {
                var repository = new DomainRepository(db);
                var service = new DomainService(repository, mockWhoIs.Object, mockDns.Object);
                var controller = new DomainController(service);

                // Act
                var response = await controller.Get("test.com");
                var result = response as OkObjectResult;
                var dto = result?.Value as DomainDTO;

                // Assert
                Assert.IsNotNull(dto);
                Assert.AreEqual("test.com", dto.Name);
                Assert.AreEqual("8.8.8.8", dto.Ip);
                Assert.AreEqual("Google LLC", dto.HostedAt);

                mockWhoIs.Verify(m => m.QueryAsync("test.com"), Times.Once);
                mockDns.Verify(m => m.QueryAsync("test.com", QueryType.ANY), Times.Once);
            }
        }

        [TestMethod]
        public async Task Get_DomainInCacheWithValidTTL_DoesNotCallExternalServices()
        {
            // Arrange
            var options = CreateInMemoryDatabaseOptions();
            var validDomain = new Domain
            {
                Id = 1,
                Name = "cached.com",
                Ip = "10.0.0.1",
                UpdatedAt = DateTime.Now.AddMinutes(-5),
                HostedAt = "Cached Host",
                Ttl = 60,
                WhoIs = "cached whois"
            };

            using (var db = new DatabaseContext(options))
            {
                db.Domains.Add(validDomain);
                await db.SaveChangesAsync();
            }

            var mockWhoIs = new Mock<IWhoIsClient>();
            var mockDns = new Mock<IDnsLookupClient>();

            using (var db = new DatabaseContext(options))
            {
                var repository = new DomainRepository(db);
                var service = new DomainService(repository, mockWhoIs.Object, mockDns.Object);
                var controller = new DomainController(service);

                // Act
                var response = await controller.Get("cached.com");
                var result = response as OkObjectResult;
                var dto = result?.Value as DomainDTO;

                // Assert
                Assert.IsNotNull(dto);
                Assert.AreEqual("cached.com", dto.Name);
                Assert.AreEqual("10.0.0.1", dto.Ip);
                Assert.AreEqual("Cached Host", dto.HostedAt);

                mockWhoIs.Verify(m => m.QueryAsync(It.IsAny<string>()), Times.Never);
                mockDns.Verify(m => m.QueryAsync(It.IsAny<string>(), It.IsAny<QueryType>()), Times.Never);
            }
        }
    }
}
