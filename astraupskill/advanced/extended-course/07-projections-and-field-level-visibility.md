# 07. Projections and field-level visibility

Read the View methods in [HrisPeople.cs](../../../server/Nethris.Api/Domain/HrisPeople.cs). They turn tracked domain records and related data into response contracts. The employee projection conditionally includes PTO balance based on the actor. The job-role projection conditionally includes salary bands. This is an example of field-level visibility: two authorized readers of the same record can receive different representations.

## Derive the employee balance rule

The balance is visible to HR or higher, to the employee themself, or to a Manager actor who is the employee's current direct manager. Otherwise the projection returns null. Write those conditions as a truth table. Notice the exact Manager comparison in the third branch and the separate broader HR branch. A vague statement that managers can see balances omits which employees they manage.

Use four fictional employees: a requester, their direct manager, a manager of another team, and an HR user. Compare the returned balance for the same employee. Then change the employee's manager relationship and repeat. Visibility depends on current relationships, so a cached response can outlive the authority or context that produced it. This observation matters when designing refresh, logout and account-switch behavior.

## Salary-band visibility is a different rule

Job-role salary bands are returned only to HR or higher. Being a manager or holding that job role does not satisfy this projection's condition. Do not generalize one sensitive-field rule to every field. The response type uses nullable decimal values, so the client must render hidden values without implying zero or an empty configured band.

A frontend-only hide can improve presentation but cannot protect a value already sent to the browser. The relevant protection here is in the server projection before serialization. A useful test inspects the response body for an unauthorized reader and confirms the sensitive numbers are absent as values under the declared null representation. A screenshot with a hidden column is weaker evidence.

## Projection completeness and query shape

The employee query includes department, job role, manager and direct reports because the view uses their names and counts. A projection that assumes navigation properties are populated can behave differently if a future caller uses a narrower query. Record this dependency in your source map. If you refactor to a direct database projection, preserve both visibility conditions and stable response meaning.

The direct-report count is a derived fact from the loaded relationship collection. It is not an editable employee input. Display labels are likewise derived from related records. A client should retain stable identities for requests rather than reconstruct relationships from names. Two people can share a name, and a department can be renamed while keeping identity.

## Exercise NH-07A: produce a visibility table

For employee balance, test self, current direct manager with Manager role, unrelated Manager, Employee viewer, HR and Admin. For salary bands, test Employee, Manager, HR and Admin. Use the same underlying values in each comparison so changes in representation are attributable to actor context.

Include an expected-null assertion and a control asserting the real value for an allowed actor. If every response is null because the fixture never set a balance, the negative assertion proves little. A distinctive nonzero value makes accidental exposure and accidental universal redaction visible.

## Exercise NH-07B: design a safe client state transition

Imagine an HR user loads salary data, signs out, and another employee signs in on the same page. Describe which cached rows, selected details and pending responses must be invalidated. A late response from the former session should not repopulate the new session's interface. Inspect the actual client generation and session checks in a later chapter before claiming the current implementation handles every case.

For now, draw the timeline and define the desired observable behavior. This connects authorization with asynchronous ownership: a response can be valid for the request that produced it while no longer relevant to the current viewer. Server authorization and client display relevance both matter, but they enforce different boundaries.

## Exercise NH-07C: add a proposed sensitive field

Suppose a future employee note should be visible only to HR. Specify input authorization, stored representation, read projection, list behavior, export behavior and audit treatment. Do not stop at hiding a form control. Identify every path that could serialize the value, including error payloads or diagnostic logs if your proposal adds them.

Keep the scope concrete. You do not need to invent an enterprise policy framework for one learning field. A small explicit visibility rule with direct boundary tests can be clearer. The important requirement is consistency across the actual supported paths.

## Review standard

A strong solution tests response data under multiple actors and avoids interpreting null as a real numeric zero. It connects projections to query dependencies and to client session transitions. The final evidence should explain which fields differ, why they differ, and which layer prevents unauthorized values from reaching the browser.
