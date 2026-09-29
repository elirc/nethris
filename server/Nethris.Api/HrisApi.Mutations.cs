using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Nethris.Api.Contracts;
using Nethris.Api.Data;
using Nethris.Api.Domain;
using Nethris.Api.Security;

namespace Nethris.Api;

public sealed partial class HrisApi
{
    /// <summary>
    /// Every write runs under an Idempotency-Key receipt: a repeated key with the same fingerprint replays the stored
    /// response, a repeated key with a different fingerprint is a 409, and a fresh key executes the mutation, records
    /// the audit row and the receipt, and commits everything together.
    /// </summary>
    private async Task Mutate(HttpContext http, HrisDbContext db, SqliteTransaction transaction, Actor actor, string[] parts, string method, string body)
    {
        var key = http.Request.Headers["Idempotency-Key"].ToString();
        if (!Guid.TryParseExact(key, "D", out _))
        {
            throw HrisError.Invalid("Send a UUID Idempotency-Key for each new mutation; reuse it only for an exact retry.");
        }

        var fingerprint = Fingerprint(http, method, body);
        var now = DateTimeOffset.UtcNow;
        await db.Receipts.Where(r => r.ExpiresAt <= now).ExecuteDeleteAsync();
        var prior = await db.Receipts.SingleOrDefaultAsync(r => r.UserAccountId == actor.UserId && r.Key == key);
        if (prior is not null)
        {
            await Replay(http, db, transaction, actor, parts, prior, fingerprint);
            return;
        }

        var target = MutationTarget.From(http, parts, method);
        var value = await Execute(db, actor, target, body);
        var status = target.Method == "POST" && target.Id is null ? 201 : 200;
        var json = JsonSerializer.Serialize(value, Json);
        var (tag, auditId) = Describe(json, target.Id);

        var action = target.Method == "POST" && target.Action.Length > 0 ? target.Action : target.Method;
        db.AuditLogs.Add(HrisAuth.Audit(actor, action, target.Kind, auditId));
        db.Receipts.Add(new OperationReceipt
        {
            Id = Guid.NewGuid(),
            UserAccountId = actor.UserId,
            Key = key,
            Fingerprint = fingerprint,
            ResponseJson = json,
            StatusCode = status,
            ETag = tag,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        });

        // The record change, its audit row and its receipt commit together or not at all.
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        await HrisRequest.SendRaw(http, json, status, tag);
    }

