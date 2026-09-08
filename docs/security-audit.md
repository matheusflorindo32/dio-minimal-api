# Auditoria de Segurança — BookStore API

**Data:** 07/09/2026  
**Baseline:** `af5f3a06f30a74cbb3cb96ed79ad819e2f0aca0c`  
**Escopo:** autenticação, autorização, entrada, dados, configuração, Docker e CI

## Resumo executivo

A auditoria atual substitui o relatório anterior que declarava 31/31 controles aprovados. A revisão Red Team encontrou uma falha real de autorização na baseline: o registro público aceitava a role enviada pelo cliente, permitindo autoatribuição de `Admin`.

Essa falha foi corrigida e recebeu teste de integração específico. Portanto, a segurança final é avaliada pelo comportamento corrigido, e não pela antiga declaração de “100%”.

## Finding principal

| ID | Severidade | Finding | Correção | Prova |
|---|---|---|---|---|
| SEC-01 | Alta / P0 no escopo | `/auth/register` confiava em `request.Role`; cliente anônimo podia pedir `Admin` | registro público força `Editor` | teste `Register_RequestingAdmin_Returns201AsEditorWithToken` |

## Controles verificados no código

### Autenticação

- senhas não são persistidas em texto puro;
- salt aleatório gerado com `RandomNumberGenerator`;
- login retorna `401` genérico para credencial inválida;
- JWT valida issuer, audience, lifetime e signing key;
- token inclui role usada pelo authorization middleware;
- `PasswordHash` não integra DTOs de resposta.

### Autorização

- `/auth/login` e `/auth/register` são públicos;
- Books e Categories exigem autenticação no grupo;
- criação de livro aceita `Admin,Editor`;
- update/delete de livros exigem `Admin`;
- CRUD administrativo de categorias exige `Admin`;
- Users exige `Admin`;
- registro público não pode criar Admin após SEC-01.

### Entrada e integridade

- IDs usam constraints `{id:int}`;
- paginação limita `pageSize` a 50;
- ISBN tem restrição 10–13 caracteres e unique index;
- email e nome possuem validação básica;
- Category.Name e User.Email possuem unique index;
- Book → Category usa FK com `DeleteBehavior.Restrict`.

### Infraestrutura

- `.env` é ignorado;
- `.env.example` contém apenas valores de exemplo;
- container roda como usuário não-root;
- Docker usa build multi-stage;
- JWT do Compose pode ser sobrescrito por variável de ambiente;
- CI usa `contents: read` e não precisa de credenciais de escrita;
- CI executa verificação de pacotes NuGet vulneráveis e build Docker.

## Limitações residuais declaradas

| Limitação | Classificação | Decisão |
|---|---|---|
| HMACSHA256 + salt não é KDF lenta dedicada a passwords | Média | aceitável apenas no escopo educacional; migrar para PBKDF2/BCrypt/Argon2 em produção |
| CORS permissivo | Média | mantido para demonstração local; restringir por origem em produção |
| Sem rate limiting em login/register | Média | roadmap; não necessário para cumprir o desafio |
| Swagger disponível no ambiente de demonstração | Baixa | intencional para avaliação do projeto |
| Sem refresh token | Baixa | fora do escopo |
| Sem deployment HTTPS/HSTS | Baixa | projeto local/container, não apresentado como produção |

## Secrets

Nenhuma credencial de produção é necessária para executar o projeto. A conta `admin@bookstore.com / Admin@123` e a chave padrão de demonstração são explicitamente dados de **desenvolvimento**, não devem ser reutilizados em ambiente real.

## Veredicto de segurança

- **Baseline:** NO-GO devido a SEC-01.
- **Branch corrigida:** SEC-01 corrigido e coberto por integração.
- **Critical/High blockers abertos conhecidos:** 0 após a correção.
- **Production-ready:** NÃO — e o projeto não faz essa alegação.

A aprovação global ainda depende do quality gate final do GitHub Actions para o SHA certificado.
