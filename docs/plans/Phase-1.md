# Phase 1: Core Resource Systems

## Phase Goal

Implement the stamina management system and enhanced knockdown states, providing the resource foundation for all combat mechanics. Stamina governs offensive actions (attacks, special moves) while defensive actions remain free. Enhanced knockdowns include stamina exhaustion (depletion penalty) and special knockdown states (for future special moves).

**Success Criteria:**
- Characters have stamina that depletes on attacks and regenerates passively
- Stamina depletion triggers exhaustion state (vulnerable, cannot attack)
- Special knockdown state exists for enhanced knockdowns
- All systems configurable via ScriptableObjects
- 85%+ test coverage for new components

**Estimated Tokens:** ~100,000

---

## Prerequisites

**Required Reading:**
- [Phase 0: Foundation](Phase-0.md) - Architecture decisions and patterns

**Existing Systems to Understand:**
- `CharacterCombat` - Attack execution (Assets/Knockout/Scripts/Characters/Components/CharacterCombat.cs)
- `CombatStateMachine` - State management (Assets/Knockout/Scripts/Combat/CombatStateMachine.cs)
- `AttackData` - Attack properties (Assets/Knockout/Scripts/Characters/Data/AttackData.cs)
- Existing combat states in `Assets/Knockout/Scripts/Combat/States/`

**Environment:**
- Unity 2021.3.8f1 LTS
- Test framework accessible (Window > General > Test Runner)

---

## Tasks

### Task 1: Create StaminaData ScriptableObject

**Goal:** Define stamina configuration data structure following existing `AttackData` pattern.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Data/StaminaData.cs` - ScriptableObject class

**Prerequisites:**
- Review `AttackData.cs` for ScriptableObject pattern
- Review `CharacterStats.cs` for data organization

**Implementation Steps:**

1. Create `StaminaData` class in `Knockout.Characters.Data` namespace
2. Add `[CreateAssetMenu]` attribute with menu path `"Knockout/Stamina Data"` (order = 3)
3. Define serialized fields for stamina configuration:
   - Max stamina pool
   - Regeneration rate (per second)
   - Attack costs (Jab, Hook, Uppercut - reference existing attack type indices)
   - Special move base cost
   - Exhaustion parameters (duration, regen multiplier, recovery threshold)
4. Expose public read-only properties for all fields
5. Implement `OnValidate()` to clamp values to valid ranges (no negative values, costs <= max stamina)
6. Add XML documentation comments following existing pattern
7. Add tooltips to all serialized fields

**Design Guidance:**
- Follow DD-001 (Stamina System Design) from Phase 0
- Default values should match balanced/realistic tuning from design (max=100, regen=25/sec)
- Use `[Header]` attributes to group related fields
- Array for attack costs indexed by attack type (0=Jab, 1=Hook, 2=Uppercut)

**Verification Checklist:**
- [ ] ScriptableObject appears in Create menu under "Knockout/Stamina Data"
- [ ] All fields have tooltips
- [ ] OnValidate clamps negative values to 0
- [ ] OnValidate prevents attack costs > max stamina
- [ ] XML docs on class and public properties

**Testing Instructions:**

Create EditMode test: `Assets/Knockout/Tests/EditMode/Stamina/StaminaDataTests.cs`

Test cases:
- ScriptableObject creation succeeds
- OnValidate clamps negative regen rate to 0
- OnValidate clamps attack costs to [0, maxStamina]
- Default values match design spec
- Boundary conditions (maxStamina = 0, regenRate = 0)

**Commit Message Template:**
```
feat(stamina): add StaminaData ScriptableObject

