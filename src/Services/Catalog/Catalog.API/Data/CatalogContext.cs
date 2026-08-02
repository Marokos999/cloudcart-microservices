using Catalog.API.Models;
using MongoDB.Driver;

namespace Catalog.API.Data;

public class CatalogContext : ICatalogContext
{
    private readonly IMongoDatabase _database;

    public CatalogContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("Database"));
        _database = client.GetDatabase("CatalogDb");
    }

    public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
}