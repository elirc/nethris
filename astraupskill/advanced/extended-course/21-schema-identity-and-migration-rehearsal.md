# 21. Schema identity and migration rehearsal

Read the constructor in [HrisDatabase.cs](../../../server/Nethris.Api/Data/HrisDatabase.cs). It resolves the configured path, opens SQLite, begins an immediate transaction, reads application and user-version markers, and refuses an existing file whose markers do not match the expected workshop identity and version. A new file receives the generated schema and initial settings. This is a compatibility boundary, not an automatic migration system.

## Refusal preserves meaning

An existing unknown file is not silently adopted or reset. The error instructs the operator to choose a new database path or perform an explicitly reviewed migration. This matters because a filename ending in sqlite does not establish that its contents belong to this application. Overwriting it to make startup succeed would destroy evidence and possibly unrelated data.

The foundation test creates an empty existing file and verifies that opening it is refused without changing its zero length. That is a focused preservation claim. It does not prove every corrupt database scenario is recoverable, but it establishes that the application does not treat this unknown file as permission to initialize over it. Keep the scope precise when reporting the test.

## New schema creation and private settings

For a new database, the context generates the creation script, stores workshop settings including private signing material, sets the markers, and commits. On reopening a compatible database, it reads the existing signing material rather than replacing it. A test verifies that the key and exact scaled quantities survive restart. Do not print the actual key in a learning report; equality and length assertions can establish the intended property without disclosing it.

The constructor validates the signing material's representation before accepting it. That check is separate from schema identity. A file with matching markers but missing or malformed required settings can still fail. Matching a version number is not proof that every data invariant is intact. Your recovery plan should preserve the file and inspect the specific failure rather than rewriting markers until startup proceeds.

## A description field requires compatibility work

The proposed department-description feature adds persisted shape. Updating the EF model does not retroactively add a column to every existing database under this constructor. Define whether the workshop will use a fresh disposable database for the exercise or implement a reviewed migration. A fresh database is appropriate for a learning prototype if the report states that existing-data migration was not delivered.

For a migration proposal, identify the source schema, destination schema, conversion steps, transaction strategy, backup artifact and post-checks. Preserve existing department identities, relationships and private settings. Do not regenerate signing material as an incidental side effect of a content-field migration. That would change session behavior for reasons unrelated to the feature.

## Exercise NH-21A: create a compatibility matrix

Include nonexistent path, known compatible file, existing empty file, file with wrong application marker, file with unsupported user version, and file with matching markers but invalid required settings. For each, predict whether creation, reopening or refusal occurs. Run only against uniquely named disposable files whose resolved paths you have checked.

Record the file's hash or size before and after a refused open where preservation is the claim. A thrown exception alone does not prove that no bytes changed. Conversely, a changed temporary locking artifact should not be confused with accepted schema conversion; identify exactly which file or database state you compare.

## Exercise NH-21B: design a reversible rehearsal

Copy a synthetic compatible database to a new rehearsal location. Apply your proposed schema change to that copy, run post-checks, and demonstrate that the original remains unchanged. Define rollback as selecting a known preserved artifact or reversing a supported migration, not guessing SQL statements until the application starts.

The rehearsal should include a department with employees and a lead relationship. A migration that preserves the row count but loses a relationship is not successful. Compare identity-to-field and identity-to-reference mappings. The same preservation principle applies to operation receipts and audit history if your migration touches their tables.

## Exercise NH-21C: write operator guidance

Draft a short message for an incompatible workshop database that explains the next action without encouraging deletion. Name the configured path and the expected compatibility decision, but avoid exposing private settings. A learner should understand whether to choose a new fictional dataset or follow a migration procedure.

Keep automatic reset out of the default path. It may make a demonstration appear convenient while erasing the very work the learner expects to keep. A deliberate reset feature would need explicit scope, preservation options and unmistakable user intent; it is not part of this documentation exercise.

## Review standard

A strong answer treats schema markers as a compatibility check, preserves unknown files, and verifies migration relationships rather than counts alone. Its report distinguishes fresh-database feature testing from an implemented migration and avoids presenting private signing material as ordinary debugging output.
