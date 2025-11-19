# Phase 3: Combo System

## Phase Goal

Implement the hybrid combo system featuring natural attack chaining with variable timing windows and predefined combo sequences with special bonuses. Combos use heavy damage scaling and can be interrupted by blocking or hitting the attacker.

**Success Criteria:**
- Natural combo chains between any attacks with variable timing windows (Jab chains easy, Uppercut requires precision)
- Predefined combo sequences grant damage bonuses and special effects
- Heavy damage scaling (2nd hit 75%, 3rd hit 50%)
- Combo interruption via blocking or hit/stagger (small windows)
- Character-specific combo sequences configurable via ScriptableObjects
- Combo tracking and events for UI integration
- 85%+ test coverage

**Estimated Tokens:** ~100,000

---

## Prerequisites

**Required Reading:**
- [Phase 0: Foundation](Phase-0.md) - Architecture and combo design (DD-002)
- [Phase 1: Core Resource Systems](Phase-1.md) - Attack execution flow
- [Phase 2: Advanced Defense](Phase-2.md) - Block timing for combo breaking

**Existing Systems to Understand:**
- `CharacterCombat` - Attack execution (Assets/Knockout/Scripts/Characters/Components/CharacterCombat.cs)
- `AttackData` - Attack properties (Assets/Knockout/Scripts/Characters/Data/AttackData.cs)
- `CombatStateMachine` - Attack state transitions
- `BlockingState` - Block timing for interruption

**Environment:**
- Unity 2021.3.8f1 LTS
- Frame-based timing at 60fps

---

## Tasks

### Task 1: Create ComboSequenceData ScriptableObject

**Goal:** Define predefined combo sequence configuration with attack chains and bonuses.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Data/ComboSequenceData.cs` - ScriptableObject class

**Prerequisites:**
- Review `AttackData.cs` for ScriptableObject pattern
- Understand DD-002 (Combo System Design)

**Implementation Steps:**

1. Create `ComboSequenceData` class in `Knockout.Characters.Data` namespace
2. Add `[CreateAssetMenu]` attribute with menu path `"Knockout/Combo Sequence Data"` (order = 6)
3. Define combo sequence structure:
   - Sequence name (string)
   - Required attack sequence (int array of attack type indices: 0=Jab, 1=Hook, 2=Uppercut)
   - Timing window frames (int, how tight the sequence timing is)
   - Damage bonus multiplier (float, applied AFTER damage scaling)
   - Special properties (optional: enhanced knockback, guaranteed stagger, etc.)
   - Visual/audio identifiers (optional: VFX prefab, sound clip)
4. Expose public read-only properties
5. Implement `OnValidate()`:
   - Ensure sequence has at least 2 attacks
   - Clamp damage bonus to reasonable range (1.0-3.0x)
   - Validate timing window > 0
6. Add helper properties: `SequenceLength`, `TimingWindowSeconds`
7. Add XML documentation and tooltips

**Design Guidance:**
- Example sequence: Jab-Jab-Hook = [0, 0, 1] with 1.5x damage bonus
- Timing window can be tighter than natural chain window (predefined sequences require precision)
- Bonus multiplier applies to final hit only (optional: apply to all hits in sequence)
- Keep sequences short (2-4 hits) for accessibility

**Verification Checklist:**
- [ ] ScriptableObject appears in Create menu
- [ ] OnValidate ensures valid sequence configuration
- [ ] Sequence length calculated correctly
- [ ] All fields have tooltips and XML docs

**Testing Instructions:**

Create EditMode test: `Assets/Knockout/Tests/EditMode/Combos/ComboSequenceDataTests.cs`

Test cases:
- ScriptableObject creation succeeds
- OnValidate prevents sequences with < 2 attacks
- OnValidate clamps damage bonus to [1.0, 3.0]
- SequenceLength property correct
- Frame-to-second conversion correct

**Commit Message Template:**
```
feat(combos): add ComboSequenceData ScriptableObject

