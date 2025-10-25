using FluentAssertions;
using WikipediaBiographyCreator.Services;

namespace WikipediaBiographyCreator.Tests
{
    [TestClass]
    public class NYTimesObituarySubjectServiceTests
    {
        private NYTimesObituarySubjectService _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new NYTimesObituarySubjectService(new NameVersionService());
        }

        [TestMethod]
        public void CreateVersions_NoComma()
        {
            // Act
            var versions = _service.GetNameVersions("OTUMFUO OPOKU WARE II");

            // Assert
            versions.Should().HaveCount(1);
            versions.Should().ContainInOrder(
                "Otumfuo Opoku Ware II"
            );
        }

        [TestMethod]
        public void CreateVersions_HighRomanNumbers()
        {
            // Act
            var versions = _service.GetNameVersions("LOUIS XIV");

            // Assert
            versions.Should().HaveCount(1);
            versions.Should().ContainInOrder(
                "Louis XIV"
            );
        }


        [TestMethod]
        public void Capitalize_Firstname()
        {
            // Act
            var versions = _service.GetNameVersions("RAMBO, JOHN");

            // Assert
            versions.Should().HaveCount(1);
            versions.Should().ContainInOrder(
                "John Rambo"
            );
        }

        [TestMethod]
        public void Capitalize_SurnameMac()
        {
            // Act
            var versions = _service.GetNameVersions("MACDONALD, JOHN");

            // Assert
            versions.Should().HaveCount(1);
            versions.Should().ContainInOrder(
                "John MacDonald"
            );
        }

        [TestMethod]
        public void Capitalize_SurnameOBrian()
        {
            // Act
            var versions = _service.GetNameVersions("O'BRIAN, JOHN");

            // Assert
            versions.Should().HaveCount(1);
            versions.Should().ContainInOrder(
                "John O'Brian"
            );
        }

        [TestMethod]
        public void CreateVersions_WithInitials()
        {
            // Act
            var versions = _service.GetNameVersions("Warwick, William E");

            // Assert
            versions.Should().HaveCount(2);
            versions.Should().ContainInOrder(
                "William E. Warwick",
                "William Warwick"
            );
        }

        [TestMethod]
        public void CreateVersion_OptionSuffix()
        {
            // Act
            var versions = _service.GetNameVersions("RAMBO, JOHN II");

            // Assert
            versions.Should().HaveCount(2);
            versions.Should().ContainInOrder(
                "John Rambo II",
                "John Rambo"
            );
        }

        [TestMethod]
        public void AddPointToSuffix_Suffix()
        {
            // Act
            var versions = _service.GetNameVersions("ROCKEFELLER, JOHN JR");

            // Assert
            versions.Should().HaveCount(2);
            versions.Should().ContainInOrder(
                "John Rockefeller Jr.",
                "John Rockefeller"
            );
        }

        [TestMethod]
        public void Replace_Suffix()
        {
            // Act
            var versions = _service.GetNameVersions("ROCKEFELLER, JOHN 3D");

            // Assert
            versions.Should().HaveCount(2);
            versions.Should().ContainInOrder(
                "John Rockefeller III",
                "John Rockefeller"
            );
        }

        [TestMethod]
        public void Escape_ValidInitial()
        {
            // Act
            var versions = _service.GetNameVersions("ROCKEFELLER, JOHN V");

            // Assert
            versions.Should().HaveCount(2);
            versions.Should().ContainInOrder(
                "John V. Rockefeller",
                "John Rockefeller"
            );
        }


        [TestMethod]
        public void Suffix_WithInitials()
        {
            // Act
            var versions = _service.GetNameVersions("ROCKEFELLER, JOHN D JR");

            // Assert
            versions.Should().HaveCount(4);
            versions.Should().ContainInOrder(
                "John D. Rockefeller Jr.",
                "John D. Rockefeller",
                "John Rockefeller Jr.",
                "John Rockefeller"
            );
        }
    }
}
