namespace WebBanPhanMem.Services
{
    public class ProductService : IProductService
    {
        // 💡 KHẮC PHỤC CẢNH BÁO CS1998: Đã xóa 'async' và trả về Task.CompletedTask
        public Task GetAllAsync()
        {
            // Xử lý logic (Đã chạy đồng bộ)

            // Trả về Task hoàn thành
            return Task.CompletedTask;
        }
    }
}