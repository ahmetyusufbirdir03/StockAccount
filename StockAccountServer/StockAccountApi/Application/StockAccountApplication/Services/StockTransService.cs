using AutoMapper;
using Microsoft.AspNetCore.Http;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.StockTrans;
using StockAccountContracts.Dtos.StockTrans.Create;
using StockAccountContracts.Interfaces;
using StockAccountContracts.Interfaces.Services;
using StockAccountDomain.Entities;
using StockAccountDomain.Enums;
using System.Security.Claims;

namespace StockAccountApplication.Services;

public class StockTransService : IStockTransService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidationService _validationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StockTransService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidationService validationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validationService = validationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ResponseDto<StockTransResponseDto>> CreateStockTransactionAsync(CreateStockTransRequestDto Request)
    {
        var validationError = await _validationService.ValidateAsync<CreateStockTransRequestDto, StockTransResponseDto>(Request);
        if (validationError != null)
            return validationError;

        var company = await _unitOfWork.GetGenericRepository<Company>().GetByIdAsync(Request.CompanyId, c => c.Stocks, c => c.User);
        if (company is null || company.DeletedAt != null)
            return ResponseDto<StockTransResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, 404);

        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(currentUserId != company.UserId.ToString())
        {
            return ResponseDto<StockTransResponseDto>.Fail(ErrorMessageService.RestrictedAccess403, 403);
        }

        var counterpartyCompany = null as Company;
        if (Request.CounterpartyCompanyId.HasValue)
        {
            counterpartyCompany = await _unitOfWork.GetGenericRepository<Company>()
                .GetByIdAsync(Request.CounterpartyCompanyId.Value);
            if (counterpartyCompany is null || counterpartyCompany.DeletedAt != null)
                return ResponseDto<StockTransResponseDto>.Fail(ErrorMessageService.CountercompanyNotFound404, 404);
        }

        var stock = company.Stocks.FirstOrDefault(s => s.Id == Request.StockId);
        if (stock is null || stock.DeletedAt != null)
            return ResponseDto<StockTransResponseDto>.Fail(ErrorMessageService.StockNotFound404, 404);

        if (Request.Type == StockTransTypeEnum.Out && stock.Quantity < Request.Quantity)
        {
            return ResponseDto<StockTransResponseDto>.Fail(ErrorMessageService.InsufficientStockQuantity400, 400);
        }

        if(Request.Type == StockTransTypeEnum.In)
        {
            stock.Quantity += Request.Quantity;
        }
        else if(Request.Type == StockTransTypeEnum.Out)
        {
            stock.Quantity -= Request.Quantity;
        }

        var stockTrans = _mapper.Map<StockTrans>(Request);
        stockTrans.TotalPrice = Request.Quantity * Request.UnitPrice;

        await _unitOfWork.GetGenericRepository<StockTrans>().CreateAsync(stockTrans);

        return ResponseDto<StockTransResponseDto>
            .Success(_mapper.Map<StockTransResponseDto>(stockTrans),201);

    }

    public Task<ResponseDto<NoContentDto>> DeleteStockTransactionAsync(Guid StockTransId)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseDto<IList<StockTransResponseDto>>> GetAllStockTransactionsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ResponseDto<IList<StockTransResponseDto>>> GetTransactionsByStockIdAsync(Guid stockId)
    {
        throw new NotImplementedException();
    }
}
