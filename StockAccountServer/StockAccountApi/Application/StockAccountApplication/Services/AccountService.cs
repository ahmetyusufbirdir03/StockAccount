using AutoMapper;
using Microsoft.AspNetCore.Http;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Account;
using StockAccountContracts.Dtos.Account.Create;
using StockAccountContracts.Dtos.Account.Update;
using StockAccountContracts.Dtos.Stock;
using StockAccountContracts.Interfaces;
using StockAccountContracts.Interfaces.Services;
using StockAccountDomain.Entities;

namespace StockAccountApplication.Services;

public class AccountService : IAccountService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountService(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IValidationService validationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ResponseDto<AccountResponseDto>> CreateAccountAsync(CreateAccountRequestDto Request)
    {
        var validationError = await _validationService.ValidateAsync<CreateAccountRequestDto, AccountResponseDto>(Request);
        if (validationError != null)
            return validationError;

        var company = await _unitOfWork.GetGenericRepository<Company>().GetByIdAsync(Request.CompanyId, c => c.Accounts);
        if(company == null || company.DeletedAt != null)
        {
            return ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound);
        }

        var accounts = company.Accounts.ToList();
        
        if(accounts.Count >= 10)
        {
            return ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.MaxAccounLimitReached403, StatusCodes.Status403Forbidden);
        }

        var isEmailConflicts = accounts.Where(a => a.Email == Request.Email).ToList();
        if(isEmailConflicts.Count != 0)
        {
            return ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.AlreadyHaveThatAccount409, StatusCodes.Status409Conflict);
        }

        var isPhoneNumberConflicts = accounts.Where(a => a.PhoneNumber == Request.PhoneNumber).ToList();
        if (isPhoneNumberConflicts.Count != 0)
        {
            return ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.AlreadyHaveThatAccount409, StatusCodes.Status409Conflict);
        }

        var account = _mapper.Map<Account>(Request);

        await _unitOfWork.GetGenericRepository<Account>().CreateAsync(account);

        var response = _mapper.Map<AccountResponseDto>(account);

        return ResponseDto<AccountResponseDto>.Success(response, StatusCodes.Status201Created);
    }

    public async Task<ResponseDto<AccountResponseDto>> UpdateAccountAsync(UpdateAccountRequestDto Request)
    {
        var validationError = await _validationService.ValidateAsync<UpdateAccountRequestDto, AccountResponseDto>(Request);
        if (validationError != null)
            return validationError;

        var company = await _unitOfWork.GetGenericRepository<Company>().GetByIdAsync(Request.CompanyId, c => c.Accounts);
        if (company is null || company.DeletedAt != null)
        {
            return ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound);
        }

        var accountToUpdate = company.Accounts.FirstOrDefault(a => a.Id == Request.Id);

        if (accountToUpdate == null)
        {
            return ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.AccountNotFound404, StatusCodes.Status404NotFound);
        }

        var isEmaiLConflicts = company.Accounts.Any(a => a.Email == Request.Email && a.Id != Request.Id);
        if (isEmaiLConflicts)
        {
            return ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.EmailAlreadyRegistered409, StatusCodes.Status409Conflict);
        }

        var isPhoneNumberConflicts = company.Accounts.Any(a => a.PhoneNumber == Request.PhoneNumber && a.Id != Request.Id);
        if (isPhoneNumberConflicts)
        {
            return ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.PhoneNumberAlreadyRegistered409, StatusCodes.Status409Conflict);
        }

        _mapper.Map(Request, accountToUpdate);

        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<AccountResponseDto>(accountToUpdate);

        return ResponseDto<AccountResponseDto>.Success(response, StatusCodes.Status200OK);
    }

    public async Task<ResponseDto<IList<AccountResponseDto>>> GetAllAccountsAsync()
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;
        if (currentUser == null || !currentUser.IsInRole("admin"))
        {
            return ResponseDto<IList<AccountResponseDto>>.Fail(ErrorMessageService.Unauthorized401, StatusCodes.Status401Unauthorized);
        }

        var accounts = await _unitOfWork.GetGenericRepository<Account>().GetAllAsync();
        if(accounts is null || accounts.Count == 0)
        {
            return ResponseDto<IList<AccountResponseDto>>
                .Fail(ErrorMessageService.AccountNotFound404, StatusCodes.Status404NotFound);
        }

        var responseList =  _mapper.Map<IList<AccountResponseDto>>(accounts);

        return ResponseDto<IList<AccountResponseDto>>
                .Success(responseList, StatusCodes.Status200OK);
    }

    public async Task<ResponseDto<IList<AccountResponseDto>>> GetAccountsByCompanyIdAsync(Guid CompanyId)
    {
        var company = await _unitOfWork.GetGenericRepository<Company>().GetByIdAsync(CompanyId, c => c.Accounts);
        if(company == null || company.DeletedAt != null)
        {
            return ResponseDto<IList<AccountResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound);
        }

        var accountList = _mapper.Map<IList<AccountResponseDto>>(company.Accounts);

        return ResponseDto<IList<AccountResponseDto>>.Success(accountList, StatusCodes.Status200OK);
    }

    public async Task<ResponseDto<NoContentDto>> DeleteAccountAsync(Guid AccountId)
    {
        var account = await _unitOfWork.GetGenericRepository<Account>().GetByIdAsync(AccountId);
        if(account == null)
        {
            return ResponseDto<NoContentDto>.Fail(ErrorMessageService.AccountNotFound404, StatusCodes.Status404NotFound);
        }

        await _unitOfWork.GetGenericRepository<Account>().DeleteAsync(account);
        return ResponseDto<NoContentDto>.Success(StatusCodes.Status204NoContent);
    }
}
