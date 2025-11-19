# Phase 2: Advanced Defense

## Phase Goal

Implement advanced defensive mechanics: directional dodging with invincibility frames (i-frames) and timed parry system. These systems add skill-based defensive options beyond basic blocking, rewarding precise timing and creating counterattack opportunities.

**Success Criteria:**
- Directional dodge system (left, right, back) with i-frame invincibility window
- Timed parry system that negates damage and staggers attacker
- Dodge and parry animations integrated
- Existing block system unchanged and coexists with new mechanics
- All systems configurable via ScriptableObjects
- 85%+ test coverage for new components

**Estimated Tokens:** ~100,000

---

## Prerequisites

**Required Reading:**
- [Phase 0: Foundation](Phase-0.md) - Architecture decisions and patterns
- [Phase 1: Core Resource Systems](Phase-1.md) - Stamina system (dodge/parry are stamina-free)

**Existing Systems to Understand:**
- `CharacterInput` - Input handling (Assets/Knockout/Scripts/Characters/Components/CharacterInput.cs)
- `CharacterMovement` - Movement system (Assets/Knockout/Scripts/Characters/Components/CharacterMovement.cs)
- `CombatStateMachine` - State management (Assets/Knockout/Scripts/Combat/CombatStateMachine.cs)
- `BlockingState` - Existing defensive state (Assets/Knockout/Scripts/Combat/States/BlockingState.cs)
- Hit detection system (Assets/Knockout/Scripts/Combat/HitDetection/)

**Available Assets:**
- Dodge animations: `left_dodge.fbx`, `right_dodge.fbx` (in `Assets/Knockout/Animations/Characters/BaseCharacter/AnimationClips/`)
- Block animations: `Block.fbx`, `Idle_to_Block.fbx` (existing, will be used for parry timing)

**Environment:**
- Unity 2021.3.8f1 LTS
- Input System 1.4.4

---

## Tasks

### Task 1: Create DodgeData ScriptableObject

**Goal:** Define dodge configuration data including i-frame timing and movement properties.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Data/DodgeData.cs` - ScriptableObject class

**Prerequisites:**
- Review `StaminaData.cs` for ScriptableObject pattern
- Understand frame-based timing from ADR-004

**Implementation Steps:**

1. Create `DodgeData` class in `Knockout.Characters.Data` namespace
2. Add `[CreateAssetMenu]` attribute with menu path `"Knockout/Dodge Data"` (order = 4)
3. Define serialized fields:
   - Dodge duration frames (total animation length)
   - i-frame start frame (when invincibility begins)
   - i-frame duration frames (how long invincibility lasts)
   - Dodge distance (how far character moves)
   - Dodge speed multiplier (movement speed during dodge)
   - Cooldown frames (optional: prevent dodge spam)
4. Expose public read-only properties
5. Implement `OnValidate()` to ensure i-frame window fits within dodge duration
6. Add XML documentation and tooltips
7. Add helper properties: `DodgeDuration` (frames to seconds), `IFrameStartTime`, `IFrameDuration`

**Design Guidance:**
- Follow DD-003 (Dodge & Parry Design) from Phase 0
- Default values:
  - Total duration: 18 frames (~0.3s at 60fps)
  - i-frame start: 2 frames (almost immediate)
  - i-frame duration: 8 frames (~0.133s, 40% of total)
  - Dodge distance: 1.5 units
  - Cooldown: 12 frames (~0.2s to prevent spam)
- i-frames are early in dodge (first 40%), vulnerable during recovery

**Verification Checklist:**
- [ ] ScriptableObject appears in Create menu
- [ ] OnValidate ensures i-frame window valid (start + duration <= total duration)
- [ ] Frame-to-second conversions correct (60fps)
- [ ] All fields have tooltips and XML docs

**Testing Instructions:**

Create EditMode test: `Assets/Knockout/Tests/EditMode/Defense/DodgeDataTests.cs`

Test cases:
- ScriptableObject creation succeeds
- OnValidate clamps i-frame start to valid range
- OnValidate prevents i-frame duration exceeding total duration
- Frame-to-second conversion correct (18 frames = 0.3s)
- Default values match design spec

**Commit Message Template:**
```
feat(defense): add DodgeData ScriptableObject

