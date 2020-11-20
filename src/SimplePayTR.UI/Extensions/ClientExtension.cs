using SimplePayTR.UI.Data.Entities;
using SimplePayTR.UI.Models;
using System;

namespace SimplePayTR.UI
{
    public static class ClientExtension
    {
        public static PaymentActionDataResult ToDataModel(this PaymentResult paymentResult)
        {
            return new PaymentActionDataResult
            {
                IsRedirect = paymentResult.IsRedirectContent,
                Content = paymentResult.IsRedirectContent ? paymentResult.ServerResponseRaw : paymentResult.ProvisionNumber
            };
        }

        public static PaySession ToPaySessionSuccess(this PaymentResult paymentResult, PaymentActionModel paymentModel)
        {
            return new PaySession
            {
                CardHolderName = paymentModel.CreditCard.CardHolderName,
                CardNumber = $"{paymentModel.CreditCard.CardNumber.Substring(0, 4)} **** **** {paymentModel.CreditCard.CardNumber.Substring(paymentModel.CreditCard.CardNumber.Length - 4, 4)}",
                CreateOn = DateTime.Now,
                ProvisionNumber = paymentResult.ProvisionNumber,
                ReferanceNumber = paymentResult.ReferanceNumber,
                Status = paymentResult.Status == false ? false : paymentResult.IsRedirectContent == false,
                UseSecure3D = paymentModel.Use3DSecure,
                Bank = paymentModel.SelectedBank,
                PosConfigurationId = paymentModel.PosConfigurationId,
                Amount = paymentModel.Order.Total,
                Installment = paymentModel.Order.Installment,
                Id = Guid.NewGuid()
            };
        }
    }
}