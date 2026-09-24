using Transporte.Application.Common;
using Transporte.Application.DTOs;
using Transporte.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Transporte.Api.Controllers;

[ApiController]
[Route("envios")]
public class EnviosController : ControllerBase
{
    private readonly EnvioService _envioService;

    public EnviosController(EnvioService envioService)
    {
        _envioService = envioService;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearEnvioRequest request)
    {
        var (estado, mensaje, envio) = await _envioService.CrearAsync(request);

        return estado switch
        {
            EstadoResultado.Creado => CreatedAtAction(nameof(GetById), new { id = envio!.Id }, envio),
            EstadoResultado.NoEncontrado => NotFound(mensaje),
            EstadoResultado.Conflicto => Conflict(mensaje),
            _ => BadRequest(mensaje)
        };
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var (estado, mensaje, envio) = await _envioService.ObtenerPorIdAsync(id);

        return estado switch
        {
            EstadoResultado.Exito => Ok(envio),
            EstadoResultado.NoEncontrado => NotFound(mensaje),
            _ => BadRequest(mensaje)
        };
    }

    [HttpPatch("{id:guid}/estado")]
    public async Task<IActionResult> ActualizarEstado(Guid id, [FromBody] ActualizarEstadoEnvioRequest request)
    {
        var (estado, mensaje, envio) = await _envioService.ActualizarEstadoAsync(id, request);

        return estado switch
        {
            EstadoResultado.Exito => Ok(envio),
            EstadoResultado.NoEncontrado => NotFound(mensaje),
            EstadoResultado.Conflicto => Conflict(mensaje),
            EstadoResultado.Invalido => BadRequest(mensaje),
            _ => BadRequest(mensaje)
        };
    }
}
