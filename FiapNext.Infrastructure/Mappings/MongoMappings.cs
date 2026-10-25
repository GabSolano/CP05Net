using FiapNext.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace FiapNext.Infrastructure.Mappings;

public static class MongoMappings
{
    private static bool _configured = false;
    private static readonly object _lock = new();

    public static void Configure()
    {
        if (_configured) return;

        lock (_lock)
        {
            if (_configured) return;

            // Registrar serializador global para Guid como String
            try
            {
                BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            }
            catch (BsonSerializationException)
            {
                // Já registrado
            }

            // Registrar serializador global para Decimal como Decimal128
            try
            {
                BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));
            }
            catch (BsonSerializationException)
            {
                // Já registrado
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Grupo)))
            {
                BsonClassMap.RegisterClassMap<Grupo>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(g => g.Id)
                      .SetIdGenerator(GuidGenerator.Instance)
                      .SetSerializer(new GuidSerializer(BsonType.String));
                    cm.MapProperty(g => g.Nome).SetElementName("nome");
                    cm.MapProperty(g => g.Turma).SetElementName("turma");
                    cm.MapProperty(g => g.Integrantes).SetElementName("integrantes");
                    cm.MapProperty(g => g.DataCriacao).SetElementName("dataCriacao");
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Trabalho)))
            {
                BsonClassMap.RegisterClassMap<Trabalho>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(t => t.Id)
                      .SetIdGenerator(GuidGenerator.Instance)
                      .SetSerializer(new GuidSerializer(BsonType.String));
                    cm.MapProperty(t => t.Titulo).SetElementName("titulo");
                    cm.MapProperty(t => t.Descricao).SetElementName("descricao");
                    cm.MapProperty(t => t.Categoria).SetElementName("categoria");
                    cm.MapProperty(t => t.LinkRepositorio).SetElementName("linkRepositorio");
                    cm.MapProperty(t => t.LinkPitch).SetElementName("linkPitch");
                    cm.MapProperty(t => t.GrupoId)
                      .SetElementName("grupoId")
                      .SetSerializer(new GuidSerializer(BsonType.String));
                    cm.MapProperty(t => t.Nota)
                      .SetElementName("nota")
                      .SetSerializer(new DecimalSerializer(BsonType.Decimal128));
                    cm.MapProperty(t => t.Relevancia)
                      .SetElementName("relevancia")
                      .SetSerializer(new DecimalSerializer(BsonType.Decimal128));
                    cm.MapProperty(t => t.DataSubmissao).SetElementName("dataSubmissao");
                    cm.UnmapProperty(t => t.Grupo);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
            {
                BsonClassMap.RegisterClassMap<User>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(u => u.Id)
                      .SetIdGenerator(GuidGenerator.Instance)
                      .SetSerializer(new GuidSerializer(BsonType.String));
                    cm.MapProperty(u => u.Nome).SetElementName("nome");
                    cm.MapProperty(u => u.Email).SetElementName("email");
                    cm.MapProperty(u => u.Senha).SetElementName("senha");
                    cm.MapProperty(u => u.Role).SetElementName("role");
                });
            }

            _configured = true;
        }
    }
}
