
using DiscussionForum.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.RegularExpressions;
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
                    AllowAutoRedirect = false,
                    HandleCookies = false
                });

            // Simulate an administrator session on the GET request.
            using var getRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "/DiscussionBoard/Create?courseId=1");

            getRequest.Headers.TryAddWithoutValidation(
                "Cookie", "test-session=admin");

            // Retrieve the Create form and antiforgery token.
            using var getResponse = await client.SendAsync(getRequest);

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var html = await getResponse.Content.ReadAsStringAsync();


            var match = Regex.Match(
                html,
                "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");

            if (!match.Success)
            {
                match = Regex.Match(
                    html,
                    "value=\"([^\"]+)\"[^>]*name=\"__RequestVerificationToken\"");
            }


            Assert.True(match.Success, "Antiforgery token not found.");

            var token = match.Groups[1].Value;

            using var boardData = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>(
                        "CourseId", "1"),
                    new KeyValuePair<string, string>(
                        "Title", "Integration Test Board"),
                    new KeyValuePair<string, string>(
                        "Description",
                        "Board created during integration testing"),
                    new KeyValuePair<string, string>(
                        "__RequestVerificationToken", token)
                });

            // Act
            using var postRequest = new HttpRequestMessage(
               HttpMethod.Post,
               "/DiscussionBoard/Create");

            postRequest.Content = boardData;

            // Preserve the administrator session on the POST request.
            // Combine the administrator cookie with the antiforgery cookie
            // using the correct Cookie header separator.
            var antiforgeryCookies = getResponse.Headers.TryGetValues(
                "Set-Cookie", out var setCookieValues)
                ? setCookieValues
                    .Select(value => value.Split(';')[0])
                    .ToArray()
                : Array.Empty<string>();

            Assert.NotEmpty(antiforgeryCookies);

            postRequest.Headers.Remove("Cookie");

            postRequest.Headers.TryAddWithoutValidation(
                "Cookie",
                string.Join("; ",
                    new[] { "test-session=admin" }
                        .Concat(antiforgeryCookies)));

            var response = await client.SendAsync(postRequest);

            // Assert
            var errorDetails =
                await response.Content.ReadAsStringAsync();

            Assert.True(
                response.IsSuccessStatusCode ||
                response.StatusCode == HttpStatusCode.Redirect,
                $"Status: {response.StatusCode}\nDetails: {errorDetails}");

            using var scope = factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var board = await db.DiscussionBoards
                .FirstOrDefaultAsync(
                    b => b.Title == "Integration Test Board");

            Assert.NotNull(board);
            Assert.Equal(1, board.CourseId);
            Assert.Equal(
                "Board created during integration testing",
                board.Description);
        }
    }
}
