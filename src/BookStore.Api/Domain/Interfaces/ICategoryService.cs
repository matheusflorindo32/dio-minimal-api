using BookStore.Api.Domain.Entities;

namespace BookStore.Api.Domain.Interfaces;

public interface ICategoryService
{
    List<Category> GetAll();
    Category? GetById(int id);
    Category Create(Category category);
    Category Update(Category category);
    void Delete(Category category);
    bool NameExists(string name, int? excludeId = null);
    bool HasBooks(int id);
}
