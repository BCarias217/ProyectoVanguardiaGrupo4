using Inventario.Application.Common;
using Inventario.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("bodegas")]
public class BodegasController : ControllerBase
{
    private readonly BodegaService _bodegaService;

    public BodegasController(BodegaService bodegaService)
    {
        _bodegaService = bodegaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var bodegas = await _bodegaService.ObtenerTodasAsync();
        return Ok(bodegas);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var (estado, bodega) = await _bodegaService.ObtenerPorIdAsync(id);

        return estado switch
        {
            EstadoResultado.Exito => Ok(bodega),
            EstadoResultado.NoEncontrado => NotFound(),
            _ => BadRequest()
        };
    }
}