- Define stamina configuration structure
- Add validation for tunable parameters
- Follow existing AttackData pattern
- Includes comprehensive tooltips and documentation
```

**Estimated Tokens:** ~3,000

---

### Task 2: Extend AttackData with Stamina Cost

**Goal:** Add stamina cost field to existing `AttackData` to support per-attack customization.

**Files to Modify:**
- `Assets/Knockout/Scripts/Characters/Data/AttackData.cs` - Add stamina cost field

**Prerequisites:**
- Task 1 complete (understand stamina values)
- Read existing `AttackData.cs` structure

**Implementation Steps:**

1. Add new serialized field in "Damage Properties" header section:
   - `staminaCost` (float, default based on attack type)
   - Tooltip explaining this overrides StaminaData default if set
2. Add public property `StaminaCost`
3. Update `OnValidate()` to clamp stamina cost >= 0
4. Document that 0 stamina cost means "use default from StaminaData"

**Design Guidance:**
- This allows per-attack customization (e.g., one character's Jab costs more)
- Default = 0 means "use StaminaData.attackCosts[attackTypeIndex]"
- Non-zero value overrides StaminaData default
- Maintains backward compatibility (existing AttackData assets default to 0)

**Verification Checklist:**
- [ ] Existing AttackData assets load without errors
- [ ] New field appears in Inspector for AttackData
- [ ] Tooltip explains override behavior
- [ ] OnValidate clamps to >= 0

**Testing Instructions:**

Update EditMode test: `Assets/Knockout/Tests/EditMode/Characters/AttackDataTests.cs` (if exists, otherwise create)

Test cases:
- Default stamina cost is 0
- OnValidate clamps negative cost to 0
- StaminaCost property returns correct value

**Commit Message Template:**
```
feat(stamina): add stamina cost to AttackData

- Support per-attack stamina cost customization
- Default 0 = use StaminaData default
- Maintains backward compatibility
```

**Estimated Tokens:** ~2,000

---

### Task 3: Create CharacterStamina Component

**Goal:** Implement stamina tracking, consumption, and regeneration component.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterStamina.cs` - Component class

**Prerequisites:**
- Task 1 complete (StaminaData exists)
- Task 2 complete (AttackData has stamina cost)
- Review `CharacterHealth.cs` for component pattern
- Review `CharacterCombat.cs` for event subscriptions

**Implementation Steps:**

1. Create `CharacterStamina` component in `Knockout.Characters.Components` namespace
2. Implement component lifecycle pattern from Phase 0:
   - Dependencies: `CharacterController`, `StaminaData`, `CharacterCombat`
   - `Initialize()` method
   - Event subscriptions in `Initialize()`
   - `FixedUpdate()` for regeneration (60fps timing)
   - `OnDestroy()` for cleanup
3. Track current stamina (private float, starts at max)
4. Implement stamina consumption:
   - **Discover attack event:** Search CharacterCombat.cs for attack events:
     - Pattern: `Grep pattern: "public event.*Attack" in CharacterCombat.cs`
     - Look for: `OnAttackStarted`, `OnAttackExecuted`, `OnAttackBegin`, or similar
     - If not found: Add `public event Action<AttackData> OnAttackStarted;` following existing event pattern
     - Document which event you used in verification checklist
   - Subscribe to discovered attack event in `Initialize()`
   - Deduct stamina based on `AttackData.StaminaCost` (if > 0) or `StaminaData.attackCosts[attackTypeIndex]`
   - Fire `OnStaminaChanged` event
   - If stamina hits 0, fire `OnStaminaDepleted` event
5. Implement passive regeneration:
   - In `FixedUpdate()`, check if not currently attacking
   - Add `StaminaData.RegenPerSecond * Time.fixedDeltaTime`
   - Clamp to max stamina
   - Fire `OnStaminaChanged` event if value changed
6. Pause regeneration during attack (startup + active + recovery frames)
7. Expose public properties: `CurrentStamina`, `MaxStamina`, `StaminaPercentage`
8. Add method `HasStamina(float cost)` to query if action is affordable

**Design Guidance:**
- Follow ADR-005 (Event-Driven Communication)
- Follow ADR-004 (Frame-Perfect Timing with FixedUpdate)
- Regeneration pauses during attack, not during movement/blocking/idle
- Determine "currently attacking" by subscribing to combat state changes or `CharacterCombat` attack state