- Define dodge timing and movement properties
- Frame-based timing for i-frame precision
- Validation ensures consistent configuration
```

**Estimated Tokens:** ~3,000

---

### Task 2: Extend Input System for Dodge Actions

**Goal:** Add dodge input actions (left, right, back) to Input System.

**Files to Modify:**
- `Assets/Knockout/Input/KnockoutInputActions.inputactions` - Add dodge actions (if using .inputactions asset)
- OR `Assets/Knockout/Scripts/Input/` - Generate dodge input bindings

**Prerequisites:**
- Review existing input setup (check `Assets/Knockout/Scripts/Input/INPUT_SYSTEM_SETUP.md`)
- Understand current input binding structure (likely using InputActions asset)

**Implementation Steps:**

1. Open KnockoutInputActions asset (or equivalent)
2. Add new action map or extend existing "Gameplay" map:
   - Action: `DodgeLeft` (button binding, e.g., Q or Left Arrow)
   - Action: `DodgeRight` (button binding, e.g., E or Right Arrow)
   - Action: `DodgeBack` (button binding, e.g., S or Down Arrow)
3. Configure as button actions (Press interaction)
4. Regenerate C# input class if using auto-generation
5. Update `CharacterInput` to expose dodge input events or properties

**Design Guidance:**
- Dodge is directional, not just generic "dodge" button
- Consider gamepad mapping (left stick + dodge button combo, or face buttons)
- PC defaults: Q (left), E (right), S (back) - or customize based on existing scheme
- Input should be responsive (no hold requirement, instant on press)

**Verification Checklist:**
- [ ] Dodge actions added to Input Actions asset
- [ ] Bindings configured for keyboard
- [ ] (Optional) Bindings configured for gamepad
- [ ] C# input class regenerated (if applicable)
- [ ] No conflicts with existing inputs

**Testing Instructions:**

Manual testing in Unity Editor:
- Open Input Actions asset
- Verify dodge actions present
- Test bindings in play mode with Input Debugger

**Commit Message Template:**
```
feat(defense): add dodge input actions

- Add DodgeLeft, DodgeRight, DodgeBack actions
- Configure keyboard bindings
- Integrate into Input System
```

**Estimated Tokens:** ~3,000

---

### Task 3: Update CharacterInput for Dodge Inputs

**Goal:** Expose dodge input events in CharacterInput component for other systems to consume.

**Files to Modify:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterInput.cs` - Add dodge events

**Prerequisites:**
- Task 2 complete (dodge input actions exist)
- Review `CharacterInput.cs` structure

**Implementation Steps:**

1. Add public events for dodge inputs:
   - `public event Action OnDodgeLeftInput;`
   - `public event Action OnDodgeRightInput;`
   - `public event Action OnDodgeBackInput;`
2. In input callback handlers (or Update/FixedUpdate if polling):
   - Detect dodge input presses
   - Fire corresponding events
3. Ensure dodge inputs only fire when character can act (not knocked down, not in special state)
4. Add input buffering if existing system supports it (optional, nice-to-have)

**Design Guidance:**
- Follow existing input event pattern (like OnAttackInput, OnBlockInput)
- Dodge events fire on button press, not hold
- Input should be responsive (immediate feedback)

**Verification Checklist:**
- [ ] Dodge events fire on input
- [ ] Events don't fire when character incapacitated
- [ ] No input conflicts with existing actions
- [ ] Events fire only once per press (no spam)

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/Characters/CharacterInputTests.cs` (update existing)

Test cases:
- OnDodgeLeftInput fires when left dodge pressed
- OnDodgeRightInput fires when right dodge pressed
- OnDodgeBackInput fires when back dodge pressed
- Dodge inputs ignored when knocked down

**Commit Message Template:**
```
feat(defense): expose dodge input events in CharacterInput

- Add OnDodgeLeft/Right/Back events
- Fire on button press
- Integrate with existing input handling
```

**Estimated Tokens:** ~4,000

---

### Task 4: Create DodgingState Combat State

**Goal:** Implement dodge state with i-frame invincibility and directional movement.

**Files to Create:**
- `Assets/Knockout/Scripts/Combat/States/DodgingState.cs` - Combat state class

**Prerequisites:**
- Task 1 complete (DodgeData exists)
- Review existing combat states for pattern
- Understand hit detection system (how to make character invulnerable)

**Implementation Steps:**

1. Create `DodgingState` inheriting from `CombatState`
2. Implement state lifecycle:
   - `OnEnter(DodgeDirection direction)`: Store dodge direction, trigger animation, start i-frame timer, fire `OnDodgeStarted` event
   - `OnUpdate()`: Track dodge timer (frame count)
   - `OnFixedUpdate()`: Apply dodge movement (directional), check for dodge completion
   - `OnExit()`: Fire `OnDodgeEnded` event, restore hurtbox vulnerability
3. Implement i-frame invincibility:
   - Track current frame in dodge animation
   - During i-frame window (start frame to start + duration), disable hurtbox or set invulnerable flag
   - Outside i-frame window, character is vulnerable
   - Communicate with hit detection system (likely via public property `IsInvulnerable`)
4. Implement directional movement:
   - Calculate movement direction based on dodge direction and character facing
   - Apply movement impulse or velocity override
   - Move character `DodgeData.DodgeDistance` over dodge duration
5. Auto-transition to `IdleState` when dodge completes (frame count >= total duration)
6. Create enum `DodgeDirection { Left, Right, Back }` for direction parameter

**Design Guidance:**
- Follow ADR-003 (State Machine Integration)
- Follow ADR-004 (Frame-Perfect Timing)
- i-frames are a property check, not a separate collider state
- Movement should feel snappy (front-loaded speed curve, or constant speed)
- Animation plays for full duration, movement may complete earlier

**Verification Checklist:**
- [ ] Dodge state entered successfully
- [ ] Dodge animation plays
- [ ] Character moves in correct direction
- [ ] i-frame invincibility functional (verified via hit test)
- [ ] Auto-transitions to IdleState on completion
- [ ] Events fire correctly

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/Defense/DodgingStateTests.cs`

