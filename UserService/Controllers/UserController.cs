using Grpc.Core;
using UserService.Protos;
using UserService.Services;
using UserService.Domain;
using UserService.Mappers;
using FluentValidation;

namespace UserService.Controllers;

public class UserController : Protos.UserService.UserServiceBase
{
    private readonly Services.UserService _service;
    private readonly UserMapper _mapper;

    public UserController(Services.UserService service, UserMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        try
        {
            var user = _mapper.MapFromCreateRequest(request);
            var created = await _service.CreateUserAsync(user, context.CancellationToken);
            return new CreateUserResponse { User = _mapper.MapToProto(created) };
        }
        catch (ValidationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
        }
    }

    public override async Task<GetUserResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
    {
        var user = await _service.GetUserByIdAsync(request.Id, context.CancellationToken);
        if (user == null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return new GetUserResponse { User = _mapper.MapToProto(user) };
    }

    public override async Task<GetUsersResponse> GetUserByName(GetUserByNameRequest request, ServerCallContext context)
    {
        var users = await _service.GetUserByNameAsync(request.Name, request.Surname, context.CancellationToken);

        var response = new GetUsersResponse();
        response.Users.AddRange(users.Select(_mapper.MapToProto));
        return response;
    }

    public override async Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        try
        {
            var user = _mapper.MapFromUpdateRequest(request);
            var updated = await _service.UpdateUserAsync(user, context.CancellationToken);

            if (updated == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"User with id={request.Id} not found"));
            }
            
            return new UpdateUserResponse { User = _mapper.MapToProto(updated) };
        }
        catch (ValidationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }

    public override async Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        var result = await _service.DeleteUserAsync(request.Id, context.CancellationToken);
        if (!result)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return new DeleteUserResponse { Success = true };
    }
}