**Verification Checklist:**
- [ ] Component initializes correctly on character prefab
- [ ] Stamina decreases when attacks performed
- [ ] Stamina regenerates when idle
- [ ] Regeneration pauses during attack
- [ ] Events fire correctly (OnStaminaChanged, OnStaminaDepleted)
- [ ] HasStamina() correctly predicts if action affordable

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/Stamina/CharacterStaminaTests.cs`

Test cases:
- Component initialization sets stamina to max
- Performing attack reduces stamina by correct amount
- Stamina regenerates over time when idle
- Stamina does not regenerate during attack
- OnStaminaDepleted fires when stamina reaches 0
- HasStamina() returns correct boolean

**Commit Message Template:**
```
feat(stamina): implement CharacterStamina component

- Track current stamina with max pool
- Consume stamina on attacks
- Passive regeneration when not attacking
- Fire events for UI/other systems integration
```

**Estimated Tokens:** ~8,000

---

### Task 4: Integrate Stamina into CharacterCombat

**Goal:** Prevent attacks when insufficient stamina, integrate stamina checks into attack execution.

**Files to Modify:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterCombat.cs` - Add stamina gating

**Prerequisites:**
- Task 3 complete (CharacterStamina exists and functional)
- Read existing `CharacterCombat.cs` attack execution logic

**Implementation Steps:**

1. Add dependency on `CharacterStamina` component (get reference in `Initialize()` or via `GetComponent<>()`)
2. **Locate attack execution method** in CharacterCombat.cs:
   - Search for methods handling attack: `PerformAttack()`, `ExecuteAttack()`, `ProcessAttackInput()`, or similar
   - Or search where AttackData is first accessed: `Grep pattern: "AttackData" in CharacterCombat.cs`
   - Likely the method called from input handler or AI
   - Add stamina check at the TOP of this method, before any state transitions:
     - Check `characterStamina.HasStamina(attackCost)` before allowing attack
     - If insufficient stamina, early return (attack does not execute)
     - Optionally fire event `OnAttackFailedNoStamina` for feedback
3. Ensure stamina consumption happens at attack initiation (not on hit)
4. Update any AI attack logic to check stamina availability (if AI uses CharacterCombat)

**Design Guidance:**
- Stamina check happens BEFORE state transition to AttackingState
- Block/parry/dodge remain free (no stamina checks for defensive actions)
- Consider feedback for player (sound, UI flash) when attack fails due to stamina - trigger via event

**Verification Checklist:**
- [ ] Attacks fail when stamina insufficient
- [ ] Attacks succeed when stamina sufficient
- [ ] Stamina consumption occurs on attack initiation
- [ ] Defensive actions (block) still work at 0 stamina
- [ ] AI respects stamina limits (if applicable)

**Testing Instructions:**

Update PlayMode test: `Assets/Knockout/Tests/PlayMode/Characters/CharacterCombatTests.cs`

Test cases:
- Attack succeeds when stamina available
- Attack fails when stamina insufficient
- Attack failure does not transition to AttackingState
- Blocking works at 0 stamina

**Commit Message Template:**
```
feat(stamina): integrate stamina gating into combat

- Prevent attacks when stamina insufficient
- Stamina consumed on attack initiation
- Defensive actions remain free
- Add OnAttackFailedNoStamina event
```

**Estimated Tokens:** ~5,000

---

### Task 5: Create ExhaustedState Combat State

**Goal:** Implement exhaustion penalty state triggered when stamina depletes to 0.

**Files to Create:**
- `Assets/Knockout/Scripts/Combat/States/ExhaustedState.cs` - Combat state class

**Prerequisites:**
- Task 3 complete (CharacterStamina fires OnStaminaDepleted)
- Review existing combat states (IdleState, BlockingState) for pattern
- Review `CombatStateMachine.cs` for state management

**Implementation Steps:**