- Define predefined combo sequence structure
- Configure attack patterns and bonuses
- Validate sequence configuration
```

**Estimated Tokens:** ~4,000

---

### Task 2: Create ComboChainData ScriptableObject

**Goal:** Define natural combo chain timing windows per attack type.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Data/ComboChainData.cs` - ScriptableObject class

**Prerequisites:**
- Understand DD-002 (variable timing by attack)

**Implementation Steps:**

1. Create `ComboChainData` class in `Knockout.Characters.Data` namespace
2. Add `[CreateAssetMenu]` attribute with menu path `"Knockout/Combo Chain Data"` (order = 7)
3. Define chain timing configuration:
   - Chain window frames array (indexed by attack type: [Jab window, Hook window, Uppercut window])
   - Global combo timeout frames (max time between any hits before combo resets)
   - Damage scaling array (float array: [1.0, 0.75, 0.5, 0.5, ...] for 1st, 2nd, 3rd+ hits)
4. Expose public read-only properties
5. Add methods:
   - `GetChainWindow(int attackTypeIndex)` → returns frame count
   - `GetDamageScale(int comboHitNumber)` → returns multiplier (1-indexed)
6. Implement `OnValidate()`:
   - Ensure 3 chain window values (one per attack type)
   - Ensure damage scaling descends (each value ≤ previous)
   - Clamp all values to valid ranges
7. Add XML documentation

**Design Guidance:**
- Default chain windows from DD-002:
  - Jab: 18 frames (~0.3s) - forgiving
  - Hook: 12 frames (~0.2s) - medium
  - Uppercut: 6 frames (~0.1s) - tight
- Heavy damage scaling: [1.0, 0.75, 0.5, 0.5] (floor at 50%)
- Global timeout: 90 frames (~1.5s) - prevents infinitely long combos

**Verification Checklist:**
- [ ] ScriptableObject created successfully
- [ ] Chain windows configurable per attack
- [ ] Damage scaling array validated
- [ ] Helper methods return correct values

**Testing Instructions:**

Create EditMode test: `Assets/Knockout/Tests/EditMode/Combos/ComboChainDataTests.cs`

Test cases:
- GetChainWindow returns correct frames for each attack type
- GetDamageScale returns correct multiplier for combo position
- OnValidate ensures descending damage scale
- Default values match design spec

**Commit Message Template:**
```
feat(combos): add ComboChainData ScriptableObject

- Define natural chain timing per attack type
- Configure damage scaling progression
- Variable timing windows (Jab easy, Uppercut hard)
```

**Estimated Tokens:** ~4,000

---

### Task 3: Create CharacterComboTracker Component

**Goal:** Track combo state, detect chains and sequences, apply damage scaling.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterComboTracker.cs` - Component class

**Prerequisites:**
- Task 1 complete (ComboSequenceData)
- Task 2 complete (ComboChainData)
- Review `CharacterCombat` for attack execution hooks

**Implementation Steps:**

1. Create `CharacterComboTracker` component in `Knockout.Characters.Components` namespace
2. Implement component lifecycle pattern:
   - Dependencies: `CharacterController`, `ComboChainData`, `CharacterCombat`, list of `ComboSequenceData[]`
   - Subscribe to attack events: `OnAttackStarted`, `OnHitLanded`, `OnAttackBlocked`, `OnHitTaken`
3. Track combo state:
   - Current combo count (int, number of consecutive hits)
   - Attack sequence history (List of attack type indices)
   - Last attack timestamp (frame count)
   - Current combo damage scaling multiplier
4. Implement natural chain detection:
   - On attack landed: check if within chain window from last attack
   - If yes: increment combo count, add to sequence history
   - If no: reset combo (late attack)
   - Use attack-specific chain window from ComboChainData
5. Implement predefined sequence detection:
   - On each hit, check sequence history against all ComboSequenceData in array
   - If sequence matches: fire `OnComboSequenceCompleted` event with sequence data
   - Apply sequence bonus to damage (modify before hit applies)
6. Implement damage scaling:
   - Expose method `GetCurrentDamageMultiplier()` for CharacterCombat to query
   - Returns scaling based on combo position (from ComboChainData)
7. Implement combo breaking:
   - Subscribe to `OnAttackBlocked`: if blocker blocked within small window, break combo
   - Subscribe to `OnHitTaken`: if attacker hit/staggered, break combo
   - Breaking resets combo count to 0
8. Expose properties: `ComboCount`, `IsInCombo`, `CurrentSequence`
9. Fire events: `OnComboStarted`, `OnComboHitLanded(hitNumber)`, `OnComboEnded`, `OnComboSequenceCompleted(sequenceData)`

**Design Guidance:**
- Follow ADR-004 (frame-based timing)
- Chain window starts after attack recovery frames end
- Sequence detection is inclusive (partial matches don't break combo, just don't grant bonus yet)
- Damage scaling applies to all hits, sequence bonus applies to final hit of sequence
- Combo breaks immediately on block/hit (small window allows defender to interrupt)

**Verification Checklist:**
- [ ] Component initializes correctly
- [ ] Natural chains detected with correct timing
- [ ] Damage scaling applied correctly
- [ ] Predefined sequences detected
- [ ] Combo breaks on block/hit
- [ ] Events fire correctly

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/Combos/CharacterComboTrackerTests.cs`

