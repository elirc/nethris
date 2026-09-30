# Capstone: deliver a department feature another learner can maintain

Deliver HRIS-01 or HRIS-02 end to end. Choose one. You may combine them only after the first meets its contract. The capstone is about owning behavior across boundaries, not increasing the number of fields.

## Before implementation

Create a source map and a contract table. Record the exact baseline you are extending, the practice database location and the tools needed to run the workshop. Keep a copy of any imported fictional data used by the test. Explain how another learner starts with equivalent data without access to your existing environment.

## First demonstration

Show a valid creation/read/edit sequence for descriptions, or a paged query with several status groups for filtering. Include a deliberately awkward input: whitespace-only description, an invalid status or equal employee names. State the expected normalization and ordering before running the action.

## Rejected-path demonstration

Use two independently captured versions of a department when testing edits. Make one stale intentionally. Show the latest accepted data and the losing browser draft. For filtering, change the query while an earlier request is still pending and prove that obsolete results do not become the current list.

Do not assert “everything rolled back” from a toast message. Show the relevant database rows, receipt/audit behavior or HTTP scenario assertions. Conversely, do not claim that a database test proves keyboard focus or draft visibility; use a browser observation/check for that layer.

## Recovery demonstration

Explain what happens if a write succeeds and its response is unreadable. Capture the exact operation and retry it under the existing contract. Then demonstrate that editing the body requires a new operation decision. If your extension changes schema, include the fresh-database workflow and state whether existing-data migration was implemented, designed only or deliberately deferred.

## Handoff packet

Provide a concise problem statement, changed file list, setup, fictional fixture, commands, observed outcomes and known limits. Include one reviewer-selected case that was not in your initial happy-path demo. A strong handoff says which contract the test establishes and which neighboring concern remains untested.

Finish with a short retrospective: which value crossed the most layers; which rejection initially behaved unexpectedly; and what would change if the application served multiple organizations or a much larger employee directory. Those are future design questions, not capabilities established by this local workshop.

[Main learning course](../README.md) · [Advanced index](README.md)