Test cases:
- Entering DodgingState triggers animation
- IsInvulnerable true during i-frame window
- IsInvulnerable false outside i-frame window
- Character moves in correct direction
- State transitions to IdleState after duration
- OnDodgeStarted and OnDodgeEnded events fire

**Commit Message Template:**
```
feat(defense): implement DodgingState with i-frames

- Directional dodge movement (left/right/back)
- i-frame invincibility window
- Frame-based timing for precision
- Auto-transition to IdleState on completion
```

**Estimated Tokens:** ~10,000

---

### Task 5: Create CharacterDodge Component

**Goal:** Manage dodge execution, cooldown, and orchestrate dodge state transitions.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterDodge.cs` - Component class

**Prerequisites:**
- Task 1 complete (DodgeData exists)
- Task 3 complete (CharacterInput dodge events)
- Task 4 complete (DodgingState exists)

**Implementation Steps:**

1. Create `CharacterDodge` component in `Knockout.Characters.Components` namespace
2. Implement component lifecycle pattern:
   - Dependencies: `CharacterController`, `DodgeData`, `CharacterInput`, `CombatStateMachine`
   - `Initialize()` method
   - Subscribe to dodge input events in `Initialize()`
3. Implement dodge execution:
   - On dodge input event, check if dodge available (not on cooldown, character in valid state)
   - Trigger `CombatStateMachine` transition to `DodgingState(direction)`
   - Start cooldown timer
4. Implement cooldown tracking:
   - Track frames since last dodge
   - Property `CanDodge` returns true if cooldown expired and state allows
   - Fire `OnDodgeReady` event when cooldown completes (for UI feedback)
5. Add public method `TryDodge(DodgeDirection direction)` for manual triggering (AI can use)
6. Expose properties: `IsDodging`, `CanDodge`, `CooldownProgress` (0-1 for UI)

**Design Guidance:**
- Dodge doesn't consume stamina (defensive action is free)
- Cooldown prevents dodge spam (small cooldown, ~0.2s)
- Cannot dodge while attacking, knocked down, or already dodging

**Verification Checklist:**
- [ ] Component initializes correctly
- [ ] Dodge triggers on input
- [ ] Cooldown prevents spam
- [ ] Cannot dodge in invalid states
- [ ] Events fire correctly

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/Defense/CharacterDodgeTests.cs`

Test cases:
- TryDodge succeeds when available
- TryDodge fails when on cooldown
- TryDodge fails when in invalid state (attacking)
- Cooldown expires after DodgeData.CooldownFrames
- OnDodgeReady event fires when cooldown completes

**Commit Message Template:**
```
feat(defense): implement CharacterDodge component

- Handle dodge execution and cooldown
- Subscribe to input events
- Trigger DodgingState transitions
- Expose dodge availability for UI
```

**Estimated Tokens:** ~8,000

---

### Task 6: Integrate Dodge with Hit Detection System

**Goal:** Make characters invulnerable during i-frame window by integrating with hit detection.

**Files to Modify:**
- `Assets/Knockout/Scripts/Combat/HitDetection/HurtboxData.cs` - Add invulnerability check
- OR `Assets/Knockout/Scripts/Combat/HitDetection/` - Modify hit detection logic

**Prerequisites:**
- Task 4 complete (DodgingState with IsInvulnerable property)
- Understand existing hit detection system (read HurtboxData.cs, HitboxData.cs, HitData.cs)

**Implementation Steps:**

1. **Locate hit detection code:**
   - Search for damage application: `Grep pattern: "ApplyDamage|ProcessHit|OnHitReceived" in Assets/Knockout/Scripts/Combat/HitDetection/`
   - Expected files: `HurtboxData.cs` (likely contains damage processing), `HitData.cs`
   - Look for method where damage is calculated and applied to character
   - Typical flow: Hitbox triggers → Hurtbox detects → Damage applied in processing method
2. Add invulnerability check to hurtbox hit processing:
   - Query character's current state (via CombatStateMachine or DodgingState directly)
   - If `IsInvulnerable == true`, ignore hit (return early, no damage applied)
   - Optionally fire `OnHitDodged` event for feedback (VFX, sound)
3. Ensure invulnerability only applies during i-frame window:
   - `DodgingState` manages `IsInvulnerable` property based on frame count
   - Outside i-frame window, vulnerable even while dodging
4. Consider edge cases:
   - Multi-hit attacks during dodge
   - Projectiles (if any) during i-frames
   - Grabs or special attacks (respect i-frames)

**Design Guidance:**
- Invulnerability is binary: either invulnerable or not (no partial damage)
- i-frames should be clear and consistent (not random)
- Feedback important: player needs to know dodge succeeded (visual/audio cue)

**Verification Checklist:**
- [ ] Hits ignored during i-frame window
- [ ] Hits connect outside i-frame window
- [ ] OnHitDodged event fires when hit dodged
- [ ] No damage applied during i-frames
- [ ] Hitbox/hurtbox interaction remains stable

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/Defense/DodgeInvulnerabilityTests.cs`

Test cases:
- Hit connects when not dodging
- Hit ignored during i-frame window
- Hit connects during dodge recovery (after i-frames)
- OnHitDodged event fires when hit dodged

**Commit Message Template:**
```
feat(defense): integrate dodge i-frames with hit detection

