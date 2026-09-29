using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Nethris.Api.Contracts;
using Nethris.Api.Data;
using Nethris.Api.Domain;
using Nethris.Api.Security;

namespace Nethris.Api;

/// <summary>
/// One HTTP boundary owns authentication, transactions, receipts and serialization.
/// This file routes a request; HrisApi.Reads.cs serves GETs, HrisApi.Mutations.cs runs writes under an
/// idempotency receipt, and HrisRequest.cs holds the HTTP helpers. The behavior is the delivered dispatcher's;
/// only the layout changed (fix pass), which the Python black-box suite and the xUnit suite both confirm.
/// </summary>
public sealed partial class HrisApi(HrisDatabase database, HrisAuth auth)
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        PropertyNameCaseInsensitive = false,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
        MaxDepth = 24
    };

    public static string ETag(Guid id, long version) => $"\"{id:N}-{version}\"";

    public static T Parse<T>(string body) => HrisRequest.Parse<T>(body);

    /// <summary>The route as the dispatcher sees it: the segments after <c>/api</c>, the joined path and the method.</summary>
    private readonly record struct Route(string[] Parts, string Path, string Method)
    {
        public string Kind => Parts.Length > 0 ? Parts[0] : "";
        public bool IsWrite => Method != "GET";
    }

    public async Task Handle(HttpContext http)
    {
        var route = ReadRoute(http);
        if (route.Method == "GET" && route.Path == "health")
        {
            await HrisRequest.Send(http, new { status = "ok", service = "Nethris learning HRIS" });
            return;
        }

        var body = route.IsWrite ? await HrisRequest.Body(http.Request) : "";

        // One request, one connection, one transaction: deferred for reads, immediate for writes.
        await using var db = database.Create();
        await db.Database.OpenConnectionAsync();
        var connection = (SqliteConnection)db.Database.GetDbConnection();
        await using var transaction = connection.BeginTransaction(deferred: !route.IsWrite);
        await db.Database.UseTransactionAsync(transaction);

        if (await HandleSetupAndLogin(http, db, transaction, route, body))
        {
            return;
        }

        var actor = await auth.Require(db, http.User, MinimumRole(route));
        if (route.Method == "GET")
        {
            await Read(http, db, actor, route.Parts);
            return;
        }

        if (route.Method == "POST" && route.Path == "auth/logout")
        {
            await auth.Logout(db, http.User);
            await transaction.CommitAsync();
            await HrisRequest.Send(http, new { signedOut = true });
            return;
        }

        if (route.Method is not ("POST" or "PUT" or "DELETE"))
        {
            throw HrisError.Missing();
        }

        await Mutate(http, db, transaction, actor, route.Parts, route.Method, body);
    }

    private static Route ReadRoute(HttpContext http)
    {
        var parts = http.Request.Path.Value!.Trim('/').Split('/').Skip(1).ToArray();
        return new Route(parts, string.Join('/', parts), http.Request.Method);
    }

    /// <summary>The unauthenticated endpoints. Returns true when the request was fully handled.</summary>
    private async Task<bool> HandleSetupAndLogin(HttpContext http, HrisDbContext db, SqliteTransaction transaction, Route route, string body)
    {
        if (route.Method == "GET" && route.Path == "setup")
        {
            var initialized = await db.UserAccounts.AnyAsync();
            var settings = await db.WorkshopSettings.SingleAsync();
            await HrisRequest.Send(http, new { initialized, companyName = settings.Name });
            return true;
        }

        if (route.Method == "POST" && route.Path is "setup" or "auth/login")
        {
            object response;
            int status;
            if (route.Path == "setup")
            {
                response = await auth.Setup(db, Parse<SetupInput>(body));
                status = 201;
            }
            else
            {
                var input = Parse<LoginRequest>(body);
                response = await auth.Login(db, input.Email, input.Password);
                status = 200;
            }

            await transaction.CommitAsync();
            await HrisRequest.Send(http, response, status);
            return true;
        }

        return false;
    }

    /// <summary>The role gate, decided from the first segment before any record is read.</summary>
    private static SecurityRole MinimumRole(Route route)
    {
        if (route.Kind is "accounts")
        {
            return SecurityRole.Admin;
        }

        if (route.Kind is "audit-logs")
        {
            return SecurityRole.Hr;
        }

        if (route.IsWrite && route.Kind is "employees" or "departments" or "job-roles")
        {
            return SecurityRole.Hr;
        }

        return SecurityRole.Employee;
    }
}
