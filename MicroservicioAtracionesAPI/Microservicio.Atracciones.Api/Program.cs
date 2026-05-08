using Microservicio.Atracciones.Api.Extensions;
using Microservicio.Atracciones.Api.Filters;
using Microservicio.Atracciones.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCachingConfig(builder.Configuration);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy =
        System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
    options.JsonSerializerOptions.DefaultIgnoreCondition =
        System.Text.Json.Serialization.JsonIgnoreCondition.Never;
});

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddRoleAuthorization();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddApiVersioningConfig();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Atracciones v2.0");
    c.RoutePrefix = string.Empty;
});

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(builder.Configuration
    .GetSection("CorsSettings:PolicyName").Value ?? "AtraccionesPolicy");
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();