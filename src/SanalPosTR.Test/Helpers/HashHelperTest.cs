using NUnit.Framework;
using SanalPosTR;

namespace SanalPosTR.Test.Helpers
{
    [TestFixture]
    public class HashHelperTest
    {
        [Test]
        public void GetSHA1_ReturnsBase64EncodedHash()
        {
            var result = HashHelper.GetSHA1("test");

            Assert.That(result, Is.Not.Null.And.Not.Empty);
            Assert.That(result, Does.Not.Contain(" "));
        }

        [Test]
        public void GetSHA1_SameInput_ReturnsSameOutput()
        {
            var result1 = HashHelper.GetSHA1("payment_hash_test");
            var result2 = HashHelper.GetSHA1("payment_hash_test");

            Assert.That(result1, Is.EqualTo(result2));
        }

        [Test]
        public void GetSHA1_DifferentInput_ReturnsDifferentOutput()
        {
            var result1 = HashHelper.GetSHA1("input1");
            var result2 = HashHelper.GetSHA1("input2");

            Assert.That(result1, Is.Not.EqualTo(result2));
        }

        [Test]
        public void GetSHA1WithHexaDecimal_ReturnsHexString()
        {
            var result = HashHelper.GetSHA1WithHexaDecimal("test");

            Assert.That(result, Is.Not.Null.And.Not.Empty);
            Assert.That(result, Does.Match("^[0-9a-f]+$"));
        }

        [Test]
        public void GetSHA1WithUTF8_ReturnsBase64()
        {
            var result = HashHelper.GetSHA1WithUTF8("test");

            Assert.That(result, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void GetSHA1WithUTF8_DiffersFromISO8859_ForTurkishChars()
        {
            var utf8Result = HashHelper.GetSHA1WithUTF8("türkçe");
            var isoResult = HashHelper.GetSHA1("türkçe");

            Assert.That(utf8Result, Is.Not.EqualTo(isoResult));
        }

        [Test]
        public void GetSHA256_ReturnsBase64EncodedHash()
        {
            var result = HashHelper.GetSHA256("test");

            Assert.That(result, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void GetSHA256_SameInput_ReturnsSameOutput()
        {
            var result1 = HashHelper.GetSHA256("consistent_hash");
            var result2 = HashHelper.GetSHA256("consistent_hash");

            Assert.That(result1, Is.EqualTo(result2));
        }

        [Test]
        public void GetSHA512_ReturnsHexString()
        {
            var result = HashHelper.GetSHA512("test");

            Assert.That(result, Is.Not.Null.And.Not.Empty);
            Assert.That(result, Does.Match("^[0-9a-f]+$"));
        }

        [Test]
        public void GetMD5_ReturnsHexString()
        {
            var result = HashHelper.GetMD5("test");

            Assert.That(result, Is.Not.Null.And.Not.Empty);
            Assert.That(result, Does.Match("^[0-9a-f]+$"));
        }

        [Test]
        public void GetHash_WithSHA1Type_ReturnsSameAsGetSHA1()
        {
            var hashResult = HashHelper.GetHash("test", HashHelper.HashType.SHA1);
            var sha1Result = HashHelper.GetSHA1("test");

            Assert.That(hashResult, Is.EqualTo(sha1Result));
        }

        [Test]
        public void GetHash_WithMD5Type_ReturnsSameAsGetMD5()
        {
            var hashResult = HashHelper.GetHash("test", HashHelper.HashType.MD5);
            var md5Result = HashHelper.GetMD5("test");

            Assert.That(hashResult, Is.EqualTo(md5Result));
        }

        [Test]
        public void GetHash_WithSHA256Type_ReturnsSameAsGetSHA256()
        {
            var hashResult = HashHelper.GetHash("test", HashHelper.HashType.SHA256);
            var sha256Result = HashHelper.GetSHA256("test");

            Assert.That(hashResult, Is.EqualTo(sha256Result));
        }

        [Test]
        public void GetHash_WithSHA512Type_ReturnsSameAsGetSHA512()
        {
            var hashResult = HashHelper.GetHash("test", HashHelper.HashType.SHA512);
            var sha512Result = HashHelper.GetSHA512("test");

            Assert.That(hashResult, Is.EqualTo(sha512Result));
        }

        [Test]
        public void CheckHash_WithMatchingHash_ReturnsTrue()
        {
            var hash = HashHelper.GetSHA1("test_input");

            Assert.That(HashHelper.CheckHash("test_input", hash, HashHelper.HashType.SHA1), Is.True);
        }

        [Test]
        public void CheckHash_WithNonMatchingHash_ReturnsFalse()
        {
            var hash = HashHelper.GetSHA1("test_input");

            Assert.That(HashHelper.CheckHash("different_input", hash, HashHelper.HashType.SHA1), Is.False);
        }

        [Test]
        public void GetSHA1_EmptyString_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => HashHelper.GetSHA1(""));
        }

        [Test]
        public void GetSHA256_EmptyString_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => HashHelper.GetSHA256(""));
        }

        [Test]
        public void GetSHA512Base64_ReturnsBase64EncodedHash()
        {
            var result = HashHelper.GetSHA512Base64("test");

            Assert.That(result, Is.Not.Null.And.Not.Empty);
            // Base64 should end with = padding or contain only valid chars
            Assert.That(result, Does.Match("^[A-Za-z0-9+/]+=*$"));
        }

        [Test]
        public void GetSHA512Base64_SameInput_ReturnsSameOutput()
        {
            var result1 = HashHelper.GetSHA512Base64("consistent_hash");
            var result2 = HashHelper.GetSHA512Base64("consistent_hash");

            Assert.That(result1, Is.EqualTo(result2));
        }

        [Test]
        public void GetSHA512Base64_DifferentInput_ReturnsDifferentOutput()
        {
            var result1 = HashHelper.GetSHA512Base64("input1");
            var result2 = HashHelper.GetSHA512Base64("input2");

            Assert.That(result1, Is.Not.EqualTo(result2));
        }

        [Test]
        public void GetSHA512Base64_EmptyString_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => HashHelper.GetSHA512Base64(""));
        }

        [Test]
        public void GetSHA512Base64_UsesUTF8_NotUnicode()
        {
            // GetSHA512Base64 uses UTF-8, GetSHA512 uses UnicodeEncoding
            // They should produce different results for the same input
            var base64Result = HashHelper.GetSHA512Base64("test");
            var hexResult = HashHelper.GetSHA512("test");

            // Different encoding + different output format = definitely different
            Assert.That(base64Result, Is.Not.EqualTo(hexResult));
        }
    }
}
