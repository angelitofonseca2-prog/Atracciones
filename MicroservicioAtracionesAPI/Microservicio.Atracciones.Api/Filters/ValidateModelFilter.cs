using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microservicio.Atracciones.Api.Models.Common;

namespace Microservicio.Atracciones.Api.Filters;

/// <summary>
/// Valida automáticamente el ModelState en cada request antes de que
/// llegue al controller. Si hay errores de DataAnnotations los convierte
/// a la estructura de error estándar del contrato (sección 6).
/// Se registra globalmente en Program.cs para no repetirlo por controller.
/// </summary>
public class ValidateModelFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;

        var detalles = context.ModelState
            .Where(e => e.Value?.Errors.Any() == true)
            .SelectMany(e => e.Value!.Errors.Select(err => err.ErrorMessage))
            .ToList();

        var errorResponse = new ApiErrorResponse
        {
            Status = 400,
            Error = "Parámetro inválido",
            Details = detalles,
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Path = context.HttpContext.Request.Path
        };

        context.Result = new BadRequestObjectResult(errorResponse);
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
