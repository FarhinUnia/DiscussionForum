
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace DiscussionForum.Services
{
    public class IdentityService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IdentityService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IdentityUser?> GetCurrentUserAsync(
            HttpContext context)
        {
            // Read the authentication cookies sent by the browser.
            var cookie = context.Request.Headers["Cookie"].ToString();

            if (string.IsNullOrWhiteSpace(cookie))
            {
                return null;
            }

            // Connect to the Identity API.
            var client = _httpClientFactory.CreateClient("IdentityApi");

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/me");

            // Forward the browser's cookies to Better Auth.
            request.Headers.TryAddWithoutValidation("Cookie", cookie);

            using var response = await client.SendAsync(request);

            // No valid authenticated session.
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            // Read the JSON response.
            var result = await response.Content.ReadFromJsonAsync<IdentityMeResponse>(
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var user = result?.User;

            // A stable user ID is required for author and ownership checks.
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
            {
                return null;
            }

            return user;
        }
    }

    public class IdentityMeResponse
    {
        public IdentityUser? User { get; set; }
    }

    public class IdentityUser
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool Active { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
