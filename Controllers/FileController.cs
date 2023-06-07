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

    [HttpPost]
    public async Task<IActionResult> UploadAsync()
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
                    }
                }
            }

            return Ok();
        }

        return BadRequest();
    }

    [HttpGet("GetEntries")]
    public async Task<IActionResult> GetEntries(string? path)
    {
        var entries = await _fileService.GetAllEntries();

        if (string.IsNullOrEmpty(path)) // Return all
            return Ok(entries);

        var parentEntry = await _fileService.FindEntryByPath(path);

        if (parentEntry != null)
            entries = await _fileService.GetEntryChildren(parentEntry);

        return entries.Any() ? Ok(entries) : NotFound();
    }

}
