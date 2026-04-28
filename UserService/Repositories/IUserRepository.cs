using System.Data;
using Dapper;
using Npgsql;
using UserService.DbModels;
using UserService.Domain;
using UserService.Mappers;

namespace UserService.Repositories;

public interface IUserRepository
{
    Task<User?> CreateUserAsync(User user, CancellationToken ct);
    Task<User?> GetUserByIdAsync(int id, CancellationToken ct);
    Task<User[]> GetUserByNameAsync(string name, string surname, CancellationToken ct);
    Task<User?> UpdateUserAsync(User user, CancellationToken ct);
    Task<bool> DeleteUserAsync(int id, CancellationToken ct);
}