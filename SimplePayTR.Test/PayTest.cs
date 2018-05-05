using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace SimplePayTR.Test
{
    [TestClass]
    public class PayTest
    {
        [TestMethod]
        public void UseESTPay()
        {
            var jsonPay = "{\"ClientId\":\"X\",\"Name\":\"x\",\"Password\":\"x\",\"StoreKey\":\"x\"}";
            Dictionary<string, object> accounts = new Dictionary<string, object>();

            var pos = new Gateway(NetworkType.EST);
            Request payRequest = new Request
            {
                Url = "https://www.sanalakpos.com/servlet/",
                Accounts = accounts,
                SuccessUrl = string.Format("https://{0}/Integration/Success/{1}", "www.lezzetlihediye.com", 1),
                ErrorUrl = string.Format("https://{0}/Integration/Error/{1}", "www.lezzetlihediye.com", 1),
                Is3D = true,
                Pos = new RequestPos
                {
                    CardNumber = "4022....1287",
                    Comission = 0,
                    CVV2 = "888",
                    EMail = "",
                    ExpireDate = "0925",
                    FullName = "FIRAT OLTULU",
                    Installment = 0,
                    Ip = ":::1",
                    ProcessId = "1",
                    Total = 10,
                    UserId = "ECommerce3D"
                }
            };

            var tAccounts = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonPay);
            foreach (var item in tAccounts)
            {
                accounts.Add(item.Key, item.Value);
            }
            var result = pos.Pay(payRequest);
        }
    }
}