using file_uploader_api.Model;
using Microsoft.EntityFrameworkCore;

namespace file_uploader_api.Data;

public class DataContext : DbContext
{
    public DataContext() : base() { }
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<Entry> Entries { get; set; }
}

