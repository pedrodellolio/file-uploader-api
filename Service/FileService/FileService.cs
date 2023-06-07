using file_uploader_api.Data;
using file_uploader_api.Model;
using Microsoft.EntityFrameworkCore;

namespace file_uploader_api.Service.FileService;

public class FileService : IFileService
{
    public readonly DataContext _context;
    public readonly string RootName = "root";

    public FileService(DataContext context)
    {
        _context = context;
    }

    public async Task InsertEntry(Entry entry)
    {
        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();
    }

    public Entry? GetRootDirectory()
    {
        return _context.Entries.FirstOrDefault(e => e.Name == RootName && e.Type == EntryType.Folder && e.ParentId == null);
    }

    public async Task<List<Entry>> GetEntryChildren(Entry entry)
    {
        return await _context.Entries.Where(e => e.ParentId == entry.Id).ToListAsync();
    }

    public async Task<List<Entry>> FindEntriesByPath(string path)
    {
        // Divide o caminho em partes com base no separador '/'
        string[] pathParts = path.Split('/');

        // Lista para armazenar as entradas encontradas
        List<Entry> result = new List<Entry>();

        // Começa a busca pelo diretório raiz
        var currentDirectory = _context.Entries.FirstOrDefault(e => e.Name == pathParts[0] && e.ParentId == null);

        // Se o diretório raiz não for encontrado, retorna uma lista vazia
        if (currentDirectory == null)
            return result;

        result.Add(currentDirectory);

        // Itera sobre as partes do caminho a partir da segunda parte
        for (int i = 1; i < pathParts.Length; i++)
        {
            // Busca o próximo diretório com base no nome e ID do diretório pai
            currentDirectory = _context.Entries.FirstOrDefault(e => e.Name == pathParts[i] && e.ParentId == currentDirectory.Id);

            // Se o próximo diretório não for encontrado, retorna a lista atual de entradas encontradas
            if (currentDirectory == null)
                return result;

            result.Add(currentDirectory);
        }

        return result;
    }

    public async Task<Entry?> FindEntryByPath(string path)
    {
        string[] pathSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var rootDirectory = GetRootDirectory();

        if (rootDirectory != null)
        {
            int? parentId = rootDirectory.Id;
            var children = await GetEntryChildren(rootDirectory);
            Entry? entry = null;
            for (int i = 1; i < pathSegments.Count(); i++)
            {
                entry = children.FirstOrDefault(e => e.ParentId == parentId && e.Name == pathSegments[i] && e.Type == EntryType.Folder);
                if (entry == null)
                    return null; // Se o segmento não for uma pasta válida, retorna -1 ou lança uma exceção
                parentId = entry.Id;
            }

            // A última entrada encontrada é a entrada final no caminho, só precisa buscar se não for o próprio root
            //Entry? finalEntry = null;
            //if (parentId != rootDirectory.Id)
            //    finalEntry = children.FirstOrDefault(e => e.ParentId == parentId && e.Type == EntryType.Folder);
            if (parentId == rootDirectory.Id)
                return rootDirectory;
            return entry;
        }
        return null;
    }

    public async Task<List<Entry>> GetAllEntries()
    {
        return await _context.Entries.ToListAsync();
    }
}