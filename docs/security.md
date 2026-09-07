# Práticas de Segurança

## 1. Autenticação

- **JWT Bearer** com chave simétrica (HMAC-SHA256)
- Token expira em 8 horas (`DateTime.UtcNow.AddHours(8)`)
- Validação completa: Issuer, Audience, Lifetime, SigningKey
- Chave JWT configurável via `appsettings.json` ou variável de ambiente
- Chave padrão marcada como "CHANGE-THIS-KEY-IN-PRODUCTION"

## 2. Senhas

- **Nunca armazenadas em texto puro**
- Hash com HMACSHA256 + salt aleatório de 16 bytes
- Formato de armazenamento: `base64(salt).base64(hash)`
- Salt gerado com `RandomNumberGenerator.GetBytes()` (CSPRNG)

### Limitação declarada

HMACSHA256 não é um KDF lento (como BCrypt/Argon2). Para produção com muitos usuários, recomenda-se migrar para `BCrypt.Net-Next` ou `Konscious.Security.Cryptography` (Argon2). A estrutura do código (`BCryptHash`/`BCryptVerify`) facilita essa troca.

## 3. Autorização

- **Role-based authorization** com 2 níveis: Admin e Editor
- Roles aplicadas via `[Authorize(Roles = "Admin")]` nos endpoints
- Endpoints de leitura requerem apenas autenticação
- Operações de escrita/exclusão requerem role Admin
- Criação de livros permite Admin e Editor

### Matriz de Permissões

| Operação | Admin | Editor | Não autenticado |
|---|---|---|---|
| Login/Register | ✓ | ✓ | ✓ |
| Listar livros/categorias | ✓ | ✓ | ✗ |
| Criar livro | ✓ | ✓ | ✗ |
| Editar/Excluir livro | ✓ | ✗ | ✗ |
| CRUD categorias | ✓ | ✗ | ✗ |
| Listar usuários | ✓ | ✗ | ✗ |

## 4. Proteção de Dados

- **CORS** configurado (aberto para desenvolvimento, restringir em produção)
- **Unique constraints** no banco: Email, ISBN, Nome de Categoria
- **Foreign key com Restrict**: não permite deletar categoria com livros associados
- **Input trimming**: campos de texto são trimados antes de persistir
- **Validação de comprimento**: todos os campos string têm limite máximo

## 5. O que NÃO está no repositório

- Senhas reais ou chaves de produção
- Connection strings de ambientes reais
- Arquivos `.env` (apenas `.env.example` com valores placeholder)
- Banco de dados SQLite (gerado automaticamente)

## 6. Docker

- Imagem de runtime usa **non-root user** (`appuser`)
- Build em multi-stage (SDK não vai para produção)
- JWT key passada via variável de ambiente no `docker-compose.yml`

## 7. Recomendações para Produção

Se este projeto fosse para produção, as seguintes melhorias seriam necessárias:

1. Migrar hash de senha para BCrypt ou Argon2
2. Adicionar rate limiting nos endpoints de login/register
3. Restringir CORS para domínios específicos
4. Usar HTTPS com certificado válido
5. Armazenar JWT key em secret manager (Azure Key Vault, AWS Secrets Manager)
6. Adicionar logging estruturado (Serilog)
7. Implementar refresh tokens
8. Migrar para PostgreSQL ou SQL Server
9. Adicionar health checks detalhados
10. Implementar auditoria de ações (quem criou/editou/deletou)
