using AutoMapper;
using Microsoft.AspNetCore.Http;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Receipt;
using StockAccountContracts.Dtos.Receipt.Create;
using StockAccountContracts.Interfaces;
using StockAccountContracts.Interfaces.Services;
using StockAccountDomain.Entities;

namespace StockAccountApplication.Services;

public class ReceiptService : IReceiptService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public ReceiptService(
        IUnitOfWork unitOfWork, 
        IValidationService validationService, 
        IHttpContextAccessor httpContextAccessor, 
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<ResponseDto<ReceiptResponseDto>> CreateReceiptAsync(CreateReceiptRequestDto Request)
    {
        // TODO 
        // CREATE ACT TRANS
        // CREATE STOCK TRANS

        var validationError = await _validationService.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(Request);
        if (validationError != null)
            return validationError;

        var company = await _unitOfWork.GetGenericRepository<Company>().GetByIdAsync(Request.CompanyId, c => c.Stocks);
        if(company == null || company.DeletedAt != null)
        {
            return ResponseDto<ReceiptResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound);
        }

        var stock = company.Stocks.FirstOrDefault(s => s.Id == Request.StockId);
        if(stock == null)
        {
            return ResponseDto<ReceiptResponseDto>.Fail(ErrorMessageService.StockNotFound404, StatusCodes.Status404NotFound);
        }
            
        var account = await _unitOfWork.GetGenericRepository<Account>().GetByIdAsync(Request.AccountId);
        if (account == null)
        {
            return ResponseDto<ReceiptResponseDto>.Fail(ErrorMessageService.AccountNotFound404, StatusCodes.Status404NotFound);
        }

        if(Request.Quantity > stock.Quantity)
        {
            return ResponseDto<ReceiptResponseDto>.Fail(ErrorMessageService.InsufficientStockQuantity400, StatusCodes.Status400BadRequest);
        }

        var totalAmount = Request.Quantity * Request.UnitCurrentPrice;

        var receipt = _mapper.Map<Receipt>(Request);
        receipt.CreatedAt = DateTime.UtcNow;
        receipt.Amount = totalAmount;

        // CREATE ACT TRANS
        // CREATE STOCK TRANS

        await _unitOfWork.GetGenericRepository<Receipt>().CreateAsync(receipt);

        var response = _mapper.Map<ReceiptResponseDto>(receipt);

        return ResponseDto<ReceiptResponseDto>.Success(response, 201);
    }

    public async Task<ResponseDto<NoContentDto>> DeleteReceiptAsync(Guid ReceiptId)
    {
        var receipt = await _unitOfWork.GetGenericRepository<Receipt>().GetByIdAsync(ReceiptId);
        if (receipt == null)
        {
            return ResponseDto<NoContentDto>.Fail(ErrorMessageService.ReceitNotFound404, StatusCodes.Status404NotFound);
        }

        await _unitOfWork.GetGenericRepository<Receipt>().DeleteAsync(receipt);

        return ResponseDto<NoContentDto>.Success(StatusCodes.Status204NoContent);
    }

    public async Task<ResponseDto<IList<ReceiptResponseDto>>> GetAllReceiptsAsync()
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;
        if (currentUser?.Identity?.IsAuthenticated != true)
        {
            return ResponseDto<IList<ReceiptResponseDto>>.Fail(
                ErrorMessageService.Unauthorized401,
                StatusCodes.Status401Unauthorized
            );
        }

        if (!currentUser!.IsInRole("admin"))
        {
            return ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.RestrictedAccess403, StatusCodes.Status403Forbidden);
        }

        var receipts = await _unitOfWork.GetGenericRepository<Receipt>().GetAllAsync();
        if (receipts == null)
        {
            return ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.InternalServerError500, StatusCodes.Status500InternalServerError);
        }

        var responseList = _mapper.Map<IList<ReceiptResponseDto>>(receipts);
        return ResponseDto<IList<ReceiptResponseDto>>.Success(responseList, StatusCodes.Status200OK);
    }

    public async Task<ResponseDto<IList<ReceiptResponseDto>>> GetReceiptsByCompanyIdAsync(Guid companyId)
    {
        var company = await _unitOfWork.GetGenericRepository<Company>()
            .GetByIdAsync(companyId, c => c.Receipts, c => c.Accounts);
        if (company == null || company.DeletedAt != null)
        {
            return ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404);
        }

        var responseList = _mapper.Map<IList<ReceiptResponseDto>>(company.Receipts);

        return ResponseDto<IList<ReceiptResponseDto>>.Success(responseList, StatusCodes.Status200OK);
    }

    public async Task<ResponseDto<IList<ReceiptResponseDto>>> GetCompanyReceiptsByAccountIdAsync(Guid companyId, Guid accountId)
    {
        var company = await _unitOfWork.GetGenericRepository<Company>()
            .GetByIdAsync(companyId, c => c.Receipts, c => c.Accounts);
        if (company == null || company.DeletedAt != null)
        {
            return ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound);
        }

        if (!company.Accounts.Any(a => a.Id == accountId))
        {
            return ResponseDto<IList<ReceiptResponseDto>>
                .Fail(ErrorMessageService.RestrictedAccess403, 403);
        }

        var receipts = company.Receipts.Where(r => r.AccountId == accountId).ToList();

        var responseList = _mapper.Map<IList<ReceiptResponseDto>>(receipts);

        return ResponseDto<IList<ReceiptResponseDto>>.Success(responseList, StatusCodes.Status200OK);
    }

    public Task<ResponseDto<IList<ReceiptResponseDto>>> GetCompanyReceiptsByAccountIdAndStockIdAsync(Guid companyId, Guid accountId, Guid stockId)
    {
        throw new NotImplementedException();
    }
}
