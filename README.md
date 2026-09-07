# BookStore API — Minimal API com .NET 8

[![CI](https://github.com/matheusflorindo32/dio-minimal-api/actions/workflows/ci.yml/badge.svg)](https://github.com/matheusflorindo32/dio-minimal-api/actions)

API REST para gerenciamento de catálogo de livraria, construída como projeto autoral para o desafio DIO Minimal API. Demonstra domínio de ASP.NET Core Minimal APIs, Entity Framework Core, autenticação JWT e boas práticas de desenvolvimento.

## Diferenças em relação ao projeto de referência DIO

| Aspecto | Referência DIO | Este projeto |
|---|---|---|
| Runtime | .NET 7 | .NET 8 (LTS) |
| Banco de dados | MySQL (Pomelo) | SQLite (zero dependência externa) |
| Senhas | Texto puro | HMACSHA256 com salt aleatório |
| DTOs | Exposição de entidades | Records separados (Create, Update, Response) |
| Validação | Inline nos endpoints | Métodos reutilizáveis com Problem Details |
| Paginação | Nenhuma | PagedResponse<T> genérico |
| Filtragem | Nenhuma | Por título e autor |
| Relacionamento | Nenhum | Book → Category (1:N com FK) |
| Testes | MSTest, 1 teste | xUnit, unitários + integração |
| Docker | Não | Multi-stage com non-root user |
| CI | Não | GitHub Actions |
| Roles | Admin/Editor (sem enforcement) | Admin/Editor com [Authorize(Roles)] |

## Stack

- **Runtime:** .NET 8.0 (LTS)
- **Framework:** ASP.NET Core Minimal APIs
- **ORM:** Entity Framework Core 8 + SQLite
- **Auth:** JWT Bearer (HMACSHA256)
- **Docs:** Swagger/OpenAPI
- **Testes:** xUnit + WebApplicationFactory
- **Container:** Docker multi-stage
- **CI:** GitHub Actions

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (opcional)

## Quick Start

```bash
# Clonar
git clone https://github.com/matheusflorindo32/dio-minimal-api.git
cd dio-minimal-api

# Restaurar e executar
dotnet restore
dotnet run --project src/BookStore.Api

# Acessar Swagger
# http://localhost:5000/swagger
```

### Usuário seed (desenvolvimento)

| Email | Senha | Role |
|---|---|---|
| admin@bookstore.com | Admin@123 | Admin |

> **Nota:** O hash da senha seed é pré-computado. A senha `Admin@123` só funciona se o seed no `AppDbContext` for mantido inalterado.

## Docker

```bash
# Build e executar
docker compose up --build

# A API estará disponível em http://localhost:8080/swagger

# Parar
docker compose down
```

## Testes

```bash
# Todos os testes
dotnet test

# Apenas unitários
dotnet test tests/BookStore.UnitTests

# Apenas integração
dotnet test tests/BookStore.IntegrationTests

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## Estrutura do Projeto

```
dio-minimal-api/
├── src/
│   └── BookStore.Api/
│       ├── Domain/
│       │   ├── DTOs/          # Request/Response records
│       │   ├── Entities/      # User, Book, Category
│       │   ├── Enums/         # UserRole
│       │   ├── Interfaces/    # Service contracts
│       │   └── Services/      # Business logic
│       ├── Endpoints/         # Minimal API route groups
│       ├── Infrastructure/
│       │   └── Data/          # AppDbContext + seed
│       └── Program.cs         # Application entry point
├── tests/
│   ├── BookStore.UnitTests/          # Service-level tests
│   └── BookStore.IntegrationTests/   # HTTP endpoint tests
├── docs/                      # Documentation
├── .github/workflows/ci.yml  # CI pipeline
├── Dockerfile                 # Multi-stage build
├── docker-compose.yml
└── requests.http              # REST Client examples
```

## Endpoints

### Authentication (público)
| Método | Rota | Descrição |
|---|---|---|
| POST | /auth/register | Registrar novo usuário |
| POST | /auth/login | Obter JWT token |

### Categories (autenticado)
| Método | Rota | Role | Descrição |
|---|---|---|---|
| GET | /categories | Qualquer | Listar todas |
| GET | /categories/{id} | Qualquer | Buscar por ID |
| POST | /categories | Admin | Criar categoria |
| PUT | /categories/{id} | Admin | Atualizar categoria |
| DELETE | /categories/{id} | Admin | Remover (se sem livros) |

### Books (autenticado)
| Método | Rota | Role | Descrição |
|---|---|---|---|
| GET | /books | Qualquer | Listar com paginação e filtros |
| GET | /books/{id} | Qualquer | Buscar por ID |
| POST | /books | Admin, Editor | Criar livro |
| PUT | /books/{id} | Admin | Atualizar livro |
| DELETE | /books/{id} | Admin | Remover livro |

### Users (Admin)
| Método | Rota | Descrição |
|---|---|---|
| GET | /users | Listar todos |
| GET | /users/{id} | Buscar por ID |

## Documentação Adicional

- [docs/architecture.md](docs/architecture.md) — Decisões arquiteturais
- [docs/security.md](docs/security.md) — Práticas de segurança
- [docs/api.md](docs/api.md) — Detalhes da API
- [docs/learning.md](docs/learning.md) — Evolução e aprendizado

## Licença

Este projeto é um exercício educacional para o desafio DIO.
