# SanalPosTR - Project Guide

## Project Overview
Turkish virtual POS (payment gateway) integration library for .NET. Multi-targets `netstandard2.0` and `net8.0`. Provides a unified API for processing payments through multiple Turkish banks.

## Tech Stack
- C# (multi-target: netstandard2.0 + net8.0)
- DotLiquid (XML template engine)
- HttpClient via `IHttpClientFactory` with DelegatingHandler pipeline (HTTP client)
- Serilog (structured logging)
- NUnit (testing)
- GitHub Actions (CI/CD)

## Project Structure
```
src/
├── SanalPosTR/                  # Main library
│   ├── Providers/               # Bank-specific implementations
│   │   ├── IProviderService.cs  # Core interface
│   │   ├── BaseProviderService.cs
│   │   ├── NestPay/             # Ziraat, Akbank, Finans, İşbank, Halk, Anadolu, TEB
│   │   ├── YKB/                 # Yapı Kredi
│   │   ├── Garanti/             # Garanti Bankası
│   │   └── KuveytTurk/         # Kuveyt Türk
│   ├── Model/                   # Payment, order, card models
│   ├── Helper/                  # Hash, template, string utilities
│   ├── HttpServices/            # SanalPosHttpClient + HttpLoggingHandler
│   ├── Configuration/           # Bank configurations & endpoints
│   └── Resources/               # DotLiquid XML templates (embedded)
├── SanalPosTR.Test/             # NUnit tests
└── SanalPosTR.Playground/       # Demo ASP.NET Core web app
```

## Architecture
- **Strategy Pattern**: `IProviderService` → `BaseProviderService` → bank-specific providers
- **DI**: `services.AddSanalPosTR()` / `app.UseSanalPosTR(options => ...)`
- **Template Engine**: DotLiquid renders XML request templates from embedded resources
- **Factory Pattern**: `Func<BankTypes, IProviderService>` resolves providers by bank type

## Key Commands
```bash
# Build
dotnet build src/SanalPosTR.sln --configuration Release

# Test
dotnet test src/SanalPosTR.Test/

# Pack NuGet
dotnet pack src/SanalPosTR/SanalPosTR.csproj --configuration Release -o bin/Release/Publish
```

## Supported Banks
| Bank | Provider | Status |
|------|----------|--------|
| Ziraat, Akbank, Finans, İşbank, Halk, Anadolu, TEB | NestPay/EST | Implemented |
| Yapı Kredi (YKB) | Posnet | Implemented |
| Garanti | Garanti | Implemented |
| Kuveyt Türk | BOA | Implemented (3D only, no refund) |
| DenizBank, VakıfBank | - | Not yet implemented |

## Payment Flows
1. **Standard**: `ProcessPayment()` → render XML template → POST to bank → parse response
2. **3D Secure**: `ProcessPayment(Use3DSecure=true)` → generate hash → HTML form redirect → bank 3D auth → `VerifyPayment()` → final result
3. **Refund**: `ProcessRefund()` → render refund XML → POST to bank

## Logging Convention
- **Information**: Method entry/exit (provider name, orderId, status, elapsed time)
- **Debug**: Internal details (hash values, template compilation, request/response bodies)
- **Warning**: Non-success HTTP responses
- **Error**: Exceptions with elapsed time
- `HttpLoggingHandler` (DelegatingHandler) handles all HTTP-level logging with `HttpProcessId` correlation via `LogContext.PushProperty`
- Structured logging format: `Log.Information("Message {Placeholder}", value)` — no string interpolation

## Important Notes
- Bank-specific hash algorithms: NestPay uses SHA1, YKB uses SHA256, Garanti uses SHA1 hex, Kuveyturk uses UTF-8 SHA1
- ISO-8859-9 (Turkish) encoding support is registered via CodePages
- XML templates are embedded resources under `Resources/{Provider}/`
- Definition.cs is the central registry mapping BankTypes → providers, configs, and URLs
- Test credentials are in Initialize.cs (test project) — never use real credentials in code
