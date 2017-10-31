using System;
using System.Threading.Tasks;
using AspNetCoreTodo.Data;
using AspNetCoreTodo.Models;
using AspNetCoreTodo.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AspNetCoreTodo.UnitTests
{
    public class TodoItemServiceTests
    {
        private DbContextOptions<ApplicationDbContext> _dbOptions;

        public TodoItemServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AddNewItem")
                .Options;
        }

        [Fact]
        public async Task AddNewItem_ShouldAddNewItem()
        {
            // Set up a context (connection to the DB)
            using (var inMemoryContext = new ApplicationDbContext(_dbOptions))
            {
                // Arrange
                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@fake"
                };

                var service = new TodoItemService(inMemoryContext);

                // Act
                await service.AddItemAsync(new NewTodoItem { Title = "Testing?" }, fakeUser);

                // Assert
                Assert.Equal(1, await inMemoryContext.Items.CountAsync());

                var item = await inMemoryContext.Items.FirstAsync();

                Assert.Equal("Testing?", item.Title);
                Assert.Equal(false, item.IsDone);
                Assert.Equal(fakeUser.Id, item.OwnerId);
                Assert.True(DateTimeOffset.Now.AddDays(3) - item.DueAt < TimeSpan.FromSeconds(1));
            }
        }

        [Fact]
        public async Task MarkDone_ShouldUpdate()
        {
            using (var inMemoryContext = new ApplicationDbContext(_dbOptions))
            {
                // Arrange
                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@fake"
                };

                var fakeItem = new TodoItem
                {
                    Id = Guid.NewGuid(),
                    OwnerId = fakeUser.Id,
                    IsDone = false,
                    Title = "Finish Testing"
                };

                inMemoryContext.Items.Add(fakeItem);

                var service = new TodoItemService(inMemoryContext);

                // Act
                var result = await service.MarkDoneAsync(fakeItem.Id, fakeUser);

                // Assert
                Assert.Equal(1, await inMemoryContext.Items.CountAsync());

                var item = await inMemoryContext.Items.FirstAsync();

                Assert.True(result);
                Assert.True(item.IsDone);
            }
        }

        [Fact]
        public async Task MarkDone_GivenInvalidGuid_ShouldNotUpdate()
        {
            using (var inMemoryContext = new ApplicationDbContext(_dbOptions))
            {
                // Arrange
                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@fake"
                };

                var fakeItem = new TodoItem
                {
                    Id = Guid.NewGuid(),
                    OwnerId = fakeUser.Id,
                    IsDone = false,
                    Title = "Finish Testing"
                };

                inMemoryContext.Items.Add(fakeItem);

                var service = new TodoItemService(inMemoryContext);

                // Act
                var result = await service.MarkDoneAsync(Guid.NewGuid(), fakeUser);

                // Assert
                Assert.Equal(1, await inMemoryContext.Items.CountAsync());

                var item = await inMemoryContext.Items.FirstAsync();

                Assert.False(result);
                Assert.False(item.IsDone);
            }
        }
    }
}