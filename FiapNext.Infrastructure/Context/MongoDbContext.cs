using FiapNext.Domain.Entities;
using FiapNext.Infrastructure.Mappings;
using FiapNext.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FiapNext.Infrastructure.Context;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _client;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        MongoMappings.Configure();

        var clientSettings = MongoClientSettings.FromConnectionString(settings.Value.ConnectionString);
        _client = new MongoClient(clientSettings);
        _database = _client.GetDatabase(settings.Value.DatabaseName);
    }

    // Construtor direto para testes/injeção personalizada
    public MongoDbContext(IMongoDatabase database, IMongoClient client)
    {
        MongoMappings.Configure();
        _database = database;
        _client = client;
    }

    public IMongoDatabase Database => _database;
    public IMongoClient Client => _client;

    public IMongoCollection<Grupo> Grupos => _database.GetCollection<Grupo>("Grupos");
    public IMongoCollection<Trabalho> Trabalhos => _database.GetCollection<Trabalho>("Trabalhos");
    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

    public async Task SeedInitialDataAsync()
    {
        // Criação de índices para otimização de busca
        var trabalhoBuilder = Builders<Trabalho>.IndexKeys;
        var tituloIndex = new CreateIndexModel<Trabalho>(trabalhoBuilder.Ascending(t => t.Titulo));
        var notaIndex = new CreateIndexModel<Trabalho>(trabalhoBuilder.Ascending(t => t.Nota));
        var relevanciaIndex = new CreateIndexModel<Trabalho>(trabalhoBuilder.Ascending(t => t.Relevancia));
        await Trabalhos.Indexes.CreateManyAsync(new[] { tituloIndex, notaIndex, relevanciaIndex });

        // Seed de usuários padrão para facilitar a execução e avaliação
        var count = await Users.CountDocumentsAsync(Builders<User>.Filter.Empty);
        if (count == 0)
        {
            var admin = new User("Coordenação NEXT", "admin@fiap.com.br", "Admin@123", "Admin");
            var aluno = new User("Aluno FIAP", "aluno@fiap.com.br", "Aluno@123", "Usuario");
            await Users.InsertManyAsync(new[] { admin, aluno });
        }
    }
}