    /// <summary>Method, path, query, If-Match and body: the parts of a request that make a retry "the same request".</summary>
    private static string Fingerprint(HttpContext http, string method, string body)
    {
        var material = method + "\n" + http.Request.Path + "\n" + http.Request.QueryString + "\n" + http.Request.Headers.IfMatch + "\n" + body;
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(material)));
    }

    private static async Task Replay(HttpContext http, HrisDbContext db, SqliteTransaction transaction, Actor actor, string[] parts, OperationReceipt prior, string fingerprint)
    {
        if (prior.Fingerprint != fingerprint)
        {
            throw HrisError.Conflict("This operation key was already used for a different request.");
        }

        // A stored PTO decision is replayed only if the caller could still make it today.
        if (parts.Length == 3 && parts[0] == "pto" && parts[2] == "decision")
        {
            await RequireDecisionStillAllowed(db, actor, parts[1], prior);
        }

        http.Response.Headers["Idempotency-Replayed"] = "true";
        await transaction.CommitAsync();
        await HrisRequest.SendRaw(http, prior.ResponseJson, prior.StatusCode, prior.ETag);
    }

    private static async Task RequireDecisionStillAllowed(HrisDbContext db, Actor actor, string requestSegment, OperationReceipt prior)
    {
        if (actor.Role < SecurityRole.Manager || !Guid.TryParse(requestSegment, out var requestId))
        {
            throw HrisError.Forbidden();
        }

        if (!await HrisPto.Visible(db, actor).AnyAsync(p => p.Id == requestId))
        {
            throw HrisError.Forbidden();
        }

        using var receipt = JsonDocument.Parse(prior.ResponseJson);
        var approvals = receipt.RootElement.GetProperty("approvals").EnumerateArray();
        var decidedHrStageAsSelf = approvals.Any(a =>
            a.GetProperty("stage").GetString() == "Hr" && a.GetProperty("approverEmployeeId").GetGuid() == actor.EmployeeId);
        if (actor.Role < SecurityRole.Hr && decidedHrStageAsSelf)
        {
            throw HrisError.Forbidden();
        }
    }

    /// <summary>What a mutation addresses: the resource kind, an optional id (with its expected version) and an optional action.</summary>
    private readonly record struct MutationTarget(string Kind, Guid? Id, long Version, string Action, string Method, int Length)
    {
        public static MutationTarget From(HttpContext http, string[] parts, string method)
        {
            Guid? id = parts.Length >= 2 && Guid.TryParse(parts[1], out var parsed) ? parsed : null;
            if (parts.Length > 1 && id is null)
            {
                throw HrisError.Missing();
            }

            var version = id is Guid entityId ? HrisRequest.Expected(http, entityId) : 0;
            var action = parts.Length == 3 ? parts[2] : "";
            return new MutationTarget(parts[0], id, version, action, method, parts.Length);
        }
    }

    private async Task<object> Execute(HrisDbContext db, Actor actor, MutationTarget target, string body)
    {
        var isCreate = target.Method == "POST" && target.Id is null;
        var isUpdate = target.Method == "PUT" && target.Id is not null;
        if (target.Length <= 2 && (isUpdate || isCreate))
        {
            return await Save(db, actor, target, body);
        }

        if (target.Length == 2 && target.Method == "DELETE" && target.Id is Guid deleteId)
        {
            if (target.Kind == "pto")
            {
                return await HrisPto.Remove(db, actor, deleteId, target.Version, false);
            }

            await HrisPeople.Delete(db, actor, target.Kind, deleteId, target.Version);
            return new { deleted = deleteId };
        }

        if (target.Length == 3 && target.Kind == "pto" && target.Id is Guid ptoId && target.Method == "POST")
        {
            return target.Action switch
            {
                "decision" => await HrisPto.Decide(db, actor, ptoId, target.Version, Parse<DecisionInput>(body)),
                "cancel" => await HrisPto.Remove(db, actor, ptoId, target.Version, true),
                _ => throw HrisError.Missing()
            };
        }

        throw HrisError.Missing();
    }

    private async Task<object> Save(HrisDbContext db, Actor actor, MutationTarget target, string body)
    {
        var id = target.Id;
        var version = target.Version;
        switch (target.Kind)
        {
            case "employees":
                return await HrisPeople.SaveEmployee(db, actor, id, version, Parse<EmployeeInput>(body));
            case "departments":
                return await HrisPeople.SaveDepartment(db, actor, id, version, Parse<DepartmentInput>(body));
            case "job-roles":
                return await HrisPeople.SaveJobRole(db, actor, id, version, Parse<JobRoleInput>(body));
            case "pto":
                return await HrisPto.Save(db, actor, id, version, Parse<PtoInput>(body));
            case "accounts":
                if (id is null)
                {
                    return await auth.CreateAccount(db, actor, Parse<AccountCreate>(body));
                }

                return await auth.EditAccount(db, actor, id.Value, version, Parse<AccountEdit>(body));
            default:
                throw HrisError.Missing();
        }
    }

    /// <summary>The ETag (when the response carries id + version) and the id to write into the audit row.</summary>
    private static (string? Tag, string AuditId) Describe(string json, Guid? targetId)
    {
        using var document = JsonDocument.Parse(json);
        var element = document.RootElement;
        var hasVersion = element.TryGetProperty("version", out var version);
        var hasId = element.TryGetProperty("id", out var recordId);
        var tag = hasVersion && hasId ? ETag(recordId.GetGuid(), version.GetInt64()) : null;

        string auditId;
        if (targetId is Guid id)
        {
            auditId = id.ToString();
        }
        else if (element.TryGetProperty("id", out var createdId))
        {
            auditId = createdId.GetString() ?? "unknown";
        }
        else
        {
            auditId = "unknown";
        }

        return (tag, auditId);
    }
}
