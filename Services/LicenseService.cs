using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Data;
using WebBanPhanMem.Models;

namespace WebBanPhanMem.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly ApplicationDbContext _context;

        public LicenseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LicenseKey>> GetAllAsync()
        {
            return await _context.LicenseKeys.ToListAsync();
        }

        public async Task<LicenseKey?> GetByIdAsync(int id)
        {
            return await _context.LicenseKeys.FindAsync(id);
        }

        public async Task CreateAsync(LicenseKey key)
        {
            _context.LicenseKeys.Add(key);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LicenseKey key)
        {
            _context.LicenseKeys.Update(key);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var key = await _context.LicenseKeys.FindAsync(id);
            if (key != null)
            {
                _context.LicenseKeys.Remove(key);
                await _context.SaveChangesAsync();
            }
        }
    }
}
