# Review guide 2: workflows and recovery

This guide supports chapters 09-16. Attempt the ledgers, state diagrams and controlled schedules first. The answers below emphasize what each observation proves. When a case requires a database or browser run, the discussion is an expected result and review standard until you execute it in your own disposable environment.

## Reservation accounting step by step

For NH-09A, begin with stored balance forty and no requests. Creating pending request A for eight leaves stored balance forty and pending reservation eight. Creating pending request B for twelve leaves stored balance forty and pending reservation twenty. Editing A from eight to ten excludes A's old hours from the other-request sum, so the proposed pending total is twelve plus ten, or twenty-two. Counting old A as well would incorrectly produce thirty.

Manager approval of A moves it to PendingHr and retains its reservation. Final HR approval deducts ten from stored balance, leaving thirty, and removes A from the pending subset because it is now Approved. B still reserves twelve. A new pending request for eighteen fits exactly against the thirty-hour stored balance together with B. Adding A's approved ten to the reservation sum would double-count an obligation already deducted.

Rejection or cancellation of a pending request removes it from the pending accounting set without refunding an approved deduction, because no deduction occurred for that pending request. The current remove path does not support canceling Approved history. If your ledger shows a refund from that path, it describes a proposed feature rather than the implemented workshop. Label such an extension and give it a distinct command and audit meaning.

For NH-09B, choose two requests whose combined pending hours exceed available balance even though each fits alone. Sequential mock calls against independent stale lists cannot establish concurrency safety. The integration test must exercise the real transaction and inspect the final accepted set. Busy, conflict and accepted responses should be recorded distinctly. The invariant is that accepted pending reservations do not exceed the stored balance under the tested state, not that every competing call receives a particular scheduling order.

For NH-09C, an adjustment ledger can make balance changes explainable, but migrating from a current balance cannot recover missing historical events. A sound proposal records an opening balance at a declared migration point and future explicit adjustments/deductions. It does not fabricate a sequence of past accruals to make the ledger look complete. The opening record's meaning should be visible in reports.

## Approval authority and history

For NH-10A, pending-without-decisions is the editable and deletable requester state. Pending with decisions can be cancelled but not rewritten or deleted through the same path. Closed states are outside pending removal. Manager-stage decisions require the assigned current direct manager and prohibit self-approval. HR-stage decisions require HR or above and a reviewer who has not already decided this request. Visibility is broader for the requester and HR than decision permission.

A useful truth-table counterexample is an HR actor who already performed the manager stage. Their rank is sufficient for HR work in general, but the existing approval history excludes them from the final decision on this request. Another is a former manager still captured as AssignedManagerId: the current employee relationship no longer matches, so the first-stage decision is forbidden. These cases show why role-only tests are incomplete.

For NH-10B, insufficient final balance should reject without an accepted approval or deduction. An invalid comment can fail after some tracked fields have been assigned, so inspect fresh accepted state after the transaction ends. The same-context object may show attempted changes. A review can recommend validating the comment earlier for clarity, but that recommendation should not be confused with evidence of committed partial state.

For NH-10C, delegation adds a new source of authority that must be evaluated at a defined time. Capturing a delegate at submission and consulting current delegation at decision are different policies. Exact replay after delegation expiry raises another question: is the stored result still authorized to be returned? A complete proposal defines this rather than treating receipt presence as unconditional permission.

## Current session state

For NH-11A, malformed or missing identity/session claims, absent or expired session, inactive account and terminated employee lead to unauthorized-session behavior in Require. A valid current account with insufficient route role is forbidden instead. Token cryptographic validation belongs to the middleware boundary before these database checks. If every test uses an invalid token, none demonstrates the later session conditions.

For NH-11B, downgrading a role affects a subsequent protected request because Require reads the current account. The browser may still display the old role until refresh, but its stale interface does not grant authority. A good test reuses the same existing session after the administrative change and confirms the server refusal and unchanged target data. A separate client test can verify the tab/editor response after me returns the new role.

For NH-11C, preserving an administrator requires another active Admin with a non-terminated employee, excluding the account being changed. A disabled Admin or one attached to a terminated employee does not satisfy the query. Test demotion, account disabling and the employee-termination path where relevant. Seed data should be controlled so a hidden extra administrator does not accidentally make every operation pass.

Password changes delete that user's sessions in the account-edit path. Logout deletes the current session and commits before reporting success. A client connection failure during logout does not itself prove revocation or non-revocation. The interface preserves the distinction between known session invalidation and other failures. Reports should not print tokens or actual signing material to establish these behaviors; scoped identities and response/state assertions suffice.

## Preconditions and versions

