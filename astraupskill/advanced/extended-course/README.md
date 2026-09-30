# Nethris engineering fieldbook

Study the actual local HRIS workshop across request contracts, normalization, exact quantities, manager graphs, reference lifecycles, visibility, PTO accounting, approval authority, sessions, preconditions, atomic receipts, client recovery, schema compatibility and verification.

The [earlier advanced route](../README.md) is a shorter introduction. Choose a focused path in [assessment and portfolio](32-assessment-routes-and-portfolio.md), or follow the chapters in order. Use fictional employee data and a disposable learning database. This is software-engineering practice using the workshop's stated rules.

Current behavior is linked to source. Department descriptions, employment-status filtering and searchable selectors are proposed learner features until implemented and verified separately. Write predictions before opening the reviews. [Hints](hints-and-investigation-prompts.md) provide intermediate help.

## Chapters

- [01. Follow one employee change](01-follow-one-employee-change.md)
- [02. Contracts connect shape with meaning](02-contracts-shape-and-meaning.md)
- [03. Text normalization and identity](03-text-normalization-and-identity.md)
- [04. Exact quantities and storage units](04-exact-quantities-and-storage-units.md)
- [05. Manager graphs and global invariants](05-manager-graphs-and-global-invariants.md)
- [06. Reference lifecycles and safe deletion](06-reference-lifecycles-and-safe-deletion.md)
- [07. Projections and field-level visibility](07-projections-and-field-level-visibility.md)
- [08. Calendar ranges and PTO eligibility](08-calendar-ranges-and-pto-eligibility.md)
- [09. Reservations and balance accounting](09-reservations-and-balance-accounting.md)
- [10. Approval workflows and authority](10-approval-workflows-and-authority.md)
- [11. Sessions and current account state](11-sessions-and-current-account-state.md)
- [12. ETags and stale-edit recovery](12-etags-and-stale-edit-recovery.md)
- [13. Operation receipts and exact replay](13-operation-receipts-and-exact-replay.md)
- [14. Transactions and late failure](14-transactions-and-late-failure.md)
- [15. Captured operations and client recovery](15-captured-operations-and-client-recovery.md)
- [16. Generations and stale-read suppression](16-generations-and-stale-read-suppression.md)
- [17. Pages, search, and complete selectors](17-pages-search-and-complete-selectors.md)
- [18. Forms and domain feedback](18-forms-and-domain-feedback.md)
- [19. Feature workshop: department description](19-feature-workshop-department-description.md)
- [20. Feature workshop: employment-status filter](20-feature-workshop-employment-filter.md)
- [21. Schema identity and migration rehearsal](21-schema-identity-and-migration-rehearsal.md)
- [22. Tests at the right boundary](22-tests-at-the-right-boundary.md)
- [23. Fault injection and response uncertainty](23-fault-injection-and-response-uncertainty.md)
- [24. Audit trails and evidence quality](24-audit-trails-and-evidence-quality.md)
- [25. HTTP input budgets and errors](25-http-input-budgets-and-errors.md)
- [26. Bounded scale and performance](26-bounded-scale-and-performance.md)
- [27. Packaging and reproducible verification](27-packaging-and-reproducible-verification.md)
- [28. Capstone: searchable reference selection](28-capstone-searchable-reference-selection.md)
- [29. Capstone: PTO recovery investigation](29-capstone-pto-recovery-investigation.md)
- [30. Refactoring with behavioral boundaries](30-refactoring-with-behavioral-boundaries.md)
- [31. Incident review clinic](31-incident-review-clinic.md)
- [32. Assessment routes and portfolio](32-assessment-routes-and-portfolio.md)
- [33. Organizational change rehearsal](33-organizational-change-rehearsal.md)

## Separate review guides

- [Review guide 1: domain foundations](review-01-domain-foundations.md)
- [Review guide 2: workflows and recovery](review-02-workflows-and-recovery.md)
- [Review guide 3: features and verification](review-03-features-and-verification.md)
- [Review guide 4: delivery and investigations](review-04-delivery-and-investigations.md)

## Evidence standard

Distinguish tracked state, SaveChanges, transaction commit and response delivery. Record actor, starting state, operation identity, prediction, observation and remaining uncertainty. Protect existing learner work and keep proposed feature experiments separate from ordinary workshop data.

Source links and instructional counts are checked. Commands and capstone procedures are reproduction instructions unless a verification record explicitly reports execution. The organizer's `reportsfile/tenfold/nethris.json` contains the exact local delivery and verification scope.
