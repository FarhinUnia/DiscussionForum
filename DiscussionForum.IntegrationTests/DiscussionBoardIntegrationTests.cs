
using DiscussionForum.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DiscussionForum.IntegrationTests
{
    public class DiscussionBoardIntegrationTests
    {
        [Fact]
        public async Task CreateBoard_ValidBoard_SavesBoardToDatabase()
        {
            // Arrange
            using var factory = new CustomWebApplicationFactory();
            using var client = factory.CreateClient(
    new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

            var boardData = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("CourseId", "1"),
                    new KeyValuePair<string, string>("Title", "Integration Test Board"),
                    new KeyValuePair<string, string>("Description", "Board created during integration testing")
                });

            // Act
            var response = await client.PostAsync(
                "/DiscussionBoard/Create", boardData);

            // Assert
            var errorDetails = await response.Content.ReadAsStringAsync();

            Assert.True(
                response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect,
                $"Status: {response.StatusCode}\nDetails: {errorDetails}");

            using var scope = factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var board = await db.DiscussionBoards
                .FirstOrDefaultAsync(b => b.Title == "Integration Test Board");

            Assert.NotNull(board);
            Assert.Equal(1, board.CourseId);
            Assert.Equal("Board created during integration testing", board.Description);
        }
    }
}
