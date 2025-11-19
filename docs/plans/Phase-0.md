# Phase 0: Foundation - Architecture & Design Decisions

## Overview

This phase establishes the architectural foundation for all Core Fighting Mechanics. It defines design patterns, conventions, and technical approaches that will be used throughout Phases 1-5. Read this document thoroughly before beginning implementation.

**Purpose:** Ensure consistency, maintainability, and alignment with existing codebase patterns.

---

## Architecture Decisions (ADRs)

### ADR-001: Component-Based Extension of Existing Systems

**Decision:** Extend the existing component-based character architecture rather than refactoring it.

**Rationale:**
- Current architecture (`CharacterController` + Components) is well-designed and tested
- New systems integrate cleanly as additional components or extensions
- Maintains backward compatibility with existing combat/AI systems
- Reduces risk of breaking existing functionality

**Implementation Pattern:**
```
CharacterController (existing)
├── CharacterAnimator (existing)
├── CharacterInput (existing)
├── CharacterMovement (existing)
├── CharacterCombat (existing - extend)
├── CharacterHealth (existing)
├── CharacterAI (existing)
├── CharacterStamina (NEW - Phase 1)
├── CharacterDodge (NEW - Phase 2)
├── CharacterParry (NEW - Phase 2)
├── CharacterComboTracker (NEW - Phase 3)
├── CharacterSpecialMoves (NEW - Phase 4)
└── CharacterScoring (NEW - Phase 4)
```

**Consequences:**
- New components must follow existing lifecycle patterns (`Initialize()`, `Update()`, events)
- Components communicate via C# events (loose coupling)
- `CharacterController` orchestrates component initialization

---

### ADR-002: ScriptableObject-Driven Configuration

**Decision:** All tunable gameplay values use ScriptableObjects, following the existing `CharacterStats` and `AttackData` patterns.

**Rationale:**
- Consistent with existing data architecture
- Enables designer-friendly balancing without code changes
- Supports character-specific customization
- Easy to version control and share between characters

**New ScriptableObject Types:**
- `StaminaData` - Stamina pool size, regen rate, consumption costs, exhaustion penalties
- `ComboSequenceData` - Predefined combo sequences with timing, damage bonuses, effects
- `SpecialMoveData` - Special move properties, cooldown, stamina cost, knockdown type
- `ScoringWeights` - Judge scoring weights for different actions

**File Locations:**
- Scripts: `Assets/Knockout/Scripts/Characters/Data/`
- Asset instances: `Assets/Knockout/Data/Characters/[CharacterName]/`

---

### ADR-003: State Machine Integration for New Combat States

**Decision:** Extend `CombatStateMachine` with new states rather than creating parallel state management.

**Rationale:**
- Existing state machine is robust and handles transitions well
- New states (Dodging, Exhausted, ParryStagger) fit naturally into combat flow
- Avoids state desynchronization issues
- Maintains single source of truth for character state

**New Combat States:**
- `DodgingState` - Active dodge with i-frames (Phase 2)
- `ExhaustedState` - Stamina depletion penalty (Phase 1)
- `ParryStaggerState` - Attacker stagger after parry (Phase 2)
- `SpecialKnockdownState` - Enhanced knockdown from special moves (Phase 4)

**State Transition Rules:**
- States added to existing state machine graph
- Follow existing state patterns (enter/exit/update lifecycle)
- Events fire on state transitions for UI updates

---

### ADR-004: Frame-Perfect Timing Using Fixed Update

**Decision:** Critical timing systems (combo windows, parry frames, i-frames) use `FixedUpdate` at 60fps.

**Rationale:**
- Game already targets 60fps with frame-based attack timing
- Existing `AttackData` uses frame counts (startup/active/recovery frames)
- Ensures deterministic, platform-independent timing
- Simplifies combo timing calculations

**Implementation Guidelines:**
- Combo timing windows: measured in frames (e.g., 12 frames = 0.2s at 60fps)
- i-frame duration: frame count (e.g., 8 frames = ~0.133s)
- Parry window: frame count (e.g., 6 frames = 0.1s)
- All timing values exposed in ScriptableObjects for tuning

**Frame Rate Constant:**
```csharp
public const int TARGET_FRAME_RATE = 60;
public const float FIXED_DELTA_TIME = 1f / TARGET_FRAME_RATE;
```

