# SanalPosTR

**Türkiye Bankaları Sanal Pos Ortak Kullanım Altyapısı**

[![.NET](https://github.com/firatoltulu/SanalPosTR/actions/workflows/dotnet.yml/badge.svg)](https://github.com/firatoltulu/SanalPosTR/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/SanalPosTR.svg)](https://www.nuget.org/packages/SanalPosTR)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SanalPosTR.svg)](https://www.nuget.org/packages/SanalPosTR)

SanalPosTR, Türkiye'deki bankaların sanal POS sistemlerini tek bir ortak arayüz üzerinden kullanmanızı sağlayan .NET kütüphanesidir. `netstandard2.0` ve `net8.0` hedefleyerek .NET Framework 4.6.1+ ve .NET 6/7/8 projelerinde kullanılabilir.

---

## Özellikler

- Tek bir API ile birden fazla banka entegrasyonu
- Normal ödeme, 3D Secure ödeme ve iade desteği
- `IHttpClientFactory` tabanlı HTTP yönetimi (connection pooling, retry policy desteği)
- Serilog ile yapılandırılmış (structured) loglama
- DotLiquid şablon motoru ile XML istek oluşturma
- ASP.NET Core Dependency Injection entegrasyonu
- Banka bazında test/production ortam desteği

---

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
| Garanti BBVA | Garanti | ✅ | ✅ | ✅ |
| Kuveyt Türk | BOA | ✅ | ❌ | ❌ |
| DenizBank | — | ❌ | ❌ | ❌ |
| VakıfBank | — | ❌ | ❌ | ❌ |

> **Not:** Kuveyt Türk yalnızca 3D Secure üzerinden ödeme destekler. Normal ödeme ve iade henüz implementasyona dahil değildir.

---

## Kurulum

### NuGet ile

```bash
dotnet add package SanalPosTR
```

### Package Manager Console

```powershell
Install-Package SanalPosTR
```

### PackageReference

```xml
<PackageReference Include="SanalPosTR" Version="2.0.1" />
```

**Gereksinimler:**
- `netstandard2.0` (.NET Framework 4.6.1+, .NET Core 2.0+) veya `net8.0`

---

## Hızlı Başlangıç

### 1. Servisleri Kaydet

```csharp
// Program.cs veya Startup.cs
services.AddSanalPosTR();
```

### 2. Banka Yapılandırması

```csharp
app.UseSanalPosTR(options =>
{
    // NestPay tabanlı bankalar (Ziraat, Akbank, Finans, İşbank, Halk, Anadolu, TEB)
    options.UseZiraat(new NestPayConfiguration
    {
        ClientId = "100100000",
        Username = "YOUR_USERNAME",
        Password = "YOUR_PASSWORD",
        HashKey = "YOUR_HASH_KEY",
        Mode = "T" // T: Test, P: Production
    });

    // Yapı Kredi
    options.UseYKB(new YKBConfiguration
    {
        MerchantId = "YOUR_MERCHANT_ID",
        TerminalId = "YOUR_TERMINAL_ID",
        PosnetId = "YOUR_POSNET_ID",
        HashKey = "YOUR_HASH_KEY"
    });

    // Garanti BBVA
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

    public async Task<IActionResult> Pay()
    {
        var provider = _providerFactory(BankTypes.Ziraat);

        var model = new PaymentModel
        {
            CreditCard = new CreditCardInfo
            {
                CardNumber = "4546711234567894",
                CVV2 = "000",
                ExpireMonth = "12",
                ExpireYear = "2026",
                CardHolderName = "Test Kullanıcı"
            },
            Order = new OrderInfo
            {
                OrderId = Guid.NewGuid().ToString(),
                Total = 1.00m,
                CurrencyCode = "949", // 949: TRY, 840: USD, 978: EUR
                Installment = 1,
                IP = "127.0.0.1"
            },
            Use3DSecure = false
        };

        var result = await provider.ProcessPayment(model);

        if (result.Status)
            return Ok($"Ödeme başarılı. Provizyon No: {result.ProvisionNumber}");
        else
            return BadRequest($"Hata: {result.Error} ({result.ErrorCode})");
    }
}
```

---

## Ödeme Akışları

### Normal Ödeme

```
Uygulama → ProcessPayment() → XML şablon render → POST (banka API) → Yanıt parse → PaymentResult
```

```csharp
var model = new PaymentModel
{
    CreditCard = new CreditCardInfo { /* kart bilgileri */ },
    Order = new OrderInfo { /* sipariş bilgileri */ },
    Use3DSecure = false
};

var result = await provider.ProcessPayment(model);
// result.Status → başarılı mı?
// result.ProvisionNumber → provizyon numarası
// result.ReferanceNumber → referans numarası
```

### 3D Secure Ödeme

3D Secure akışı üç adımdan oluşur:

```
1. ProcessPayment()    → HTML form üretir (bankaya yönlendirme)
2. Banka 3D doğrulama  → Kullanıcı SMS/PIN ile doğrular → Geri yönlendirir
3. VerifyPayment()     → Banka yanıtını doğrular → Son sonuç
```

**Adım 1 — Ödeme başlat ve bankaya yönlendir:**

```csharp
var model = new PaymentModel
{
    CreditCard = new CreditCardInfo { /* kart bilgileri */ },
    Order = new OrderInfo { /* sipariş bilgileri */ },
    Use3DSecure = true
};

var result = await provider.ProcessPayment(model);

if (result.IsRedirectContent)
{
    // Banka 3D sayfasına yönlendirme HTML formu
    return Content(result.ServerResponseRaw, "text/html");
}
```

**Adım 2 — Banka dönüşünü doğrula:**

```csharp
[HttpPost("payment/callback")]
public async Task<IActionResult> PaymentCallback()
{
    var provider = _providerFactory(BankTypes.Ziraat);

    var verifyModel = new VerifyPaymentModel
    {
        Order = new OrderInfo
        {
            OrderId = "orijinal-siparis-id"
        }
    };

    var result = await provider.VerifyPayment(verifyModel, Request.Form);

    if (result.Status)
        return RedirectToAction("Success");
    else
        return RedirectToAction("Fail", new { error = result.Error });
}
```

### İade İşlemi

```csharp
var refund = new Refund
{
    OrderId = "orijinal-siparis-id",
    RefundAmount = 50.00m,
    CurrencyCode = "949",
    ProvisionNumber = "orijinal-provizyon-no",
    ReferanceNumber = "orijinal-referans-no"
};

var result = await provider.ProcessRefound(refund);

if (result.Status)
    Console.WriteLine("İade başarılı");
```

---

## Banka Yapılandırma Detayları

### NestPay (Ziraat, Akbank, Finans, İşbank, Halk, Anadolu, TEB)

```csharp
options.UseZiraat(new NestPayConfiguration
{
    ClientId = "100100000",        // Mağaza numarası
    Username = "api_user",         // API kullanıcı adı
    Password = "api_password",     // API şifresi
    HashKey = "SKEY_123456",       // Store Key (3D hash için)
    Mode = "T"                     // T: Test, P: Production
});

// Diğer NestPay bankaları için aynı yapılandırma kullanılır:
// options.UseAkbank(config), options.UseFinansbank(config),
// options.UseIsbank(config), options.UseHalkbank(config),
// options.UseAnadolubank(config), options.UseTEB(config)
```

### Yapı Kredi (YKB)

```csharp
options.UseYKB(new YKBConfiguration
{
    MerchantId = "6706598320",     // Üye işyeri numarası
    TerminalId = "67005551",       // Terminal numarası
    PosnetId = "1010028724",       // Posnet ID
    HashKey = "10,10,10,10,10,10,10,10" // Encryption key
});
```

### Garanti BBVA

```csharp
options.UseGaranti(new GarantiConfiguration
{
    TerminalId = "30691297",       // Terminal numarası
    UserId = "PROVAUT",            // Kullanıcı adı
    MerchantId = "7000679",        // Mağaza numarası
    SecureKey = "12345678",        // 3D Secure anahtarı
    Password = "123qweASD/"        // Terminal şifresi
});
```

### Kuveyt Türk

```csharp
options.UseKuveytTurk(new KuveytTurkConfiguration
{
    MerchantId = "496",            // Mağaza numarası
    CustomerId = "400235",         // Müşteri numarası
    UserName = "apitest",          // API kullanıcı adı
    Password = "api123"            // API şifresi
});
```

### Ortam Ayarları

```csharp
// Belirli bir banka için test ortamı etkinleştir
options.SetBankEnvironment(BankTypes.Ziraat, useTestEndPoint: true);
options.SetBankEnvironment(BankTypes.Garanti, useTestEndPoint: true);

// 3D Secure dönüş URL'leri (tüm bankalar için geçerli)
options.SetSuccessReturnUrl("https://example.com/payment/success");
options.SetFailReturnUrl("https://example.com/payment/fail");
```

---

## Modeller

### PaymentModel

```csharp
var model = new PaymentModel
{
    CreditCard = new CreditCardInfo
    {
        CardNumber = "4546711234567894",
        CVV2 = "000",
        ExpireMonth = "12",         // MM formatı
        ExpireYear = "2026",        // YYYY formatı
        CardHolderName = "Ad Soyad"
    },
    Order = new OrderInfo
    {
        OrderId = "unique-order-id",
        Total = 100.50m,            // Tutar
        CurrencyCode = "949",       // 949: TRY, 840: USD, 978: EUR
        Installment = 3,            // Taksit sayısı (1 = peşin)
        IP = "192.168.1.1",         // Müşteri IP adresi
        EMail = "musteri@email.com" // Müşteri e-posta
    },
    Use3DSecure = true              // 3D Secure kullanımı
};
```

### PaymentResult

```csharp
PaymentResult result = await provider.ProcessPayment(model);

result.Status;             // bool   — İşlem başarılı mı?
result.ProvisionNumber;    // string — Provizyon numarası
result.ReferanceNumber;    // string — Referans numarası
result.Error;              // string — Hata mesajı
result.ErrorCode;          // string — Hata kodu
result.IsRedirectContent;  // bool   — 3D yönlendirme HTML'i mi?
result.ServerResponseRaw;  // string — Bankanın ham yanıtı
result.OrderContentRaw;    // string — Gönderilen istek içeriği
```

### Refund

```csharp
var refund = new Refund
{
    OrderId = "siparis-id",
    RefundAmount = 50.00m,
    CurrencyCode = "949",
    ProvisionNumber = "provizyon-no",
    ReferanceNumber = "referans-no"
};
```

### Para Birimi Kodları

| Kod | Para Birimi |
|-----|-------------|
| `949` | Türk Lirası (TRY) |
| `840` | Amerikan Doları (USD) |
| `978` | Euro (EUR) |
| `826` | İngiliz Sterlini (GBP) |

---

## Mimari

### Strategy Pattern

Proje, **Strategy Pattern** üzerine inşa edilmiştir. Tüm banka sağlayıcıları ortak bir arayüzü (`IProviderService`) implemente eder:

```
IProviderService (Arayüz)
│   ProcessPayment()
│   VerifyPayment()
│   ProcessRefound()
│
├── BaseProviderService (Ortak temel işlemler)
│   ├── NestPayProviderService  → Ziraat, Akbank, Finans, İşbank, Halk, Anadolu, TEB
│   ├── YKBProviderServices     → Yapı Kredi
│   ├── GarantiProviderService  → Garanti BBVA
│   └── KuveytTurkProviderServices → Kuveyt Türk
```

### Factory Pattern

Banka sağlayıcıları DI üzerinden `Func<BankTypes, IProviderService>` factory delegate ile çözülür:

```csharp
// Constructor injection
private readonly Func<BankTypes, IProviderService> _providerFactory;

// Kullanım
var provider = _providerFactory(BankTypes.Garanti);
```

### XML Şablon Motoru (DotLiquid)

Her banka sağlayıcısı için XML istek şablonları embedded resource olarak saklanır. Çalışma zamanında `DotLiquid` ile render edilir:

```
Providers/
├── NestPay/Resources/    → Pay.xml, 3D.xml, 3DEnd.xml, Refund.xml
├── YKB/Resources/        → Pay.xml, 3D.xml, 3D_before.xml, 3D_Resolve.xml, 3DEnd.xml, Refund.xml
├── Garanti/Resources/    → Pay.xml, 3D.xml, 3DEnd.xml, Refund.xml
└── KuveytTurk/Resources/ → 3D.xml, 3DEnd.xml
```

### HTTP Katmanı

- **SanalPosHttpClient**: `IHttpClientFactory` üzerinden inject edilen `HttpClient` ile banka API'lerine istek gönderir
- **HttpLoggingHandler**: `DelegatingHandler` olarak HTTP istek/yanıtlarını Serilog ile loglar. Her istek bir `HttpProcessId` (GUID) ile korelasyonlanır

### Hash Algoritmaları

| Banka | Algoritma | Encoding | Format |
|-------|-----------|----------|--------|
| NestPay | SHA-512 (ver3) | UTF-8 | Base64 |
| Garanti | SHA-1 | ISO-8859-9 | Hexadecimal |
| YKB | SHA-256 HMAC | UTF-8 | Base64 |
| Kuveyt Türk | SHA-1 | ISO-8859-9 | Base64 |

---

## Loglama

SanalPosTR, Serilog ile yapılandırılmış loglama kullanır. Log seviyeleri:

| Seviye | Kullanım |
|--------|----------|
| **Information** | Metot giriş/çıkışı (provider adı, orderId, durum, süre) |
| **Debug** | Hash değerleri, şablon derleme, istek/yanıt gövdeleri |
| **Warning** | Başarısız HTTP yanıtları |
| **Error** | Exception detayları ve süre bilgisi |

```csharp
// Serilog yapılandırması (Program.cs)
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext(); // HttpProcessId korelasyonu için gerekli
});
```

`HttpLoggingHandler`, tüm HTTP trafiğini otomatik olarak loglar. Ekstra yapılandırma gerekmez.

---

## Proje Yapısı

```
SanalPosTR/
├── src/
│   ├── SanalPosTR/                      # Ana kütüphane
│   │   ├── Providers/                   # Banka sağlayıcı implementasyonları
│   │   │   ├── IProviderService.cs      # Temel arayüz
│   │   │   ├── BaseProviderService.cs   # Ortak temel sınıf
│   │   │   ├── NestPay/                 # Ziraat, Akbank, Finans, İşbank, Halk, Anadolu, TEB
│   │   │   ├── YKB/                     # Yapı Kredi
│   │   │   ├── Garanti/                 # Garanti BBVA
│   │   │   └── KuveytTurk/             # Kuveyt Türk
│   │   ├── Model/                       # Ödeme, sipariş, kart modelleri
│   │   ├── Helper/                      # Hash, şablon, string yardımcıları
│   │   ├── HttpServices/               # HttpClient wrapper + HttpLoggingHandler
│   │   ├── Configuration/              # Banka yapılandırmaları ve endpoint'ler
│   │   └── Resources/                  # DotLiquid XML şablonları (embedded)
│   ├── SanalPosTR.Test/                # NUnit test projesi
│   └── SanalPosTR.Playground/          # Demo ASP.NET Core web uygulaması
├── .github/workflows/
│   ├── dotnet.yml                      # Build & test (master push/PR)
│   └── release.yml                     # NuGet publish (release branch)
└── README.md
```

---

## Geliştirme

### Build

```bash
dotnet build src/SanalPosTR.sln --configuration Release
```

### Test

```bash
dotnet test src/SanalPosTR.Test/
```

### NuGet Paketi Oluştur

```bash
dotnet pack src/SanalPosTR/SanalPosTR.csproj --configuration Release -o bin/Release/Publish
```

### CI/CD

| Workflow | Tetikleyici | İşlem |
|----------|-------------|-------|
| `dotnet.yml` | master'a push/PR | Build → Test → Pack |
| `release.yml` | release branch'e push/PR | Build → Test → Pack → NuGet Publish |

---

## Katkıda Bulunma

Katkılarınızı memnuniyetle karşılıyoruz! Özellikle şu alanlarda katkıya açığız:

- **DenizBank** entegrasyonu
- **VakıfBank** entegrasyonu
- Mevcut sağlayıcılar için eksik özellikler (Kuveyt Türk normal ödeme ve iade)
- Birim test kapsamının artırılması

### Yeni Banka Entegrasyonu Ekleme

1. `Providers/` altında yeni bir klasör oluşturun (örn. `Providers/DenizBank/`)
2. `BaseProviderService`'den türeyen bir sınıf yazın
3. `Resources/` altına XML şablonlarını ekleyin (Pay.xml, 3D.xml, 3DEnd.xml, Refund.xml)
4. `Definition.cs`'de banka → sağlayıcı eşleşmesini tanımlayın
5. `ConfigurationService.cs`'de `UseDenizbank()` gibi bir yapılandırma metodu ekleyin
6. Test projesi altına testleri yazın

---

## Lisans

Bu proje açık kaynaklıdır.
