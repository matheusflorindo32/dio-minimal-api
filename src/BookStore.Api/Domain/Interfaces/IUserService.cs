using BookStore.Api.Domain.DTOs;
using BookStore.Api.Domain.Entities;

namespace BookStore.Api.Domain.Interfaces;

public interface IUserService
{
    User? Login(LoginRequest request);
    User Create(CreateUserRequest request);
    User? GetById(int id);
    List<User> GetAll(int page);
    bool EmailExists(string email);
}