1. Create `ExhaustedState` inheriting from `CombatState`
2. Implement state lifecycle:
   - `OnEnter()`: Trigger exhaustion animation (or reuse idle/tired animation), fire `OnExhaustedStart` event
   - `OnUpdate()`: Track exhaustion duration timer
   - `OnFixedUpdate()`: Check for recovery condition (stamina >= threshold)
   - `OnExit()`: Fire `OnExhaustedEnd` event
3. Prevent state transitions:
   - Cannot transition to `AttackingState` while exhausted
   - Can transition to `BlockingState`, `DodgingState` (defensive actions allowed)
   - Can transition to `HitStunnedState` if hit
4. Auto-recovery: transition back to `IdleState` when `CharacterStamina.StaminaPercentage >= StaminaData.ExhaustionRecoveryThreshold`
5. Slower stamina regen: communicate to `CharacterStamina` to apply regen multiplier during exhaustion (via event or state query)

**Design Guidance:**
- Follow ADR-003 (State Machine Integration)
- Exhaustion is a vulnerable state but not helpless (can still defend)
- Exhaustion duration can be minimum time before checking recovery
- Animation: use existing "Idle_tired.fbx" or idle animation with slower speed

**Verification Checklist:**
- [ ] State entered when stamina depletes
- [ ] Cannot attack while exhausted
- [ ] Can block/dodge while exhausted
- [ ] Automatically exits when stamina recovers to threshold
- [ ] Stamina regenerates slower during exhaustion
- [ ] Events fire on enter/exit

**Testing Instructions:**

Create EditMode test: `Assets/Knockout/Tests/EditMode/Combat/ExhaustedStateTests.cs`

Test cases:
- State transitions to IdleState when recovery threshold met
- CanTransitionTo() prevents AttackingState
- CanTransitionTo() allows BlockingState

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/Combat/ExhaustedStateTests.cs`

Test cases:
- Depleting stamina triggers ExhaustedState
- Recovery to threshold exits ExhaustedState
- Cannot attack while exhausted
- Can block while exhausted

**Commit Message Template:**
```
feat(stamina): add ExhaustedState combat state

- Triggered when stamina depletes to 0
- Prevents attacking, allows defending
- Auto-recovers when stamina threshold met
- Slower stamina regeneration during exhaustion
```

**Estimated Tokens:** ~7,000

---

### Task 6: Integrate ExhaustedState into CombatStateMachine

**Goal:** Wire ExhaustedState into state machine transition graph.

**Files to Modify:**
- `Assets/Knockout/Scripts/Combat/CombatStateMachine.cs` - Add state and transitions

**Prerequisites:**
- Task 5 complete (ExhaustedState exists)
- Read `CombatStateMachine.cs` state registration and transition logic

**Implementation Steps:**

1. Register `ExhaustedState` in state machine initialization:
   - Add state instance to state dictionary
   - Set valid transitions from/to ExhaustedState
2. Add transition trigger:
   - Subscribe to `CharacterStamina.OnStaminaDepleted`
   - Trigger transition to ExhaustedState on event
3. Configure transition rules:
   - From IdleState → ExhaustedState (on depletion)
   - From AttackingState → ExhaustedState (if depletion during attack)
   - From ExhaustedState → IdleState (on recovery)
   - From ExhaustedState → HitStunnedState (if hit)
   - From ExhaustedState → BlockingState (defensive allowed)
4. Handle exhaustion during attack:
   - If stamina depletes during attack recovery, queue ExhaustedState transition
   - Or wait until attack completes, then transition

**Design Guidance:**
- Follow existing state registration pattern in CombatStateMachine
- Exhaustion transition should be high priority (interrupts most actions)
- Use state machine's event-driven transition system

**Verification Checklist:**
- [ ] ExhaustedState registered in state machine
- [ ] Transition to ExhaustedState on stamina depletion
- [ ] Valid transitions configured (can block, can't attack)
- [ ] Transition to IdleState on recovery
- [ ] State machine remains stable with new state

**Testing Instructions:**

Update PlayMode test: `Assets/Knockout/Tests/PlayMode/Combat/CombatStateMachineTests.cs` (if exists)

Test cases:
- State machine transitions to ExhaustedState on depletion event
- Cannot transition to AttackingState while in ExhaustedState
- Transition to IdleState when recovery condition met

**Commit Message Template:**
```
feat(stamina): integrate ExhaustedState into CombatStateMachine

