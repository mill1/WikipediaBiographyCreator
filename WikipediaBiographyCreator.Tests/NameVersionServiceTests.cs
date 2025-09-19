using FluentAssertions;
using WikipediaBiographyCreator.Services;

namespace YourNamespace.Tests
{
    [TestClass]
    public class NameVersionServiceTests
    {
        private NameVersionService _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new NameVersionService();
        }

        [TestMethod]
        public void SingleFirstname_SingleSurname_NoSuffix()
        {
            // Act
            var versions = _service.GetNameVersions("John", "Rambo", string.Empty);

            // Assert
            versions.Should().HaveCount(1);
            versions.First().Should().Be("John Rambo");
        }

        [TestMethod]
        public void FirstnameWithInitial_NoSuffix()
        {
            // Act
            var versions = _service.GetNameVersions("John J.", "Rambo", string.Empty);

            // Assert
            versions.Should().HaveCount(2);
            versions.Should().ContainInOrder("John J. Rambo", "John Rambo");
        }

        [TestMethod]
        public void MultipleInitials_NoSuffix()
        {
            // Act
            var versions = _service.GetNameVersions("John J. K.", "Rambo", string.Empty);

            // Assert
            versions.Should().HaveCount(2);
            versions.Should().ContainInOrder("John J. K. Rambo", "John Rambo");
        }

        [TestMethod]
        public void MultipleFirstnames_NoSuffix()
        {
            // Act
            var versions = _service.GetNameVersions("John Jack", "Rambo", string.Empty);

            // Assert
            versions.Should().HaveCount(2);
            versions.Should().ContainInOrder("John Jack Rambo", "John Rambo");
        }

        [TestMethod]
        public void MaidenName_NoSuffix()
        {
            // Act
            var versions = _service.GetNameVersions("Ruth Bader", "Ginsburg", string.Empty);

            // Assert
            versions.Should().HaveCount(2);
            versions.Should().ContainInOrder("Ruth Bader Ginsburg", "Ruth Ginsburg");
        }

        [TestMethod]
        public void MultipleInitials_OnlyInitials_NoSuffix()
        {
            // Act
            var versions = _service.GetNameVersions("J. J.", "Rambo", string.Empty);

            // Assert
            versions.Should().HaveCount(2);
            versions.Should().ContainInOrder("J. J. Rambo", "J. Rambo");
        }


        [TestMethod]
        public void Suffix_SingleFirstname()
        {
            // Act
            var versions = _service.GetNameVersions("John", "Rambo", "Sr.");

            // Assert
            versions.Should().HaveCount(2);
            versions.Should().ContainInOrder("John Rambo Sr.", "John Rambo");
        }

        [TestMethod]
        public void Suffix_WithInitials()
        {
            // Act
            var versions = _service.GetNameVersions("John J.", "Rambo", "Sr.");

            // Assert
            versions.Should().HaveCount(4);
            versions.Should().ContainInOrder(
                "John J. Rambo Sr.",
                "John J. Rambo",
                "John Rambo Sr.",
                "John Rambo"
            );
        }

        [TestMethod]
        public void EscapedSuffix_WithInitials()
        {
            // Act
            var versions = _service.GetNameVersions("John V.", "Rambo", "V");

            // Assert
            versions.Should().HaveCount(4);
            versions.Should().ContainInOrder(
                "John V. Rambo V",
                "John V. Rambo",
                "John Rambo V",
                "John Rambo"
            );
        }
    }
}
