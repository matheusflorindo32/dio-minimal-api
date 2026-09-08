# Auditoria independente — status baseado em evidências

**Data:** 07/09/2026  
**Baseline auditada:** `af5f3a06f30a74cbb3cb96ed79ad819e2f0aca0c`  
**Branch de correção:** `fix/premium-elite-audit`

> Este documento substitui a auditoria anterior que atribuía nota 9,0/10 sem conseguir executar o build. A certificação atual só considera evidência produzida por código, testes e GitHub Actions.

## Achados confirmados na baseline

| ID | Achado | Prioridade | Evidência/impacto | Estado |
|---|---|---|---|---|
| A-01 | Build quebrado por uso de `.WithOpenApi()` sem pacote correspondente | P0 | CI da `main` falhava com 14 erros CS1061 | corrigido na branch |
| A-02 | Projeto de integração fora da solution | P0 | restore/build da solution não compilava a suíte de integração | corrigido |
| A-03 | IntegrationTests usava EF InMemory sem o pacote | P0 | compilação falhava assim que o projeto passou a entrar na solution | corrigido |
| A-04 | Teste de paginação dependia do seed e esperava contagem incorreta | P1 | 30/31 testes unitários passavam | corrigido com isolamento |
| A-05 | Registro público permitia solicitar `Admin` | P0 Segurança | elevação de privilégio anônima | corrigido + teste de integração |
| A-06 | Credencial Admin documentada não correspondia ao hash seed | P1 | login de demonstração inconsistente | corrigido |
| A-07 | Startup chamava `Migrate()` sem migrations versionadas | P1 | fresh clone poderia não inicializar o schema como documentado | corrigido com `EnsureCreated()` para este escopo educacional |
| A-08 | README/requests usavam porta 5000 enquanto launch profile usa 5004 | P1 DX | onboarding incorreto | corrigido |
| A-09 | Healthcheck Docker dependia de `curl` ausente | P1 DevOps | container poderia ficar unhealthy | corrigido |
| A-10 | Compose usava campo `version` obsoleto | P2 | warning e ruído operacional | corrigido |

## Revisão Backend

Pontos positivos mantidos:

- Minimal APIs organizadas em route groups;
- DTOs separados das entidades;
- EF Core com constraints e relacionamento Book → Category;
- paginação e filtros;
- sem camadas artificiais somente para aumentar complexidade.

Pontos residuais não bloqueantes:

- `UserService` usa HMACSHA256 com salt, não uma KDF lenta específica para armazenamento de senha;
- a solução é propositalmente monolítica e educacional.

## Revisão Security / Red Team

O principal blocker encontrado foi a possibilidade de autoatribuição de `Admin` pelo endpoint público de registro. A correção força qualquer registro público para `Editor` e existe teste de integração que envia `role=0` e exige retorno `Editor`.

Limitações assumidas e documentadas:

- sem rate limiting;
- CORS permissivo para facilitar execução educacional;
- Swagger disponível no ambiente do exercício;
- estratégia de senha não é apresentada como production-grade.

Esses itens ficam fora do escopo obrigatório do desafio e não justificam adicionar infraestrutura desproporcional.

## Revisão QA

O processo de auditoria mostrou por que quantidade de arquivos de teste não é suficiente: na baseline, IntegrationTests existia mas não era compilado pela solution. Após corrigir isso, o CI passou a revelar problemas reais nos fixtures e no contrato.

Critério final: somente considerar a suíte aprovada quando GitHub Actions registrar build + unit tests + integration tests com sucesso no mesmo SHA.

## Revisão DevOps

O quality gate foi ampliado para verificar:

1. restore;
2. build Release;
3. unit tests;
4. integration tests;
5. pacotes NuGet vulneráveis;
6. `docker compose config`;
7. `docker build`;
8. artifacts `.trx`.

## Veredicto

A baseline `af5f3a06...` é **NO-GO** e a antiga nota 9,0/10 não deve ser usada como certificação.

O veredicto final da branch corrigida deve ser lido em `docs/delivery-report.md` e depende do resultado real do GitHub Actions para o SHA final.
