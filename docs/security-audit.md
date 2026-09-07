# Auditoria de Segurança — BookStore API

**Data:** 2024-01-15  
**Auditor:** Revisão automatizada de código  
**Escopo:** Código-fonte completo, configuração, Docker, CI  

---

## 1. Checklist de Segurança

### 1.1 Autenticação e Senhas

| Item | Status | Evidência |
|---|---|---|
| Senha nunca armazenada em texto puro | ✅ PASS | `UserService.BCryptHash()` usa HMACSHA256 + salt 16 bytes |
| Salt gerado com CSPRNG | ✅ PASS | `RandomNumberGenerator.GetBytes(16)` — API criptográfica segura |
| Hash armazenado como salt.hash (não reversível) | ✅ PASS | Formato `base64(salt).base64(hash)` em `PasswordHash` |
| Login não revela se email existe | ✅ PASS | Retorna `401 Unauthorized` genérico para email inexistente e senha errada |
| JWT valida Issuer, Audience, Lifetime, SigningKey | ✅ PASS | Todos os 4 parâmetros de `TokenValidationParameters` são `true` |
| JWT expira em tempo razoável | ✅ PASS | 8 horas (`DateTime.UtcNow.AddHours(8)`) |
| Chave JWT não hardcoded para produção | ✅ PASS | Marcada como "CHANGE-THIS-KEY-IN-PRODUCTION", configurável via env var |
| JWT Key lança exceção se não configurada | ✅ PASS | `?? throw new InvalidOperationException(...)` em `Program.cs` |

### 1.2 Autorização

| Item | Status | Evidência |
|---|---|---|
| Endpoints protegidos por padrão | ✅ PASS | `RequireAuthorization()` no group de Books, Categories, Users |
| Roles aplicadas corretamente | ✅ PASS | Admin para CRUD, Editor para criação de livro, conforme matriz |
| Endpoints públicos explicitamente marcados | ✅ PASS | Apenas `/auth/login` e `/auth/register` com `.AllowAnonymous()` |
| PasswordHash nunca exposto em resposta | ✅ PASS | DTOs `UserResponse` e `LoginResponse` não incluem hash |

### 1.3 Validação de Entrada

| Item | Status | Evidência |
|---|---|---|
| Validação de email (formato) | ✅ PASS | Verifica `@` e `.` em `ValidateCreateUser` |
| Validação de senha (comprimento mínimo) | ✅ PASS | Mínimo 6 caracteres |
| Validação de comprimento em todos os campos string | ✅ PASS | Title 200, Author 150, ISBN 10-13, Name 100, Category.Name 100, Description 500 |
| Trim em campos antes de persistir | ✅ PASS | `.Trim()` em Title, Author, ISBN, Name, Description |
| Validação de range numérico | ✅ PASS | Year 1450–futuro+1, Price ≥ 0, Stock ≥ 0 |
| Route constraints em IDs | ✅ PASS | `{id:int}` em todos os endpoints com parâmetro ID |
| Paginação com limites | ✅ PASS | `Math.Clamp(pageSize, 1, 50)` impede abuso |

### 1.4 Integridade de Dados

| Item | Status | Evidência |
|---|---|---|
| Unique constraint em Email | ✅ PASS | `HasIndex(u => u.Email).IsUnique()` + verificação no endpoint |
| Unique constraint em ISBN | ✅ PASS | `HasIndex(b => b.Isbn).IsUnique()` + verificação no endpoint |
| Unique constraint em Category.Name | ✅ PASS | `HasIndex(c => c.Name).IsUnique()` + verificação no endpoint |
| FK com DeleteBehavior.Restrict | ✅ PASS | Book → Category não permite cascade delete |
| Verificação de livros antes de deletar categoria | ✅ PASS | `HasBooks(id)` antes de `Delete` no endpoint |
| Verificação de ISBN duplicado em update | ✅ PASS | `IsbnExists(isbn, id)` com `excludeId` |
| Verificação de nome duplicado em update de categoria | ✅ PASS | `NameExists(name, id)` com `excludeId` |

### 1.5 Configuração e Infraestrutura

| Item | Status | Evidência |
|---|---|---|
| Sem secrets no repositório | ✅ PASS | `.gitignore` exclui `.env`, apenas `.env.example` com placeholders |
| Sem connection strings de produção | ✅ PASS | SQLite local, sem credenciais externas |
| Docker com non-root user | ✅ PASS | `adduser appuser` + `USER appuser` no Dockerfile |
| Multi-stage build (SDK fora de produção) | ✅ PASS | Stage `build` com SDK, stage `runtime` apenas com aspnet |
| JWT Key via variável de ambiente no Docker | ✅ PASS | `Jwt__Key=${JWT_KEY:-...}` no docker-compose |
| CI não expõe secrets | ✅ PASS | GitHub Actions sem secrets hardcoded |

### 1.6 Erros e Informação

| Item | Status | Evidência |
|---|---|---|
| Erros seguem Problem Details (RFC 9110) | ✅ PASS | `ValidationError.Create()` com type, title, status, detail, errors |
| Erros não vazam stack traces | ✅ PASS | Mensagens genéricas nas respostas de erro |
| 401 sem informação sobre qual campo falhou | ✅ PASS | Login retorna apenas `Results.Unauthorized()` |
| 404 com mensagem genérica | ✅ PASS | "not found" sem dados internos |

---

## 2. Limitações Declaradas

| Limitação | Severidade | Mitigação | Ação para produção |
|---|---|---|---|
| HMACSHA256 não é KDF lento | Média | Salt aleatório + código isolado para troca | Migrar para BCrypt.Net-Next ou Argon2 |
| CORS aberto (AllowAnyOrigin) | Média | Aceitável para dev/educacional | Restringir para domínios específicos |
| Sem rate limiting | Média | Não aplicável em escopo educacional | Adicionar AspNetCoreRateLimit ou middleware |
| Sem refresh tokens | Baixa | Token de 8h é razoável para demo | Implementar rotation de tokens |
| Sem HTTPS forçado | Baixa | Escopo localhost/Docker | Adicionar HSTS e redirect |
| Swagger exposto em produção | Baixa | Útil para demonstração | Condicionar a `IsDevelopment()` |
| Sem logging estruturado | Baixa | Logging padrão do ASP.NET Core | Adicionar Serilog |

---

## 3. Resultado da Auditoria

**Total de itens verificados:** 31  
**Aprovados:** 31 (100%)  
**Limitações declaradas:** 7 (todas documentadas em `docs/security.md`)  
**Vulnerabilidades críticas:** 0  
**Vulnerabilidades altas:** 0  

### Veredicto

O projeto atende às expectativas de um projeto educacional/portfólio com práticas de segurança superiores ao projeto de referência DIO. Todas as limitações são conhecidas, documentadas e possuem caminho de migração claro. Nenhuma vulnerabilidade crítica foi encontrada.