- Register state and configure transitions
- Subscribe to stamina depletion event
- Enforce transition rules (no attacking while exhausted)
```

**Estimated Tokens:** ~4,000

---

### Task 7: Create SpecialKnockdownState Combat State

**Goal:** Implement enhanced knockdown state for special moves (future Phase 4 integration).

**Files to Create:**
- `Assets/Knockout/Scripts/Combat/States/SpecialKnockdownState.cs` - Combat state class

**Prerequisites:**
- Review existing `KnockedDownState.cs` for pattern
- Understand knockdown mechanics from existing codebase

**Implementation Steps:**

1. **Determine inheritance:**
   - Read `KnockedDownState.cs` to check if it's designed for extension (virtual methods, protected fields)
   - If extensible: Inherit from `KnockedDownState` and override recovery time/animation
   - If not extensible: Inherit from `CombatState` and implement knockdown behavior from scratch
   - Document choice in verification checklist
2. Implement enhanced knockdown properties:
   - Longer recovery time than normal knockdown (configurable)
   - Enhanced animation (or reuse knockout animation with longer hold)
   - Fire `OnSpecialKnockdown` event for VFX/audio hooks
3. Implement state lifecycle:
   - `OnEnter()`: Trigger special knockdown animation, start recovery timer
   - `OnUpdate()`: Track recovery time
   - `OnExit()`: Fire recovery event
4. Transition to `IdleState` after recovery time elapses
5. Allow early recovery on player input (if existing knockdown supports this)

**Design Guidance:**
- This state is a placeholder for Phase 4 special moves
- Differentiate from `KnockedDownState` via longer duration and enhanced presentation
- Consider inheriting from `KnockedDownState` and overriding recovery time if architecture supports it

**Verification Checklist:**
- [ ] State can be triggered (manually for now, special moves will trigger in Phase 4)
- [ ] Recovery time configurable and longer than normal knockdown
- [ ] Transitions to IdleState after recovery
- [ ] Events fire for VFX/audio integration

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/Combat/SpecialKnockdownStateTests.cs`

Test cases:
- State enters and triggers animation
- Recovery timer expires and transitions to IdleState
- OnSpecialKnockdown event fires on enter

**Commit Message Template:**
```
feat(knockdown): add SpecialKnockdownState for enhanced knockdowns

- Longer recovery time than normal knockdown
- Enhanced animation support
- Prepare for Phase 4 special moves integration
```

**Estimated Tokens:** ~5,000

---

### Task 8: Integrate SpecialKnockdownState into CombatStateMachine

**Goal:** Register SpecialKnockdownState in state machine for future use.

**Files to Modify:**
- `Assets/Knockout/Scripts/Combat/CombatStateMachine.cs` - Add state registration

**Prerequisites:**
- Task 7 complete (SpecialKnockdownState exists)

**Implementation Steps:**

1. Register `SpecialKnockdownState` in state machine initialization
2. Configure valid transitions:
   - From AttackingState → SpecialKnockdownState (when hit by special move)
   - From SpecialKnockdownState → IdleState (on recovery)
   - From SpecialKnockdownState → KnockedOutState (if health depleted)
3. Add public method `TriggerSpecialKnockdown()` for hit detection to call (Phase 4 will use this)

**Design Guidance:**
- State exists but won't be actively used until Phase 4
- Provide clean API for future special move hit detection

**Verification Checklist:**
- [ ] SpecialKnockdownState registered
- [ ] Transitions configured
- [ ] Public trigger method available
- [ ] State machine stable with new state

**Testing Instructions:**

Update EditMode test: `Assets/Knockout/Tests/EditMode/Combat/CombatStateMachineTests.cs`

Test cases:
- State machine includes SpecialKnockdownState
- Valid transitions configured
- TriggerSpecialKnockdown() transitions to correct state

