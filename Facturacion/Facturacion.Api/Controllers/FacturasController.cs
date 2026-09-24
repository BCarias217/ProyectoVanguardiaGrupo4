using Facturacion.Application.Common;
using Facturacion.Application.DTOs;
using Facturacion.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Controllers;

[ApiController]
[Route("api/v1/facturas")]
public class FacturasController : ControllerBase
{
    private readonly FacturaService _facturaService;

    public FacturasController(FacturaService facturaService)
    {
        _facturaService = facturaService;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearFacturaRequest request)
    {
        var (estado, mensaje, factura) = await _facturaService.CrearAsync(request);

        return estado switch
        {
            EstadoResultado.Creado => CreatedAtAction(nameof(ObtenerPorId), new { id = factura!.Id }, factura),
            EstadoResultado.NoEncontrado => NotFound(mensaje),
            EstadoResultado.Conflicto => Conflict(mensaje),
            EstadoResultado.DependenciaNoDisponible => StatusCode(StatusCodes.Status503ServiceUnavailable, mensaje),
            EstadoResultado.Invalido => BadRequest(mensaje),
            _ => BadRequest(mensaje)
        };
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas([FromQuery] Guid? envioId)
    {
        var facturas = await _facturaService.ObtenerTodasAsync(envioId);
        return Ok(facturas); // [] cuando no hay coincidencias, nunca null.
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
    {
        var (estado, factura) = await _facturaService.ObtenerPorIdAsync(id);

        return estado switch
        {
            EstadoResultado.Exito => Ok(factura),
            EstadoResultado.NoEncontrado => NotFound(),
            _ => BadRequest()
        };
    }
}
