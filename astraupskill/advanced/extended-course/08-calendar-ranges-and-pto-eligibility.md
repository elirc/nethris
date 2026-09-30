# 08. Calendar ranges and PTO eligibility

The PTO rules in this workshop deliberately use a simplified calendar model. Read `PtoDates` in [HrisRules.cs](../../../server/Nethris.Api/Domain/HrisRules.cs). The helper accepts a supplied today value, validates positive quarter-hour quantities up to 240, limits the start date to today through one year ahead, requires end on or after start, bounds the inclusive span to thirty-one calendar dates, and permits at most eight hours per calendar date. Weekends are not automatically excluded.

## Inclusive dates differ from elapsed instants

A request starting and ending on the same date spans one calendar date, not zero. The duration expression adds one to the day-number difference before multiplying by eight. A request from Monday through Tuesday spans two dates under this rule. There are no time-of-day instants in the request range, so reasoning in milliseconds or daylight-saving hours would address a different model.

The maximum difference between end and start day numbers is thirty, yielding thirty-one inclusive dates. The independent 240-hour quantity maximum can still be more restrictive than thirty-one times eight. A valid span does not imply every possible quantity over that span is valid. Tests should exercise the date-span and quantity bounds separately.

## Today is explicit in the helper

PtoDates receives today as an argument, making its unit-level behavior deterministic. The calling save operation derives today from UTC time. Separate those facts. A helper test can pass a fixed date and investigate leap years or the one-year boundary without waiting for the clock. A full request test must control or account for the caller's actual date source.

Do not describe the workshop rule as a universal leave policy. Real organizations may use working days, holidays, accrual schedules and local calendars. Those are future design topics if the product requires them. This course teaches precise implementation and testing of the current stated rule, including its explicit weekend limitation.

## Overlap is another boundary

The save operation in [HrisPto.cs](../../../server/Nethris.Api/Domain/HrisPto.cs) checks active requests using inclusive overlap: an existing start on or before the candidate end and an existing end on or after the candidate start. Two requests sharing an endpoint date overlap under that rule. Rejected or cancelled requests are not part of the same active set used for this check.

The overlap rule concerns date ranges, while the balance reservation rule sums pending hours. Approved requests participate in active overlap but their hours are not added again to pending reservations. This difference is essential to understanding the workflow. A single query result can feed two predicates with different subsets; do not summarize both as all open requests count the same way.

## Exercise NH-08A: create a calendar boundary table

Use a fixed today date. Include a start yesterday, today, exactly one year ahead, and beyond one year. Include end before start, same-day end, a thirty-one-date span, and a thirty-two-date span. Pair each with otherwise valid quarter-hour quantities. Then test a same-day request at eight hours and above eight hours.

Choose a leap-year example for the one-year calculation and verify the runtime's DateOnly behavior in your installed environment rather than guessing from a generic 365-day duration. The source uses AddYears, which expresses a calendar operation. Explain how that differs conceptually from adding a fixed number of elapsed milliseconds.

## Exercise NH-08B: draw overlap cases

Draw candidate and existing ranges for disjoint before, disjoint after, touching at one date, contained, containing, and identical ranges. Predict the inclusive predicate for each. Use a small date axis and mark endpoints. This is more reliable than trying to remember a formula without understanding its boundary meaning.

Add an identical range whose existing request is Cancelled, then one whose status is Approved. Explain why status changes whether the request participates in the overlap query. Keep hours and balance valid so the overlap condition is the intended source of rejection.

## Exercise NH-08C: propose working-day support

As an independent design exercise, add a company calendar that excludes weekends and named holidays. Specify whose calendar applies, how calendar changes affect existing requests, and whether already-approved history is recalculated. A helper that counts weekdays is not a complete policy when holiday sets can change after approval.

Write two examples with the same date range but different calendars. Identify what must be stored to reproduce the original accepted decision later. You might store a policy version or captured chargeable units, but explain the tradeoff and compatibility. Do not retrofit the current workshop's stored hours with invented historical calendar data.

## Review standard

A strong answer names inclusive date semantics, separates span from quantity, uses a fixed today in helper tests, and distinguishes overlap membership from balance reservations. The proposed working-day extension should preserve historical meaning and remain visibly separate from the current simplified rules.
