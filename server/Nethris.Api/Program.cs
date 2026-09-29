using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Nethris.Api;
using Nethris.Api.Data;
using Nethris.Api.Domain;
using Nethris.Api.Security;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(builder.Configuration["Hris:Urls"] ?? "http://127.0.0.1:5187");
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 32768);
var database = new HrisDatabase(builder.Configuration["Hris:Database"] ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data", "nethris-learning.sqlite"));
var tokens = new JwtTokenService(database);
builder.Services.AddSingleton(database); builder.Services.AddSingleton(tokens);
builder.Services.AddSingleton<HrisAuth>(); builder.Services.AddSingleton<HrisApi>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{ options.TokenValidationParameters = tokens.ValidationParameters; options.MapInboundClaims = false; });
var app = builder.Build();
app.Use(async (http, next) =>
{
    http.Response.Headers["X-Content-Type-Options"] = "nosniff";
    http.Response.Headers["Referrer-Policy"] = "no-referrer";
    http.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; connect-src 'self'; frame-ancestors 'none'; base-uri 'none'; form-action 'self'";
    if (http.Request.Path.StartsWithSegments("/api")) http.Response.Headers.CacheControl = "no-store";
    try
    {
        var remote = http.Connection.RemoteIpAddress;
        if ((remote is not null && !IPAddress.IsLoopback(remote)) || (http.Request.Host.Host is not ("localhost" or "127.0.0.1" or "[::1]")))
            throw HrisError.Forbidden("This workshop accepts loopback requests only.");
        var origin = http.Request.Headers.Origin.ToString();
        if (origin.Length > 0 && (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) || uri.Scheme != "http" ||
            uri.Host != http.Request.Host.Host || (uri.Port != http.Request.Host.Port && uri.Port != 5173)))
            throw HrisError.Forbidden("Cross-origin request rejected.");
        await next();
    }
    catch (Exception error) when (error is HrisError or JsonException or BadHttpRequestException or DbUpdateException or SqliteException or System.Text.DecoderFallbackException)
    {
        var (status, code, message) = error switch
        {
            HrisError known => (known.Status, known.Code, known.Message),
            DbUpdateConcurrencyException => (409, "stale", "This record changed. Reload and review your draft."),
            SqliteException sqlite when sqlite.SqliteErrorCode is 5 or 6 => (503, "busy", "Database is busy. Retry the exact operation."),
            DbUpdateException => (409, "constraint", "A uniqueness, relationship or value constraint rejected this change."),
            SqliteException sqlite when sqlite.SqliteErrorCode == 19 => (409, "constraint", "A database constraint rejected this change."),
            BadHttpRequestException bad => (bad.StatusCode, "request", "Request is invalid or too large."),
            _ => (400, "json", "Invalid JSON, unknown fields, or invalid value types.")
        };
        if (!http.Response.HasStarted) { http.Response.StatusCode = status; await http.Response.WriteAsJsonAsync(new { code, message }); }
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "Unhandled HRIS request error");
        if (!http.Response.HasStarted) { http.Response.StatusCode = 500; await http.Response.WriteAsJsonAsync(new { code = "internal", message = "The request failed. Review server logs and retry the exact operation after resolving the cause." }); }
    }
});
app.UseAuthentication();
app.UseDefaultFiles(); app.UseStaticFiles();
app.MapMethods("/api/{**path}", ["GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS"], (HttpContext http, HrisApi api) => api.Handle(http));
app.MapFallbackToFile("index.html");
app.Run();
public partial class Program { }
