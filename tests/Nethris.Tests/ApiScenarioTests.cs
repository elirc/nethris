using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Nethris.Tests;

/// <summary>
/// HTTP scenarios ported from scripts/check_api.py into xUnit, so `dotnet test` proves the
/// contract that previously only a Python script could exercise.
///
/// <see cref="WebApplicationFactory{TEntryPoint}"/> boots the real Program.cs in-process: the real
/// middleware, the real JWT handler, the real EF/SQLite transaction per request. There is no port,
/// no child process and no readiness polling, and a server-side exception surfaces in the test's
/// own stack trace. Each test gets a private SQLite file through the `Hris:Database` configuration
/// key, so no test can see another test's data and none of them can open a real workshop database.
/// </summary>
public sealed class ApiScenarioTests : IAsyncLifetime
{
    // Test-only credential. It must satisfy HrisRules.Password; it is never a real secret.
    private const string Password = "Workshop-Tests-Only-2026!";

    private readonly string directory = Path.Combine(Path.GetTempPath(), "nethris-api-" + Guid.NewGuid().ToString("N"));
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;
    private string adminToken = "";

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(directory);
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseSetting("Hris:Database", Path.Combine(directory, "api-test.sqlite")));
        client = factory.CreateClient();

        var setup = await Post("setup", new
        {
            companyName = "Nethris test workshop",
            firstName = "Avery",
            lastName = "Admin",
            email = "admin@example.test",
            password = Password,
            seedDemo = true
        }, token: null, key: Guid.NewGuid());

        Assert.Equal(HttpStatusCode.Created, setup.StatusCode);
        adminToken = (await Read(setup)).GetProperty("token").GetString()!;
    }

    public Task DisposeAsync()
    {
        client.Dispose();
        factory.Dispose();
        try
        {
            Directory.Delete(directory, recursive: true);
        }
        catch (IOException)
        {
            // A held SQLite file must not turn a green run red.
        }
        return Task.CompletedTask;
    }

    private static async Task<JsonElement> Read(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    private HttpRequestMessage Request(HttpMethod method, string path, object? body, string? token, Guid? key, string? ifMatch)
    {
        var request = new HttpRequestMessage(method, "/api/" + path);
        if (body is not null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }

        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (key is not null) request.Headers.Add("Idempotency-Key", key.Value.ToString("D"));
        if (ifMatch is not null) request.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        return request;
    }

    private Task<HttpResponseMessage> Post(string path, object? body, string? token, Guid? key = null) =>
        client.SendAsync(Request(HttpMethod.Post, path, body, token, key, null));

    private Task<HttpResponseMessage> Put(string path, object? body, string? token, Guid? key, string? ifMatch) =>
        client.SendAsync(Request(HttpMethod.Put, path, body, token, key, ifMatch));

    private Task<HttpResponseMessage> Get(string path, string? token) =>
        client.SendAsync(Request(HttpMethod.Get, path, null, token, null, null));

    private async Task<string> LoginAsync(string email)
    {
        using var response = await Post("auth/login", new { email, password = Password }, token: null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await Read(response)).GetProperty("token").GetString()!;
    }

    private static string Tag(JsonElement row) =>
        $"\"{row.GetProperty("id").GetString()!.Replace("-", "")}-{row.GetProperty("version").GetInt64()}\"";

    private static object Department(string name) => new { name, costCenter = "ENG-001", leadEmployeeId = (Guid?)null, isActive = true };

    // ---------------------------------------------------------------- setup and authentication

    [Fact]
    public async Task RepeatedSetup_IsRefusedWith409()
    {
        using var again = await Post("setup", new
        {
            companyName = "Replacement",
            firstName = "No",
            lastName = "No",
            email = "no@example.test",
            password = Password
        }, token: null);

        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
    }

    [Fact]
    public async Task AnUnauthenticatedRead_Is401()
    {
        using var response = await Get("employees", token: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AWrongPassword_Is401()
    {
        using var response = await Post("auth/login", new { email = "employee@example.test", password = "Incorrect-password-2026" }, token: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------------------------------------------------------------- idempotency receipts

    [Fact]
    public async Task AMutationWithoutAValidIdempotencyKey_IsRejected()
    {
        // The API demands a UUID Idempotency-Key on every mutation, so that a retry after a lost
        // response can be recognised instead of creating a second record.
        using var response = await client.SendAsync(
            Request(HttpMethod.Post, "departments", Department("Bad"), adminToken, null, null));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode); // 422
    }

    [Fact]
    public async Task ARetryWithTheSameKeyAndBody_ReplaysTheStoredReceipt()
    {
        var key = Guid.NewGuid();
        var body = Department("Workshop lab");

        using var created = await Post("departments", body, adminToken, key);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var first = await Read(created);

        // The client never saw the response and retries the *exact* same request.
        using var replay = await Post("departments", body, adminToken, key);

        Assert.Equal(HttpStatusCode.Created, replay.StatusCode);
        Assert.Equal("true", replay.Headers.GetValues("Idempotency-Replayed").Single());
        Assert.Equal(first.GetProperty("id").GetString(), (await Read(replay)).GetProperty("id").GetString());

        // And exactly one department was created, not two.
        var list = await Read(await Get("departments?limit=100", adminToken));
        Assert.Single(list.GetProperty("items").EnumerateArray(),
            item => item.GetProperty("name").GetString() == "Workshop lab");
    }

    [Fact]
    public async Task ReusingAKeyForADifferentBody_Is409()
    {
        var key = Guid.NewGuid();
        using var created = await Post("departments", Department("Key reuse original"), adminToken, key);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        // Same key, different payload: a client bug, not a retry. Replaying the old receipt here
        // would silently discard the second request.
        using var response = await Post("departments", Department("Key reuse different"), adminToken, key);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // ---------------------------------------------------------------- optimistic concurrency

    [Fact]
    public async Task AnUpdateWithoutIfMatch_Is428()
    {
        using var created = await Post("departments", Department("Precondition required"), adminToken, Guid.NewGuid());
        var row = await Read(created);

        // 428 Precondition Required: the client stated no version at all. Answering 409 or 412 here
        // would tell the client to reload when the real fix is "send If-Match".
        using var response = await Put("departments/" + row.GetProperty("id").GetString(),
            Department("Renamed"), adminToken, Guid.NewGuid(), ifMatch: null);

        Assert.Equal((HttpStatusCode)428, response.StatusCode);
    }

    [Fact]
    public async Task AnUpdateWithAStaleIfMatch_Is409()
    {
        using var created = await Post("departments", Department("Stale etag"), adminToken, Guid.NewGuid());
        var row = await Read(created);
        var id = row.GetProperty("id").GetString()!;
        var firstTag = Tag(row);

        using var accepted = await Put("departments/" + id, Department("Stale etag v2"), adminToken, Guid.NewGuid(), firstTag);
        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        Assert.NotEqual(firstTag, Tag(await Read(accepted)));

        // A second editor still holding the first ETag must be refused, not silently win.
        using var conflict = await Put("departments/" + id, Department("Stale etag v3"), adminToken, Guid.NewGuid(), firstTag);

        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
    }

    [Fact]
    public async Task AReadReturnsTheRecordsStrongETag()
    {
        using var created = await Post("departments", Department("Etag read"), adminToken, Guid.NewGuid());
        var row = await Read(created);

        using var response = await Get("departments/" + row.GetProperty("id").GetString(), adminToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Tag(row), response.Headers.ETag?.ToString());
    }

    // ---------------------------------------------------------------- role-based authorization

    [Fact]
    public async Task AnEmployeeCannotWriteDepartments()
    {
        var employeeToken = await LoginAsync("employee@example.test");

        using var response = await Post("departments", Department("Employee attempt"), employeeToken, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AnEmployeeCannotReadTheAuditLog()
    {
        var employeeToken = await LoginAsync("employee@example.test");

        using var response = await Get("audit-logs?limit=10", employeeToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AnEmployeeCannotAdministerAccounts()
    {
        var employeeToken = await LoginAsync("employee@example.test");

        using var response = await Get("accounts?limit=10", employeeToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------------------------------------------------------------- sessions

    [Fact]
    public async Task ASignedOutTokenIsRejectedImmediately()
    {
        var employeeToken = await LoginAsync("employee@example.test");
        Assert.Equal(HttpStatusCode.OK, (await Get("employees?limit=5", employeeToken)).StatusCode);

        // An empty JSON object, not a null body: the server reads a body for every non-GET
        // request, and a request with no Content-Type is answered 415 before it reaches routing.
        using var logout = await Post("auth/logout", new { }, employeeToken);
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);

        // Sessions are revocable: the JWT is still cryptographically valid, and still refused,
        // because the server checks the session row as well as the signature.
        using var afterLogout = await Get("employees?limit=5", employeeToken);
        Assert.Equal(HttpStatusCode.Unauthorized, afterLogout.StatusCode);
    }

    // ---------------------------------------------------------------- pagination bounds

    [Fact]
    public async Task ListsAreBounded()
    {
        using var response = await Get("employees?limit=100000", adminToken);

        // An unbounded limit is a denial-of-service vector, so the server rejects it rather than
        // quietly returning the whole table.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // ---------------------------------------------------------------- transactional writes

    [Fact]
    public async Task ARejectedWrite_LeavesNoAuditRow()
    {
        var before = (await Read(await Get("audit-logs?limit=100", adminToken))).GetProperty("items").GetArrayLength();

        // Duplicate name: rejected by a uniqueness rule after the handler has already started its
        // work. Because the whole request runs in one transaction, the audit row must not survive.
        using var created = await Post("departments", Department("Rollback probe"), adminToken, Guid.NewGuid());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var duplicate = await Post("departments", Department("rollback probe"), adminToken, Guid.NewGuid());
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var after = (await Read(await Get("audit-logs?limit=100", adminToken))).GetProperty("items").GetArrayLength();
        Assert.Equal(before + 1, after); // one for the accepted create, none for the rejected one
    }
}
