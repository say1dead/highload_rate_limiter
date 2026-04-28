using System.Data;
using Dapper;
using Npgsql;
using UserService.DbModels;
using UserService.Domain;
using UserService.Mappers;

namespace UserService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly UserDbMapper _mapper;

    public UserRepository(NpgsqlDataSource dataSource, UserDbMapper mapper)
    {
        _dataSource = dataSource;
        _mapper = mapper;
    }

    public async Task<User?> CreateUserAsync(User user, CancellationToken ct)
    {
        var dbUser = _mapper.MapToDb(user);

        var p = new DynamicParameters();
        p.Add("login", dbUser.login, DbType.String);
        p.Add("password", dbUser.password, DbType.String);
        p.Add("name", dbUser.name, DbType.String);
        p.Add("surname", dbUser.surname, DbType.String);
        p.Add("age", dbUser.age, DbType.Int32);

        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync(ct);
            var created = await connection.QueryFirstOrDefaultAsync<UserDb>(
                new CommandDefinition(
                    "SELECT * FROM create_user(@login, @password, @name, @surname, @age)",
                    p, cancellationToken: ct));

            return created is null ? null : _mapper.MapToDomain(created);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new InvalidOperationException("User with this login already exists.", ex);
        }
    }

    public async Task<User?> GetUserByIdAsync(int id, CancellationToken ct)
    {
        var p = new DynamicParameters();
        p.Add("id", id, DbType.Int32);

        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        var dbUser = await connection.QueryFirstOrDefaultAsync<UserDb>(
            new CommandDefinition("SELECT * FROM get_user_by_id(@id)", p, cancellationToken: ct));

        return dbUser is null ? null : _mapper.MapToDomain(dbUser);
    }

    public async Task<User[]> GetUserByNameAsync(string name, string surname, CancellationToken ct)
    {
        var p = new DynamicParameters();
        p.Add("name", name, DbType.String);
        p.Add("surname", surname, DbType.String);

        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        var dbUsers = await connection.QueryAsync<UserDb>(
            new CommandDefinition("SELECT * FROM get_user_by_name(@name, @surname)", p, cancellationToken: ct));

        return dbUsers.Select(_mapper.MapToDomain).ToArray();
    }

    public async Task<User?> UpdateUserAsync(User user, CancellationToken ct)
    {
        var dbUser = _mapper.MapToDb(user);

        var p = new DynamicParameters();
        p.Add("id", dbUser.id, DbType.Int32);
        p.Add("password", dbUser.password, DbType.String);
        p.Add("name", dbUser.name, DbType.String);
        p.Add("surname", dbUser.surname, DbType.String);
        p.Add("age", dbUser.age, DbType.Int32);

        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        var updated = await connection.QueryFirstOrDefaultAsync<UserDb>(
            new CommandDefinition(
                "SELECT * FROM update_user(@id, @password, @name, @surname, @age)",
                p, cancellationToken: ct));

        return updated is null ? null : _mapper.MapToDomain(updated);
    }

    public async Task<bool> DeleteUserAsync(int id, CancellationToken ct)
    {
        var p = new DynamicParameters();
        p.Add("id", id, DbType.Int32);

        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition("SELECT delete_user(@id)", p, cancellationToken: ct));
    }
}
