using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WikipediaBiographyCreator.Services;

namespace WikipediaBiographyCreator.Tests
{
    [TestClass]
    public class IndependentObitSubjectServiceTests
    {
        private readonly IndependentObitSubjectService _service = new();

        [DataTestMethod]
        [DataRow(
            "TEXT, born London 21 October 1933, married 1974 Gareth Wigan (one son; marriage dissolved), died London 5 July 1992.TEXT",
            "1933-10-21",
            "1992-07-05")]
        [DataRow(
            "TEXT, born Taku Japan 30 January 1920, died Tokyo 27 May 1992.TEXT",
            "1920-01-30",
            "1992-05-27")]
        [DataRow(
            "TEXT, born 14 May 1914, TEXT, died 22 August 1992.TEXT",
            "1914-05-14",
            "1992-08-22")]
        [DataRow(
            "TEXT: born Minneapolis 3 January 1916; married 1941 Lou Levy (one son, one daughter; marriage dissolved 1950); died Boston, Massachusetts 21 October 1995.",
            "1916-01-03",
            "1995-10-21")]
        [DataRow(
            "TEXT: born London 3 June 1918; died 23 May 1996.",
            "1918-06-03",
            "1996-05-23")]
        [DataRow(
            "TEXT: born London 18 July 1917; married first Sylvia Durham, second Marianne Stone (two daughters); died London 17 August 1997.",
            "1917-07-18",
            "1997-08-17")]
        [DataRow(
            "TEXT, born Springfield, Massachusetts 21 May 1945; married 1971 Marcus Cunliffe (died 1990; marriage dissolved 1980); died Adderbury, Oxfordshire 28 March 1997.",
            "1945-05-21",
            "1997-03-28")]
        public void ResolveDoBAndDoD_ShouldExtractCorrectDates(string text, string expectedBirth, string expectedDeath)
        {
            // Act
            var (dob, dod) = _service.ResolveDoBAndDoD(text);

            // Assert
            Assert.AreEqual(DateOnly.Parse(expectedBirth), dob, "Date of Birth mismatch");
            Assert.AreEqual(DateOnly.Parse(expectedDeath), dod, "Date of Death mismatch");
        }
    }
}
