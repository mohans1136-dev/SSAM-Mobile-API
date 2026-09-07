# Migrating the project into the AVD

Goal: get this code into the Azure Virtual Desktop, open it in Visual Studio,
point it at the **real** `[SIGSSAMI_OS]` database, build, run, and verify — then
keep committing from inside the AVD.

Pick **one** transfer method (A or B), then do the common steps.

---

## A. Git clone (preferred — if the AVD can reach GitHub)

Inside the AVD, in a terminal / Developer PowerShell:

```powershell
cd C:\Source                      # wherever you keep repos
git clone https://github.com/mohans1136-dev/SSAM-Mobile-API.git
cd SSAM-Mobile-API
```

You now have full history and can `git pull` / `git push` normally. Done —
skip to **Common steps**.

---

## B. File copy (AVD is locked down / no GitHub)

1. **Outside the AVD**, from the repo root:

   ```powershell
   powershell -ExecutionPolicy Bypass -File tools\bundle.ps1
   ```

   Writes `ssam-mobile-api.zip` (~38 KB) containing only committed files, with
   the correct folder structure, under a top folder `SSAM-Mobile-API/`.

2. Copy `ssam-mobile-api.zip` into the AVD (clipboard paste into an Explorer
   window, or the AVD's file-transfer feature).

3. **Inside the AVD**, extract it, e.g. to `C:\Source\SSAM-Mobile-API`.

> Want git history too? Instead of the zip, copy the **entire project folder
> including its hidden `.git` directory** (select the folder in Explorer,
> Ctrl+C, paste). Then `git remote -v` still works and you can push.
>
> If you only copied the zip, you can still connect it to GitHub later inside
> the AVD:
> ```powershell
> git init
> git remote add origin https://github.com/mohans1136-dev/SSAM-Mobile-API.git
> git fetch origin
> git reset --soft origin/main
> ```

---

## Common steps (after A or B)

### 1. Open in Visual Studio

Open `SsamMobileApi.slnx` (VS 2022 17.10+) or the folder in VS Code / Rider.

### 2. Restore packages

```powershell
dotnet restore
```

or right-click the solution → **Restore NuGet Packages**. Every version is
pinned in `src/SsamMobileApi/SsamMobileApi.csproj`, so you get exactly what was
tested. If the AVD has no nuget.org access, see README section 8.

### 3. Restore the EF CLI tool (for later scaffolding)

```powershell
dotnet tool restore
```

### 4. Point at the real database

Never put the real connection string in a file. Use user secrets — from
`src\SsamMobileApi\`:

```powershell
dotnet user-secrets set "ConnectionStrings:SqlDb" "Server=REAL_SERVER;Database=SIGSSAMI_OS;Encrypt=True;TrustServerCertificate=True;Authentication=Active Directory Integrated"
```

(Use `User Id=...;Password=...` instead of `Authentication=...` if it's a SQL
login. Ask the DBA for the exact string.)

User secrets override `appsettings.Development.json`, so the LocalDB string
there is ignored once this is set.

### 5. Decide on auth for local running

`appsettings.Development.json` has `"DevAuth": { "Enabled": true }` — leave it
**on** to run without an Entra token while you wire things up. Set it to
`false` once you have a real token and want to test the real security path.

(It can never activate outside the Development environment, so it's safe to
leave on locally.)

### 6. Build

```powershell
dotnet build
```

Expected: `Build succeeded`.

### 7. Verify the real schema matches the entities

The entities in `Data/Entities/MR.cs` and `MRDetail.cs` were built from
screenshots. Confirm they match the live DB:

```powershell
dotnet ef dbcontext scaffold "Name=ConnectionStrings:SqlDb" Microsoft.EntityFrameworkCore.SqlServer `
  --schema MTL --table MTL.MR --table MTL.MRDetail `
  --output-dir Data/_ScaffoldCheck --context ScaffoldCheckContext --context-dir Data/_ScaffoldCheck `
  --namespace SsamMobileApi.Data._ScaffoldCheck --context-namespace SsamMobileApi.Data._ScaffoldCheck `
  --data-annotations --force
```

Compare the generated classes under `Data/_ScaffoldCheck/` with the hand-written
`MR.cs` / `MRDetail.cs` (column names, types, nullability, identity, FK). Fix the
hand-written ones if anything differs, then **delete `Data/_ScaffoldCheck/`** —
it's only a diff aid, not part of the build.

### 8. Run

```powershell
dotnet run --project src\SsamMobileApi
```

Then open:

- `http://localhost:5199/health/db` → `"status": "Healthy"` means the API
  reached `SIGSSAMI_OS`.
- `http://localhost:5199/scalar` → try `GET /api/v1/mr` against real data.
- `http://localhost:5199/api/v1/mr/{id}` → a real MR with its detail lines.

If `/health/db` is `Unhealthy`, the `error` field says why (bad server name,
login failure, firewall). See `docs/verify-db-connection.md`.

### 9. Commit from inside the AVD

```powershell
git add -A
git commit -m "..."
git push        # if the AVD can reach GitHub
```

If the AVD can't push, run `tools\bundle.ps1` inside the AVD and carry the zip
back out, or copy the changed files back.

---

## What NOT to bring across

- `bin/`, `obj/` — rebuilt locally (already excluded from the zip).
- User secrets — they live in `%APPDATA%\Microsoft\UserSecrets\`, set them fresh
  in each environment.
- Any real connection string, token, or client data — keep those in the AVD only.
