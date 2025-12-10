using AutoMapper;
using Microsoft.AspNetCore.Http;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Account;
using StockAccountContracts.Dtos.Account.Create;
using StockAccountContracts.Dtos.Company;
using StockAccountContracts.Dtos.Company.Create;
using StockAccountContracts.Interfaces;
using StockAccountContracts.Interfaces.Services;
using StockAccountDomain.Entities;
using StockAccountDomain.Models;
using StockAccountDomain.Services;

namespace StockAccountApplication.Services;

public class AccountService : IAccountService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly IAccountCompanyDomainService _accountCompanyDomainService;

    public AccountService(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IValidationService validationService,
        IAccountCompanyDomainService accountCompanyDomainService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _accountCompanyDomainService = accountCompanyDomainService;
    }
    public async Task<ResponseDto<AccountResponseDto>> CreateAccountAsync(CreateAccountRequestDto Request)
    {
        var validationError = await _validationService.ValidateAsync<CreateAccountRequestDto, AccountResponseDto>(Request);
        if (validationError != null)
            return validationError;

        var company = await _unitOfWork.GetGenericRepository<Company>().GetByIdAsync(Request.CompanyId);
        if (company == null || company.DeletedAt != null)
        {
            return ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound);
        }

        var isEmailExist = await IsAccountEmailExist(Request.Email);

        if (isEmailExist)
        {
            return ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.EmailAlreadyRegistered409,StatusCodes.Status409Conflict);
        }

        var isPhoneNumberExist = await IsAccountPhoneNumberExist(Request.PhoneNumber);
        if (isPhoneNumberExist)
        {
            return ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.PhoneNumberAlreadyRegistered409, StatusCodes.Status409Conflict);
        }

        var mappedAccount = _mapper.Map<Account>(Request);

        var account = await _unitOfWork.GetGenericRepository<Account>().CreateAsync(mappedAccount);

        // TODO: CREATE RELATION ACCOUNCOMPANY
        if(account != null) {
            var model = new AccountCompanyModel
            {
                CompanyId = Request.CompanyId,
                AccountId = account.Id
            };

            await _accountCompanyDomainService.CreateAccountCompanyAsync(model);
        }

        var response = _mapper.Map<AccountResponseDto>(account);

        return ResponseDto<AccountResponseDto>.Success(response, StatusCodes.Status200OK);
    }

    public async Task<bool> IsAccountEmailExist(string email)
    {
        var account = await _unitOfWork
            .GetGenericRepository<Account>()
            .GetAsync(x => x.Email == email);

        return account != null;
    }

    public async Task<bool> IsAccountPhoneNumberExist(string phoneNumber)
    {
        var account = await _unitOfWork
            .GetGenericRepository<Account>()
            .GetAsync(x => x.PhoneNumber == phoneNumber);

        return account != null;
    }
}
