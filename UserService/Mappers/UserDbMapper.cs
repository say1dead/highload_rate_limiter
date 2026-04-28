using Riok.Mapperly.Abstractions;
using UserService.DbModels;
using UserService.Domain;

namespace UserService.Mappers;

[Mapper(PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseInsensitive)]
public partial class UserDbMapper
{
    public partial User MapToDomain(UserDb dbUser);
    
    public partial UserDb MapToDb(User user);
}
