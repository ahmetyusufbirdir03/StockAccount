using StockAccountContracts.Dtos;

namespace StockAccountContracts.Interfaces.Services;

public interface IValidationService
{
    Task<ResponseDto<TResponse>?> ValidateAsync<TRequest, TResponse>(TRequest request)
        where TResponse : class;
}
