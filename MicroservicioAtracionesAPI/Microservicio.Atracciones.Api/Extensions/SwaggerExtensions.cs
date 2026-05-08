using Microsoft.OpenApi.Models;

namespace Microservicio.Atracciones.Api.Extensions;

/// <summary>
/// E-02: Swagger con soporte JWT Bearer.
/// Agrega el botón "Authorize" en la UI para poder pegar el token
/// obtenido del POST /api/v1/admin/auth/login y probrar
/// todos los endpoints protegidos con [Authorize].
/// </summary>
public static class SwaggerExtensions
{
    private const string BearerScheme = "Bearer";

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API de Atracciones - Booking Prototipo",
                Version = "v2.0.0",
                Description = "Microservicio de atracciones turísticas. Contrato v2.0.\n\n" +
                              "**Autenticación:** Haz POST en `/api/v1/admin/auth/login`, " +
                              "copia el campo `token` del response y pégalo aquí con el prefijo `Bearer `."
            });

            // E-02: Definición del esquema de seguridad JWT Bearer
            c.AddSecurityDefinition(BearerScheme, new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = "bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Description  = "Pega el token JWT obtenido del login. **No** incluyas el prefijo 'Bearer' — Swagger lo añade automáticamente."
            });

            // E-02: Aplicar candadito y Bearer sA3lo en endpoints que tengan [Authorize]
            c.OperationFilter<AuthorizeOperationFilter>();
        });

        return services;
    }
}