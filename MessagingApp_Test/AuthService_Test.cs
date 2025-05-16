using AuthService.Models;
using Moq;
using AuthService.Interfaces;
using AuthService.Services;

namespace MessagingApp_Test
{
    public class AuthService_Test
    {
        [Fact]
        public async Task LoginShouldReturnTrueIfLoginSuccess()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var fakeUser = new User
            {
                Id = 1,
                Email = "test@example.com",
                Password = "securepass"
            };

            // Mock the repository method to return the fake user
            userRepositoryMock.Setup(repo => repo.GetByEmailAsync(fakeUser.Email))
                              .ReturnsAsync(fakeUser);

            // Mock the repository method to return false for password check
            var userService = new AuthService.Services.UserService(userRepositoryMock.Object);

            // Act
            var user = new User { Email = "test@example.com", Password = "securepass" };
            var result = await userService.AuthenticateUser(user);

            // Assert
            Assert.True(result);
            userRepositoryMock.Verify(repo => repo.GetByEmailAsync(fakeUser.Email), Times.Once);
        }

        [Fact]
        public async Task CreateNewUserShouldReturnTheUserAfterSavingHimInTheDB()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
        
            // Mock the repository method to return false for password check
            var userService = new AuthService.Services.UserService(userRepositoryMock.Object);

            // Act
            var user = new User { Email = "test@example.com", Password = "securepass" };
            var result = await userService.CreateUser(user);

            // Assert
            Assert.True(result);
            userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public void ShouldReturnJWTToken()
        {
            // Arrange
            var jwtService = new JWTService();

            var user = new User()
            {
                Id = 1,
                Email = "test@example.com",
                Password = "securepass"
            };

            // Act
            var token = jwtService.GenerateJWTToken(user);

            // Assert
            Assert.NotNull(token);
            Assert.IsType<string>(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task DeleteUserShouldReturnAfterDeletingHim()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var fakeUser = new User
            {
                Id = 1,
                Email = "test@example.com",
                Password = "securepass"
            };

            // Mock the repository method to return the fake user
            userRepositoryMock.Setup(repo => repo.GetByEmailAsync(fakeUser.Email))
                              .ReturnsAsync(fakeUser);


            // Mock the repository method to return false for password check
            var userService = new AuthService.Services.UserService(userRepositoryMock.Object);

            // Act
            var user = new User {Id= 1, Email = "test@example.com", Password = "securepass" };
            var result = await userService.DeleteUser(user);

            // Assert
            Assert.True(result);
            userRepositoryMock.Verify(repo => repo.DeleteAsync(fakeUser.Id), Times.Once);
        }
    }
}