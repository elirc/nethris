# 33. Organizational change rehearsal

This exercise combines employee, department, account, manager and PTO rules in one fictional maintenance scenario. It does not introduce a new application feature. The task is to plan a supported sequence of accepted changes, anticipate conflicts, and preserve history. Use a disposable workshop with synthetic people and record a baseline before attempting any transition.

## The starting organization

Employee Rowan leads department North, manages two direct reports, has an active Manager account, and is assigned to review one report's pending PTO request. HR wants to move Rowan to department South and eventually mark Rowan terminated. Another HR user may edit the same records during the rehearsal. This is intentionally more complicated than changing one status dropdown: several current and historical relationships constrain the sequence.

Draw the starting graph with employee identities, department membership, leadership, manager links, account role and PTO assignment. Keep display names beside identities, not in place of them. Record versions for each record you plan to edit. The drawing should make it possible to tell which relationships are current and which were captured in a request or approval history.

## Identify the immediate blockers

The employee save rejects moving a department lead before leadership is cleared. Termination rejects unresolved pending PTO, direct reports and department leadership under the current rules. A manager relationship requires a non-terminated selected manager and a valid bounded acyclic graph. A PTO decision requires the assigned person still to be the current direct manager. These constraints interact, so a random sequence of edits can strand a workflow even when each attempted form looks reasonable.

Do not assume that deactivating an account removes all references or resolves PTO. Account access, employment status and manager relationships are different state. Likewise, moving an employee's department does not automatically transfer their direct reports. The supported operations should be traced individually, and each accepted result becomes the basis for the next step.

## Exercise NH-33A: plan the transition

Write a sequence that clears or changes North's lead, assigns direct reports to valid replacement managers, resolves pending PTO through supported decisions or requester cancellation/resubmission, moves Rowan if still needed, and finally performs the intended status/account changes under the declared authority. The exact sequence may depend on the fixture and chosen business outcome, so explain each choice rather than claiming one universal order.

For every step, record actor, target identity, loaded version, operation key label, expected accepted state and the relationships that must still hold. Use a new operation key for each new intent and retain it only for exact retries of that step. A multi-step administrative plan is not one atomic transaction merely because it appears in one checklist. If step three fails after steps one and two commit, the accepted organization reflects those earlier changes.

Describe recovery from that partial progress. The correct response is to inspect current accepted state and continue or deliberately reverse supported steps, not to replay the entire checklist blindly with stale versions. A future atomic organizational-change command would be a separate feature with a much larger transaction and validation contract.

## Exercise NH-33B: introduce a concurrent edit

Pause after loading the department lead record. Let another authorized user rename the department or change its leadership and save first. Then attempt the original stale update. Predict the precondition conflict and preserve the draft. Compare current accepted values before forming a new intent. Do not refresh only the version and resend a full old representation that could erase the other user's change.

Add the outcome to the transition ledger. Explain which planned later steps remain valid and which need reconsideration. A plan based on current leadership may no longer apply after another user changes it. Concurrency recovery is therefore partly a product decision about intent, not just a mechanical retry of HTTP bytes.

## Exercise NH-33C: examine the pending PTO request

If a direct report's manager changes while a request remains assigned to Rowan, inspect both current ManagerId and AssignedManagerId. Predict whether Rowan can still view and decide the request under the Manager predicate. Determine the supported requester recovery path and explain why merely editing the displayed approver name is not enough.

If Rowan already made a manager-stage decision, preserve that history. A cancellation retains decisions, while deletion is refused once decisions exist. The eventual HR stage also requires a different reviewer. Your plan should not erase or rewrite approval history to make the organization diagram look tidy. Historical references can remain valid reasons that an employee record cannot be deleted later.

## Exercise NH-33D: distinguish termination from deletion

After the planned transitions, ask whether Rowan can be deleted. An account or historical PTO/approval reference can still block deletion even when current direct reports and leadership are resolved. Termination and deletion are not equivalent endpoints. Explain which state the product actually needs and why retaining a historical employee identity may be appropriate under the workshop's rules.

Do not weaken restrictive relationships solely to make the delete button succeed. A proposed retention or anonymization feature would need explicit history and projection policies. This rehearsal is about using and understanding existing supported transitions. Keep broader policy design clearly separate from the code's current behavior.

## Evidence package

Submit the initial graph, transition ledger, one stale conflict, the PTO reassignment consequence, and fresh accepted-state observations after the final step. Include audit records scoped to the synthetic targets and operation labels. Avoid collecting whole tables when a small relevant projection establishes the claim. Preserve tokens and private settings outside the shared report.

The final explanation should say which changes were atomic individually and which sequence was intentionally multi-step. Identify any point where the user could be uncertain because a response was lost, and show how exact replay of that one captured operation would resolve it. Do not reuse one key across the entire administrative checklist; each command has its own request identity.

## Review standard

A strong rehearsal treats organizational maintenance as a set of constrained transitions with current and historical relationships. It anticipates partial progress, version conflicts and changing decision authority. The goal is not to produce the shortest click sequence. It is to make every accepted change understandable, recoverable through supported actions, and consistent with the history the workshop preserves.