Test cases:
- Landing consecutive hits within chain window creates combo
- Landing hit outside chain window resets combo
- Combo count increments correctly
- Damage scaling applied (2nd hit = 75%, 3rd = 50%)
- Predefined sequence detected (e.g., Jab-Jab-Hook)
- Blocking during combo breaks it
- Getting hit during combo breaks it
- OnComboHitLanded events fire with correct hit number

**Commit Message Template:**
```
feat(combos): implement CharacterComboTracker component

- Track combo state and hit sequences
- Detect natural chains with variable timing
- Detect predefined combo sequences
- Apply damage scaling and bonuses
- Handle combo interruption
```

**Estimated Tokens:** ~12,000

---

### Task 4: Integrate Combo Tracking with CharacterCombat

**Goal:** Apply combo damage scaling to attacks and enable combo tracking hooks.

**Files to Modify:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterCombat.cs` - Add combo integration

**Prerequisites:**
- Task 3 complete (CharacterComboTracker exists)
- Understand attack damage calculation in CharacterCombat

**Implementation Steps:**

1. Add dependency on `CharacterComboTracker` (GetComponent or explicit reference)
2. Modify damage calculation:
   - Before applying damage, query `comboTracker.GetCurrentDamageMultiplier()`
   - Multiply attack damage by combo scaling
   - Apply any sequence bonuses if sequence completed
3. Fire attack events with combo context:
   - Ensure `OnHitLanded` includes combo information (hit number)
   - Fire events that ComboTracker subscribes to
4. Handle combo-modified attacks:
   - Sequence bonuses may modify knockback, stagger chance, etc.
   - Apply these modifications from ComboSequenceData properties

**Design Guidance:**
- Damage calculation flow: base damage → combo scaling → sequence bonus → final damage
- Combo scaling is multiplicative with existing damage multipliers (CharacterStats)
- Sequence bonus applies only on sequence completion hit

**Verification Checklist:**
- [ ] Combo damage scaling applied to hits
- [ ] Sequence bonuses applied on completion
- [ ] First hit does full damage
- [ ] Subsequent hits scaled correctly (75%, 50%)
- [ ] Events fire with correct combo context

**Testing Instructions:**

Update PlayMode test: `Assets/Knockout/Tests/PlayMode/Characters/CharacterCombatTests.cs`

Test cases:
- First hit deals 100% damage
- Second hit in combo deals 75% damage
- Third hit in combo deals 50% damage
- Completing predefined sequence applies bonus
- Combo damage stacks with character damage multipliers correctly

**Commit Message Template:**
```
feat(combos): integrate combo damage scaling into combat

- Query combo tracker for damage multiplier
- Apply combo scaling to attack damage
- Apply sequence bonuses on completion
```

**Estimated Tokens:** ~6,000

---

### Task 5: Implement Combo Interruption Logic

**Goal:** Allow blocking or hitting attacker to break combos with small timing windows.

**Files to Modify:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterComboTracker.cs` - Add interruption detection
- `Assets/Knockout/Scripts/Combat/States/BlockingState.cs` - Signal combo break on successful block

