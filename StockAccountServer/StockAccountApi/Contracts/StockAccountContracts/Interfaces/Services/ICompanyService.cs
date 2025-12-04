using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Company;
using StockAccountContracts.Dtos.Company.Create;
using StockAccountContracts.Dtos.Company.Update;

namespace StockAccountContracts.Interfaces.Services;

public interface ICompanyService
{
    Task<ResponseDto<CompanyResponseDto>> CreateCompany(CreateCompanyRequestDto request);
    Task<ResponseDto<IList<CompanyResponseDto>>> GetUserCompanies(Guid userId);
    Task<ResponseDto<NoContentDto>> SoftDeleteCompany(Guid companyId);
    Task<ResponseDto<CompanyResponseDto>> UpdateCompany(UpdateCompanyRequestDto request);

    /// <summary>
    /// Get all companies, admin function.
    /// </summary>
    Task<ResponseDto<IList<CompanyResponseDto>>> GetAllCompanies();


}
