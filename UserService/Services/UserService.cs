using FluentValidation;
using UserService.Domain;
using UserService.Repositories;
using UserService.Validators;

namespace UserService.Services;

public class UserService
{
    private readonly IUserRepository _repo;
    private readonly IValidator<User> _createValidator;
    private readonly IValidator<User> _updateValidator;

    public UserService(IUserRepository repo, UserCreateValidator createValidator, UserUpdateValidator updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public Task<User?> CreateUserAsync(User user, CancellationToken ct)
    {
        _createValidator.ValidateAndThrow(user);

        return _repo.CreateUserAsync(user, ct);
    }

    public Task<User?> GetUserByIdAsync(int id, CancellationToken ct)
    {
        return _repo.GetUserByIdAsync(id, ct);
    }

    public Task<User[]> GetUserByNameAsync(string name, string surname, CancellationToken ct)
    {
        return _repo.GetUserByNameAsync(name, surname, ct);
    }

    public Task<User?> UpdateUserAsync(User user, CancellationToken ct)
    {
        _updateValidator.ValidateAndThrow(user);

        return _repo.UpdateUserAsync(user, ct);
    }

    public Task<bool> DeleteUserAsync(int id, CancellationToken ct)
    {
        return _repo.DeleteUserAsync(id, ct);
    }
}
