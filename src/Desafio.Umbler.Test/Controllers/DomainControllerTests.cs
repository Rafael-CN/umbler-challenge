using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Desafio.Umbler.Models;
using Desafio.Umbler.Services;
using Desafio.Umbler.Repository;
using Desafio.Umbler.Controllers;
using Desafio.Umbler.Test.Helpers;
using Desafio.Umbler.Controllers.DTOs;
using Desafio.Umbler.Services.Interfaces;

using DnsClient;
using Moq;

namespace Desafio.Umbler.Test.Controllers
{
    [TestClass]
    public class DomainControllerTests
    {
        #region Validation Tests (BadRequest 400)

        [TestMethod]
        public async Task Get_EmptyDomain_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var response = await controller.Get("");

            // Assert
            Assert.IsInstanceOfType(response, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Get_DomainWithoutExtension_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var response = await controller.Get("google");

            // Assert
            Assert.IsInstanceOfType(response, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Get_DomainTooShort_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var response = await controller.Get("a.b");

            // Assert
            Assert.IsInstanceOfType(response, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Get_DomainWithInvalidCharacters_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var response = await controller.Get("test@domain.com");

            // Assert
            Assert.IsInstanceOfType(response, typeof(BadRequestObjectResult));
        }

        #endregion

        #region NotFound Test (NotFound 404)

        [TestMethod]
        public async Task Get_DomainNotResolvable_ReturnsNotFound()
        {
            // Arrange
            var options = TestHelper.CreateInMemoryDatabaseOptions();
            var mockWhoIs = new Mock<IWhoIsClient>();
            var mockDns = new Mock<IDnsLookupClient>();
            mockDns.Setup(m => m.QueryAsync(It.IsAny<string>(), QueryType.ANY))
                   .ReturnsAsync(TestHelper.CreateEmptyDnsResponse().Object);

            using var db = new DatabaseContext(options);
            var controller = new DomainController(
                new DomainService(new DomainRepository(db), mockWhoIs.Object, mockDns.Object)
            );

            // Act
            var response = await controller.Get("unknown.com");

            // Assert
            Assert.IsInstanceOfType(response, typeof(NotFoundObjectResult));
        }

        #endregion

        #region Success Tests (Ok 200)


        [TestMethod]
        public async Task Get_ValidDomainInCache_ReturnsOkWithDTO()
        {
            // Arrange
            var options = TestHelper.CreateInMemoryDatabaseOptions();
            var domain = TestHelper.CreateDomain();

            using (var db = new DatabaseContext(options))
            {
                db.Domains.Add(domain);
                await db.SaveChangesAsync();
            }

            var mockWhoIs = new Mock<IWhoIsClient>();
            var mockDns = new Mock<IDnsLookupClient>();

            using var db2 = new DatabaseContext(options);

            var repository = new DomainRepository(db2);
            var service = new DomainService(repository, mockWhoIs.Object, mockDns.Object);
            var controller = new DomainController(service);

            // Act
            var response = await controller.Get("test.com");

            // Assert
            Assert.IsInstanceOfType(response, typeof(OkObjectResult));

            var result = response as OkObjectResult;
            var dto = result?.Value as DomainDTO;

            Assert.IsNotNull(dto);
            Assert.AreEqual("test.com", dto.Name);
        }

        [TestMethod]
        public async Task Get_ValidDomainNotInCache_ReturnsOkWithDTO()
        {
            // Arrange
            var options = TestHelper.CreateInMemoryDatabaseOptions();

            var mockWhoIs = TestHelper.CreateMockWhoIsClientResponse("valid.com", "5.5.5.5", "Valid Org");

            var mockDns = new Mock<IDnsLookupClient>();
            mockDns.Setup(m => m.QueryAsync("valid.com", QueryType.ANY))
                   .ReturnsAsync(TestHelper.CreateMockDnsResponse("5.5.5.5", 300).Object);

            using var db = new DatabaseContext(options);

            var repository = new DomainRepository(db);
            var service = new DomainService(repository, mockWhoIs.Object, mockDns.Object);
            var controller = new DomainController(service);

            // Act
            var response = await controller.Get("valid.com");

            // Assert
            Assert.IsInstanceOfType(response, typeof(OkObjectResult));

            var result = response as OkObjectResult;
            var dto = result?.Value as DomainDTO;

            Assert.AreEqual("5.5.5.5", dto.Ip);
            Assert.AreEqual("Valid Org", dto.HostedAt);
        }

        #endregion

        private static DomainController CreateController()
        {
            var options = TestHelper.CreateInMemoryDatabaseOptions();
            var mockWhoIs = new Mock<IWhoIsClient>();
            var mockDns = new Mock<IDnsLookupClient>();

            var db = new DatabaseContext(options);
            return new DomainController(
                new DomainService(new DomainRepository(db), mockWhoIs.Object, mockDns.Object)
            );
        }
    }
}