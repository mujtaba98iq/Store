using System.Text;
using System.Text.Json;
using Domain;
using Domain.Exeptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using RestApi.ErrorHandling;

namespace RestApi.Test.ErrorHandling;

/// <summary>
/// The handler is exercised on its own rather than through the running app: these
/// are statements about what a caller is allowed to see, and they should hold
/// without a database, a token or a network standing behind them.
/// </summary>
public class GlobalExceptionHandlerTests
{
    private static readonly GlobalExceptionHandler Handler =
        new(NullLogger<GlobalExceptionHandler>.Instance);

    /// <summary>A request carrying the things that must never come back out of one.</summary>
    private static DefaultHttpContext Request()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/users";
        context.Request.QueryString = new QueryString("?access_token=super-secret-token");
        context.Request.Headers.Authorization = "Bearer eyJhbGciOiJIUzI1NiJ9.payload.signature";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<(int Status, string Body)> Handle(Exception exception)
    {
        var context = Request();

        var handled = await Handler.TryHandleAsync(context, exception, CancellationToken.None);
        Assert.True(handled);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        return (context.Response.StatusCode, body);
    }

    private static async Task<(int Status, JsonElement Json)> HandleJson(Exception exception)
    {
        var (status, body) = await Handle(exception);
        return (status, JsonDocument.Parse(body).RootElement.Clone());
    }

    private static string Message(JsonElement json) => json.GetProperty("message").GetString()!;

    [Fact]
    public async Task Answers_a_name_already_taken_with_409_and_the_rule_own_sentence()
    {
        var (status, json) = await HandleJson(
            new ResourceAlreadyExistsException("User", "username hussen.iq"));

        Assert.Equal(StatusCodes.Status409Conflict, status);
        Assert.Equal("User with username hussen.iq already exists", Message(json));
    }

    [Fact]
    public async Task Answers_something_missing_with_404_and_the_rule_own_sentence()
    {
        var (status, json) = await HandleJson(
            new ResourceNotFoundException("Product", "Product with ID 3f2a not found"));

        Assert.Equal(StatusCodes.Status404NotFound, status);
        Assert.Equal("Product with ID 3f2a not found", Message(json));
    }

    [Theory]
    [InlineData(typeof(CouponCodeAlreadyExistsException))]
    [InlineData(typeof(InsufficientStockException))]
    [InlineData(typeof(OrderAlreadyPaidException))]
    [InlineData(typeof(InvalidOrderStatusTransitionException))]
    [InlineData(typeof(ShipmentAlreadyExistsException))]
    public async Task Answers_a_clash_with_the_current_state_of_the_shop_with_409(Type exceptionType)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Only 2 left in stock")!;

        var (status, json) = await HandleJson(exception);

