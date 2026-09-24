using Transporte.Application.Interfaces;
using Transporte.Application.Interfaces.Repositories;
using Transporte.Application.Services;
using Transporte.Domain.Entities;
using Transporte.Domain.Validation;
using Transporte.Infrastructure.Persistence;
using Transporte.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Los estados (Pendiente/EnTransito/Entregado) se serializan como texto, no como número:
// así Facturación (y Postman) los lee legibles al consultar GET /envios/{id}.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Persistencia — una base de datos propia de Transporte. En local usa SQLite;
// en Azure, la cadena de conexión de Azure SQL (con "Server=") activa UseSqlServer.
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=transporte.db";
var esAzureSql = connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<TransporteDbContext>(options =>
{
    if (esAzureSql)
        options.UseSqlServer(connectionString);
    else
        options.UseSqlite(connectionString);
});

builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TransporteDbContext>());

// Repositorios (Infrastructure -> Application.Interfaces)
builder.Services.AddScoped<IRutaRepository, RutaRepository>();
builder.Services.AddScoped<IEnvioRepository, EnvioRepository>();

// Validadores de dominio (puros, sin dependencias de infraestructura)
builder.Services.AddScoped<IEnvioEstadoValidator, EnvioEstadoValidator>();

// Servicios de aplicación
builder.Services.AddScoped<RutaService>();
builder.Services.AddScoped<EnvioService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TransporteDbContext>();
    db.Database.Migrate();
    await SeedData.EjecutarAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

static class SeedData
{
    public static async Task EjecutarAsync(TransporteDbContext db)
    {
        if (await db.Rutas.AnyAsync()) return;

        // BodegaOrigenId/BodegaDestinoId son referencias lógicas al servicio Inventario
        // (1 = Bodega Central, 2 = Bodega Norte en el seed de Inventario).
        db.Rutas.AddRange(
            new Ruta { BodegaOrigenId = 1, BodegaDestinoId = 2, DistanciaKm = 240.50m, Activa = true },
            new Ruta { BodegaOrigenId = 2, BodegaDestinoId = 1, DistanciaKm = 240.50m, Activa = true }
        );

        await db.SaveChangesAsync();
    }
}
