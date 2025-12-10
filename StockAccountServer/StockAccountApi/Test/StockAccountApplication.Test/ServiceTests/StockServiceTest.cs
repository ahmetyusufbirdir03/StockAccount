using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using StockAccountApplication.Services;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Stock;
using StockAccountContracts.Dtos.Stock.Create;
using StockAccountContracts.Dtos.Stock.QuantityUpdate;
using StockAccountContracts.Dtos.Stock.Update;
using StockAccountDomain.Entities;
using StockAccountDomain.Enums;
using StockAccountDomain.Models;
using System.Linq.Expressions;

namespace StockAccountApplication.Test.ServiceTests;

public class StockServiceTest : ServiceTestBase
{
    private readonly StockService _stockService;

    private readonly CreateStockRequestDto _createStockRequest = new CreateStockRequestDto
    {
        CompanyId = Guid.NewGuid(),
        Quantity = 100,
        Price = 50.0m,
        Name = "Test Stock",
        Unit = UnitEnum.Piece,
        Description = "Test Description"
    };

    private readonly UpdateStockRequestDto updateStockRequestDto = new()
    {
        Id = Guid.NewGuid(),
        Name = "Updated Test Stock",
        Description = "Updated Test Description"
    };

    private readonly StockResponseDto stockResponseDto = new()
    {
        CompanyId = Guid.NewGuid(),
        Quantity = 100,
        Price = 50.0m,
        Name = "Test Stock",
        Unit = UnitEnum.Piece,
        Description = "Test Description"
    };

    private readonly QuantityRequestDto decreaseQuantityInsufficientRequestDto = new()
    {
        StockId = Guid.NewGuid(),
        Amount = 100,
        IsAddition = false,
    };
    private readonly QuantityRequestDto decreaseQuantitySufficientRequestDto = new()
    {
        StockId = Guid.NewGuid(),
        Amount = 3,
        IsAddition = false,
    };
    private readonly QuantityRequestDto increaseQuantityRequestDto = new()
    {
        StockId = Guid.NewGuid(),
        Amount = 10,
        IsAddition = true,
    };

    public StockServiceTest()
    {
         _stockService = new StockService(
            HttpContextAccessorMock.Object,
            ValidationServiceMock.Object,
            MapperMock.Object,
            UnitOfWorkMock.Object,
            StockTransDomainServiceMock.Object
         );
    }

    


    // Create Stock Tests
    [Fact]
    public async Task CreateStockAsync_ShouldReturnValidationError_WhenRequestIsInvalid()
    {
        //Arrange 
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateStockRequestDto, StockResponseDto>(_createStockRequest))
            .ReturnsAsync(ResponseDto<StockResponseDto>.Fail("Validation Error", 400));