- Add invulnerability check to hurtbox processing
- Ignore hits during i-frame window
- Fire OnHitDodged event for feedback
```

**Estimated Tokens:** ~6,000

---

### Task 7: Integrate DodgingState into CombatStateMachine

**Goal:** Wire DodgingState into state machine transition graph.

**Files to Modify:**
- `Assets/Knockout/Scripts/Combat/CombatStateMachine.cs` - Add state and transitions

**Prerequisites:**
- Task 4 complete (DodgingState exists)
- Review existing state machine transitions

**Implementation Steps:**

1. Register `DodgingState` in state machine initialization
2. Add method `TriggerDodge(DodgeDirection direction)` to transition to DodgingState with direction parameter
3. Configure valid transitions:
   - From IdleState → DodgingState
   - From BlockingState → DodgingState (can dodge while blocking)
   - From DodgingState → IdleState (on completion)
   - From DodgingState → HitStunnedState (if hit during recovery, outside i-frames)
   - Cannot transition to DodgingState from AttackingState, KnockedDownState, ExhaustedState
4. Ensure state priority: dodge input during attack is buffered or ignored (don't interrupt attack)

**Design Guidance:**
- Dodge is high-priority defensive action but doesn't interrupt offense
- Can cancel block into dodge for repositioning
- State machine validates transitions (CanTransitionTo)

**Verification Checklist:**
- [ ] DodgingState registered
- [ ] TriggerDodge method functional
- [ ] Valid transitions configured
- [ ] Invalid transitions blocked
- [ ] State machine stable

**Testing Instructions:**

Update PlayMode test: `Assets/Knockout/Tests/PlayMode/Combat/CombatStateMachineTests.cs`

Test cases:
- TriggerDodge transitions to DodgingState from IdleState
- Cannot transition to DodgingState from AttackingState
- DodgingState transitions to IdleState on completion

**Commit Message Template:**
```
feat(defense): integrate DodgingState into CombatStateMachine

- Register state and configure transitions
- Add TriggerDodge method with direction parameter
- Enforce transition rules
```

**Estimated Tokens:** ~5,000

---

### Task 8: Create ParryData ScriptableObject

**Goal:** Define parry timing configuration for timed perfect block.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Data/ParryData.cs` - ScriptableObject class

**Prerequisites:**
- Review `DodgeData.cs` for ScriptableObject pattern
- Understand parry mechanics from DD-003

**Implementation Steps:**

1. Create `ParryData` class in `Knockout.Characters.Data` namespace
2. Add `[CreateAssetMenu]` attribute with menu path `"Knockout/Parry Data"` (order = 5)
3. Define serialized fields:
   - Parry window frames (how early before hit player can parry, e.g., 6 frames = 0.1s)
   - Parry success duration frames (how long parry state lasts after successful parry)
   - Attacker stagger duration seconds (how long attacker is vulnerable)
   - Counter window duration seconds (how long defender has to counter)
   - Parry cooldown frames (prevent parry spam)
4. Expose public read-only properties
5. Implement `OnValidate()` for range validation
6. Add XML documentation and tooltips

**Design Guidance:**
- Follow DD-003 (Dodge & Parry Design)
- Default values:
  - Parry window: 6 frames (~0.1s before hit)
  - Parry success duration: 12 frames (~0.2s)
  - Attacker stagger: 0.5s
  - Counter window: 0.5s (same as stagger)
  - Cooldown: 18 frames (~0.3s)
- Parry is stricter timing than dodge (smaller window)

**Verification Checklist:**
- [ ] ScriptableObject created successfully
- [ ] OnValidate ensures valid ranges
- [ ] Frame-to-second conversions correct
- [ ] All fields documented

**Testing Instructions:**

Create EditMode test: `Assets/Knockout/Tests/EditMode/Defense/ParryDataTests.cs`

Test cases:
- ScriptableObject creation succeeds
- OnValidate clamps values to valid ranges
- Frame conversion correct
- Default values match design spec

**Commit Message Template:**
```
feat(defense): add ParryData ScriptableObject

- Define parry timing configuration
- Frame-based precision for timing window
- Configure stagger and counter window durations
```

**Estimated Tokens:** ~3,000

---

### Task 9: Create ParryStaggerState Combat State

**Goal:** Implement attacker stagger state triggered by successful parry.

**Files to Create:**
- `Assets/Knockout/Scripts/Combat/States/ParryStaggerState.cs` - Combat state class

**Prerequisites:**
- Task 8 complete (ParryData exists)
- Review existing HitStunnedState for stun pattern

**Implementation Steps:**

1. Create `ParryStaggerState` inheriting from `CombatState`
2. Implement state lifecycle:
   - `OnEnter(float duration)`: Trigger stagger animation, start timer, fire `OnParryStaggered` event
   - `OnUpdate()`: Track stagger duration
   - `OnExit()`: Fire `OnParryStaggerEnded` event
3. Prevent all actions during stagger:
   - Cannot attack, block, dodge
   - Can be hit (vulnerable state)
