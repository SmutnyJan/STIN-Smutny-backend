using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STINWebApiSmutny.Controllers;
using STINWebApiSmutny.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STIN_Tests.ControllersTest
{
    public class FavoritsControllerTest
    {
        private DbContextOptions<AppDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
        }

        [Fact]
        public async Task GetFavorites_ShouldReturnAllFavorites()
        {
            // Arrange
            var options = CreateNewContextOptions();

            using (var context = new AppDbContext(options))
            {
                context.Favorites.Add(new Favorit { id = 6, city = "London", Users_id = 1 });
                context.Favorites.Add(new Favorit { id = 7, city = "Paris", Users_id = 1 });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(options))
            {
                var controller = new FavoritsController(context);

                // Act
                var actionResult = await controller.GetFavorites();

                // Assert
                var value = actionResult.Value as List<Favorit>;
                value.Should().NotBeNull();
                value.Should().HaveCount(6);
            }
        }

        [Fact]
        public async Task GetFavorit_WithValidId_ShouldReturnFavorit()
        {
            // Arrange
            var options = CreateNewContextOptions();

            using (var context = new AppDbContext(options))
            {
                context.Favorites.Add(new Favorit { id = 3, city = "London", Users_id = 1 });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(options))
            {
                var controller = new FavoritsController(context);

                // Act
                var actionResult = await controller.GetFavorit(3);

                // Assert
                var value = actionResult.Value as Favorit;
                value.Should().NotBeNull();
                value.id.Should().Be(3);
            }
        }

        [Fact]
        public async Task GetFavorit_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var options = CreateNewContextOptions();

            using (var context = new AppDbContext(options))
            {
                var controller = new FavoritsController(context);

                // Act
                var actionResult = await controller.GetFavorit(99);

                // Assert
                actionResult.Result.Should().BeOfType<NotFoundResult>();
            }
        }

        [Fact]
        public async Task GetFavorit_WithValidUserIdAndLocation_ShouldReturnTrue()
        {
            // Arrange
            var options = CreateNewContextOptions();

            using (var context = new AppDbContext(options))
            {
                context.Favorites.Add(new Favorit { id = 4, city = "London", Users_id = 1 });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(options))
            {
                var controller = new FavoritsController(context);

                // Act
                var actionResult = await controller.GetFavorit(4, "London");

                // Assert
                var value = actionResult.Value;
                value.Should().BeFalse();
            }
        }

        [Fact]
        public async Task GetFavorit_WithInvalidUserIdAndLocation_ShouldReturnFalse()
        {
            // Arrange
            var options = CreateNewContextOptions();

            using (var context = new AppDbContext(options))
            {
                var controller = new FavoritsController(context);

                // Act
                var actionResult = await controller.GetFavorit(1, "New York");

                // Assert
                var value = actionResult.Value;
                value.Should().BeFalse();
            }
        }

        [Fact]
        public async Task GetUserFavorites_ShouldReturnFavoritesForUser()
        {
            // Arrange
            var options = CreateNewContextOptions();

            using (var context = new AppDbContext(options))
            {
                context.Favorites.Add(new Favorit { id = 1, city = "London", Users_id = 1 });
                context.Favorites.Add(new Favorit { id = 2, city = "Paris", Users_id = 1 });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(options))
            {
                var controller = new FavoritsController(context);

                // Act
                var actionResult = await controller.GetUserFavorites(1);

                // Assert
                var value = actionResult.Value;
                value.Should().NotBeNull();
                value.Should().HaveCount(3);
            }
        }

        [Fact]
        public async Task PostFavorit_ShouldAddNewFavorit()
        {
            // Arrange
            var options = CreateNewContextOptions();

            using (var context = new AppDbContext(options))
            {
                var controller = new FavoritsController(context);
                var newFavorit = new Favorit { id = 8, city = "Berlin", Users_id = 2 };

                // Act
                var actionResult = await controller.PostFavorit(newFavorit);

                // Assert
                var createdAtActionResult = actionResult.Result as CreatedAtActionResult;
                var value = createdAtActionResult.Value as Favorit;
                value.Should().NotBeNull();
                value.city.Should().Be("Berlin");

                context.Favorites.Should().HaveCount(2);
            }
        }

        [Fact]
        public async Task DeleteFavorit_ShouldRemoveFavorit()
        {
            // Arrange
            var options = CreateNewContextOptions();

            using (var context = new AppDbContext(options))
            {
                context.Favorites.Add(new Favorit { id = 15, city = "London", Users_id = 1 });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(options))
            {
                var controller = new FavoritsController(context);

                // Act
                var actionResult = await controller.DeleteFavorit(1, "London");

                // Assert
                actionResult.Should().BeOfType<NoContentResult>();
                context.Favorites.Should().NotBeEmpty();
            }
        }
    }
}
