# Own a feature from contract to recovery

This continuation turns the HRIS workshop into a sequence of reviewable feature assignments. Start with a department description, then add a server-side employment-status filter. Each feature crosses the same boundaries: browser draft, request contract, authorization, domain rules, database transaction and response projection.

You should be comfortable reading a React controlled form, a C# record, an EF query and an HTTP response. If a boundary is unfamiliar, use the existing [codebase map](../01-CODEBASE-MAP.md) first. Plan seven sessions of 45–90 minutes, adjusting for setup and your experience. A checkpoint is complete when you can demonstrate its behavior and explain one rejected case.

1. [Feature route](01-feature-route.md): placement questions, files and checkpoints.
2. [Request and transaction traces](02-request-traces.md): versions, operation identity and atomic effects.
3. [Debugging labs](03-debugging-labs.md): stale edits, missing headers and response loss.
4. [Implementation tickets](04-implementation-tickets.md): explicit acceptance contracts.
5. [Solutions and review](05-solutions-and-review.md): reasoning, counterexamples and a rubric.
6. [Capstone](06-capstone.md): an independently delivered feature with evidence.

Use a separate fictional learning database. The [root setup guide](../../README.md) explains `Hris__Database` and the package/start scripts. The schema identifier is a compatibility check, not an automatic migration system. Do not point schema experiments at the database containing your regular workshop work.

The [prediction worksheet](labs/README.md) checks your understanding of the documented contract without starting the server. Passing it does not establish that an implementation obeys that contract. Existing application checks and their historical results remain described in [verification](../VERIFICATION.md).

Keep solutions closed until you have written a prediction or implementation. This pack adds learning material; department descriptions, employment-status filtering and searchable reference selectors remain proposed learner features until you implement and verify them.

Continue with the [extended engineering fieldbook](extended-course/README.md): 33 chapters, worked investigations, feature capstones, hints and four separate review guides.
