using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Domain;
using Domain.Exeptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestApi.ErrorHandling;
using RestApi.Setup;

namespace RestApi.Test.ErrorHandling;

/// <summary>
/// The wiring, rather than the handler: an endpoint with no try/catch of its own,
/// behind the same registration Program.cs uses, in the environment where ASP.NET
/// Core would otherwise answer with its developer exception page.
/// </summary>
public class ExceptionHandlingPipelineTests : IAsyncLifetime
{
    private WebApplication app = null!;
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            // The environment that renders the stack, the source paths and the request
            // headers when nothing catches first. If the handler holds here, it holds
            // everywhere.
            EnvironmentName = Environments.Development,
        });

        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.AddErrorHandling();

        app = builder.Build();
        app.UseExceptionHandler();

        // Not one try/catch between the throw and the response.
        app.MapGet("/already-exists", void () =>
            throw new ResourceAlreadyExistsException("User", "username hussen.iq"));
        app.MapGet("/not-found", void () =>
            throw new ResourceNotFoundException("Product", "Product with ID 3f2a not found"));
        app.MapGet("/bad-request", void () =>
            throw new InvalidReviewRatingException("A rating runs from 1 to 5."));
        app.MapGet("/unauthorized", void () =>
            throw new UnauthorizedAccessException("Invalid username or password."));
        app.MapGet("/boom", void () =>
            throw new InvalidOperationException("Npgsql connection to store-dev failed: Password=admin"));

        await app.StartAsync();
        client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiJ9.payload.signature");
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private async Task<(HttpStatusCode Status, string Body)> Get(string path)
    {
        var response = await client.GetAsync(path);
        return (response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/already-exists", HttpStatusCode.Conflict, "User with username hussen.iq already exists")]
    [InlineData("/not-found", HttpStatusCode.NotFound, "Product with ID 3f2a not found")]
    [InlineData("/bad-request", HttpStatusCode.BadRequest, "A rating runs from 1 to 5.")]
    [InlineData("/unauthorized", HttpStatusCode.Unauthorized, "Invalid username or password.")]
    public async Task Answers_a_broken_rule_with_its_status_and_its_sentence(
        string path,
        HttpStatusCode expected,
        string message)
    {
        var response = await client.GetAsync(path);

        Assert.Equal(expected, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(message, body.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Answers_an_unexpected_failure_with_500_and_nothing_from_inside_the_server()
    {
        var (status, body) = await Get("/boom");

        Assert.Equal(HttpStatusCode.InternalServerError, status);

        var json = JsonDocument.Parse(body).RootElement;
        Assert.Equal("An unexpected error occurred.", json.GetProperty("message").GetString());

        // The developer exception page would have carried every one of these.
        Assert.DoesNotContain("InvalidOperationException", body);
        Assert.DoesNotContain("Npgsql", body);
        Assert.DoesNotContain("Password=admin", body);
        Assert.DoesNotContain("store-dev", body);
        Assert.DoesNotContain(".cs:line", body);
        Assert.DoesNotContain("at RestApi", body);
        Assert.DoesNotContain("HEADERS", body);
        Assert.DoesNotContain("Bearer", body);
    }

    [Fact]
    public async Task Never_answers_with_the_developer_exception_page()
    {
        foreach (var path in new[] { "/already-exists", "/not-found", "/bad-request", "/unauthorized", "/boom" })
        {
            var response = await client.GetAsync(path);
            var body = await response.Content.ReadAsStringAsync();

            // That page is HTML, and it is the only thing here that would be.
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
            Assert.DoesNotContain("<!DOCTYPE", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("<html", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Stack Query Cookies Headers", body);
        }
    }
}