---

### ADR-005: Event-Driven Communication Between Systems

**Decision:** Components communicate via C# events rather than direct references.

**Rationale:**
- Follows existing codebase pattern (e.g., `OnHealthChanged`, `OnDeath`)
- Maintains loose coupling between systems
- UI can subscribe to gameplay events without tight dependencies
- Easy to add new listeners (e.g., VFX, audio, scoring)

**Event Naming Convention:**
```csharp
// Component events
public event Action<float> OnStaminaChanged;
public event Action OnStaminaDepleted;
public event Action<int> OnComboHitLanded;
public event Action<ComboSequenceData> OnComboSequenceCompleted;
public event Action<SpecialMoveData> OnSpecialMoveUsed;
public event Action<ParryData> OnParrySuccessful;

// Scoring events
public event Action<ScoringAction> OnScoringActionPerformed;
```

**Event Flow Example:**
```
Player lands hit in combo
→ CharacterCombat fires OnHitLanded
→ CharacterComboTracker listens, increments combo count, fires OnComboHitLanded
→ ComboUI listens, updates combo counter display
→ CharacterScoring listens, records hit for judge scoring
```

---

### ADR-006: Momentum System via Scoring Accumulation

**Decision:** Momentum is implicitly tracked via scoring system rather than explicit meter.

**Rationale:**
- User requested "momentum-based" combat (offense builds advantage, defense resets)
- Judge scoring already tracks offensive/defensive actions
- Avoids adding another explicit resource (stamina already exists)
- Momentum = accumulated scoring advantage in current round

**Implementation:**
- `CharacterScoring` tracks all actions (hits, blocks, parries, combos, aggression time)
- Scoring differential represents current momentum
- Defensive actions (blocks, parries) don't add opponent momentum
- Round timer expiry triggers judge decision based on accumulated scores

**Note on Phase 4 Task 13 (MomentumTracker):**
This task is marked "optional" because momentum is **already implicit** in the scoring differential. Task 13 provides an optional UI visualization (momentum bar) to display this differential visually. The core momentum-based philosophy is implemented through scoring - the UI is just a nice-to-have visualization.

---

### ADR-007: Test-Driven Development with Component Isolation

**Decision:** All new components have dedicated EditMode unit tests and PlayMode integration tests.

**Rationale:**
- Existing codebase has 80%+ test coverage - maintain this standard
- EditMode tests for data validation and state logic (fast)
- PlayMode tests for component integration and gameplay flow (comprehensive)
- Isolated component tests enable confident refactoring

**Test Structure:**
```
Assets/Knockout/Tests/
├── EditMode/
│   ├── Stamina/
│   │   ├── StaminaDataTests.cs
│   │   └── StaminaCalculatorTests.cs
│   ├── Combos/
│   │   ├── ComboSequenceDataTests.cs
│   │   └── ComboTimingTests.cs
│   └── Scoring/
│       └── ScoringCalculatorTests.cs
└── PlayMode/
    ├── Stamina/
    │   └── CharacterStaminaTests.cs
    ├── Defense/
    │   ├── CharacterDodgeTests.cs
    │   └── CharacterParryTests.cs
    ├── Combos/
    │   └── CharacterComboTrackerTests.cs
    └── Integration/
        └── CoreMechanicsIntegrationTests.cs
```

**Test Coverage Targets:**
- Data classes (ScriptableObjects): 100%
- Component logic: 85%+
- Integration scenarios: Key gameplay flows covered

---

## Design Decisions

### DD-001: Stamina System Design

**Consumption Model:**
- Only offensive actions consume stamina (attacks, special moves)
- Defensive actions free (blocking, dodging, parrying)
- Movement free (existing movement system unchanged)

**Regeneration Model:**
- Passive regeneration when not attacking (configurable rate)
- Regeneration pauses during attack startup/active/recovery frames
- Full regeneration available during all other states (idle, moving, blocking, dodging)

**Depletion Penalty:**
- Hitting 0 stamina triggers `ExhaustedState` (brief vulnerable period)
- Cannot attack while exhausted (defensive actions still available)
- Stamina regenerates slower during exhaustion
- Exhaustion clears when stamina reaches threshold (e.g., 25%)

