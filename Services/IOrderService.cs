using WebBanPhanMem.Models;

namespace WebBanPhanMem.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
        Task CreateAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int id);
    }
}
