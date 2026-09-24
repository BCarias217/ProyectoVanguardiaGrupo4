using Transporte.Application.Interfaces.Repositories;
using Transporte.Domain.Entities;

namespace Transporte.Application.Services;

public class RutaService
{
    private readonly IRutaRepository _rutaRepository;

    public RutaService(IRutaRepository rutaRepository)
    {
        _rutaRepository = rutaRepository;
    }

    public async Task<List<Ruta>> ObtenerTodasAsync()
        => await _rutaRepository.GetAllAsync();
}
