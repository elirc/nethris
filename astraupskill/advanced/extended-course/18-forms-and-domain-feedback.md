# 18. Forms and domain feedback

The Workshop component describes fields through a small metadata structure and converts its draft into request bodies. Read `fields`, `open`, `submit`, and `fieldInput` in [Workshop.tsx](../../../client/src/Workshop.tsx). The form is a user-facing guide to domain input, but it is not the authoritative source of every rule. Browser constraints, conversion, request parsing and domain validation each have a role.

## Keep raw input until a decision is made

The draft stores text for numbers and dates, booleans for checkboxes, and selected identities for reference fields. Submit converts numeric text using Number and maps empty optional values to null. An empty or unusual numeric string can have language-level conversion behavior that differs from the domain's accepted range. The server's quantity checks remain necessary even when an input has min, max and step attributes.

A form test should therefore cover both ordinary browser feedback and direct server rejection. A crafted request can bypass client constraints. Conversely, server rejection alone does not prove that the form preserves text and explains the issue. The complete user journey includes a correction and successful resubmission after a known rejection.

## Existing rows determine defaults and choices

Opening an editor initializes fields from the row or from field-type defaults. Account password starts blank even for an existing row. A blank optional new-password field maps to null, meaning retain the existing password under the current account-edit contract. This differs from sending an empty string as a proposed password, which would fail the password rule if it reached hashing validation.

The department and job-role options include active records plus the employee's existing selected record. Manager options exclude the employee themself and terminated employees, but the server still checks longer cycles and eligibility. The interface can prevent obvious invalid choices without attempting to duplicate every graph invariant locally.

## Rejected forms are valuable state

On known validation or domain rejection, the client clears pending operation recovery but retains the editor so the learner can correct the draft. On uncertain outcomes, it keeps the captured operation and locks ordinary edits. On stale comparison, it preserves draft and current saved representation separately. These are distinct recovery states, not interchangeable error banners.

An error message should tell the learner which action is possible next. A duplicate name can be corrected. A stale row needs comparison. A missing reference may require refresh or another selection. An uncertain save needs exact retry or deliberate inspection. Generic please try again guidance can be actively confusing when the correct next step differs by category.

## Exercise NH-18A: trace field conversion

Choose employee balance, optional phone, active checkbox, manager selection and account new password. For each, record draft representation, submitted representation and server rule. Include blank and boundary inputs. Explain why a TypeScript Row index signature does not validate the runtime response or establish every field's type.

Use a table rather than a broad statement that the form converts values. The conversion is field-type dependent and action-specific. A PTO decision replaces the ordinary entity body with decision and comment, while cancellation uses an empty object. A deletion omits its body. These details matter to exact fingerprint replay.

## Exercise NH-18B: build a correction journey

Submit a department name that conflicts with another accepted department. Verify the editor remains, correct the name, and submit a new operation. Record that the new intent receives a new key rather than reusing the rejected request's key with changed bytes. Then test a stale version separately and demonstrate the comparison workflow.

Your evidence should include the field values after rejection and the absence of unintended accepted changes. A screenshot of an alert alone cannot prove either. A component test can inspect retained controls, while an integration or browser test can verify accepted state through the real boundary.

## Exercise NH-18C: inspect accessibility during recovery

Navigate the form and recovery controls with a keyboard. Identify labels, focus position after an error, and whether the learner can reach comparison values and retry actions. The markup includes labels and status/alert roles, but those facts do not establish every aspect of usability. Record the actual observed sequence.

Propose one improvement with a concrete acceptance example. For instance, after a stale conflict the comparison heading could receive focus if that helps the learner understand why fields are disabled. Keep the proposal scoped and testable. Do not rewrite unrelated styling to satisfy a behavior that can be improved locally.

## Review standard

A strong answer links field metadata to request representation and domain rules while preserving their separate responsibilities. It demonstrates a full correction journey, exact retry ownership and accessible recovery. The final artifact should explain what the learner sees and which server evidence confirms that rejected input did not become accepted data.
