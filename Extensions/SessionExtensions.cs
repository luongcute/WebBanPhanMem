using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace WebBanPhanMem.Extensions
{
    public static class SessionExtensions
    {
        // Thêm '?' vào object value để khai báo rằng tham số này có thể là null.
        // Điều này loại bỏ cảnh báo "Possible null reference".
        public static void SetObjectAsJson(this ISession session, string key, object? value)
        {
            // Logic xử lý null của bạn đã đúng
            if (value == null)
            {
                session.Remove(key);
                return;
            }

            // value đã được đảm bảo không null ở đây
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            // Logic đã đúng: trả về default (null) nếu chuỗi không tồn tại
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}