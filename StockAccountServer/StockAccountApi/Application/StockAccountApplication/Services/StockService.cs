using AutoMapper;
using Microsoft.AspNetCore.Http;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Stock;
using StockAccountContracts.Dtos.Stock.Create;
using StockAccountContracts.Dtos.Stock.QuantityUpdate;
using StockAccountContracts.Dtos.Stock.Update;
using StockAccountContracts.Interfaces;
using StockAccountContracts.Interfaces.Services;
using StockAccountDomain.Entities;
using StockAccountDomain.Enums;
using StockAccountDomain.Models;
using StockAccountDomain.Services;
using System.Security.Claims;

namespace StockAccountApplication.Services;

public class StockService : IStockService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IValidationService _validationService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStockTransDomainService _stockTransDomainService;

    public StockService(
        IHttpContextAccessor httpContextAccessor,
        IValidationService validationService,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IStockTransDomainService stockTransDomainService)
    {
        _httpContextAccessor = httpContextAccessor;
        _validationService = validationService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _stockTransDomainService = stockTransDomainService;
    }
    public async Task<ResponseDto<StockResponseDto>> CreateStockAsync(CreateStockRequestDto Request)
    {
        var validationError = await _validationService.ValidateAsync<CreateStockRequestDto, StockResponseDto>(Request);
        if (validationError != null)
            return validationError;

        var company = await _unitOfWork.GetGenericRepository<Company>()
            .GetByIdAsync(Request.CompanyId);

        if (company == null || company.DeletedAt != null)
        {
            return ResponseDto<StockResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound);
        }

        Stock newStock = _mapper.Map<Stock>(Request);
        newStock.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.GetGenericRepository<Stock>().CreateAsync(newStock);
        
        var response = _mapper.Map<StockResponseDto>(newStock);

        return ResponseDto<StockResponseDto>.Success(response, StatusCodes.Status201Created);
    }

    public async Task<ResponseDto<NoContentDto>> DeleteStockAsync(Guid StockId)
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;
        if (currentUser == null || !currentUser.IsInRole("admin"))
        {
            return ResponseDto<NoContentDto>.Fail(ErrorMessageService.Unauthorized401, StatusCodes.Status401Unauthorized);
        }

        var stock = await _unitOfWork.GetGenericRepository<Stock>().GetByIdAsync(StockId);
        if (stock == null)
        {
            return ResponseDto<NoContentDto>.Fail(ErrorMessageService.StockNotFound404, StatusCodes.Status404NotFound);
        }

        await _unitOfWork.GetGenericRepository<Stock>().DeleteAsync(stock);

        return ResponseDto<NoContentDto>.Success(204);
    }

    public async Task<ResponseDto<IList<StockResponseDto>>> GetAllStocksAsync()
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;
        if (currentUser == null || !currentUser.IsInRole("admin"))
        {
            return ResponseDto<IList<StockResponseDto>>.Fail(ErrorMessageService.Unauthorized401, StatusCodes.Status401Unauthorized);
        }

        var stocks = await _unitOfWork.GetGenericRepository<Stock>()
            .GetAllAsync(x => x.DeletedAt == null, null);
        if (!stocks.Any())
        {
            return ResponseDto<IList<StockResponseDto>>.Fail(ErrorMessageService.StockNotFound404, StatusCodes.Status404NotFound);
        }

        var response = _mapper.Map<IList<StockResponseDto>>(stocks);

        return ResponseDto<IList<StockResponseDto>>.Success(response, 200);
    }

    public async Task<ResponseDto<IList<StockResponseDto>>> GetCompanyStokcsAsync(Guid CompanyId)
    {
        var systemUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _unitOfWork
            .GetGenericRepository<User>()
            .GetByIdAsync(Guid.Parse(systemUserId!), u => u.Companies);

        bool isCompanyMatchWithSystemUser = false;
        foreach (var companies in user!.Companies)
        {
            if (companies.Id == CompanyId)
            {
                isCompanyMatchWithSystemUser = true;
            }
        }

        if (!isCompanyMatchWithSystemUser)
        {
            return ResponseDto<IList<StockResponseDto>>.Fail(ErrorMessageService.RestrictedAccess403, StatusCodes.Status403Forbidden);
        }

        var company = await _unitOfWork.GetGenericRepository<Company>().GetByIdAsync(CompanyId);
        if (company == null || company.DeletedAt != null)
        {
            return ResponseDto<IList<StockResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound);
        }

        var stocks = await _unitOfWork.GetGenericRepository<Stock>().GetAllAsync(s => s.CompanyId == CompanyId,null);
        if (stocks.Count == 0)
        {
            return ResponseDto<IList<StockResponseDto>>.Fail(ErrorMessageService.StockNotFound404, StatusCodes.Status404NotFound);
        }

        var response = _mapper.Map<IList<StockResponseDto>>(stocks);

        return ResponseDto<IList<StockResponseDto>>.Success(response, StatusCodes.Status200OK);
    }

    public async Task<ResponseDto<NoContentDto>> SoftDeleteStockAsync(Guid stockId)
    {
        var stock = await _unitOfWork
            .GetGenericRepository<Stock>()
            .GetByIdAsync(stockId, x => x.Company);


        if (stock == null || stock.DeletedAt != null)
        {
            return ResponseDto<NoContentDto>.Fail(
                ErrorMessageService.StockNotFound404,
                StatusCodes.Status404NotFound
            );
        }

        var systemUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (systemUserId == null)
        {
            return ResponseDto<NoContentDto>.Fail(
                ErrorMessageService.Unauthorized401,
                StatusCodes.Status401Unauthorized
            );
        }

        if (systemUserId != stock.Company.UserId.ToString())
        {
            return ResponseDto<NoContentDto>.Fail(
                ErrorMessageService.RestrictedAccess403,
                StatusCodes.Status403Forbidden
            );
        }

        stock.DeletedAt = DateTime.UtcNow;
        stock.DeletedBy = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        await _unitOfWork.GetGenericRepository<Stock>().UpdateAsync(stock);

        return ResponseDto<NoContentDto>.Success(StatusCodes.Status204NoContent);
    }


    public async  Task<ResponseDto<StockResponseDto>> UpdateStockAsync(UpdateStockRequestDto Request)
    {
        var validationError = await _validationService.ValidateAsync<UpdateStockRequestDto, StockResponseDto>(Request);
        if (validationError != null)
            return validationError;

        var stock = await _unitOfWork.GetGenericRepository<Stock>().GetByIdAsync(Request.Id,s => s.Company);

        if (stock == null || stock.DeletedAt != null)
        {
            return ResponseDto<StockResponseDto>.Fail(ErrorMessageService.StockNotFound404, StatusCodes.Status404NotFound);
        }

        var systemUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (systemUserId == null)
        {
            return ResponseDto<StockResponseDto>.Fail(
                ErrorMessageService.Unauthorized401,
                StatusCodes.Status401Unauthorized
            );
        }

        if (systemUserId != stock.Company.UserId.ToString())
        {
            return ResponseDto<StockResponseDto>.Fail(
                ErrorMessageService.RestrictedAccess403,
                StatusCodes.Status403Forbidden
            );
        }
        
        var updatedStock = _mapper.Map(Request, stock);

        var response = _mapper.Map<StockResponseDto>(updatedStock);

        await _unitOfWork.GetGenericRepository<Stock>().UpdateAsync(updatedStock);

        return ResponseDto<StockResponseDto>.Success(response,StatusCodes.Status200OK);
    }

    public async Task<ResponseDto<StockResponseDto>> UpdateStockQuantityAsync(QuantityRequestDto Request)
    {
        var validationError = await _validationService.ValidateAsync<QuantityRequestDto, StockResponseDto>(Request);
        if (validationError != null)
            return validationError;

        var stock = await _unitOfWork.GetGenericRepository<Stock>().GetByIdAsync(Request.StockId);
        if(stock == null || stock.DeletedAt != null)
        {
            return ResponseDto<StockResponseDto>.Fail(ErrorMessageService.StockNotFound404, StatusCodes.Status404NotFound);
        }

        if (Request.IsAddition)
        {
            stock.Quantity += Request.Amount;
        }
        else { 
            if(stock.Quantity < Request.Amount)
            {
                return ResponseDto<StockResponseDto>.Fail(ErrorMessageService.InsufficientStockQuantity400,StatusCodes.Status400BadRequest);
            }
            stock.Quantity -= Request.Amount;
        }

  
        var model = new StockTransModel
        {
            CompanyId = stock.CompanyId,
            StockId = stock.Id,
            Quantity = Request.Amount,
            UnitPrice = stock.Price,
            Type = Request.IsAddition ? StockTransTypeEnum.In : StockTransTypeEnum.Out,
            TotalPrice = Request.Amount * stock.Price
        };

        StockTrans stockTrans = await _stockTransDomainService.CreateStockTransAsync(model);
        if(stockTrans == null)
        {
            return ResponseDto<StockResponseDto>.Fail(ErrorMessageService.InternalServerError500,StatusCodes.Status500InternalServerError);
        }

        await _unitOfWork.GetGenericRepository<Stock>().UpdateAsync(stock);

        var stockResponse = _mapper.Map<StockResponseDto>(stock);

        return ResponseDto<StockResponseDto>.Success(stockResponse, StatusCodes.Status200OK);
    }
}
        