namespace file_uploader_api.Model;
public class Entry
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SizeInBytes { get; set; }
    public string? Content { get; set; }
    public DateTime LastModified { get; set; } = DateTime.Now;
    public EntryType Type { get; set; }

    public EntryType GetType(string fileType)
    {
        return string.IsNullOrEmpty(fileType) ? EntryType.Folder : EntryType.File;
    }
}

public enum EntryType
{
    Folder,
    File
}