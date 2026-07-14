using AuthService.Interfaces;
using AuthService.Models;
using AuthService.Services;
using Moq;

namespace MessagingApp_Test
{
    /// <summary>
    /// Tests de la logique du participant du SAGA (AuthService) : validation d'identité,
    /// unicité d'email (en excluant soi-même), et compensation. Cœur métier déterministe,
    /// sans broker RabbitMQ.
    /// </summary>
    public class AccountUpdateSaga_Test
    {
        private static User Existing(int id = 1, string email = "old@example.com", string hash = "HASH")
            => new() { Id = id, Email = email, Password = hash };

        [Fact]
        public async Task PrepareAccountUpdate_WrongPassword_ReturnsInvalidCredentials()
        {
            var repo = new Mock<IUserRepository>();
            var hasher = new Mock<IPasswordHasher>();
            var user = Existing();
            repo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            hasher.Setup(h => h.Verify(user.Password!, "wrong")).Returns(false);

            var service = new AuthService.Services.UserService(repo.Object, hasher.Object);

            var (outcome, oldEmail) = await service.PrepareAccountUpdate(user.Id, "new@example.com", "wrong");

            Assert.Equal(AccountUpdateOutcome.InvalidCredentials, outcome);
            Assert.Null(oldEmail);
            repo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never); // aucune écriture
        }

        [Fact]
        public async Task PrepareAccountUpdate_UnknownUser_ReturnsNotFound()
        {
            var repo = new Mock<IUserRepository>();
            var hasher = new Mock<IPasswordHasher>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null);

            var service = new AuthService.Services.UserService(repo.Object, hasher.Object);

            var (outcome, _) = await service.PrepareAccountUpdate(42, "x@y.z", "pwd");

            Assert.Equal(AccountUpdateOutcome.NotFound, outcome);
        }

        [Fact]
        public async Task PrepareAccountUpdate_EmailTakenByAnother_ReturnsEmailConflict()
        {
            var repo = new Mock<IUserRepository>();
            var hasher = new Mock<IPasswordHasher>();
            var user = Existing(1, "old@example.com");
            var other = Existing(2, "taken@example.com");
            repo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            hasher.Setup(h => h.Verify(user.Password!, "good")).Returns(true);
            repo.Setup(r => r.GetByEmailAsync("taken@example.com")).ReturnsAsync(other);

            var service = new AuthService.Services.UserService(repo.Object, hasher.Object);

            var (outcome, _) = await service.PrepareAccountUpdate(user.Id, "taken@example.com", "good");

            Assert.Equal(AccountUpdateOutcome.EmailConflict, outcome);
            repo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task PrepareAccountUpdate_SameEmailKept_DoesNotConflictWithSelf()
        {
            var repo = new Mock<IUserRepository>();
            var hasher = new Mock<IPasswordHasher>();
            var user = Existing(1, "old@example.com");
            repo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            hasher.Setup(h => h.Verify(user.Password!, "good")).Returns(true);

            var service = new AuthService.Services.UserService(repo.Object, hasher.Object);

            // Même email : ne doit PAS déclencher de conflit (bug de régression corrigé).
            var (outcome, oldEmail) = await service.PrepareAccountUpdate(user.Id, "old@example.com", "good");

            Assert.Equal(AccountUpdateOutcome.Confirmed, outcome);
            Assert.Equal("old@example.com", oldEmail);
            repo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never); // rien à changer
        }

        [Fact]
        public async Task PrepareAccountUpdate_ValidPasswordAndNewEmail_ConfirmsAndUpdatesEmailOnly()
        {
            var repo = new Mock<IUserRepository>();
            var hasher = new Mock<IPasswordHasher>();
            var user = Existing(1, "old@example.com", "HASH");
            repo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            hasher.Setup(h => h.Verify(user.Password!, "good")).Returns(true);
            repo.Setup(r => r.GetByEmailAsync("new@example.com")).ReturnsAsync((User?)null);

            var service = new AuthService.Services.UserService(repo.Object, hasher.Object);

            var (outcome, oldEmail) = await service.PrepareAccountUpdate(user.Id, "new@example.com", "good");

            Assert.Equal(AccountUpdateOutcome.Confirmed, outcome);
            Assert.Equal("old@example.com", oldEmail);
            // Email mis à jour, mot de passe (hash) inchangé.
            repo.Verify(r => r.UpdateAsync(It.Is<User>(u =>
                u.Id == user.Id && u.Email == "new@example.com" && u.Password == "HASH")), Times.Once);
            hasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never); // le mdp n'est jamais re-hashé
        }

        [Fact]
        public async Task RevertAccountEmail_RestoresOldEmail()
        {
            var repo = new Mock<IUserRepository>();
            var hasher = new Mock<IPasswordHasher>();
            var user = Existing(1, "new@example.com", "HASH");
            repo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            var service = new AuthService.Services.UserService(repo.Object, hasher.Object);

            await service.RevertAccountEmail(user.Id, "old@example.com");

            repo.Verify(r => r.UpdateAsync(It.Is<User>(u =>
                u.Id == user.Id && u.Email == "old@example.com" && u.Password == "HASH")), Times.Once);
        }
    }
}
