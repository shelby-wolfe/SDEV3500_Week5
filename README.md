# SDEV 3500 — Week 5 individual test repair

GearTrack manages equipment reservations, checkout, return, and activity history. Your task is to diagnose and improve this starter's automated tests using xUnit facts/theories, reusable setup, test doubles, invariants, isolation, and maintainable assertions. A green run is not sufficient evidence that a suite protects its contract.

## Provenance

Original repository: https://github.com/uwyo-soc-sdev/geartrack

Fixed canonical baseline: `42a80cfaa3303bb2a9017aa61304c5a094fb834c`.

This repository is an exported snapshot with independently authored Week 5 tests. Two approved service guards were added after that baseline: reversed calendar dates and maintenance equipment are rejected. Checkout/return production behavior is unchanged. The test suite is the repair target.

## Setup and execution

Install the .NET 10 SDK. No credentials, external database, Docker, or running web application are required for tests. Clone **your assigned repository**, not the canonical repository:

```sh
git clone <your-assigned-repository-url>
cd <your-assigned-repository-directory>
dotnet --version
dotnet restore
dotnet build --no-restore
dotnet test
```

The supplied starter has 12 test methods and 15 discovered cases. The expected baseline is all green. Record your own baseline before editing. Optional local tooling is pinned in `.config/dotnet-tools.json`; `dotnet tool restore` installs it, but it is not required for `dotnet test`.

To run a selected test, substitute its fully qualified class and method name:

```sh
dotnet test --filter "FullyQualifiedName~GearTrack.Core.Tests.ReservationTests.Create_handles_date_boundaries"
```

Optional application launch: `dotnet run --project src/GearTrack.Web`. The application creates its own local SQLite database; automated tests must use test-owned data and must not use that database.

## Approved behavior to protect

- Creation requires existing equipment and a nonblank requester name and email. Names and emails are trimmed; blank purpose defaults to `General course use`.
- Compare calendar dates: reject `EndDate.Date < StartDate.Date`; allow equal dates, even when the end time is earlier on that day. Store normalized dates.
- Reject creation for equipment currently in `Maintenance`. Do not introduce an additional creation restriction for `CheckedOut` equipment.
- Creation succeeds with a `Reserved` reservation and a linked `Reserved` activity. It does not change equipment status.
- Checkout succeeds only from `Reserved`: reservation and equipment become `CheckedOut`, the checkout timestamp is assigned, and a linked `Checked out` activity is recorded.
- Return succeeds only from `CheckedOut`: reservation becomes `Completed`, equipment becomes `Available`, the return timestamp is assigned, and a linked `Returned` activity is recorded.
- Missing reservations and invalid transitions fail. Rejection preserves existing reservation/equipment state and creates no reservation, activity, or save effect. Repeated operations follow the same transition rules.
- Successful operations save their intended changes. Activity records identify the correct reservation and equipment. A fake's observations and actual SQLite persistence are different kinds of evidence.
- Feedback must communicate the outcome; exact sentence wording is not a fixed contract. Exact wall-clock timestamps are not required.

Overlap policy, concurrency, atomic multi-save recovery, full email syntax validation, past-date restrictions, and new checkout availability rules are outside this assignment. Preserve the approved behavior; do not redesign the application.

## Work and deliverables

Work individually. Author your own tests, implementation choices, and explanation. Do not copy another student's code or submit a shared repair.

1. Record the baseline command, revision, and results. Inspect tests and supporting doubles/setup before editing.
2. Repair weaknesses in boundary/state coverage, assertions and invariants, setup ownership, doubles, persistence evidence, and test maintainability. Use facts and theories intentionally. Keep the suite compact and readable, with focused lifecycle unit tests and one intentional SQLite lifecycle component scenario.
3. Demonstrate that repaired tests work individually and together, without relying on execution order. Keep databases and mutable test state owned by the tests that use them.
4. Complete `docs/test-review.md` with diagnosis, reasoning, evidence, and remaining limitations. Explain what your doubles prove and what your component test proves. Include one concrete example of a behavior error your repaired assertions would detect that the starter could miss.

Do not remove meaningful coverage, skip tests, disable all parallel execution, or weaken product rules to obtain green results. Production changes are not expected; raise a suspected contract conflict in your PR instead.

## Required branch and pull-request workflow

**Never push to the canonical GearTrack repository.** Verify that `origin` is your assigned repository before pushing.

```sh
git remote -v
git switch -c week5-test-repair
# Make and verify your changes.
git add tests docs/test-review.md
git commit -m "Repair Week 5 test suite"
git push -u origin week5-test-repair
```

Open a pull request from `week5-test-repair` into `main` **within your assigned repository**. Leave it open for review; do not merge unless instructed. Include a summary of repaired problems and exact validation commands/results.

Submit the PR URL, final commit SHA, repaired test/support files, and completed review worksheet. Include evidence of both full-suite and individual runs, with pass/fail/skip counts. Follow the course submission channel and deadline provided by your instructor.