        Assert.Equal(StatusCodes.Status409Conflict, status);
        Assert.Equal("Only 2 left in stock", Message(json));
    }

    [Theory]
    [InlineData(typeof(InvalidReviewRatingException))]
    [InlineData(typeof(EmptyCartCheckoutException))]
    [InlineData(typeof(InvalidCartQuantityException))]
    [InlineData(typeof(InvalidOrderAmountException))]
    public async Task Answers_a_request_that_could_not_be_right_with_400(Type exceptionType)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "A rating runs from 1 to 5.")!;

        var (status, json) = await HandleJson(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, status);
        Assert.Equal("A rating runs from 1 to 5.", Message(json));
    }

    [Fact]
    public async Task Answers_a_validation_failure_with_400_and_the_field_it_belongs_to()
    {
        var exception = new ValidationException(
        [
            new ValidationFailure("Username", "Username must be at least 3 characters."),
            new ValidationFailure("Password", "Password is required."),
        ]);

        var (status, json) = await HandleJson(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, status);
        Assert.Contains("Username must be at least 3 characters.", Message(json));

        var errors = json.GetProperty("errors");
        Assert.Equal(
            "Username must be at least 3 characters.",
            errors.GetProperty("Username")[0].GetString());
        Assert.Equal("Password is required.", errors.GetProperty("Password")[0].GetString());
    }

    [Fact]
    public async Task Answers_every_failure_to_authenticate_in_the_same_words()
    {
        var unknownUser = await HandleJson(new UnauthorizedAccessException("Invalid username or password."));
        var wrongPassword = await HandleJson(new UnauthorizedAccessException("Invalid username or password."));
        var deadToken = await HandleJson(new UnauthorizedAccessException("Refresh token has expired."));

        Assert.Equal(StatusCodes.Status401Unauthorized, unknownUser.Status);
        Assert.Equal(StatusCodes.Status401Unauthorized, wrongPassword.Status);
        Assert.Equal(StatusCodes.Status401Unauthorized, deadToken.Status);

        // Identical down to the word, so which usernames exist cannot be read off the
        // reply, and a spent token cannot be told apart from a forged one.
        Assert.Equal(Message(unknownUser.Json), Message(wrongPassword.Json));
        Assert.Equal(Message(unknownUser.Json), Message(deadToken.Json));
    }

    [Fact]
    public async Task Never_names_the_image_store_or_repeats_what_it_said()
    {
        var exception = new ImageStorageException(
            "Failed to upload the image to Cloudinary: Invalid API key 176992785384359");

        var (status, json) = await HandleJson(exception);

        Assert.Equal(StatusCodes.Status502BadGateway, status);
        Assert.DoesNotContain("Cloudinary", Message(json));
        Assert.DoesNotContain("176992785384359", Message(json));
    }

    [Fact]
    public async Task Answers_an_unexpected_failure_with_500_and_a_sentence_of_its_own()
    {
        var (status, json) = await HandleJson(
            new InvalidOperationException("The connection pool is exhausted."));

        Assert.Equal(StatusCodes.Status500InternalServerError, status);
        Assert.Equal("An unexpected error occurred.", Message(json));
    }

    [Fact]
    public async Task Never_puts_a_stack_trace_in_the_response()
    {
        Exception thrown;
        try
        {
            // Thrown rather than constructed, so it carries a real stack to leak.
            throw new InvalidOperationException("Object reference not set.");
        }
        catch (Exception exception)
        {
            thrown = exception;
        }

        var (_, body) = await Handle(thrown);

        Assert.NotNull(thrown.StackTrace);
        Assert.DoesNotContain("at RestApi.Test", body);
        Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".cs:line", body);
    }

    [Fact]
    public async Task Never_names_the_exception_type_behind_an_unexpected_failure()
    {
        var (_, body) = await Handle(new InvalidOperationException("The connection pool is exhausted."));

        Assert.DoesNotContain("InvalidOperationException", body);
        Assert.DoesNotContain("System.", body);
        // Its own words describe the server rather than the request, so they go too.
        Assert.DoesNotContain("connection pool", body);
    }

    [Fact]
    public async Task Never_returns_a_database_detail_or_a_connection_string()
    {
        var exception = new InvalidOperationException(
            "Npgsql failure for Host=localhost;Port=32300;Database=store-dev;Username=postgres;Password=admin");

        var (_, body) = await Handle(exception);

        Assert.DoesNotContain("Password=admin", body);
        Assert.DoesNotContain("Npgsql", body);
        Assert.DoesNotContain("store-dev", body);
    }

    [Fact]
    public async Task Never_returns_the_authorization_header_or_the_token_on_the_request()
    {
        // Every branch, not only the unexpected one: the header and the query string sit
        // on the context throughout, and no branch may copy anything off it.
        Exception[] everyKind =
        [
            new ResourceAlreadyExistsException("User", "username hussen.iq"),
            new ResourceNotFoundException("User", "User with ID 3f2a not found"),
            new UnauthorizedAccessException("Invalid username or password."),
            new ImageStorageException("Failed to upload the image to Cloudinary."),
            new ValidationException([new ValidationFailure("Username", "Username is required.")]),
            new InvalidOperationException("The connection pool is exhausted."),
        ];

        foreach (var exception in everyKind)
        {
            var (_, body) = await Handle(exception);

            Assert.DoesNotContain("Bearer", body);
            Assert.DoesNotContain("eyJhbGciOiJIUzI1NiJ9", body);
            Assert.DoesNotContain("Authorization", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("super-secret-token", body);
            Assert.DoesNotContain("access_token", body);
        }
    }

    [Fact]
    public async Task Ties_a_withheld_failure_to_its_log_line_without_saying_so_in_the_body()
    {
        var unexpected = Request();
        await Handler.TryHandleAsync(unexpected, new InvalidOperationException("boom"), CancellationToken.None);

        var refused = Request();
        await Handler.TryHandleAsync(
            refused,
            new ResourceAlreadyExistsException("User", "username hussen.iq"),
            CancellationToken.None);

        // Only the answer that withheld the detail needs a way back to it.
        Assert.True(unexpected.Response.Headers.ContainsKey(GlobalExceptionHandler.TraceHeader));
        Assert.False(refused.Response.Headers.ContainsKey(GlobalExceptionHandler.TraceHeader));
    }

    /// <summary>
    /// The web client reads the body for the sentence to show, and treats any other
    /// loose string in it as part of that sentence. So the body carries the message
    /// and, for a validation failure, the fields - and never a third thing.
    /// </summary>
    [Theory]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(UnauthorizedAccessException))]
    [InlineData(typeof(ImageStorageException))]
    public async Task Answers_with_a_body_of_nothing_but_the_message(Type exceptionType)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "something went wrong")!;

        var (_, json) = await HandleJson(exception);

        Assert.Equal(JsonValueKind.Object, json.ValueKind);
        Assert.Equal(["message"], json.EnumerateObject().Select(property => property.Name));
    }

    [Fact]
    public async Task Answers_a_validation_failure_with_the_message_and_the_fields_only()
    {
        var exception = new ValidationException([new ValidationFailure("Username", "Username is required.")]);

        var (_, json) = await HandleJson(exception);

        Assert.Equal(["message", "errors"], json.EnumerateObject().Select(property => property.Name));
    }

    [Fact]
    public async Task Answers_with_the_json_the_web_client_already_reads()
    {
        var context = Request();

        await Handler.TryHandleAsync(
            context,
            new ResourceAlreadyExistsException("User", "username hussen.iq"),
            CancellationToken.None);

        Assert.StartsWith("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task Says_nothing_to_a_caller_that_has_already_hung_up()
    {
        var context = Request();
        context.RequestAborted = new CancellationToken(canceled: true);

        var handled = await Handler.TryHandleAsync(
            context,
            new OperationCanceledException(),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(0, context.Response.Body.Length);
    }

    [Fact]
    public async Task Steps_aside_once_the_response_is_already_on_its_way()
    {
        var features = new FeatureCollection();
        features.Set<IHttpRequestFeature>(new HttpRequestFeature { Method = "POST", Path = "/api/users" });
        features.Set<IHttpResponseFeature>(new StartedResponseFeature());
        features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(new MemoryStream()));
        var context = new DefaultHttpContext(features);

        // Nothing useful can be substituted now, so the failure is left to abort the
        // response rather than be dressed up as a complete one.
        var handled = await Handler.TryHandleAsync(
            context,
            new InvalidOperationException("boom"),
            CancellationToken.None);

        Assert.False(handled);
    }

    /// <summary>A response whose status line has already gone out.</summary>
    private sealed class StartedResponseFeature : IHttpResponseFeature
    {
        public Stream Body { get; set; } = new MemoryStream();
        public bool HasStarted => true;
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public string? ReasonPhrase { get; set; }
        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        public void OnCompleted(Func<object, Task> callback, object state) { }

        public void OnStarting(Func<object, Task> callback, object state) { }
    }
}
