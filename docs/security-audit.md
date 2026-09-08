# Auditoria de Segurança — BookStore API

**Data:** 08/09/2026  
**Baseline inicial:** `af5f3a06f30a74cbb3cb96ed79ad819e2f0aca0c`  
**Baseline supply chain:** `23319b9073ac241ab2c61b36a0fd5f3b5ecd0c7b`  
**Branch de remediação:** `fix/supply-chain-security`  
**Escopo:** autenticação, autorização, entrada, dados, dependências NuGet, Docker e CI

## Resumo executivo

A auditoria evidence-first encontrou dois grupos de problemas relevantes antes da certificação final:

1. uma falha de autorização no registro público, que permitia ao cliente solicitar a role `Admin`;
2. dependências transitivas com advisories High/Moderate que eram apenas exibidas pelo CI, sem fazer o workflow falhar.

A falha de autorização foi corrigida no PR #1 e recebeu teste de integração dedicado. A remediação de supply chain foi executada no PR #2: os pacotes Microsoft da linha .NET 8 foram atualizados para 8.0.30, o stack de testes foi modernizado e o CI passou a falhar explicitamente se detectar dependência NuGet Critical ou High.

## Findings confirmados

| ID | Severidade | Finding | Correção | Prova |
|---|---|---|---|---|
| SEC-01 | High / P0 | `/auth/register` confiava em `request.Role`; cliente anônimo podia pedir `Admin` | registro público força `Editor` | teste `Register_RequestingAdmin_Returns201AsEditorWithToken` |
| SEC-02 | High / P0 de release | dependências transitivas vulneráveis eram reportadas pelo NuGet, mas o comando retornava exit code 0 e a CI ficava verde | pacotes .NET 8 atualizados para 8.0.30 e stack de testes modernizado | run #21 reporta os 3 projetos sem pacotes vulneráveis nas fontes atuais |
| SEC-03 | High / P0 de CI | vulnerability scan não era um gate real | workflow captura relatório e falha em `Critical`/`High`; `Moderate` gera warning | run #19 falhou propositalmente ao encontrar dois High nos testes; run #21 passou após a correção |

## Evidência automatizada — PR #2 run #21

- Restore: **PASS**
- Build Release: **PASS** — 0 warnings / 0 errors
- Unit Tests: **31/31 PASS**
- Integration Tests: **34/34 PASS**
- Total: **65/65 PASS**
- NuGet vulnerability gate: **PASS**
- `BookStore.Api`: **no vulnerable packages given the current sources**
- `BookStore.UnitTests`: **no vulnerable packages given the current sources**
- `BookStore.IntegrationTests`: **no vulnerable packages given the current sources**
- Docker Compose config: **PASS**
- Docker image build: **PASS**
- TRX + vulnerability report artifact: **PASS**

## Dependências remediadas

| Grupo | Antes | Depois |
|---|---|---|
| ASP.NET Core Authentication/OpenAPI | 8.0.0 | 8.0.30 |
| EF Core / Design / SQLite | 8.0.0 | 8.0.30 |
| ASP.NET Core Mvc.Testing | 8.0.0 | 8.0.30 |
| EF Core InMemory de testes | 8.0.0 | 8.0.30 |
| Microsoft.NET.Test.Sdk | 17.6.0 | 17.14.1 |
| xUnit | 2.4.2 | 2.9.3 |
| xunit.runner.visualstudio | 2.4.5 | 2.8.2 |

A atualização eliminou os transitivos High observados anteriormente, incluindo `System.Net.Http 4.3.0` e `System.Text.RegularExpressions 4.3.0` nos projetos de teste, sem regressão funcional.

## Controles de aplicação mantidos

### Autenticação

- senhas não são persistidas em texto puro;
- salt aleatório gerado com `RandomNumberGenerator`;
- login retorna `401` genérico para credencial inválida;
- JWT valida issuer, audience, lifetime e signing key;
- `PasswordHash` não integra DTOs de resposta.

### Autorização

- `/auth/login` e `/auth/register` são públicos;
- Books e Categories exigem autenticação;
- criação de livro aceita `Admin,Editor`;
- update/delete de livros exigem `Admin`;
- CRUD administrativo de categorias e Users exige `Admin`;
- registro público não pode criar Admin.

### Infraestrutura e supply chain

- `.env` é ignorado e `.env.example` usa valores de demonstração;
- container roda como usuário não-root e usa build multi-stage;
- CI usa `contents: read`;
- CI compila, executa 65 testes, aplica gate Critical/High, valida Compose, constrói Docker e publica artifacts;
- nenhuma suppression de advisory foi usada.

## Limitações residuais declaradas

| Limitação | Classificação | Decisão |
|---|---|---|
| HMACSHA256 + salt não é KDF lenta dedicada a passwords | Medium | aceitável somente no escopo educacional; PBKDF2/BCrypt/Argon2 seria obrigatório para produção |
| CORS permissivo | Medium | mantido para demonstração local; restringir por origem em produção |
| Sem rate limiting em login/register | Medium | roadmap; não bloqueia o desafio educacional |
| Swagger disponível no ambiente de demonstração | Low | intencional para avaliação |
| Sem refresh token | Low | fora do escopo |
| Sem deployment HTTPS/HSTS/secret manager | Low | projeto não é apresentado como production-ready |

## Veredicto de segurança pré-merge do PR #2

- Critical conhecidos em dependências: **0**
- High conhecidos em dependências: **0**
- Moderate reportados pelo scan do run #21: **0**
- Blockers de autorização conhecidos: **0**
- Vulnerability gate real: **VERIFICADO** — falhou no run #19 e passou no run #21 após remediação
- Production-ready: **NÃO** — e o projeto não faz essa alegação

A certificação definitiva depende do PR #2 ser mergeado sem mudanças regressivas e de uma nova execução verde na `main`.
