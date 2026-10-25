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
            var aluno = new User("Gabriel Costa Solano", "aluno@fiap.com.br", "Aluno@123", "Usuario");
            await Users.InsertManyAsync(new[] { admin, aluno });
        }

        // Seed do grupo oficial dos integrantes
        var grupoCount = await Grupos.CountDocumentsAsync(Builders<Grupo>.Filter.Empty);
        if (grupoCount == 0)
        {
            var grupoIntegrantes = new Grupo(
                nome: "Equipe NEXT 2TDSPV",
                turma: "2TDSPV",
                integrantes: new[]
                {
                    "Gabriel Costa Solano (RM562325)",
                    "Leandro Guarido (RM561760)",
                    "Kaiky Pereira Rodrigues (RM564578)"
                }
            );
            await Grupos.InsertOneAsync(grupoIntegrantes);

            var trabalhoInicial = new Trabalho(
                titulo: "Ecossistema Inteligente de Gestão FIAP NEXT",
                descricao: "API RESTful corporativa para gestão de projetos acadêmicos e jurados do festival FIAP NEXT.",
                categoria: "Enterprise .NET & NoSQL",
                linkRepositorio: "https://github.com/gabriel-solano/fiap-next-api",
                linkPitch: "https://youtube.com/watch?v=pitch_fiap_next",
                grupoId: grupoIntegrantes.Id,
                nota: 10.0m,
                relevancia: 98.0m
            );
            await Trabalhos.InsertOneAsync(trabalhoInicial);
        }
    }
}