**Commit Message Template:**
```
feat(knockdown): integrate SpecialKnockdownState into state machine

- Register state and configure transitions
- Add public trigger method for future special moves
```

**Estimated Tokens:** ~3,000

---

### Task 9: Create Default StaminaData Asset

**Goal:** Create a default stamina configuration asset for testing and as template.

**Files to Create:**
- `Assets/Knockout/Data/Stamina/DefaultStamina.asset` - ScriptableObject instance

**Prerequisites:**
- Task 1 complete (StaminaData ScriptableObject exists)

**Implementation Steps:**

1. In Unity Editor: Create > Knockout > Stamina Data
2. Name: "DefaultStamina"
3. Configure with balanced default values from DD-001:
   - Max Stamina: 100
   - Regen Per Second: 25
   - Attack Costs: [10 (Jab), 15 (Hook), 20 (Uppercut)]
   - Special Move Cost: 40
   - Exhaustion Duration: 2.0s
   - Exhaustion Regen Multiplier: 0.5
   - Exhaustion Recovery Threshold: 25
4. Save in `Assets/Knockout/Data/Stamina/` directory (create if needed)
5. Document in asset description field (if exists)

**Design Guidance:**
- This asset serves as template for character-specific customization
- Values match design document DD-001 balanced tuning

**Verification Checklist:**
- [ ] Asset created successfully
- [ ] Values match design spec defaults
- [ ] Asset can be assigned to CharacterStamina component

**Testing Instructions:**

Manual testing:
- Assign DefaultStamina to character in scene
- Enter play mode
- Verify stamina values display correctly in Inspector (debug view)

**Commit Message Template:**
```
feat(stamina): add default StaminaData asset

- Configured with balanced default values
- Template for character-specific customization
```

**Estimated Tokens:** ~2,000

---

### Task 10: Update Character Prefab with Stamina Components

**Goal:** Add stamina system to base character prefab for testing.

**Files to Modify:**
- `Assets/Knockout/Prefabs/Characters/BaseCharacter.prefab` - Add components

**Prerequisites:**
- Task 3 complete (CharacterStamina component exists)
- Task 9 complete (DefaultStamina asset exists)

**Implementation Steps:**

1. Open BaseCharacter prefab
2. Add `CharacterStamina` component
3. Assign dependencies:
   - Character Controller (auto-find or manual)
   - Stamina Data: DefaultStamina asset
   - Character Combat (auto-find or manual)
4. Ensure component initialization order:
   - CharacterStamina initializes after CharacterCombat
   - Update CharacterController if it manages initialization order
5. Test in play mode

**Design Guidance:**
- Follow existing component setup pattern in prefab
- CharacterController should initialize all components

**Verification Checklist:**
- [ ] CharacterStamina component added to prefab
- [ ] Dependencies assigned correctly
- [ ] Component initializes without errors in play mode
- [ ] Stamina system functional in gameplay test

**Testing Instructions:**

Manual testing:
- Open scene with character
- Enter play mode
- Perform attacks, observe stamina depletion
- Wait idle, observe stamina regeneration
- Deplete stamina fully, verify exhaustion state

**Commit Message Template:**
```
feat(stamina): add stamina components to character prefab

- Add CharacterStamina component
- Assign DefaultStamina configuration
- Integrate into component initialization
```

**Estimated Tokens:** ~3,000

---

### Task 11: Add Stamina Regeneration Modifier System

**Goal:** Allow CharacterStamina to modify regeneration rate based on character state (e.g., exhaustion).

