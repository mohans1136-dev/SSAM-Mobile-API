# SSAM Mobile API

Secure .NET 10 Web API over an existing MS SQL database, hosted on Azure App
Service via Azure DevOps, consumed by an OutSystems app using Entra ID
(OIDC/OAuth2) bearer tokens.

---

## 1. What's in here

```
SSAM-Mobile-API/
├─ SsamMobileApi.slnx                   Solution file
├─ azure-pipelines.yml                  Azure DevOps CI/CD pipeline
├─ .config/dotnet-tools.json            Pins the `dotnet ef` CLI version
├─ .gitignore
└─ src/SsamMobileApi/
   ├─ SsamMobileApi.csproj              Project + NuGet package versions
   ├─ Program.cs                        App startup / all wiring
   ├─ appsettings.json                  Config template (no secrets)
   ├─ appsettings.Development.json       Local dev overrides
   ├─ Properties/launchSettings.json    F5 profile (opens /scalar)
   ├─ Controllers/
   │  └─ MrController.cs                Read endpoints for MR / MRDetail
   ├─ Services/
   │  ├─ IMrService.cs                  Read-side contract + MrQuery filters
   │  └─ MrService.cs                   Implementation (only place touching the DB)
   ├─ Models/
   │  ├─ MrDtos.cs                      Response shapes (mirror the DB columns)
   │  ├─ MrMappings.cs                  Entity -> DTO projections
   │  └─ CommonDtos.cs                  PagedResult<T>
   ├─ Data/
   │  ├─ AppDbContext.cs                EF Core context (DbSets + FK config)
   │  ├─ AppDbContextFactory.cs         Design-time factory for `dotnet ef`
   │  └─ Entities/
   │     ├─ MR.cs                       Maps [MTL].[MR] exactly (38 columns)
   │     └─ MRDetail.cs                 Maps [MTL].[MRDetail] exactly (68 columns)
   └─ Infrastructure/
      ├─ GlobalExceptionHandler.cs      Turns exceptions into clean problem+json
      ├─ DatabaseHealthCheck.cs         SELECT 1 against SQL for /health/db
      ├─ HealthCheckResponse.cs         JSON writer for health endpoints
      └─ DevAuthHandler.cs              Development-only fake auth
```

`MR.cs` / `MRDetail.cs` replicate the production `[MTL]` schema **exactly** —
column names, types and nullability, including apparent typos like
`OrderRecjectedBy`. Keep them in sync with the real tables; do not "fix" the
names. `db/01-schema.sql` builds the same two tables in LocalDB.

---

## 2. Moving this code into the AVD

The real code and client data live in the Azure Virtual Desktop. This repo was
built outside it, so you need to transfer the files in. Pick whichever the AVD
session policy allows:

| Method | How |
| --- | --- |
| **Clipboard file copy** (most AVD setups) | Select all files in this folder in Explorer, Ctrl+C, paste into the empty project folder in the AVD. |
| **Zip via clipboard** | Run `powershell -ExecutionPolicy Bypass -File tools\bundle.ps1` — it writes `ssam-mobile-api.zip` with `bin/obj/.git` excluded. Copy that one file in and extract. |
| **Text-only clipboard** | Recreate each file by hand in Visual Studio using the tree above, pasting file contents one at a time. Start with `.csproj`, then `Program.cs`, then the folders. |

Full walkthrough (clone vs zip, restore, point at the real DB, verify, push back): **[docs/migrate-to-avd.md](docs/migrate-to-avd.md)**.

### After the files are in the AVD

1. Open the `.slnx` (or the `.csproj`) in Visual Studio.
2. Right-click the solution → **Restore NuGet Packages** (or `dotnet restore`).
   All package versions are pinned in the `.csproj`, so you get exactly what was
   tested here.
3. Build. If the AVD has no internet access to nuget.org, see section 8.
4. Set the local connection string and Entra ID values (section 3).

