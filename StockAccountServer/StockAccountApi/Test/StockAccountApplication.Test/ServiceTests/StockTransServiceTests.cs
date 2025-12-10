using FluentAssertions;
using Moq;
using StockAccountApplication.Services;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.StockTrans;
using StockAccountContracts.Dtos.StockTrans.Create;
using StockAccountDomain.Entities;
using StockAccountDomain.Enums;
using Xunit.Abstractions;

namespace StockAccountApplication.Test.ServiceTests;

public class StockTransServiceTests : ServiceTestBase
{
    private readonly ITestOutputHelper _output;
    private readonly StockTransService _stockTransService;
    private readonly CreateStockTransRequestDto _createStockTransRequestDtoIn = new()
    {
        CompanyId = Guid.NewGuid(),
        StockId = Guid.NewGuid(),
        CounterpartyCompanyId = Guid.NewGuid(),
        Type = StockTransTypeEnum.In,
        Quantity = 100,
        UnitPrice = 10,
        TotalPrice = 1000,
        Description = "Valid stock transaction"
    };

    private readonly CreateStockTransRequestDto _createStockTransRequestDtoOut = new()
    {
        CompanyId = Guid.NewGuid(),
        StockId = Guid.NewGuid(),
        CounterpartyCompanyId = Guid.NewGuid(),
        Type = StockTransTypeEnum.Out,
        Quantity = 100,
        UnitPrice = 10,
        TotalPrice = 1000,
        Description = "Valid stock transaction"
    };

    private readonly StockTransResponseDto _stockTransResponseDto = new()
    {
        CompanyId = Guid.NewGuid(),
        StockId = Guid.NewGuid(),
        CounterpartyCompanyId = Guid.NewGuid(),
        Type = StockTransTypeEnum.Out,
        Quantity = 100,
        UnitPrice = 10,
        TotalPrice = 1000,
        Description = "Valid stock transaction"
    };

    public StockTransServiceTests(ITestOutputHelper output)
    {
        _stockTransService = new StockTransService(
            UnitOfWorkMock.Object,
            MapperMock.Object,
            ValidationServiceMock.Object,
            HttpContextAccessorMock.Object
        );
        _output = output;
    }


