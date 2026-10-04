# Hosting a public demo

This guide shows how to host the API so anyone can open the storefront (`/store/`) and Swagger (`/swagger`) in a browser. It uses free tiers, and each part can be swapped for an equivalent service.

> **Status:** these steps are a checklist, not a tested deployment. The app has been run against SQL Server and Redis locally and in CI, but it has not been deployed to the services below yet.

## What the app needs

| Part | Free-tier option | Notes |
|---|---|---|
| Web host for ASP.NET Core 8 | Azure App Service (F1), Render | Any host that runs .NET 8 works |
| SQL Server database(s) | Azure SQL Database free offer | Two databases: store and identity |
| Redis | Upstash Redis (free) | Used for baskets only |

The app creates its tables and seeds products on first start, so the database login needs permission to create tables.

## Settings

Set these as environment variables (or "Application settings") on the host. Double underscores map to the `:` in `appsettings.json`. **Never commit real values.**

| Setting | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__StoreContext` | Azure SQL connection string for the store database |
| `ConnectionStrings__IdentityContext` | Azure SQL connection string for the identity database |
| `ConnectionStrings__Redis` | e.g. `<host>:6379,password=<password>,ssl=True,abortConnect=False` |
| `JwtSettings__Key` | A new random secret of 64+ characters (do not reuse your local one) |
| `Urls__ApiBaseUrl` | The public URL of the site, e.g. `https://talabat-demo.example.com` (used to build product image URLs) |
| `Swagger__Enabled` | `true` to expose `/swagger` in Production |

Leave `Seed__DemoUserPassword` unset. Visitors can register their own account from Swagger, Postman or the API.

## Steps

1. **Create the databases.** Create two empty databases (for example `talabat-store` and `talabat-identity`) and note their connection strings.
2. **Create a Redis database** and copy its host and password.
3. **Create the web app** (.NET 8, Linux) and add the settings above.
4. **Publish and deploy.**
   ```bash
   dotnet publish LinkDev.Talabat.APIs -c Release -o publish
   ```
   Deploy the `publish` folder with your host's zip-deploy or GitHub deployment option. The seed data and product images are included in the publish output.
5. **Open it.** Visit `/store/` for the storefront and `/swagger` for the interactive API docs. The first request can take a few seconds while the app migrates and seeds the databases.
6. **Point Postman at it.** Change the `baseUrl` variable in the Postman environment to your site URL (see the README).

## Before sharing the link

- Use throw-away databases. Anyone can register users and create orders on a public demo.
- Free tiers sleep when idle, so the first visit may be slow.
- There is no rate limiting. Keep the demo out of anything you care about.
- Rotate `JwtSettings__Key` if it is ever exposed.
