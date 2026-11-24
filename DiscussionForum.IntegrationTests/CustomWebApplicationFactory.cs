
using System.Net;
using System.Net.Http;
using System.Text;
using DiscussionForum.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DiscussionForum.IntegrationTests
{
    public class CustomWebApplicationFactory
        : WebApplicationFactory<global::Program>
    {
        private readonly string _databaseId = Guid.NewGuid().ToString();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ApplicationDbContext>();
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();


                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(
                        $"ForumIntegrationTests_{_databaseId}"));


                services.AddHttpClient("IdentityApi")
                    .ConfigurePrimaryHttpMessageHandler(
                        () => new FakeIdentityHandler());
            });
        }

        private sealed class FakeIdentityHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var cookie = request.Headers.TryGetValues(
                    "Cookie", out var values)
                    ? string.Join("; ", values)
                    : string.Empty;
                

                if (!cookie.Split(';', StringSplitOptions.TrimEntries)
                    .Any(c => string.Equals(
                         c,
                         "test-session=admin",
                         StringComparison.Ordinal)))

                {
                    return Task.FromResult(
                        new HttpResponseMessage(HttpStatusCode.Unauthorized));
                }

                const string json = """
                {
                    "user": {
                        "id": "test-admin-1",
                        "name": "Test Administrator",
                        "email": "admin@example.test",
                        "role": "admin",
                        "active": true
                    }
                }
                """;

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        json, Encoding.UTF8, "application/json")
                });
            }
        }
    }
}
