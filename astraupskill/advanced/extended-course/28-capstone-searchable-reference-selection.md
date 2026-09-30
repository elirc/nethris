# 28. Capstone: searchable reference selection

This capstone proposes an improvement to the current bounded allRows reference-loading approach. Build it in a separate practice branch after the pagination, lifecycle and stale-read chapters. The goal is a selector that can search a supported reference collection while preserving the current selected identity and the domain rules for new assignments. It is not enough to replace a select element with a text box that searches the first page.

## User problem and contract

An HR user editing an employee needs to find a department, job role or manager without scanning a long list. The currently selected value must remain understandable even when it is inactive or outside the latest search results. The user should be able to clear an optional manager, retain an allowed existing inactive reference, and choose a valid new reference. The backend remains authoritative for cycles, status and relationship eligibility.

Write separate contracts for search results and selected-value resolution. Search returns a bounded page matching a term and eligibility policy. Selected-value resolution returns the authorized current representation of one identity, or a clear missing/unavailable result. Absence from the search page does not mean the selected record was deleted. This distinction is the capstone's central modeling requirement.

## State model

Represent typed query, applied query if relevant, result page, selected identity, selected display value, loading generation and error separately. Do not store only the selected label. Two managers can share a name, and labels can change. A pending search should not erase an already selected identity merely because results are temporarily empty.

Choose keyboard behavior: opening results, moving among options, selecting, clearing optional values and leaving the control. Use established accessible control semantics appropriate to your implementation and verify the actual interaction. Do not claim accessibility from appearance alone. The learner should be able to complete the selection without a pointer and understand loading or no-result feedback.

## Concurrency and stale responses

Search A can return after search B. Use a relevance mechanism so A cannot replace B's results. Selected-value resolution can also return after the editor has switched to another record or session. Treat that as another ownership boundary. A single global loading boolean is unlikely to describe every independent request accurately.

Consider a department becoming inactive after it appears in search but before save. The server may reject the new assignment. Preserve the form and explain the conflict. Do not promise that filtering choices in the browser guarantees acceptance forever. The selector guides the user under observed state; the mutation checks current authoritative state.

## Exercise NH-28A: specify acceptance examples

Include a selected value outside the first page, an existing inactive department retained during an unrelated edit, an inactive department excluded from new choices, two equal labels with distinct identities, no search matches, a missing selected record, a stale search response and an account switch during loading. State both visible behavior and submitted identity.

The equal-label case should make the display disambiguation usable without changing identity semantics. A department may use a cost center as additional context, while a manager may use another appropriate public field. Choose information already allowed by the response contract and avoid exposing sensitive data merely to distinguish options.

## Exercise NH-28B: build the narrowest vertical slice

Implement one reference kind first, such as departments. Prove search pagination, selected-value preservation, keyboard selection, stale-response suppression and a server-rejected late eligibility change. Then decide which logic is actually shared with job roles and managers. Do not extract a universal selector abstraction before understanding the differing eligibility and labels.

For the server query, retain stable ordering and bounded inputs. For the client, preserve captured mutation semantics after a selection. The selected identity becomes part of the serialized operation body; retry must use that captured value even if the search control later changes in a future interaction.

## Exercise NH-28C: review performance and completeness

Compare initial loading and interaction with the current allRows approach using supported synthetic datasets. Record request count, transferred records and user-visible latency if measured. Also compare correctness: can every valid reference be found, and is the current selected value always represented? A faster control that hides valid choices fails the capstone.

Do not broaden this exercise into unlimited enterprise search. State the workshop's supported bounds and the new selector's behavior at those bounds. If you change a limit, identify the database, API and UI consequences and verify them separately.

## Assessment

Use twenty points each for identity modeling, query correctness, stale-response ownership, accessible interaction and failure recovery. Full credit requires a direct server rejection case after a previously valid choice becomes invalid. The final report should include a state diagram, acceptance matrix, implemented slice and measured or explicitly unmeasured performance comparison.
