using WebBanPhanMem.Models;

namespace WebBanPhanMem.Services
{
    public interface ILicenseService
    {
        Task<List<LicenseKey>> GetAllAsync();
        Task<LicenseKey?> GetByIdAsync(int id);
        Task CreateAsync(LicenseKey key);
        Task UpdateAsync(LicenseKey key);
        Task DeleteAsync(int id);
    }
}
