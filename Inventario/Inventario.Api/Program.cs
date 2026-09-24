using Inventario.Application.Interfaces;
using Inventario.Application.Interfaces.Repositories;
using Inventario.Application.Services;
using Inventario.Domain.Entities;
using Inventario.Domain.Validation;
using Inventario.Infrastructure.Persistence;
using Inventario.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Tipo/Sentido se serializan como texto en vez de número, por legibilidad en Postman/Swagger.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Persistencia — una base de datos propia de Inventario. En local usa SQLite;
// en Azure, la cadena de conexión de Azure SQL (con "Server=") activa UseSqlServer.
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=inventario.db";
var esAzureSql = connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<InventarioDbContext>(options =>
{
    if (esAzureSql)
        options.UseSqlServer(connectionString);
    else
        options.UseSqlite(connectionString);
});

// El propio DbContext actúa como Unit of Work.
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<InventarioDbContext>());

// Repositorios (Infrastructure -> Application.Interfaces)
builder.Services.AddScoped<IBodegaRepository, BodegaRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IExistenciaRepository, ExistenciaRepository>();
builder.Services.AddScoped<IMovimientoStockRepository, MovimientoStockRepository>();

// Validadores de dominio (puros, sin dependencias de infraestructura)
builder.Services.AddScoped<IMovimientoStockValidator, MovimientoStockValidator>();

// Servicios de aplicación (orquestan validadores + repositorios)
builder.Services.AddScoped<BodegaService>();
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<StockService>();

var app = builder.Build();

// Aplica migraciones pendientes y siembra datos de prueba al iniciar.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
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
    public static async Task EjecutarAsync(InventarioDbContext db)
    {
        if (await db.Bodegas.AnyAsync()) return;

        var bodegaCentral = new Bodega { Codigo = "BOD-01", Nombre = "Bodega Central", Ubicacion = "Tegucigalpa", Activa = true };
        var bodegaNorte = new Bodega { Codigo = "BOD-02", Nombre = "Bodega Norte", Ubicacion = "San Pedro Sula", Activa = true };
        db.Bodegas.AddRange(bodegaCentral, bodegaNorte);

        var producto = new Producto { Sku = "SKU-001", Nombre = "Caja estándar 20kg", Unidad = "unidad", Activo = true };
        db.Productos.Add(producto);

        await db.SaveChangesAsync();

        db.Existencias.Add(new Existencia { BodegaId = bodegaCentral.Id, ProductoId = producto.Id, Cantidad = 100 });
        await db.SaveChangesAsync();
    }
}