**Prerequisites:**
- Task 3 complete (CharacterComboTracker base implementation)
- Understand blocking and hit detection timing

**Implementation Steps:**

1. Define "small window" for combo breaking:
   - Create configurable field in ComboChainData: `comboBreakWindow` (frames, e.g., 6-8 frames)
   - Window represents reaction time defender has to block/counter
2. In CharacterComboTracker:
   - Track when each hit lands (timestamp)
   - On `OnAttackBlocked` event: check if block occurred within break window after hit
   - If yes: break combo (reset count, fire `OnComboBroken` event)
   - If no: combo continues (defender blocked too late)
3. On `OnHitTaken` (attacker gets hit):
   - Immediately break combo (mutual trading)
   - Reset combo state
   - Fire `OnComboBroken` event
4. In BlockingState:
   - Fire event when block successfully stops attack
   - Include timestamp for combo break window check

**Design Guidance:**
- Small window means defender must react quickly (6-8 frames = 0.1-0.133s)
- Blocking late (after window) doesn't break combo, just reduces damage
- Getting hit always breaks combo (no combo continuation on trade)
- Visual/audio feedback important (defender needs to know combo broke)

**Verification Checklist:**
- [ ] Blocking within window breaks combo
- [ ] Blocking outside window doesn't break combo
- [ ] Getting hit breaks combo immediately
- [ ] OnComboBroken event fires
- [ ] Combo state resets correctly

**Testing Instructions:**

Update PlayMode test: `Assets/Knockout/Tests/PlayMode/Combos/CharacterComboTrackerTests.cs`

Test cases:
- Blocking immediately after hit (within window) breaks combo
- Blocking late (outside window) doesn't break combo
- Attacker getting hit breaks combo
- Combo count resets to 0 on break
- OnComboBroken event fires

**Commit Message Template:**
```
feat(combos): implement combo interruption system

- Add combo break window configuration
- Break combo on quick block reaction
- Break combo when attacker hit
- Fire OnComboBroken event for feedback
```

**Estimated Tokens:** ~6,000

---

### Task 6: Create Character-Specific Combo Sequences

**Goal:** Design and create example combo sequences for base character.

**Files to Create:**
- `Assets/Knockout/Data/Combos/BaseCharacter_Jab12Combo.asset` - "1-2" combo (Jab-Jab)
- `Assets/Knockout/Data/Combos/BaseCharacter_Hook12Combo.asset` - "1-2-Hook" (Jab-Jab-Hook)
- `Assets/Knockout/Data/Combos/BaseCharacter_UppercutFinisher.asset` - Uppercut finisher (Hook-Uppercut)

**Prerequisites:**
- Task 1 complete (ComboSequenceData exists)

**Implementation Steps:**

1. Create "1-2" combo (Jab-Jab):
   - Sequence: [0, 0] (Jab, Jab)
   - Timing window: 18 frames (same as Jab chain window)
   - Damage bonus: 1.25x on second Jab
   - Properties: None (basic combo)
2. Create "1-2-Hook" combo (Jab-Jab-Hook):
   - Sequence: [0, 0, 1] (Jab, Jab, Hook)
   - Timing window: 15 frames (tighter than natural)
   - Damage bonus: 1.5x on Hook
   - Properties: Enhanced knockback
3. Create "Hook-Uppercut Finisher":
   - Sequence: [1, 2] (Hook, Uppercut)
   - Timing window: 8 frames (tight timing)
   - Damage bonus: 2.0x on Uppercut
   - Properties: Guaranteed stagger or enhanced knockdown
4. Save in `Assets/Knockout/Data/Combos/` directory

**Design Guidance:**
- Start with 2-3 sequences for testing
- Vary difficulty: easy (Jab-Jab), medium (Jab-Jab-Hook), hard (Hook-Uppercut)
- Rewards scale with difficulty (tighter timing = better bonus)
- These are templates; different characters will have unique sequences

**Verification Checklist:**
- [ ] Assets created successfully
- [ ] Sequences configured correctly
- [ ] Timing windows and bonuses balanced
- [ ] Assets assignable to CharacterComboTracker

