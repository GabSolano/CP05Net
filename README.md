# FIAP NEXT - API RESTful para Gestão de Trabalhos e Grupos

[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![MongoDB](https://img.shields.io/badge/MongoDB-Atlas%20%2F%20NoSQL-green.svg)](https://www.mongodb.com/)
[![RESTful](https://img.shields.io/badge/Maturidade-HATEOAS%20Nível%203-blue.svg)](#3-hateoas-e-maturidade-restful-nível-3)
[![Testes](https://img.shields.io/badge/Testes-50%20Passando%20(xUnit%20%2B%20Moq)-brightgreen.svg)](#7-testes-automatizados-xunit--moq--fluentassertions)

> **Checkpoint 5**  
> **Disciplina:** Advanced Business Development with .NET  
> **Curso:** Tecnologia em Análise e Desenvolvimento de Sistemas (2TDS - 2026)  
> **Turma:** 2TDSPV  
> **Professor:** Alex Sander Resende de Deus  

### 👥 Integrantes do Grupo
| Nome | RM | Turma |
| :--- | :---: | :---: |
| **Gabriel Costa Solano** | 562325 | 2TDSPV |
| **Leandro Guarido** | 561760 | 2TDSPV |
| **Kaiky Pereira Rodrigues** | 564578 | 2TDSPV |

---

## 1. Visão Geral e Contexto do Projeto

O **FIAP NEXT** é um dos maiores festivais universitários de inovação, reunindo centenas de projetos e desafios propostos por alunos, empresas parceiras e professores. Para apoiar a organização e a banca de jurados, esta API corporativa foi desenvolvida utilizando **Minimal APIs no .NET**, arquitetura limpa (**Clean Architecture**), persistência **NoSQL MongoDB Atlas**, navegação com hiperlinks dinâmicos (**HATEOAS Nível 3 de Richardson**), segurança baseada em **JWT & RBAC**, observabilidade corporativa (**OpenTelemetry, Serilog e Health Checks**) e testes unitários automatizados com **xUnit, Moq e FluentAssertions**.

---

## 2. Arquitetura, Clean Code & Princípios S.O.L.I.D.

O projeto é estruturado em **4 camadas bem definidas**, garantindo desacoplamento e aderência total aos princípios de arquitetura corporativa:

```
fiap-next-api/
├── FiapNext.Domain/               # Camada de Domínio (Entidades puras, Validações, Interfaces)
│   ├── Entities/                  # Grupo, Trabalho, User
│   ├── Exceptions/                # DomainValidationException
│   └── Interfaces/                # ITrabalhoRepository, IGrupoRepository, IUserRepository
├── FiapNext.Application/          # Camada de Aplicação (Casos de uso, DTOs imutáveis, Orquestração)
│   ├── Common/Hateoas/            # ResourceEnvelope<T>, PagedResult<T>, LinkDto
│   ├── Common/Metrics/            # IFiapNextMetrics
│   ├── DTOs/                      # Records imutáveis para requests e responses
│   ├── Interfaces/                # ITrabalhoService, IGrupoService, IAuthService, IHateoasService
│   └── Services/                  # Implementação das regras da aplicação
├── FiapNext.Infrastructure/       # Camada de Infraestrutura (Persistência, Drivers, Auth, Health)
│   ├── Authentication/            # JwtTokenService
│   ├── Context/                   # MongoDbContext com BsonClassMap e inicialização
│   ├── Health/                    # MongoHealthCheck (IHealthCheck)
│   ├── Mappings/                  # Serialização de Guid como String e Decimal como Decimal128
│   ├── Repositories/              # TrabalhoRepository (Regex, Paginação, Filtros), GrupoRepository
│   └── Settings/                  # MongoDbSettings, JwtSettings
├── FiapNext.Api/                  # Apresentação / Minimal APIs
│   ├── Endpoints/                 # AuthEndpoints, TrabalhosEndpoints, GruposEndpoints
│   ├── Middlewares/               # CorrelationIdMiddleware, GlobalExceptionHandlerMiddleware
│   ├── Telemetry/                 # FiapNextTelemetry (OpenTelemetry Counters e ActivitySource)
│   └── Program.cs                 # Configuração de Pipeline, DI, JWT, Swagger e Serilog
└── FiapNext.Tests.Unit/           # Testes Automatizados (AAA, xUnit, Moq, FluentAssertions)
    ├── Domain/                    # Testes de regras de negócio com [Theory] e [InlineData]
    └── Application/               # Testes de serviços isolando repositórios com Mocks
```

### Princípios S.O.L.I.D. Aplicados
- **S (Single Responsibility Principle):** Cada classe possui um propósito único. Endpoints apenas roteiam requisições; Services contêm a orquestração de negócios; Repositories cuidam exclusivamente do MongoDB; Entidades protegem suas próprias invariantes.
- **O (Open/Closed Principle):** Com o HATEOAS e a injeção de dependência via interfaces (`ITrabalhoService`, `IHateoasService`), novos fluxos e hipermídias podem ser estendidos sem modificar os clientes existentes.
- **L (Liskov Substitution Principle):** Todas as implementações de repositório e serviços respeitam integralmente os contratos de suas interfaces.
- **I (Interface Segregation Principle):** Interfaces segregadas e enxutas (`ITrabalhoRepository`, `IGrupoRepository`, `ITokenService`, `IFiapNextMetrics`).
- **D (Dependency Inversion Principle):** A camada de Apresentação e a camada de Aplicação dependem de abstrações (interfaces) do Domínio, e não de classes concretas de banco de dados ou infraestrutura externa.
- **DTOs Imutáveis:** Criados utilizando `record`, garantindo imutabilidade thread-safe.

---

## 3. Persistência de Dados com MongoDB NoSQL

A camada de persistência utiliza o driver oficial `MongoDB.Driver` com as seguintes convenções:
1. **Chaves Guid como String:** Mapeamento explícito configurado em `MongoMappings.cs` utilizando `GuidSerializer(BsonType.String)`.
2. **Decimais e Métricas como Decimal128:** As propriedades `Nota` e `Relevancia` são mapeadas via `DecimalSerializer(BsonType.Decimal128)` para precisão analítica.
3. **Repository Pattern Assíncrono:** Todas as operações utilizam métodos assíncronos (`Find`, `InsertOneAsync`, `ReplaceOneAsync`, `DeleteOneAsync`, `CountDocumentsAsync`) com suporte a `CancellationToken`.

### Configuração da Conexão (`appsettings.json`)
```json
"MongoDb": {
  "ConnectionString": "mongodb://localhost:27017",
  "DatabaseName": "FiapNextDb"
}
```
*Para conectar ao MongoDB Atlas na nuvem, basta substituir o valor de `ConnectionString` pela URI fornecida pelo Atlas (`mongodb+srv://<usuario>:<senha>@cluster.mongodb.net/?retryWrites=true&w=majority`).*

---

## 4. HATEOAS e Maturidade RESTful (Nível 3)

A API adota o modelo de maturidade de Richardson no nível 3 por meio de respostas envelopadas e hiperlinks semânticos dinâmicos (`LinkDto`):

### Estrutura do Envelope
- **`ResourceEnvelope<T>`**: Contém o objeto de dados em `data` e a lista de ações disponíveis em `links`.
- **`PagedResult<T>`**: Envelopa paginação no servidor com metadados (`skip`, `limit`, `totalItems`, `totalPages`, `currentPage`) e links de navegação (`self`, `first`, `prev`, `next`, `last`).

### Hiperlinks com Controle de Acesso Dinâmico (RBAC)
Os hiperlinks de mutação (`update` - PUT e `delete` - DELETE) são inseridos **dinamicamente apenas quando a requisição é realizada por um usuário com perfil `Admin`**. Quando um usuário do perfil `Usuario` (aluno) consulta o recurso, apenas os links de leitura e navegação são retornados:

#### Exemplo de Resposta para Aluno (`Usuario`):
```json
{
  "data": {
    "id": "e331cbf7-0fb5-48b5-93ca-62b8b9f71c35",
    "titulo": "Assistente IA de Estudos",
    "descricao": "Plataforma de inteligência artificial generativa para suporte a alunos",
    "categoria": "IA",
    "linkRepositorio": "https://github.com/fiap/ia-studies",
    "linkPitch": "https://youtube.com/watch?v=pitch123",
    "grupoId": "2f24ef38-d636-4074-b5bc-ce51f4728564",
    "grupoNome": "Equipe BrainTech",
    "nota": 9.5,
    "relevancia": 92.0,
    "dataSubmissao": "2026-10-25T15:00:00Z"
  },
  "links": [
    { "href": "http://localhost:5000/api/trabalhos/e331cbf7-0fb5-48b5-93ca-62b8b9f71c35", "rel": "self", "method": "GET" },
    { "href": "http://localhost:5000/api/grupos/2f24ef38-d636-4074-b5bc-ce51f4728564", "rel": "grupo", "method": "GET" },
    { "href": "http://localhost:5000/api/trabalhos", "rel": "collection", "method": "GET" }
  ]
}
```

#### Exemplo de Resposta para Professor / Jurado (`Admin`):
*Inclui dinamicamente as permissões de edição e exclusão:*
```json
{
  "data": { ... },
  "links": [
    { "href": "http://localhost:5000/api/trabalhos/e331cbf7-0fb5-48b5-93ca-62b8b9f71c35", "rel": "self", "method": "GET" },
    { "href": "http://localhost:5000/api/grupos/2f24ef38-d636-4074-b5bc-ce51f4728564", "rel": "grupo", "method": "GET" },
    { "href": "http://localhost:5000/api/trabalhos", "rel": "collection", "method": "GET" },
    { "href": "http://localhost:5000/api/trabalhos/e331cbf7-0fb5-48b5-93ca-62b8b9f71c35", "rel": "update", "method": "PUT" },
    { "href": "http://localhost:5000/api/trabalhos/e331cbf7-0fb5-48b5-93ca-62b8b9f71c35", "rel": "delete", "method": "DELETE" }
  ]
}
```

---

## 5. Segurança, JWT e RBAC

A segurança utiliza **JSON Web Tokens (JWT)** autenticados via middleware e validados por políticas de autorização:
- **Policy `"SomenteAdmin"`**: Registrada via `builder.Services.AddAuthorization(options => options.AddPolicy("SomenteAdmin", policy => policy.RequireRole("Admin")));` e aplicada com `.RequireAuthorization("SomenteAdmin")`.

### Usuários de Demonstração Pré-Configurados:
| Perfil | E-mail | Senha | Permissões |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@fiap.com.br` | `Admin@123` | Acesso total: criação, alteração e exclusão de trabalhos e grupos |
| **Usuario** (Aluno) | `aluno@fiap.com.br` | `Aluno@123` | Consulta e navegação por trabalhos e grupos |

---

## 6. Diagnóstico, Monitoramento e Observabilidade

### 1. Health Checks (`/health`)
Endpoint de verificação de integridade da infraestrutura executando comando `ping` no MongoDB Atlas:
```bash
curl http://localhost:5000/health
```

### 2. Serilog (Logging Estruturado)
- Logs gravados em formato estruturado tanto no **Console** quanto em arquivos rotativos diários em **`logs/app-.log`**.
- Integrado ao **`CorrelationIdMiddleware`**: toda requisição recebe ou gera um cabeçalho `X-Correlation-ID` que é inserido no `LogContext` do Serilog e devolvido no cabeçalho da resposta HTTP, permitindo rastreabilidade de ponta a ponta.

### 3. OpenTelemetry (Métricas e Tracing Distribuído)
- **Métrica Customizada:** Criação do Meter `FiapNext.Metrics` com o contador:
  - `fiap_next.trabalhos.submetidos_count`: Incrementado a cada trabalho cadastrado no festival NEXT com a tag de categoria correspondente.
- **Tracing Distribuído:** ActivitySource `FiapNext.Api` com criação de Spans instrumentados (`Trabalho.CriarSubmissao`, `Trabalho.ConsultarPaginado`, `Trabalho.ConsultarPorId`) com tags detalhadas de contexto.

---

## 7. Testes Automatizados (xUnit + Moq + FluentAssertions)

O projeto conta com **50 testes unitários automatizados** com **100% de taxa de aprovação**:
- Estruturação estrita sob o padrão **AAA (Arrange, Act, Assert)**.
- Cobertura de regras de negócio puras nas Entidades de Domínio com testes parametrizados utilizando `[Theory]` e `[InlineData]`.
- Isolamento dos repositórios na camada de Aplicação através de `Moq` (`Mock<ITrabalhoRepository>`, `Mock<IGrupoRepository>`, `Mock<IUserRepository>`).
- Asserções legíveis e expressivas com `FluentAssertions`.

### Como Executar os Testes:
```bash
dotnet test
```

---

## 8. Guia de Execução da Aplicação

### Pré-requisitos
- .NET 10.0 SDK (ou .NET 8+)
- MongoDB (local ou MongoDB Atlas)

### Inicialização
```bash
cd FiapNext.Api
dotnet run
```
A API iniciará e a documentação interativa estará disponível em:
- **Swagger UI:** `http://localhost:5000/` (ou porta configurada no launchSettings)
- **Health Check:** `http://localhost:5000/health`

---

## 9. Exemplos de Requisições (Endpoints)

### RF01 - Autenticação e Login (JWT)
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@fiap.com.br",
    "senha": "Admin@123"
  }'
```

### Cadastro de Grupo (Admin)
```bash
curl -X POST http://localhost:5000/api/grupos \
  -H "Authorization: Bearer <SEU_TOKEN_ADMIN>" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Tech Warriors",
    "turma": "2TDSPV",
    "integrantes": ["Gabriel Silva", "Mariana Costa", "Lucas Mendes"]
  }'
```

### RF02 - Cadastro de Trabalhos (Escrita - Somente Admin)
```bash
curl -X POST http://localhost:5000/api/trabalhos \
  -H "Authorization: Bearer <SEU_TOKEN_ADMIN>" \
  -H "Content-Type: application/json" \
  -d '{
    "titulo": "EcoTrack IoT",
    "descricao": "Monitoramento de consumo energético em tempo real utilizando ESP32 e Cloud",
    "categoria": "IoT & Smart Cities",
    "linkRepositorio": "https://github.com/fiap/ecotrack",
    "linkPitch": "https://youtube.com/watch?v=pitch_ecotrack",
    "grupoId": "<ID_DO_GRUPO_CRIADO>",
    "nota": 9.8,
    "relevancia": 95.0
  }'
```

### RF03 - Consulta Paginada e Filtrada (Autenticado)
*Suporta regex case-insensitive por título, filtro por notas/relevância, ordenação e paginação:*
```bash
curl -X GET "http://localhost:5000/api/trabalhos?titulo=eco&minNota=8.0&maxNota=10.0&sortBy=nota&sortOrder=desc&skip=0&limit=10" \
  -H "Authorization: Bearer <SEU_TOKEN>"
```

### RF04 - Consulta de Trabalho por ID com HATEOAS
```bash
curl -X GET http://localhost:5000/api/trabalhos/<ID_DO_TRABALHO> \
  -H "Authorization: Bearer <SEU_TOKEN>"
```
