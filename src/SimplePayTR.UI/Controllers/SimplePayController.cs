using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using SanalPosTR;
using SanalPosTR.Model;
using SanalPosTR.Providers;
using SanalPosTR.Playground.Caching;
using SanalPosTR.Playground.Data;
using SanalPosTR.Playground.Data.Entities;
using SanalPosTR.Playground.Models;
using System;
using System.Threading.Tasks;

namespace SanalPosTR.Playground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimplePayController : ControllerBase
    {
        private readonly Func<BankTypes, IProviderService> _paymentServices;
        private readonly IDataServices _dataServices;
        private readonly ICache _cache;
        private readonly AppConfig _appConfig;
        private readonly ILogger<SimplePayController> _logger;
        private string COOKIE_NAME => $"SimplePayGuid";

        public SimplePayController(
            Func<BankTypes, IProviderService> paymentServices,
            IDataServices dataServices,
            ICache cache,
            AppConfig appConfig,
            ILogger<SimplePayController> logger
            )
        {
            _paymentServices = paymentServices;
            _dataServices = dataServices;
            _cache = cache;
            _appConfig = appConfig;
            _logger = logger;
        }

        [HttpPost("Pay")]
        public async Task<object> Pay([FromBody] PaymentActionModel paymentModel)
        {
            using (_logger.BeginScope("Pay"))
            {
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
                        _cache.Set($"VerifyModel:{paymentModel.SessionId}", new VerifyPaymentModel()
                        {
                            Order = paymentModel.Order,
                            SelectedBank = paymentModel.SelectedBank
                        });
                        _cache.Set($"PaySession:{paymentModel.SessionId}", paySession);
                    }

                    return new PaymentActionResult { Status = true, Data = bankResponse.ToDataModel() };
                }
                else
                {
                    return new PaymentActionResult { Status = false, Error = bankResponse.ErrorCode, ErrorMessage = bankResponse.Error };
                }
            }
        }

        [HttpPost()]
        public IActionResult Fail([FromForm] IFormCollection paymentModel)
        {
            return Redirect("/");
        }

        [HttpPost("ValidatePayment/{id?}")]
        public async Task<IActionResult> ValidatePayment(string id, [FromForm] IFormCollection paymentModel)
        {
            using (_logger.BeginScope("ValidatePayment"))
            {
                var sessionId = id;

                if (string.IsNullOrEmpty(sessionId))
                    return Redirect($"{string.Format(_appConfig.FailRedirectUrl, "Uygulama Hatası")}");

                var verifyPaymentModel = _cache.Get<VerifyPaymentModel>($"VerifyModel:{sessionId}");
                var paySession = _cache.Get<PaySession>($"PaySession:{sessionId}");

                Log.Information($"{sessionId} - Load From Cache");

                _cache.Delete($"VerifyModel:{sessionId}");
                _cache.Delete($"PaySession:{sessionId}");

                Log.Information($"{sessionId} - Cache Removed");

                var bankProvider = _paymentServices(verifyPaymentModel.SelectedBank);

                Log.Information($"{sessionId} - BankProvider - ${bankProvider.CurrentBank}");

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
        }

        public virtual Task<string> GetGuid()
        {
            foreach (var item in HttpContext.Request.Cookies.Keys)
            {
                Log.Information($"GetGuid {item}");
            }

            return Task.FromResult(HttpContext.Request.Cookies[COOKIE_NAME]);
        }

        public virtual Task SetGuid(Guid guid)
        {
            if (Request.Cookies[COOKIE_NAME] != null)
            {
                Log.Information($"{COOKIE_NAME} zaten var");
                return Task.CompletedTask;
            }

            //delete current cookie value
            // HttpContext.Response.Cookies.Delete(COOKIE_NAME);

            //get date of cookie expiration
            var cookieExpiresDate = DateTime.Now.AddMinutes(10);

            //if passed guid is empty set cookie as expired
            if (guid == Guid.Empty)
                cookieExpiresDate = DateTime.UtcNow.AddMonths(-1);

            //set new cookie value
            var options = new CookieOptions
            {
                Expires = cookieExpiresDate,
            };

            Response.Cookies.Append(COOKIE_NAME, guid.ToString(), options);

            return Task.CompletedTask;
        }

        [HttpPost("Installment")]
        public async Task<IActionResult> Installment(InstallmentModel model)
        {
            var installments = await _dataServices.GetPosInstallments(model.BinNumber);
            return new JsonResult(installments);
        }

        [HttpPost("Detail")]
        public async Task<IActionResult> Detail(OrderDetailModel model)
        {
            var installments = await _dataServices.GetPaySession(model.OrderId);
            return new JsonResult(installments);
        }
    }
}