4. Auto-transition to `IdleState` when stagger duration expires
5. Animation: reuse hit reaction animation or create subtle stagger animation (can use existing hit animation)

**Design Guidance:**
- Stagger is a punish state (attacker is vulnerable)
- Similar to HitStunnedState but triggered by parry, not hit
- Duration configurable via ParryData

**Verification Checklist:**
- [ ] State entered successfully
- [ ] Stagger animation plays
- [ ] Cannot perform actions during stagger
- [ ] Auto-transitions to IdleState after duration
- [ ] Events fire correctly

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/Defense/ParryStaggerStateTests.cs`

Test cases:
- State enters and triggers animation
- Duration timer expires and transitions to IdleState
- Cannot attack during stagger
- OnParryStaggered event fires

**Commit Message Template:**
```
feat(defense): implement ParryStaggerState

- Attacker vulnerable state after parry
- Prevent actions during stagger
- Configurable duration via ParryData
```

**Estimated Tokens:** ~5,000

---

### Task 10: Create CharacterParry Component

**Goal:** Detect parry timing, trigger parry success, and manage parry cooldown.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterParry.cs` - Component class

**Prerequisites:**
- Task 8 complete (ParryData exists)
- Task 9 complete (ParryStaggerState exists)
- Understand block input from CharacterInput
- Understand hit detection system

**Implementation Steps:**

1. Create `CharacterParry` component in `Knockout.Characters.Components` namespace
2. Implement component lifecycle pattern:
   - Dependencies: `CharacterController`, `ParryData`, `CharacterInput`, `CombatStateMachine`
   - Subscribe to block input events
3. Implement parry timing detection:
   - Track when block button pressed (timestamp or frame count)
   - **Integrate with hit detection:** This will be handled in Task 11 (integration with hit detection)
     - Search for hit detection events: `Grep pattern: "OnHit|BeforeHit" in HurtboxData or CharacterCombat`
     - Look for: timing when hit is about to land (before damage applied)
     - Add parry check in hit processing: if block pressed within parry window, trigger parry
     - Check if block pressed within parry window (last N frames before hit)
     - If yes: trigger parry success
     - If no: normal block occurs
4. Implement parry success:
   - Negate incoming hit damage
   - Trigger attacker's `ParryStaggerState` (via attacker's CombatStateMachine)
   - Start counter window timer (defender can attack with bonus)
   - Fire `OnParrySuccess` event
   - Start parry cooldown
5. Implement parry cooldown (prevent spam)
6. Expose properties: `CanParry`, `IsInCounterWindow`, `CooldownProgress`

**Design Guidance:**
- Parry is timing-based: block input must be within parry window before hit
- Holding block = normal block (no parry)
- Tapping block just before hit = parry
- Implementation may require hit detection system integration (predict incoming hit timing)
- Counter window gives defender opportunity, not auto-counter (defender still inputs attack)

**Verification Checklist:**
- [ ] Component initializes correctly
- [ ] Parry detects correct timing
- [ ] Parry negates damage
- [ ] Attacker gets staggered
- [ ] Counter window tracked
- [ ] Cooldown prevents spam
- [ ] Events fire correctly

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/Defense/CharacterParryTests.cs`

Test cases:
- Block input within parry window triggers parry
- Block input outside parry window = normal block
- Parry negates damage
- Parry staggers attacker
- Counter window opens after parry
- Cooldown prevents consecutive parries

**Commit Message Template:**
```
feat(defense): implement CharacterParry component

- Detect parry timing window
- Trigger parry success and stagger attacker
- Manage counter window and cooldown
- Fire events for UI/feedback
```

**Estimated Tokens:** ~10,000

---

### Task 11: Integrate Parry with Hit Detection System

**Goal:** Allow CharacterParry to intercept hits and trigger parry logic.

**Files to Modify:**
- `Assets/Knockout/Scripts/Combat/HitDetection/HurtboxData.cs` - Add parry check
- OR hit processing logic

**Prerequisites:**
- Task 10 complete (CharacterParry exists)
- Understand hit detection flow (when hit registers)

**Implementation Steps:**

1. In hit detection processing (before damage applied):
   - Check if character has `CharacterParry` component
   - Call `characterParry.TryParry(attackData, attacker)` before applying damage
   - If parry successful (returns true):
     - Do NOT apply damage
     - Trigger attacker's ParryStaggerState
     - Fire feedback events
     - Return early (hit negated)
   - If parry failed (returns false):
     - Proceed with normal block or hit logic
2. Implement `TryParry` method in CharacterParry:
   - Check if parry available (timing window, cooldown)
   - If successful, execute parry logic
   - Return success boolean
3. Ensure parry takes priority over normal block (if both possible, parry wins)

**Design Guidance:**
- Parry check happens early in hit processing pipeline
- Parry is all-or-nothing: either fully negates or doesn't apply
- Attacker stagger is triggered immediately on parry success

**Verification Checklist:**
- [ ] Parry check integrated into hit detection
- [ ] Successful parry negates damage
- [ ] Failed parry allows normal block/hit
- [ ] Attacker staggers on parry
- [ ] No damage edge cases (parry + hit simultaneously)

**Testing Instructions:**

Update PlayMode test: `Assets/Knockout/Tests/PlayMode/Defense/CharacterParryTests.cs`

Test cases:
- Parry within window negates damage
- Parry triggers attacker stagger
- Parry outside window = normal block
- Multiple parry attempts respect cooldown

**Commit Message Template:**
```
feat(defense): integrate parry with hit detection

