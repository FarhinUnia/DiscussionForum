
using System.Net;
using System.Text;
using DiscussionForum.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DiscussionForum.Tests
{
    public class IdentityServiceTests
    {
        private sealed class FakeIdentityHandler : HttpMessageHandler
        {
            public List<string> ReceivedCookies { get; } = new();

            public Func<HttpRequestMessage, HttpResponseMessage> ResponseFactory
                { get; set; } = _ => new HttpResponseMessage(HttpStatusCode.Unauthorized);

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var cookie = request.Headers.TryGetValues(
                    "Cookie", out var values)
                    ? string.Join("; ", values)
                    : string.Empty;

                ReceivedCookies.Add(cookie);

                return Task.FromResult(ResponseFactory(request));
            }
        }

        private static (IdentityService Service, ServiceProvider Provider)
            CreateService(FakeIdentityHandler handler)
        {
            var services = new ServiceCollection();

            services.AddLogging();

            services.AddHttpClient("IdentityApi", client =>
            {
                client.BaseAddress =
                    new Uri("http://localhost/identity/");
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler);

            var provider = services.BuildServiceProvider();

            return (
                new IdentityService(
                    provider.GetRequiredService<IHttpClientFactory>()),
                provider
            );
        }

        private static HttpContext CreateContext(string? cookie)
        {
            var context = new DefaultHttpContext();

            if (cookie != null)
            {
                context.Request.Headers.Cookie = cookie;
            }

            return context;
        }

        private static HttpResponseMessage JsonResponse(string json)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    json, Encoding.UTF8, "application/json")
            };
        }

        [Fact]
        public async Task NoCookie_ReturnsNull()
        {
            var handler = new FakeIdentityHandler();
            var (service, provider) = CreateService(handler);

            using (provider)
            {
                var user = await service.GetCurrentUserAsync(
                    CreateContext(null));

                Assert.Null(user);
                Assert.Empty(handler.ReceivedCookies);
            }
        }

        [Fact]
        public async Task UnauthorizedResponse_ReturnsNull()
        {
            var handler = new FakeIdentityHandler();
            var (service, provider) = CreateService(handler);

            using (provider)
            {
                var user = await service.GetCurrentUserAsync(
                    CreateContext("session=invalid"));

                Assert.Null(user);
                Assert.Single(handler.ReceivedCookies);
            }
        }

        [Theory]
        [InlineData("{}")]
        [InlineData("""{"user":{"id":""}}""")]
        [InlineData("""{"user":{"id":"   "}}""")]
        public async Task MissingOrBlankUserId_ReturnsNull(string json)
        {
            var handler = new FakeIdentityHandler
            {
                ResponseFactory = _ => JsonResponse(json)
            };

            var (service, provider) = CreateService(handler);

            using (provider)
            {
                var user = await service.GetCurrentUserAsync(
                    CreateContext("session=valid"));

                Assert.Null(user);
            }
        }

        [Fact]
        public async Task ValidAdministrator_ReturnsCorrectUser()
        {
            var handler = new FakeIdentityHandler
            {
                ResponseFactory = _ => JsonResponse(
                    """
                    {
                        "user": {
                            "id": "admin-123",
                            "name": "Test Admin",
                            "email": "admin@example.test",
                            "role": "admin",
                            "active": true
                        }
                    }
                    """)
            };

            var (service, provider) = CreateService(handler);

            using (provider)
            {
                var user = await service.GetCurrentUserAsync(
                    CreateContext("session=admin"));

                Assert.NotNull(user);
                Assert.Equal("admin-123", user.Id);
                Assert.Equal("admin", user.Role);
                Assert.Equal("session=admin",
                    Assert.Single(handler.ReceivedCookies));
            }
        }

        [Fact]
        public async Task DifferentRequests_ForwardTheirOwnCookies()
        {
            var handler = new FakeIdentityHandler
            {
                ResponseFactory = _ => JsonResponse(
                    """{"user":{"id":"test-user","role":"user"}}""")
            };

            var (service, provider) = CreateService(handler);

            using (provider)
            {
                await service.GetCurrentUserAsync(
                    CreateContext("session=userA"));

                await service.GetCurrentUserAsync(
                    CreateContext("session=userB"));

                Assert.Equal(2, handler.ReceivedCookies.Count);
                Assert.Equal("session=userA", handler.ReceivedCookies[0]);
                Assert.Equal("session=userB", handler.ReceivedCookies[1]);
            }
        }
    }
}
