using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SimplePayTR.Test
{
    [TestClass]
    public class PayTest
    {
        [TestMethod]
        public void UseESTPay()
        {
            string text = "19031500648276583200http://localhost:50063/api/simplePay/Successhttp://localhost:50063/api/simplePay/fail0DB3820DA0744AFD931F84099898As";

            Encoding UE = Encoding.GetEncoding("ISO-8859-9");
            byte[] hashValue;
            byte[] message = UE.GetBytes(text);

            SHA1Managed hashString = new SHA1Managed();
            string hex = "";

            hashValue = hashString.ComputeHash(message);

            var result = Convert.ToBase64String(hashValue);

            Assert.AreEqual("EDieUdFmY4RilJ306HpeNU8f50w=", result);

        }
    }
}