- Add parry check before damage application
- Negate damage on successful parry
- Trigger attacker stagger state
```

**Estimated Tokens:** ~6,000

---

### Task 12: Update Character Prefab with Defense Components

**Goal:** Add dodge and parry components to character prefab.

**Files to Modify:**
- `Assets/Knockout/Prefabs/Characters/BaseCharacter.prefab` - Add components

**Prerequisites:**
- Task 5 complete (CharacterDodge exists)
- Task 10 complete (CharacterParry exists)
- Default data assets created (next task)

**Implementation Steps:**

1. Open BaseCharacter prefab
2. Add `CharacterDodge` component:
   - Assign dependencies (auto-find where possible)
   - Assign DodgeData asset
3. Add `CharacterParry` component:
   - Assign dependencies
   - Assign ParryData asset
4. Verify component initialization order in CharacterController
5. Test in play mode

**Design Guidance:**
- Follow existing component setup pattern
- Components initialize after CharacterInput and CombatStateMachine

**Verification Checklist:**
- [ ] Components added to prefab
- [ ] Dependencies assigned
- [ ] Components initialize without errors
- [ ] Dodge functional in play mode
- [ ] Parry functional in play mode

**Testing Instructions:**

Manual testing:
- Enter play mode
- Test dodge inputs (left, right, back)
- Test parry timing (block just before hit)
- Verify i-frames work (dodge through attack)
- Verify parry staggers opponent

**Commit Message Template:**
```
feat(defense): add dodge and parry to character prefab

- Add CharacterDodge component
- Add CharacterParry component
- Assign default configuration assets
```

**Estimated Tokens:** ~3,000

---

### Task 13: Create Default Defense Data Assets

**Goal:** Create default dodge and parry configuration assets.

**Files to Create:**
- `Assets/Knockout/Data/Defense/DefaultDodge.asset` - DodgeData instance
- `Assets/Knockout/Data/Defense/DefaultParry.asset` - ParryData instance

**Prerequisites:**
- Task 1 complete (DodgeData ScriptableObject)
- Task 8 complete (ParryData ScriptableObject)

**Implementation Steps:**

1. Create DefaultDodge asset:
   - Total duration: 18 frames
   - i-frame start: 2 frames
   - i-frame duration: 8 frames
   - Dodge distance: 1.5 units
   - Cooldown: 12 frames
2. Create DefaultParry asset:
   - Parry window: 6 frames
   - Parry success duration: 12 frames
   - Attacker stagger: 0.5s
   - Counter window: 0.5s
   - Cooldown: 18 frames
3. Save in `Assets/Knockout/Data/Defense/` directory

**Design Guidance:**
- Values match design spec from DD-003
- These serve as templates for character-specific customization

**Verification Checklist:**
- [ ] Assets created successfully
- [ ] Values match design spec
- [ ] Assets assignable to components

**Testing Instructions:**

Manual verification:
- Assets appear in project
- Values display correctly in Inspector
- No validation errors

**Commit Message Template:**
```
feat(defense): add default dodge and parry data assets

- Create DefaultDodge with balanced timing
- Create DefaultParry with strict timing
- Templates for character customization
```

**Estimated Tokens:** ~2,000

---

### Task 14: Integrate Dodge and Parry Animations

**Goal:** Wire dodge and parry animations into Animator Controller.

**Files to Modify:**
- Animator Controller asset (e.g., `Assets/Knockout/Animations/Characters/BaseCharacter/BaseCharacterAnimator.controller`)
- `Assets/Knockout/Scripts/Characters/Components/CharacterAnimator.cs` - Add triggers

**Prerequisites:**
- Review existing Animator Controller structure
- Understand animation trigger system

**Implementation Steps:**

1. Open Animator Controller
2. Add dodge animations:
   - Create states: DodgeLeft, DodgeRight, DodgeBack
   - Assign animation clips: left_dodge.fbx, right_dodge.fbx, (reuse for back or create)
   - Add transitions from Idle/Any State → Dodge states
   - Add triggers: `DodgeLeft`, `DodgeRight`, `DodgeBack`
3. Add parry/block animation (reuse existing Block animation):
   - Parry uses same animation as block, just different timing
   - Ensure Block state can be entered quickly (low transition time)
4. Update `CharacterAnimator.cs`:
   - Add methods `PlayDodgeLeft()`, `PlayDodgeRight()`, `PlayDodgeBack()`
   - Trigger corresponding animator parameters
   - Subscribe to DodgingState enter event to trigger animations
5. Ensure animation lengths match DodgeData frame durations

**Design Guidance:**
- Dodge animations should be quick and snappy
- Parry reuses block animation (visual distinction comes from timing/feedback)
- If back dodge animation missing, mirror left or right dodge

**Verification Checklist:**
- [ ] Dodge animations integrated into Animator Controller
- [ ] Animator triggers configured
- [ ] CharacterAnimator triggers animations correctly
- [ ] Animation durations match data configuration
- [ ] Transitions smooth and responsive

**Testing Instructions:**

Manual testing:
- Enter play mode
- Trigger dodge inputs
- Verify correct animations play
- Check animation timing matches dodge duration

**Commit Message Template:**
```
feat(defense): integrate dodge animations

