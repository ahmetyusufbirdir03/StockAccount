using AutoMapper;
using StockAccountContracts.Dtos.Auth.Register;
using StockAccountContracts.Dtos.Company;
using StockAccountContracts.Dtos.Company.Create;
using StockAccountContracts.Dtos.Stock;
using StockAccountContracts.Dtos.Stock.Create;
using StockAccountContracts.Dtos.Stock.Update;
using StockAccountContracts.Dtos.StockTrans.Create;
using StockAccountContracts.Dtos.User;
using StockAccountDomain.Entities;
using StockAccountDomain.Models;

namespace StockAccountApplication;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterRequestDto, User>();
        CreateMap<User, UserResponseDto>();
        CreateMap<Company, CompanyResponseDto>();
        CreateMap<CreateCompanyRequestDto, Company>();
        CreateMap<Stock, StockResponseDto>();
        CreateMap<CreateStockRequestDto, Stock>();
        CreateMap<UpdateStockRequestDto, Stock>();
        CreateMap<CreateStockTransRequestDto, StockTrans>();
    }
}
