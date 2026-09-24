using Inventario.Application.Common;
using Inventario.Application.DTOs;
using Inventario.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
public class MovimientosStockController : ControllerBase
{
    private readonly StockService _stockService;

    public MovimientosStockController(StockService stockService)
    {
        _stockService = stockService;
    }

    // GET /bodegas/{bodegaId}/stock/{productoId}
    [HttpGet("bodegas/{bodegaId:int}/stock/{productoId:int}")]
    public async Task<IActionResult> ConsultarStock(int bodegaId, int productoId)
    {
        var (estado, mensaje, existencia) = await _stockService.ConsultarStockAsync(bodegaId, productoId);

        return estado switch
        {
            EstadoResultado.Exito => Ok(existencia),
            EstadoResultado.NoEncontrado => NotFound(mensaje),
            _ => BadRequest(mensaje)
        };
    }

    // POST /movimientos-stock
    [HttpPost("movimientos-stock")]
    public async Task<IActionResult> Crear([FromBody] CrearMovimientoStockRequest request)
    {
        var (estado, mensaje, movimiento) = await _stockService.RegistrarMovimientoAsync(request);

        return estado switch
        {
            EstadoResultado.Creado => CreatedAtAction(nameof(ObtenerPorId), new { id = movimiento!.Id }, movimiento),
            EstadoResultado.Exito => Ok(movimiento), // reintento con el mismo SolicitudId
            EstadoResultado.Invalido => BadRequest(mensaje),
            EstadoResultado.NoEncontrado => NotFound(mensaje),
            EstadoResultado.Conflicto => Conflict(mensaje),
            _ => BadRequest(mensaje)
        };
    }

    // GET /movimientos-stock/{id}
    [HttpGet("movimientos-stock/{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
    {
        var (estado, mensaje, movimiento) = await _stockService.ObtenerPorIdAsync(id);

        return estado switch
        {
            EstadoResultado.Exito => Ok(movimiento),
            EstadoResultado.NoEncontrado => NotFound(mensaje),
            _ => BadRequest(mensaje)
        };
    }
}
