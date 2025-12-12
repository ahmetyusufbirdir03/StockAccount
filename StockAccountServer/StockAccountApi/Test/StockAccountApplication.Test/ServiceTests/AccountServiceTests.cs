using FluentAssertions;
using Moq;
using StockAccountApplication.Services;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Account;
using StockAccountContracts.Dtos.Account.Create;
using StockAccountContracts.Dtos.Account.Update;
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

    private readonly UpdateAccountRequestDto _updateAccountRequestDto = new UpdateAccountRequestDto
    {
        Id = Guid.NewGuid(),
        CompanyId = Guid.NewGuid(),
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
            ValidationServiceMock.Object,
            HttpContextAccessorMock.Object
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
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createAccountRequestDto.CompanyId, c => c.Accounts))
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
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createAccountRequestDto.CompanyId, c => c.Accounts))
            .ReturnsAsync(TestCompany);
        // Act
        var result = await _accountService.CreateAccountAsync(_createAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, 404));
    }

    [Fact]
    public async Task CreateAccountAsync_ShouldReturnMaxAccountLimitReached_WhenAccountsCountMoreThan10()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateAccountRequestDto, AccountResponseDto>(_createAccountRequestDto))
            .ReturnsAsync(null as ResponseDto<AccountResponseDto>);

        _createAccountRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = null;

        TestCompany.Accounts = Enumerable.Range(1, 10)
            .Select(i => new Account
            {
                Id = Guid.NewGuid(),
                AccountName = $"Account {i}"
            })
            .ToList();

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createAccountRequestDto.CompanyId, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _accountService.CreateAccountAsync(_createAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.MaxAccounLimitReached403, 403));
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

        TestCompany.Accounts = new List<Account>();

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createAccountRequestDto.CompanyId, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        TestAccount.Email = _createAccountRequestDto.Email;
        TestCompany.Accounts = new List<Account>
        {
            TestAccount
        };

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
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createAccountRequestDto.CompanyId, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        TestAccount.PhoneNumber = _createAccountRequestDto.PhoneNumber;
        TestCompany.Accounts = new List<Account>
        {
            TestAccount
        };

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
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createAccountRequestDto.CompanyId, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        TestAccount.PhoneNumber = "0312313213";
        TestAccount.Email = "test123@mail";
        TestCompany.Accounts = new List<Account>
        {
            TestAccount
        };

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
    public async Task GetAllStocksAsync_ShoulReturnUnauthorized_WhenUserIsNotAdmin()
    {
        //Arrange
        SetupAuthenticatedUser(role: "user");

        //Act
        var result = await _accountService.GetAllAccountsAsync();

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<AccountResponseDto>>.Fail(ErrorMessageService.Unauthorized401, 401));
    }

    [Fact]
    public async Task GetAllAccountsAsync_ShouldReturnNotFound_WhenAnyAccountNotExist()
    {
        SetupAuthenticatedUser(role: "admin");

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
        SetupAuthenticatedUser(role: "admin");

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


    // Delete Account Tests
    [Fact]
    public async Task DeleteAccountAsync_ShouldReturnNotFound_WhenAccountNotExist()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().GetByIdAsync(TestAccount.Id))
            .ReturnsAsync(null as Account);

        // Act
        var result = await _accountService.DeleteAccountAsync(TestAccount.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<NoContentDto>.Fail(ErrorMessageService.AccountNotFound404, 404));

    }

    [Fact]
    public async Task DeleteAccountAsync_ShouldReturnSuccess_WhenAllStatementsPass()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().GetByIdAsync(TestAccount.Id))
            .ReturnsAsync(TestAccount);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().DeleteAsync(TestAccount))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _accountService.DeleteAccountAsync(TestAccount.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<NoContentDto>.Success(204));

    }


    // Get Account By Company Id Tests
    [Fact]
    public async Task GetAccountByCompanyIdAsync_ShouldReturnNotFound_WhenCompanyNotExist()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Accounts))
            .ReturnsAsync(null as Company);

        // Act
        var result = await _accountService.GetAccountsByCompanyIdAsync(TestCompany.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<AccountResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, 404));
    }

    [Fact]
    public async Task GetAccountByCompanyIdAsync_ShouldReturnNotFound_WhenCompanyIsDeleted()
    {
        TestCompany.DeletedAt = DateTime.UtcNow;
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _accountService.GetAccountsByCompanyIdAsync(TestCompany.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<AccountResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, 404));
    }

    [Fact]
    public async Task GetAccountByCompanyIdAsync_ShouldReturnSuccess_WhenAllStatementsPass()
    {
        // Arrange
        TestCompany.Accounts = new List<Account> { TestAccount };
        TestCompany.DeletedAt = null;

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        var responseList = new List<AccountResponseDto> { _accountResponseDto };
        MapperMock
            .Setup(m => m.Map<IList<AccountResponseDto>>(TestCompany.Accounts))
            .Returns(responseList);

        // Act
        var result = await _accountService.GetAccountsByCompanyIdAsync(TestCompany.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<AccountResponseDto>>.Success(responseList, 200));
    }


    // Update Account Tests
    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnValidationError_WhenRequestNotValid()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateAccountRequestDto, AccountResponseDto>(_updateAccountRequestDto))
            .ReturnsAsync(ResponseDto<AccountResponseDto>.Fail("ValidationError", 400));

        // Act
        var result = await _accountService.UpdateAccountAsync(_updateAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Fail("ValidationError", 400));
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnNotFound_WhenCompanyNotExist()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateAccountRequestDto, AccountResponseDto>(_updateAccountRequestDto))
            .ReturnsAsync(null as ResponseDto<AccountResponseDto>);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Accounts))
            .ReturnsAsync(null as Company);

        // Act
        var result = await _accountService.UpdateAccountAsync(_updateAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, 404));
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnNotFound_WhenCompanyIsDeleted()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateAccountRequestDto, AccountResponseDto>(_updateAccountRequestDto))
            .ReturnsAsync(null as ResponseDto<AccountResponseDto>);

        TestCompany.DeletedAt = DateTime.UtcNow;
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _accountService.UpdateAccountAsync(_updateAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, 404));
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnNotFound_WhenAccountNotExist()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateAccountRequestDto, AccountResponseDto>(_updateAccountRequestDto))
            .ReturnsAsync(null as ResponseDto<AccountResponseDto>);

        TestCompany.Accounts = new List<Account> { TestAccount };
        TestCompany.DeletedAt = null;

        _updateAccountRequestDto.CompanyId = TestCompany.Id;

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_updateAccountRequestDto.CompanyId, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _accountService.UpdateAccountAsync(_updateAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.AccountNotFound404, 404));
    }


    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnAlreadyRegistered_WhenAccountEmailAlreadyRegistered()
    {
        // Arrange

        var conflictEmail = "conflit@mail";
        _updateAccountRequestDto.CompanyId = TestCompany.Id;
        _updateAccountRequestDto.Id = Guid.NewGuid();   
        _updateAccountRequestDto.Email = conflictEmail;

        var accountToUpdate = TestDataFactory.CreateTestAccount(email: "old@mail.com");
        accountToUpdate.Id = _updateAccountRequestDto.Id;

        var conflictAccount = TestDataFactory.CreateTestAccount(email: conflictEmail);

        // Listeyi şirkete ekle
        TestCompany.Accounts = new List<Account> { accountToUpdate, conflictAccount };
        TestCompany.DeletedAt = null;

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateAccountRequestDto, AccountResponseDto>(_updateAccountRequestDto))
            .ReturnsAsync(null as ResponseDto<AccountResponseDto>);


        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_updateAccountRequestDto.CompanyId, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _accountService.UpdateAccountAsync(_updateAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.EmailAlreadyRegistered409, 409));
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnAlreadyRegistered_WhenAccountPhoneNumberAlreadyRegistered()
    {
        // Arrange
        var conflictPhoneNumber = "0557757575";
        _updateAccountRequestDto.CompanyId = TestCompany.Id;
        _updateAccountRequestDto.Id = Guid.NewGuid();

        var accountToUpdate = TestDataFactory.CreateTestAccount(phoneNumber: "87298479218");
        accountToUpdate.Id = _updateAccountRequestDto.Id;

        var conflictAccount = TestDataFactory.CreateTestAccount(email: conflictPhoneNumber);

        // Listeyi şirkete ekle
        TestCompany.Accounts = new List<Account> { accountToUpdate, conflictAccount };
        TestCompany.DeletedAt = null;

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateAccountRequestDto, AccountResponseDto>(_updateAccountRequestDto))
            .ReturnsAsync(null as ResponseDto<AccountResponseDto>);


        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_updateAccountRequestDto.CompanyId, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _accountService.UpdateAccountAsync(_updateAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Fail(ErrorMessageService.PhoneNumberAlreadyRegistered409, 409));
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnSuccess_WhenAllStatementsPass()
    {
        // Arrange
        _updateAccountRequestDto.CompanyId = TestCompany.Id;
        _updateAccountRequestDto.Id = Guid.NewGuid();

        var accountToUpdate = TestDataFactory.CreateTestAccount(phoneNumber: "87298479218");
        accountToUpdate.Id = _updateAccountRequestDto.Id;

        // Listeyi şirkete ekle
        TestCompany.Accounts = new List<Account> { accountToUpdate };
        TestCompany.DeletedAt = null;

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateAccountRequestDto, AccountResponseDto>(_updateAccountRequestDto))
            .ReturnsAsync(null as ResponseDto<AccountResponseDto>);


        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_updateAccountRequestDto.CompanyId, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        MapperMock
            .Setup(m => m.Map<Account>(_updateAccountRequestDto))
            .Returns(accountToUpdate);

        UnitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        MapperMock
            .Setup(m => m.Map<AccountResponseDto>(accountToUpdate))
            .Returns(_accountResponseDto);

        // Act
        var result = await _accountService.UpdateAccountAsync(_updateAccountRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<AccountResponseDto>.Success(_accountResponseDto, 200));
    }
}
    
