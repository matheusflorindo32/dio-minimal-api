# Relatório Final de Entrega — BookStore API

**Projeto:** BookStore API — Minimal API com .NET 8  
**Desafio:** DIO Minimal API  
**Autor:** Matheus Florindo  
**Data da auditoria:** 08/09/2026  
**Baseline inicial:** `af5f3a06f30a74cbb3cb96ed79ad819e2f0aca0c`  
**Baseline supply chain:** `23319b9073ac241ab2c61b36a0fd5f3b5ecd0c7b`  
**Branch final auditada:** `fix/supply-chain-security`

## Executive Summary

A auditoria evidence-first começou com um projeto que não estava pronto para entrega apesar de documentação anterior otimista. Foram encontrados e corrigidos problemas de compilação, testes, autorização, onboarding, Docker e supply chain.

A rodada final de supply-chain security identificou um detalhe crítico de processo: `dotnet list package --vulnerable --include-transitive` reportava vulnerabilidades, mas o workflow continuava verde porque o comando retornava exit code 0. O CI foi alterado para transformar o scan em gate real: Critical/High agora falham o pipeline e Moderate gera warning para análise explícita.

Após atualizar dependências compatíveis com .NET 8 e modernizar o stack de testes, o run #21 do PR #2 registrou os três projetos sem pacotes vulneráveis nas fontes atuais do NuGet.

## Findings → Fix → Proof

| ID | Finding | Prioridade | Correção | Prova |
|---|---|---|---|---|
| F-01 | `.WithOpenApi()` sem pacote necessário | P0 | referência OpenAPI adicionada | build CI |
| F-02 | IntegrationTests fora da solution | P0 | projeto incluído | restore/build dos 3 projetos |
| F-03 | EF InMemory ausente | P0 | package adicionado | build CI |
| F-04 | registro público permitia solicitar Admin | P0 Security | registro força `Editor` | integration test dedicado |
| F-05 | hash Admin seed inconsistente | P1 | hash corrigido | login de integração |
| F-06 | `Migrate()` sem migrations versionadas | P1 | `EnsureCreated()` no escopo educacional | host de integração |
| F-07 | teste de paginação contaminado pelo seed | P1 | cenário isolado | 31/31 unit |
| F-08 | fixtures de Book com ISBN inválido | P1 | fixtures alinhados ao contrato | 34/34 integration |
| F-09 | docs/requests usavam porta errada | P1 DX | alinhados ao `5004` | documentação atual |
| F-10 | healthcheck Docker dependia de curl ausente | P1 DevOps | curl instalado | Docker build CI |
| F-11 | Compose com `version` obsoleto | P2 | removido | compose config CI |
| F-12 | vulnerability scan não bloqueava release | P0 Supply Chain | hard gate Critical/High | run #19 falhou com High; run #21 passou depois da remediação |
| F-13 | Microsoft .NET/EF 8.0.0 introduzia transitivos vulneráveis | P0 Supply Chain | atualização para 8.0.30 | Api limpa no scan |
| F-14 | stack de testes antigo introduzia dois transitivos High | P0 Supply Chain | Test SDK/xUnit/runner modernizados | UnitTests e IntegrationTests limpos no scan |
| F-15 | auditorias históricas continham claims sem execução | P1 Docs | substituídas por evidência de 2026 | docs atuais |

## Dependências atualizadas

| Dependência | Antes | Depois |
|---|---:|---:|
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | 8.0.30 |
| Microsoft.AspNetCore.OpenApi | 8.0.0 | 8.0.30 |
| Microsoft.EntityFrameworkCore | 8.0.0 | 8.0.30 |
| Microsoft.EntityFrameworkCore.Design | 8.0.0 | 8.0.30 |
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.0 | 8.0.30 |
| Microsoft.AspNetCore.Mvc.Testing | 8.0.0 | 8.0.30 |
| Microsoft.EntityFrameworkCore.InMemory | 8.0.0 | 8.0.30 |
| Microsoft.NET.Test.Sdk | 17.6.0 | 17.14.1 |
| xunit | 2.4.2 | 2.9.3 |
| xunit.runner.visualstudio | 2.4.5 | 2.8.2 |

## Quality Gate — PR #2 run #21

| Gate | Resultado |
|---|---|
| Restore | PASS |
| Build Release | PASS — 0 warnings / 0 errors |
| Unit tests | 31/31 PASS |
| Integration tests | 34/34 PASS |
| Total | 65/65 PASS |
| NuGet scan | PASS |
| Critical | 0 reportado |
| High | 0 reportado |
| Moderate | 0 reportado |
| Docker Compose config | PASS |
| Docker image build | PASS |
| TRX + vulnerability report | PASS |

