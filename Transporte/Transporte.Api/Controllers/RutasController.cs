using Transporte.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Transporte.Api.Controllers;

[ApiController]
[Route("rutas")]
public class RutasController : ControllerBase
{
    private readonly RutaService _rutaService;

    public RutasController(RutaService rutaService)
    {
        _rutaService = rutaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rutas = await _rutaService.ObtenerTodasAsync();
        return Ok(rutas);
    }
}
