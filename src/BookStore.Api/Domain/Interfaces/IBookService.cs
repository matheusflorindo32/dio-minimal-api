using BookStore.Api.Domain.Entities;

namespace BookStore.Api.Domain.Interfaces;

public interface IBookService
{
    (List<Book> Items, int TotalCount) GetAll(int page, int pageSize, string? title, string? author);
    Book? GetById(int id);
    Book Create(Book book);
    Book Update(Book book);
    void Delete(Book book);
    bool IsbnExists(string isbn, int? excludeId = null);
}
