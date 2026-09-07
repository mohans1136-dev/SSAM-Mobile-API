# Migrating the code into the AVD project

You already created an empty .NET project in the AVD:

```
P:\Core\SSAMMobileApp\                     solution folder
P:\Core\SSAMMobileApp\SSAMMobileApp-API\   project folder
```

and asked the admin to allow this executable to run:

```
P:\Core\SSAMMobileApp\SSAMMobileApp-API\bin\Debug\net10.0\SSAMMobileApp-API.exe
```

This repo is deliberately structured the **same way** (solution `SSAMMobileApp`,
project `SSAMMobileApp-API`, assembly `SSAMMobileApp-API`). So migrating = drop
the repo's files on top of `P:\Core\SSAMMobileApp\` and delete the leftover
template files. The exe name and path never change.

> **Check with the admin how the allow-listing works.** If it's by **path** or by
> **publisher/signing**, you're fine — rebuild as often as you like. If it's by
> **file hash**, every rebuild produces a new hash and re-breaks it; ask for a
> path-based rule on that folder instead.

---

## Step 1 — Build the transfer zip (outside the AVD)

From the repo root:

```powershell
git pull                                              # get the latest
powershell -ExecutionPolicy Bypass -File tools\bundle.ps1
```

This writes `ssam-mobile-api.zip`. Its contents sit under a top folder
`SSAMMobileApp/` that mirrors `P:\Core\SSAMMobileApp\` exactly:

```
SSAMMobileApp/
├─ SSAMMobileApp.sln
├─ SSAMMobileApp-API/            (Program.cs, Controllers/, Services/, Data/, ...)
├─ db/  docs/  tools/  .config/
├─ azure-pipelines.yml  .gitignore  README.md
```

---

## Step 2 — Copy it in and overlay (inside the AVD)

1. Copy `ssam-mobile-api.zip` into the AVD (clipboard paste into an Explorer
   window, or the AVD file-transfer feature).
2. **Close Visual Studio** (so no files are locked).
3. Extract the zip somewhere temporary, e.g. `C:\Temp\SSAMMobileApp\`.
4. Copy the **contents** of that `SSAMMobileApp\` folder into
   `P:\Core\SSAMMobileApp\`, letting it **overwrite** existing files
   (`Program.cs`, `appsettings*.json`, `.csproj`, `.sln`, `launchSettings.json`).
5. Delete the template files the repo doesn't include:

   ```powershell
   Remove-Item P:\Core\SSAMMobileApp\SSAMMobileApp-API\WeatherForecast.cs
   Remove-Item P:\Core\SSAMMobileApp\SSAMMobileApp-API\Controllers\WeatherForecastController.cs
   ```

6. If your solution file is `SSAMMobileApp.slnx` (not `.sln`), delete it and keep
   the `SSAMMobileApp.sln` from the zip — the project reference inside is the
   same.

> The repo's `.csproj` sets `<UserSecretsId>` to a fixed GUID. If you'd already
> run `dotnet user-secrets` against the old stub, those values are now under a
> different id — just set them again in Step 5.

---

## Step 3 — Restore NuGet packages

Open `P:\Core\SSAMMobileApp\SSAMMobileApp.sln` in Visual Studio.

### If the AVD can reach nuget.org

```powershell
cd P:\Core\SSAMMobileApp
dotnet restore
dotnet tool restore        # the `dotnet ef` CLI, for Step 6
```

### If it can't (the usual case — see the error below)

Visual Studio shows package-restore errors, or a prompt to install components it
can't download. The fix is to carry the packages in as files.

**First, check whether a feed is reachable** (any of these avoids carrying files):

| Try from the AVD browser | If it works |
| --- | --- |
| `https://dev.azure.com/<your-org>` | Create an **Azure Artifacts** feed with **nuget.org as an upstream source**, then add its URL to `nuget.config`. Restore pulls through it. |
| An internal company NuGet URL (ask IT) | Put that URL in `nuget.config` as the only `<packageSource>`. |
| Nothing | Use the offline bundle below. |

**Offline bundle (always works):**

1. **Outside the AVD**, from the repo root:

   ```powershell
   powershell -ExecutionPolicy Bypass -File tools\pack-offline-nuget.ps1
   ```

   Produces `offline-nuget.zip` (~265 MB) — every package the solution needs,
   plus a `nuget.config`.