**Tunable Parameters (StaminaData):**
```csharp
public float MaxStamina = 100f;
public float RegenPerSecond = 25f; // ~4s to full recovery
public float JabCost = 10f;
public float HookCost = 15f;
public float UppercutCost = 20f;
public float SpecialMoveCost = 30f;
public float ExhaustionDuration = 2f;
public float ExhaustionRegenMultiplier = 0.5f;
public float ExhaustionRecoveryThreshold = 25f;
```

---

### DD-002: Combo System Design

**Natural Chaining:**
- Any attack can chain into any other attack within timing window
- Timing windows vary by attack type (attack-specific, not universal):
  - Jab: 18 frames (~0.3s) - fast, forgiving
  - Hook: 12 frames (~0.2s) - medium
  - Uppercut: 6 frames (~0.1s) - tight, precise
- Chain window starts at end of recovery frames
- Missing window breaks combo (resets to 0)

**Predefined Sequences:**
- Specific attack sequences unlock bonuses (e.g., Jab-Jab-Hook)
- Bonuses: damage multiplier, special effects, enhanced knockback
- Sequences defined per-character via `ComboSequenceData` ScriptableObjects
- Sequence completion triggers bonus on final hit

**Damage Scaling (Heavy):**
- 1st hit: 100% damage
- 2nd hit: 75% damage
- 3rd hit: 50% damage
- 4th+ hits: 50% damage (floor)
- Predefined sequence bonuses apply AFTER scaling (multiplicative)

**Combo Breaking:**
- Blocked hit stops combo (small window to react)
- Hit/stagger interrupts combo (mutual trading)
- Windows are "small" (configurable, ~6-8 frames to block/counter)

---

### DD-003: Dodge & Parry Design

**Dodge System:**
- Directional input: left, right, back (no forward dodge)
- Quick dash movement (~0.3s total animation)
- i-frames during first ~40% of dash (~8 frames at 60fps)
- No stamina cost (defensive action)
- Animations already exist (left_dodge.fbx, right_dodge.fbx)

**Parry System:**
- Timing-based perfect block (normal block already exists)
- Parry window: ~6 frames before hit connects
- Input: block button pressed at precise moment
- Success: negates damage, staggers attacker briefly (~0.5s)
- Counter window: defender can attack during stagger
- No stamina cost (defensive action)

**Differentiation:**
- Block: hold button, reduces damage, no counter opportunity
- Parry: timed press, negates damage, opens counter window
- Dodge: directional movement, i-frames, repositioning

---

### DD-004: Special Moves Design

**Resource Gating:**
- Each special move has cooldown timer (e.g., 30-60s)
- AND stamina cost (e.g., 30-50 stamina)
- Both must be available to use special move
- Prevents spamming even when cooldown ready

**Character Variety:**
- Each character has ONE signature special move
- Special moves use existing attack animations with enhanced properties
- OR unique animations added per-character later
- Examples: "Haymaker" (powerful hook variant), "Liver Shot" (body uppercut)

**Special Knockdown:**
- Special moves can trigger enhanced knockdown state
- Longer recovery time than normal knockdown
- Visual distinction (animation, VFX)
- Scores higher in judge system

**Tunable Parameters (SpecialMoveData):**
```csharp
public string SpecialMoveName;
public AttackData BaseAttackData; // Reference to Jab/Hook/Uppercut as base
public float DamageMultiplier = 2f;
public float KnockbackMultiplier = 2f;
public float CooldownSeconds = 45f;
public float StaminaCost = 40f;
public bool TriggersSpecialKnockdown = true;
```

---

### DD-005: Judge Scoring Design

**Tracked Metrics (Comprehensive):**
- **Offensive Stats:**
  - Clean hits landed (head/body)
  - Total damage dealt
  - Combos completed (2+ hits)
  - Predefined sequences landed
  - Special moves landed
  - Knockdowns inflicted

- **Defensive Stats:**
  - Hits blocked
  - Parries successful
  - Dodges successful (avoided hits)

- **Control Stats:**
  - Aggression time (time spent in offensive range)
  - Ring control (time spent in center vs corners)
  - Stamina management (avg stamina %, exhaustion count)

**Scoring Weights:**
- Configurable via `ScoringWeights` ScriptableObject
- Different actions have different point values
- Example weights:
  - Clean hit: 1 point
  - Combo (3+ hits): 3 points
  - Predefined sequence: 5 points
  - Special move landed: 8 points
  - Knockdown: 10 points
  - Parry: 2 points
  - Aggression (per second): 0.1 points

