using Inventario.Application.Interfaces.Repositories;
using Inventario.Domain.Entities;

namespace Inventario.Application.Services;

public class ProductoService
{
    private readonly IProductoRepository _productoRepository;

    public ProductoService(IProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;
    }

    public async Task<List<Producto>> ObtenerTodosAsync()
        => await _productoRepository.GetAllAsync();
}