- Add dodge states to Animator Controller
- Configure dodge animation triggers
- Update CharacterAnimator to trigger dodge animations
```

**Estimated Tokens:** ~6,000

---

### Task 15: Comprehensive Integration Testing

**Goal:** Verify all Phase 2 systems work together in gameplay scenarios.

**Files to Create:**
- `Assets/Knockout/Tests/PlayMode/Integration/DefenseIntegrationTests.cs` - Integration tests

**Prerequisites:**
- All previous tasks complete

**Implementation Steps:**

1. Create integration test suite covering defense mechanics:
   - Test: Dodge through attack with i-frames
   - Test: Dodge recovery (hit during recovery frames)
   - Test: Parry negates damage and staggers attacker
   - Test: Parry counter window allows follow-up attack
   - Test: Block vs Parry differentiation (hold vs tap)
   - Test: Dodge cooldown prevents spam
   - Test: Parry cooldown prevents spam
2. Test multi-scenario flows:
   - Dodge → immediate attack
   - Parry → counter attack
   - Block → dodge (cancel block into dodge)
3. Test edge cases:
   - Dodging with 0 stamina (should work, dodge is free)
   - Parry while exhausted (should work, defensive action)
   - Multiple attackers (dodge i-frames vs multiple hits)
   - Dodge during hit recovery (should be blocked)

**Design Guidance:**
- Test realistic combat scenarios
- Verify timing windows function as designed
- Check event firing order

**Verification Checklist:**
- [ ] All integration tests pass
- [ ] Dodge i-frames functional in gameplay
- [ ] Parry timing reliable and consistent
- [ ] No console errors or warnings
- [ ] Performance maintained (60fps)

**Testing Instructions:**

Run all tests:
- EditMode tests (all defense tests)
- PlayMode tests (all defense tests)
- Integration tests
- Verify all green

**Commit Message Template:**
```
test(defense): add comprehensive integration tests

- Test dodge i-frame mechanics
- Test parry timing and stagger
- Test defense system interactions
- Cover edge cases and multi-scenario flows
```

**Estimated Tokens:** ~10,000

---

### Task 16: Documentation and Code Cleanup

**Goal:** Document defense systems, clean up debug code, finalize Phase 2.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/DEFENSE_SYSTEMS.md` - System documentation

**Files to Modify:**
- All Phase 2 scripts - Remove debug logs, finalize comments

**Prerequisites:**
- All previous tasks complete
- All tests passing

**Implementation Steps:**

1. Create DEFENSE_SYSTEMS.md documentation:
   - Overview of dodge and parry mechanics
   - How to configure timing windows
   - How i-frames work technically
   - Parry timing guide for players/designers
   - Troubleshooting common issues
2. Document animation integration
3. Review all scripts for cleanup:
   - Remove debug logs
   - Finalize XML comments
   - Verify naming conventions
4. Update main docs with defense system summary

**Design Guidance:**
- Documentation helps designers tune defense feel
- Explain frame counts and timing for balance

**Verification Checklist:**
- [ ] DEFENSE_SYSTEMS.md comprehensive
- [ ] All debug logs removed
- [ ] XML comments complete
- [ ] No compiler warnings

**Testing Instructions:**

Final checks:
- Build project
- Run all tests
- Code review

**Commit Message Template:**
```
docs(defense): add defense systems documentation

- Create DEFENSE_SYSTEMS.md guide
- Document i-frame and parry mechanics
- Remove debug logging
- Finalize code documentation
```

**Estimated Tokens:** ~5,000

---

## Phase 2 Verification

**Completion Checklist:**
- [ ] All 16 tasks completed
- [ ] All tests passing (EditMode + PlayMode)
- [ ] Dodge system functional (i-frames work)
- [ ] Parry system functional (timing reliable)
- [ ] Animations integrated and playing correctly
- [ ] Character prefab updated with defense components
- [ ] Documentation complete
- [ ] Code reviewed and cleaned
- [ ] No console errors or warnings
- [ ] Performance maintained (60fps, Profiler shows CharacterDodge and CharacterParry < 0.1ms per frame each, no GC allocations)

**Integration Points for Next Phase:**
- `CharacterDodge` and `CharacterParry` available for AI (future)
- Defense events available for UI feedback (Phase 5)
- Parry counter window tracked for combo integration (Phase 3)
- i-frame and parry systems ready for gameplay tuning

**Known Limitations:**
- Defense UI not implemented (cooldown indicators - Phase 5)
- AI doesn't use dodge/parry yet (post-Phase 5)
- Parry counter window doesn't grant damage bonus yet (could be Phase 3 or future)
- No tutorial for defense mechanics (consider for Phase 5 training mode)

---

## Review Feedback (Iteration 1)