**Round Decision:**
- At round timer expiry (no knockout), compare total scores
- Higher score wins round
- Tie: sudden death or round draw (configurable)
- Scores reset each round (not cumulative across match)

---

### DD-006: Animation Availability and Fallbacks

**Confirmed Available Animations:**
Located in `Assets/Knockout/Animations/Characters/BaseCharacter/AnimationClips/`:
- **Attack animations:** Left_Jab.fbx, left_hook.fbx, Right_hook.fbx, left_uppercut.fbx, Right_uppercut.fbx
- **Defensive animations:** Block.fbx, Block_to_idle.fbx, Idle_to_Block.fbx, left_dodge.fbx, right_dodge.fbx
- **Hit reactions:** hit_by_jab_V1.fbx, hit_by_hook_V1.fbx, Block_straigh_hit_V1.fbx
- **State animations:** knockout_V1.fbx, Knockouts_Countdown_V1.fbx, Win_V1.fbx, Idle_tired.fbx

**Animation Fallbacks (If Missing):**
- **Exhaustion animation:** Use `Idle_tired.fbx` or standard `Idle` animation with slower speed
- **Parry animation:** Reuse `Block.fbx` (differentiation comes from timing/VFX, not animation)
- **Back dodge animation:** Mirror `left_dodge.fbx` or `right_dodge.fbx` if dedicated back dodge missing
- **Special knockdown animation:** Reuse `knockout_V1.fbx` with longer duration
- **Special move animations:** Reuse base attack animations (Hook for Haymaker) with enhanced VFX

**Documentation Requirement:**
If using fallback animations, note the substitution in the commit message and verification checklist. Example:
```
feat(stamina): add ExhaustedState with animation

- Uses Idle_tired.fbx for exhaustion animation
- Note: No dedicated exhaustion animation, using existing tired idle
```

---

## Shared Patterns & Conventions

### Naming Conventions

**Scripts:**
- Components: `Character[Feature].cs` (e.g., `CharacterStamina.cs`)
- Data: `[Feature]Data.cs` (e.g., `StaminaData.cs`)
- States: `[Feature]State.cs` (e.g., `ExhaustedState.cs`)
- UI: `[Feature]UI.cs` (e.g., `StaminaBarUI.cs`)

**Namespaces:**
```csharp
namespace Knockout.Characters.Components
namespace Knockout.Characters.Data
namespace Knockout.Combat.States
namespace Knockout.UI
namespace Knockout.Systems
```

**Events:**
- Prefix: `On`
- Past tense for completed actions: `OnStaminaDepleted`, `OnComboCompleted`
- Present tense for state changes: `OnStaminaChanging` (rare, prefer past tense)

---

### Component Lifecycle Pattern

All new components follow this pattern:

```csharp
public class CharacterFeature : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FeatureData featureData;

    [Header("Runtime State")]
    private bool isInitialized = false;

    // Events
    public event Action OnFeatureEvent;

    // Properties
    public bool IsActive { get; private set; }

    public void Initialize()
    {
        if (isInitialized) return;

        // Setup dependencies
        // Subscribe to events
        // Initialize state

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;
        // Frame-rate independent logic
    }

    private void FixedUpdate()
    {
        if (!isInitialized) return;
        // Fixed-rate timing logic (60fps)
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        // Cleanup
    }
}
```

---

### ScriptableObject Pattern

```csharp
[CreateAssetMenu(fileName = "FeatureData", menuName = "Knockout/Feature Data", order = N)]
public class FeatureData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string featureName = "Feature";

    [Header("Configuration")]
    [SerializeField]
    [Tooltip("Clear description of this parameter")]
    private float tunableValue = 10f;

    // Public read-only properties
    public string FeatureName => featureName;
    public float TunableValue => tunableValue;

    // Validation
    private void OnValidate()
    {
        tunableValue = Mathf.Max(0f, tunableValue);
    }
}
```

---

### State Machine State Pattern

```csharp
public class NewState : CombatState
{
    private float stateTimer;

    public override void OnEnter()
    {
        base.OnEnter();
        stateTimer = 0f;
        // Setup state
        // Trigger animations
        // Fire events
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        stateTimer += Time.deltaTime;
        // State logic
        // Check transition conditions
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        // Fixed-rate logic
    }

    public override void OnExit()
    {
        base.OnExit();
        // Cleanup
        // Fire exit events
    }
}
```

