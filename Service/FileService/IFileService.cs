using file_uploader_api.Model;

namespace file_uploader_api.Service.FileService;

public interface IFileService
{
    Task InsertEntry(Entry entry);
    Task<List<Entry>> FindEntriesByPath(string path);
    Task<Entry?> FindEntryByPath(string path);
    Task<List<Entry>> GetAllEntries();
    Task<List<Entry>> GetEntryChildren(Entry entry);
}