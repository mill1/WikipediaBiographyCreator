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
        public void Escape_Initial()
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
