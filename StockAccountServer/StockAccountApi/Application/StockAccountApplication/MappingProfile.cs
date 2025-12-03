using AutoMapper;
using StockAccountContracts.Dtos.Auth.Register;
using StockAccountContracts.Dtos.User;
using StockAccountDomain.Entities;

namespace StockAccountApplication;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterRequestDto, User>();
        CreateMap<User, UserResponseDto>();
    }
}
