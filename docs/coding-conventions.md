# Coding conventions

**Status: draft (2026-11-01).** Applies to project code in `Assets/Scripts/`. SDK samples and imported art under `Assets/` are not ours. Don't edit them.

**How to apply:** follow these rules in **new code** and in **the lines you change** for a backlog item. Don't rewrite whole old files just for style. That makes diffs hard to review and risks breaking Inspector references. When an item already touches an old script, bring the touched parts up to these rules.

The reference for "good" is `Assets/Scripts/PiecesContainerCounter.cs`, apart from its hard-coded numbers and English text (§5, §6), its commented-out code and its unused `IsAcceptedType` (§8).

Rules marked **(pending decision)** wait for an architecture decision in `docs/decisions/`. Until then, follow the interim rule given.

## 1. Files and naming
- One public class per file. **File name = class name.** For a new MonoBehaviour this is required: otherwise Unity can't add the script to a GameObject. Old exceptions: `MixPickingContainer.cs` / `MixPickingValidator.cs` hold `MixedPicking*` classes. Keep them as they are. Renaming means moving the `.cs` and its `.meta` together, see §9.
- `PascalCase`: classes, methods, properties, enums and enum values.
- `camelCase`: fields and locals. No `m_` or `_` prefixes, to match the existing code.
- Names say what the trainee or the domain sees: `requiredPieces`, `ConfirmOrder()`. Avoid `calc`, `temp`, `data2`.
- WMS field names stay German in identifiers when they name a WMS field (`sollText`, `istText`, `restText`). Everything else in English.
- New scripts go in `Assets/Scripts/` (or a subfolder named after the feature, without spaces). Don't add to `WS Scripts/`.

## 2. MonoBehaviour fields and the Inspector
- Inspector values are `[SerializeField] private`, not `public`. Expose read-only state with a property: `public int GGACount { get; private set; }`.
- Changing an **existing** `public` field to `[SerializeField] private` with the same name keeps its Inspector values. But it breaks compilation if other scripts read the field: e.g. `PackageData.packageType`, `CalcForWMS.currentSize`, `BoxInfo.packageSize`. Grep for readers first, and add a read-only property for them.
- Group fields with `[Header("…")]`. Add a `[Tooltip("…")]` when the meaning or unit isn't obvious (pieces vs packages).
- Use `TMP_Text` for text references, not `TextMeshProUGUI`. `TMP_Text` also accepts world-space `TextMeshPro`.
- **Never rename a serialized field without `[FormerlySerializedAs("oldName")]`.** Never rename a public method that a UnityEvent (button) calls without checking with the `scene-ref-auditor` agent first.
- Public methods meant for buttons or UnityEvents are named as actions (`ConfirmOrder`, `ResetOrder`) and take no parameters, or one simple parameter.

## 3. Packages: identification and counting
- Identify packages with `PackageData` / `PackageType` via `other.GetComponentInParent<PackageData>()`. **No new tag checks.** Per `docs/decisions/007-material-labels-and-keys.md`, every package also carries a material key (snake_case, same in every scene, never shown to trainees), its displayed Mat.-Nr and its VPE on `PackageData`; never derive the key from the `PackageType` int, the GameObject name or the label. Never remove or reorder `PackageType` values (serialized as ints). Tags (`GGPackage`, `VPEPackage`, `LooseItem`) exist only in old BasicPickingScene code and in the unused `ScaleItemCounter`.
- Hands, controllers and loose colliders also enter triggers. Return early when `PackageData` is `null`.
- Count each package once: keep a `HashSet<PackageData>` and update counts only when `Add`/`Remove` returns `true`.
- `OnTriggerExit` must mirror `OnTriggerEnter`. A package that is destroyed or disabled inside a trigger gets no exit call. Handle this when a scenario can destroy or disable packages.

## 4. Updating the UI (Quest performance)
- Don't write `TMP_Text.text` in `Update`. Write it only when the value changes, e.g. in a `Recalculate()` / `UpdateDisplay()` called from trigger events.
- No `GetComponent`, `Find*`, LINQ or string building in `Update` / `FixedUpdate`. Cache references in `Awake` or the Inspector.
- Use `FindFirstObjectByType` / `FindAnyObjectByType`. `FindObjectOfType` is deprecated in Unity 6. In Unity 6.4 the `FindObjectsByType` overloads with `FindObjectsSortMode` are deprecated too: use `FindObjectsByType<T>()` or `FindObjectsByType<T>(FindObjectsInactive)`.
- Unsubscribe events and stop coroutines in `OnDisable` / `OnDestroy`.

## 5. Order data and validation
- **Single source for order quantities (decided, `docs/decisions/005-order-single-source.md`):** each scene declares its order once, in WMS units: order lines of {`PackageType`, Soll in pieces, VPE in pieces per package} as serialized fields on the scene's counter. Full packages (Soll ÷ VPE) and loose pieces (remainder) are derived, never declared. Validators, feedback and the WMS panel text read the counter. BasicPickingScene keeps `CalcForWMS.necessarySize` (pieces). Never repeat a quantity as a literal: no `== 2`, `"/ 2"`, `"2 GG"` in code or text.
- Keep the rule itself separate from Unity, so it can be tested. Pass in plain values, e.g. `static bool IsOrderCorrect(int gga, int ggb, int loose, OrderRequirement req)`, and get a plain result back. The MonoBehaviour only collects counts and shows the result. See §10.
- A validator decides **right/wrong and why** (e.g. too few, too many, wrong type). It doesn't decide **wording**. Feedback text is built from that reason (§6).
- **Validator result format (decided, `docs/decisions/006-validator-result-format.md`):** the order rule lives in the pure C# asmdef `XRPick.Orders` (EditMode-tested) and returns an ordered list of `{Code, Item}` errors plus per-order-line counts; `Correct` means the list is empty. `Item` is a stable, language-independent material key, never an enum name or display text. The screen shows only the first error (reason + what to do) and a generic "more errors" line; the log records the whole list. The result holds no text and no target numbers.