        //Act
        var result = await _stockService.CreateStockAsync(_createStockRequest);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<StockResponseDto>.Fail("Validation Error", 400));
    }

    [Fact]
    public async Task CreateStockAsync_ShouldReturnNotFound_WhenCompanyDoesNotExist()
    {
        //Arrange 
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateStockRequestDto, StockResponseDto>(_createStockRequest))
            .ReturnsAsync((ResponseDto<StockResponseDto>)null!);

        UnitOfWorkMock
            .Setup(c => c.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id, c => c.DeletedAt == null))
            .ReturnsAsync((Company?)null);

        //Act
        var result = await _stockService.CreateStockAsync(_createStockRequest);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<StockResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, 404));
    }

    [Fact]
    public async Task CreateStockAsync_ShouldReturnStock_WhenAllStatementPasses()
    {
        //Arrange 
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateStockRequestDto, StockResponseDto>(_createStockRequest))
            .ReturnsAsync((ResponseDto<StockResponseDto>)null!);

        UnitOfWorkMock
            .Setup(c => c.GetGenericRepository<Company>().GetByIdAsync(_createStockRequest.CompanyId))
            .ReturnsAsync(TestCompany);

        TestCompany.DeletedAt = null;

        MapperMock
            .Setup(m => m.Map<Stock>(_createStockRequest))
            .Returns(TestStock);

        TestStock.CreatedAt = DateTime.UtcNow;

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>().CreateAsync(It.IsAny<Stock>()))
            .ReturnsAsync(TestStock);

        MapperMock
            .Setup(m => m.Map<StockResponseDto>(TestStock))
            .Returns(stockResponseDto);

        //Act
        var result = await _stockService.CreateStockAsync(_createStockRequest);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<StockResponseDto>.Success(stockResponseDto, 201));
    }


    // Delete Stock Tests
    [Fact]
    public async Task DeleteStockAsync_ShouldReturnUnauthorized_WhenUserIsNotAdmin()
    {
        //Arrange
        SetupAuthenticatedUser(role: "user");

        //Act
        var result = await _stockService.DeleteStockAsync(TestStock.Id);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<NoContentDto>.Fail(ErrorMessageService.Unauthorized401,401));
    }

    [Fact]
    public async Task DeleteStockAsync_ShouldReturnNotFound_WhenStokcIsNotExist()
    {
        //Arrange
        SetupAuthenticatedUser(role: "admin");

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>().GetByIdAsync(TestStock.Id, s => s.DeletedAt == null))
            .ReturnsAsync((Stock?)null);

        //Act
        var result = await _stockService.DeleteStockAsync(TestStock.Id);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<NoContentDto>.Fail(ErrorMessageService.StockNotFound404, 404));
    }

    [Fact]
    public async Task DeleteStockAsync_ShouldReturnSuccess_WhenAllStatementPasses()
    {
        //Arrange
        SetupAuthenticatedUser(role: "admin");

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>().GetByIdAsync(TestStock.Id))
            .ReturnsAsync(TestStock);

        TestStock.DeletedAt = null;

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>().DeleteAsync(TestStock))
            .Returns(Task.CompletedTask);

        //Act
        var result = await _stockService.DeleteStockAsync(TestStock.Id);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<NoContentDto>.Success(204));
    }

    
    // Get All Stocks Tests
    [Fact]
    public async Task GetAllStocksAsync_ShoulReturnUnauthorized_WhenUserIsNotAdmin()
    {
        //Arrange
        SetupAuthenticatedUser(role: "user");

        //Act
        var result = await _stockService.GetAllStocksAsync();

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<Stock>>.Fail(ErrorMessageService.Unauthorized401, 401));
    }

    [Fact]
    public async Task GetAllStocksAsync_ShoulReturnNotFound_WhenAnyStockIsNotExists()
    {
        //Arrange
        SetupAuthenticatedUser(role: "admin");

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>().GetAllAsync(s => s.DeletedAt == null,null))
            .ReturnsAsync(new List<Stock> { });

        //Act
        var result = await _stockService.GetAllStocksAsync();

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<Stock>>.Fail(ErrorMessageService.StockNotFound404, 404));
    }

    [Fact]
    public async Task GetAllStocksAsync_ShoulReturnSuccess_WhenAllStatementsPasses()
    {
        //Arrange
        SetupAuthenticatedUser(role: "admin");
        var stockList = new List<StockResponseDto>
        {
           stockResponseDto
        };

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>().GetAllAsync(s => s.DeletedAt == null, null))
            .ReturnsAsync(new List<Stock> { TestStock});

        MapperMock
            .Setup(m => m.Map<IList<StockResponseDto>>(new List<Stock> { TestStock}))
            .Returns(stockList);
        //Act
        var result = await _stockService.GetAllStocksAsync();

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<StockResponseDto>>.Success(stockList, 200));
    }


    // Get Company Stocks Tests
    [Fact]
    public async Task GetCompanyStocksAsync_ShouldReturnRestrictedAccess_WhenUserAndCompanyNotMatches()
    {
        //Arrange
        SetupAuthenticatedUser(role: "user");

        TestUser.Companies = new List<Company> {
            TestDataFactory.CreateTestCompany()
        };

        UnitOfWorkMock
        .Setup(u => u.GetGenericRepository<User>()
            .GetByIdAsync(
                TestUser.Id,
                u => u.Companies
            ))
        .ReturnsAsync(TestUser);


        // Act
        var result = await _stockService.GetCompanyStokcsAsync(TestCompany.Id);

        // Assert
        result.Should().BeEquivalentTo(
            ResponseDto<IList<StockResponseDto>>.Fail(ErrorMessageService.RestrictedAccess403, StatusCodes.Status403Forbidden)
        );
    }

    [Fact]
    public async Task GetCompanyStocksAsync_ShouldReturnNotFound_WheNCompanyNotExists()
    {
        // Arrange
        SetupAuthenticatedUser(role: "user");

        TestUser.Companies = new List<Company> { TestCompany };

        UnitOfWorkMock
        .Setup(u => u.GetGenericRepository<User>()
            .GetByIdAsync(
                TestUser.Id,
                u => u.Companies
            ))
        .ReturnsAsync(TestUser);

        UnitOfWorkMock
        .Setup(u => u.GetGenericRepository<Company>()
            .GetByIdAsync(
                TestCompany.Id)
            )
        .ReturnsAsync((Company?)null);


        // Act
        var result = await _stockService.GetCompanyStokcsAsync(TestCompany.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<StockResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, 404));
    }

    [Fact]
    public async Task GetCompanyStocksAsync_ShouldReturnNotFound_WhenStockNotExits()
    {
        // Arrange
        SetupAuthenticatedUser(role: "user");

        TestUser.Companies = new List<Company> { TestCompany };
        TestCompany.Stocks = new List<Stock> { TestDataFactory.CreateTestStock() };

        UnitOfWorkMock
        .Setup(u => u.GetGenericRepository<User>()
            .GetByIdAsync(
                TestUser.Id,
                u => u.Companies
            ))
        .ReturnsAsync(TestUser);

        UnitOfWorkMock
        .Setup(u => u.GetGenericRepository<Company>()
            .GetByIdAsync(
                TestCompany.Id)
            )
        .ReturnsAsync(TestCompany);

        TestCompany.DeletedAt = null;

        UnitOfWorkMock
        .Setup(u => u.GetGenericRepository<Stock>()
            .GetAllAsync(
                It.IsAny<Expression<Func<Stock, bool>>>(),
                null)
            )
        .ReturnsAsync(new List<Stock>() { });


        // Act
        var result = await _stockService.GetCompanyStokcsAsync(TestCompany.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<StockResponseDto>>.Fail(ErrorMessageService.StockNotFound404, 404));
    }

    [Fact]
    public async Task GetCompanyStocksAsync_ShouldReturnSuccess_WhenAllStatementPasses()
    {
        // Arrange
        SetupAuthenticatedUser(role: "user");

        TestUser.Companies = new List<Company> { TestCompany };
        TestCompany.Stocks = new List<Stock> { TestDataFactory.CreateTestStock() };

        var stockList = new List<StockResponseDto>
        {
           stockResponseDto
        };

        UnitOfWorkMock
        .Setup(u => u.GetGenericRepository<User>()
            .GetByIdAsync(
                TestUser.Id,
                u => u.Companies
            ))
        .ReturnsAsync(TestUser);

        UnitOfWorkMock
        .Setup(u => u.GetGenericRepository<Company>()
            .GetByIdAsync(
                TestCompany.Id)
            )
        .ReturnsAsync(TestCompany);

        TestCompany.DeletedAt = null;

        UnitOfWorkMock
        .Setup(u => u.GetGenericRepository<Stock>()
            .GetAllAsync(
                It.IsAny<Expression<Func<Stock, bool>>>(),
                null)
            )
        .ReturnsAsync(new List<Stock>() { TestStock });

        TestStock.DeletedAt = null;

        MapperMock
            .Setup(m => m.Map<IList<StockResponseDto>>(new List<Stock> { TestStock }))
            .Returns(stockList);

        // Act
        var result = await _stockService.GetCompanyStokcsAsync(TestCompany.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<StockResponseDto>>.Success(stockList, 200));
    }


    // Soft Delete Stock Tests
    [Fact]
    public async Task SoftDeleteStockAsync_ShouldReturnNotFound_WhenStokcNotExists()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>()
                .GetByIdAsync(TestStock.Id, s => s.DeletedAt == null)
                )
            .ReturnsAsync((Stock?)null);

        // Act
        var result = await _stockService.SoftDeleteStockAsync(TestCompany.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<NoContentDto>.Fail(ErrorMessageService.StockNotFound404,404));
    }

    [Fact]
    public async Task SoftDeleteStockAsync_ShouldReturnNotFound_WhenCompanyNotExists()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>()
                .GetByIdAsync(TestStock.Id, s => s.Company)
                )
            .ReturnsAsync((Stock?)null);

        // Act
        var result = await _stockService.SoftDeleteStockAsync(TestCompany.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<NoContentDto>.Fail(ErrorMessageService.StockNotFound404, 404));
    }

    [Fact]
    public async Task SoftDeleteStockAsync_ShouldReturnUnauthorized_WhenUserNotExists()
    {
        // Arrange
        UnitOfWorkMock
        .Setup(u => u.GetGenericRepository<Stock>()
            .GetByIdAsync(TestStock.Id, It.IsAny<Expression<Func<Stock, object>>[]>()))
        .ReturnsAsync(TestStock);

        // Act
        var result = await _stockService.SoftDeleteStockAsync(TestStock.Id);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<NoContentDto>.Fail(ErrorMessageService.Unauthorized401, 401));
    }

    [Fact]
    public async Task SoftDeleteStockAsync_ShouldReturnUnauthorized_WhenUserNotMatchWithStock()
    {
        // Arrange
        UnitOfWorkMock
        .Setup(u => u.GetGenericRepository<Stock>()
            .GetByIdAsync(TestStock.Id, It.IsAny<Expression<Func<Stock, object>>[]>()))
        .ReturnsAsync(TestStock);

        SetupAuthenticatedUser(role: "user");

        TestCompany.UserId = Guid.NewGuid();
        TestStock.Company = TestCompany;
        TestStock.DeletedAt = null;

        // Act
        var result = await _stockService.SoftDeleteStockAsync(TestStock.Id);

        // Assert
        result.Should().BeEquivalentTo(
        ResponseDto<NoContentDto>.Fail(ErrorMessageService.RestrictedAccess403, StatusCodes.Status403Forbidden));
    }

    [Fact]
    public async Task SoftDeleteStockAsync_ShouldReturnSuccess_WhenAllStatementsPasses()
    {
        // Arrange
        UnitOfWorkMock
        .Setup(u => u.GetGenericRepository<Stock>()
            .GetByIdAsync(TestStock.Id, It.IsAny<Expression<Func<Stock, object>>[]>()))
        .ReturnsAsync(TestStock);

        SetupAuthenticatedUser(role: "user");

        TestCompany.UserId = TestUser.Id;
        TestStock.Company = TestCompany;
        TestStock.DeletedAt = null;

        // Act
        var result = await _stockService.SoftDeleteStockAsync(TestStock.Id);

        // Assert
        result.Should().BeEquivalentTo(
        ResponseDto<NoContentDto>.Success(StatusCodes.Status204NoContent));
    }


    // Update Stock Tests
    [Fact]
    public async Task UpdateStockAsync_ShouldReturnValidationError_WhenRequestNotValid()
    {
        //Arrange 
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateStockRequestDto, StockResponseDto>(updateStockRequestDto))
            .ReturnsAsync(ResponseDto<StockResponseDto>.Fail("Validation Error", 400));

        //Act
        var result = await _stockService.UpdateStockAsync(updateStockRequestDto);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<StockResponseDto>.Fail("Validation Error", 400));
    }

    [Fact]
    public async Task UpdateStockAsync_ShouldReturnNotFound_WhenStockNotExists()
    {
        //Arrange 
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateStockRequestDto, StockResponseDto>(updateStockRequestDto))
            .ReturnsAsync((ResponseDto<StockResponseDto>?)null);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>()
                .GetByIdAsync(TestStock.Id, s => s.Company))
            .ReturnsAsync((Stock?)null);
        //Act
        var result = await _stockService.UpdateStockAsync(updateStockRequestDto);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<StockResponseDto>.Fail(ErrorMessageService.StockNotFound404, 404));
    }

    [Fact]
    public async Task UpdateStockAsync_ShouldReturnUnauthorized_WhenUserNotExists()
    {
        //Arrange 
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateStockRequestDto, StockResponseDto>(updateStockRequestDto))
            .ReturnsAsync((ResponseDto<StockResponseDto>?)null);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>()
                .GetByIdAsync(updateStockRequestDto.Id, s => s.Company))
            .ReturnsAsync(TestStock);
        //Act
        var result = await _stockService.UpdateStockAsync(updateStockRequestDto);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<StockResponseDto>.Fail(ErrorMessageService.Unauthorized401, 401));
    }

    [Fact]
    public async Task UpdateStockAsync_ShouldReturnNotFound_WhenUserNotMatchWithStock()
    {
        //Arrange 
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateStockRequestDto, StockResponseDto>(updateStockRequestDto))
            .ReturnsAsync((ResponseDto<StockResponseDto>?)null);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>()
                .GetByIdAsync(updateStockRequestDto.Id, s => s.Company))
            .ReturnsAsync(TestStock);

        SetupAuthenticatedUser(role: "user");

        TestStock.Company = TestCompany;

        //Act
        var result = await _stockService.UpdateStockAsync(updateStockRequestDto);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<StockResponseDto>.Fail(ErrorMessageService.RestrictedAccess403, 403));
    }

    [Fact]
    public async Task UpdateStockAsync_ShouldReturnSuccess_WhenAllStatementsPasses()
    {
        //Arrange 
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<UpdateStockRequestDto, StockResponseDto>(updateStockRequestDto))
            .ReturnsAsync((ResponseDto<StockResponseDto>?)null);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>()
                .GetByIdAsync(updateStockRequestDto.Id, s => s.Company))
            .ReturnsAsync(TestStock);

        SetupAuthenticatedUser(role: "user");

        TestStock.Company = TestCompany;

        TestStock.Company.UserId = TestUser.Id;

        MapperMock
            .Setup(m => m.Map(updateStockRequestDto, TestStock))
            .Returns(TestStock);


        MapperMock
            .Setup(m => m.Map<StockResponseDto>(TestStock))
            .Returns(stockResponseDto);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>().UpdateAsync(TestStock))
            .ReturnsAsync(TestStock); 

        //Act
        var result = await _stockService.UpdateStockAsync(updateStockRequestDto);

        //Assert
        result.Data.Should().BeEquivalentTo(stockResponseDto);
        result.StatusCode.Should().Be(200);
        result.IsSuccess.Should().BeTrue();
    }



    // Quantity Update Tests
    [Fact]
    public async Task QuantityUpdateAsync_ShouldReturnValidationError_WhenRequestNotValid()
    {
        //Arrange 
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<QuantityRequestDto, StockResponseDto>(decreaseQuantitySufficientRequestDto))
            .ReturnsAsync(ResponseDto<StockResponseDto>.Fail("Validation Error", 400));

        //Act
        var result = await _stockService.UpdateStockQuantityAsync(decreaseQuantitySufficientRequestDto);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<StockResponseDto>.Fail("Validation Error", 400));

    }

    [Fact]
    public async Task QuantityUpdateAsync_ShouldReturnNotFound_WhenStockNotExist()
    {
        //Arrange 
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<QuantityRequestDto, StockResponseDto>(decreaseQuantitySufficientRequestDto))
            .ReturnsAsync((ResponseDto<StockResponseDto>?)null);

        UnitOfWorkMock
           .Setup(u => u.GetGenericRepository<Stock>()
               .GetByIdAsync(decreaseQuantitySufficientRequestDto.StockId))
           .ReturnsAsync(null as Stock);
        //Act
        var result = await _stockService.UpdateStockQuantityAsync(decreaseQuantitySufficientRequestDto);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<StockResponseDto>.Fail(ErrorMessageService.StockNotFound404, 404));

    }

    [Fact]
    public async Task QuantityUpdateAsync_ShouldReturnNotFound_WhenStockIsDeleted()
    {
        //Arrange 
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<QuantityRequestDto, StockResponseDto>(decreaseQuantitySufficientRequestDto))
            .ReturnsAsync((ResponseDto<StockResponseDto>?)null);

        TestStock.DeletedAt = DateTime.UtcNow;

        UnitOfWorkMock
           .Setup(u => u.GetGenericRepository<Stock>()
               .GetByIdAsync(decreaseQuantitySufficientRequestDto.StockId))
           .ReturnsAsync(TestStock);
        //Act
        var result = await _stockService.UpdateStockQuantityAsync(decreaseQuantitySufficientRequestDto);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<StockResponseDto>.Fail(ErrorMessageService.StockNotFound404, 404));

    }

    [Fact]
    public async Task QuantityUpdateAsync_ShouldIncreseQuantity_WhenIsAdditionIsTrue()
    {
        //Arrange 

        var model = new StockTransModel
        {
            CompanyId = TestStock.CompanyId,
            StockId = TestStock.Id,
            Quantity = increaseQuantityRequestDto.Amount,
            Type = StockTransTypeEnum.In,
            UnitPrice = TestStock.Price,
            TotalPrice = increaseQuantityRequestDto.Amount * TestStock.Price,
        };

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<QuantityRequestDto, StockResponseDto>(increaseQuantityRequestDto))
            .ReturnsAsync((ResponseDto<StockResponseDto>?)null);


        UnitOfWorkMock
           .Setup(u => u.GetGenericRepository<Stock>()
               .GetByIdAsync(increaseQuantityRequestDto.StockId))
           .ReturnsAsync(TestStock);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>().UpdateAsync(TestStock))
            .ReturnsAsync(TestStock);

        MapperMock
            .Setup(m => m.Map<StockResponseDto>(TestStock))
            .Returns(stockResponseDto);

        StockTransDomainServiceMock
            .Setup(s => s.CreateStockTransAsync(It.IsAny<StockTransModel>()))
            .ReturnsAsync(TestStockTrans);


        //Act
        var result = await _stockService.UpdateStockQuantityAsync(increaseQuantityRequestDto);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<StockResponseDto>.Success(stockResponseDto, 200));
        TestStock.Quantity.Should().Be(15);

        StockTransDomainServiceMock.Verify(s =>
        s.CreateStockTransAsync(It.Is<StockTransModel>(m =>
            m.CompanyId == TestStock.CompanyId &&
            m.StockId == TestStock.Id &&
            m.Quantity == increaseQuantityRequestDto.Amount &&
            m.UnitPrice == TestStock.Price &&
            m.Type == StockTransTypeEnum.In &&
            m.TotalPrice == increaseQuantityRequestDto.Amount * TestStock.Price
        )), Times.Once);
    }

    [Fact]
    public async Task QuantityUpdateAsync_ShouldDecreaseQuantity_WhenIsAdditionIsFalseAndStockIsInsufficient()
    {
        //Arrange 
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<QuantityRequestDto, StockResponseDto>(decreaseQuantityInsufficientRequestDto))
            .ReturnsAsync((ResponseDto<StockResponseDto>?)null);


        UnitOfWorkMock
           .Setup(u => u.GetGenericRepository<Stock>()
               .GetByIdAsync(decreaseQuantityInsufficientRequestDto.StockId))
           .ReturnsAsync(TestStock);

        //Act
        var result = await _stockService.UpdateStockQuantityAsync(decreaseQuantityInsufficientRequestDto);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<StockResponseDto>.Fail(ErrorMessageService.InsufficientStockQuantity400,400));
        TestStock.Quantity.Should().NotBe(15);
    }

    [Fact]
    public async Task QuantityUpdateAsync_ShouldDecreseQuantity_WhenIsAdditionIsFalseAndStockIsSufficient()
    {
        //Arrange 
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<QuantityRequestDto, StockResponseDto>(decreaseQuantitySufficientRequestDto))
            .ReturnsAsync((ResponseDto<StockResponseDto>?)null);


        UnitOfWorkMock
           .Setup(u => u.GetGenericRepository<Stock>()
               .GetByIdAsync(decreaseQuantitySufficientRequestDto.StockId))
           .ReturnsAsync(TestStock);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Stock>().UpdateAsync(TestStock))
            .ReturnsAsync(TestStock);

        MapperMock
            .Setup(m => m.Map<StockResponseDto>(TestStock))
            .Returns(stockResponseDto);

        var fakeStockTrans = new StockTrans { Id = Guid.NewGuid() };

        StockTransDomainServiceMock
            .Setup(s => s.CreateStockTransAsync(It.IsAny<StockTransModel>()))
            .ReturnsAsync(fakeStockTrans);


        //Act
        var result = await _stockService.UpdateStockQuantityAsync(decreaseQuantitySufficientRequestDto);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<StockResponseDto>.Success(stockResponseDto, 200));
        TestStock.Quantity.Should().Be(2);
    }
}
