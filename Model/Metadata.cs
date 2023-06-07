namespace file_uploader_api.Model;

public class Metadata
{
    public string Filename { get; set; } = string.Empty;
    public int Size { get; set; }
    public Int64 LastModified { get; set; }
    public string Type { get; set; } = string.Empty;
    public int TotalChunks { get; set; }
    public string Parent { get; set; } = string.Empty;
}