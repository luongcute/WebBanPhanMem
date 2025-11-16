public class EmailAttachment
{
    public int Id { get; set; }  // Khóa chính
    public string FileName { get; set; } = "";
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}
