using NUnit.Framework;
using SanalPosTR;

namespace SanalPosTR.Test.Helpers
{
    [TestFixture]
    public class TemplateHelperTest : Initialize
    {
        [Test]
        public void GetInlineContent_XmlTag_ReturnsContent()
        {
            var xml = "<Response>Approved</Response>";
            var result = TemplateHelper.GetInlineContent(xml, "Response");

            Assert.That(result, Is.EqualTo("Approved"));
        }

        [Test]
        public void GetInlineContent_NestedXml_ReturnsInnerContent()
        {
            var xml = "<root><AuthCode>123456</AuthCode><Response>Approved</Response></root>";

            Assert.That(TemplateHelper.GetInlineContent(xml, "AuthCode"), Is.EqualTo("123456"));
            Assert.That(TemplateHelper.GetInlineContent(xml, "Response"), Is.EqualTo("Approved"));
        }

        [Test]
        public void GetInlineContent_MissingTag_ReturnsEmpty()
        {
            var xml = "<Response>Approved</Response>";
            var result = TemplateHelper.GetInlineContent(xml, "NonExistent");

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetInlineContent_EmptyTag_ReturnsEmpty()
        {
            var xml = "<Response></Response>";
            var result = TemplateHelper.GetInlineContent(xml, "Response");

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetInlineContent_KeyValueFormat_ReturnsValue()
        {
            var content = "Response=\"Approved\"";
            var result = TemplateHelper.GetInlineContent(content, "Response");

            Assert.That(result, Is.EqualTo("Approved"));
        }

        [Test]
        public void GetInlineContent_NestPayResponse_ParsesCorrectly()
        {
            var xml = @"<CC5Response>
                <Response>Approved</Response>
                <AuthCode>P12345</AuthCode>
                <HostRefNum>REF001</HostRefNum>
                <ErrMsg></ErrMsg>
            </CC5Response>";

            Assert.That(TemplateHelper.GetInlineContent(xml, "Response"), Is.EqualTo("Approved"));
            Assert.That(TemplateHelper.GetInlineContent(xml, "AuthCode"), Is.EqualTo("P12345"));
            Assert.That(TemplateHelper.GetInlineContent(xml, "HostRefNum"), Is.EqualTo("REF001"));
        }

        [Test]
        public void GetInlineContent_GarantiResponse_ParsesCorrectly()
        {
            var xml = @"<GVPSResponse>
                <Transaction>
                    <Response><Code>00</Code><ReasonCode>00</ReasonCode></Response>
                    <RetrefNum>REF002</RetrefNum>
                    <AuthCode>A12345</AuthCode>
                </Transaction>
            </GVPSResponse>";

            Assert.That(TemplateHelper.GetInlineContent(xml, "Code"), Is.EqualTo("00"));
            Assert.That(TemplateHelper.GetInlineContent(xml, "RetrefNum"), Is.EqualTo("REF002"));
            Assert.That(TemplateHelper.GetInlineContent(xml, "AuthCode"), Is.EqualTo("A12345"));
        }

        [Test]
        public void GetInlineContent_YKBResponse_ParsesCorrectly()
        {
            var xml = @"<posnetResponse>
                <approved>1</approved>
                <authCode>Y12345</authCode>
                <hostlogkey>HOST001</hostlogkey>
            </posnetResponse>";

            Assert.That(TemplateHelper.GetInlineContent(xml, "approved"), Is.EqualTo("1"));
            Assert.That(TemplateHelper.GetInlineContent(xml, "authCode"), Is.EqualTo("Y12345"));
            Assert.That(TemplateHelper.GetInlineContent(xml, "hostlogkey"), Is.EqualTo("HOST001"));
        }

        [Test]
        public void GetWrapContent_ReturnsContentWithoutTag()
        {
            var xml = "<Response><Code>00</Code></Response>";
            var result = TemplateHelper.GetWrapContent(xml, "Response");

            Assert.That(result, Does.Contain("<Code>00</Code>"));
            Assert.That(result, Does.Not.Contain("<Response>"));
        }

        [Test]
        public void GetWrapContent_MissingTag_ReturnsEmpty()
        {
            var xml = "<Response>test</Response>";
            var result = TemplateHelper.GetWrapContent(xml, "Missing");

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetSHA1_DelegatesToHashHelper()
        {
            var templateResult = TemplateHelper.GetSHA1("test");
            var hashResult = HashHelper.GetSHA1("test");

            Assert.That(templateResult, Is.EqualTo(hashResult));
        }

        [Test]
        public void ReadEmbedResource_NestPayPay_ReturnsContent()
        {
            var result = TemplateHelper.ReadEmbedResource("NestPay.Resources.Pay.xml");

            Assert.That(result, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void ReadEmbedResource_YKBPay_ReturnsContent()
        {
            var result = TemplateHelper.ReadEmbedResource("YKB.Resources.Pay.xml");

            Assert.That(result, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void ReadEmbedResource_GarantiPay_ReturnsContent()
        {
            var result = TemplateHelper.ReadEmbedResource("Garanti.Resources.Pay.xml");

            Assert.That(result, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void ReadEmbedResource_NestPay3D_ReturnsContent()
        {
            var result = TemplateHelper.ReadEmbedResource("NestPay.Resources.3D.xml");

            Assert.That(result, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void ReadEmbedResource_NestPayRefund_ReturnsContent()
        {
            var result = TemplateHelper.ReadEmbedResource("NestPay.Resources.Refund.xml");

            Assert.That(result, Is.Not.Null.And.Not.Empty);
        }
    }
}
