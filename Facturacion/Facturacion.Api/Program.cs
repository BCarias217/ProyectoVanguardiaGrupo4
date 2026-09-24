using Facturacion.Application.Interfaces;
using Facturacion.Application.Interfaces.Repositories;
using Facturacion.Application.Services;
using Facturacion.Domain.Entities;
using Facturacion.Infrastructure.Clients;
using Facturacion.Infrastructure.Persistence;
using Facturacion.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Persistencia — una base de datos propia de Facturación. En local usa SQLite;
// en Azure, la cadena de conexión de Azure SQL (con "Server=") activa UseSqlServer.
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=facturacion.db";
var esAzureSql = connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<FacturacionDbContext>(options =>
{
    if (esAzureSql)
        options.UseSqlServer(connectionString);
    else
        options.UseSqlite(connectionString);
});

builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FacturacionDbContext>());

// Repositorios (Infrastructure -> Application.Interfaces)
builder.Services.AddScoped<ITarifaRepository, TarifaRepository>();
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();

// Cliente HTTP hacia Transporte: timeout corto y SIN reintentos automáticos (R4 / Avance 1).
var transporteBaseUrl = builder.Configuration["Servicios:TransporteBaseUrl"] ?? "http://localhost:5102";
builder.Services.AddHttpClient<ITransporteClient, TransporteClient>(client =>
{
    client.BaseAddress = new Uri(transporteBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(3);
});

// Servicios de aplicación
builder.Services.AddScoped<FacturaService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FacturacionDbContext>();
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
    public static async Task EjecutarAsync(FacturacionDbContext db)
    {
        if (await db.Tarifas.AnyAsync()) return;

        db.Tarifas.Add(new Tarifa
        {
            Version = "2026-Q3",
            CargoBase = 50m,
            PrecioPorKm = 2.5m,
            Moneda = "HNL",
            Activa = true
        });

        await db.SaveChangesAsync();
    }
}
