using Microsoft.Extensions.Logging;
using EncuestAPP_Movil.Services.Exportacion;
using EncuestAPP_Movil.Configuration;
using EncuestAPP_Movil.Handlers;
using EncuestAPP_Movil.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace EncuestAPP_Movil
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<ISesionService, SesionService>();
            builder.Services.AddSingleton<BearerTokenHandler>();
            builder.Services.AddSingleton<HttpClient>(services =>
            {
                var handler = services.GetRequiredService<BearerTokenHandler>();
                handler.InnerHandler = new HttpClientHandler();
                return new HttpClient(handler)
                {
                    BaseAddress = new Uri(ApiConfiguration.BaseAddress, UriKind.Absolute)
                };
            });
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<IEncuestaService, EncuestaService>();
            builder.Services.AddSingleton<IPreguntaService, PreguntaService>();
            builder.Services.AddSingleton<IResultadoService, ResultadoService>();
            builder.Services.AddSingleton<GeneradorExcelResultados>();
            builder.Services.AddSingleton<GeneradorPdfResultados>();
            builder.Services.AddSingleton<ServicioArchivosExportados>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, SesionAuthenticationStateProvider>();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
