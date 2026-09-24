using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        estado = "ok",
        servicio = "Inventario",
        version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0"
    });
}
