# 19. Feature workshop: department description

This is a proposed learner feature. The current DepartmentInput and DepartmentView do not contain a description. Implement the feature in a separate practice branch or copy after completing the source-reading chapters. Its purpose is to make a small change travel coherently through contract, domain, persistence, projection, form, tests and verification. A text field is small in the interface but still crosses real boundaries.

## Define the acceptance contract

Choose an optional bounded description, using the existing OptionalText behavior if appropriate. State that null and whitespace-only input become null, ordinary text is trimmed, excessive normalized length is rejected, and embedded control characters are rejected after trimming. Include surrounding tabs and newlines in the examples so the helper's actual normalization order is understood.

Decide how an omitted property from an older client is handled. With a full replacement-style input record, treating omission as null can clear an existing description. You may require coordinated client/server deployment for this workshop or design explicit compatibility behavior. Either choice must be documented. Do not accidentally promise patch semantics while implementing ordinary record deserialization.

## Map the change before editing

Identify the Department domain model, request and view records, SaveDepartment, database compatibility mechanism, form metadata, card or detail rendering, and tests. Add the field to the input contract without accepting arbitrary unknown fields globally. Normalize at the domain boundary and return the accepted representation. Preserve identity, version, lead relationship and active-state behavior.

The database schema identifier is a compatibility mechanism in this project, not evidence of an automatic migration framework. Inspect [HrisDatabase.cs](../../../server/Nethris.Api/Data/HrisDatabase.cs) before choosing a persistence update. A model property alone does not guarantee that an existing SQLite file acquires the corresponding column. Use a disposable database for schema experiments and write a deliberate compatibility or migration plan.

## Keep the existing mutation protocol

Description edits should still require appropriate HR authority, a valid operation key and the record precondition for existing departments. The ordinary mutation envelope should still create audit and receipt and commit atomically. Do not add a quick special endpoint that bypasses those boundaries just to make the field easier to save.

An exact retry of a description change should replay its accepted result. A changed description under the same operation key should conflict because the body fingerprint differs. A stale department version should retain the learner's draft. These are inherited feature obligations, not optional extras to add after the happy path.

## Exercise NH-19A: write the feature ticket

Produce a ticket with current behavior, desired behavior, normalization examples, compatibility choice, affected source paths and acceptance cases. Include create with description, create without description, edit description, clear description, excessive input, embedded control, unauthorized write, stale version and exact replay. Keep the list focused on distinct boundaries rather than many cosmetic wording variations.

The ticket should state where the description appears and how an empty value is displayed. Avoid showing the literal string null or an ambiguous zero-length card. A concise placeholder or omission may be appropriate. Describe the user meaning rather than exposing schema details in the ordinary interface.

## Exercise NH-19B: implement a vertical slice

Begin with domain and contract support plus a focused test. Add persistence compatibility deliberately, then expose the field in the form and view. After a successful save, reload through the API or browser and inspect the normalized accepted text. The visible draft alone is insufficient persistence evidence.

Use a recognizable description containing surrounding spaces to demonstrate normalization, then an embedded control fixture through a direct request to demonstrate server enforcement. Do not depend on a browser text area to generate every invalid case. The authoritative boundary should reject a crafted request as well.

## Exercise NH-19C: test rollback and recovery

Use the existing transaction test approach to inject a late failure after the department change is attempted. Verify description, version, audit and receipt remain at their prior accepted state. Then test a stale edit from two clients and show the description draft is available for comparison. These checks connect a seemingly simple field addition to the system's established guarantees.

For compatibility, open an old-format disposable database under the chosen deployment plan and record the actual outcome. If the application intentionally refuses incompatible schema, call that refusal a compatibility check rather than a migration failure. If you implement migration, verify preservation of existing department identities and relationships.

## Review standard

A complete feature submission includes the accepted contract, coherent source changes, schema handling, useful interface behavior and targeted evidence. It should not claim that adding one property to a C# record finishes the feature. The strongest review explains how the small addition preserves the existing transaction, authorization and retry protocols.
