using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Desafio.Umbler.Models;
using Desafio.Umbler.Services;
using Desafio.Umbler.Repository;
using Desafio.Umbler.Test.Helpers;
using Desafio.Umbler.Services.Interfaces;

using DnsClient;
using Moq;

namespace Desafio.Umbler.Test.Services
{
    [TestClass]
    public class DomainServiceTests
    {
        [TestMethod]
        public async Task GetDomainAsync_DomainNotInCache_CallsExternalServices()
        {
            // Arrange
            var options = TestHelper.CreateInMemoryDatabaseOptions();
            
            var mockWhoIs = TestHelper.CreateMockWhoIsClientResponse("test.com", "1.1.1.1", "Test org");
            var mockDns = new Mock<IDnsLookupClient>();
            mockDns.Setup(m => m.QueryAsync("test.com", QueryType.ANY))
                                   .ReturnsAsync(TestHelper.CreateMockDnsResponse("1.1.1.1", 300).Object);

            using var db = new DatabaseContext(options);
            var repository = new DomainRepository(db);
            var service = new DomainService(repository, mockWhoIs.Object, mockDns.Object);

            // Act
            var result = await service.GetDomainAsync("test.com");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("1.1.1.1", result.Ip);

            mockDns.Verify(m => m.QueryAsync("test.com", QueryType.ANY), Times.Once);

        }

        [TestMethod]
        public async Task GetDomainAsync_DnsReturnsNoRecords_ReturnsNull()
        {
            // Arrange
            var options = TestHelper.CreateInMemoryDatabaseOptions();

            var mockWhoIs = new Mock<IWhoIsClient>();
            var mockDns = new Mock<IDnsLookupClient>();
            mockDns.Setup(m => m.QueryAsync("unknown.com", QueryType.ANY))
                                   .ReturnsAsync(TestHelper.CreateEmptyDnsResponse().Object);

            using var db = new DatabaseContext(options);
            var repository = new DomainRepository(db);
            var service = new DomainService(repository, mockWhoIs.Object, mockDns.Object);

            // Act
            var result = await service.GetDomainAsync("unknown.com");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetDomainAsync_DnsReturnsNoRecord_DoesNotSaveDatabase()
        {
            // Arrange
            var options = TestHelper.CreateInMemoryDatabaseOptions();

            var mockWhoIs = new Mock<IWhoIsClient>();
            var mockDns = new Mock<IDnsLookupClient>();
            mockDns.Setup(m => m.QueryAsync(It.IsAny<string>(), QueryType.ANY))
                                   .ReturnsAsync(TestHelper.CreateEmptyDnsResponse().Object);

            // Act
            using (var db = new DatabaseContext(options))
            {
                var repository = new DomainRepository(db);
                var service = new DomainService(repository, mockWhoIs.Object, mockDns.Object);
                await service.GetDomainAsync("unknown.com");
            }

            // Assert
            using (var db = new DatabaseContext(options))
            {
                var saved = await db.Domains.FindAsync(1);
                Assert.IsNull(saved);
            }
        }

        [TestMethod]
        public async Task GetDomainAsync_ValidResponse_SavesNewDomainToDatabase()
        {
            // Arrange
            var options = TestHelper.CreateInMemoryDatabaseOptions();
            
            var mockWhoIs = TestHelper.CreateMockWhoIsClientResponse("new.com", "2.2.2.2", "New Org");
            var mockDns = new Mock<IDnsLookupClient>();
            mockDns.Setup(m => m.QueryAsync("new.com", QueryType.ANY))
                                   .ReturnsAsync(TestHelper.CreateMockDnsResponse("2.2.2.2", 300).Object);

            // Act
            using (var db = new DatabaseContext(options))
            {
                var repository = new DomainRepository(db);
                var service = new DomainService(repository, mockWhoIs.Object, mockDns.Object);
                await service.GetDomainAsync("new.com");
            }

            // Assert
            using (var db = new DatabaseContext(options))
            {
                var saved = await new DomainRepository(db).GetDomainByNameAsync("new.com");
                Assert.IsNotNull(saved);
                Assert.AreEqual("2.2.2.2", saved.Ip);
            }
        }

        [TestMethod]
        public async Task GetDomainAsync_CachedDomainWithValidTtl_DoesNotCallExternalServices()
        {
            // Arrange
            var options = TestHelper.CreateInMemoryDatabaseOptions();
            var cachedDomain = TestHelper.CreateDomain(minutesAgo: 5, ttl: 60);

            using (var db = new DatabaseContext(options))
            {
                db.Domains.Add(cachedDomain);
                await db.SaveChangesAsync();
            }

            var mockWhoIs = new Mock<IWhoIsClient>();
            var mockDns = new Mock<IDnsLookupClient>();

            using var db2 = new DatabaseContext(options);
            var repository = new DomainRepository(db2);
            var service = new DomainService(repository, mockWhoIs.Object, mockDns.Object);

            // Act
            await service.GetDomainAsync("test.com");

            // Assert
            mockDns.Verify(m => m.QueryAsync(It.IsAny<string>(), It.IsAny<QueryType>()), Times.Never);
        }

        [TestMethod]
        public async Task GetDomainAsync_CachedDomainWithExpiredTtl_CallsExternalServices()
        {
            // Arrange
            var options = TestHelper.CreateInMemoryDatabaseOptions();
            var cachedDomain = TestHelper.CreateDomain(minutesAgo: 120, ttl: 60);

            using (var db = new DatabaseContext(options))
            {
                db.Domains.Add(cachedDomain);
                await db.SaveChangesAsync();
            }

            var mockWhoIs = TestHelper.CreateMockWhoIsClientResponse("test.com", "3.3.3.3", "Refreshed Org");
            var mockDns = new Mock<IDnsLookupClient>();
            mockDns.Setup(m => m.QueryAsync("test.com", QueryType.ANY))
                .ReturnsAsync(TestHelper.CreateMockDnsResponse("3.3.3.3", 300).Object);

            using var db2 = new DatabaseContext(options);
            var repository = new DomainRepository(db2);
            var service = new DomainService(repository, mockWhoIs.Object, mockDns.Object);

            // Act
            var result = await service.GetDomainAsync("test.com");

            // Assert
            mockDns.Verify(m => m.QueryAsync("test.com", QueryType.ANY), Times.Once);
            Assert.AreEqual("3.3.3.3", result.Ip);
        }
    }
}