> The `.csproj` is the source of truth for dependencies. If you paste files
> individually, paste the `.csproj` **first** and restore before adding the rest,
> so IntelliSense resolves types as you go.

---

## 3. Configuration

Nothing secret is committed. Config is read from (in order) `appsettings.json` →
`appsettings.{Environment}.json` → **user secrets** (local) / **environment
variables** (Azure).

### Local development

From `src/SsamMobileApi/`:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:SqlDb" "Server=YOUR_SQL;Database=YOUR_DB;User Id=...;Password=...;TrustServerCertificate=True;Encrypt=True"
dotnet user-secrets set "AzureAd:TenantId" "<entra-tenant-guid>"
dotnet user-secrets set "AzureAd:ClientId" "<api-app-registration-client-id>"
dotnet user-secrets set "AzureAd:Audience" "api://<api-app-registration-client-id>"
```

User secrets live outside the repo (`%APPDATA%\Microsoft\UserSecrets\`), so they
never get committed or copied by accident.

### Keys explained

| Key | Meaning |
| --- | --- |
| `ConnectionStrings:SqlDb` | ADO.NET connection string to the MS SQL database. |
| `AzureAd:Instance` | Always `https://login.microsoftonline.com/`. |
| `AzureAd:TenantId` | Your Entra ID tenant (directory) GUID. |
| `AzureAd:ClientId` | Client ID of the **app registration that represents this API**. |
| `AzureAd:Audience` | The token audience this API accepts, usually `api://<ClientId>`. |
| `Cors:AllowedOrigins` | Only needed if a browser calls the API directly. Leave `[]` for OutSystems server-side integration. |

---

## 3b. Developing without access to the real SQL Server

The real database lives in the AVD. Outside it you reproduce it locally with
**SQL Server LocalDB** — the same SQL Server engine, installed with Visual Studio
/ the SQL tools, so T-SQL and EF Core behave identically. No Docker needed.

```
db/
├─ 01-schema.sql   MTL.MR + MTL.MRDetail, replicating production exactly
└─ 02-seed.sql     fake rows for local testing — never real client data
tools/db-reset.ps1   drops + rebuilds + seeds the local DB
```

### Make local match production

1. In the AVD: SSMS → right-click the real DB → **Tasks → Generate Scripts** →
   *schema only* → save the `.sql`. This is table/view/proc **structure**, no
   client data — safe to bring out. (A DACPAC via **Extract Data-tier
   Application** works too.)
2. Copy that file out and replace the body of `db/01-schema.sql` with it.
3. Rebuild: `powershell -ExecutionPolicy Bypass -File tools\db-reset.ps1`

`appsettings.Development.json` already points at
`Server=(localdb)\MSSQLLocalDB;Database=SsamMobileApiLocal;...`.

### Running secured endpoints without Entra ID

`appsettings.Development.json` has `"DevAuth": { "Enabled": true }`. In the
**Development** environment only, this swaps real token validation for a fake
authenticated user (`Infrastructure/DevAuthHandler.cs`), so you can call
`/api/v1/mr` with no `Authorization` header. It **cannot** activate in
Azure (the check requires `IsDevelopment()`). Set it to `false` when you get a
real token to test against.

### Scaffolding entities without local DB access

Alternatively, run the `dotnet ef dbcontext scaffold` command (section 4) **once
inside the AVD**, commit the generated `Data/` files, and pull them out. After
that the project compiles and runs against LocalDB with no need to touch the real
database again until its schema changes.

---

## 4. Entities and the existing database

`MR.cs` / `MRDetail.cs` were written by hand to match the production `[MTL]`
schema exactly. The database stays the source of truth — this API does **not**
run migrations against it.

### When the real schema changes, or to add more tables

Re-generate from the live DB instead of hand-editing. Run from `src/SsamMobileApi/`:

