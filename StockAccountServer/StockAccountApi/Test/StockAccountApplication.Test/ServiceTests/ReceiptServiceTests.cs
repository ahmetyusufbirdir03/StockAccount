using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using StockAccountApplication.Services;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Account;
using StockAccountContracts.Dtos.Receipt;
using StockAccountContracts.Dtos.Receipt.Create;
using StockAccountDomain.Entities;
using StockAccountDomain.Enums;

namespace StockAccountApplication.Test.ServiceTests;

public class ReceiptServiceTests : ServiceTestBase
{
    private readonly ReceiptService _receiptService;

    private CreateReceiptRequestDto _createReceiptRequestDto = new CreateReceiptRequestDto
    {
        AccountId = Guid.NewGuid(),
        CompanyId = Guid.NewGuid(),
        StockId = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow,
        Quantity = 100,
        Type = ReceiptTypeEnum.Sale,
        UnitCurrentPrice = 100,
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
            MapperMock.Object
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
    public async Task CreateReceiptAsync_ShouldReturnNotFound_WhenCompanyNotExist()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createReceiptRequestDto.CompanyId))
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
        _createReceiptRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = DateTime.UtcNow;
        
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createReceiptRequestDto.CompanyId, c => c.Stocks))
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
        _createReceiptRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = null;
        TestCompany.Stocks = new List<Stock>
        {
            TestStock
        };

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createReceiptRequestDto.CompanyId, c => c.Stocks))
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
        _createReceiptRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = null;
        _createReceiptRequestDto.StockId = TestStock.Id;
        TestCompany.Stocks = new List<Stock>
        {
            TestStock
        };

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createReceiptRequestDto.CompanyId, c => c.Stocks))
            .ReturnsAsync(TestCompany);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().GetByIdAsync(_createReceiptRequestDto.AccountId))
            .ReturnsAsync(null as Account);

        // Act
        var result = await _receiptService.CreateReceiptAsync(_createReceiptRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<ReceiptResponseDto>.Fail(ErrorMessageService.AccountNotFound404, 404));
    }

    [Fact]
    public async Task CreateReceiptAsync_ShouldReturnInsufficientQuantity_WhenStockQuantityLesserThanRequestQuantity()
    {
        // Arrange
        _createReceiptRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = null;

        _createReceiptRequestDto.StockId = TestStock.Id;
        TestStock.Quantity = 10;

        _createReceiptRequestDto.AccountId = TestAccount.Id;

        TestCompany.Stocks = new List<Stock>
        {
            TestStock
        };

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createReceiptRequestDto.CompanyId, c => c.Stocks))
            .ReturnsAsync(TestCompany);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().GetByIdAsync(_createReceiptRequestDto.AccountId))
            .ReturnsAsync(TestAccount);

        // Act
        var result = await _receiptService.CreateReceiptAsync(_createReceiptRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<ReceiptResponseDto>.Fail(ErrorMessageService.InsufficientStockQuantity400, 400));
    }

    [Fact]
    public async Task CreateReceiptAsync_ShouldReturnSuccess_WhenAllStatementsPass()
    {
        // Arrange
        _createReceiptRequestDto.CompanyId = TestCompany.Id;
        TestCompany.DeletedAt = null;

        _createReceiptRequestDto.StockId = TestStock.Id;
        TestStock.Quantity = 200;

        _createReceiptRequestDto.AccountId = TestAccount.Id;

        TestCompany.Stocks = new List<Stock>
        {
            TestStock
        };

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateReceiptRequestDto, ReceiptResponseDto>(_createReceiptRequestDto))
            .ReturnsAsync(null as ResponseDto<ReceiptResponseDto>);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createReceiptRequestDto.CompanyId, c => c.Stocks))
            .ReturnsAsync(TestCompany);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Account>().GetByIdAsync(_createReceiptRequestDto.AccountId))
            .ReturnsAsync(TestAccount);

        MapperMock
            .Setup(m => m.Map<Receipt>(_createReceiptRequestDto))
            .Returns(TestReceipt);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Receipt>().CreateAsync(TestReceipt))
            .ReturnsAsync(TestReceipt);

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

    // Get Company Receipts By Account Id Tests devam 
}