    // Create Stock Transaction Tests
    [Fact]
    public async Task CreateStockTransAsync_ShouldReturnValidationError_WhenRequestNotValid()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateStockTransRequestDto, StockTransResponseDto>(_createStockTransRequestDtoIn))
            .ReturnsAsync(ResponseDto<StockTransResponseDto>.Fail("Validation Error", 400));

        // Act
        var result = await _stockTransService.CreateStockTransactionAsync(_createStockTransRequestDtoIn);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<StockTransResponseDto>.Fail("Validation Error",400));
        result.StatusCode.Should().Be(400);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateStockTransAsync_ShouldReturnNotFound_WhenCompanyNotExists()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateStockTransRequestDto, StockTransResponseDto>(_createStockTransRequestDtoIn))
            .ReturnsAsync((ResponseDto<StockTransResponseDto>?)null);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>()
                .GetByIdAsync(_createStockTransRequestDtoIn.CompanyId, c => c.Stocks, c => c.User))
            .ReturnsAsync((Company?)null);

        // Act
        var result = await _stockTransService.CreateStockTransactionAsync(_createStockTransRequestDtoIn);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<StockTransResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, 404));
        result.StatusCode.Should().Be(404);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateStockTransAsync_ShouldReturnRestrictedAccess_WhenUserNotMatch()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateStockTransRequestDto, StockTransResponseDto>(_createStockTransRequestDtoIn))
            .ReturnsAsync((ResponseDto<StockTransResponseDto>?)null);

        TestCompany.DeletedAt = null;
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>()
                .GetByIdAsync(_createStockTransRequestDtoIn.CompanyId, c => c.Stocks, c => c.User))
            .ReturnsAsync(TestCompany);

        SetupAuthenticatedUser(role: "user");

        // Act
        var result = await _stockTransService.CreateStockTransactionAsync(_createStockTransRequestDtoIn);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<StockTransResponseDto>.Fail(ErrorMessageService.RestrictedAccess403, 403));
        result.StatusCode.Should().Be(403);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateStockTransAsync_ShouldReturnNotFound_WhenCounterpartyCompanyDoesNotExist()
    {
        // Arrange
        _createStockTransRequestDtoIn.CounterpartyCompanyId = Guid.NewGuid();

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateStockTransRequestDto, StockTransResponseDto>(_createStockTransRequestDtoIn))
            .ReturnsAsync((ResponseDto<StockTransResponseDto>?)null);

        TestCompany.DeletedAt = null;
        TestCompany.UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>()
                .GetByIdAsync(_createStockTransRequestDtoIn.CompanyId, c => c.Stocks, c => c.User))
            .ReturnsAsync(TestCompany);

        SetupAuthenticatedUser(role: "user");

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createStockTransRequestDtoIn.CounterpartyCompanyId.Value))
            .ReturnsAsync((Company?)null);

        // Act
        var result = await _stockTransService.CreateStockTransactionAsync(_createStockTransRequestDtoIn);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<StockTransResponseDto>.Fail(ErrorMessageService.CountercompanyNotFound404, 404));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateStockTransAsync_ShouldReturnNotFound_WhenCounterpartyCompanyIsDeleted()
    {
        // Arrange
        _createStockTransRequestDtoIn.CounterpartyCompanyId = Guid.NewGuid();

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateStockTransRequestDto, StockTransResponseDto>(_createStockTransRequestDtoIn))
            .ReturnsAsync((ResponseDto<StockTransResponseDto>?)null);

        TestCompany.DeletedAt = null;
        TestCompany.UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>()
                .GetByIdAsync(_createStockTransRequestDtoIn.CompanyId, c => c.Stocks, c => c.User))
            .ReturnsAsync(TestCompany);

        SetupAuthenticatedUser(role: "user");

        var deletedCompany = TestDataFactory.CreateTestCompany();
        deletedCompany.DeletedAt = DateTime.UtcNow;

        // Repo bu silinmiş şirketi dönsün
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createStockTransRequestDtoIn.CounterpartyCompanyId.Value))
            .ReturnsAsync(deletedCompany);

        // Act
        var result = await _stockTransService.CreateStockTransactionAsync(_createStockTransRequestDtoIn);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<StockTransResponseDto>.Fail(ErrorMessageService.CountercompanyNotFound404, 404));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateStockTransAsync_ShouldReturnNotFound_WhenStockNotExist()
    {
        // Arrange
        _createStockTransRequestDtoIn.CounterpartyCompanyId = Guid.NewGuid();

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateStockTransRequestDto, StockTransResponseDto>(_createStockTransRequestDtoIn))
            .ReturnsAsync((ResponseDto<StockTransResponseDto>?)null);

        TestCompany.DeletedAt = null;
        TestCompany.UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>()
                .GetByIdAsync(_createStockTransRequestDtoIn.CompanyId, c => c.Stocks, c => c.User))
            .ReturnsAsync(TestCompany);

        TestCompany.Stocks = new List<Stock> { TestStock };

        SetupAuthenticatedUser(role: "user");

        var counterPartyCompany = TestDataFactory.CreateTestCompany();
        counterPartyCompany.DeletedAt = null;

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createStockTransRequestDtoIn.CounterpartyCompanyId.Value))
            .ReturnsAsync(counterPartyCompany);

       
        // Act
        var result = await _stockTransService.CreateStockTransactionAsync(_createStockTransRequestDtoIn);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<StockTransResponseDto>.Fail(ErrorMessageService.StockNotFound404, 404));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateStockTransAsync_ShouldReturnNotFound_WhenStockIsDeleted()
    {
        // Arrange
        _createStockTransRequestDtoIn.CounterpartyCompanyId = Guid.NewGuid();

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateStockTransRequestDto, StockTransResponseDto>(_createStockTransRequestDtoIn))
            .ReturnsAsync((ResponseDto<StockTransResponseDto>?)null);

        TestCompany.DeletedAt = null;
        TestCompany.UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");


        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>()
                .GetByIdAsync(_createStockTransRequestDtoIn.CompanyId, c => c.Stocks, c => c.User))
            .ReturnsAsync(TestCompany);
        
        TestStock.DeletedAt = DateTime.UtcNow;
        TestStock.Id = _createStockTransRequestDtoIn.StockId;
        TestCompany.Stocks = new List<Stock> { TestStock };

        SetupAuthenticatedUser(role: "user");

        var counterPartyCompany = TestDataFactory.CreateTestCompany();
        counterPartyCompany.DeletedAt = null;

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createStockTransRequestDtoIn.CounterpartyCompanyId.Value))
            .ReturnsAsync(counterPartyCompany);


        // Act
        var result = await _stockTransService.CreateStockTransactionAsync(_createStockTransRequestDtoIn);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<StockTransResponseDto>.Fail(ErrorMessageService.StockNotFound404, 404));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateStockTransAsync_ShouldReturnInsufficientStockQuantity_WhenStockQuantityNotEnoughForOutTrans()
    {
        _createStockTransRequestDtoIn.CounterpartyCompanyId = Guid.NewGuid();

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateStockTransRequestDto, StockTransResponseDto>(_createStockTransRequestDtoOut))
            .ReturnsAsync((ResponseDto<StockTransResponseDto>?)null);

        TestCompany.DeletedAt = null;
        TestCompany.UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");


        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>()
                .GetByIdAsync(_createStockTransRequestDtoOut.CompanyId, c => c.Stocks, c => c.User))
            .ReturnsAsync(TestCompany);

        TestStock.DeletedAt = null;
        TestStock.Id = _createStockTransRequestDtoOut.StockId;
        TestCompany.Stocks = new List<Stock> { TestStock };

        SetupAuthenticatedUser(role: "user");

        var counterPartyCompany = TestDataFactory.CreateTestCompany();
        counterPartyCompany.DeletedAt = null;

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createStockTransRequestDtoOut.CounterpartyCompanyId.Value))
            .ReturnsAsync(counterPartyCompany);

        TestStock.Quantity = 5;

        // Act
        var result = await _stockTransService.CreateStockTransactionAsync(_createStockTransRequestDtoOut);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<StockTransResponseDto>.Fail(ErrorMessageService.InsufficientStockQuantity400, 400));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }
    [Fact]
    public async Task CreateStockTransAsync_ShouldCreateTransaction_WhenAllStatementsPass()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateStockTransRequestDto, StockTransResponseDto>(_createStockTransRequestDtoIn))
            .ReturnsAsync((ResponseDto<StockTransResponseDto>?)null);

        TestCompany.DeletedAt = null;
        TestCompany.UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>()
                .GetByIdAsync(_createStockTransRequestDtoIn.CompanyId, c=> c.Stocks, c => c.User))
            .ReturnsAsync(TestCompany);

        TestStock.DeletedAt = null;
        TestStock.Id = _createStockTransRequestDtoIn.StockId;
        TestStock.Quantity = 50m; // starting quantity
        TestCompany.Stocks = new List<Stock> { TestStock };

        var counterParty = TestDataFactory.CreateTestCompany();
        counterParty.DeletedAt = null;
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(_createStockTransRequestDtoIn.CounterpartyCompanyId.Value))
            .ReturnsAsync(counterParty);

        SetupAuthenticatedUser(role: "user");

        var mappedStockTrans = new StockTrans
        {
            Id = Guid.NewGuid(),
            CompanyId = _createStockTransRequestDtoIn.CompanyId,
            StockId = _createStockTransRequestDtoIn.StockId,
            Quantity = _createStockTransRequestDtoIn.Quantity,
            UnitPrice = _createStockTransRequestDtoIn.UnitPrice,
        };

        MapperMock
            .Setup(m => m.Map<StockTrans>(_createStockTransRequestDtoIn))
            .Returns(mappedStockTrans);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<StockTrans>().CreateAsync(mappedStockTrans))
            .ReturnsAsync(mappedStockTrans);

        MapperMock
            .Setup(m => m.Map<StockTransResponseDto>(mappedStockTrans))
            .Returns(_stockTransResponseDto);

        // Act
        var result = await _stockTransService.CreateStockTransactionAsync(_createStockTransRequestDtoIn);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<StockTransResponseDto>.Success(_stockTransResponseDto, 201));
        result.StatusCode.Should().Be(201);
        result.IsSuccess.Should().BeTrue();

        TestStock.Quantity.Should().Be(50m + _createStockTransRequestDtoIn.Quantity);

        UnitOfWorkMock.Verify(u => u.GetGenericRepository<StockTrans>().CreateAsync(mappedStockTrans), Times.Once);
    }


    // Delete Stock Transaction Test
    [Fact]
    public async Task DeleteStockTransAsync_ShouldReturnNotFound_WhenStockTransNotExist()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<StockTrans>().GetByIdAsync(TestStockTrans.Id))
            .ReturnsAsync(null as StockTrans);

        // Act
        var result = await _stockTransService.DeleteStockTransactionAsync(TestStockTrans.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<NoContentDto>.Fail(ErrorMessageService.StockTransNotFound404, 404));
    }

    [Fact]
    public async Task DeleteStockTransAsync_ShouldReturnSuccess_WhenAllStatementsPass()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<StockTrans>().GetByIdAsync(TestStockTrans.Id))
            .ReturnsAsync(TestStockTrans);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<StockTrans>().DeleteAsync(TestStockTrans))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _stockTransService.DeleteStockTransactionAsync(TestStockTrans.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<NoContentDto>.Success(204));
    }


    // Get All Stock Transactions Tests
    [Fact]
    public async Task GetAllStockTransAsync_ShoulReturnRestrictedAccess_WhenUserIsNotAdmnin()
    {
        // Arrange
        SetupAuthenticatedUser(role: "user");

        // Act
        var result = await _stockTransService.GetAllStockTransactionsAsync();

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<StockTransResponseDto>>.Fail(ErrorMessageService.RestrictedAccess403, 403));
    }

    [Fact]
    public async Task GetAllStockTransAsync_ShoulReturnNotFound_WhenStockTransNotExist()
    {
        // Arrange
        SetupAuthenticatedUser(role:"admin");

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<StockTrans>().GetAllAsync(x => x.DeletedAt == null, null))
            .ReturnsAsync(new List<StockTrans> { });

        // Act
        var result = await _stockTransService.GetAllStockTransactionsAsync();

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<StockTransResponseDto>>.Fail(ErrorMessageService.StockTransNotFound404, 404));
    }

    [Fact]
    public async Task GetAllStockTransAsync_ShoulReturnSuccess_WhenAllStatementsPass()
    {
        // Arrange
        SetupAuthenticatedUser(role: "admin");

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<StockTrans>().GetAllAsync(x => x.DeletedAt == null, null))
            .ReturnsAsync(new List<StockTrans> { TestStockTrans });

        var responseList = new List<StockTransResponseDto>
        {
            _stockTransResponseDto,
        };

        MapperMock
            .Setup(m => m.Map<IList<StockTransResponseDto>>(new List<StockTrans> { TestStockTrans }))
            .Returns(responseList);

        // Act
        var result = await _stockTransService.GetAllStockTransactionsAsync();

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<StockTransResponseDto>>.Success(responseList, 200));
    }


    // Get Transactions By Stock Id Tests
    [Fact]
    public async Task GetTransactionsByStockIdAsync_ShouldReturnNotFound_WhenStockNotExist()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>().GetByIdAsync(TestStock.Id, s => s.StockTransactions))
            .ReturnsAsync(null as Stock);

        // Act
        var result = await _stockTransService.GetTransactionsByStockIdAsync(TestStock.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<StockTransResponseDto>>.Fail(ErrorMessageService.StockNotFound404, 404));
    }

    [Fact]
    public async Task GetTransactionsByStockIdAsync_ShouldReturnNotFound_WhenStockIsDeleted()
    {
        // Arrange
        TestStock.DeletedAt = DateTime.UtcNow;
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>().GetByIdAsync(TestStock.Id, s => s.StockTransactions))
            .ReturnsAsync(TestStock);

        // Act
        var result = await _stockTransService.GetTransactionsByStockIdAsync(TestStock.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<StockTransResponseDto>>.Fail(ErrorMessageService.StockNotFound404, 404));
    }

    [Fact]
    public async Task GetTransactionsByStockIdAsync_ShouldReturnNotFound_WhenStockHasNoTransaction()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>().GetByIdAsync(TestStock.Id, s => s.StockTransactions))
            .ReturnsAsync(TestStock);

        TestStock.StockTransactions = [];

        // Act
        var result = await _stockTransService.GetTransactionsByStockIdAsync(TestStock.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<StockTransResponseDto>>.Fail(ErrorMessageService.StockTransNotFound404, 404));
    }

    [Fact]
    public async Task GetTransactionsByStockIdAsync_ShouldReturnSuccess_WhenAllStatementsPass() { 
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>().GetByIdAsync(TestStock.Id, s => s.StockTransactions))
            .ReturnsAsync(TestStock);

        TestStock.StockTransactions = 
        [
            TestStockTrans
        ];

        var responseList = new List<StockTransResponseDto>{
            _stockTransResponseDto 
        };
        MapperMock
            .Setup(m => m.Map<IList<StockTransResponseDto>>(TestStock.StockTransactions))
            .Returns(responseList);

        // Act
        var result = await _stockTransService.GetTransactionsByStockIdAsync(TestStock.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<StockTransResponseDto>>.Success(responseList, 200));
    }
}
