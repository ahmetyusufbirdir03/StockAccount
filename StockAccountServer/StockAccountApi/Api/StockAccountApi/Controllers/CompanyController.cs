using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAccountContracts.Dtos.Company.Create;
using StockAccountContracts.Interfaces.Services;

namespace StockAccountApi.Controllers;


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;
    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }


    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _companyService.GetAllCompanies();
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, result);
        }
        return Ok(result);
    }

    [HttpPost("CreateCompany")]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequestDto request)
    {
        var result = await _companyService.CreateCompany(request);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, result);
        }
        return Ok(result);
    }

    [HttpDelete("Delete/{companyId}")]
    public async Task<IActionResult> SoftDeleteCompany(Guid companyId)
    {
        var result = await _companyService.SoftDeleteCompany(companyId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, result);
        }
        return Ok(result);
    }
}