**Verification Evidence:**
- ✅ Bash("git log --format='%s' 3e31515~1..d17b378"): 12 commits, all following conventional format
- ✅ Glob: All implementation files exist (DodgeData, ParryData, CharacterDodge, CharacterParry, DodgingState, ParryStaggerState)
- ✅ Read("Assets/Knockout/Scripts/Characters/Components/DEFENSE_SYSTEMS.md"): Documentation comprehensive
- ✅ Bash("ls Assets/Knockout/Tests/EditMode/Defense/"): EditMode tests for ScriptableObjects present
- ❌ Bash("ls Assets/Knockout/Tests/PlayMode/Defense/"): Directory does not exist
- ❌ Bash("find Assets/Knockout/Data -name '*Dodge*'"): No default data assets found

### Task 4: DodgingState Tests

> **Consider:** Task 4 verification checklist mentions creating `Assets/Knockout/Tests/PlayMode/Defense/DodgingStateTests.cs`. When you run `find Assets/Knockout/Tests/PlayMode -name "*Dodging*"`, what do you find?
>
> **Think about:** The Phase 2 Verification checklist at line 1058 states "All tests passing (EditMode + PlayMode)". Have you created the PlayMode test suite for DodgingState?
>
> **Reflect:** Look at how Phase 1 structured their tests with both EditMode and PlayMode directories. Are you following the same pattern for Phase 2?

### Task 5: CharacterDodge Tests

> **Consider:** Task 5 specifies creating `Assets/Knockout/Tests/PlayMode/Defense/CharacterDodgeTests.cs` with 6 test cases. When you check `ls Assets/Knockout/Tests/PlayMode/Defense/`, does this file exist?
>
> **Think about:** The plan's success criteria at line 13 requires "85%+ test coverage for new components". How can you achieve this without PlayMode tests for CharacterDodge?

### Task 6: Dodge Invulnerability Tests

> **Consider:** Task 6 requires creating `Assets/Knockout/Tests/PlayMode/Defense/DodgeInvulnerabilityTests.cs` to verify i-frames work correctly. Have you verified that hits are actually ignored during the i-frame window through automated tests?
>
> **Reflect:** Integration between DodgingState.IsInvulnerable and hit detection is critical. How are you ensuring this works without PlayMode integration tests?

### Task 9: ParryStaggerState Tests

> **Consider:** Task 9 specifies `Assets/Knockout/Tests/PlayMode/Defense/ParryStaggerStateTests.cs`. When you search for this file, what do you find?
>
> **Think about:** ParryStaggerState has timing-critical behavior (stagger duration, transitions). How are you verifying this works correctly without PlayMode tests?

### Task 10: CharacterParry Tests

> **Consider:** Task 10 requires `Assets/Knockout/Tests/PlayMode/Defense/CharacterParryTests.cs` with 6 specific test cases. Are these tests present?
>
> **Reflect:** Parry timing detection is complex (6-frame window, block press timing). How can you be confident this works without automated tests?

### Task 13: Default Data Assets

> **Consider:** Task 13 specifies creating `Assets/Knockout/Data/Defense/DefaultDodge.asset` and `DefaultParry.asset` with specific default values. When you run `find Assets/Knockout/Data -name "*Dodge*"`, what does it return?
>
> **Think about:** Line 567 in Task 13 states "Save in Assets/Knockout/Data/Defense/ directory (create if needed)". Has this directory been created?
>
> **Reflect:** Default data assets serve as templates for character customization. Without them, how will designers configure dodge and parry for different characters?

### Task 12: Character Prefab Update

> **Consider:** Task 12 requires adding CharacterDodge and CharacterParry components to `Assets/Knockout/Prefabs/Characters/BaseCharacter.prefab` with default data asset references. Have you verified these components are present in the prefab?
>
> **Think about:** The verification checklist at line 1062 states "Character prefab updated with defense components". How can you verify this was done?
>
> **Reflect:** If the prefab doesn't have these components, the dodge and parry systems won't function in gameplay. What command or tool can you use to inspect the prefab?

### Task 15: Integration Tests

> **Consider:** Task 15 requires creating `Assets/Knockout/Tests/PlayMode/Integration/DefenseIntegrationTests.cs` covering end-to-end defense flows. When you search for integration tests mentioning defense with `find Assets/Knockout/Tests -name "*Integration*" | xargs grep -l defense`, what do you find?
>
> **Think about:** The task specifies 7 integration scenarios and 8 edge cases to test. Without these tests, how do you know all Phase 2 systems work together correctly?

### Overall Test Coverage

> **Consider:** Running `find Assets/Knockout/Tests/PlayMode/Defense -type f 2>/dev/null` shows no PlayMode tests exist. The success criteria requires "85%+ test coverage for new components". Do you meet this requirement with only 2 EditMode tests?
>
> **Reflect:** Phase 0's ADR-007 emphasizes component isolation testing. EditMode tests cover ScriptableObjects well, but component behavior (CharacterDodge, CharacterParry, DodgingState, ParryStaggerState) requires PlayMode tests. What's your testing strategy?

---

## Next Phase

After Phase 2 completion and verification, proceed to:

**[Phase 3: Combo System](Phase-3.md)**

Implement natural combo chains and predefined combo sequences.
