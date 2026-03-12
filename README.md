# SanalPosTR

**Türkiye Bankaları Sanal Pos Ortak Kullanım Altyapısı**

[![.NET](https://github.com/firatoltulu/SanalPosTR/actions/workflows/dotnet.yml/badge.svg)](https://github.com/firatoltulu/SanalPosTR/actions/workflows/dotnet.yml)

SanalPosTR, Türkiye'deki bankaların sanal POS sistemlerini tek bir ortak arayüz üzerinden kullanmanızı sağlayan .NET 6 kütüphanesidir. E-ticaret projelerinizde farklı banka entegrasyonlarını kolayca yönetebilirsiniz.

## Desteklenen Bankalar

| Banka | Altyapı | 3D Secure | Normal Ödeme | İade |
|-------|---------|:---------:|:------------:|:----:|
| Ziraat Bankası | NestPay/EST | ✅ | ✅ | ✅ |
| Akbank | NestPay/EST | ✅ | ✅ | ✅ |
| Finans Bank | NestPay/EST | ✅ | ✅ | ✅ |
| İş Bankası | NestPay/EST | ✅ | ✅ | ✅ |
| Halk Bankası | NestPay/EST | ✅ | ✅ | ✅ |
| Anadolu Bank | NestPay/EST | ✅ | ✅ | ✅ |
| TEB | NestPay/EST | ✅ | ✅ | ✅ |
| Yapı Kredi (YKB) | Posnet | ✅ | ✅ | ✅ |
| Garanti Bankası | Garanti | ✅ | ✅ | ✅ |
| Kuveyt Türk | BOA | ✅ | ❌ | ❌ |
| DenizBank | - | ❌ | ❌ | ❌ |
| VakıfBank | - | ❌ | ❌ | ❌ |

## Kurulum

```bash
dotnet add package SanalPosTR
```

## Kullanım

### 1. Servisleri Kaydet (Startup / Program.cs)

```csharp
// DI servis kaydı
services.AddSanalPosTR();
```

### 2. Banka Yapılandırması

```csharp
app.UseSanalPosTR(options =>
{
    // NestPay tabanlı bankalar (Ziraat, Akbank, İşbank vb.)
    options.UseZiraat(new NestPayConfiguration
    {
        ClientId = "100100000",
        HashKey = "YOUR_HASH_KEY",
        Mode = "T", // T: Test, P: Production
        Type = "3d" // 3d veya standart
    });

    // Yapı Kredi
    options.UseYKB(new YKBConfiguration
    {
        MerchantId = "YOUR_MERCHANT_ID",
        TerminalId = "YOUR_TERMINAL_ID",
        PosnetId = "YOUR_POSNET_ID",
        HashKey = "YOUR_HASH_KEY"
    });

    // Garanti Bankası
    options.UseGaranti(new GarantiConfiguration
    {
        TerminalId = "YOUR_TERMINAL_ID",
        UserId = "YOUR_USER_ID",
        MerchantId = "YOUR_MERCHANT_ID",
        SecureKey = "YOUR_SECURE_KEY",
        Password = "YOUR_PASSWORD"
    });

    // Kuveyt Türk
    options.UseKuveytTurk(new KuveytTurkConfiguration
    {
        MerchantId = "YOUR_MERCHANT_ID",
        CustomerId = "YOUR_CUSTOMER_ID",
        UserName = "YOUR_USERNAME",
        Password = "YOUR_PASSWORD"
    });

    // 3D Secure dönüş URL'leri
    options.SetSuccessReturnUrl("https://example.com/payment/success");
    options.SetFailReturnUrl("https://example.com/payment/fail");

    // Test ortamı kullanımı
    options.SetBankEnvironment(BankTypes.Ziraat, useTestEndPoint: true);
});
```

### 3. Ödeme İşlemi

```csharp
public class PaymentController : Controller
{
    private readonly Func<BankTypes, IProviderService> _providerFactory;

    public PaymentController(Func<BankTypes, IProviderService> providerFactory)
    {
        _providerFactory = providerFactory;
    }

    // Normal ödeme
    public IActionResult Pay()
    {
        var provider = _providerFactory(BankTypes.Ziraat);

        var model = new PaymentModel
        {
            CreditCard = new CreditCardInfo
            {
                CardNumber = "4546711234567894",
                CVV = "000",
                Month = 12,
                Year = 2026,
                CardHolderName = "Test Kullanıcı"
            },
            Order = new OrderInfo
            {
                OrderId = Guid.NewGuid().ToString(),
                Amount = 1.00m,
                CurrencyCode = "TL",
                Installment = 1
            },
            Use3DSecure = false
        };

        var result = provider.ProcessPayment(model);

        if (result.Status)
            return Ok("Ödeme başarılı");
        else
            return BadRequest(result.ErrorMessage);
    }

    // 3D Secure ödeme
    public IActionResult Pay3D()
    {
        var provider = _providerFactory(BankTypes.Ziraat);

        var model = new PaymentModel
        {
            CreditCard = new CreditCardInfo { /* ... */ },
            Order = new OrderInfo { /* ... */ },
            Use3DSecure = true
        };

        var result = provider.ProcessPayment(model);

        if (result.IsRedirectContent)
            return Content(result.Content, "text/html"); // Bankaya yönlendirme formu
        else
            return BadRequest(result.ErrorMessage);
    }

    // 3D Secure doğrulama (banka dönüşü)
    [HttpPost]
    public IActionResult PaymentCallback()
    {
        var provider = _providerFactory(BankTypes.Ziraat);

        var verifyModel = new VerifyPaymentModel();
        var result = provider.VerifyPayment(verifyModel, Request.Form);

        if (result.Status)
            return RedirectToAction("Success");
        else
            return RedirectToAction("Fail");
    }

    // İade
    public IActionResult Refund(string orderId, decimal amount)
    {
        var provider = _providerFactory(BankTypes.Ziraat);

        var refund = new Refund
        {
            OrderId = orderId,
            Amount = amount
        };

        var result = provider.ProcessRefund(refund);
        return Ok(result);
    }
}
```

## Mimari

Proje **Strategy Pattern** üzerine inşa edilmiştir:

```
IProviderService (Arayüz)
├── BaseProviderService (Ortak işlemler)
│   ├── NestPayProviderService  → Ziraat, Akbank, Finans, İşbank, Halk, Anadolu, TEB
│   ├── YKBProviderServices     → Yapı Kredi
│   ├── GarantiProviderService  → Garanti
│   └── KuveytTurkProviderServices → Kuveyt Türk
```

Her banka için XML şablonları (`DotLiquid`) embedded resource olarak saklanır ve çalışma zamanında render edilir.

## Geliştirme

```bash
# Build
dotnet build src/SanalPosTR.sln

# Test
dotnet test src/SanalPosTR.Test/

# NuGet paketi oluştur
dotnet pack src/SanalPosTR/SanalPosTR.csproj -c Release -o bin/Release/Publish
```

## Katkıda Bulunma

Açık issue'lardaki entegrasyon maddelerinden herhangi birini yapabilirsiniz. Özellikle DenizBank ve VakıfBank entegrasyonları katkıya açıktır.

## Lisans

Bu proje açık kaynaklıdır.
