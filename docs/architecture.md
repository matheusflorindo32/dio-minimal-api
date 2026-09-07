# Decisões Arquiteturais

## Visão Geral

Este documento registra as decisões de arquitetura tomadas no projeto BookStore API e o raciocínio por trás de cada uma.

## 1. Minimal API vs Controller-based

**Decisão:** Minimal APIs (sem controllers).

**Motivo:** O desafio DIO exige demonstração de Minimal APIs. A abordagem com `MapGroup`, `MapGet`, `MapPost` etc. é mais concisa, alinhada com o .NET 8 e suficiente para o escopo deste projeto. Controllers seriam overengineering aqui.

## 2. SQLite vs MySQL/PostgreSQL

**Decisão:** SQLite com EF Core.

**Motivo:** Elimina dependência de serviço externo. O avaliador do desafio pode clonar e rodar `dotnet run` sem instalar banco de dados. Para um projeto de portfólio educacional, a simplicidade operacional tem mais valor do que features de banco enterprise. A migração para PostgreSQL em produção exige apenas trocar o provider no `Program.cs`.

## 3. Estrutura de Pastas

```
Domain/
├── DTOs/         → Contratos de entrada/saída
├── Entities/     → Modelos de persistência
├── Enums/        → Enumerações do domínio
├── Interfaces/   → Abstrações dos serviços
└── Services/     → Implementação da lógica de negócio

Endpoints/        → Agrupamento de rotas por recurso
Infrastructure/
└── Data/         → DbContext e configuração de persistência
```

**Motivo:** Separação clara entre domínio (regras), infraestrutura (banco) e apresentação (endpoints), sem introduzir camadas que não resolvam um problema concreto. Não há Repository Pattern separado porque o EF Core já é o repositório — adicionar uma abstração sobre outra abstração não traz benefício real neste escopo.

## 4. Password Hashing com HMACSHA256

**Decisão:** HMACSHA256 com salt aleatório de 16 bytes, armazenado como `salt.hash` em Base64.

**Motivo:** Evita dependência de NuGet externo (BCrypt) em um projeto educacional. O HMACSHA256 com salt aleatório é seguro para demonstração. Em produção, o recomendado é migrar para BCrypt ou Argon2 via pacote NuGet. O código foi estruturado para facilitar essa troca (métodos `BCryptHash`/`BCryptVerify` isolados).

**Limitação conhecida:** HMACSHA256 não é um KDF (Key Derivation Function) propositalmente lento como BCrypt. Para um sistema de produção com milhares de usuários, a resistência a brute-force seria inferior.

## 5. DTOs como Records

**Decisão:** Request e Response como `record` em C#.

**Motivo:** Records são imutáveis por padrão, concisos (positional syntax), geram `Equals`/`GetHashCode` automaticamente, e comunicam claramente a intenção de "dados de transporte". Separar Create, Update e Response evita over-posting e mantém contratos explícitos.

## 6. Sem AutoMapper

**Decisão:** Mapping manual com métodos `ToResponse()` estáticos.

**Motivo:** Com 3 entidades e DTOs simples, AutoMapper adicionaria uma dependência e uma camada de configuração sem benefício mensurável. O mapping manual é explícito, rastreável e não esconde bugs de conversão.

## 7. Validação Inline com Problem Details

**Decisão:** Validação em métodos privados nos endpoints, retornando RFC 9110 Problem Details.

**Motivo:** FluentValidation seria overengineering para o número de regras. Os métodos `ValidateBook()`, `ValidateCategory()`, `ValidateCreateUser()` são reutilizáveis dentro do endpoint group e retornam uma lista de erros clara. O formato Problem Details é o padrão HTTP para erros de validação.

## 8. Testes: xUnit + WebApplicationFactory

**Decisão:** Testes unitários com InMemory DB + testes de integração com WebApplicationFactory.

**Motivo:** xUnit é o framework de teste mais adotado no ecossistema .NET moderno. WebApplicationFactory permite testar a aplicação como um todo (middleware, auth, routing, serialização) sem subir servidor real. InMemory DB isola os testes unitários de I/O.
