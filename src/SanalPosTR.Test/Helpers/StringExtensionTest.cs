using NUnit.Framework;
using SanalPosTR.Extensions;

namespace SanalPosTR.Test.Helpers
{
    [TestFixture]
    public class StringExtensionTest
    {
        #region IsEmpty

        [Test]
        public void IsEmpty_Null_ReturnsTrue()
        {
            string value = null;
            Assert.That(value.IsEmpty(), Is.True);
        }

        [Test]
        public void IsEmpty_EmptyString_ReturnsTrue()
        {
            Assert.That("".IsEmpty(), Is.True);
        }

        [Test]
        public void IsEmpty_WhitespaceOnly_ReturnsTrue()
        {
            Assert.That("   ".IsEmpty(), Is.True);
        }

        [Test]
        public void IsEmpty_TabsAndNewlines_ReturnsTrue()
        {
            Assert.That("\t\n\r ".IsEmpty(), Is.True);
        }

        [Test]
        public void IsEmpty_NonEmptyString_ReturnsFalse()
        {
            Assert.That("test".IsEmpty(), Is.False);
        }

        [Test]
        public void IsEmpty_StringWithSpacesAndChars_ReturnsFalse()
        {
            Assert.That(" a ".IsEmpty(), Is.False);
        }

        #endregion

        #region IsNullOrEmpty

        [Test]
        public void IsNullOrEmpty_Null_ReturnsTrue()
        {
            string value = null;
            Assert.That(value.IsNullOrEmpty(), Is.True);
        }

        [Test]
        public void IsNullOrEmpty_Empty_ReturnsTrue()
        {
            Assert.That("".IsNullOrEmpty(), Is.True);
        }

        [Test]
        public void IsNullOrEmpty_Whitespace_ReturnsTrue()
        {
            Assert.That("  ".IsNullOrEmpty(), Is.True);
        }

        [Test]
        public void IsNullOrEmpty_ValidString_ReturnsFalse()
        {
            Assert.That("hello".IsNullOrEmpty(), Is.False);
        }

        #endregion

        #region Remove

        [Test]
        public void Remove_RemovesXmlTag()
        {
            var input = "<Response>Approved</Response>";
            var result = input.Remove("Response");

            Assert.That(result, Is.EqualTo("Approved"));
        }

        [Test]
        public void Remove_RemovesSelfClosingTag()
        {
            var input = "<br/>text<br/>";
            var result = input.Remove("br");

            Assert.That(result, Is.EqualTo("text"));
        }

        [Test]
        public void Remove_NoMatchingTag_ReturnsOriginal()
        {
            var input = "<Response>test</Response>";
            var result = input.Remove("Missing");

            Assert.That(result, Is.EqualTo(input));
        }

        #endregion

        #region Match

        [Test]
        public void Match_ValidRegex_ReturnsMatch()
        {
            var result = "abc123def".Match(@"\d+");

            Assert.That(result.Success, Is.True);
            Assert.That(result.Value, Is.EqualTo("123"));
        }

        [Test]
        public void Match_NoMatch_ReturnsUnsuccessful()
        {
            var result = "abcdef".Match(@"\d+");

            Assert.That(result.Success, Is.False);
        }

        [Test]
        public void Match_XmlContentPattern_ReturnsGroups()
        {
            var xml = "<Code>00</Code>";
            var pattern = @"(<(Code)[^>]*\/?>)(.*?)(<\/(?:\2)>)";
            var result = xml.Match(pattern);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Groups[3].Value, Is.EqualTo("00"));
        }

        #endregion
    }
}
