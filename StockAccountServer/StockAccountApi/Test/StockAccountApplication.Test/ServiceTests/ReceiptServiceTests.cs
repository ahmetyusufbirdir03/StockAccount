using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using StockAccountApplication.Services;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Receipt;
using StockAccountContracts.Dtos.Receipt.Create;
using StockAccountDomain.Entities;
using StockAccountDomain.Enums;
using StockAccountDomain.Models;

namespace StockAccountApplication.Test.ServiceTests;

public class ReceiptServiceTests : ServiceTestBase
{
    private readonly ReceiptService _receiptService;

    private CreateReceiptRequestDto _createReceiptRequestDto = new CreateReceiptRequestDto
    {
        AccountId = Guid.NewGuid(),
        CompanyId = Guid.NewGuid(),
        StockId = Guid.NewGuid(),
        Quantity = 10,
        Type = ReceiptTypeEnum.Sale,
    };

    private ReceiptResponseDto _receiptResponseDto = new ReceiptResponseDto
    {
        Id = Guid.NewGuid(),
        AccountId = Guid.NewGuid(),
        CompanyId = Guid.NewGuid(),
        StockId = Guid.NewGuid(),
        Quantity = 1,
        Type = ReceiptTypeEnum.Sale,
        UnitCurrentPrice = 1,
        TotalAmount = 1,
    };
    
    public ReceiptServiceTests()
    {
        _receiptService = new ReceiptService(
            UnitOfWorkMock.Object,
            ValidationServiceMock.Object,
            HttpContextAccessorMock.Object,
            MapperMock.Object,
            StockTransDomainServiceMock.Object,
            ActTransDomainServiceMock.Object
        );
    }

    // Create Receipt Tests
    [Fact]
    public async Task CreateReceiptAsync_ShouldReturnValidationError_WhenRequestNotValid()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(ResponseDto<ReceiptResponseDto>.Fail("ValidationError", 400));

