# Auditoria Independente — Evidence-First

**Data:** 08/09/2026  
**Baseline inicial:** `af5f3a06f30a74cbb3cb96ed79ad819e2f0aca0c`  
**Baseline supply chain:** `23319b9073ac241ab2c61b36a0fd5f3b5ecd0c7b`  
**Branch final de remediação:** `fix/supply-chain-security`

> Esta auditoria não usa a antiga nota 9,0/10 como evidência. O veredicto é baseado em código, testes e GitHub Actions.

## Revisor A — .NET Engineering

### Findings resolvidos

- `.WithOpenApi()` sem dependência correspondente quebrava a compilação;
- IntegrationTests estava fora da solution;
- provider EF InMemory ausente;
- fixtures e teste de paginação continham premissas incorretas;
- pacotes Microsoft 8.0.0 estavam desatualizados e introduziam transitivos vulneráveis;
- stack de testes antigo introduzia `System.Net.Http 4.3.0` e `System.Text.RegularExpressions 4.3.0` com advisories High.

### Estado verificado

No run #21 do PR #2:

- Release build: PASS, 0 warnings / 0 errors;
- 31/31 unit tests: PASS;
- 34/34 integration tests: PASS;
- total: 65/65;
- .NET permanece em `net8.0`;
- nenhuma mudança arquitetural desproporcional foi introduzida.

**Conclusão do Revisor A:** APROVADO para o escopo educacional.

## Revisor B — Application / Supply Chain Security

### Findings resolvidos

- autoatribuição pública de `Admin` foi eliminada e coberta por integração;
- vulnerability scan que antes era apenas informativo foi convertido em hard gate para Critical/High;
- run #19 provou que o gate falha quando High existe;
- após atualizar dependências, run #21 informou que Api, UnitTests e IntegrationTests não possuem pacotes vulneráveis nas fontes NuGet consultadas.

### Limitações não bloqueantes

- hashing de senha usa HMACSHA256 + salt, não KDF lenta dedicada;
- CORS permissivo para demonstração;
- sem rate limiting;
- sem deployment production-grade.

Esses itens permanecem documentados e não são apresentados como segurança de produção.

**Conclusão do Revisor B:** ZERO Critical/High conhecidos no scan atual; APROVADO para o desafio DIO.

## Revisor C — DIO / Portfolio

O projeto mantém o objetivo do desafio e acrescenta, sem converter o exercício em arquitetura enterprise:

- Minimal API em .NET 8;
- EF Core + SQLite;
- JWT + roles;
- DTOs;
- paginação/filtros;
- 65 testes automatizados;
- Docker;
- CI com supply-chain gate;
- documentação de arquitetura, segurança e aprendizado.

A documentação evita claim `production-ready` e distingue qualidade educacional de requisitos de produção.

**Conclusão do Revisor C:** aderência DIO integral e forte valor de portfólio.

## Consolidação independente

| Área | Resultado |
|---|---|
| .NET / Build | PASS |
| Unit tests | 31/31 PASS |
| Integration tests | 34/34 PASS |
| AppSec blocker conhecido | 0 |
| NuGet Critical | 0 conhecido no run #21 |
| NuGet High | 0 conhecido no run #21 |
| NuGet Moderate | 0 reportado no run #21 |
| Security gate | PASS e comportamento de falha verificado no run #19 |
| Docker Compose | PASS |
| Docker image build | PASS |
| Overengineering | NÃO identificado |

## Veredicto independente pré-merge

### 🟢 GO PARA MERGE DO PR #2

O projeto cumpre os hard gates na branch de remediação. A aprovação definitiva para entrega à DIO exige apenas repetir o mesmo pipeline com sucesso no commit final da `main` após o merge.
