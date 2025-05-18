using AuthService.Models;
using Moq;
using AuthService.Interfaces;
using AuthService.Services;
using Isopoh.Cryptography.Argon2;
using Microsoft.CodeAnalysis.Elfie.Model.Strings;

namespace MessagingApp_Test
{
    public class AuthService_Test
    {
        [Fact]
        public async Task LoginShouldReturnTrueIfLoginSuccess()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var argonHasherMock = new Mock<IPasswordHasher>();

            var fakeUser = new User
            {
                Id = 1,
                Email = "test@example.com",
                Password = "hashedPassword"
            };

            var inputUser = new User
            {
                Email = "test@example.com",
                Password = "plainPassword"
            };

            userRepositoryMock.Setup(repo => repo.GetByEmailAsync(fakeUser.Email))
                              .ReturnsAsync(fakeUser);

            argonHasherMock.Setup(h => h.Verify(fakeUser.Password, inputUser.Password))
                           .Returns(true);

            var userService = new AuthService.Services.UserService(userRepositoryMock.Object, argonHasherMock.Object);

            // Act
            var result = await userService.AuthenticateUser(inputUser);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fakeUser.Id, result.Id);
            Assert.Equal(fakeUser.Email, result.Email);

            userRepositoryMock.Verify(repo => repo.GetByEmailAsync(fakeUser.Email), Times.Once);
            argonHasherMock.Verify(h => h.Verify(fakeUser.Password, inputUser.Password), Times.Once);
        }

        [Fact]
        public void  ShouldReturnFalseIfPasswordIsIncorrect()
        {
            // Arrange
            var passwordTest = "securePassword";
            var passwordVerifyTest = "wrongPassword";

            // Act
            var hashed = Argon2.Hash(passwordTest);
            var result = Argon2.Verify(hashed, passwordVerifyTest);

            Assert.False(result);
            Assert.NotEmpty(hashed);
        }

        [Fact]
        public async Task CreateNewUserShouldReturnTheUserAfterSavingHimInTheDB()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var argonMock = new Mock<IPasswordHasher>();

            // Mock the repository method to return false for password check
            var userService = new AuthService.Services.UserService(userRepositoryMock.Object, argonMock.Object);

            // Act
            var user = new User { Email = "test@example.com", Password = "securepass" };
            var result = await userService.CreateUser(user);

            // Assert
            Assert.NotNull(result);
            userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
            argonMock.Verify(argon => argon.Hash(It.IsAny<string>()), Times.Once);
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
            var argonMock = new Mock<IPasswordHasher>();

            // Mock the repository method to return the fake user
            userRepositoryMock.Setup(repo => repo.GetByEmailAsync(fakeUser.Email))
                              .ReturnsAsync(fakeUser);


            // Mock the repository method to return false for password check
            var userService = new AuthService.Services.UserService(userRepositoryMock.Object, argonMock.Object);

            // Act
            var user = new User {Id= 1, Email = "test@example.com", Password = "securepass" };
            var result = await userService.DeleteUser(user);

            // Assert
            Assert.True(result);
            userRepositoryMock.Verify(repo => repo.DeleteAsync(fakeUser.Id), Times.Once);
        }
    }
}