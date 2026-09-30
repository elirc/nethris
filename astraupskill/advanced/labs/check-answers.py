"""Grade prediction answers only. No application, database or network access."""
import argparse, json
from pathlib import Path

def unique_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result: raise ValueError("Duplicate JSON key: " + key)
        result[key] = value
    return result

def read(path):
    if path.stat().st_size > 100_000: raise ValueError("Answer file exceeds 100,000 bytes")
    return json.loads(path.read_text(encoding="utf-8-sig"), object_pairs_hook=unique_object)

def grade(answers, key):
    if not isinstance(answers, dict): raise ValueError("Answers must be a JSON object")
    if set(answers) != set(key): raise ValueError("Answer IDs must exactly match the worksheet")
    canonical = lambda value: json.dumps(value, sort_keys=True, separators=(",", ":"))
    return [case for case in key if canonical(answers[case]) != canonical(key[case])]

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("answers", type=Path)
    args = parser.parse_args()
    try:
        key = read(Path(__file__).with_name("answer-key.json"))
        wrong = grade(read(args.answers), key)
    except (OSError, ValueError) as error:
        print("Worksheet input rejected:", error)
        return 2
    print(f"{len(key)-len(wrong)}/{len(key)} predictions match the documented contract.")
    if wrong: print("Review these case IDs:", ", ".join(wrong))
    print("This checks predictions; it does not run or verify the application.")
    return 1 if wrong else 0

if __name__ == "__main__": raise SystemExit(main())
