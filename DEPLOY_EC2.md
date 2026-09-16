# EC2 Docker deployment

## What was fixed

The backend now:

1. waits for SQL Server to accept connections;
2. connects to `master` first, so a missing `LitigationDb` does not cause SQL Server error 4060;
3. creates `LitigationDb` when necessary;
4. applies all pending EF Core migrations before hosted background services start;
5. uses the Docker service name `db` instead of a developer-PC SQL Server hostname.

The Compose file also uses a SQL Server health check, so the backend starts only after SQL Server reports healthy.

The frontend's production API URL is set to `http://backend:8080`, which is the correct Docker-to-Docker address for this Blazor Server application.

## Deploy

Create the environment file:

```bash
cp .env.example .env
nano .env
```

Set at least:

- `FRONTEND_ORIGIN` to the URL users will open in their browser.
- `MSSQL_SA_PASSWORD` to a strong SQL Server password.
- `JWT_SECRET` to a long random secret.
- `SMTP_SENDER_EMAIL` and `SMTP_APP_PASSWORD` to your SMTP credentials.

Then run:

```bash
docker compose down
mkdir -p mssql_data
docker compose build --no-cache
docker compose up -d
```

Check status and logs:

```bash
docker compose ps
docker compose logs -f db backend
```

The backend should log that SQL Server is ready, the database is initialized, and the API is starting.

## Existing database

Do **not** delete `mssql_data` if it contains data you need. The EF migration initializer will update an existing database to the latest migration.

For a brand-new deployment, an empty `mssql_data` directory is fine: `LitigationDb` will be created automatically.

## Important security note

The original project contained credentials/secrets in `appsettings.json`. The corrected project removes those hard-coded production secrets and reads them from environment variables instead. Any credentials that were previously committed to source control should still be rotated.

Do not commit `.env` to Git. Only commit `.env.example`.