## 6. Trainee-facing text
- End users read **German only** (formal *Sie*). WMS labels (`Soll:`, `Ist:`, the confirm key label…) are never translated. See `docs/i18n/README.md` and `docs/i18n/glossary.csv`.
- **Mechanism (decided, `docs/decisions/002-localization-mechanism.md`):** Unity Localization package, one String Table Collection `UI`, locales `de` (default) and `en`.
  - Scene texts use `LocalizeStringEvent`.
  - Code gets strings only through `Loc.Get(key, args)`; don't put `LocalizedString` fields in each script.
  - The language is chosen once on WelcomeScene and saved in `PlayerPrefs`. Changing it reloads the scene.
- **Until #11 is done:** no **new** hard-coded English or *du* text shown to the trainee. Every new or changed string goes through the `localize-strings` skill: key + EN + DE, with a row in `docs/i18n/strings-todo.csv` (the skill creates it if missing). Put the strings in one place per script (a `const` block or serialized fields), not scattered through the logic, so they move easily to `Loc.Get`.
- Don't build sentences by concatenating word pieces (`"Sie haben " + n + " Pakete"`). German word order differs. Use whole sentences with named placeholders (`{count}`).
- No Smart String plural forms. Write counters as labels instead: `Pakete: {placed} / {required}`. Put rich text (`<color>`, `<b>`) around a whole sentence, never around single words.
- Colours for feedback: green = correct, red = wrong. Use the same colour values in every scene.
- Debug and log messages (`Debug.Log*`) are in English. They are for developers.

## 7. Errors and logging
- A required Inspector reference that is missing gets `Debug.LogError("<Class>: <field> is not assigned.", this)`, then return. Passing `this` makes a click on the Console line select the GameObject. In a button method, check all required references with `RequiredReference.IsMissing(field, nameof(field), this)` joined by `|` (not `||`, so every missing one is logged), as the validators do since #7.
- Optional references: null-check silently.
- No `Debug.Log` in per-frame code. Remove temporary debug logs before committing.
- Don't swallow exceptions with an empty `catch`.

## 8. Code style
- 4 spaces, no tabs. Braces on their own line (Allman), as in the existing code.
- Always use braces for `if`/`else` bodies with more than one line. A one-line guard `if (x == null) return;` without braces is fine.
- `using` order: `System…`, then `TMPro` / `UnityEngine…`, then project. Remove unused `using` lines.
- No large blocks of commented-out code. Delete them; git keeps the history. `MenuSelectionController.cs` is an old exception.
- `var` only when the type is obvious from the right-hand side.
- Use `new()` target-typed construction and `string.Empty`, as in the existing code.

## 9. Unity asset safety
- **Never delete, regenerate or hand-edit `.meta` files.** Scenes reference scripts by the GUID stored there. Rename or move a script with `git mv` on both the `.cs` and its `.meta`.
- Scene names are loaded by string in `SceneMenuController`. Renaming a scene means updating that script and `EditorBuildSettings.asset` together.
- Don't hand-edit `.unity` / `.prefab` YAML unless the change is trivial and explained. Prefer listing Inspector steps for the person in the Editor.
- Don't hand-edit `*.csproj` / `*.slnx`. Unity generates them.
- Never put confidential requirement documents or their images under `Assets/`.

## 10. Testability
- Logic that doesn't need Unity goes in plain C# classes or static methods: order rules, quantity maths, feedback reason. These can get EditMode tests (`com.unity.test-framework` is installed).
- When tests are added, put them in `Assets/Tests/EditMode/` with their own asmdef that references the code under test. A test asmdef can't reference `Assembly-CSharp`, so this first needs an asmdef for `Assets/Scripts/`. That is a separate decision. The new asmdef must reference TMPro and every Meta/XR assembly the scripts use. Code elsewhere in `Assets/` that uses project classes would then stop compiling.
- Whatever can only be checked in Play mode is covered by the `unity-safe-change` test plan (UnityMCP plus manual cases).

## 11. Comments
- Write new comments in **English**. Not all developers read Vietnamese. Existing Vietnamese comments can stay. Translate them when you change that code.
- Comment **why**, not what: the reason for a rule, a link to a backlog item (`// #34: values hidden until confirm`) or a decision (`docs/decisions/001`).

## 12. Git
- From now on, one branch per change: `fix/<n>-<slug>`, `feat/<n>-<slug>` for backlog items (as in the `review-to-backlog` skill), and `docs/<slug>` for documentation. Merge into `main`; don't commit directly to `main`. Earlier docs commits went straight to `main`.
- Commit messages: `<type>: <summary>`, type is one of `fix`, `feat`, `refactor`, `docs`, `chore`, `test`. Mention the backlog item: `fix: count packages once in CalcForWMS (#2)`.
- Commit scene changes together with the code that needs them. Check `git diff` for unintended scene or `ProjectSettings` changes before committing.
- `.gitattributes` keeps Unity YAML as LF. Don't remove those rules.