**Testing Instructions:**

Manual testing:
- Assign sequences to character in scene
- Enter play mode
- Execute sequences and verify bonuses apply

**Commit Message Template:**
```
feat(combos): add base character combo sequences

- Create "1-2" Jab combo
- Create "1-2-Hook" combo
- Create "Hook-Uppercut Finisher"
- Template sequences for character customization
```

**Estimated Tokens:** ~4,000

---

### Task 7: Create Default ComboChainData Asset

**Goal:** Create default combo chain configuration with variable timing windows.

**Files to Create:**
- `Assets/Knockout/Data/Combos/DefaultComboChain.asset` - ComboChainData instance

**Prerequisites:**
- Task 2 complete (ComboChainData exists)

**Implementation Steps:**

1. Create DefaultComboChain asset
2. Configure chain windows (frames at 60fps):
   - Jab chain window: 18 frames (~0.3s)
   - Hook chain window: 12 frames (~0.2s)
   - Uppercut chain window: 6 frames (~0.1s)
3. Configure damage scaling:
   - 1st hit: 1.0 (100%)
   - 2nd hit: 0.75 (75%)
   - 3rd hit: 0.5 (50%)
   - 4th+ hits: 0.5 (floor at 50%)
4. Set global combo timeout: 90 frames (~1.5s)
5. Set combo break window: 8 frames (~0.133s)
6. Save in `Assets/Knockout/Data/Combos/`

**Design Guidance:**
- Values match DD-002 heavy scaling design
- Variable windows create skill expression (Jab chains easy, Uppercut requires precision)

**Verification Checklist:**
- [ ] Asset created successfully
- [ ] Values match design spec
- [ ] Chain windows vary by attack type
- [ ] Damage scaling configured correctly

**Testing Instructions:**

Manual verification:
- Asset appears in project
- Values display correctly
- No validation errors

**Commit Message Template:**
```
feat(combos): add default combo chain configuration

- Configure variable timing windows per attack
- Set heavy damage scaling (75%, 50%)
- Template for character-specific tuning
```

**Estimated Tokens:** ~3,000

---

### Task 8: Update Character Prefab with Combo System

**Goal:** Add combo tracking to character prefab and assign configurations.

**Files to Modify:**
- `Assets/Knockout/Prefabs/Characters/BaseCharacter.prefab` - Add component

**Prerequisites:**
- Task 3 complete (CharacterComboTracker exists)
- Task 6 complete (combo sequence assets exist)
- Task 7 complete (DefaultComboChain exists)

**Implementation Steps:**

1. Open BaseCharacter prefab
2. Add `CharacterComboTracker` component
3. Assign dependencies:
   - Character Controller
   - Character Combat
   - ComboChainData: DefaultComboChain
4. Assign combo sequences:
   - Add array of 3 combo sequences (Jab-Jab, Jab-Jab-Hook, Hook-Uppercut)
5. Verify component initialization order
6. Test in play mode

**Design Guidance:**
- Combo tracker initializes after CharacterCombat
- Sequences are character-specific (different characters will have different arrays)

**Verification Checklist:**
- [ ] Component added to prefab
- [ ] Dependencies assigned
- [ ] Combo sequences assigned
- [ ] Component initializes without errors
- [ ] Combos functional in play mode

**Testing Instructions:**

Manual testing:
- Enter play mode
- Execute natural chains (Jab-Jab-Jab)
- Execute predefined sequences (Jab-Jab-Hook)
- Verify damage scaling applies
- Verify sequence bonuses apply

**Commit Message Template:**
```
feat(combos): add combo system to character prefab

- Add CharacterComboTracker component
- Assign default combo chain configuration
- Assign base character combo sequences
```

**Estimated Tokens:** ~3,000

---

### Task 9: Add Combo Visual Feedback Hooks

**Goal:** Fire events for UI/VFX to react to combo hits and completions.

