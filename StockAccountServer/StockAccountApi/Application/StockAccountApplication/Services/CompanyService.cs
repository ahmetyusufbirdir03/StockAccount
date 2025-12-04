using AutoMapper;
using Microsoft.AspNetCore.Http;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Company;
using StockAccountContracts.Dtos.Company.Create;
using StockAccountContracts.Dtos.Company.Update;
using StockAccountContracts.Interfaces;
using StockAccountContracts.Interfaces.Services;
using StockAccountDomain.Entities;
using System.Security.Claims;

namespace StockAccountApplication.Services;

public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;   
    private readonly IValidationService _validationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CompanyService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidationService validationService,
        IHttpContextAccessor httpContextAccesor)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validationService = validationService;
        _httpContextAccessor = httpContextAccesor;
    }

    public async Task<ResponseDto<CompanyResponseDto>> CreateCompany(CreateCompanyRequestDto request)
    {
        var validationError = await _validationService.ValidateAsync<CreateCompanyRequestDto, CompanyResponseDto>(request);
        if (validationError != null)
            return validationError;

        var user = await _unitOfWork
            .GetGenericRepository<User>()
            .GetByIdAsync(request.UserId, u => u.Companies.Where(c => c.DeletedAt == null));

        if(user == null || user.DeletedAt != null)
        {
            return ResponseDto<CompanyResponseDto>.Fail(ErrorMessageService.UserNotFound404, 404);
        }

        if(user.Companies != null && user.Companies.Count >= 3)
        {
            return ResponseDto<CompanyResponseDto>.Fail("A user can create a maximum of 3 companies", 400);
        }

        //Email conflict check
        var nameConflict = await _unitOfWork.GetGenericRepository<Company>().GetAllAsync(
            predicate: c => c.CompanyName == request.CompanyName && c.DeletedAt == null);
        if (nameConflict.Any())
        {
            return ResponseDto<CompanyResponseDto>.Fail(ErrorMessageService.CompanyNameAlredyRegistered409, 409);
        }


        //Email conflict check
        var emailConflict = await _unitOfWork.GetGenericRepository<Company>().GetAllAsync(
            predicate: c => c.Email == request.Email && c.DeletedAt == null);
        if (emailConflict.Any())
        {
            return ResponseDto<CompanyResponseDto>.Fail(ErrorMessageService.EmailAlreadyRegistered409, 409);
        }

        //Phone number conflict check
        var phoneConflict = await _unitOfWork.GetGenericRepository<Company>().GetAllAsync(
            predicate: c => c.PhoneNumber == request.PhoneNumber && c.DeletedAt == null);
        if (phoneConflict.Any())
        {
            return ResponseDto<CompanyResponseDto>.Fail(ErrorMessageService.PhoneNumberAlreadyRegistered409, 409);
        }

        var company = _mapper.Map<Company>(request);
        await _unitOfWork.GetGenericRepository<Company>().CreateAsync(company);

        var companyResponse = _mapper.Map<CompanyResponseDto>(company);
        return ResponseDto<CompanyResponseDto>.Success(companyResponse, 201);

    }

    public async Task<ResponseDto<NoContentDto>> SoftDeleteCompany(Guid companyId)
    {
        var company = await _unitOfWork.GetGenericRepository<Company>().GetAsync(c => c.DeletedAt == null && c.Id == companyId);
        if (company == null)
        {
            return ResponseDto<NoContentDto>.Fail(ErrorMessageService.CompanyNotFound404);
        }

        company.DeletedAt = DateTime.UtcNow;
        company.DeletedBy = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? "System";

        await _unitOfWork.GetGenericRepository<Company>().UpdateAsync(company);

        return ResponseDto<NoContentDto>.Success(204);  
    }

    public async Task<ResponseDto<IList<CompanyResponseDto>>> GetAllCompanies()
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;
        if (currentUser == null || !currentUser.IsInRole("admin"))
        {
            return ResponseDto<IList<CompanyResponseDto>>
                .Fail(ErrorMessageService.Unauthorized401, StatusCodes.Status401Unauthorized);
        }

        var companies = await _unitOfWork.GetGenericRepository<Company>().GetAllAsync(c => c.DeletedAt == null);    
        if (!companies.Any())
            return ResponseDto<IList<CompanyResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound);

        IList<CompanyResponseDto> _companies = _mapper.Map<IList<CompanyResponseDto>>(companies);
        return ResponseDto<IList<CompanyResponseDto>>.Success(_companies, StatusCodes.Status200OK);
    }

    public async Task<ResponseDto<IList<CompanyResponseDto>>> GetUserCompanies(Guid userId)
    {
        var user = await _unitOfWork
            .GetGenericRepository<User>()
            .GetByIdAsync(userId, u => u.Companies.Where(c => c.DeletedAt == null));

        if (user == null || user.DeletedAt != null)
        {
            return ResponseDto<IList<CompanyResponseDto>>
                .Fail(ErrorMessageService.UserNotFound404, 404);
        }

        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (currentUserId == null || currentUserId != user.Id.ToString())
        {
            return ResponseDto<IList<CompanyResponseDto>>
                .Fail(ErrorMessageService.RestrictedAccess401, StatusCodes.Status401Unauthorized);
        }

        if(user.Companies == null || !user.Companies.Any())
        {
            return ResponseDto<IList<CompanyResponseDto>>
                .Fail(ErrorMessageService.CompanyNotFound404, 404);
        }

        IList<CompanyResponseDto> companies =
            _mapper.Map<IList<CompanyResponseDto>>(user.Companies);

        return ResponseDto<IList<CompanyResponseDto>>.Success(companies, 200);
    }


    public async Task<ResponseDto<CompanyResponseDto>> UpdateCompany(UpdateCompanyRequestDto request)
    {
        var company = await _unitOfWork.GetGenericRepository<Company>().GetByIdAsync(request.Id, c => c.DeletedAt == null);
        if(company == null)
        {
            return ResponseDto<CompanyResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, 404);
        }

        var user = await _unitOfWork.
            GetGenericRepository<User>()
            .GetByIdAsync(company.UserId, u => u.DeletedAt == null);

        if (user == null || user.DeletedAt != null)
        {
            return ResponseDto<CompanyResponseDto>
                .Fail(ErrorMessageService.UserNotFound404, 404);
        }

        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (currentUserId == null || currentUserId != user.Id.ToString())
        {
            return ResponseDto<CompanyResponseDto>
                .Fail(ErrorMessageService.RestrictedAccess401, StatusCodes.Status401Unauthorized);
        }

        company = _mapper.Map(request, company);
        await _unitOfWork.GetGenericRepository<Company>().UpdateAsync(company);
        var companyResponse = _mapper.Map<CompanyResponseDto>(company);
        return ResponseDto<CompanyResponseDto>.Success(companyResponse, 200);
    }
}
