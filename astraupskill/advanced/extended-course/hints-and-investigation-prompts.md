# Hints and investigation prompts

Use these prompts when an exercise is stuck, before reading the full review guides. They point toward a useful observation without supplying every expected result. Write down which hint helped and why. The purpose is to develop a method you can reuse when the next codebase has different names and frameworks.

## When a source trace feels too large

Choose one user action and one field. Follow the value from the form to the request helper, dispatcher, domain operation, persistence and response. At each stage ask what changed and what has only been checked. Do not expand every import immediately. If a helper is pure, note its contract and continue; if it opens a context, starts a transaction or sends a response, investigate its ownership carefully.

Use a different symbol for attempted state and accepted state in your diagram. This small visual distinction often reveals the missing step in a confusing explanation. A tracked employee assigned a new name and a fresh database read showing that name are different observations. If your diagram cannot represent that difference, refine it before running more tests.

## When a negative test passes too easily

List every invalid condition in the fixture. If there is more than one, identify which is encountered first. Repair all but the intended condition and add a valid control. Check required headers, current actor, operation key, body shape, reference identities and version. An exception with the wrong cause is a clue about setup, not successful coverage of the named rule.

Ask which plausible broken implementation would still pass the test. If removing the rule under investigation would not change the result because another rule rejects first, the test is weak. Add an assertion or fixture that makes the intended decision observable. This method is often faster than adding many more broad negative cases.

## When a text case is confusing

Show invisible characters explicitly and write both raw and normalized representations. Locate Trim relative to length and control checks. Distinguish whitespace-only from embedded whitespace, and field length from request byte size. For OptionalText, compare only the rows that differ from Text before assuming the helpers have unrelated behavior.

For email or identity-like text, inspect normalization and database comparison together. A user-visible spelling difference may collapse to the same accepted identity value. Do not assume a name, email and employee number all share identical semantics. Follow each actual helper and mapping.

## When a quantity result looks four or one hundred times too large

Write the unit next to the value at every boundary. Hours become quarter units; currency amounts become cents. Compare bounds only after converting them to the same unit. Check whether the invalid input is rejected before conversion or accidentally truncated. Use exact decimal literals in the relevant language rather than introducing a different numeric representation during the test.

For a round-trip property, define the accepted input domain first. A conversion that rejects unsupported increments is not required to preserve every arbitrary decimal. Separate acceptance tests from rejection tests and give each an independent expected result.

## When a manager or approval rule seems contradictory

Write down role, employee identity, current manager identity, assigned manager identity, workflow status, version and prior approvals. Do not reduce authority to a single role label. Visibility and decision permission can differ. A person can see a record yet be prohibited from making a particular transition.

For manager graphs, draw the proposed edge and then inspect descendants. A local edit can lengthen someone else's chain. For PTO, distinguish the current organization relationship from the assignment captured when the request was submitted. The source may deliberately require both to match.

## When balances do not add up

Create separate columns for stored balance, pending reservations and approved deductions. Exclude the edited request from other pending hours before adding its proposed amount. Follow status changes through the ledger. Approved hours have already affected stored balance and should not be counted again as pending under the current implementation.

If you propose a refund or adjustment, give it a new explicit command and history meaning. Do not assume that cancellation of pending work and reversal of approved history are the same operation. A ledger should explain where every accepted quantity came from without fabricating missing historical events.

## When a retry conflicts

Compare method, path, query, If-Match and serialized body, then compare user and operation key. The same visible form is not proof of the same request. Determine whether the client retained the original captured operation or rebuilt it. A changed version or reordered serialization can matter under an exact fingerprint contract.

Next ask whether the receipt is still retained and whether current authority permits replay. A key is not a permanent permission token. Keep expiration and role changes separate from fingerprint mismatch so the diagnosis points to the actual boundary.

## When the UI shows old data

Identify which request produced the data and which context currently owns the screen. Record generation, session, tab, offset and applied filter. Resolve controlled promises in the opposite order from their start order to expose stale publication. Also test an old failure after a new success; obsolete errors can be as confusing as obsolete rows.

Cancellation and relevance are different questions. Aborting a request may save work, but a result that raced with cancellation still needs an ownership decision. Look for checks after awaits, not only before the first request begins.

## When a migration appears to work

Compare identities and relationships, not only row counts. Verify settings and history relevant to the change. Preserve the original synthetic file and rehearse on a copy. Matching schema markers do not prove every required field or setting is valid, and changing markers alone is not a schema conversion.

State whether the exercise supports a fresh database or an upgrade from an existing one. Both can be legitimate learning scopes. The mistake is claiming the latter after testing only the former. A precise limitation is more useful than an apparently seamless reset that discards work.

## When a report feels hard to write

Start with the concrete outcome and the evidence that supports it. Name what changed, the boundary tested, the command or action used, and any material unexecuted scope. Keep passed, failed, setup-blocked and not-run separate. Do not use a large word count or test count as a substitute for explaining the important behavior.

Finish by asking whether another learner can reproduce the result without your verbal help. If not, add the missing fixture, actor, working directory or expected observation. Clear engineering writing is part of the upskilling work because it lets other people inspect and build on your reasoning.
