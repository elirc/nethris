# 13. Operation receipts and exact replay

Read `Mutate`, `Fingerprint`, and `Replay` in [HrisApi.Mutations.cs](../../../server/Nethris.Api/HrisApi.Mutations.cs). Every ordinary mutation requires an Idempotency-Key in UUID D format. The dispatcher scopes receipt lookup to the current user account and key. An existing receipt with the same fingerprint can replay the stored response; the same key with different request material produces a conflict. A fresh key executes the mutation and records its result.

## Define what exact means here

The fingerprint material includes method, request path, query string, If-Match header and body text, separated by newlines and hashed. This is exact request identity at the selected representation, not abstract semantic equivalence. Reordering JSON properties or changing insignificant whitespace can change the body bytes and therefore the fingerprint. The client avoids that ambiguity by capturing JSON once in its operation object.

Do not describe the receipt as deduplicating every request that means the same thing. Two independently created operation keys can represent two separate commands even with identical bodies. Conversely, reusing one key for a changed version or body is rejected. The protocol depends on a stable definition of logical intent on the client and a precise fingerprint on the server.

## Receipt contents and acceptance

The receipt stores response JSON, status code and ETag alongside the user, key, fingerprint and expiry. The mutation's audit row and receipt are saved under the same outer transaction as the domain change. After commit, the server sends the stored response representation. If delivery fails, a later exact retry can return that accepted result without rerunning the business operation.

A replay commits its transaction and sets the Idempotency-Replayed response header before sending the stored response. The replay path does not call the ordinary Execute branch again. Test this using business effects, not only the header: employee version, approval count, balance deduction and audit count can reveal accidental repetition. Choose the relevant effect for the operation under test.

## Expiry bounds the guarantee

The code removes receipts whose expiry is at or before the current instant before looking for a prior receipt. New receipts expire twenty-four hours after creation. An exact key retried after expiry no longer has that stored replay record available. Do not promise indefinite replay recovery. The client guidance to keep the page open preserves the captured operation locally, but it does not extend server receipt retention.

Use fixed or controlled time in a dedicated test design to investigate just before, exactly at and after expiry. If the existing integration harness does not inject time, distinguish a design-level prediction from an executed boundary test. Avoid waiting a real day merely to claim deterministic coverage when a controlled fixture can express the intended state.

## Replay still has authority requirements

The dispatcher authenticates and obtains the current actor before entering ordinary mutation handling. PTO decision replay additionally verifies that the actor can still view and decide under the relevant current conditions. It checks manager-or-higher role, current visibility, and whether an earlier HR-stage decision by this actor now requires HR authority the actor no longer has.

A stored success is therefore not an unconditional bearer capability. Knowing another user's key does not retrieve their receipt because lookup is user-scoped. Retaining one's own key does not bypass current authorization. This is an important interaction between caching accepted results and protecting information after relationships or roles change.

## Exercise NH-13A: write a replay matrix

Include same user/key/fingerprint, same user/key with changed body, changed If-Match, changed query, different user with the same textual key, expired receipt, and a revoked session. For each, identify whether the route reaches replay, conflicts, executes a fresh operation, or fails earlier. Keep other request details valid so the intended distinction is visible.

Do not assume a different user with the same key should receive the first user's result. The lookup scope is part of the identity. For an expired receipt, explain that the later business preconditions still apply; the absence of a receipt does not guarantee that a repeated stale edit will succeed as a fresh operation.

## Exercise NH-13B: simulate lost delivery

Design an isolated HTTP test in which the mutation commits but the client does not receive a usable response. Preserve the original operation object, retry it, and inspect the returned representation and business effect counts. If you simulate response loss by discarding the first successful client result, state that limitation: it tests replay behavior after known commit, not every possible network failure mechanism.

For a PTO final approval, verify balance is deducted once, one stage approval exists, and the replay returns the stored result. A second response with the same JSON is insufficient if hidden duplicate accounting occurred. Independent database observations make the claim stronger.

## Exercise NH-13C: test changed authority

Choose a PTO decision accepted under a valid manager or HR relationship. Change the relevant current authority through supported administrative operations, then retry the original captured decision. Predict which replay checks apply. Record the current actor, request visibility and prior approval stage so the result is explainable.

## Review standard

A strong answer names fingerprint inputs, user scope, atomic receipt creation, retention and replay authorization. It proves one accepted business effect in the lost-response scenario and avoids claiming semantic deduplication or permanent recovery beyond the actual contract.
