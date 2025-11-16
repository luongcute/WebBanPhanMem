// File: WebBanPhanMem/Areas/Admin/Models/ReportRecord.cs

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanPhanMem.Areas.Admin.Models
{
    public class ReportRecord
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }

        // ✅ Khắc phục: Khai báo là nullable (string?) vì dữ liệu có thể null từ DB
        public string? CustomerName { get; set; }

        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }

        // ✅ Khắc phục: Khai báo là nullable (string?)
        public string? Status { get; set; }

        public string OrderLink => $"/Admin/Orders/Details/{OrderId}";
    }
}