using Microsoft.VisualStudio.TestTools.UnitTesting;

using Desafio.Umbler.Validators;

namespace Desafio.Umbler.Test.Validators
{
    [TestClass]
    public class DomainValidatorTests
    {
        [TestMethod]
        public void IsValid_ValidDomain_ReturnsTrue()
        {
            // Arrange
            var domainName = "example.com";

            // Act
            var result = DomainValidator.IsValid(domainName, out var errorMessage);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNull(errorMessage);
        }

        [TestMethod]
        public void IsValid_ValidDomainWithSubdomain_ReturnsTrue()
        {
            // Arrange
            var domainName = "www.example.com";

            // Act
            var result = DomainValidator.IsValid(domainName, out var errorMessage);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNull(errorMessage);
        }

        [TestMethod]
        public void IsValid_NullDomain_ReturnsFalse()
        {
            // Arrange
            string domainName = null;

            // Act
            var result = DomainValidator.IsValid(domainName, out var errorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNotNull(errorMessage);
        }

        [TestMethod]
        public void IsValid_EmptyDomain_ReturnsFalse()
        {
            // Arrange
            var domainName = "";

            // Act
            var result = DomainValidator.IsValid(domainName, out var errorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNotNull(errorMessage);
        }

        [TestMethod]
        public void IsValid_WhiteSpaceDomain_ReturnsFalse()
        {
            // Arrange
            var domainName = "   ";

            // Act
            var result = DomainValidator.IsValid(domainName, out var errorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNotNull(errorMessage);
        }

        [TestMethod]
        public void IsValid_TooShortDomain_ReturnsFalse()
        {
            // Arrange
            var domainName = "a.c";

            // Act
            var result = DomainValidator.IsValid(domainName, out var errorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNotNull(errorMessage);
        }

        [TestMethod]
        public void IsValid_TooLongDomain_ReturnsFalse()
        {
            // Arrange
            var domainName = new string('a', 300) + ".com";

            // Act
            var result = DomainValidator.IsValid(domainName, out var errorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNotNull(errorMessage);
        }

        [TestMethod]
        public void IsValid_DomainWithoutExtension_ReturnsFalse()
        {
            // Arrange
            var domainName = "example";

            // Act
            var result = DomainValidator.IsValid(domainName, out var errorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNotNull(errorMessage);
        }

        [TestMethod]
        public void IsValid_DomainWithInvalidCharacters_ReturnsFalse()
        {
            // Arrange
            var domainName = "exam!ple.com";

            // Act
            var result = DomainValidator.IsValid(domainName, out var errorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNotNull(errorMessage);
        }

        [TestMethod]
        public void IsValid_DomainWithSpaces_ReturnsFalse()
        {
            // Arrange
            var domainName = "example .com";

            // Act
            var result = DomainValidator.IsValid(domainName, out var errorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNotNull(errorMessage);
        }

        [TestMethod]
        public void IsValid_DomainWithHyphens_ReturnsTrue()
        {
            // Arrange
            var domainName = "my-example.com";

            // Act
            var result = DomainValidator.IsValid(domainName, out var errorMessage);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNull(errorMessage);
        }
    }
}