2. Copy `offline-nuget.zip` into the AVD and **extract it into
   `P:\Core\SSAMMobileApp\`**. You now have:

   ```
   P:\Core\SSAMMobileApp\_offline-nuget\    (all packages)
   P:\Core\SSAMMobileApp\nuget.config       (points restore at that folder, no network)
   ```

3. Restore — now fully offline:

   ```powershell
   cd P:\Core\SSAMMobileApp
   dotnet restore
   dotnet tool restore
   ```

   In Visual Studio: **Tools → NuGet Package Manager → Package Manager Settings →
   Clear All NuGet Cache(s)** is *not* needed; just **Build → Rebuild Solution**.

> The bundled `nuget.config` clears all online sources. When you later get feed
> access, replace it with a `<packageSources>` entry for that feed.

> The `.NET desktop development` workload the VS Installer offers is **not**
> needed for this Web API (that's for WPF/WinForms). What you need is the
> **.NET 10 SDK** — if the installer is trying to fetch that and can't, ask IT to
> install "ASP.NET and web development" + the .NET 10 SDK from an approved source.

---

## Step 4 — Build and confirm the exe path

```powershell
dotnet build
```

Expect `Build succeeded`, then confirm the whitelisted file exists:

```powershell
Test-Path P:\Core\SSAMMobileApp\SSAMMobileApp-API\bin\Debug\net10.0\SSAMMobileApp-API.exe
```

Must print `True`. (Visual Studio's default F5 build produces the same path.)

---

## Step 5 — Point at the real database

Never put the real connection string in a file. Use user secrets — from
`P:\Core\SSAMMobileApp\SSAMMobileApp-API\`:

```powershell
dotnet user-secrets set "ConnectionStrings:SqlDb" "Server=REAL_SERVER;Database=SIGSSAMI_OS;Encrypt=True;TrustServerCertificate=True;Authentication=Active Directory Integrated"
```

Use `User Id=...;Password=...` instead of `Authentication=...` for a SQL login.
Ask the DBA for the exact string.

User secrets override `appsettings.Development.json`, so the LocalDB string there
is ignored once this is set.

Leave `"DevAuth": { "Enabled": true }` in `appsettings.Development.json` **on**
for now — it lets you call secured endpoints without an Entra token while wiring
up. It can never activate outside the Development environment. Set it `false`
once you have a real token to test.

---

## Step 6 — Verify the entities match the real schema

`Data/Entities/MR.cs` and `MRDetail.cs` were built from screenshots. Confirm
against the live DB by scaffolding into a throwaway folder and diffing:

```powershell
cd P:\Core\SSAMMobileApp\SSAMMobileApp-API
dotnet ef dbcontext scaffold "Name=ConnectionStrings:SqlDb" Microsoft.EntityFrameworkCore.SqlServer `
  --schema MTL `
  --output-dir Data/_ScaffoldCheck --context-dir Data/_ScaffoldCheck `
  --context ScaffoldCheckContext `
  --namespace SSAMMobileApp.Data._ScaffoldCheck `
  --context-namespace SSAMMobileApp.Data._ScaffoldCheck `
  --data-annotations --force
```

Compare the generated `MR` / `MRDetail` with the hand-written ones — column
names, types, nullability, identity, the FK. Fix the hand-written files if
anything differs (keep the exact names, including `OrderRecjectedBy`). Then
**delete `Data/_ScaffoldCheck/`** — it's a diff aid, not part of the build.

---

## Step 7 — Run and verify the DB connection

```powershell
dotnet run --project P:\Core\SSAMMobileApp\SSAMMobileApp-API
```

Then, in a browser or a second terminal:

- `http://localhost:5199/health/db` → `"status": "Healthy"` means the API
  reached `SIGSSAMI_OS`. If `Unhealthy`, the `error` field says why
  (bad server, login, firewall) — see `docs/verify-db-connection.md`.
- `http://localhost:5199/scalar` → interactive console; try `GET /api/v1/mr`.
- `http://localhost:5199/api/v1/mr/{id}` → a real MR with its detail lines.

---

## Step 8 — Get changes back out of the AVD

If the AVD **can** reach github.com:

```powershell
cd P:\Core\SSAMMobileApp
git init
git remote add origin https://github.com/mohans1136-dev/SSAM-Mobile-API.git
git fetch origin
git reset --soft origin/main          # adopt history without touching your files
git add -A && git commit -m "..." && git push
```

If it **can't**: run `powershell -ExecutionPolicy Bypass -File tools\bundle.ps1`
inside the AVD, carry `ssam-mobile-api.zip` back out, extract over your local
repo, then commit and push from outside.

---

## What NOT to carry across

- `bin/`, `obj/` — rebuilt on each side (already excluded from the zip).
- User secrets — they live in `%APPDATA%\Microsoft\UserSecrets\`; set them fresh
  in each environment.
- Any real connection string, token, or client data — those stay in the AVD only.