```bash
dotnet tool restore
dotnet ef dbcontext scaffold "Name=ConnectionStrings:SqlDb" Microsoft.EntityFrameworkCore.SqlServer \
  --context AppDbContext \
  --context-dir Data \
  --output-dir Data/Entities \
  --namespace SsamMobileApi.Data.Entities \
  --context-namespace SsamMobileApi.Data \
  --schema MTL \
  --no-onconfiguring \
  --data-annotations \
  --force
```

Notes:

- `--schema MTL` limits it to that schema; add `--table MTL.MR --table MTL.MRDetail`
  to narrow further.
- `--no-onconfiguring` keeps the connection string out of the generated code.
- `--force` overwrites previous output.
- The scaffolder pascal-cases names (`Mr`, `MrDetail`). If you want the classes
  to keep the exact table casing (`MR`, `MRDetail`), rename after generating or
  keep the current hand-written files as the reference.

---

## 5. Security model

- **Transport**: HTTPS only. HSTS in non-dev, `UseHttpsRedirection` always.
- **AuthN**: every request must carry `Authorization: Bearer <jwt>`. The JWT is
  validated against Entra ID (`Microsoft.Identity.Web` → `AddMicrosoftIdentityWebApi`):
  signature, issuer, audience, expiry.
- **AuthZ**: a global fallback policy (`Program.cs`) means endpoints are
  *deny-by-default*; add `[AllowAnonymous]` only where truly public (only
  `/health` and, in Development, `/openapi` + `/scalar`).
- **Scopes/roles**: `Program.cs` shows a `ReadAccess` policy checking the `scp`
  (delegated) or `roles` (app) claim for `Mr.Read`. Apply it with
  `[Authorize(Policy = "ReadAccess")]`.
- **SQL injection**: all data access goes through EF Core LINQ → parameterized
  SQL. No string-concatenated queries.
- **Error leakage**: `GlobalExceptionHandler` returns RFC 7807 problem+json; real
  exceptions are logged server-side only.
- **Secrets**: never in source. User secrets locally, App Service settings / Key
  Vault in Azure.

### Entra ID app registrations you need

1. **API app registration** (this service)
   - *Expose an API* → set Application ID URI `api://<clientId>` → add a scope,
     e.g. `Mr.Read`.
   - Optionally *App roles* for app-only (client-credentials) access from
     OutSystems, e.g. role value `Mr.Read`.
2. **Client app registration** (OutSystems)
   - *Certificates & secrets* → new client secret (OutSystems stores this).
   - *API permissions* → add the API's scope or app role → **grant admin
     consent**.

---

## 6. How OutSystems consumes it

1. In Service Studio: **Logic → Integrations → REST → Consume REST API**.
2. Paste the OpenAPI URL: `https://<app>.azurewebsites.net/openapi/v1.json`
   (expose it outside Development, or import the downloaded JSON file).
3. For auth, OutSystems fetches a token with the **client-credentials** flow:
   - Token endpoint: `https://login.microsoftonline.com/<tenantId>/oauth2/v2.0/token`
   - Body: `grant_type=client_credentials`,
     `client_id=<outsystems-client-id>`, `client_secret=<secret>`,
     `scope=api://<api-clientId>/.default`
   - Use an OAuth/OIDC OutSystems Forge component (or a small helper action) to
     get the token, cache it until near expiry, and set the
     `Authorization: Bearer <token>` header on each call (an `OnBeforeRequest`
     callback is the usual place).
4. Map the JSON response structures to OutSystems entities/structures.

---

## 7. Azure resources + DevOps pipeline

### Create once (Azure Portal or CLI)

