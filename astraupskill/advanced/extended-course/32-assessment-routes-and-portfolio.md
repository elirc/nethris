# 32. Assessment routes and portfolio

The fieldbook supports several learning goals. Choose a route that matches the behavior you want to explain or implement, then use the assessment criteria to produce evidence. Reading every file is not the only way to make progress, and adding a feature is not the only valuable outcome. A precise source investigation can demonstrate skills that a broad unfinished rewrite does not.

## Route one: domain and data reasoning

Study chapters 01-10, then the first two review guides. Produce a request trace, contract matrix, normalization ledger, quantity table, manager graph set, reference-lifecycle matrix and PTO accounting ledger. The route's central skill is explaining why an operation is allowed or rejected using explicit data and relationships.

Begin with predictions on paper. For each helper or domain operation, choose a valid control and one boundary case. Record which rule you expect to decide the outcome. When a test fails earlier than expected, correct the fixture and explain the difference. This habit builds the ability to isolate causes rather than treating every exception as confirmation of your hypothesis.

The final oral demonstration should explain a non-obvious case: retaining an inactive existing department, rejecting a manager change because of a descendant's depth, or avoiding double-counting approved PTO in pending reservations. Use concrete values and identities. A definition of a term is less persuasive than a worked transition that another learner can inspect.

## Route two: reliable mutation and recovery

Study chapters 11-16, 22-25 and 29. Produce a session-state matrix, ETag table, replay matrix, transaction failure ladder, client state diagram and PTO recovery evidence package. This route connects server acceptance with the client's ability to know what happened.

Keep separate columns for accepted server state and client certainty. They can diverge after response loss. A successful exact replay resolves uncertainty without repeating the business effect. A known validation rejection allows a corrected new intent. A stale conflict requires comparison with current data. These distinctions should appear in both your diagrams and user-facing explanations.

For the final demonstration, reproduce a lost-response case and show one accepted deduction or creation. Then change fingerprint material under the same key and demonstrate conflict. Include a late rollback case so the presentation does not imply that every failure occurs after acceptance. The strongest route artifact is a causal ledger with fresh accepted-state observations.

## Route three: deliver a small full-stack feature

Study chapters 17-23 and 27-30, using either department description or employment-status filtering as the main assignment. Begin with a feature ticket that names current and desired behavior. Map the affected contract, domain, persistence, projection and client paths before editing. Keep the first implementation narrow enough to verify completely.

For description, make omission and normalization semantics explicit and address schema compatibility. For status filtering, compose predicates before Count and paging, retain stable order, and reset client offset. Both inherit authorization, versioning, receipt and recovery obligations where their mutation or read paths use them. A new control is only one part of the feature.

The final report should include a successful path, direct invalid input, a stale or obsolete-response case, and a compatibility statement. If you chose a fresh database for a schema exercise, say that migration was not implemented. If you implemented migration, include preserved identity and relationship evidence. Do not let a polished screenshot substitute for the boundary the feature changes.

## Route four: maintenance and review

Study chapters 21-27 and 30-31. Produce a schema-rehearsal plan, test-layer map, failure matrix, audit interpretation, performance decision and refactor review. This route is useful for learners who want to become better at reviewing changes and diagnosing reports before editing code.

Choose three fictional incidents and identify the smallest observation that separates competing hypotheses. At least one should conclude that the current implementation follows its contract and the improvement belongs in documentation or UI guidance. This prevents a common maintenance habit: changing code whenever a result is surprising, without first checking the intended rule.

The final review should name a plausible wrong implementation and the evidence that would reject it. For example, an audit helper with its own transaction may pass ordinary happy-path tests but fail late rollback. A client status filter may look correct on page one but fail when matches occur later. Concrete counterexamples make a review actionable.

## Common assessment rubric

Use five dimensions worth twenty points each: contract accuracy, causal reasoning, experiment design, preservation, and communication. Contract accuracy requires exact current behavior and clear proposal labels. Causal reasoning traces actor and state through the deciding boundary. Experiment design uses valid controls and independent expected results. Preservation checks accepted state and valuable drafts after rejection. Communication makes the work reproducible without relying on hidden conversation context.