---

## Testing Strategy

### Unit Testing (EditMode)

**Test Data Classes:**
- Validate ScriptableObject constraints (OnValidate)
- Test calculations/formulas
- Boundary conditions
- Invalid inputs

**Example:**
```csharp
[Test]
public void StaminaData_RegenPerSecond_NeverNegative()
{
    var data = ScriptableObject.CreateInstance<StaminaData>();
    // Set via reflection to bypass OnValidate
    // Verify OnValidate clamps to minimum
}
```

### Integration Testing (PlayMode)

**Test Component Behavior:**
- Component initialization
- Event firing
- State transitions
- Integration with existing systems

**Example:**
```csharp
[UnityTest]
public IEnumerator CharacterStamina_ConsumesOnAttack()
{
    // Setup character with stamina
    // Perform attack
    // Verify stamina decreased
    // Verify events fired
    yield return null;
}
```

### Performance Testing

**Frame Rate Stability:**
- New systems must maintain 60fps target
- No excessive allocations (GC pressure)
- Use existing performance test patterns

### Test Timing (Hybrid TDD)

**When to Write Tests:**
- **ScriptableObjects**: Test AFTER implementation (validation-focused, simple data classes)
- **Component logic**: Test BEFORE when feasible for complex, isolable logic (true TDD)
- **Integration tests**: Test AFTER phase complete (requires all systems working together)

**Rationale:**
Unity's component dependencies and MonoBehaviour lifecycle make pure TDD difficult. This hybrid approach balances code quality with practical velocity:
- Data validation tests are straightforward and fast to write after implementation
- Complex component logic benefits from test-first design when isolable
- Integration tests require working systems and are naturally written last

**In Practice:**
Most tasks will say "write tests" after implementation steps. For complex algorithmic logic (e.g., combo sequence detection, damage scaling calculations), consider writing unit tests first if you can isolate the logic from Unity dependencies.

---

## Common Pitfalls to Avoid

### Pitfall 1: Breaking Existing Functionality

**Problem:** New components interfere with existing combat/AI systems

**Prevention:**
- Test with existing gameplay before adding new features
- Use events rather than direct coupling
- Verify AI can use new systems (Phase 4+ consideration)

---

### Pitfall 2: Hardcoded Values

**Problem:** Magic numbers in code, difficult to balance

**Prevention:**
- ALL tunable values in ScriptableObjects
- Constants for system-wide values (TARGET_FRAME_RATE)
- No hardcoded timing, damage, costs in component code

---

### Pitfall 3: Frame-Rate Dependent Timing

**Problem:** Combo windows, i-frames vary on different hardware

**Prevention:**
- Use `FixedUpdate` for timing-critical systems
- Frame counts, not time deltas
- Test at variable frame rates

---

### Pitfall 4: State Desynchronization

**Problem:** Combo tracker thinks player attacking, state machine says idle

**Prevention:**
- Single source of truth: `CombatStateMachine`
- Components react to state changes via events
- Don't duplicate state tracking

---

### Pitfall 5: Event Memory Leaks

**Problem:** Components subscribe to events but don't unsubscribe

**Prevention:**
- Always unsubscribe in `OnDestroy()`
- Pattern: `component.OnEvent += Handler` in `Initialize()`
- Pattern: `component.OnEvent -= Handler` in `OnDestroy()`

---

## Tech Stack Summary

**Unity Systems:**
- State Machines (existing `CombatStateMachine` + new states)
- Input System (existing `CharacterInput` + new dodge/parry inputs)
- Animation System (existing `CharacterAnimator` + new triggers/states)
- ScriptableObjects (data-driven configuration)

**C# Patterns:**
- Component-based architecture
- Event-driven communication
- State pattern (combat states)
- ScriptableObject pattern (data)

**Testing:**
- Unity Test Framework (EditMode + PlayMode)
- NUnit assertions
- UnityEngine.TestTools (UnityTest)

---

## Next Steps

After reading Phase 0, proceed to:

**[Phase 1: Core Resource Systems](Phase-1.md)**

Start with stamina and enhanced knockdown implementation, the foundation for all other systems.