- **Resource group** e.g. `rg-ssam-mobile-api`
- **App Service plan** (Linux, B1 to start; scale up later)
- **App Service (Web App)** runtime **.NET 10**
- On the Web App:
  - *Configuration → Connection strings*: `SqlDb` (type `SQLAzure`) — App Service
    injects it as `ConnectionStrings__SqlDb`.
  - *Configuration → Application settings*: `AzureAd__TenantId`,
    `AzureAd__ClientId`, `AzureAd__Audience`, `ASPNETCORE_ENVIRONMENT=Production`.
  - *Identity → System-assigned* → On. Grant this managed identity access to the
    SQL DB (recommended) or to a Key Vault holding the secrets.
  - *TLS/SSL settings*: HTTPS Only = On, min TLS 1.2.
  - *Health check*: path `/health/db` (fails over to a new instance if SQL is unreachable).
- **SQL**: allow the App Service to reach it (VNet integration + private endpoint,
  or "Allow Azure services"). Prefer **managed identity** auth over SQL logins:
  connection string `Server=tcp:...;Authentication=Active Directory Managed Identity;Encrypt=True;Database=...`.

### Pipeline (`azure-pipelines.yml`)

1. Push this repo to Azure Repos (or GitHub connected to Azure DevOps).
2. Create a **service connection** (`Azure Resource Manager`, workload identity
   federation) named `azure-svc-connection`, scoped to the resource group.
3. Create an **Environment** named `production` (add approval checks if you want
   a manual gate before deploy).
4. Edit the `variables:` block in `azure-pipelines.yml` (`azureSubscription`,
   `appServiceName`).
5. **Pipelines → New pipeline → Existing YAML file** → select
   `azure-pipelines.yml`.

Stages: restore → build → test → `dotnet publish` → upload artifact → deploy to
App Service on `main`.

---

## 8. Running locally

```bash
cd src/SsamMobileApi
dotnet run
```

Opens `https://localhost:7199/scalar` — an interactive API console. Use the
"Authorize" button with a real Entra ID token to call secured endpoints (or
leave `DevAuth:Enabled` on to skip that locally).

### Checking the database connection through the API

All endpoints are anonymous — call them from a browser, `curl`, or the `.http`
file.

| Endpoint | Purpose | Response |
| --- | --- | --- |
| `GET /health/live` | Is the process up? Checks nothing else. | `Healthy` / 200 |
| `GET /health/db` | Opens a SQL connection and runs `SELECT 1`. | JSON, 200 if OK / 503 if not |
| `GET /health` | Same as `/health/db` today (all checks). | JSON |

Healthy response:

```json
{
  "status": "Healthy",
  "checks": [
    { "name": "sql-db", "status": "Healthy", "description": "Database connection OK.",
      "data": { "server": "(localdb)\\MSSQLLocalDB", "database": "SsamMobileApiLocal", "responseMs": 12 } }
  ]
}
```

Failed response (HTTP 503) — in Development/Staging the `error` field carries the
real SQL message (`Cannot open database ...`, `Login failed ...`, timeout, etc.).
In Production only `status` is shown, to avoid leaking server internals.

The check lives in `Infrastructure/DatabaseHealthCheck.cs`. Point Azure App
Service's **Health check** setting at `/health/db` so a bad connection string or
firewall rule fails the deployment instead of serving errors.

Full step-by-step (local, AVD, and Azure): **[docs/verify-db-connection.md](docs/verify-db-connection.md)**.

### AVD with no nuget.org access

Options:
- Configure the AVD's internal NuGet feed (Azure Artifacts) and
  `dotnet restore` from there.
- Or, on a machine with internet, run
  `dotnet restore --packages ./nuget-packages` and copy the `nuget-packages`
  folder in, then `dotnet restore --source ./nuget-packages`.

---

## 9. Suggested next steps

- Add an xUnit test project (`tests/SsamMobileApi.Tests`) with
  `WebApplicationFactory<Program>` integration tests — the pipeline already runs
  `dotnet test`.
- Add `Microsoft.EntityFrameworkCore` logging redaction for PII.
- Add rate limiting (`builder.Services.AddRateLimiter(...)`) if the API is
  internet-facing.
- Move secrets to Key Vault with `Azure.Extensions.AspNetCore.Configuration.Secrets`.
