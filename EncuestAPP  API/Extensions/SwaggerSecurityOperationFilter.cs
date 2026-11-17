using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EncuestAPP__API.Extensions;

public sealed class SwaggerSecurityOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodAttributes = context.MethodInfo.GetCustomAttributes(true);
        var controllerAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? [];
        var attributes = methodAttributes.Concat(controllerAttributes).ToArray();
        var isPublic = attributes.OfType<IAllowAnonymous>().Any();
        var requiresAuthorization = attributes.OfType<IAuthorizeData>().Any();

        if (!isPublic && requiresAuthorization)
        {
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                }] = Array.Empty<string>()
            });
            operation.Description = JoinDescription(operation.Description, "Requiere autenticación JWT.");
            return;
        }

        operation.Description = JoinDescription(operation.Description, "Endpoint público; no requiere autenticación JWT.");
    }

    private static string JoinDescription(string? current, string access) =>
        string.IsNullOrWhiteSpace(current) ? access : $"{current}\n\n{access}";
}