For NH-12A, missing or malformed If-Match relative to the target produces the application's 428 precondition error. A correct target identity with positive parseable stale version reaches domain version comparison and conflicts. The exact public status contract here is not replaced by whichever convention another API uses. Test the current helper and middleware behavior.

A wrong target identity in an otherwise ETag-shaped header is not evidence that the target record itself changed. The header fails the expected target prefix contract. A zero or nonnumeric version likewise fails parsing. Keep the operation key and body valid so these cases reach Expected. For the successful control, assert the returned identity and advanced version, not merely an HTTP success code.

For NH-12B, editor A and B both observe version four. A's accepted edit creates version five. B's full candidate still contains old fields alongside its intended change. Automatically substituting version five could overwrite A's unrelated fields. The deliberate comparison action lets the user form a new intent against current data. That new captured operation needs a new key because its precondition or body differs from the old request.

For NH-12C, duplicate-name or relationship conflicts can return 409 with the record version unchanged. The client loads current data and compares versions rather than assuming every 409 proves a stale row. A useful message explains the actual domain rule. Repeatedly reloading cannot make a duplicate department name unique unless accepted data changes for an independent reason.

## Exact receipt behavior

For NH-13A, same user, key and fingerprint selects replay while retained. Changed body, query or If-Match under the same key conflicts. Another user with the same textual key has a different receipt scope. An expired receipt is removed by the cleanup predicate before lookup, so the later request follows fresh-operation behavior subject to current domain state. A revoked session fails before ordinary receipt handling.

The fingerprint is built from exact selected request strings. Two JSON objects with equal meaning but different whitespace can hash differently. The captured client body avoids that accidental variation for retry. Do not promise canonical semantic deduplication. Likewise, two different keys with identical bodies are two logical operations under the protocol, even if later domain rules prevent duplicate business results.

For NH-13B, a final PTO approval should deduct once and create one final-stage approval. Exact replay returns the stored representation without rerunning Execute. Inspect business state, not only the replay header. Discarding a known successful response is a valid bounded simulation of client uncertainty after acceptance, but report it as such rather than as exhaustive network-failure coverage.

For NH-13C, current authentication and PTO-specific replay checks matter after role or relationship changes. A receipt does not permanently grant the authority its creator once had. The test should record current actor role, visibility predicate and prior approval stage. Otherwise a forbidden result can be hard to attribute to the intended replay rule.

## Transaction failure ladder

For NH-14A, a body rejection occurs before domain mutation. A domain validation error can leave temporary tracked assignments depending on the path but should not create accepted effects. A domain SaveChanges failure or a later audit/receipt save failure remains inside the outer transaction. A commit failure is not a successful accepted response. A delivery failure after successful commit is different: accepted business, audit and receipt can exist while the client lacks confirmation.

For NH-14B, begin with known accepted fields/version and scoped audit/receipt counts. Inject a late failure, then use a fresh context after the request ends. All accepted categories should remain at the prior state under the atomicity claim. Remove the fault and run a successful control. If the control also fails, the negative test may only demonstrate a broken fixture rather than the intended rollback boundary.

For NH-14C, an extracted audit helper opening its own context can commit independently. Awaiting it does not merge its transaction with the caller's. The concrete counterexample is audit committed, caller later fails, leaving an audit action with no accepted business change; or business accepted while an independent audit fails. A review should request shared transaction ownership or a deliberately different consistency contract, not merely more try/catch blocks.

## Client recovery states

For NH-15A, editing holds mutable draft; submitting holds captured operation and busy state; uncertain pending retains the operation after the active wait ends; known rejection clears pending but can retain editor; stale comparison retains draft and current saved row; successful mutation clears pending/editor and can proceed to refresh; signed out clears workspace. These distinctions explain which controls are enabled and why a single boolean error flag would be insufficient.

For NH-15B, assert equality of key, serialized body, route and ETag across retries. A request count of two alone does not prove exact retry. The known-validation contrast should allow a corrected draft to create a new operation. For NH-15C, a failed refresh after a successful mutation must retain the known saved fact. The client already received acceptance, so it should not offer the old mutation as if its result were still unknown.

For NH-16A, resolve newer B before older A and confirm only B publishes. Then fail obsolete A and confirm its error is suppressed. Exercise both the initial me stage and the later parallel data stage because generation can change during either await. For NH-16B, clearWorkspace increments generation before clearing state, preventing an old session's response from restoring data. For NH-16C, cancellation can reduce work but does not replace relevance checks; a result can race with cancellation or arrive from a path that did not honor the signal.

The combined review standard is causal precision. Your diagrams should identify the actor, record basis, operation identity, transaction boundary and current UI owner. When those are explicit, apparently contradictory observations become understandable: saved data with an uncertain client, changed tracked objects with unchanged accepted storage, or a valid old response that must not appear in the new session.
