# 25. HTTP input budgets and errors

Read [HrisRequest.cs](../../../server/Nethris.Api/HrisRequest.cs) and the middleware in [Program.cs](../../../server/Nethris.Api/Program.cs). The workshop bounds request bodies to thirty-two KiB at the host and helper layers. The helper checks declared ContentLength when present and also counts streamed bytes while reading. This second check matters because a declared length is not the only way a body can arrive.

## Size, encoding, syntax and meaning

The body reader rejects excessive bytes, requires the chosen JSON media type for ordinary non-delete writes, decodes strict UTF-8, and then the parser handles JSON. Domain rules follow after deserialization. These are separate stages. A byte limit is not a character limit; non-ASCII text can use multiple bytes per character. A valid UTF-8 string can still be malformed JSON, and valid JSON can still violate the request contract.

The DELETE branch returns an empty body after the initial declared-size check. Do not generalize the JSON-body requirement to every method without reading this branch. Exact request fingerprints include the body supplied by the helper, so method-specific body handling also connects to replay semantics. A future endpoint with a different media type would need an explicit contract rather than silently bypassing the shared reader.

## Query numbers are bounded inputs too

The Number helper applies defaults only when a query parameter is absent. If present, the value must parse as an integer within the supplied bounds. An invalid present value does not silently become the fallback. The read dispatcher uses this for offset and limit. That distinction prevents a malformed request from appearing to succeed with a plausible default page.

Write cases for absent, valid minimum, valid maximum, below minimum, above maximum, fractional-looking text and nonnumeric text. Keep route and authorization valid. A request rejected before query parsing does not establish the numeric rule. The same focused-fixture discipline applies at HTTP boundaries as in pure tests.

## Error mapping is a public contract

The middleware translates known domain errors, concurrency failures, database contention, constraints, JSON problems and unexpected exceptions into public responses. It checks whether a response has already started before attempting to write an error. That matters because a late delivery failure cannot always be turned into a clean alternative JSON response after bytes have been sent.

Do not expose an internal exception message as a universal user instruction. The existing mapping uses bounded public messages while logging unexpected failures through the server logger. For a new feature, choose a domain error that communicates the actionable rule where possible. A generic internal failure should not falsely accuse the learner of invalid input.

## Exercise NH-25A: make a stage-isolation suite

Design one valid control and one case each for oversized declared body, oversized streamed body, wrong media type, invalid UTF-8, malformed JSON, unknown property and validly shaped domain rejection. Identify which test harness can produce each condition. A high-level JSON helper may make it difficult to send invalid bytes, so use a lower-level request construction where needed.

For the size boundary, create exact-limit and one-byte-over fixtures whose contents are otherwise appropriate to the stage being tested. If the exact-limit JSON violates a later domain shape, distinguish successful size acceptance from overall request rejection. One response cannot be interpreted without knowing which boundary failed first.

## Exercise NH-25B: analyze resource ownership

Explain where the body bytes, decoded string, parsed request and response JSON exist during a mutation. The bounded reader still accumulates the body in memory. It is not a constant-memory streaming domain processor. The small request limit makes that design bounded, but the source does not prove an optimal throughput target.

Propose a measurement for a maximum-sized valid request and a rejected oversized request. Record memory or latency observations only if actually measured. Do not infer denial-of-service resilience for every deployment from a single local body limit; the application is explicitly a loopback workshop with a narrower operating context.

## Exercise NH-25C: write actionable errors

Draft feature-specific messages for an invalid status filter, a stale department description edit, and a missing reference selection. Each should explain the next reasonable action without exposing private data. Keep uncertain-save wording distinct from known validation rejection. The learner should know whether to correct input, compare current state, or retry the exact operation.

## Review standard

A strong answer separates bytes, encoding, JSON, shape, query parsing and domain meaning. It tests absence versus invalid presence, names method-specific behavior, and keeps error messages proportional to what the server knows. The final matrix should let a reviewer identify the exact boundary each case establishes.
