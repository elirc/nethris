# Prediction lab

Predict each outcome before reading the answer key. These questions isolate the conditions in their prompts. Real requests may be rejected earlier when those assumptions do not hold.

Use Python 3. Copy `answers.template.json` to a new filename in a location you choose, replace each null with your answer, then pass its absolute path to the checker. Copying is a learner action; the checker only reads files and never changes your answers.

From the project root:

```powershell
python astraupskill/advanced/labs/check-answers.py C:/path/to/my-answers.json
```

Use JSON booleans (`true`/`false`), numbers, arrays or objects exactly as requested. Array order matters. Keys in an object may appear in any order. Exit 0 means every prediction matches; exit 1 means one or more answers need review; exit 2 means malformed/missing input.

## Cases

### HRIS-P1

Authenticated valid department update with valid operation key but no required If-Match: expected status in this project?

### HRIS-P2

Authenticated update with a well-formed but stale If-Match and a fresh operation key: expected status in this project?

### HRIS-P3

A first create commits once. Its response is lost. One eligible exact retry replays the retained receipt. How many domain creates occurred in total?

### HRIS-P4

Same account/key as an existing eligible receipt, but changed serialized body: expected conflict status?

### HRIS-P5

A request performs an early domain SaveChanges within an explicit transaction, then receipt insertion fails before commit. Should that transaction commit the domain change?

### HRIS-P6

A proposed server filter has three matches among 30 records and returns the first two matching rows. What should its filtered total be?

### HRIS-P7

May the client supply DepartmentInput headcount to make the read model agree with its UI?

### HRIS-P8

Does a new entity property automatically provide an existing-database migration through HrisDatabase?

## After checking

For each mismatch, name the source function or stated contract that changes your prediction. The checker reports case IDs without printing the answer, so you can make another independent attempt.

Compare with [the answer key](answer-key.json) only after your attempt. The separate chapter answer guide explains the reasoning. This lab grades a mental model and does not compile, start, test or modify the application.

[Return to the advanced course](../README.md)