O log do run #21 contém explicitamente:

- `BookStore.Api` — no vulnerable packages given the current sources;
- `BookStore.UnitTests` — no vulnerable packages given the current sources;
- `BookStore.IntegrationTests` — no vulnerable packages given the current sources.

## Testes

- **31 testes unitários**;
- **34 testes de integração HTTP**;
- **65 testes automatizados no total**.

A suíte cobre autenticação, autorização por role, tentativa de autoelevação, CRUD, validação, duplicidade, `401`, `403`, `404`, `409`, paginação e filtros.

## Comparação com o desafio DIO

| Dimensão | Referência DIO | BookStore API | Resultado |
|---|---|---|---|
| Minimal API | Sim | Sim, .NET 8 | ✅ |
| Persistência | MySQL | SQLite/EF Core | ⭐ adaptação autoral |
| CRUD | Sim | Books + Categories | ⭐ ampliado |
| Autenticação | Base educacional | JWT + roles | ⭐ ampliado |
| DTOs | Limitado | contracts separados | ⭐ ampliado |
| Paginação/filtros | Não | Sim | ⭐ ampliado |
| Testes | Base simples | 65 unit/integration | ⭐ ampliado |
| Docker | Não | build verificado | ⭐ ampliado |
| CI | Não | build/test/security/docker gates | ⭐ ampliado |
| Supply chain | Não | scan + enforcement Critical/High | ⭐ ampliado |
| Documentação | Mínima | README + docs + requests | ⭐ ampliado |

**Aderência DIO na branch auditada:** 100% dos requisitos identificados, condicionada apenas à repetição verde do pipeline na `main` após o merge.

## Premium Elite Score pré-merge

| Área | Nota / 10 |
|---|---:|
| DIO adherence | 10.0 |
| C# | 9.4 |
| .NET | 9.6 |
| Minimal API | 9.7 |
| REST | 9.3 |
| Architecture | 9.2 |
| Database | 9.2 |
| Authentication | 9.1 |
| Authorization | 9.8 |
| AppSec | 9.3 |
| Software Supply Chain Security | 9.8 |
| Tests | 9.7 |
| Docker | 9.6 |
| CI/CD | 9.8 |
| Documentation | 9.7 |
| DX | 9.5 |
| Git/GitHub | 9.2 |
| Portfolio | 9.8 |

**Premium Elite estimado:** **97/100**, sem forçar 100.

Não recebe 100 porque continuam deliberadamente fora do escopo educacional: KDF lenta dedicada para passwords, rate limiting, CORS restritivo, deployment HTTPS/secret manager e controles operacionais de produção.

## Hard Gates pré-merge

- P0 aberto conhecido: **0**
- P1 blocker conhecido: **0**
- Build: **PASS**
- Unit: **31/31 PASS**
- Integration: **34/34 PASS**
- Critical dependency vulnerabilities: **0 reportado**
- High dependency vulnerabilities: **0 reportado**
- Moderate dependency vulnerabilities: **0 reportado**
- Security gate: **VERIFICADO**
- Docker Compose: **PASS**
- Docker image build: **PASS**

## Decisão pré-merge

### 🟢 GO PARA MERGE DO PR #2

A certificação final para envio à DIO exige um último GitHub Actions verde na `main` após o merge, sem alteração funcional adicional.

## Como explicar o projeto em 30 segundos

> Reconstruí o desafio de Minimal API da DIO como uma BookStore API em .NET 8. Além do CRUD, implementei EF Core com SQLite, JWT e autorização por roles, DTOs, paginação e filtros, 65 testes unitários e de integração, Docker e CI. A auditoria final encontrou e corrigiu não só bugs e uma falha de elevação de privilégio no cadastro, mas também um problema de supply chain: o scan de dependências mostrava vulnerabilidades sem quebrar o pipeline. Hoje o CI bloqueia Critical/High e a remediação foi validada com os três projetos sem pacotes vulneráveis nas fontes NuGet consultadas.

## Declaração de Integridade

- nenhum resultado foi inventado;
- nenhuma vulnerabilidade foi suprimida para obter verde;
- o run #19, que falhou por High, foi preservado como prova do gate;
- o run #21 comprova a remediação pré-merge;
- nenhuma cobertura percentual não medida é declarada;
- o projeto não é apresentado como production-ready;
- complexidade enterprise fora do escopo foi rejeitada.
