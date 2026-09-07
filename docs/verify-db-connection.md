# Verifying the database connection through the API — step by step

The API exposes health endpoints that actually open a SQL connection and run
`SELECT 1`. You never need SSMS or a separate tool to know whether the API can
reach its database.

| Endpoint | Checks | Auth |
| --- | --- | --- |
| `GET /health/live` | process is running | none |
| `GET /health/db` | **SQL connection + `SELECT 1`** | none |
| `GET /health` | every registered check | none |

---

## A. Locally, outside the AVD (LocalDB)

### 1. Build the local database (first time / after schema changes)

```powershell
powershell -ExecutionPolicy Bypass -File tools\db-reset.ps1
```

Expected tail:

```
Done. Connection string:
  Server=(localdb)\MSSQLLocalDB;Database=SsamMobileApiLocal;Trusted_Connection=True;TrustServerCertificate=True;
```

### 2. Confirm the connection string the API will use

Open `src/SsamMobileApi/appsettings.Development.json` and check:

```json
"ConnectionStrings": {
  "SqlDb": "Server=(localdb)\\MSSQLLocalDB;Database=SsamMobileApiLocal;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Start the API

```powershell
cd src\SsamMobileApi
dotnet run
```

Watch the console for the port:

```
Now listening on: http://localhost:5199
DevAuth is ENABLED - all requests run as a fake authenticated user.
```

### 4. Call the DB health endpoint

In a second terminal:

```powershell
curl http://localhost:5199/health/db
```

or open `http://localhost:5199/health/db` in a browser, or use
`src/SsamMobileApi/SsamMobileApi.http` in Visual Studio / VS Code ("Send
Request" above the `/health/db` line).

### 5. Read the result

**Success — HTTP 200:**

```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "sql-db",
      "status": "Healthy",
      "description": "Database connection OK.",
      "data": { "server": "(localdb)\\MSSQLLocalDB", "database": "SsamMobileApiLocal", "responseMs": 12 }
    }
  ]
}
```

**Failure — HTTP 503:** `status` is `Unhealthy` and `error` holds the real
reason, e.g.:

- `A network-related or instance-specific error occurred...` → LocalDB not
  started. Run `sqllocaldb start MSSQLLocalDB`.
- `Cannot open database "..." requested by the login` → database name wrong or
  not created. Re-run `tools\db-reset.ps1`.
- `Login failed for user '...'` → auth part of the connection string is wrong.

### 6. (Optional) prove it also serves data

```powershell
curl "http://localhost:5199/api/v1/mr?pageSize=3"
curl "http://localhost:5199/api/v1/mr/1"
```

`DevAuth` lets these through with no token. You should get the seeded MR rows
and, for `/mr/1`, its MRDetail lines.

---

## B. Inside the AVD, against the real SQL Server

### 1. Put the real connection string in user secrets (not in any file)

```powershell
cd src\SsamMobileApi
dotnet user-secrets set "ConnectionStrings:SqlDb" "Server=REAL_SERVER;Database=REAL_DB;User Id=...;Password=...;Encrypt=True;TrustServerCertificate=True"
```

If the real server uses Windows/Entra auth instead, use
`Authentication=Active Directory Integrated` (or `...Default`) and no
`User Id`/`Password`.

### 2. Turn DevAuth off so you test the real security path (optional)

`appsettings.Development.json` → `"DevAuth": { "Enabled": false }`. You'll then
need a real bearer token to call `/api/v1/*`, but `/health/db` stays anonymous.

### 3. Run and check

```powershell
dotnet run
curl http://localhost:5199/health/db
```

Same success/failure shapes as section A. Because you're in Development, the
`error` field still shows the full SQL message — useful while wiring things up.

---

## C. In Azure, after deployment

### 1. Set configuration on the App Service (once)

Portal → your Web App → **Settings → Environment variables**:

| Name | Value |
| --- | --- |
| `ConnectionStrings__SqlDb` | the production connection string (or a Key Vault reference) |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

(Double underscore `__` maps to the `:` in `ConnectionStrings:SqlDb`.)

### 2. Wire the platform health probe

Portal → Web App → **Monitoring → Health check** → enable, path `/health/db`.
Azure now polls it; an instance that can't reach SQL is pulled out of rotation.

### 3. Check it yourself

```powershell
curl https://<your-app>.azurewebsites.net/health/db
```

- **200 / `Healthy`** → the deployed API can reach the database.
- **503 / `Unhealthy`** → it can't. In Production the body shows only
  `status`; get the detail from **Log stream** (Portal → Monitoring → Log
  stream) or Application Insights — the exception is logged there by
  `DatabaseHealthCheck`.

Common Azure causes of 503 here:

- SQL firewall doesn't allow the App Service (enable "Allow Azure services", or
  use a private endpoint / VNet integration).
- Connection string points at the wrong server or database.
- Using managed identity but the identity has no login/user in the SQL database.

---

## How it works (for reference)

`src/SsamMobileApi/Infrastructure/DatabaseHealthCheck.cs` is registered in
`Program.cs`:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("sql-db", tags: ["db", "ready"]);
```

and mapped (anonymous) as `/health`, `/health/db`, `/health/live`. The JSON
writer in `Infrastructure/HealthCheckResponse.cs` hides `error`/`data` when
`ASPNETCORE_ENVIRONMENT=Production`.