For each dimension, zero means missing or fundamentally incorrect; five means a partial idea with major gaps; ten means the main path is understood but important boundaries are omitted; fifteen means a sound account with limited remaining uncertainty; twenty means precise, reproducible reasoning with a meaningful counterexample. Use the rubric to identify a next action rather than merely assign a grade.

A submission can score well without implementing every proposed feature. Scope matters. A source-reading route should be judged on accurate traces and evidence, while an implementation route must demonstrate its actual changes. Do not award implementation credit for a design document or penalize an explicitly scoped design for not claiming deployment.

## Evidence quality ladder

An unsupported assertion is the weakest evidence: it says the feature works without showing why. A source trace is stronger when it names the actual functions, assumptions and branch conditions. A controlled prediction adds an expected result for a specific fixture. An executed test adds an observation from a named environment. A fresh accepted-state read can establish persistence beyond an in-memory result. A reproducible integration package connects several boundaries into one causal account.

These levels are not a demand to run a browser for every rule. The right evidence depends on the claim. A pure quantity conversion can be established with a small test and an independent table. A stale-draft recovery claim needs the component or browser behavior. A transaction rollback claim needs the actual persistence boundary. Choose the smallest observation that fully supports the statement.

Every artifact should say whether it is a prediction, proposal or executed result. This makes uncertainty expressible and prevents accidental overclaiming. If an experiment could not run, record the reason and the exact remaining question. Do not fill the gap with an invented output or label an uninvoked suite as passed.

## Oral defense prompts

Prepare concise answers to the following prompts. Why can SaveChanges succeed while the overall request later rolls back? Why does exact retry preserve the serialized body? Why can a current-version request still receive 409? Why is a nullable salary field not equivalent to zero? Why does an employee manager change require descendant checks? Why can an approved request participate in overlap but not pending reservation sums? Why can an old authorized response be inappropriate for the current client session?

Answer each with a specific example. Use a version-four editor, a ten-hour pending request, an inactive current department or a delayed generation-one response. Concrete examples expose whether the mechanism is understood. If your answer relies only on terms such as concurrency, security or state management, add the actual sequence and observer.

## Revision practice

After completing a route, return later to a small set of prediction tasks without opening the solutions. Change one meaningful condition: move a date to the exact boundary, change only the request key, retain the key but alter the ETag, deactivate an existing reference, or change a manager relationship after submission. Predict again and compare with source or a controlled run.

Avoid counting cosmetic renamings as new mastery. Useful variants change a boundary or relationship that can alter the result. If you repeatedly miss the same distinction, write a short corrected rule and one counterexample to the wrong rule. For example: receipt identity is user plus key and exact fingerprint, not body meaning alone. Then test a case that makes the difference visible.

Keep a learning log with original prediction, observed result, corrected explanation and next check. Preserving a wrong initial prediction is useful because it shows what changed in your understanding. The goal is reliable future reasoning, not the appearance that every first answer was correct.

## Portfolio assembly

Select a coherent subset of artifacts: one system trace, one state or schema diagram, an acceptance matrix, a successful path, two rejected paths, a recovery demonstration and a verification note. Give each a short caption explaining the claim it supports. Keep synthetic employee data and omit tokens, real passwords and private signing material.

State the delivery scope at the beginning: source investigation, proposed feature, implemented local feature, verified package or published deployment. These are different achievements. A local feature can be valuable without being production-ready, and a design can be thoughtful without being implemented. Accurate scope makes the work easier for a reviewer to trust.

Include the exact commands that ran, their outcomes and relevant environment versions. Separate a setup failure from an assertion failure. Link artifacts to the source state they describe. If unrelated local changes existed, preserve and disclose that limitation rather than claiming a clean revision. Your report should be understandable to someone who did not participate in the authoring session.

## A final review meeting

Ask a peer to choose one acceptance case and reproduce it from your instructions. Observe where they need to guess. Missing working directory, unspecified fixture state, unclear actor role or an ambiguous saved-versus-draft claim are documentation defects you can repair. Do not explain the missing step verbally and leave the written guide unchanged; update the artifact so the next learner benefits.

Then ask the peer to propose a plausible wrong implementation. Identify which test or observation would catch it. If none does, add a focused case or narrow the claim. This review process turns the portfolio from a collection of files into a demonstration of engineering judgment: explicit contracts, controlled evidence, preserved state and honest conclusions.