**Files to Modify:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterComboTracker.cs` - Add feedback events

**Prerequisites:**
- Task 3 complete (CharacterComboTracker base implementation)

**Implementation Steps:**

1. Add public events in CharacterComboTracker:
   - `OnComboHitLanded(int hitNumber, float damageDealt)` - fires on each combo hit
   - `OnComboSequenceCompleted(ComboSequenceData sequence)` - fires when predefined sequence lands
   - `OnComboBroken(int finalComboCount)` - fires when combo interrupted
   - `OnComboEnded(int finalComboCount, float totalDamage)` - fires when combo times out naturally
2. Fire events at appropriate points:
   - OnComboHitLanded: after hit registered and damage applied
   - OnComboSequenceCompleted: immediately when sequence detected
   - OnComboBroken: when block/hit interrupts combo
   - OnComboEnded: when combo timeout expires without continuation
3. Include useful data in events for UI/VFX:
   - Hit number in combo
   - Damage dealt
   - Sequence that completed
   - Final combo stats (count, total damage)

**Design Guidance:**
- Events provide hooks for Phase 5 UI (combo counter, damage numbers)
- VFX can react to sequence completions (special effects)
- Audio can play increasing pitch for combo hits

**Verification Checklist:**
- [ ] Events defined with correct signatures
- [ ] Events fire at correct moments
- [ ] Event data accurate (hit numbers, damage values)
- [ ] No event spam (fires once per occurrence)

**Testing Instructions:**

Update PlayMode test: `Assets/Knockout/Tests/PlayMode/Combos/CharacterComboTrackerTests.cs`

Test cases:
- OnComboHitLanded fires for each hit with correct number
- OnComboSequenceCompleted fires when sequence detected
- OnComboBroken fires when combo interrupted
- OnComboEnded fires when combo times out
- Event data correct (hit numbers, damage, sequence info)

**Commit Message Template:**
```
feat(combos): add combo feedback events for UI/VFX

- Fire OnComboHitLanded with hit number and damage
- Fire OnComboSequenceCompleted when sequence lands
- Fire OnComboBroken and OnComboEnded with combo stats
- Prepare hooks for Phase 5 UI integration
```

**Estimated Tokens:** ~5,000

---

### Task 10: Implement Combo Counter Window After Parry

**Goal:** Integrate parry counter window with combo system for bonus damage opportunities.

**Files to Modify:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterComboTracker.cs` - Add counter window tracking
- `Assets/Knockout/Scripts/Characters/Components/CharacterParry.cs` - Signal counter window

**Prerequisites:**
- Phase 2 complete (CharacterParry exists)
- Task 3 complete (CharacterComboTracker exists)

**Implementation Steps:**

1. In CharacterParry:
   - On successful parry, fire `OnCounterWindowOpened(float duration)` event
2. In CharacterComboTracker:
   - Subscribe to `OnCounterWindowOpened`
   - Track if currently in counter window
   - Property: `IsInCounterWindow` (bool)
3. Apply counter window bonus:
   - First hit during counter window gets damage bonus (e.g., +25%)
   - OR first hit during counter window starts combo at 2x instead of 1x
   - Configurable in ComboChainData: `counterWindowDamageBonus`
4. Counter window expires after parry duration (from ParryData)

**Design Guidance:**
- Counter window reward is optional (can be just psychological advantage of attacker stagger)
- If implementing damage bonus, keep it modest (1.25x, not game-breaking)
- Bonus applies to first hit only (subsequent hits follow normal combo scaling)

**Verification Checklist:**
- [ ] Parry opens counter window
- [ ] IsInCounterWindow tracked correctly
- [ ] Counter window damage bonus applies (if implemented)
- [ ] Counter window expires after duration

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/Integration/ParryComboIntegrationTests.cs`

Test cases:
- Successful parry opens counter window
- Attack during counter window gets bonus damage
- Counter window expires after duration
- Combo continues normally after counter window hit

**Commit Message Template:**
```
feat(combos): integrate parry counter window with combos

- Track counter window from successful parries
- Apply damage bonus to first counter hit
- Counter window tracked for combo timing
```

**Estimated Tokens:** ~6,000

---

### Task 11: Add Combo Reset on Round Start/End

**Goal:** Reset combo state between rounds to avoid cross-round combos.

**Files to Modify:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterComboTracker.cs` - Add reset method
- `Assets/Knockout/Scripts/Systems/RoundManager.cs` - Call combo reset

