using System.Text;
using System.Text.Json;
using Nethris.Api.Domain;

namespace Nethris.Api;

/// <summary>HTTP plumbing shared by the dispatcher: body reading with limits, query numbers, If-Match, JSON responses.</summary>
internal static class HrisRequest
{
    private const int MaximumBodyBytes = 32768;

    public static T Parse<T>(string body)
    {
        var value = JsonSerializer.Deserialize<T>(body, HrisApi.Json);
        return value ?? throw HrisError.Invalid("A JSON object is required.");
    }

    public static int Number(HttpContext http, string name, int fallback, int min, int max)
    {
        if (!http.Request.Query.TryGetValue(name, out var raw))
        {
            return fallback;
        }

        if (int.TryParse(raw, out var number) && number >= min && number <= max)
        {
            return number;
        }

        throw HrisError.Invalid($"{name} must be {min}–{max}.");
    }

    /// <summary>The version the caller says it edited, from a strong ETag in If-Match; 428 when absent or malformed.</summary>
    public static long Expected(HttpContext http, Guid id)
    {
        var tag = http.Request.Headers.IfMatch.ToString();
        var prefix = $"\"{id:N}-";
        var wellFormed = tag.StartsWith(prefix, StringComparison.Ordinal) && tag.EndsWith('"');
        if (wellFormed && long.TryParse(tag[prefix.Length..^1], out var version) && version > 0)
        {
            return version;
        }

        throw new HrisError(428, "precondition", "Send the record's strong ETag in If-Match.");
    }

    public static async Task<string> Body(HttpRequest request)
    {
        if (request.ContentLength > MaximumBodyBytes)
        {
            throw new HrisError(413, "body", "Request body exceeds 32 KiB.");
        }

        if (request.Method == "DELETE")
        {
            return "";
        }

        var mediaType = request.ContentType?.Split(';')[0].Trim();
        if (mediaType != "application/json")
        {
            throw new HrisError(415, "contentType", "Use application/json.");
        }

        using var stream = new MemoryStream();
        var buffer = new byte[4096];
        int count;
        while ((count = await request.Body.ReadAsync(buffer)) > 0)
        {
            if (stream.Length + count > MaximumBodyBytes)
            {
                throw new HrisError(413, "body", "Request body exceeds 32 KiB.");
            }

            await stream.WriteAsync(buffer.AsMemory(0, count));
        }

        // Strict UTF-8: an invalid byte sequence is a 400 (DecoderFallbackException) rather than silent replacement.
        return new UTF8Encoding(false, true).GetString(stream.ToArray());
    }

    public static Task Send(HttpContext http, object value, int status = 200, string? tag = null)
    {
        var json = JsonSerializer.Serialize(value, HrisApi.Json);
        return SendRaw(http, json, status, tag);
    }

    public static async Task SendRaw(HttpContext http, string json, int status, string? tag)
    {
        http.Response.StatusCode = status;
        http.Response.ContentType = "application/json; charset=utf-8";
        if (tag is not null)
        {
            http.Response.Headers.ETag = tag;
        }

        await http.Response.WriteAsync(json);
    }
}