**Files to Modify:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterStamina.cs` - Add regen modifier

**Prerequisites:**
- Task 3 complete (CharacterStamina exists)
- Task 5 complete (ExhaustedState exists)

**Implementation Steps:**

1. Add private field `regenRateMultiplier` (default 1.0)
2. Modify regeneration calculation in `FixedUpdate()`:
   - `regenAmount = StaminaData.RegenPerSecond * regenRateMultiplier * Time.fixedDeltaTime`
3. Add public method `SetRegenMultiplier(float multiplier)`:
   - Allows external systems to modify regen rate
   - Clamp multiplier to [0, 2] range (reasonable bounds)
4. Subscribe to ExhaustedState enter/exit events (or query CombatStateMachine state):
   - On enter exhaustion: `SetRegenMultiplier(StaminaData.ExhaustionRegenMultiplier)`
   - On exit exhaustion: `SetRegenMultiplier(1.0)`
5. Add public method `ResetRegenMultiplier()` to restore default

**Design Guidance:**
- Modifier system allows future extensions (buffs, debuffs, special states)
- Multiple modifiers could be additive or multiplicative (start with single multiplier)
- Exhaustion applies via this system

**Verification Checklist:**
- [ ] Regeneration rate modifiable at runtime
- [ ] Exhaustion applies slower regen multiplier
- [ ] Exiting exhaustion restores normal regen
- [ ] Multiplier clamped to valid range

**Testing Instructions:**

Update PlayMode test: `Assets/Knockout/Tests/PlayMode/Stamina/CharacterStaminaTests.cs`

Test cases:
- SetRegenMultiplier affects regeneration rate
- Exhaustion state reduces regen rate
- Exiting exhaustion restores normal regen
- Multiplier clamped to [0, 2]

**Commit Message Template:**
```
feat(stamina): add regeneration rate modifier system

- Support dynamic regen rate adjustments
- Apply slower regen during exhaustion
- Extensible for future buffs/debuffs
```

**Estimated Tokens:** ~4,000

---

### Task 12: Comprehensive Integration Testing

**Goal:** Verify all Phase 1 systems work together correctly in gameplay scenarios.

**Files to Create:**
- `Assets/Knockout/Tests/PlayMode/Integration/StaminaIntegrationTests.cs` - Integration tests

**Prerequisites:**
- All previous tasks complete

**Implementation Steps:**

1. Create integration test suite covering end-to-end stamina flow:
   - Test: Full stamina depletion and recovery cycle
   - Test: Attack spam until exhaustion
   - Test: Exhaustion prevents attacks but allows blocking
   - Test: Stamina regeneration during idle
   - Test: No stamina regen during attack
   - Test: Attack costs vary by attack type
   - Test: Special knockdown state triggered (manually for now)
2. Test multi-character scenarios:
   - Both characters have independent stamina
   - Stamina states don't interfere
3. Test edge cases:
   - Stamina exactly at attack cost (attack succeeds)
   - Stamina 0.1 below attack cost (attack fails)
   - Hitting character during exhaustion
   - Rapid state transitions

**Design Guidance:**
- Use UnityTest coroutines for time-based tests
- Test realistic gameplay flows, not just isolated component behavior
- Verify events fire in correct order

**Verification Checklist:**
- [ ] All integration tests pass
- [ ] Stamina system functional in gameplay
- [ ] No errors or warnings in console
- [ ] Performance acceptable (60fps maintained)

**Testing Instructions:**

Run all tests:
- EditMode tests (Window > General > Test Runner > EditMode)
- PlayMode tests (Window > General > Test Runner > PlayMode)
- All tests green

**Commit Message Template:**
```
test(stamina): add comprehensive integration tests

- Test end-to-end stamina flow
- Verify exhaustion mechanics
- Test multi-character scenarios
- Cover edge cases and state transitions
```

**Estimated Tokens:** ~10,000

---

### Task 13: Documentation and Code Cleanup

**Goal:** Document stamina system, clean up debug code, finalize Phase 1.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/STAMINA_SYSTEM.md` - System documentation

**Files to Modify:**
- All Phase 1 scripts - Remove debug logs, finalize comments

**Prerequisites:**
- All previous tasks complete
- All tests passing

**Implementation Steps:**

1. Create STAMINA_SYSTEM.md documentation:
   - System overview
   - How to configure stamina for characters
   - How to customize attack costs
   - How exhaustion works
   - Integration points for other systems
   - Troubleshooting common issues
