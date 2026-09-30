# 11. Sessions and current account state

Read `Require`, `IssueSession`, `Logout`, and `EditAccount` in [HrisAuth.cs](../../../server/Nethris.Api/Security/HrisAuth.cs). The workshop does more than parse a token and trust its old role forever. Require extracts user and session identities, loads an unexpired server-side session with the current account and employee, rejects inactive or terminated access, checks the current role against the route requirement, and returns a current actor projection.

## A token and a session have different jobs

The authentication middleware validates the token under its configured parameters. The application then checks the corresponding session and current account state. A token that has not reached its encoded expiry can still be unusable after the server session is revoked. Conversely, the mere presence of a session row is not enough if the account is inactive or its employee is terminated.

This layered design supports changes taking effect on later requests. A role downgrade is evaluated from the current account rather than a stale client display. The client refreshes its own actor through the me route, but server enforcement does not depend on the client updating its buttons first. Hidden navigation is a convenience; Require is part of the actual boundary.

## Logout is a server action

Logout deletes the matching session identified by the token's session claim, then the dispatcher commits before returning signed-out success. The client clears its local workspace after a successful logout, or when a 401 establishes that the session is no longer active. A connection failure is treated differently because the result can be uncertain. Read the client signOut path before claiming that every click immediately revokes server access.

The workshop keeps the client session in memory, and its interface explains that reloading requires sign-in again. That is a specific local workflow. Do not introduce persistent token storage as an incidental course change. A persistence proposal would need to evaluate lifetime, revocation, account switching and the application's local deployment scope.

## Account edits affect sessions and administrative continuity

Changing a password through EditAccount deletes sessions for that user. Role and active-state changes are checked on subsequent requests through current account lookup. The method also preserves at least one active Admin account when demoting or disabling an active administrator. Employee termination has a related protection so the workshop does not casually remove its last usable administrator through another route.

The preservation query excludes the account being changed and requires another active Admin whose employee is not terminated. An account merely labeled Admin but disabled does not satisfy the rule. Build fixtures that vary each condition independently. Otherwise a test can appear to cover the last-admin rule while actually relying on an unintended extra administrator in seed data.

## Exercise NH-11A: create a session-state matrix

Include missing claims, malformed user identity, malformed session identity, absent session, expired session, inactive account, terminated employee, insufficient role and valid access. Predict 401 versus forbidden behavior according to the actual application boundary. Keep token-validation tests separate from Require tests unless the full HTTP pipeline is deliberately under investigation.

For a valid control, use a current unexpired session and a permitted role. Then change one database condition per case. A test that supplies an invalid token to every scenario never reaches the server-side session checks it claims to test.

## Exercise NH-11B: demonstrate role downgrade

Start with an authenticated HR account and an allowed employee-edit route. Change its role through an authorized administrative action in a disposable database. Reuse the existing session for a later request and predict the server's decision based on current account state. Inspect both response and accepted data. The test should not rely on the client hiding the edit button.

Then describe the client experience when a refresh learns that the current tab is no longer allowed. The interface can redirect to a permitted tab and clear an editor. Distinguish that usability behavior from the authoritative server refusal. Both deserve evidence, but they answer different questions.

## Exercise NH-11C: test administrative continuity

Construct one active Admin, then attempt to disable, demote or terminate the relevant account/employee through supported routes. Add a second usable Admin and repeat. Finally make the second Admin inactive or terminated and predict whether it still satisfies preservation. Record exactly which identity the query excludes.

Do not use real passwords in shared evidence. Synthetic fixtures and redacted request summaries are sufficient to establish the state transition. The course is about session and role semantics, not collecting credentials or evaluating a production identity platform.

## Review standard

A strong answer distinguishes token validation, session existence, expiry, current account state and route role. It demonstrates revocation or downgrade with an existing session and verifies last-admin protection through each relevant path. The final explanation should make clear why the server remains authoritative even when the browser still displays an old role.
