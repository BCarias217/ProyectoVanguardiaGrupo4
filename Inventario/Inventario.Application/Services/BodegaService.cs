using Inventario.Application.Common;
using Inventario.Application.Interfaces.Repositories;
using Inventario.Domain.Entities;

namespace Inventario.Application.Services;

public class BodegaService
{
    private readonly IBodegaRepository _bodegaRepository;

    public BodegaService(IBodegaRepository bodegaRepository)
    {
        _bodegaRepository = bodegaRepository;
    }

    public async Task<List<Bodega>> ObtenerTodasAsync()
        => await _bodegaRepository.GetAllAsync();

    public async Task<(EstadoResultado Estado, Bodega? Bodega)> ObtenerPorIdAsync(int id)
    {
        var bodega = await _bodegaRepository.GetByIdAsync(id);
        return bodega is null
            ? (EstadoResultado.NoEncontrado, null)
            : (EstadoResultado.Exito, bodega);
    }
}