2. Review all Phase 1 scripts:
   - Remove `Debug.Log` statements used during development
   - Ensure XML comments on all public methods/properties
   - Verify naming conventions followed
   - Check for unused code/fields
3. Update main README.md with stamina system summary (if project has one)

**Design Guidance:**
- Documentation should help future developers extend or debug the system
- Include examples of common configurations
- Link to relevant design decisions from Phase 0

**Verification Checklist:**
- [ ] STAMINA_SYSTEM.md created and comprehensive
- [ ] All debug logs removed
- [ ] XML comments complete
- [ ] No compiler warnings
- [ ] Code follows project style guide

**Testing Instructions:**

Final checks:
- Build project (verify no build errors)
- Run all tests (verify all pass)
- Review code in pull request format

**Commit Message Template:**
```
docs(stamina): add system documentation and cleanup

- Create STAMINA_SYSTEM.md guide
- Remove debug logging
- Finalize XML documentation
- Code cleanup and polish
```

**Estimated Tokens:** ~5,000

---

---

## Review Feedback (Iteration 1)

### Task 9: Default StaminaData Asset

> **Consider:** Running `ls -la Assets/Knockout/Data/Stamina/` shows the directory doesn't exist. Did Task 9 get completed?
>
> **Think about:** Where would a character get its stamina configuration if no default asset exists? Would the game break in Unity Editor when trying to assign stamina data to a character?
>
> **Reflect:** Task 9 specifies "Create DefaultStamina.asset in Assets/Knockout/Data/Stamina/ (create directory if needed)". What command would you use to verify this asset exists?

### Task 10: Character Prefab Integration

> **Consider:** The plan requires adding CharacterStamina component to BaseCharacter.prefab. How can you verify this was done?
>
> **Think about:** Running `find Assets/Knockout/Prefabs -name "*.prefab"` returns no results. Does the Prefabs directory exist? Should you check if the prefab is in a different location?
>
> **Reflect:** If you open Unity Editor and inspect BaseCharacter prefab, would you see the CharacterStamina component with DefaultStamina assigned? How would you verify this without opening Unity?

### Excellent Work

The core implementation is outstanding:
- ✓ All scripts well-written (verified with Read tool)
- ✓ Comprehensive tests (210 lines in StaminaDataTests.cs alone)
- ✓ Documentation exists (STAMINA_SYSTEM.md)
- ✓ Conventional commits followed (verified with `git log`)
- ✓ Proper validation and error handling

**Next Steps:** Address the asset creation and prefab integration questions above, then this phase will be ready for approval.

---

## Phase 1 Verification

**Completion Checklist:**
- [ ] All 13 tasks completed
- [ ] All tests passing (EditMode + PlayMode)
- [ ] Stamina system functional in gameplay
- [ ] Exhaustion state working correctly
- [ ] SpecialKnockdownState integrated (ready for Phase 4)
- [ ] Character prefab updated with stamina components
- [ ] Documentation complete
- [ ] Code reviewed and cleaned
- [ ] No console errors or warnings
- [ ] Performance target maintained (60fps, Profiler shows CharacterStamina < 0.1ms per frame, no GC allocations during gameplay)

**Integration Points for Next Phase:**
- `CharacterStamina` component available for other systems to query/modify
- `OnStaminaChanged` event available for UI (Phase 5)
- `HasStamina(cost)` method available for special moves (Phase 4)
- `ExhaustedState` integrated into combat flow
- `SpecialKnockdownState` ready for special move hits (Phase 4)

**Known Limitations:**
- Stamina UI not implemented (Phase 5)
- Special moves don't consume stamina yet (Phase 4)
- AI doesn't consider stamina in decisions (post-Phase 5, AI enhancement)
- No stamina recovery on round end (consider for future balancing)

---

## Next Phase

After Phase 1 completion and verification, proceed to:

**[Phase 2: Advanced Defense](Phase-2.md)**

Implement dodging with i-frames and timed parry system.
