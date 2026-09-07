# Documentação da API

## Base URL

- **Desenvolvimento:** `http://localhost:5000`
- **Docker:** `http://localhost:8080`
- **Swagger UI:** `{baseUrl}/swagger`

## Autenticação

Todos os endpoints (exceto login e register) requerem um JWT token no header:

```
Authorization: Bearer <token>
```

O token é obtido via `/auth/login` ou `/auth/register`.

## Endpoints

### POST /auth/register

Registra um novo usuário e retorna um JWT token.

**Body:**
```json
{
  "email": "user@example.com",
  "password": "MinSixChars",
  "name": "User Name",
  "role": 0
}
```

- `role`: `0` = Admin, `1` = Editor

**Respostas:**
- `201`: Usuário criado, retorna `LoginResponse` com token
- `400`: Dados inválidos (email, senha < 6 chars, nome vazio)
- `409`: Email já registrado

### POST /auth/login

Autentica e retorna JWT token.

**Body:**
```json
{
  "email": "user@example.com",
  "password": "MinSixChars"
}
```

**Respostas:**
- `200`: Login bem-sucedido, retorna `LoginResponse`
- `400`: Email ou senha vazios
- `401`: Credenciais inválidas

### GET /categories

Lista todas as categorias. Requer autenticação.

**Resposta 200:**
```json
[
  {
    "id": 1,
    "name": "Fiction",
    "description": "Novels and literary works",
    "bookCount": 5
  }
]
```

### GET /categories/{id}

Retorna uma categoria por ID. Requer autenticação.

**Respostas:** `200` (categoria), `401`, `404`

### POST /categories

Cria uma categoria. Requer role Admin.

**Body:**
```json
{
  "name": "Category Name",
  "description": "Optional description"
}
```

**Respostas:** `201`, `400` (validação), `401`, `403`, `409` (nome duplicado)

### PUT /categories/{id}

Atualiza uma categoria. Requer role Admin.

**Body:** Mesmo formato de POST.

**Respostas:** `200`, `400`, `401`, `403`, `404`, `409`

### DELETE /categories/{id}

Remove uma categoria (somente se não tiver livros associados). Requer role Admin.

**Respostas:** `204`, `401`, `403`, `404`, `409` (tem livros)

### GET /books

Lista livros com paginação e filtros opcionais. Requer autenticação.

**Query params:**
- `page` (int, default 1)
- `pageSize` (int, default 10, max 50)
- `title` (string, filtro parcial case-insensitive)
- `author` (string, filtro parcial case-insensitive)

**Resposta 200:**
```json
{
  "items": [
    {
      "id": 1,
      "title": "Clean Code",
      "author": "Robert C. Martin",
      "isbn": "9780132350884",
      "year": 2008,
      "price": 39.99,
      "stock": 15,
      "categoryId": 2,
      "categoryName": "Technology",
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": null
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 2,
  "totalPages": 1
}
```

### GET /books/{id}

Retorna um livro por ID. Requer autenticação.

**Respostas:** `200`, `401`, `404`

### POST /books

Cria um livro. Requer role Admin ou Editor.

**Body:**
```json
{
  "title": "Book Title",
  "author": "Author Name",
  "isbn": "9780000000000",
  "year": 2024,
  "price": 29.99,
  "stock": 10,
  "categoryId": 1
}
```

**Validações:**
- Title: obrigatório, max 200 chars
- Author: obrigatório, max 150 chars
- ISBN: obrigatório, 10-13 chars, único
- Year: entre 1450 e ano atual + 1
- Price: >= 0
- Stock: >= 0
- CategoryId: deve existir

**Respostas:** `201`, `400`, `401`, `403`, `409` (ISBN duplicado)

### PUT /books/{id}

Atualiza um livro. Requer role Admin.

**Body:** Mesmo formato de POST.

**Respostas:** `200`, `400`, `401`, `403`, `404`, `409`

### DELETE /books/{id}

Remove um livro. Requer role Admin.

**Respostas:** `204`, `401`, `403`, `404`

### GET /users

Lista todos os usuários (paginado). Requer role Admin.

**Respostas:** `200`, `401`, `403`

### GET /users/{id}

Retorna um usuário por ID. Requer role Admin.

**Respostas:** `200`, `401`, `403`, `404`

## Formato de Erro (Problem Details)

Erros de validação seguem o padrão RFC 9110:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Invalid book data.",
  "instance": "/books",
  "errors": [
    "Title is required.",
    "ISBN must be between 10 and 13 characters."
  ]
}
```
