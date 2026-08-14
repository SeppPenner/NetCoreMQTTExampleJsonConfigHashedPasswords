# Project rules for Claude

## What this is

NetCoreMQTTExampleJsonConfigHashedPasswords is a runnable example of an MQTT broker built on
[MQTTnet](https://github.com/dotnet/MQTTnet) that takes its users, their hashed passwords and their
topic permissions from a `config.json` file. The repository is a code example, it is **not**
published as a NuGet package and there is no installer: no `GeneratePackageOnBuild`, no push script,
no setup folder. Consumers copy the classes or run the server as it is.

One solution `src/NetCoreMQTTExampleJsonConfigHashedPasswords.sln` with exactly three projects:

- `src/NetCoreMQTTExampleJsonConfigHashedPasswords/NetCoreMQTTExampleJsonConfigHashedPasswords.csproj`,
  `OutputType` `Exe`, the broker itself.
- `src/CreateHashes/CreateHashes.csproj`, `OutputType` `Exe`, a console tool that asks for a password
  and prints its hash for pasting into `config.json`. It references the broker project only to get
  the `User` type for `PasswordHasher<User>`.
- `src/TopicCheckerTest/TopicCheckerTest.csproj`, MSTest. The name is older than its content, it
  covers the topic checker, the configuration and the date extensions.

Layout inside `src/NetCoreMQTTExampleJsonConfigHashedPasswords`:

- `Program.cs`: everything that runs. `Main` loads the certificate, sets up Serilog, reads the
  configuration and starts the server. `ValidateConnectionAsync` is the authentication,
  `InterceptSubscriptionAsync` and `InterceptApplicationMessagePublishAsync` are the authorization,
  `ClientDisconnectedAsync` frees the client identifier. The private helpers `GetClientIdPrefix`,
  `IsUserThrottled`, `ReadConfiguration` and the three `LogMessage` overloads do one thing each,
  keep new logic in that shape.
- `Config.cs`, `User.cs`, `TopicTuple.cs`: the shape of `config.json`, nothing else.
- `TopicChecker.cs`: turns an allowed topic into a regular expression and matches a topic against
  it. This is the only class with its own tests from the beginning.
- `DateTimeExtensions.cs` and `DateTimeOffsetExtensions.cs`: `EndOfCurrentMonth` for the expiration
  of the data limit cache, and the local time zone offset it needs.
- `GlobalUsings.cs`: all usings of the project.
- `config.json`, `certificate.pfx`, `cert.pem`, `key.pem`: the example configuration and the example
  certificate. `config.json` and `certificate.pfx` are copied to the output directory on every build.

Layout inside `src/TopicCheckerTest`:

- `TopicCheckerTestsPlusOperator.cs`, `TopicCheckerTestsCrossOperator.cs`,
  `TopicCheckerTestsMultipleOperators.cs`, `TopicCheckerTestsWithoutOperators.cs`: the topic checker,
  split by the operator under test.
- `ConfigTests.cs`: the deserialization of the shipped `config.json`, including the proof that the
  documented password verifies against the shipped hashes and that the client identifier validation
  is on by default.
- `DateTimeOffsetExtensionsTests.cs`: the end of the month and the local time zone offset.
- `GlobalUsings.cs`: all usings of the test project.
- The `config.json` of the broker project is linked into this project and copied to the output
  directory, so the tests read the file that is actually shipped. Do not copy the file, the point is
  that both sides cannot drift apart.

Repository root: `README.md` (the only user documentation, with the JSON samples and the table of
supported topic characters), `Changelog.md`, `License.txt` (MIT), `.gitattributes` and `.gitignore`.
There is no `Updating.md`, no `HowToUse.md`, no `.github` folder and no pipeline file.

## Build

```powershell
dotnet build src/NetCoreMQTTExampleJsonConfigHashedPasswords.sln
```

```powershell
dotnet test src/NetCoreMQTTExampleJsonConfigHashedPasswords.sln
```

- Single target framework `net10.0` in all three projects, no multi-targeting, no
  `RuntimeIdentifiers`. Nothing in the code is Windows specific.
- All build properties live directly in the three `.csproj` files and are duplicated there. There is
  **no** `Directory.Build.props` in this repository.
- `TreatWarningsAsErrors` is enabled in all projects, so every warning breaks the build, NuGet
  warnings (`NU****`) from restore included. A clean build reports zero warnings, keep it that way.
- `NU1803` (HTTP source usage during restore) is the one warning suppressed via `NoWarn`. Fix
  warnings instead of extending that list. `NuGetAudit` and `NuGetAuditMode=all` are on, so a
  vulnerable transitive package fails the build too.
- Obsolete API warnings are the usual reason the build breaks after a framework move. Version
  1.0.12.0 shipped a `master` that no longer compiled at all, because
  `new X509Certificate2(path, password, flags)` became `SYSLIB0057`. The certificate is loaded with
  `X509CertificateLoader.LoadPkcs12FromFile` since then.
- Versions come from GitVersion.MsBuild out of the git tags, for example `1.0.13-1` for the first
  commit after tag `1.0.13`. Never edit a version property or an assembly version by hand.
- Restore needs nuget.org. If a private feed is configured globally on the machine and answers 404
  for public packages, restore fails with `NU1301`. Then build with an explicit source:
  `dotnet build src/NetCoreMQTTExampleJsonConfigHashedPasswords.sln --source https://api.nuget.org/v3/index.json`.
- Tests are MSTest, in the single test project `src/TopicCheckerTest`, which follows the same package
  set as the sibling repositories: `Microsoft.NET.Test.Sdk`, `MSTest.TestAdapter`,
  `MSTest.TestFramework`, `coverlet.collector` and `GitVersion.MsBuild`. `dotnet test` runs 29 tests,
  they need no network, no broker and no fixture outside the repository. Never claim a test run
  happened without running it.
- Beyond the tests, a behaviour change in `Program.cs` is only proven by a real client against a
  running broker. Build `Debug` (that is the configuration with the unencrypted endpoint), start the
  executable, connect with an MQTT client on port 1883 and read
  `bin/Debug/net10.0/log/NetCoreMQTTExampleJsonConfigHashedPasswords_<date>.txt`. The log is the only
  place that tells the truth about subscription and publish decisions, see the quirk about the
  acknowledgement codes below.

## Code conventions

Follow the surrounding code, it is consistent throughout every file:

- File header comment block with `<copyright file="..." company="Hämmer Electronics">` and a
  `<summary>`, then the file-scoped namespace.
- XML doc comments on every type and every member, private members included, no exceptions. There is
  no `GenerateDocumentationFile`, so nothing enforces this but the convention.
- `Nullable`, `ImplicitUsings` and `LangVersion latest` are enabled.
- New `using` directives go into the `GlobalUsings.cs` of the respective project, inside the existing
  `#pragma warning disable IDE0065` block, never at the top of a file. The editorconfig requires
  usings inside the namespace (`csharp_using_directive_placement=inside_namespace:warning`), which
  global usings cannot satisfy, that is what the pragma is for. Do not add other pragmas except the
  `Serilog004` ones that wrap the two log calls with a conditional message template. The comment text
  in that block is German because Visual Studio generated it, leave it alone.
- Fields, properties, methods and events are always accessed with `this.` qualification
  (`dotnet_style_qualification_for_*` at severity `warning`). `Program` is static throughout, so the
  broker project has almost no place where this shows.
- `src/.editorconfig` also enforces braces everywhere, no multiple blank lines, four spaces, CRLF,
  UTF-8, file scoped namespaces, `System` usings sorted first and `IDE0005` as warning. Analyzer
  warnings are fixed, not silenced.

## Known quirks

Do not silently "clean up" these, they are existing behaviour:

- **The unencrypted endpoint exists only in Debug.** `Main` has an `#if DEBUG` that adds
  `WithDefaultEndpoint().WithDefaultEndpointPort(1883)`. A Release build listens on the encrypted
  endpoint only, on the port from `config.json`, with `SslProtocols.Tls12` fixed. That is why every
  manual test runs against a Debug build.
- **`Console.ReadLine` is the whole lifetime of the process.** Started without a console or with a
  closed standard input the call returns immediately, the broker stops a moment after it started.
  When starting the executable from a script, keep the console of the calling process alive for as
  long as the broker is needed.
- **The example certificate is committed.** `certificate.pfx`, `cert.pem` and `key.pem` are tracked
  and their password is `test`, documented in the readme. `Main` exports the certificate to bytes
  without a password and hands those to MQTTnet. This is an example, not a template for production.
- **The acknowledgement codes do not carry the decision.** The interceptors only set
  `ProcessSubscription` and `ProcessPublish`. A client that subscribes to a blacklisted topic still
  receives `GrantedQoS0`, a publish to a blacklisted topic still gets a success reason code. The
  subscription really is dropped and the message really is not published, the log lines
  `Subscription failed for clientId = ...` and the missing `Message: ...` line are the proof. If this
  should ever change, it changes what clients see on the wire, so it is a decision, not a cleanup.
- **Blacklist beats whitelist, exact match beats pattern.** Both interceptors first look for the
  topic in the blacklist with `Contains`, then in the whitelist, and only then walk both lists again
  with `TopicChecker.Regex`. A topic that matches nothing at all is rejected.
- **`TopicChecker.Regex` only knows the characters from the readme table.** `+` and `#` are replaced
  by long explicit character classes that end at `U+00FF`, everything outside them never matches. The
  regular expression is not anchored, which is why the result is compared against the whole topic
  afterwards. The two crosses rules count the crosses in the **topic**, not in the allowed topic.
- **`args.UserName != currentUser.UserName` is dead code.** The user was looked up by exactly that
  name one line above. The check is harmless and was left in place.
- **Throttling counts published bytes only.** `IsUserThrottled` runs for a user with `ThrottleUser`
  and a `MonthlyByteLimit`, keys `MemoryCache.Default` by client identifier and lets the entry expire
  at the end of the current month. Subscriptions and received messages are not counted, and an
  overflow of the running total locks the client for the rest of the month.
- **`ClientIdPrefixesUsed` is never cleaned up.** A prefix stays in that list until the process ends,
  and `GetClientIdPrefix` returns the first prefix a client identifier starts with. With two prefixes
  where one is the beginning of the other, the first one added wins.
- **The session items key differs per user type.** A user without a prefix is stored under its client
  identifier, a user with a prefix under the prefix. Both interceptors resolve the user the same way,
  so both sides have to be changed together.
- **The shipped hashes are older than the current hasher.** They carry format marker 1 with 10000
  iterations, so `VerifyHashedPassword` answers `SuccessRehashNeeded` and not `Success`. The
  connection validation only rejects `Failed`, which is why both results let the user in. A test
  pins this, so a package update that changes the verification result is noticed.
- **`Microsoft.AspNetCore.Identity.UI` is referenced for a single class.** Both executables use
  nothing but `PasswordHasher<TUser>`, which lives in `Microsoft.Extensions.Identity.Core`. The
  heavier package has been referenced since the beginning, so the hashes stay comparable across
  versions.
- **The version 1.0.13.0 fixes are easy to undo by accident.** Four of them look like harmless
  simplifications:
  - `User.Password` must **not** get a `[JsonIgnore]` back. With it the hash is dropped while reading
    `config.json`, and then no user can log in at all. That was the state from 1.0.0.0 to 1.0.12.0.
  - `User.ValidateClientId` defaults to `true` on purpose. The flag was introduced as a way out for a
    user that may use every client identifier, so the C# default `false` silently switched off the
    client identifier validation for everybody.
  - The client identifier prefix is checked with `StartsWith` in `ValidateConnectionAsync`. Without
    that check a prefix user is accepted with any client identifier at all.
  - `clientIds` is filled on the successful connection paths. It used to be queried and cleared, but
    never filled, so the duplicate client identifier check never triggered.
- **The connection log never contains a password.** `LogMessage(ValidatingConnectionEventArgs, bool)`
  writes `New connection: ...` or `Connection failed for clientId = ...`. It used to write the
  plaintext password of every rejected attempt into a file next to the executable, which is a strange
  thing to do in an example about hashed passwords.
- **AppVeyor badge without CI in the repository.** `README.md` links an AppVeyor build that is
  configured outside of this repository. There is no `.github` folder and no pipeline file here.
- **`src/NetCoreMQTTExampleJsonConfigHashedPasswords.sln.DotSettings`** is tracked and holds nothing
  but a ReSharper user dictionary. Leave it alone.
- **`.gitattributes` sets `* text=auto`** and every rule of the Visual Studio template below it is
  commented out. `certificate.pfx` is binary and gets away with it because git detects it, a further
  binary file is better off with its own rule.

## Releasing

1. Make the change.
2. Add an entry at the top of `Changelog.md` in the existing format:
   `* **Version 1.0.13.0 (2026-08-14)** : Short description.`
3. Commit that.
4. Tag the commit with the plain version number, no `v` prefix (`1.0.13`, `1.0.12`, ...). The
   existing tags are lightweight tags, create new ones the same way.
5. Push the commits and the tag.

The version in the `Changelog.md` has four parts (`1.0.13.0`), the tag has three (`1.0.13`).
GitVersion turns the tag into the assembly version, so an untagged commit produces something like
`1.0.13-1+Branch.master.Sha...`. There is no installer to build and no package to push, so the
release ends with the push.

## Git

- **Never amend a commit.** No `git commit --amend`, not for a typo in the message, not to add a
  forgotten file, not even when the commit is still local. Write a follow-up commit instead. The
  release versions come from tags on exact commits, an amended commit leaves its tag pointing at a
  commit that no longer exists in the branch.

## Writing style

- Commit messages are written **in English only**: short, precise subject line, explanatory body
  when needed.
- Code comments and comments in project files such as `.csproj` are **always English**, regardless
  of the language used in the conversation.
- **No em dashes or en dashes** (`—`, `–`), neither in prose, commit messages, code comments nor
  documentation. Use a regular hyphen, comma, colon, parentheses or a separate sentence.
- German texts (documentation, chat replies) always use real umlauts and ß, never ASCII
  transliterations such as `ae`, `oe`, `ue` or `ss`. Identifiers, file names and configuration keys
  stay unchanged where umlauts are technically undesirable.
