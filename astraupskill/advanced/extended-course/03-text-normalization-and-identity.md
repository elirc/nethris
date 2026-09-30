# 03. Text normalization and identity

The actual text helpers in [HrisRules.cs](../../../server/Nethris.Api/Domain/HrisRules.cs) are compact, but their order of operations matters. `Text` trims first, then rejects empty content, excessive normalized length, or remaining control characters. `OptionalText` maps null or whitespace-only input to null; otherwise it delegates to Text. This means surrounding whitespace controls removed by trimming can be accepted while embedded control characters are rejected.

## Build examples that reveal the order

Compare a tab before and after an ordinary word with a tab inside the word. The surrounding tabs can disappear during trimming. The embedded tab remains and is rejected by the control-character rule. A statement saying all tabs are rejected is therefore too broad. A statement saying controls are allowed is also wrong. The accurate contract names normalization before validation and distinguishes position.

Likewise, a long raw string of surrounding spaces plus a short word can normalize to a value within the maximum. The helper bounds normalized length, unlike a helper that bounds raw input first. The HTTP body limit still bounds the entire request, so field normalization does not permit arbitrarily large network input. These are separate resource and semantic boundaries.

## Optional text has three common input forms

Null, an empty string, and whitespace-only text all become null under OptionalText. Ordinary text is trimmed and validated. This is useful for fields such as phone, location, cost center and decision comment, each with its own maximum supplied by the caller. It does not imply that every field in the application is optional or shares the same bound. Read the call sites to identify field-specific limits.

A proposed department description can reuse OptionalText if that behavior matches the product. The assignment should state the expected accepted representation for each input, including surrounding newline or tab characters. Reusing a helper without understanding its behavior can make tests contradict the implementation and create confusing documentation.

## Email normalization has additional meaning

The email helper trims and bounds text, lowercases invariantly, uses MailAddress parsing, requires the parsed address to equal the normalized input, and requires a dot. This is the workshop's acceptance rule. It is not a deliverability check or a universal description of every address format accepted by every mail system. Teach the actual rule and its tests rather than claiming complete email validation.

The database model also declares uniqueness and case-insensitive collation for employee and account email. Employee numbers receive a uniqueness rule and case-insensitive collation too. Application prechecks can provide helpful conflict messages, while database constraints protect accepted storage. Explain how these layers support one another without assuming a precheck alone resolves concurrent insertion.

## Exercise NH-03A: create a normalization ledger

For Text with a small chosen maximum, list null, empty, spaces, ordinary text, surrounding tabs, embedded tab, surrounding newlines, embedded newline, normalized exact limit, and normalized one over. Record accepted value or rejection. Repeat for OptionalText and mark only the rows that change. This makes the optionality distinction visible without duplicating an entire test suite blindly.

When executing the exercise, use escaped representations or code-point inspection so invisible whitespace is evident. A screenshot of two visually identical fields may not establish which characters were supplied. Your evidence should show input representation and accepted representation separately.

## Exercise NH-03B: investigate duplicate identity text

Create two employee candidates whose emails differ only in casing or surrounding spaces. Predict the normalized values and the uniqueness outcome in a disposable database. Then choose employee numbers with case differences and inspect the database comparison rule. Do not assume all other strings use the same collation or normalization merely because these identity-like fields do.

Add a control case with genuinely different normalized values. A test that only proves one duplicate is rejected can be accidentally satisfied by a setup error that rejects every create. The successful control establishes that the conflict condition, rather than a broken fixture, explains the negative result.

## Exercise NH-03C: review a proposed sanitizer

Someone proposes removing every control character from every request string before domain validation. Explain how that changes the contract. An embedded control currently produces a rejection; silently deleting it could turn malformed input into a different accepted value. Two distinct inputs might normalize to the same identity, and the learner would not know what changed.

Recommend a policy for the proposed description field using the existing helper's semantics or a deliberately different helper. If you choose different behavior, give it a clear name and document why. Avoid a generic sanitizer whose transformations are invisible to callers. Validation should preserve the ability to explain why a value was accepted or rejected.

## Review standard

A strong answer predicts exact accepted strings and identifies the caller-specific maximum. It distinguishes normalized length from request byte size and syntax acceptance from deliverability. It also recognizes that normalization affects uniqueness and compatibility. These details are small enough to test directly and important enough to prevent recurring form/import disagreements.
