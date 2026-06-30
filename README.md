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

```powershell
dotnet restore AI.PoweredEducation.sln --source https://api.nuget.org/v3/index.json
dotnet build AI.PoweredEducation.sln
```

## Yerel yapılandırma

API aşağıdaki değerleri environment variable veya güvenli bir configuration provider üzerinden bekler:

```text
ConnectionStrings__DefaultConnection
Jwt__Secret
```

`Jwt__Secret` en az 32 byte olmalıdır. Secret ve veritabanı parolaları repository'ye eklenmemelidir.

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
