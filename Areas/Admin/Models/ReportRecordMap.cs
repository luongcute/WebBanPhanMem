using CsvHelper.Configuration;
using System.Globalization;

namespace WebBanPhanMem.Areas.Admin.Models
{
    public class ReportRecordMap : ClassMap<ReportRecord>
    {
        public ReportRecordMap()
        {
            Map(m => m.OrderId).Name("Mã đơn hàng");
            Map(m => m.OrderDate).Name("Ngày đặt").TypeConverterOption.Format("dd/MM/yyyy HH:mm");
            Map(m => m.CustomerName).Name("Khách hàng");
            Map(m => m.TotalItems).Name("Số lượng sản phẩm");
            Map(m => m.TotalAmount).Name("Tổng tiền").TypeConverterOption.Format("N0");
            Map(m => m.Status).Name("Trạng thái thanh toán");
            Map(m => m.OrderLink).Name("Liên kết chi tiết");
        }
    }
}
