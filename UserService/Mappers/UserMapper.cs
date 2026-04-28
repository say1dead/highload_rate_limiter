using Riok.Mapperly.Abstractions;
using UserService.Domain;
using UserService.Protos;

namespace UserService.Mappers;

[Mapper]
public partial class UserMapper
{
    public partial UserModel MapToProto(User user);
    
    [MapperIgnoreTarget(nameof(User.Id))]
    public partial User MapFromCreateRequest(CreateUserRequest request);
    
    [MapperIgnoreTarget(nameof(User.Login))]
    public partial User MapFromUpdateRequest(UpdateUserRequest request);
}
