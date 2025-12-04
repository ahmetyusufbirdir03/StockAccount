using FluentAssertions;
using Moq;
using StockAccountApplication.Services;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.User;
using StockAccountDomain.Entities;
using System.Linq.Expressions;

namespace StockAccountApplication.Test.ServiceTests;

public class UserServiceTests : ServiceTestBase
{
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userService = new UserService(
            HttpContextAccessorMock.Object,
            UnitOfWorkMock.Object,
            MapperMock.Object,
            UserManagerMock.Object
        );
    }


    // GET ALL USERS TESTS
    [Fact]
    public async Task GetAllUsers_ShouldReturnUnauthorized_WhenaIsNotAuthenticatedAsAdmin()
    {
        // Arrange
        var ecpectedError = ResponseDto<IList<UserResponseDto>>.Fail(ErrorMessageService.Unauthorized401, 401);

        SetupAuthenticatedUser(role: "user");

        // Act
        var result = await _userService.GetAllUsers();
        // Assert

        result.Should().BeEquivalentTo(ecpectedError);
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnAllUsers_WhenAuthenticatedAsAdmin()
    {
        // Arrange
        var testUserList = new List<User>
        {
            TestUser,
            TestDataFactory.CreateTestUser(
                id: Guid.Parse("33333333-3333-3333-3333-333333333333"),
                name: "User2",
                email: "u2@test.com"
            )
        };

        var userDtoList = new List<UserResponseDto>
        {
            new UserResponseDto { FirstName = "User1", Email = "u1@test.com" },
            new UserResponseDto { FirstName = "User2", Email = "u2@test.com" }
        };

        SetupAuthenticatedUser(role: "admin");

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
                         .GetAllAsync(x => x.DeletedAt == null, null))
            .ReturnsAsync(testUserList);


        MapperMock
            .Setup(m => m.Map<IList<UserResponseDto>>(It.IsAny<IList<User>>()))
            .Returns(userDtoList);

        var expectedResponse = ResponseDto<IList<UserResponseDto>>.Success(userDtoList, 200);

        // Act
        var result = await _userService.GetAllUsers();

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    }


    // SOFT DELETE USER TESTS
    [Fact]
    public async Task SoftDeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
                         .GetByIdAsync(userId))
            .ReturnsAsync((User?)null);
        var expectedResponse = ResponseDto<NoContentDto>
            .Fail(ErrorMessageService.UserNotFound404, 404);
        // Act
        var result = await _userService.SoftDeleteUserAsync(userId);
        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task SoftDeleteUser_ShouldSoftDeleteUser_WhenUserExists()
    {
        // Arrange
        var userId = TestUser.Id;
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
                         .GetByIdAsync(userId))
            .ReturnsAsync(TestUser);

        UserManagerMock
            .Setup(u => u.UpdateAsync(TestUser));

        var expectedResponse = ResponseDto<NoContentDto>
            .Success(204);

        // Act
        var result = await _userService.SoftDeleteUserAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        UserManagerMock.Verify(u => u.UpdateAsync(It.Is<User>(usr => usr.DeletedAt != null)), Times.Once);
        UserManagerMock.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Once);

    }


    //DELETE USER TESTS
    [Fact]
    public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
                         .GetAllAsync(It.IsAny<Expression<Func<User, bool>>>(), null))
            .ReturnsAsync(new List<User>());

        var expectedResponse = ResponseDto<NoContentDto>
            .Fail(ErrorMessageService.UserNotFound404, 404);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task DeleteUser_ShouldDeleteUser_WhenUserExists()
    {
        // Arrange
        var userId = TestUser.Id;
        var userList = new List<User> { TestUser };

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
                         .GetAllAsync(It.IsAny<Expression<Func<User, bool>>>(), null))
            .ReturnsAsync(userList);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
                         .DeleteAsync(TestUser))
            .Returns(Task.CompletedTask);

        var expectedResponse = ResponseDto<NoContentDto>
            .Success(204);

        // Act
        var result = await _userService.DeleteUserAsync(userId);
        
        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        UnitOfWorkMock.Verify(u => u.GetGenericRepository<User>()
            .DeleteAsync(It.Is<User>(usr => usr.Id == userId)), Times.Once);
        UnitOfWorkMock.Verify(m => m.GetGenericRepository<User>()
            .DeleteAsync(It.IsAny<User>()), Times.Once);
    }

    // GET USER BY EMAIL TEST
}
