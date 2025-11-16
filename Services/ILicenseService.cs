using WebBanPhanMem.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebBanPhanMem.Services
{
    public interface ILicenseService
    {
        // CRUD Methods
        Task<List<LicenseKey>> GetAllAsync();
        Task<LicenseKey?> GetByIdAsync(int id);
        Task CreateAsync(LicenseKey key);
        Task UpdateAsync(LicenseKey key);
        Task DeleteAsync(int id);

        // Nghiệp vụ mới: thêm tham số attachments với giá trị mặc định null
        Task<List<LicenseKey>> AssignKeysToOrderAsync(int orderId);
        Task SendKeysByEmailAsync(string toEmail, int orderId, List<LicenseKey> keys, List<EmailAttachment>? attachments = null);
    }
}