        // Act
        var result = await _receiptService.CreateReceiptAsync(_createReceiptRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<ReceiptResponseDto>.Fail("ValidationError", 400));
    }

    [Fact]
    public async Task CreateReceiptAsync_ShouldReturnRestrictedAccess_WhenUserNotMatchWithCompany()
    {
        // Arrange
        TestUser.Id = Guid.NewGuid();
        SetupAuthenticatedUser(role: "user");
        TestCompany.DeletedAt = null;

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
           .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createReceiptRequestDto.CompanyId, c=> c.Stocks, c=> c.Accounts))
           .ReturnsAsync(TestCompany);

        // Act
        var result = await _receiptService.CreateReceiptAsync(_createReceiptRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<ReceiptResponseDto>.Fail(ErrorMessageService.RestrictedAccess403, 403));
    }

    [Fact]
    public async Task CreateReceiptAsync_ShouldReturnNotFound_WhenCompanyNotExist()
    {
        // Arrange
        var testUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        TestUser.Id = testUserId;
        SetupAuthenticatedUser(role: "user");

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
           .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createReceiptRequestDto.CompanyId, c => c.Stocks, c => c.Accounts))
           .ReturnsAsync(null as Company);

        // Act
        var result = await _receiptService.CreateReceiptAsync(_createReceiptRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<ReceiptResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, 404));
    }

    [Fact]
    public async Task CreateReceiptAsync_ShouldReturnNotFound_WhenCompanyIsDeleted()
    {
        // Arrange
        var testUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        TestUser.Id = testUserId;
        SetupAuthenticatedUser(role: "user");

        _createReceiptRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = DateTime.UtcNow;
        
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
           .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createReceiptRequestDto.CompanyId, c => c.Stocks, c => c.Accounts))
           .ReturnsAsync(TestCompany);

        // Act
        var result = await _receiptService.CreateReceiptAsync(_createReceiptRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<ReceiptResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, 404));
    }

    [Fact]
    public async Task CreateReceiptAsync_ShouldReturnNotFound_WhenStockNotExistOrFound()
    {
        // Arrange
        SetupAuthenticatedUser(role: "user");
        TestCompany.UserId = TestUser.Id;

        _createReceiptRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = null;

        _createReceiptRequestDto.StockId = TestStock.Id;
        TestCompany.Accounts = new List<Account> { TestAccount };
        TestCompany.Stocks = new List<Stock>();

        _createReceiptRequestDto.AccountId = TestAccount.Id;

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
           .Setup(u => u.GetGenericRepository<Company>()
           .GetByIdAsync(_createReceiptRequestDto.CompanyId, c => c.Stocks, c => c.Accounts))
           .ReturnsAsync(TestCompany);

        // Act
        var result = await _receiptService.CreateReceiptAsync(_createReceiptRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<ReceiptResponseDto>.Fail(ErrorMessageService.StockNotFound404, 404));
    }

    [Fact]
    public async Task CreateReceiptAsync_ShouldReturnNotFound_WhenAccountNotExistOrFound()
    {
        // Arrange
        SetupAuthenticatedUser(role: "user");
        TestCompany.UserId = TestUser.Id;

        _createReceiptRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = null;

        _createReceiptRequestDto.StockId = TestStock.Id;
        TestCompany.Accounts = new List<Account>();

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
           .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createReceiptRequestDto.CompanyId, c => c.Stocks, c => c.Accounts))
           .ReturnsAsync(TestCompany);

        // Act
        var result = await _receiptService.CreateReceiptAsync(_createReceiptRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<ReceiptResponseDto>.Fail(ErrorMessageService.AccountNotFound404, 404));
    }

    [Fact]
    public async Task CreateReceiptAsync_ShouldReturnInsufficientQuantity_WhenStockQuantityLesserThanRequestQuantity()
    {
        // Arrange
        SetupAuthenticatedUser(role: "user");
        TestCompany.UserId = TestUser.Id;

        _createReceiptRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = null;

        _createReceiptRequestDto.StockId = TestStock.Id;
        TestCompany.Accounts = new List<Account> { TestAccount };
        TestCompany.Stocks = new List<Stock> { TestStock };

        _createReceiptRequestDto.AccountId = TestAccount.Id;

        TestStock.Quantity = 5;

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createReceiptRequestDto.CompanyId, c => c.Stocks, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _receiptService.CreateReceiptAsync(_createReceiptRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<ReceiptResponseDto>.Fail(ErrorMessageService.InsufficientStockQuantity400, 400));
    }

    [Fact]
    public async Task CreateReceiptAsync_ShouldReturnInternalServerError_WhenStockTransNotCreated()
    {
        // Arrange
        SetupAuthenticatedUser(role: "user");
        TestCompany.UserId = TestUser.Id;

        _createReceiptRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = null;

        _createReceiptRequestDto.StockId = TestStock.Id;
        TestCompany.Accounts = new List<Account> { TestAccount };
        TestCompany.Stocks = new List<Stock> { TestStock };

        _createReceiptRequestDto.AccountId = TestAccount.Id;

        TestStock.Quantity = 100;

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createReceiptRequestDto.CompanyId, c => c.Stocks, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        MapperMock
           .Setup(m => m.Map<Receipt>(_createReceiptRequestDto))
           .Returns(TestReceipt);

        TestReceipt.CreatedAt = DateTime.UtcNow;

        StockTransDomainServiceMock
            .Setup(s => s.CreateStockTransAsync(It.IsAny<StockTransModel>()))!
            .ReturnsAsync(null as StockTrans);

        // Act
        var result = await _receiptService.CreateReceiptAsync(_createReceiptRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<ReceiptResponseDto>.Fail(ErrorMessageService.InternalServerError500, 500));
    }

    [Fact]
    public async Task CreateReceiptAsync_ShouldReturnInternalServerError_WhenActTransNotCreated()
    {
        // Arrange
        SetupAuthenticatedUser(role: "user");
        TestCompany.UserId = TestUser.Id;

        _createReceiptRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = null;

        _createReceiptRequestDto.StockId = TestStock.Id;
        TestCompany.Accounts = new List<Account> { TestAccount };
        TestCompany.Stocks = new List<Stock> { TestStock };

        _createReceiptRequestDto.AccountId = TestAccount.Id;

        TestStock.Quantity = 100;

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createReceiptRequestDto.CompanyId, c => c.Stocks, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        MapperMock
           .Setup(m => m.Map<Receipt>(_createReceiptRequestDto))
           .Returns(TestReceipt);

        TestReceipt.CreatedAt = DateTime.UtcNow;

        StockTransDomainServiceMock
            .Setup(s => s.CreateStockTransAsync(It.IsAny<StockTransModel>()))!
            .ReturnsAsync(TestStockTrans);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Receipt>().CreateAsync(TestReceipt))
            .ReturnsAsync(TestReceipt);

        ActTransDomainServiceMock
            .Setup(s => s.CreateActTransAsync(It.IsAny<ActTransModel>()))!
            .ReturnsAsync(null as ActTrans);

        // Act
        var result = await _receiptService.CreateReceiptAsync(_createReceiptRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<ReceiptResponseDto>.Fail(ErrorMessageService.InternalServerError500, 500));
    }


    [Fact]
    public async Task CreateReceiptAsync_ShouldReturnSuccess_WhenAllStatementsPass()
    {
        // Arrange
        SetupAuthenticatedUser(role: "user");
        TestCompany.UserId = TestUser.Id;

        _createReceiptRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = null;

        _createReceiptRequestDto.StockId = TestStock.Id;
        TestCompany.Accounts = new List<Account> { TestAccount };
        TestCompany.Stocks = new List<Stock> { TestStock };

        _createReceiptRequestDto.AccountId = TestAccount.Id;

        TestStock.Quantity = 100;

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createReceiptRequestDto.CompanyId, c => c.Stocks, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        MapperMock
           .Setup(m => m.Map<Receipt>(_createReceiptRequestDto))
           .Returns(TestReceipt);

        TestReceipt.CreatedAt = DateTime.UtcNow;

        StockTransDomainServiceMock
            .Setup(s => s.CreateStockTransAsync(It.IsAny<StockTransModel>()))!
            .ReturnsAsync(TestStockTrans);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Receipt>().CreateAsync(TestReceipt))
            .ReturnsAsync(TestReceipt);

        ActTransDomainServiceMock
           .Setup(s => s.CreateActTransAsync(It.IsAny<ActTransModel>()))!
           .ReturnsAsync(TestActTrans);

        MapperMock
           .Setup(m => m.Map<ReceiptResponseDto>(TestReceipt))
           .Returns(_receiptResponseDto);

        // Act
        var result = await _receiptService.CreateReceiptAsync(_createReceiptRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<ReceiptResponseDto>.Success(_receiptResponseDto, 201));
    }


    // Delete Receipt Tests
    [Fact]
    public async Task DeleteReceiptAsync_ShouldReturnNotFound_WhenReceiptNotExist()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Receipt>().GetByIdAsync(TestReceipt.Id))
            .ReturnsAsync(null as Receipt);

        // Act
        var result = await _receiptService.DeleteReceiptAsync(TestReceipt.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<NoContentDto>.Fail(ErrorMessageService.ReceitNotFound404, 404));
    }

    [Fact]
    public async Task DeleteReceiptAsync_ShouldReturnNoContet_WhenAllStatementsPass()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Receipt>().GetByIdAsync(TestReceipt.Id))
            .ReturnsAsync(TestReceipt);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Receipt>().DeleteAsync(TestReceipt))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _receiptService.DeleteReceiptAsync(TestReceipt.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<NoContentDto>.Success(204));
    }


    // Get All Receipts Tests
    [Fact]
    public async Task GetAllReceiptsAsync_ShouldRetrunUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        SetupUnauthenticatedUser();

        // Act
        var result = await _receiptService.GetAllReceiptsAsync();

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.Unauthorized401,StatusCodes.Status401Unauthorized));
    }

    [Fact]
    public async Task GetAllReceiptsAsync_ShouldRetrunRestrictedAccess_WhenUserNotAdmin()
    {
        // Arrange
        SetupAuthenticatedUser(role:"user");

        // Act
        var result = await _receiptService.GetAllReceiptsAsync();

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.RestrictedAccess403, StatusCodes.Status403Forbidden));
    }

    [Fact]
    public async Task GetAllReceiptsAsync_ShouldRetrunInternalServerError_WhenReceiptListReturnsNull()
    {
        // Arrange
        SetupAuthenticatedUser(role: "admin");

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Receipt>().GetAllAsync(null, null))!
            .ReturnsAsync(null as List<Receipt>);

        // Act
        var result = await _receiptService.GetAllReceiptsAsync();

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.InternalServerError500, StatusCodes.Status500InternalServerError));
    }

    [Fact]
    public async Task GetAllReceiptsAsync_ShouldRetrunSuccess_WhenAllStatementPass()
    {
        // Arrange
        var receiptList = new List<Receipt>
        {
            TestReceipt
        };

        var responseList = new List<ReceiptResponseDto>
        {
            _receiptResponseDto
        };

        SetupAuthenticatedUser(role: "admin");

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Receipt>().GetAllAsync(null,null))
            .ReturnsAsync(receiptList);

        MapperMock
            .Setup(m => m.Map<IList<ReceiptResponseDto>>(receiptList))
            .Returns(responseList);

        // Act
        var result = await _receiptService.GetAllReceiptsAsync();

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Success(responseList, StatusCodes.Status200OK));
    }


    // Get Company Receipts By Account Id Tests
    [Fact]
    public async Task GetCompanyReceiptsByAccountId_ShouldReturnNotFound_WhenCompanyNotExit()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Receipts, c => c.Accounts))
            .ReturnsAsync(null as Company);

        // Act
        var result = await _receiptService.GetCompanyReceiptsByAccountIdAsync(TestCompany.Id, TestAccount.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound));
    }

    [Fact]
    public async Task GetCompanyReceiptsByAccountId_ShouldReturnNotFound_WhenCompanyIsDeleted()
    {
        // Arrange
        TestCompany.DeletedAt = DateTime.UtcNow;
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Receipts, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _receiptService.GetCompanyReceiptsByAccountIdAsync(TestCompany.Id, TestAccount.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound));
    }

    [Fact]
    public async Task GetCompanyReceiptsByAccountId_ShouldReturnRestrictedAccess_WhenAcountNotInCompanyAccountsList()
    {
        // Arrange
        TestCompany.Accounts = new List<Account>
        {
            TestDataFactory.CreateTestAccount(email: "t1@mail", phoneNumber:"3123123")
        };

        TestCompany.DeletedAt = null;

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Receipts, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _receiptService.GetCompanyReceiptsByAccountIdAsync(TestCompany.Id, TestAccount.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.RestrictedAccess403, StatusCodes.Status403Forbidden));
    }

    [Fact]
    public async Task GetCompanyReceiptsByAccountId_ShouldReturnSuccess_WhenAllStatementsPass()
    {
        var testAccount = TestDataFactory.CreateTestAccount(email: "t1@mail", phoneNumber: "3123123");
        // Arrange
        TestCompany.Accounts = new List<Account>
        {
            testAccount
        };

        TestReceipt.AccountId = testAccount.Id;

        TestCompany.Receipts = new List<Receipt> { TestReceipt };

        TestCompany.DeletedAt = null;

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Receipts, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        var responseList = new List<ReceiptResponseDto> { new ReceiptResponseDto() };

        MapperMock
           .Setup(m => m.Map<IList<ReceiptResponseDto>>(TestCompany.Receipts))
           .Returns(responseList);

        // Act
        var result = await _receiptService.GetCompanyReceiptsByAccountIdAsync(TestCompany.Id, testAccount.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Success(responseList, StatusCodes.Status200OK));
    }


    //Get Receipts By Company Id Tests
    [Fact]
    public async Task GetReceiptsByCompanyId_ShouldReturnNotFound_WhenCompanyNotExist()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Receipts, c => c.Accounts))
            .ReturnsAsync(null as Company);

        // Act
        var result = await _receiptService.GetReceiptsByCompanyIdAsync(TestCompany.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound));
    }

    [Fact]
    public async Task GetReceiptsByCompanyId_ShouldReturnNotFound_WhenCompanyIsDeleted()
    {
        // Arrange
        TestCompany.DeletedAt = DateTime.UtcNow;
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Receipts, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _receiptService.GetReceiptsByCompanyIdAsync(TestCompany.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound));
    }

    [Fact]
    public async Task GetReceiptsByCompanyId_ShouldReturnSuccess_WhenAllStatementsPass()
    {
        // Arrange
        var responseList = new List<ReceiptResponseDto> { _receiptResponseDto };

        TestCompany.Receipts = new List<Receipt> { TestReceipt };

        TestCompany.DeletedAt = null;
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Receipts, c => c.Accounts))
            .ReturnsAsync(TestCompany);


        MapperMock
           .Setup(m => m.Map<IList<ReceiptResponseDto>>(TestCompany.Receipts))
           .Returns(responseList);

        // Act
        var result = await _receiptService.GetReceiptsByCompanyIdAsync(TestCompany.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Success(responseList, StatusCodes.Status200OK));
    }


    // Get Company Receipts By Account Id And Stock Id Tests
    [Fact]
    public async Task GetCompanyReceiptsByAccountIdAndStockId_ShouldReturnNotFound_WhenCompanyNotExist()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Receipts, c => c.Accounts))
            .ReturnsAsync(null as Company);

        // Act
        var result = await _receiptService.GetCompanyReceiptsByAccountIdAndStockIdAsync(TestCompany.Id, TestAccount.Id, TestStock.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound));
    }

    [Fact]
    public async Task GetCompanyReceiptsByAccountIdAndStockId_ShouldReturnNotFound_WhenCompanyIsDeleted()
    {
        // Arrange
        TestCompany.DeletedAt = DateTime.UtcNow;
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Receipts, c => c.Accounts))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _receiptService.GetCompanyReceiptsByAccountIdAndStockIdAsync(TestCompany.Id, TestAccount.Id, TestStock.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, StatusCodes.Status404NotFound));
    }

    [Fact]
    public async Task GetCompanyReceiptsByAccountIdAndStockId_ShouldReturnNotFound_WhenAccountListIsEmpty()
    {
        // Arrange
        TestCompany.DeletedAt = null;
        TestCompany.Accounts = new List<Account>();

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Receipts, c => c.Accounts, c => c.Stocks))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _receiptService.GetCompanyReceiptsByAccountIdAndStockIdAsync(TestCompany.Id, TestAccount.Id, TestStock.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.AccountNotFound404, StatusCodes.Status404NotFound));
    }

    [Fact]
    public async Task GetCompanyReceiptsByAccountIdAndStockId_ShouldReturnRestrictedAccess_WhenAcountNotInCompanyAccountsList()
    {
        // Arrange
        TestCompany.DeletedAt = null;
        TestCompany.Accounts = new List<Account> { TestDataFactory.CreateTestAccount() };
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Receipts, c => c.Accounts, c => c.Stocks))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _receiptService.GetCompanyReceiptsByAccountIdAndStockIdAsync(TestCompany.Id, TestAccount.Id, TestStock.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.RestrictedAccess403, StatusCodes.Status403Forbidden));
    }

    [Fact]
    public async Task GetCompanyReceiptsByAccountIdAndStockId_ShouldReturnNotFound_WhenStockListIsEmpty()
    {
        // Arrange
        TestCompany.DeletedAt = null;
        TestCompany.Accounts = new List<Account> { TestAccount };
        TestCompany.Stocks = new List<Stock>();

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Receipts, c => c.Accounts, c => c.Stocks))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _receiptService.GetCompanyReceiptsByAccountIdAndStockIdAsync(TestCompany.Id, TestAccount.Id, TestStock.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.StockNotFound404, StatusCodes.Status404NotFound));
    }

    [Fact]
    public async Task GetCompanyReceiptsByAccountIdAndStockId_ShouldReturnRestrictedAccess_WhenStockNotInCompanyStocksList()
    {
        // Arrange
        TestCompany.DeletedAt = null;
        TestCompany.Accounts = new List<Account> { TestAccount };
        TestCompany.Stocks = new List<Stock> { TestDataFactory.CreateTestStock() };

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Receipts, c => c.Accounts, c => c.Stocks))
            .ReturnsAsync(TestCompany);

        // Act
        var result = await _receiptService.GetCompanyReceiptsByAccountIdAndStockIdAsync(TestCompany.Id, TestAccount.Id, TestStock.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Fail(ErrorMessageService.RestrictedAccess403, StatusCodes.Status403Forbidden));
    }

    [Fact]
    public async Task GetCompanyReceiptsByAccountIdAndStockId_ShouldReturnSuccess_WhenAllStatementsPass()
    {
        // Arrange
        var responseList = new List<ReceiptResponseDto> { _receiptResponseDto };
        var receiptList = new List<Receipt> { TestReceipt };

        TestCompany.DeletedAt = null;
        TestCompany.Accounts = new List<Account> { TestAccount };
        TestCompany.Stocks = new List<Stock> { TestStock };
        TestCompany.Receipts = new List<Receipt> { TestReceipt };

        TestReceipt.AccountId = TestAccount.Id;
        TestReceipt.StockId = TestStock.Id;
        
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.Receipts, c => c.Accounts, c => c.Stocks))
            .ReturnsAsync(TestCompany);

        MapperMock
           .Setup(m => m.Map<IList<ReceiptResponseDto>>(receiptList))
           .Returns(responseList);

        // Act
        var result = await _receiptService.GetCompanyReceiptsByAccountIdAndStockIdAsync(TestCompany.Id, TestAccount.Id, TestStock.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<ReceiptResponseDto>>.Success(responseList, StatusCodes.Status200OK));
    }
}
