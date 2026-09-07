# BookStore API — Minimal API com .NET 8

[![CI](https://github.com/matheusflorindo32/dio-minimal-api/actions/workflows/ci.yml/badge.svg)](https://github.com/matheusflorindo32/dio-minimal-api/actions)

API REST para gerenciamento de catálogo de livraria, desenvolvida como evolução autoral do desafio **Minimal API** da DIO. O projeto aplica ASP.NET Core Minimal APIs, EF Core, autenticação JWT, autorização por roles, testes automatizados, Docker e CI com foco em simplicidade profissional e reprodutibilidade.

> **Referência oficial DIO:** https://github.com/digitalinnovationone/minimal-api

## O que foi evoluído

| Aspecto | Referência DIO | Este projeto |
|---|---|---|
| Runtime | .NET 7 | .NET 8 |
| Banco | MySQL/Pomelo | SQLite para execução local simples |
| Senhas | Exemplo educacional simples | Hash HMACSHA256 + salt aleatório* |
| DTOs | Estrutura original | Requests/Responses separados |
| Validação | Básica | Validação consistente + respostas de erro |
| Paginação | Não | `PagedResponse<T>` |
| Filtros | Não | Título e autor |
| Domínio | Base original | Books + Categories + Users |
| Testes | Suite original | xUnit unitário + integração HTTP |
| Docker | Não | Multi-stage, usuário não-root e volume SQLite |
| CI | Não | Restore, build, testes, dependências e Docker |
| Autorização | Base educacional | JWT + Admin/Editor com enforcement |

\* Adequado ao escopo educacional deste projeto; não é apresentado como estratégia de senha production-grade.

## Stack

- **.NET 8 / C#**
- **ASP.NET Core Minimal APIs**
- **Entity Framework Core 8 + SQLite**
- **JWT Bearer**
- **Swagger / OpenAPI**
- **xUnit + WebApplicationFactory**
- **Docker / Docker Compose**
- **GitHub Actions**

## Quick Start

Pré-requisito: .NET 8 SDK.

```bash
git clone https://github.com/matheusflorindo32/dio-minimal-api.git
cd dio-minimal-api

dotnet restore
dotnet run --project src/BookStore.Api
```

Com o perfil HTTP padrão de desenvolvimento:

- API: `http://localhost:5004`
- Swagger: `http://localhost:5004/swagger`

### Conta administrativa de demonstração

| Email | Senha | Role |
|---|---|---|
| `admin@bookstore.com` | `Admin@123` | Admin |

A conta existe apenas para facilitar a execução educacional local. **O cadastro público sempre cria usuários `Editor`, mesmo que o cliente tente enviar `Admin`**, impedindo autoelevação de privilégio.

## Docker

```bash
docker compose up --build
```

A API fica disponível em:

- `http://localhost:8080`
- `http://localhost:8080/swagger`

O container usa usuário não-root, volume persistente para SQLite e healthcheck HTTP. O CI também valida o Compose e constrói a imagem Docker.

Para encerrar:

```bash
docker compose down
```

## Testes e quality gates

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

Suítes separadas:

```bash
dotnet test tests/BookStore.UnitTests
dotnet test tests/BookStore.IntegrationTests
```

O workflow `.github/workflows/ci.yml` executa:

1. restore;
2. build Release;
3. testes unitários;
4. testes de integração;
5. verificação de pacotes NuGet vulneráveis;
6. `docker compose config`;
7. build real da imagem Docker;
8. publicação dos resultados `.trx`.

O badge no topo reflete o estado atual do workflow da branch principal; não considere documentação substituta para o CI.

## Estrutura

```text
dio-minimal-api/
├── src/
│   └── BookStore.Api/
│       ├── Domain/
│       │   ├── DTOs/
│       │   ├── Entities/
│       │   ├── Enums/
│       │   ├── Interfaces/
│       │   └── Services/
│       ├── Endpoints/
│       ├── Infrastructure/Data/
│       └── Program.cs
├── tests/
│   ├── BookStore.UnitTests/
│   └── BookStore.IntegrationTests/
├── docs/
├── .github/workflows/ci.yml
├── Dockerfile
├── docker-compose.yml
└── requests.http
```

## Contrato principal da API

### Authentication — público

| Método | Rota | Descrição |
|---|---|---|
| POST | `/auth/register` | Registra usuário como `Editor` |
| POST | `/auth/login` | Autentica e retorna JWT |

### Categories — autenticado

| Método | Rota | Role |
|---|---|---|
| GET | `/categories` | autenticado |
| GET | `/categories/{id}` | autenticado |
| POST | `/categories` | Admin |
| PUT | `/categories/{id}` | Admin |
| DELETE | `/categories/{id}` | Admin |

### Books — autenticado

| Método | Rota | Role |
|---|---|---|
| GET | `/books` | autenticado |
| GET | `/books/{id}` | autenticado |
| POST | `/books` | Admin, Editor |
| PUT | `/books/{id}` | Admin |
| DELETE | `/books/{id}` | Admin |

### Users — Admin

| Método | Rota |
|---|---|
| GET | `/users` |
| GET | `/users/{id}` |

## Exemplos de requisição

O arquivo [`requests.http`](requests.http) contém uma sequência executável para login, categorias, livros, filtros e autorização.

## Documentação

- [`docs/architecture.md`](docs/architecture.md) — arquitetura e decisões
- [`docs/api.md`](docs/api.md) — contrato detalhado
- [`docs/security.md`](docs/security.md) — modelo e limitações de segurança
- [`docs/learning.md`](docs/learning.md) — aprendizados do desafio

## O que aprendi

A evolução do projeto reforçou, na prática, que uma Minimal API não significa uma aplicação sem arquitetura. Endpoints pequenos ficam mais fáceis de manter quando contratos, regras de domínio, persistência e respostas HTTP têm responsabilidades claras. Também ficou evidente que testes precisam realmente entrar no build da solution: uma suíte existente no repositório não oferece garantia alguma se o CI nunca a compila ou executa.

Outro aprendizado importante foi tratar segurança e documentação como comportamentos verificáveis. JWT e roles só têm valor quando cenários de `401`, `403` e tentativa de elevação de privilégio são testados. Da mesma forma, README, Docker e CI precisam representar o sistema real — não apenas descrever o que deveria funcionar.

## Escopo

Este é um projeto educacional de portfólio criado para o desafio DIO. Ele demonstra práticas profissionais proporcionais ao exercício, sem alegar ser uma solução production-ready.
