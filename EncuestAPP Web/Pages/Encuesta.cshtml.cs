using System.Security.Cryptography;
using System.Text;
using EncuestAPP_Web.Contracts;
using EncuestAPP_Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace EncuestAPP_Web.Pages;

public sealed class EncuestaModel(EncuestaPublicaApiClient apiClient) : PageModel
{
    [BindProperty]
    public List<RespuestaFormulario> Respuestas { get; set; } = [];

    [TempData]
    public string? TokenEncuestaEnviada { get; set; }

    public EncuestaPublicaDto? Encuesta { get; private set; }
    public string TokenPublico { get; private set; } = string.Empty;
    public string? ErrorGeneral { get; private set; }
    public Dictionary<int, string> ErroresPorPregunta { get; } = new();
    public IReadOnlyList<PreguntaEncuestaDto> PreguntasOrdenadas => Encuesta is null
        ? Array.Empty<PreguntaEncuestaDto>()
        : Encuesta.Preguntas.OrderBy(pregunta => pregunta.Orden).ToArray();
    public bool EncuestaEnviada { get; private set; }

    public async Task<IActionResult> OnGetAsync(string tokenPublico, CancellationToken cancellationToken)
    {
        TokenPublico = tokenPublico;
        if (string.Equals(TokenEncuestaEnviada, tokenPublico, StringComparison.Ordinal))
        {
            TokenEncuestaEnviada = null;
            EncuestaEnviada = true;
            return Page();
        }

        try
        {
            await CargarEncuestaAsync(tokenPublico, cancellationToken);
            Respuestas = Encuesta!.Preguntas
                .OrderBy(pregunta => pregunta.Orden)
                .Select(pregunta => new RespuestaFormulario { PreguntaId = pregunta.Id })
                .ToList();

            if (Encuesta.AdmiteRespuestas && Encuesta.UnaRespuestaPorDispositivo)
            {
                AsegurarCookieDispositivo();
            }
        }
        catch (EncuestaPublicaApiException exception)
        {
            ErrorGeneral = exception.Message;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string tokenPublico, CancellationToken cancellationToken)
    {
        TokenPublico = tokenPublico;

        try
        {
            await CargarEncuestaAsync(tokenPublico, cancellationToken);
        }
        catch (EncuestaPublicaApiException exception)
        {
            ErrorGeneral = exception.Message;
            return Page();
        }

        if (!Encuesta!.AdmiteRespuestas)
        {
            ErrorGeneral = "Esta encuesta no está disponible para recibir respuestas.";
            return Page();
        }

        if (!IntentarCrearSolicitud(out var participacion))
        {
            return Page();
        }

        if (Encuesta.UnaRespuestaPorDispositivo)
        {
            if (!Request.Cookies.TryGetValue(NombreCookieDispositivo(), out var identificador) || !EsIdentificadorValido(identificador))
            {
                AsegurarCookieDispositivo();
                ErrorGeneral = "El navegador debe aceptar cookies para aplicar el límite de una respuesta por dispositivo. Revisá la configuración e intentá nuevamente.";
                return Page();
            }

            participacion.IdentificadorDispositivo = identificador;
        }

        try
        {
            await apiClient.EnviarParticipacionAsync(tokenPublico, participacion, cancellationToken);
            TokenEncuestaEnviada = tokenPublico;
            return RedirectToPage("/Encuesta", new { tokenPublico });
        }
        catch (EncuestaPublicaApiException exception)
        {
            ErrorGeneral = exception.Message;
            return Page();
        }
    }

    public RespuestaFormulario ObtenerRespuesta(int preguntaId) =>
        Respuestas.FirstOrDefault(respuesta => respuesta.PreguntaId == preguntaId)
        ?? new RespuestaFormulario { PreguntaId = preguntaId };

    private async Task CargarEncuestaAsync(string tokenPublico, CancellationToken cancellationToken)
    {
        Encuesta = await apiClient.ObtenerEncuestaAsync(tokenPublico, cancellationToken);
        if (Encuesta.Preguntas is null)
        {
            throw new EncuestaPublicaApiException("La encuesta devolvió una estructura inválida.");
        }
    }

    private bool IntentarCrearSolicitud(out CrearParticipacionDto participacion)
    {
        participacion = new CrearParticipacionDto();
        Respuestas ??= [];
        ErroresPorPregunta.Clear();

        var preguntas = Encuesta!.Preguntas.OrderBy(pregunta => pregunta.Orden).ToList();
        var idsPregunta = preguntas.Select(pregunta => pregunta.Id).ToHashSet();
        if (Respuestas.Any(respuesta => !idsPregunta.Contains(respuesta.PreguntaId)))
        {
            ErrorGeneral = "El formulario cambió. Recargá la encuesta e intentá nuevamente.";
            return false;
        }

        var grupos = Respuestas.GroupBy(respuesta => respuesta.PreguntaId).ToList();
        if (grupos.Any(grupo => grupo.Count() > 1))
        {
            ErrorGeneral = "El formulario contiene respuestas duplicadas. Recargá la encuesta e intentá nuevamente.";
            return false;
        }

        var respuestas = Respuestas.ToDictionary(respuesta => respuesta.PreguntaId);
        var esValido = true;

        void AgregarError(PreguntaEncuestaDto pregunta, string mensaje)
        {
            ErroresPorPregunta[pregunta.Id] = mensaje;
            esValido = false;
        }

        foreach (var pregunta in preguntas)
        {
            respuestas.TryGetValue(pregunta.Id, out var respuesta);
            respuesta ??= new RespuestaFormulario { PreguntaId = pregunta.Id };
            respuesta.OpcionRespuestaIds ??= [];
            var texto = respuesta.Texto?.Trim();
            var opcionIds = respuesta.OpcionRespuestaIds;
            var opcionesIdsValidas = pregunta.Opciones.Select(opcion => opcion.Id).ToHashSet();

            switch (pregunta.Tipo)
            {
                case "Texto corto":
                case "Texto largo":
                    if (opcionIds.Count > 0)
                    {
                        AgregarError(pregunta, "Esta pregunta espera una respuesta de texto.");
                    }
                    else if (string.IsNullOrWhiteSpace(texto))
                    {
                        if (pregunta.Obligatoria)
                        {
                            AgregarError(pregunta, "Esta pregunta es obligatoria.");
                        }
                    }
                    else
                    {
                        participacion.Respuestas.Add(new RespuestaParticipanteDto
                        {
                            PreguntaId = pregunta.Id,
                            Texto = texto
                        });
                    }
                    break;

                case "Selección única":
                case "Escala de valoración":
                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        AgregarError(pregunta, "Esta pregunta espera una opción de respuesta.");
                    }
                    else if (opcionIds.Count == 0)
                    {
                        if (pregunta.Obligatoria)
                        {
                            AgregarError(pregunta, "Esta pregunta es obligatoria.");
                        }
                    }
                    else if (opcionIds.Count != 1 || !opcionesIdsValidas.Contains(opcionIds[0]))
                    {
                        AgregarError(pregunta, "Seleccioná una opción válida.");
                    }
                    else
                    {
                        participacion.Respuestas.Add(new RespuestaParticipanteDto
                        {
                            PreguntaId = pregunta.Id,
                            OpcionRespuestaIds = [opcionIds[0]]
                        });
                    }
                    break;

                case "Selección múltiple":
                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        AgregarError(pregunta, "Esta pregunta espera opciones de respuesta.");
                    }
                    else if (opcionIds.Count == 0)
                    {
                        if (pregunta.Obligatoria)
                        {
                            AgregarError(pregunta, "Esta pregunta es obligatoria.");
                        }
                    }
                    else if (opcionIds.Distinct().Count() != opcionIds.Count ||
                             opcionIds.Any(opcionId => !opcionesIdsValidas.Contains(opcionId)))
                    {
                        AgregarError(pregunta, "Seleccioná únicamente opciones válidas.");
                    }
                    else
                    {
                        participacion.Respuestas.Add(new RespuestaParticipanteDto
                        {
                            PreguntaId = pregunta.Id,
                            OpcionRespuestaIds = [.. opcionIds]
                        });
                    }
                    break;

                default:
                    ErrorGeneral = "La encuesta contiene un tipo de pregunta que esta página todavía no puede responder.";
                    return false;
            }
        }

        return esValido;
    }

    private void AsegurarCookieDispositivo()
    {
        if (Request.Cookies.TryGetValue(NombreCookieDispositivo(), out var existing) && EsIdentificadorValido(existing))
        {
            return;
        }

        var identificador = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(24));
        Response.Cookies.Append(NombreCookieDispositivo(), identificador, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
            MaxAge = TimeSpan.FromDays(365),
            Path = $"/encuesta/{Uri.EscapeDataString(TokenPublico)}"
        });
    }

    private string NombreCookieDispositivo()
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(TokenPublico));
        return $"encuestapp_survey_{WebEncoders.Base64UrlEncode(hash)}";
    }

    private static bool EsIdentificadorValido(string? identificador) =>
        identificador is { Length: 32 } && identificador.All(caracter =>
            char.IsAsciiLetterOrDigit(caracter) || caracter is '-' or '_');

    public sealed class RespuestaFormulario
    {
        public int PreguntaId { get; set; }
        public string? Texto { get; set; }
        public List<int> OpcionRespuestaIds { get; set; } = [];
    }
}
