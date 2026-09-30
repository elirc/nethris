# 09. Reservations and balance accounting

PTO accounting in this workshop distinguishes pending reservations from an approved deduction. Read `Save` and `Decide` in [HrisPto.cs](../../../server/Nethris.Api/Domain/HrisPto.cs), then the balance checks in [HrisPeople.cs](../../../server/Nethris.Api/Domain/HrisPeople.cs). A pending request reserves capacity without immediately reducing the employee's stored balance. Final approval deducts hours. These are separate representations of an obligation at different workflow stages.

## Calculate available capacity

Suppose an employee has forty stored hours and two pending requests for eight and twelve hours. A new sixteen-hour request can fit under the pending-reservation calculation because eight plus twelve plus sixteen is thirty-six. A new twenty-four-hour request would exceed forty. The current request being edited is excluded from the other-request set, so its old hours are not counted alongside its proposed replacement hours.

That exclusion is essential. If an eight-hour request is edited to twelve hours, counting both old eight and new twelve would reserve twenty for one request. Build a fixture where the erroneous double count changes acceptance, rather than using a large balance under which both implementations pass. An independent arithmetic table makes the intended accounting visible.

## Approved hours are not reserved again

The active set includes approved requests for overlap checks, but only pending requests are summed for reservation capacity. After final approval, the stored balance has already been reduced. Adding approved hours to pending reservations would count the same obligation twice. This is why the source uses a status-specific subset for the sum even though a broader set was loaded for another rule.

Use a timeline: balance forty, pending eight, final approval deducts eight to balance thirty-two, then a new pending request for ten. The reservation check compares pending ten against stored thirty-two; it does not subtract the approved eight again. Explain both the stored balance and reserved total at every stage. A single field called available can obscure which quantity is persisted and which is derived.

## Administrative edits must respect reservations

When HR edits an employee's PTO balance, the save operation rejects a candidate below the sum of pending hours. Otherwise an administrative correction could invalidate already accepted reservations. This creates a relationship between employee maintenance and the PTO workflow. A test of the employee form alone may miss the reservation constraint unless the fixture includes pending requests.

Termination is also constrained by pending PTO, direct reports and department leadership. These checks prevent a local status edit from leaving unsupported active relationships. The course is teaching the implemented workshop rules; it does not claim that all real HR systems use the same termination process. A future alternative needs its own transition policy and history treatment.

## Exercise NH-09A: prepare a balance ledger

Write rows for creation, editing a pending request, manager approval, final approval, rejection and cancellation. Record stored balance, pending reserved hours and accepted request status after each. Choose values that expose double counting. For manager approval, the request remains pending at the HR stage, so reservation remains relevant. For final approval, the deduction changes the stored balance and the request leaves the pending subset.

Do not infer that cancellation refunds approved leave. The current remove operation permits only pending requests, and its message describes approved history and its deduction as immutable in the workshop. A refund feature would require a new command and accounting rule. Keep that proposed feature out of the current ledger unless clearly labeled.

## Exercise NH-09B: design a concurrent request test

Two requests each fit against the same initially observed balance but do not fit together. Explain why both must be evaluated under an authoritative transaction boundary. Trace the dispatcher's write transaction and the domain queries. Then design a controlled integration test that starts competing valid requests and checks the final accepted set and balance invariant.

Do not claim a concurrency guarantee from a unit test that invokes the method sequentially with a mocked list. The useful integration evidence includes accepted request count, total pending hours and final stored balance. Record any busy or conflict response as a distinct outcome rather than treating every unsuccessful request as the same failure category.

## Exercise NH-09C: propose a balance-adjustment history

Imagine adding explicit administrative adjustment records instead of directly replacing a balance. Define a reason, signed quantity, actor, accepted instant and operation identity. Decide how an adjustment interacts with pending reservations and whether negative resulting balance is ever allowed. Keep the proposal separate from the current stored-balance model.

Explain how you would reconcile a displayed balance from opening amount, accepted adjustments and approved deductions. A ledger can improve explanation but introduces completeness and migration requirements. You cannot reconstruct all historical adjustments from the current balance alone. Preserve that uncertainty rather than inventing events during migration.

## Review standard

A strong answer keeps stored balance, pending reservation and approved deduction distinct. It handles edited-request exclusion, demonstrates a double-counting counterexample, and tests concurrent acceptance at the actual persistence boundary. The final ledger should make every hour accountable under the declared workshop policy.
