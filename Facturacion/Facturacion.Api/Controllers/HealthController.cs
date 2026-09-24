using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Facturacion.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        estado = "ok",
        servicio = "Facturacion",
        version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0"
    });
}
