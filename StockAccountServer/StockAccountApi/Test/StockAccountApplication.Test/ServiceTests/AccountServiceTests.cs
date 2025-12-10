using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using StockAccountApplication.Services;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Account;
using StockAccountContracts.Dtos.Account.Create;
using StockAccountContracts.Dtos.Company;
using StockAccountContracts.Dtos.Company.Create;
using StockAccountDomain.Entities;

namespace StockAccountApplication.Test.ServiceTests;

public class AccountServiceTests : ServiceTestBase
{
    private readonly AccountService _accountService;
    private readonly CreateAccountRequestDto _createAccountRequestDto = new CreateAccountRequestDto
    {
        CompanyId = Guid.NewGuid(),
        AccountName = "Account",
        Email = "account@mail.com",
        PhoneNumber = "00000000001",
        Address = "address"
    };

    private readonly AccountResponseDto _accountResponseDto = new AccountResponseDto
    {
        AccountName = "Account",
        Email = "account@mail.com",
        PhoneNumber = "00000000001",
        Address = "address",
    };
    public AccountServiceTests( )
    {
        _accountService = new AccountService(
            MapperMock.Object,
            UnitOfWorkMock.Object, 
            ValidationServiceMock.Object, 
            AccountCompanyDomainServiceMock.Object
        );
    }

    // CREATE ACCOUNT TESTS
    [Fact]
    public async Task CreateAccountAsync_ShouldReturnValidationError_WhenRequestNotValid()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateAccountRequestDto, AccountResponseDto>(_createAccountRequestDto))
            .ReturnsAsync(ResponseDto<AccountResponseDto>.Fail("ValidationError", 400));

        // Act
        var result = await _accountService.CreateAccountAsync(_createAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Fail("ValidationError", 400));
    }

    [Fact]
    public async Task CreateAccountAsync_ShouldReturnNotFound_WhenCompanyNotExist()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateAccountRequestDto, AccountResponseDto>(_createAccountRequestDto))
            .ReturnsAsync(null as ResponseDto<AccountResponseDto>);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createAccountRequestDto.CompanyId))
            .ReturnsAsync(null as Company);
        // Act
        var result = await _accountService.CreateAccountAsync(_createAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, 404));
    }

    [Fact]
    public async Task CreateAccountAsync_ShouldReturnNotFound_WhenCompanyIsDeleted()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateAccountRequestDto, AccountResponseDto>(_createAccountRequestDto))
            .ReturnsAsync(null as ResponseDto<AccountResponseDto>);

        _createAccountRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = DateTime.UtcNow;

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createAccountRequestDto.CompanyId))
            .ReturnsAsync(TestCompany);
        // Act
        var result = await _accountService.CreateAccountAsync(_createAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, 404));
    }
    
    // create test devam

}
