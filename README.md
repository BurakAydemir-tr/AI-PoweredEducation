# AI-Powered Education

AI destekli öğrenme oyunları oluşturmak ve oynatmak için geliştirilen platformun backend proje iskeletidir.

## Source of truth

Projenin ürün, mimari ve teknik kararları [`docs`](docs) klasöründeki dokümanlarda tanımlanır. Uygulama geliştirilirken özellikle `07-BackendArchitecture.md` ve `08-TechnicalDecisions.md` esas alınır.

## Teknoloji ve mimari

- C# ve ASP.NET Core Web API
- .NET 9
- N-Layer Architecture
- PostgreSQL ve Entity Framework Core (ileriki aşamalar için onaylanmış teknoloji)

## Solution yapısı

```text
src/Backend/
├─ AI.PoweredEducation.Core
├─ AI.PoweredEducation.Entity
├─ AI.PoweredEducation.DataAccess
├─ AI.PoweredEducation.Business
└─ AI.PoweredEducation.API
```

Katmanlar arasındaki proje referansları:

```text
Core
Entity     -> Core
DataAccess -> Core, Entity
Business   -> Core, Entity, DataAccess
API        -> Core, Business
```

Repository interface'leri ve implementasyonları ileriki aşamalarda DataAccess katmanında yer alacaktır.

## Mevcut kapsam

Repository şu anda yalnızca proje iskeletini içerir. Entity, DbContext, controller, service, kimlik doğrulama ve iş kuralları henüz uygulanmamıştır.

## Derleme

Gerekli sürümler: .NET SDK **9.0.318** (Visual Studio 2022 17.14 uyumlu),
.NET Runtime ve ASP.NET Core Runtime **9.0.20** veya aynı 9.0 serisindeki daha yeni
kararlı güvenlik yamaları. `global.json` SDK'nın 9.0.3xx bandında kalmasını,
API runtime ayarları ise 9.0.20 altındaki runtime'larla açılmamasını sağlar.
TargetFramework `net9.0` olarak korunur. Production hosting ortamında da bu
runtime'lar bulunmalıdır; NuGet paket güncellemesi sistem runtime'ını güncellemez.

```powershell
dotnet restore AI.PoweredEducation.sln --source https://api.nuget.org/v3/index.json
dotnet build AI.PoweredEducation.sln
```

## Yerel yapılandırma

Development ortamında API projesindeki `UserSecretsId` ve `WebApplication.CreateBuilder`
sayesinde ASP.NET Core User Secrets otomatik yüklenir. Visual Studio'da API projesine
sağ tıklayıp **Manage User Secrets** ile aşağıdaki anahtarları yerel secret dosyasına
ekleyin. Aşağıdaki değerler yalnızca yer tutucudur; gerçek değerleri repository'ye,
terminal çıktısına veya shell geçmişine yazmayın.

```json
{
  "ConnectionStrings:DefaultConnection": "<local PostgreSQL connection string>",
  "Jwt:Secret": "<new cryptographically random signing secret, at least 32 bytes>"
}
```

User Secrets repository dışında saklanır ve yalnızca geliştirme içindir; şifreli
bir production secret store değildir. Mevcut AI provider secret ayarlarını koruyun.

Production ortamında `ASPNETCORE_ENVIRONMENT=Production` kullanın ve dağıtım
ortamının secret yönetimi üzerinden aşağıdaki environment variable'ları sağlayın:

```text
ConnectionStrings__DefaultConnection
Jwt__Secret
```

`Jwt__Secret` en az 32 byte olmalıdır. Secret ve veritabanı parolaları repository'ye eklenmemelidir.

Gerekli değerler yoksa uygulamanın açılması başarısız olur; kaynak kodunda varsayılan
parola veya imzalama anahtarı bulunmaz. `.env` dosyaları ASP.NET Core tarafından
otomatik yüklenmez; production ortam değişkenlerini hosting ortamı sağlamalıdır.

Önceden Git'e eklenmiş JWT anahtarı ve PostgreSQL parolası ifşa olmuş kabul
edilmelidir. Kullanıldıkları tüm ortamlarda JWT anahtarını ve veritabanı parolasını
manuel olarak döndürün; mevcut refresh tokenları da iptal edin. User Secrets'a
taşıma veritabanındaki parolayı değiştirmez. Git geçmişindeki eski değerleri
silmek, rotasyonun yerine geçmez; geçmiş temizliği ayrı ve koordineli bir işlemdir.

## Veritabanı

Repository-local EF Core aracını ve migration'ları çalıştırmak için:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/Backend/AI.PoweredEducation.DataAccess --startup-project src/Backend/AI.PoweredEducation.API
```

## AI provider durumu

`IAiProvider` ve `IAiService` sözleşmeleri üzerinden OpenAI ve Gemini provider implementasyonları eklenmiştir.

Provider seçimi config üzerinden yapılır. Varsayılan provider OpenAI'dir:

```powershell
dotnet user-secrets set "AI:Provider" "OpenAI" --project src/Backend/AI.PoweredEducation.API
dotnet user-secrets set "OpenAI:ApiKey" "<openai_api_key>" --project src/Backend/AI.PoweredEducation.API
dotnet user-secrets set "OpenAI:Model" "gpt-4.1-mini" --project src/Backend/AI.PoweredEducation.API
```

Gemini kullanmak için:

```powershell
dotnet user-secrets set "AI:Provider" "Gemini" --project src/Backend/AI.PoweredEducation.API
dotnet user-secrets set "Gemini:ApiKey" "<gemini_api_key>" --project src/Backend/AI.PoweredEducation.API
dotnet user-secrets set "Gemini:Model" "gemini-2.5-flash" --project src/Backend/AI.PoweredEducation.API
```

AI endpointleri öğretmen JWT token'ı gerektirir:

```text
POST /api/ai/quiz-tasks
POST /api/ai/qr-code-tasks
```
