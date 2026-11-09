namespace ScanProWeb.Services.OrdenCompra
{
    public interface IOrdenCompraService
    {
        Task<List<OrdenCompraDto>> GetOrdersAsync();
        Task<OrdenCompraDto?> GetByIdAsync(string numero);
        Task<bool> AddAsync(OrdenCompraDto dto);
        Task<bool> DeleteAsync(string numero);
        Task<string> GetNextNumAsync();
    }
}
