using MessageService.Events;
using MessageService.Interfaces;
using MessageService.Models;
using MessageService.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MessagingApp_Test
{
    /// <summary>
    /// Tests unitaires du cœur de la messagerie (MessageService) : persistance des
    /// messages/conversations et logique de groupe (join code). Utilise une base
    /// EF Core InMemory isolée par test et un IProducer mocké (pas de RabbitMQ réel).
    /// </summary>
    public class MessageService_Test
    {
        private static MessageDbContext NewInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<MessageDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new MessageDbContext(options);
        }

        // ───────────────────────── MsgService ─────────────────────────

        [Fact]
        public async Task CreateMessageAsync_PersistsMessage_AndNotifiesOtherMembers()
        {
            // Arrange
            using var context = NewInMemoryContext();
            var producerMock = new Mock<IProducer>();
            var service = new MsgService(context, producerMock.Object);

            // Conversation 42 : expéditeur 1 + deux autres membres (2 et 3).
            context.UserConversations.AddRange(
                new UserConversation { ConversationId = 42, UserId = 1 },
                new UserConversation { ConversationId = 42, UserId = 2 },
                new UserConversation { ConversationId = 42, UserId = 3 });
            await context.SaveChangesAsync();

            var message = new Message
            {
                Content = "Bonjour",
                Status = "sent",
                SenderId = 1,
                ReceiverId = 0, // placeholder de groupe
                ConversationId = 42
            };

            // Act
            var created = await service.CreateMessageAsync(message);

            // Assert
            Assert.True(created.Id > 0);
            Assert.Equal(1, await context.Messages.CountAsync());
            // Une notification par membre SAUF l'expéditeur (2 et 3), jamais pour UserId 0.
            producerMock.Verify(p => p.Send(
                It.Is<CreatedMessageEvent>(e => e.ReceiverId == 2), "new-message-event"), Times.Once);
            producerMock.Verify(p => p.Send(
                It.Is<CreatedMessageEvent>(e => e.ReceiverId == 3), "new-message-event"), Times.Once);
            producerMock.Verify(p => p.Send(
                It.Is<CreatedMessageEvent>(e => e.ReceiverId == 1 || e.ReceiverId == 0), "new-message-event"), Times.Never);
        }

        [Fact]
        public async Task GetMessageByIdAsync_ReturnsPersistedMessage()
        {
            using var context = NewInMemoryContext();
            var service = new MsgService(context, Mock.Of<IProducer>());
            var created = await service.CreateMessageAsync(new Message
            {
                Content = "hi", Status = "sent", SenderId = 1, ReceiverId = 2, ConversationId = 1
            });

            var found = await service.GetMessageByIdAsync(created.Id);

            Assert.NotNull(found);
            Assert.Equal("hi", found.Content);
        }

        [Fact]
        public async Task UpdateMessageAsync_UpdatesContent_WhenMessageExists()
        {
            using var context = NewInMemoryContext();
            var service = new MsgService(context, Mock.Of<IProducer>());
            var created = await service.CreateMessageAsync(new Message
            {
                Content = "ancien", Status = "sent", SenderId = 1, ReceiverId = 2, ConversationId = 1
            });

            var updated = await service.UpdateMessageAsync(created.Id, new Message
            {
                Content = "nouveau", Status = "read", ConversationId = 1
            });

            Assert.NotNull(updated);
            Assert.Equal("nouveau", updated.Content);
            Assert.Equal("read", updated.Status);
        }

        [Fact]
        public async Task UpdateMessageAsync_ReturnsNull_WhenMessageMissing()
        {
            using var context = NewInMemoryContext();
            var service = new MsgService(context, Mock.Of<IProducer>());

            var result = await service.UpdateMessageAsync(999, new Message { Content = "x", Status = "sent" });

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteMessageAsync_RemovesMessage_AndReturnsFalseWhenMissing()
        {
            using var context = NewInMemoryContext();
            var service = new MsgService(context, Mock.Of<IProducer>());
            var created = await service.CreateMessageAsync(new Message
            {
                Content = "x", Status = "sent", SenderId = 1, ReceiverId = 2, ConversationId = 1
            });

            Assert.True(await service.DeleteMessageAsync(created.Id));
            Assert.Equal(0, await context.Messages.CountAsync());
            Assert.False(await service.DeleteMessageAsync(created.Id)); // déjà supprimé
        }

        // ─────────────────────── ConversationService ───────────────────────

        [Fact]
        public async Task CreateConversationAsync_AddsOwnerToUserConversation()
        {
            using var context = NewInMemoryContext();
            var service = new ConversationService(context, Mock.Of<IProducer>());

            var conversation = new Conversation
            {
                Title = "Équipe projet",
                OwnerId = 5,
                JoinCode = "ABCD-1234",
                Date = DateTime.Now
            };

            var created = await service.CreateConversationAsync(conversation);

            Assert.True(created.Id > 0);
            var membership = await context.UserConversations.SingleAsync();
            Assert.Equal(5, membership.UserId);
            Assert.Equal(created.Id, membership.ConversationId);
        }

        [Fact]
        public async Task AddUserToConversationAsync_AddsUser_WhenJoinCodeValid()
        {
            using var context = NewInMemoryContext();
            var service = new ConversationService(context, Mock.Of<IProducer>());
            await service.CreateConversationAsync(new Conversation
            {
                Title = "Groupe", OwnerId = 1, JoinCode = "JOIN-CODE", Date = DateTime.Now
            });

            var result = await service.AddUserToConversationAsync("JOIN-CODE", 2);

            Assert.NotNull(result);
            // Propriétaire (1) + nouvel utilisateur (2)
            Assert.Equal(2, await context.UserConversations.CountAsync());
            Assert.True(await context.UserConversations.AnyAsync(uc => uc.UserId == 2));
        }

        [Fact]
        public async Task AddUserToConversationAsync_ReturnsNull_WhenJoinCodeInvalid()
        {
            using var context = NewInMemoryContext();
            var service = new ConversationService(context, Mock.Of<IProducer>());

            var result = await service.AddUserToConversationAsync("CODE-INEXISTANT", 2);

            Assert.Null(result);
        }

        [Fact]
        public async Task AddUserToConversationAsync_IsIdempotent_WhenUserAlreadyMember()
        {
            using var context = NewInMemoryContext();
            var service = new ConversationService(context, Mock.Of<IProducer>());
            await service.CreateConversationAsync(new Conversation
            {
                Title = "Groupe", OwnerId = 1, JoinCode = "DUP-CODE", Date = DateTime.Now
            });

            await service.AddUserToConversationAsync("DUP-CODE", 2);
            await service.AddUserToConversationAsync("DUP-CODE", 2); // deuxième fois

            // Propriétaire + utilisateur 2 (pas de doublon)
            Assert.Equal(2, await context.UserConversations.CountAsync());
        }

        [Fact]
        public async Task GetAllConversationsByUserId_ReturnsOnlyUserConversations()
        {
            using var context = NewInMemoryContext();
            var service = new ConversationService(context, Mock.Of<IProducer>());
            await service.CreateConversationAsync(new Conversation
            {
                Title = "A", OwnerId = 1, JoinCode = "AAAA", Date = DateTime.Now
            });
            await service.CreateConversationAsync(new Conversation
            {
                Title = "B", OwnerId = 2, JoinCode = "BBBB", Date = DateTime.Now
            });

            var forUser1 = await service.GetAllConversationsByUserId(1);

            Assert.Single(forUser1);
            Assert.Equal("A", forUser1.First().Title);
        }

        [Fact]
        public async Task DeleteConversationAsync_ReturnsFalse_WhenMissing()
        {
            using var context = NewInMemoryContext();
            var service = new ConversationService(context, Mock.Of<IProducer>());

            Assert.False(await service.DeleteConversationAsync(12345));
        }

        [Fact]
        public async Task RemoveUserFromConversationAsync_RemovesMembership_ButKeepsConversation()
        {
            using var context = NewInMemoryContext();
            var service = new ConversationService(context, Mock.Of<IProducer>());
            var conv = await service.CreateConversationAsync(new Conversation
            {
                Title = "Groupe", OwnerId = 1, JoinCode = "LEAVE-CODE", Date = DateTime.Now
            });
            await service.AddUserToConversationAsync("LEAVE-CODE", 2);
            Assert.Equal(2, await context.UserConversations.CountAsync());

            var left = await service.RemoveUserFromConversationAsync(conv.Id, 2);

            Assert.True(left);
            Assert.False(await context.UserConversations.AnyAsync(uc => uc.UserId == 2));
            // La conversation existe toujours (seul le lien d'appartenance est supprimé)
            Assert.NotNull(await context.Conversations.FindAsync(conv.Id));
            // Retirer à nouveau → false (plus membre)
            Assert.False(await service.RemoveUserFromConversationAsync(conv.Id, 2));
        }

        // ─────────────────── JoinCodeConversationService ───────────────────

        [Fact]
        public void Generate_ProducesCodeOfRequestedLength_WithoutTrailingHyphen()
        {
            var generator = new JoinCodeConversationService();

            var code = generator.Generate(40);

            Assert.NotNull(code);
            Assert.True(code.Length <= 40);
            Assert.DoesNotContain(" ", code);
            Assert.NotEqual('-', code[^1]); // pas de tiret final
        }

        [Fact]
        public void Generate_ProducesDistinctCodes()
        {
            var generator = new JoinCodeConversationService();

            var a = generator.Generate(40);
            var b = generator.Generate(40);

            Assert.NotEqual(a, b);
        }
    }
}
