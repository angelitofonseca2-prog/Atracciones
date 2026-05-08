using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.Business.Interfaces.Auth;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microservicio.Atracciones.Business.Rules.Admin;
using Microservicio.Atracciones.Business.Rules.Public;
using Microservicio.Atracciones.Business.Services.Admin;
using Microservicio.Atracciones.Business.Services.Auth;
using Microservicio.Atracciones.Business.Services.Public;
using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataAccess.Queries;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microservicio.Atracciones.Api.Helpers;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Api.Models.Settings;
using Microservicio.Atracciones.Api.Services;
using System.Text.Json;

namespace Microservicio.Atracciones.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services, IConfiguration config)
        {
            // ── Base de datos ──────────────────────────────────────────
            services.AddDbContext<AtraccionesDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("AtraccionesDb")));

            // ── Settings ───────────────────────────────────────────────
            services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
            services.Configure<CorsSettings>(config.GetSection("CorsSettings"));
            services.Configure<CacheSettings>(config.GetSection("CacheSettings"));
            services.Configure<ApiSettings>(config.GetSection("ApiSettings"));

            // ── DataAccess — QueryRepositories (Scoped) ────────────────
            services.AddScoped<AtraccionQueryRepository>();
            services.AddScoped<ReservaQueryRepository>();
            services.AddScoped<TicketQueryRepository>();
            services.AddScoped<FacturaQueryRepository>();

            // ── DataManagement — UnitOfWork y DataServices ─────────────
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUsuarioDataService, UsuarioDataService>();
            services.AddScoped<IClienteDataService, ClienteDataService>();
            services.AddScoped<IDestinoDataService, DestinoDataService>();
            services.AddScoped<IAtraccionDataService, AtraccionDataService>();
            services.AddScoped<ITicketDataService, TicketDataService>();
            services.AddScoped<IReservaDataService, ReservaDataService>();
            services.AddScoped<IFacturaDataService, FacturaDataService>();
            services.AddScoped<IReseniaDataService, ReseniaDataService>();
            services.AddScoped<IAuditoriaLogDataService, AuditoriaLogDataService>();
            services.AddScoped<ICategoriaDataService, CategoriaDataService>();
            services.AddScoped<IIdiomaDataService, IdiomaDataService>();
            services.AddScoped<IIncluyeDataService, IncluyeDataService>();
            services.AddScoped<IImagenDataService, ImagenDataService>();

            // ── Business — Infraestructura (viven en API) ──────────────
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

            // ── Business — Rules ───────────────────────────────────────
            services.AddScoped<ReservaRules>();
            services.AddScoped<ReseniaRules>();
            services.AddScoped<AtraccionRules>();
            services.AddScoped<TicketRules>();
            services.AddScoped<ReservaAdminRules>();
            services.AddScoped<FacturaRules>();

            // ── Business — Auth ────────────────────────────────────────
            services.AddScoped<IAuthService, AuthService>();

            // ── Business — Public Services ─────────────────────────────
            services.AddScoped<IAtraccionPublicService, AtraccionPublicService>();
            services.AddScoped<IReservaPublicService, ReservaPublicService>();
            services.AddScoped<IReseniaPublicService, ReseniaPublicService>();
            services.AddScoped<IClientePerfilService, ClientePerfilService>();
            services.AddScoped<IFacturaPublicService, FacturaPublicService>();

            // ── Business — Admin Services ──────────────────────────────
            services.AddScoped<IUsuarioAdminService, UsuarioAdminService>();
            services.AddScoped<IClienteAdminService, ClienteAdminService>();
            services.AddScoped<IDestinoAdminService, DestinoAdminService>();
            services.AddScoped<ICatalogoAdminService, CatalogoAdminService>();
            services.AddScoped<IAtraccionAdminService, AtraccionAdminService>();
            services.AddScoped<ITicketAdminService, TicketAdminService>();
            services.AddScoped<IReservaAdminService, ReservaAdminService>();
            services.AddScoped<IFacturaAdminService, FacturaAdminService>();
            services.AddScoped<IReseniaAdminService, ReseniaAdminService>();
            services.AddScoped<IImagenAdminService, ImagenAdminService>();

            // ── API — Token Service ────────────────────────────────────
            services.AddScoped<TokenService, JwtTokenService>();

            return services;
        }
    }
}
