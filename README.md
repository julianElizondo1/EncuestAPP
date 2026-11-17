# EncuestAPP

EncuestAPP centraliza la creación, publicación y análisis de encuestas. La administración se realiza desde una aplicación móvil y la participación se realiza desde el navegador mediante un enlace público.

## Funcionalidades

- **Administrador:** iniciar sesión, crear encuestas en borrador, configurar preguntas y opciones, publicar, cerrar, archivar y crear nuevas versiones. Puede compartir encuestas publicadas mediante un enlace o código QR, consultar resultados y exportarlos a Excel y PDF.
- **Participante:** abrir el enlace de una encuesta publicada, responder preguntas de texto, selección o valoración y enviar las respuestas desde el navegador sin iniciar sesión.

## Arquitectura y tecnologías

`EncuestAPP.sln` reúne tres proyectos basados en .NET 9. Las dependencias se declaran en sus archivos `.csproj`.

| Proyecto | Tecnologías y responsabilidad |
| --- | --- |
| `EncuestAPP  API/` | ASP.NET Core Web API, Entity Framework Core y SQL Server. Centraliza las reglas de negocio, la persistencia y la autenticación JWT. Incluye las migraciones. |
| `EncuestAPP Movil/` | .NET MAUI Blazor Hybrid para administrar encuestas y consultar o exportar resultados. |
| `EncuestAPP Web/` | ASP.NET Core Razor Pages para mostrar encuestas publicadas y recibir respuestas. |

La aplicación móvil consume la API mediante HTTPS y JSON, con JWT para las operaciones privadas. El navegador envía el formulario a la web pública, cuyo servidor consulta la encuesta y remite las respuestas a la API mediante HTTPS y JSON. La API valida las operaciones y accede a SQL Server mediante Entity Framework Core.

## Despliegue en monsterASP

| Servicio | Dirección |
| --- | --- |
| Web pública | [encuestapp.runasp.net](https://encuestapp.runasp.net/) |
| API | [encuestapi.runasp.net](https://encuestapi.runasp.net/) |
| Endpoint de ejemplo | [GET /api/catalogos/estados](https://encuestapi.runasp.net/api/catalogos/estados) |

Para abrir una encuesta se necesita su enlace completo, con el formato `/encuesta/{tokenPublico}`. El formulario admite respuestas mientras la encuesta está publicada.

## Validación funcional

- Se realizó una validación con 300 participaciones sintéticas para revisar los resultados de una encuesta.
- Se verificaron la generación, el guardado y la visualización de los informes en Excel y PDF en un dispositivo físico.

La ejecución de la aplicación en dispositivos se demostrará durante la presentación académica.

## Desarrollo local

Se requiere el SDK de .NET 9, una instancia de SQL Server para desarrollo y los workloads de .NET MAUI de la plataforma elegida. Para aplicar las migraciones desde la terminal, se necesita la herramienta `dotnet-ef` compatible con EF Core 9.

1. En la API, usá `appsettings.example.json` como plantilla para `appsettings.json`. Configurá `ConnectionStrings:ConexionSQL` y `Jwt:Secret` con User Secrets o con las variables de entorno `ConnectionStrings__ConexionSQL` y `Jwt__Secret`. El secreto JWT debe tener al menos 32 bytes.
2. Desde la raíz del repositorio, aplicá las migraciones a la base de desarrollo y ejecutá la API con el perfil HTTPS.

   ```powershell
   dotnet ef database update --project "EncuestAPP  API/EncuestAPP  API.csproj"
   dotnet run --project "EncuestAPP  API/EncuestAPP  API.csproj" --launch-profile https
   ```

3. En otra terminal, configurá `Api__BaseUrl` con la URL HTTPS de la API e iniciá la web pública. El ejemplo usa los puertos de los perfiles locales del repositorio.

   ```powershell
   $env:Api__BaseUrl = "https://localhost:7288/"
   dotnet run --project "EncuestAPP Web/EncuestAPP Web.csproj" --launch-profile https
   ```

   Abrí la web en `https://localhost:7015/`. Si el certificado HTTPS de desarrollo todavía no es de confianza, usá `dotnet dev-certs https --trust`.

4. Para ejecutar la aplicación móvil, abrí la solución en un IDE compatible con .NET MAUI y seleccioná el proyecto móvil y la plataforma de destino. Revisá las direcciones de `Configuration/ApiConfiguration.cs` y `Configuration/PublicWebConfiguration.cs`. Para usar servicios locales desde un dispositivo físico, las direcciones deben ser accesibles desde ese dispositivo y sus certificados HTTPS deben ser de confianza.
