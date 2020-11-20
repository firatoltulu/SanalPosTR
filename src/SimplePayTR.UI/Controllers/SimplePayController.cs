using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimplePayTR.Core;
using SimplePayTR.Core.Model;
using SimplePayTR.Core.Providers;
using SimplePayTR.UI.Caching;
using SimplePayTR.UI.Data;
using SimplePayTR.UI.Data.Entities;
using SimplePayTR.UI.Models;
using System;
using System.Threading.Tasks;

namespace SimplePayTR.UI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SimplePayController : ControllerBase
    {
        private readonly Func<BankTypes, IProviderService> _paymentServices;
        private readonly IDataServices _dataServices;
        private readonly ICache _cache;
        private readonly AppConfig _appConfig;
        private string COOKIE_NAME => $"{_appConfig.RedisPrefix}Pay";

        public SimplePayController(
            Func<BankTypes, IProviderService> paymentServices,
            IDataServices dataServices,
            ICache cache,
            AppConfig appConfig
            )
        {
            _paymentServices = paymentServices;
            _dataServices = dataServices;
            _cache = cache;
            _appConfig = appConfig;
        }

        [HttpPost()]
        public async Task<object> Pay([FromBody] PaymentActionModel paymentModel)
        {
            var sessionId = Guid.NewGuid();

            var bankProvider = _paymentServices(paymentModel.SelectedBank);
            var posConfiguration = Program.PosConfiguration[paymentModel.SelectedBank];

            if (posConfiguration.ForcePayRequest3D)
                paymentModel.Use3DSecure = posConfiguration.ForcePayRequest3D;

            paymentModel.PosConfigurationId = posConfiguration.Id;

            var bankResponse = await bankProvider.ProcessPayment(paymentModel);
            var paySession = bankResponse.ToPaySessionSuccess(paymentModel);
            await _dataServices.InsertPaySessionAsync(paySession);

            if (bankResponse.Status)
            {
                if (bankResponse.IsRedirectContent)
                {
                    _cache.Set($"VerifyModel:{sessionId}", new VerifyPaymentModel()
                    {
                        Order = paymentModel.Order,
                        SelectedBank = paymentModel.SelectedBank
                    });
                    _cache.Set($"PaySession:{sessionId}", paySession);

                    await SetGuid(sessionId);
                }

                return new PaymentActionResult { Status = true, Data = bankResponse.ToDataModel() };
            }
            else
            {
                return new PaymentActionResult { Status = false, Error = bankResponse.ErrorCode, ErrorMessage = bankResponse.Error };
            }
        }

        [HttpPost()]
        public IActionResult Fail([FromForm] IFormCollection paymentModel)
        {
            return Redirect("/");
        }

        [HttpPost()]
        public async Task<IActionResult> ValidatePayment([FromForm] IFormCollection paymentModel)
        {
            var sessionId = await GetGuid();
            var verifyPaymentModel = _cache.Get<VerifyPaymentModel>($"VerifyModel:{sessionId}");
            var paySession = _cache.Get<PaySession>($"PaySession:{sessionId}");

            _cache.Delete($"VerifyModel:{sessionId}");
            _cache.Delete($"PaySession:{sessionId}");

            var bankProvider = _paymentServices(verifyPaymentModel.SelectedBank);
            var bankResponse = await bankProvider.VerifyPayment(verifyPaymentModel, paymentModel);

            if (bankResponse.Status)
            {
                paySession.ReferanceNumber = bankResponse.ReferanceNumber;
                paySession.ProvisionNumber = bankResponse.ProvisionNumber;
                paySession.Status = true;

                await _dataServices.UpdatePaySessionAsync(paySession);

                return Redirect($"{string.Format(_appConfig.SuccessRedirectUrl, bankResponse.ProvisionNumber)}");
            }
            else
            {
                paySession.Error = bankResponse.Error;
                paySession.ErrorCode = bankResponse.ErrorCode;
                return Redirect($"{string.Format(_appConfig.FailRedirectUrl, bankResponse.Error)}");
            }
        }

        public virtual Task<string> GetGuid()
        {
            return Task.FromResult(HttpContext.Request.Cookies[COOKIE_NAME]);
        }

        public virtual Task SetGuid(Guid guid)
        {
            //delete current cookie value
            HttpContext.Response.Cookies.Delete(COOKIE_NAME);

            //get date of cookie expiration
            var cookieExpiresDate = DateTime.UtcNow.AddMinutes(10);

            //if passed guid is empty set cookie as expired
            if (guid == Guid.Empty)
                cookieExpiresDate = DateTime.UtcNow.AddMonths(-1);

            //set new cookie value
            var options = new CookieOptions
            {
                HttpOnly = true,
                Expires = cookieExpiresDate
            };
            HttpContext.Response.Cookies.Append(COOKIE_NAME, guid.ToString(), options);

            return Task.CompletedTask;
        }
    }
}