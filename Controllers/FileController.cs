using file_uploader_api.Model;
using file_uploader_api.Service.FileService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace file_uploader_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly IMemoryCache _memoryCache;

    public FileController(IFileService fileService, IMemoryCache memoryCache)
    {
        _fileService = fileService;
        _memoryCache = memoryCache;
    }

    [HttpGet("GetEntryChildren")]
    public async Task<IActionResult> GetEntryChildren(string path)
    {
        path = path.Replace("%", "/");
        var entries = await _fileService.GetAllEntries(false);
        var parentEntry = await _fileService.FindEntryByPath(path);

        if (parentEntry != null)
            entries = await _fileService.GetEntryChildren(parentEntry);

        return Ok(entries);
    }

    [HttpPost("Entry")]
    public async Task<IActionResult> CreateEntry()
    {
        var metadata = Request.Form["metadata"];
        var chunk = Request.Form.Files.GetFile("chunk");

        // Processar a parte recebida (salvar em disco, processar, etc.)
        if (!StringValues.IsNullOrEmpty(metadata) && chunk != null)
        {
            var metadataObj = JsonConvert.DeserializeObject<Metadata>(metadata);

            if (metadataObj != null)
            {
                if (!_memoryCache.TryGetValue(metadataObj.Filename, out List<byte[]> chunks))
                {
                    chunks = new List<byte[]>();
                    _memoryCache.Set(metadataObj.Filename, chunks);
                }

                using (var memoryStream = new MemoryStream())
                {
                    await chunk.CopyToAsync(memoryStream);
                    var chunkData = memoryStream.ToArray();

                    chunks.Add(chunkData);

                    // Verificar se todos os chunks foram recebidos
                    if (chunks.Count == metadataObj.TotalChunks)
                    {
                        // Juntar os chunks
                        var fileData = Convert.ToBase64String(chunks.SelectMany(c => c).ToArray());

                        var parent = await _fileService.FindEntryByPath(metadataObj.Parent);

                        var entry = new Entry
                        {
                            Name = metadataObj.Filename,
                            Content = fileData,
                            LastModified = DateTimeOffset.FromUnixTimeMilliseconds(metadataObj.LastModified).LocalDateTime, //metadataObj.LastModified,
                            SizeInBytes = metadataObj.Size,
                            ParentId = parent?.Id,
                        };
                        entry.Type = entry.GetType(metadataObj.Type);

                        await _fileService.InsertEntry(entry);
                        _memoryCache.Remove(metadataObj.Filename);
                        return Ok(entry);
                    }
                }
            }

            return BadRequest();
        }

        return BadRequest();
    }

    [HttpGet("Entry")]
    public async Task<IActionResult> GetEntry(bool onlyFolders = false, string? path = "")
    {
        if (string.IsNullOrEmpty(path)) // Return all
        {
            var entries = await _fileService.GetAllEntries(onlyFolders);
            return Ok(entries);
        }

        path = path.Replace("%", "/");
        var entry = await _fileService.FindEntryByPath(path);
        return entry != null ? Ok(entry) : NotFound();
    }

    [HttpGet("EntryFullPath/{entryId:int}")]
    public async Task<ActionResult<string>> GetEntryFullPath(int entryId)
    {
        var entry = await _fileService.FindEntryById(entryId);

        if (entry == null)
            return NotFound(); // Entrada não encontrada

        var pathSegments = new List<string> { entry.Name };
        var currentEntry = entry;

        while (currentEntry.ParentId.HasValue)
        {
            var parentEntry = await _fileService.FindEntryById(currentEntry.ParentId.Value);
            if (parentEntry == null)
                return NotFound(); // Se o parent não for encontrado, interrompe o loop

            pathSegments.Add(parentEntry.Name);
            currentEntry = parentEntry;
        }

        pathSegments.Reverse();
        var fullPath = string.Join("/", pathSegments);
        return Ok(fullPath);
    }

    [HttpDelete("Entry")]
    public async Task<IActionResult> DeleteEntry(Entry entry)
    {
        var dbEntry = await _fileService.FindEntryById(entry.Id);

        if (dbEntry == null)
            return NotFound();

        await _fileService.DeleteEntry(dbEntry);
        return Ok();
    }

    [HttpPut("Entry")]
    public async Task<IActionResult> EditEntry(Entry entry)
    {
        var dbEntry = await _fileService.FindEntryById(entry.Id);

        if (dbEntry == null)
            return NotFound();

        dbEntry.Name = entry.Name;
        dbEntry.ParentId = entry.ParentId;
        dbEntry.LastModified = DateTime.Now;

        await _fileService.UpdateEntry(dbEntry);
        return Ok();
    }
}

