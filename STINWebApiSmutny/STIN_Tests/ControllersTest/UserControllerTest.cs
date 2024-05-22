using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STINWebApiSmutny.Models;
using FluentAssertions;
using STINWebApiSmutny.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace STIN_Tests.ControllersTest
{
    public class UserControllerTest
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;

        public UserControllerTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "UsersControllerTests")
                .Options;
        }

        [Fact]
        public async Task GetUser_ShouldReturnAllUsers()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                if (!context.Users.Any(u => u.id == 1))
                {
                    context.Users.Add(new User { id = 1, username = "John", email = "john@email.com", password = "password", pass = "true" });
                }

                if (!context.Users.Any(u => u.id == 2))
                {
                    context.Users.Add(new User { id = 2, username = "Jane", email="email@email.cz", password = "password", pass = "true" });
                }

                context.SaveChanges();

                var controller = new UsersController(context);

                // Act
                var result = await controller.GetUsers();

                // Assert
                result.Value.Should().BeAssignableTo<IEnumerable<User>>();

                var users = result.Value as IEnumerable<User>;
                users.Should().HaveCount(4);
                users.Should().ContainEquivalentOf(
                    new User { id = 1, username = "Johnny", email = "john@email.com", password = "newpassword", pass = "true" },
                    options => options.ExcludingMissingMembers()
                );
                users.Should().ContainEquivalentOf(
                    new User { id = 2, username = "Jane", email = "email@email.cz", password = "password", pass = "true" },
                    options => options.ExcludingMissingMembers()
                );
            }
        }

        [Fact]
        public async Task GetUser_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                context.Users.Add(new User { id = 3, username = "John", email = "john@email.com", password = "password", pass = "true" });
                context.SaveChanges();

                var controller = new UsersController(context);

                // Act
                var result = await controller.GetUser(3);

                // Assert
                result.Value.Should().BeEquivalentTo(new User { id = 3, username = "John", email = "john@email.com", password = "password", pass = "true" }, options => options.ExcludingMissingMembers());
            }
        }

        [Fact]
        public async Task GetUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                var controller = new UsersController(context);

                // Act
                var result = await controller.GetUser(1000);

                // Assert
                result.Result.Should().BeOfType<NotFoundResult>();
            }
        }

        [Fact]
        public async Task LoginUser_ShouldReturnUser_WhenCredentialsAreCorrect()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                context.Users.Add(new User { id = 5, username = "John", email = "john@email.com", password = "john_password", pass = "true" });
                context.SaveChanges();

                var controller = new UsersController(context);

                // Act
                var result = await controller.LoginUser(new UserLoginModel { email = "john@email.com", password = "john_password" });

                // Assert
                result.Value.Should().BeEquivalentTo(new User { id = 5, username = "John", email = "john@email.com", password = "john_password", pass = "true" }, options => options.ExcludingMissingMembers());
            }
        }


        [Fact]
        public async Task LoginUser_ShouldReturnNotFound_WhenCredentialsAreIncorrect()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                var controller = new UsersController(context);

                // Act
                var result = await controller.LoginUser(new UserLoginModel { email = "john@email.com", password = "wrongpassword" });

                // Assert
                result.Result.Should().BeOfType<NotFoundResult>();
            }
        }


        [Fact]
        public async Task PutUser_ShouldUpdateUser_WhenUserExists()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a new database instance for each test
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                context.Users.Add(new User { id = 1, username = "John", email = "john@email.com", password = "password", pass = "true" });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(dbContextOptions))
            {
                var controller = new UsersController(context);
                var updatedUser = new User { id = 1, username = "Johnny", email = "john@email.com", password = "newpassword", pass = "true" };

                // Act
                var result = await controller.PutUser(1, updatedUser);

                // Assert
                result.Should().BeOfType<NoContentResult>();

                var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.id == 1);
                user.Should().BeEquivalentTo(updatedUser);
            }
        }

        [Fact]
        public async Task PutUser_ShouldReturnBadRequest_WhenUserIdDoesNotMatch()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                var controller = new UsersController(context);
                var updatedUser = new User { id = 2, username = "Johnny", email = "john@email.com", password = "newpassword", pass = "true" };

                // Act
                var result = await controller.PutUser(1, updatedUser);

                // Assert
                result.Should().BeOfType<BadRequestResult>();
            }
        }

        [Fact]
        public async Task PutUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                var controller = new UsersController(context);
                var updatedUser = new User { id = 1, username = "Johnny", email = "john@email.com", password = "newpassword", pass = "true" };

                // Act
                var result = await controller.PutUser(1, updatedUser);

                // Assert
                result.Should().BeOfType<NoContentResult>();
            }
        }

        [Fact]
        public async Task PayUser_ShouldUpdatePass_WhenUserExists()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                context.Users.Add(new User { id = 8, username = "John", email = "john@email.com", password = "password", pass = "false" });
                context.SaveChanges();

                var controller = new UsersController(context);

                // Act
                var result = await controller.PayUser(8);

                // Assert
                result.Should().BeOfType<NoContentResult>();

                var user = await context.Users.FindAsync(8);
                user.pass.Should().Be("true");
            }

        }

        [Fact]
        public async Task PayUser_ShouldReturnBadRequest_WhenUserDoesNotExist()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                var controller = new UsersController(context);

                // Act
                var result = await controller.PayUser(1);

                // Assert
                result.Should().BeOfType<NoContentResult>();
            }
        }

        [Fact]
        public async Task PostUser_ShouldCreateUser()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                var controller = new UsersController(context);
                var newUser = new User { username = "John", email = "john@email.com", password = "password", pass = "true" };

                // Act
                var result = await controller.PostUser(newUser);

                // Assert
                var createdAtActionResult = result.Result as CreatedAtActionResult;
                createdAtActionResult.Should().NotBeNull();
                createdAtActionResult.Value.Should().BeEquivalentTo(newUser, options => options.ExcludingMissingMembers());

                var user = await context.Users.FindAsync(((User)createdAtActionResult.Value).id);
                user.Should().BeEquivalentTo(newUser, options => options.ExcludingMissingMembers());
            }
        }

        [Fact]
        public async Task RegisterUser_ShouldCreateUser_WhenEmailIsUnique()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                var controller = new UsersController(context);
                var newUser = new UserRegisterModel { username = "John", email = "john2@email.com", password = "password" };

                // Act
                var result = await controller.RegisterUser(newUser);

                // Assert
                var createdResult = result.Result as CreatedResult;
                createdResult.Should().NotBeNull();

                var user = await context.Users.FirstOrDefaultAsync(u => u.email == newUser.email);
                user.Should().NotBeNull();
                user.username.Should().Be(newUser.username);
                user.email.Should().Be(newUser.email);
                user.password.Should().Be(newUser.password);
                user.pass.Should().Be("none");
            }
        }

        [Fact]
        public async Task RegisterUser_ShouldReturnBadRequest_WhenEmailIsNotUnique()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                context.Users.Add(new User { id = 10, username = "Jane", email = "jane@email.com", password = "password", pass = "true" });
                context.SaveChanges();

                var controller = new UsersController(context);
                var newUser = new UserRegisterModel { username = "John", email = "jane@email.com", password = "password" };

                // Act
                var result = await controller.RegisterUser(newUser);

                // Assert
                result.Result.Should().BeOfType<BadRequestObjectResult>();
            }
        }

        [Fact]
        public async Task DeleteUser_ShouldRemoveUser_WhenUserExists()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                context.Users.Add(new User { id = 10, username = "John", email = "john@email.com", password = "password", pass = "true" });
                context.SaveChanges();

                var controller = new UsersController(context);

                // Act
                var result = await controller.DeleteUser(10);

                // Assert
                result.Should().BeOfType<NoContentResult>();

                var user = await context.Users.FindAsync(10);
                user.Should().BeNull();
            }
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(dbContextOptions))
            {
                var controller = new UsersController(context);

                // Act
                var result = await controller.DeleteUser(100);

                // Assert
                result.Should().BeOfType<NotFoundResult>();
            }
        }
    }
}