**Prerequisites:**
- Task 3 complete (CharacterComboTracker exists)
- Understand RoundManager round flow

**Implementation Steps:**

1. In CharacterComboTracker:
   - Add public method `ResetCombo()`:
     - Clear combo count
     - Clear attack sequence history
     - Reset timestamps
     - Fire `OnComboEnded` if combo was active
2. In RoundManager:
   - On round start (`OnRoundStart` or countdown begin):
     - Find all CharacterComboTracker components in scene
     - Call `ResetCombo()` on each
   - On round end (`OnRoundEnd`):
     - Reset combos to clean state

**Design Guidance:**
- Prevents combos from spanning rounds (unrealistic)
- Clean slate each round
- Reset happens during countdown, not instantly (gives time for animations to finish)

**Verification Checklist:**
- [ ] ResetCombo method clears all combo state
- [ ] RoundManager calls reset at round transitions
- [ ] Combos don't persist across rounds
- [ ] No errors on reset

**Testing Instructions:**

Update PlayMode test: `Assets/Knockout/Tests/PlayMode/Systems/RoundManagerTests.cs`

Test cases:
- Starting new round resets combo state
- Combo active at round end gets reset
- Multiple rounds in sequence handle resets correctly

**Commit Message Template:**
```
feat(combos): reset combos at round transitions

- Add ResetCombo method to clear combo state
- RoundManager resets combos on round start/end
- Prevent cross-round combo persistence
```

**Estimated Tokens:** ~4,000

---

### Task 12: Optimize Combo Detection Performance

**Goal:** Ensure combo tracking doesn't degrade performance (60fps target).

**Files to Modify:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterComboTracker.cs` - Optimize detection

**Prerequisites:**
- Task 3 complete (CharacterComboTracker functional)

**Implementation Steps:**

1. Profile combo tracking:
   - Use Unity Profiler to measure CharacterComboTracker.Update() cost
   - Identify any expensive operations (list searches, allocations)
2. Optimize sequence detection:
   - Cache sequence data on initialization (don't repeatedly access ScriptableObjects)
   - Use efficient matching algorithm (avoid nested loops)
   - Consider: matching from longest sequence to shortest (early exit on partial matches)
3. Avoid allocations:
   - Reuse attack sequence list (clear and add, don't recreate)
   - Cache frame count queries
   - Avoid LINQ in hot paths
4. Use FixedUpdate for frame-based timing (not Update)

**Design Guidance:**
- Combo tracking runs every frame for active combo, must be lightweight
- Most expensive: sequence matching (minimize comparisons)
- Cache immutable data, recalculate only when combo state changes

**Verification Checklist:**
- [ ] Combo tracking cost < 0.1ms per frame (Profiler)
- [ ] No GC allocations during combo tracking
- [ ] 60fps maintained with combo system active
- [ ] Sequence detection remains accurate after optimization

**Testing Instructions:**

Performance testing:
- Use Unity Profiler
- Create scenario with long combos
- Measure CharacterComboTracker CPU cost
- Verify < 0.1ms per frame
- Check GC allocations (should be 0 during gameplay)

**Commit Message Template:**
```
perf(combos): optimize combo detection performance

