namespace Microservicio.Atracciones.Api.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddRoleAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("SoloAdmin", p => p.RequireRole("ADMIN"));
                options.AddPolicy("ClienteAutenticado", p => p.RequireRole("CLIENTE"));
                options.AddPolicy("AdminOCliente", p => p.RequireRole("ADMIN", "CLIENTE"));
            });

            return services;
        }
    }
}
