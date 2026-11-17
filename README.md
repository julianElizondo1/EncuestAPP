# EncuestAPP

EncuestAPP permite crear, publicar y responder encuestas. Desde la aplicación móvil se administran encuestas, se comparten enlaces y códigos QR, y se consultan o exportan resultados en Excel y PDF. La web pública permite responder desde el navegador.

## Estructura

| Proyecto | Tecnologías y alcance |
| --- | --- |
| `EncuestAPP  API/` | ASP.NET Core Web API, Entity Framework Core, SQL Server y autenticación JWT. Incluye las migraciones. |
| `EncuestAPP Movil/` | .NET MAUI Blazor Hybrid para administrar encuestas y resultados. |
| `EncuestAPP Web/` | ASP.NET Core Razor Pages para participar en encuestas publicadas. |

`EncuestAPP.sln` reúne los tres proyectos. Las dependencias están declaradas en los archivos `.csproj`.

## Desarrollo local

Se requiere el SDK de .NET 9, los workloads de .NET MAUI para la plataforma elegida y una instancia de SQL Server para desarrollo.

1. En la API, copiá `appsettings.example.json` a `appsettings.json`. Configurá `ConnectionStrings:ConexionSQL` y `Jwt:Secret` con User Secrets en desarrollo o con las variables de entorno `ConnectionStrings__ConexionSQL` y `Jwt__Secret`. El secreto JWT debe tener al menos 32 bytes.
2. Aplicá las migraciones de Entity Framework Core a la base de desarrollo antes de usar la API.
3. Para usar la API local desde la web, configurá `Api__BaseUrl` con su URL HTTPS. La aplicación móvil usa las direcciones definidas en `Configuration/ApiConfiguration.cs` y `Configuration/PublicWebConfiguration.cs`.
