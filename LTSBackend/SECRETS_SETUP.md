# Secrets Setup

`appsettings.json` no longer contains real credentials — only `CHANGE_ME_...`
placeholders. This is intentional: **appsettings.json is committed to source
control and must never hold real secrets.**

Two secrets need to be supplied outside of appsettings.json:

| Setting | Purpose | Old value (now rotated/placeholder) |
|---|---|---|
| `JwtSettings:SecretKey` | Signs/validates all JWT access tokens | was a weak, guessable string |
| `EmailSettings:SenderEmail` / `EmailSettings:AppPassword` | Sends OTP + password-reset emails via Gmail SMTP | was a real Gmail account + App Password |

> **If this repo/zip has ever been shared, committed, or uploaded anywhere:**
> the Gmail App Password that used to be in `appsettings.json` is
> compromised. Revoke it now at
> https://myaccount.google.com/apppasswords and generate a new one — this
> is independent of anything below.

## How configuration resolution works

ASP.NET Core automatically layers configuration sources, environment
variables take priority over `appsettings.json`. Nested keys use `__`
(double underscore) as the section separator:

```
JwtSettings:SecretKey       →  env var  JwtSettings__SecretKey
EmailSettings:AppPassword   →  env var  EmailSettings__AppPassword
```

The app **fails to start** if `JwtSettings:SecretKey` is missing, still the
placeholder, or shorter than 32 bytes (see `Program.cs`). It only **logs a
warning** (doesn't crash) if the email settings are unconfigured, since
email isn't required for the API's core security to function.

## Local development

Preferred: `dotnet user-secrets` (keeps secrets out of any file in the repo,
stored instead under your user profile).

```bash
cd LTSBackend
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "$(openssl rand -base64 48)"
dotnet user-secrets set "EmailSettings:SenderEmail" "your-real-sender@gmail.com"
dotnet user-secrets set "EmailSettings:AppPassword" "your-new-gmail-app-password"
```

On Windows PowerShell, generate the JWT secret with:
```powershell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }))
```

## Production / staging

Set the same two environment variables on whatever is hosting the API
(App Service configuration blade, container env vars, systemd
`EnvironmentFile`, CI/CD secret store, etc.):

```
JwtSettings__SecretKey=<64+ char random string>
EmailSettings__SenderEmail=<sender address>
EmailSettings__AppPassword=<gmail app password>
```

For anything beyond a single small deployment, prefer a real secret
manager (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault) over plain
environment variables, and rotate `JwtSettings:SecretKey` periodically —
note that rotating it invalidates every currently-issued access token.
