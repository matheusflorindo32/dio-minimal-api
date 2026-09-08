# Documentação da API

## Base URL

- **Desenvolvimento (perfil HTTP):** `http://localhost:5004`
- **Docker:** `http://localhost:8080`
- **Swagger UI:** `{baseUrl}/swagger`

## Autenticação e roles

Todos os endpoints, exceto `/auth/login` e `/auth/register`, requerem JWT Bearer:

```http
Authorization: Bearer <token>
```

Roles utilizadas:

- `Admin`: operações administrativas, update/delete e gerenciamento de usuários/categorias;
- `Editor`: leitura autenticada e criação de livros.

### Regra de segurança do registro público

`POST /auth/register` **sempre cria o usuário como `Editor`**. O valor de `role` recebido no contrato legado não é confiado para conceder privilégio. Isso impede que um cliente anônimo se cadastre como `Admin`.

Para demonstrar operações administrativas em desenvolvimento, utilize a conta seed:

- email: `admin@bookstore.com`
- senha: `Admin@123`

## Authentication

### POST /auth/register

Registra um novo usuário `Editor` e retorna JWT.

```json
{
  "email": "user@example.com",
  "password": "MinSixChars",
  "name": "User Name",
  "role": 0
}
```

Mesmo no exemplo acima, `role: 0` **não concede Admin**; a resposta deve reportar `Editor`.

Respostas: `201`, `400`, `409`.

### POST /auth/login

```json
{
  "email": "user@example.com",
  "password": "MinSixChars"
}
```

Respostas: `200`, `400`, `401`.

## Categories

| Método | Rota | Autorização | Respostas principais |
|---|---|---|---|
| GET | `/categories` | autenticado | 200, 401 |
| GET | `/categories/{id}` | autenticado | 200, 401, 404 |
| POST | `/categories` | Admin | 201, 400, 401, 403, 409 |
| PUT | `/categories/{id}` | Admin | 200, 400, 401, 403, 404, 409 |
| DELETE | `/categories/{id}` | Admin | 204, 401, 403, 404, 409 |

Uma categoria com livros associados não pode ser removida.

## Books

### GET /books

Requer autenticação. Query params:

- `page`: default 1;
- `pageSize`: default 10, limitado a 1–50;
- `title`: filtro parcial;
- `author`: filtro parcial.

Exemplo de resposta:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalCount": 0,
  "totalPages": 0
}
```

### POST /books

Roles: `Admin,Editor`.

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

Validações:

- título obrigatório, máximo 200 caracteres;
- autor obrigatório, máximo 150;
- ISBN obrigatório, 10–13 caracteres e único;
- ano entre 1450 e ano atual + 1;
- preço >= 0;
- estoque >= 0;
- categoria deve existir.

Respostas: `201`, `400`, `401`, `403`, `409`.

### Demais endpoints de livros

| Método | Rota | Autorização | Respostas principais |
|---|---|---|---|
| GET | `/books/{id}` | autenticado | 200, 401, 404 |
| PUT | `/books/{id}` | Admin | 200, 400, 401, 403, 404, 409 |
| DELETE | `/books/{id}` | Admin | 204, 401, 403, 404 |

## Users

| Método | Rota | Autorização |
|---|---|---|
| GET | `/users` | Admin |
| GET | `/users/{id}` | Admin |

## Erros de validação

A API usa uma representação consistente com informações de Problem Details, incluindo `type`, `title`, `status`, `detail`, `instance` e `errors`.

Exemplo:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Invalid book data.",
  "instance": "/books",
  "errors": [
    "ISBN must be between 10 and 13 characters."
  ]
}
```

Para uma sequência prática de requests, consulte [`../requests.http`](../requests.http).