- Cache sequence data on initialization
- Use efficient matching algorithm
- Eliminate allocations in hot paths
- Maintain 60fps target
```

**Estimated Tokens:** ~5,000

---

### Task 13: Comprehensive Integration Testing

**Goal:** Verify combo system works correctly in all scenarios.

**Files to Create:**
- `Assets/Knockout/Tests/PlayMode/Integration/ComboIntegrationTests.cs` - Integration tests

**Prerequisites:**
- All previous tasks complete

**Implementation Steps:**

1. Create integration test suite:
   - Test: Natural combo chains with all attack types
   - Test: Variable timing windows (Jab easy, Uppercut hard)
   - Test: Damage scaling applied correctly across combo
   - Test: Predefined sequence detection and bonuses
   - Test: Combo breaking via blocking (timing window)
   - Test: Combo breaking via attacker getting hit
   - Test: Multiple predefined sequences in one combo
   - Test: Combo timeout (exceeding global timeout resets)
   - Test: Combo reset on round transitions
   - Test: Parry counter window integration
2. Test edge cases:
   - Exactly on timing window boundary (hit at frame N)
   - Switching attack types mid-combo
   - Combo with 0 stamina (attacks fail, combo breaks)
   - Two characters comboing simultaneously
   - Sequence partial matches (Jab-Jab-Uppercut when only Jab-Jab-Hook sequence exists)

**Design Guidance:**
- Test realistic gameplay flows
- Verify frame-perfect timing boundaries
- Check event firing order and accuracy

**Verification Checklist:**
- [ ] All integration tests pass
- [ ] Combo system functional in gameplay
- [ ] No console errors or warnings
- [ ] Performance maintained (60fps)
- [ ] Damage calculations correct

**Testing Instructions:**

Run all tests:
- EditMode combo tests
- PlayMode combo tests
- Integration tests
- Verify all green

**Commit Message Template:**
```
test(combos): add comprehensive integration tests

- Test natural chains and variable timing
- Test damage scaling and sequence bonuses
- Test combo interruption mechanics
- Cover edge cases and timing boundaries
```

**Estimated Tokens:** ~10,000

---

### Task 14: Documentation and Code Cleanup

**Goal:** Document combo system, clean up code, finalize Phase 3.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/COMBO_SYSTEM.md` - System documentation

**Files to Modify:**
- All Phase 3 scripts - Remove debug logs, finalize comments

**Prerequisites:**
- All previous tasks complete
- All tests passing

**Implementation Steps:**

1. Create COMBO_SYSTEM.md documentation:
   - Overview of natural chains vs predefined sequences
   - How to configure timing windows
   - How to create character-specific combo sequences
   - Damage scaling explanation
   - Combo interruption mechanics
   - Tips for balancing combos
   - Troubleshooting guide
2. Document combo sequence creation workflow:
   - How to design sequences
   - Timing window tuning
   - Bonus balancing
3. Review all scripts:
   - Remove debug logs
   - Finalize XML comments
   - Verify naming conventions
4. Update main documentation

**Design Guidance:**
- Documentation helps designers create balanced combos
- Explain frame counts for precise tuning
- Provide example sequences for reference

**Verification Checklist:**
- [ ] COMBO_SYSTEM.md comprehensive
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
docs(combos): add combo system documentation

- Create COMBO_SYSTEM.md guide
- Document natural chains and sequences
- Explain damage scaling and interruption
- Remove debug logging and finalize code
```

**Estimated Tokens:** ~5,000

---

## Phase 3 Verification

**Completion Checklist:**
- [ ] All 14 tasks completed
- [ ] All tests passing (EditMode + PlayMode)
- [ ] Natural combo chains functional (variable timing windows)
- [ ] Predefined sequences detected and bonuses applied
- [ ] Damage scaling working correctly (75%, 50%)
- [ ] Combo interruption functional (block/hit)
- [ ] Character prefab updated with combo system
- [ ] Default configurations created
- [ ] Documentation complete
- [ ] Code reviewed and cleaned
- [ ] No console errors or warnings
- [ ] Performance maintained (60fps, Profiler shows CharacterComboTracker < 0.1ms per frame, no GC allocations during combo detection)

**Integration Points for Next Phase:**
- Combo events available for UI (combo counter - Phase 5)
- Combo tracking available for judge scoring (Phase 4)
- Sequence completion events for VFX/audio (Phase 5)
- Counter window integrated with parry system

**Known Limitations:**
- Combo UI not implemented (Phase 5)
- AI doesn't execute combos yet (post-Phase 5)
- No combo challenges/trials in training mode (Phase 5)
- Special moves don't benefit from combo damage scaling (Phase 4 decision)

---

## Next Phase

After Phase 3 completion and verification, proceed to:

**[Phase 4: Special Moves & Judge Scoring](Phase-4.md)**

Implement character signature special moves and comprehensive judge scoring system.
