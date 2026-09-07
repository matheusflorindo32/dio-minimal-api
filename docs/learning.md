# Evolução e Aprendizado

Este documento registra as decisões de aprendizado tomadas durante a construção do projeto, comparando com o projeto de referência DIO e explicando o que mudou e por quê.

## 1. De Texto Puro para Hash de Senha

**Referência DIO:** Armazena `Senha` como string em texto puro no banco e no seed. O login compara strings diretamente.

**Este projeto:** Implementa hash com HMACSHA256 + salt aleatório. A senha nunca é armazenada ou comparada em texto puro.

**Aprendizado:** Armazenar senhas em texto puro é a vulnerabilidade mais básica em segurança de aplicações. Mesmo em projetos educacionais, demonstrar a prática correta é fundamental. A implementação usa apenas APIs built-in do .NET (`System.Security.Cryptography`), evitando dependências externas.

## 2. De Entidades Expostas para DTOs

**Referência DIO:** O endpoint `GET /veiculos` retorna a entidade `Veiculo` diretamente, incluindo todos os campos internos.

**Este projeto:** Usa records separados para cada operação (CreateBookRequest, UpdateBookRequest, BookResponse). O PasswordHash do User nunca aparece em nenhuma resposta.

**Aprendizado:** Expor entidades de banco diretamente na API é um anti-pattern que causa over-posting (o cliente pode enviar campos que não deveria) e vazamento de dados internos. DTOs como records em C# são concisos e comunicam claramente o contrato de cada operação.

## 3. De Startup.cs para Minimal API real

**Referência DIO:** Usa o padrão `Host.CreateDefaultBuilder` + `Startup.cs` com `ConfigureServices` e `Configure`. Apesar de se chamar "Minimal API", usa o padrão antigo de hosting.

**Este projeto:** Usa o top-level statement pattern do .NET 8 com `WebApplication.CreateBuilder`, que é o verdadeiro Minimal API hosting model. Os endpoints são organizados em extension methods com `MapGroup`.

**Aprendizado:** A diferença entre "API sem controllers" e "Minimal API" está no hosting model. O padrão moderno (`builder` → `app`) é mais conciso, elimina classes desnecessárias e é o default em .NET 6+.

## 4. De Sem Relacionamentos para Modelo Relacional

**Referência DIO:** As entidades `Administrador` e `Veiculo` são independentes, sem relacionamento entre elas.

**Este projeto:** Book tem FK para Category (1:N), com `DeleteBehavior.Restrict` e unique indexes em Email, ISBN e Category.Name. O endpoint de delete de Category verifica se há livros associados antes de permitir a exclusão.

**Aprendizado:** Relacionamentos, constraints e integridade referencial são fundamentais em qualquer API de produção. Demonstrar que sei configurar FKs, cascatas e validações de integridade antes de operações destrutivas é diferencial em entrevista.

## 5. De Sem Paginação para PagedResponse<T>

**Referência DIO:** `GET /veiculos` retorna todos os registros sem limite.

**Este projeto:** `GET /books` aceita `page` e `pageSize` (clamped entre 1-50) e retorna um `PagedResponse<T>` com metadata de paginação (page, pageSize, totalCount, totalPages).

**Aprendizado:** APIs sem paginação são um problema de performance e UX em qualquer cenário com mais de dezenas de registros. O padrão PagedResponse é reutilizável e demonstra preocupação com escalabilidade.

## 6. De MSTest para xUnit + WebApplicationFactory

**Referência DIO:** Usa MSTest com um mock service e apenas 1 teste de integração (login).

**Este projeto:** Usa xUnit com testes unitários (InMemory DB testando serviços isoladamente) e testes de integração (WebApplicationFactory testando endpoints completos com auth, routing, serialização).

**Aprendizado:** Testes de integração com WebApplicationFactory são o padrão moderno para testar Minimal APIs. Eles exercitam o pipeline completo (middleware, auth, model binding, serialização JSON) sem servidor externo.

## 7. De Sem Docker para Multi-stage Build

**Referência DIO:** Sem containerização.

**Este projeto:** Dockerfile multi-stage (SDK para build, ASP.NET runtime para produção), non-root user, docker-compose com healthcheck e volume para dados.

**Aprendizado:** Docker é requisito de mercado. O multi-stage build mantém a imagem final leve (apenas runtime), e rodar como non-root é uma prática de segurança básica.

## 8. De Sem CI para GitHub Actions

**Referência DIO:** Sem integração contínua.

**Este projeto:** Pipeline que executa restore, build, testes unitários e testes de integração em cada push/PR.

**Aprendizado:** CI automatizado garante que o código no main sempre compila e passa nos testes. É o mínimo esperado em qualquer projeto profissional.

## Resumo: O que esse projeto demonstra em entrevista

1. Sei a diferença entre hash e texto puro para senhas
2. Sei usar DTOs para proteger o contrato da API
3. Conheço o hosting model moderno do .NET 8
4. Sei modelar relacionamentos e integridade referencial
5. Implemento paginação e filtragem
6. Escrevo testes unitários e de integração
7. Containerizo aplicações com boas práticas
8. Configuro CI automatizado
9. Documento decisões técnicas com raciocínio
10. Não copio — construo com entendimento
