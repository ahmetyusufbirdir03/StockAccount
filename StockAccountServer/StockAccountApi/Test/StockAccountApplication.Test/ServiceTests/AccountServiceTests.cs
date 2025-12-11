using FluentAssertions;
using Moq;
using StockAccountApplication.Services;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Account;
using StockAccountContracts.Dtos.Account.Create;
using StockAccountContracts.Dtos.Account.Update;
using StockAccountContracts.Dtos.Stock;
using StockAccountDomain.Entities;
using StockAccountDomain.Models;
using System.Linq.Expressions;
using System.Security.Principal;

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

    private readonly UpdateAccountRequestDto _updateAccountRequestDto = new UpdateAccountRequestDto
    {
        Id = Guid.NewGuid(),
        AccountName = "UPDATE",
        Email = "mail@mail",
        PhoneNumber = "00000000001",
        Address = "address",
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
            ValidationServiceMock.Object
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

    [Fact]
    public async Task CreateAccountAsync_ShouldReturnAlreadyHaveAccount_WhenEmailConflicts()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateAccountRequestDto, AccountResponseDto>(_createAccountRequestDto))
            .ReturnsAsync(null as ResponseDto<AccountResponseDto>);

        _createAccountRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = null;

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createAccountRequestDto.CompanyId))
            .ReturnsAsync(TestCompany);

        TestAccount.Email = _createAccountRequestDto.Email;
        var accountList = new List<Account>
        {
            TestAccount
        };

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().GetAllAsync(It.IsAny<Expression<Func<Account, bool>>>(),null))
            .ReturnsAsync(accountList);

        // Act
        var result = await _accountService.CreateAccountAsync(_createAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.AlreadyHaveThatAccount409, 409));
    }

    [Fact]
    public async Task CreateAccountAsync_ShouldReturnAlreadyHaveAccount_WhenPhoneNumberConflicts()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateAccountRequestDto, AccountResponseDto>(_createAccountRequestDto))
            .ReturnsAsync(null as ResponseDto<AccountResponseDto>);

        _createAccountRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = null;

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createAccountRequestDto.CompanyId))
            .ReturnsAsync(TestCompany);

        TestAccount.PhoneNumber = _createAccountRequestDto.PhoneNumber;
        var accountList = new List<Account>
        {
            TestAccount
        };

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().GetAllAsync(It.IsAny<Expression<Func<Account, bool>>>(), null))
            .ReturnsAsync(accountList);



        // Act
        var result = await _accountService.CreateAccountAsync(_createAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.AlreadyHaveThatAccount409, 409));
    }

    [Fact]
    public async Task CreateAccountAsync_ShouldReturnSuccess_WhenAllStatementsPass()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateAccountRequestDto, AccountResponseDto>(_createAccountRequestDto))
            .ReturnsAsync(null as ResponseDto<AccountResponseDto>);

        _createAccountRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = null;

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createAccountRequestDto.CompanyId))
            .ReturnsAsync(TestCompany);

        var email = _createAccountRequestDto.Email;
        TestAccount.Email = "denem@mail";
        TestAccount.PhoneNumber = "03123123";
        var accountList = new List<Account>
        {
            TestAccount
        };

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().GetAllAsync(It.IsAny<Expression<Func<Account, bool>>>(), null))
            .ReturnsAsync(accountList);

        MapperMock
            .Setup(m => m.Map<Account>(_createAccountRequestDto))
            .Returns(TestAccount);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().CreateAsync(TestAccount))
            .ReturnsAsync(TestAccount);

        MapperMock
            .Setup(m => m.Map<AccountResponseDto>(TestAccount))
            .Returns(_accountResponseDto);

        // Act
        var result = await _accountService.CreateAccountAsync(_createAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Success(_accountResponseDto, 201));
    }


    // Get All Accounts Tests
    [Fact]
    public async Task GetAllAccountsAsync_ShouldReturnNotFound_WhenAnyAccountNotExist()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().GetAllAsync(null, null))!
            .ReturnsAsync(null as List<Account>);

        // Act
        var result = await _accountService.GetAllAccountsAsync();

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<AccountResponseDto>>.Fail(ErrorMessageService
            .AccountNotFound404, 404));
    }

    [Fact]
    public async Task GetAllAccountsAsync_ShouldReturnSuccess_WhenAllStatementsPass()
    {
        // Arrange
        var accountList = new List<Account>
        {
            TestAccount,
        };

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().GetAllAsync(null, null))!
            .ReturnsAsync(accountList);


        var responseList = new List<AccountResponseDto> { new AccountResponseDto() };

        MapperMock
            .Setup(m => m.Map<IList<AccountResponseDto>>(accountList))
            .Returns(responseList);
        // Act
        var result = await _accountService.GetAllAccountsAsync();

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<AccountResponseDto>>
            .Success(responseList, 200));
    }


    // Update Account Tests
    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnValidatonError_WhenRequestNotValid()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateAccountRequestDto, AccountResponseDto>(_updateAccountRequestDto))
            .ReturnsAsync(ResponseDto<AccountResponseDto>.Fail("ValidationError", 400));

        // Act
        var result = await _accountService.UpdateAccount(_updateAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Fail("ValidationError", 400));
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnNotFound_WhenAccountNotExist()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateAccountRequestDto, AccountResponseDto>(_updateAccountRequestDto))
            .ReturnsAsync(null as ResponseDto<AccountResponseDto>);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().GetByIdAsync(_updateAccountRequestDto.Id))
            .ReturnsAsync(null as Account);

        // Act
        var result = await _accountService.UpdateAccount(_updateAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>
            .Fail(ErrorMessageService.AccountNotFound404, 404));
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnSuccess_WhenAllStatementsPass()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateAccountRequestDto, AccountResponseDto>(_updateAccountRequestDto))
            .ReturnsAsync(null as ResponseDto<AccountResponseDto>);
       
        TestAccount.Id = _updateAccountRequestDto.Id;

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().GetByIdAsync(_updateAccountRequestDto.Id))
            .ReturnsAsync(TestAccount);

        MapperMock
            .Setup(m => m.Map<Account>(_updateAccountRequestDto))
            .Returns(TestAccount);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().UpdateAsync(TestAccount))
            .ReturnsAsync(TestAccount);

        MapperMock
            .Setup(m => m.Map<AccountResponseDto>(TestAccount))
            .Returns(_accountResponseDto);


        // Act
        var result = await _accountService.UpdateAccount(_updateAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>
            .Success(_accountResponseDto, 200));
    }
}
