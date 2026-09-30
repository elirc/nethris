# 04. Exact quantities and storage units

Nethris uses decimal domain values for PTO hours and salary amounts, then maps them to integral storage units. Read `QuarterUnits` and `Cents` in [HrisRules.cs](../../../server/Nethris.Api/Domain/HrisRules.cs), followed by the conversions and check constraints in [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs). The important skill is connecting a human-facing quantity, an accepted increment, and its stored representation.

## Quarter-hour units

QuarterUnits accepts a decimal number of hours only when it lies within the caller's bounds and multiplying by four produces an integer. It returns the checked integer unit count. A PTO request uses a positive minimum and a maximum of 240 hours. Employee balance allows zero and has a larger maximum. The same helper therefore supports related quantities without making their allowed ranges identical.

For a worked example, 1.25 hours becomes five quarter units. A value of 1.2 hours becomes 4.8 units and is rejected because the increment is not a quarter hour. A zero request is rejected, while a zero balance can be valid. These cases show why validation needs both increment and context-specific range. Merely converting a decimal to an integer would risk truncating an invalid quantity into a different value.

The database check constraint for employee balance uses stored units, not displayed hours. Its upper bound of forty thousand quarter units corresponds to ten thousand hours. The PTO request bound of nine hundred sixty units corresponds to 240 hours. When reviewing a schema, label the unit beside every numeric bound. Otherwise a maintainer can mistake a correct conversion for a fourfold business-policy error.

## Salary cents

Cents accepts nonnegative decimal amounts up to the configured maximum and rejects values with more than two decimal places. It multiplies by one hundred and converts to a checked long. The job-role operation separately requires the maximum salary to be at least the minimum. Individual amount validity and the relationship between the two amounts are different invariants.

The salary check constraint is written in stored cents. A maximum amount of one hundred million corresponds to ten billion cents. Do not compare that storage constant directly with a displayed amount and conclude the rules disagree. Build a unit conversion table and use it in tests. Exact representation is easier to reason about when the unit is visible in variable names and assertions.

## Round-trip properties and their limits

For an accepted quarter-hour value h, converting to units and back with division by four should recover h. For an accepted currency amount a, converting to cents and back with division by one hundred should recover a. These properties are useful, but only over the accepted domain. A test that generates arbitrary decimals and expects every value to round-trip through a rejecting validator is testing the wrong contract.

Also distinguish precision from policy. Exact cents do not determine whether a salary band is appropriate, and exact quarter hours do not define employment entitlements. This workshop uses simplified product rules for software practice. The course teaches how to enforce those stated rules without presenting them as real-world HR policy.

## Exercise NH-04A: write a quantity table

For PTO request hours, include negative, zero, one quarter, one half, 1.2, 1.25, exactly 240 and above 240. For employee balance, include zero and the larger exact maximum. Record domain acceptance and stored unit count where accepted. Use decimal literals in the C# experiment so the input representation matches the helper's contract.

Then create a deliberately wrong conversion in a disposable practice function that truncates after multiplication. Show which invalid values it incorrectly accepts. Your test should compare against a hand-written expected table, not reuse the wrong conversion to compute its own expected result. This produces a meaningful mutation exercise.

## Exercise NH-04B: test salary relationships

Create valid equal minimum and maximum, an inverted band, a negative endpoint, an endpoint with three decimal places, and an exact maximum endpoint. Identify whether rejection comes from individual amount validation or the band relationship. Keep the other endpoint valid so each test reaches the intended rule.

Add a storage-level fixture that attempts a relationship violation through the database in an isolated test. Explain why a domain test and a database constraint test establish different boundaries. Neither should be described as a substitute for the other when the feature depends on both.

## Exercise NH-04C: review an API change

Suppose a future API accepts minutes instead of hours. Describe the compatibility work: request field meaning, conversion, allowed increments, stored representation, response projection, client display and tests. A field rename alone is insufficient. Decide whether existing clients can continue sending hours and how the server distinguishes versions without guessing from the magnitude.

For example, a value of eight could mean eight minutes or eight hours. Heuristics based on typical values cannot make an ambiguous contract reliable. An explicit field or versioned contract is easier to reason about. Write two request examples that would otherwise be ambiguous and show how your proposal resolves them.

## Review standard

A strong quantity model names units at every boundary, rejects unsupported increments before conversion, and checks relationships separately from individual values. The final evidence should contain exact tables, round-trip tests over accepted values, and at least one counterexample that exposes truncation or unit confusion. These habits transfer to inventory, durations and other bounded decimal domains.
