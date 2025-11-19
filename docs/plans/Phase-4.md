# Phase 4: Special Moves & Judge Scoring

## Review Feedback (Iteration 1)

### Task 8: CharacterScoring Component - Event Integration

> **Consider:** Looking at `CharacterScoring.cs:202-204`, there's a note saying "We need an OnHitLanded event - this may not exist yet". Have you verified whether `CharacterCombat` actually fires events when hits land?
>
> **Think about:** Running `Grep pattern: "public event.*OnHitLanded" in CharacterCombat.cs` shows no results. How can CharacterScoring track clean hits and damage dealt if there's no event to subscribe to?
>
> **Reflect:** The plan at Task 8, line 517 specifies subscribing to "CharacterCombat: OnHitLanded, OnAttackBlocked, OnHitTaken". Are these events present in CharacterCombat? If not, what needs to be added?
>
> **Consider:** At `CharacterScoring.cs:292`, there's another note: "We need to track if dodge actually avoided a hit". Are you counting all dodges, or only successful ones that avoided damage?
>
> **Think about:** At `CharacterScoring.cs:312`, the comment says "This counts special move execution, not necessarily landing". Should scoring track when special moves are used, or when they successfully hit the opponent?

### Task 8: Missing Event Handlers

> **Reflect:** The `CharacterScoring` component subscribes to several events in `SubscribeToEvents()`. Have you implemented ALL the event handlers that the component needs?
>
> **Consider:** Looking at lines 200-204, the combat event subscription is commented out with a note. What happens to clean hit tracking, damage tracking, and block tracking without these events?
>
> **Think about:** Would adding the missing events to `CharacterCombat` (OnHitLanded, OnAttackBlocked, OnHitTaken) allow the scoring system to work as designed?

### Integration Question

> **Reflect:** The plan specifies comprehensive stat tracking including "Clean hits landed" and "Total damage dealt" (Task 8, step 3). Without hit events from CharacterCombat, how is your implementation tracking these stats?
>
> **Consider:** Should `CharacterCombat` be extended to fire events when:
> - A hit successfully lands (OnHitLanded with damage parameter)?
> - An attack is blocked (OnAttackBlocked)?
> - Character takes a hit (OnHitTaken with damage parameter)?
>
> **Think about:** Would this require modifying Phase 3 (CharacterCombat) to add these events before Phase 4 scoring can work correctly?

---

## Phase Goal

Implement character signature special moves with cooldown and stamina gating, plus comprehensive judge scoring system for round decisions. Special moves are powerful, unique techniques with dual-resource requirements. Judge scoring tracks all combat actions to determine round winners when time expires without knockout.

**Success Criteria:**
- Special move system with cooldown + stamina cost gating
- Each character has one signature special move
- Special moves can trigger enhanced knockdown states
- Comprehensive judge scoring tracking all combat metrics
- Round decision logic based on accumulated scores
- Momentum-based scoring (offense builds advantage, defense resets)
- All systems configurable via ScriptableObjects
- 85%+ test coverage

**Estimated Tokens:** ~100,000

---

## Prerequisites

**Required Reading:**
- [Phase 0: Foundation](Phase-0.md) - Special move and scoring design (DD-004, DD-005, ADR-006)
- [Phase 1: Core Resource Systems](Phase-1.md) - Stamina system, SpecialKnockdownState
- [Phase 2: Advanced Defense](Phase-2.md) - Parry system for scoring
- [Phase 3: Combo System](Phase-3.md) - Combo tracking for scoring

**Existing Systems to Understand:**
- `CharacterStamina` - Stamina checking
- `CharacterCombat` - Attack execution
- `CharacterComboTracker` - Combo events
- `RoundManager` - Round flow and timer
- `SpecialKnockdownState` - Enhanced knockdown (Phase 1)

**Environment:**
- Unity 2021.3.8f1 LTS

---

## Tasks

### Task 1: Create SpecialMoveData ScriptableObject

**Goal:** Define special move configuration including damage, costs, and cooldown.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Data/SpecialMoveData.cs` - ScriptableObject class

**Prerequisites:**
- Review `AttackData.cs` for ScriptableObject pattern
- Understand DD-004 (Special Moves Design)

**Implementation Steps:**

1. Create `SpecialMoveData` class in `Knockout.Characters.Data` namespace
2. Add `[CreateAssetMenu]` attribute with menu path `"Knockout/Special Move Data"` (order = 8)
3. Define special move properties:
   - Special move name (string)
   - Base attack reference (AttackData - special move uses as template)
   - Damage multiplier (float, applied to base attack damage)
   - Knockback multiplier (float)
   - Cooldown duration (seconds)
   - Stamina cost (float)
   - Triggers special knockdown (bool)
   - Special knockdown duration (seconds, if applicable)
   - Animation trigger name (string)
   - Visual/audio identifiers (optional)
4. Expose public read-only properties
5. Implement `OnValidate()`:
   - Ensure cooldown > 0
   - Ensure stamina cost > 0
   - Clamp multipliers to reasonable ranges (1.0-3.0x)
6. Add XML documentation and tooltips

**Design Guidance:**
- Special move references existing AttackData (e.g., Hook) and enhances it
- Default cooldown: 45 seconds (one per round approximately)
- Default stamina cost: 40 (significant investment)
- Damage multiplier: 2.0x (powerful but not one-shot)
- Most special moves trigger special knockdown

**Verification Checklist:**
- [ ] ScriptableObject appears in Create menu
- [ ] OnValidate ensures valid configuration
- [ ] Base attack reference required
- [ ] All fields have tooltips and XML docs

**Testing Instructions:**

Create EditMode test: `Assets/Knockout/Tests/EditMode/SpecialMoves/SpecialMoveDataTests.cs`

Test cases:
- ScriptableObject creation succeeds
- OnValidate clamps multipliers to [1.0, 3.0]
- OnValidate ensures cooldown > 0
- OnValidate ensures stamina cost > 0
- Default values match design spec

**Commit Message Template:**
```
feat(special): add SpecialMoveData ScriptableObject

- Define special move properties and costs
- Reference base attack for animation/timing
- Configure cooldown and stamina gating
- Support special knockdown configuration
```

**Estimated Tokens:** ~4,000

---

### Task 2: Extend Input System for Special Move Input

**Goal:** Add special move input action to Input System.

**Files to Modify:**
- `Assets/Knockout/Input/KnockoutInputActions.inputactions` - Add special move action
- `Assets/Knockout/Scripts/Characters/Components/CharacterInput.cs` - Expose special move event

**Prerequisites:**
- Review Phase 2 dodge input integration

**Implementation Steps:**

1. Open KnockoutInputActions asset
2. Add new action in "Gameplay" map:
   - Action: `SpecialMove` (button binding, e.g., R key or gamepad trigger)
3. Configure as button action (Press interaction)
4. Regenerate C# input class if needed
5. In CharacterInput:
   - Add event: `public event Action OnSpecialMoveInput;`
   - Fire event on special move button press
   - Ensure event only fires when character can act

**Design Guidance:**
- Special move is dedicated button, not combo of inputs
- PC default: R key or middle mouse button
- Gamepad: Right trigger or face button
- Input should feel important (not accidental press)

**Verification Checklist:**
- [ ] SpecialMove action added to Input Actions
- [ ] Binding configured for keyboard/gamepad
- [ ] CharacterInput fires OnSpecialMoveInput event
- [ ] No input conflicts

**Testing Instructions:**

Update PlayMode test: `Assets/Knockout/Tests/PlayMode/Characters/CharacterInputTests.cs`

Test cases:
- OnSpecialMoveInput fires when button pressed
- Event doesn't fire when character incapacitated

**Commit Message Template:**
```
feat(special): add special move input action

- Add SpecialMove input action
- Configure keyboard/gamepad bindings
- Expose OnSpecialMoveInput event in CharacterInput
```

**Estimated Tokens:** ~3,000

---

### Task 3: Create CharacterSpecialMoves Component

**Goal:** Manage special move execution, cooldown tracking, and stamina gating.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterSpecialMoves.cs` - Component class

**Prerequisites:**
- Task 1 complete (SpecialMoveData exists)
- Task 2 complete (special move input)
- Phase 1 complete (CharacterStamina exists)

**Implementation Steps:**

1. Create `CharacterSpecialMoves` component in `Knockout.Characters.Components` namespace
2. Implement component lifecycle pattern:
   - Dependencies: `CharacterController`, `SpecialMoveData`, `CharacterInput`, `CharacterStamina`, `CharacterCombat`
   - Subscribe to `OnSpecialMoveInput` in `Initialize()`
3. Track cooldown state:
   - Current cooldown time remaining (float, counts down)
   - Property: `IsOnCooldown` (bool)
   - Property: `CooldownProgress` (0-1, for UI)
   - Property: `CooldownTimeRemaining` (seconds)
4. Implement special move execution:
   - On input: check `CanUseSpecialMove()` (cooldown ready AND stamina available AND valid state)
   - If yes: execute special move (trigger attack with enhanced properties)
   - Consume stamina via CharacterStamina
   - Start cooldown timer
   - Fire `OnSpecialMoveUsed(SpecialMoveData)` event
   - If no: fire `OnSpecialMoveFailed(FailureReason)` event (feedback)
5. Implement cooldown countdown:
   - In `Update()`: decrement cooldown time
   - When cooldown reaches 0, fire `OnSpecialMoveReady` event
6. Expose method `TryUseSpecialMove()` for manual/AI triggering
7. Create enum `SpecialMoveFailureReason { OnCooldown, InsufficientStamina, InvalidState }`

**Design Guidance:**
- Special move is double-gated: both cooldown and stamina must be available
- Cooldown starts when special move initiated (not when it lands)
- Special move executes like normal attack but with enhanced properties from SpecialMoveData
- Cannot use special move while attacking, dodging, knocked down, etc.

**Verification Checklist:**
- [ ] Component initializes correctly
- [ ] Special move executes when available
- [ ] Cooldown tracking functional
- [ ] Stamina consumption works
- [ ] Double-gating prevents use when either resource unavailable
- [ ] Events fire correctly

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/SpecialMoves/CharacterSpecialMovesTests.cs`

Test cases:
- TryUseSpecialMove succeeds when both resources available
- TryUseSpecialMove fails when on cooldown
- TryUseSpecialMove fails when insufficient stamina
- TryUseSpecialMove fails when in invalid state
- Cooldown counts down over time
- OnSpecialMoveReady fires when cooldown expires
- Stamina consumed on special move use

**Commit Message Template:**
```
feat(special): implement CharacterSpecialMoves component

- Track cooldown and stamina gating
- Execute special moves with enhanced properties
- Fire events for UI/feedback
- Double-gating prevents spam
```

**Estimated Tokens:** ~10,000

---

### Task 4: Integrate Special Moves with Combat System

**Goal:** Execute special moves as enhanced attacks through CharacterCombat.

**Files to Modify:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterCombat.cs` - Add special move execution path
- `Assets/Knockout/Scripts/Combat/HitDetection/HitData.cs` - Add special move flag

**Prerequisites:**
- Task 3 complete (CharacterSpecialMoves exists)
- Understand attack execution flow in CharacterCombat

**Implementation Steps:**

1. In HitData (or wherever hit information is stored):
   - Add field: `isSpecialMove` (bool)
   - Add field: `specialMoveData` (SpecialMoveData reference)
2. In CharacterCombat:
   - Add method `ExecuteSpecialMove(SpecialMoveData specialMove)`
   - Execution flow:
     - Use specialMove.BaseAttackData for animation and timing
     - Apply specialMove.DamageMultiplier to damage calculation
     - Apply specialMove.KnockbackMultiplier to knockback
     - Flag hit as special move
     - Trigger SpecialKnockdownState if specialMove.TriggersSpecialKnockdown
3. CharacterSpecialMoves calls `CharacterCombat.ExecuteSpecialMove()` when executing special
4. Hit detection recognizes special move flag and applies special knockdown

**Design Guidance:**
- Special move execution reuses attack execution pipeline with modifiers
- Special move is essentially "super-powered" version of base attack
- Special knockdown triggered by special move hit, not normal attacks

**Verification Checklist:**
- [ ] Special moves execute as attacks
- [ ] Damage and knockback multipliers applied
- [ ] Special knockdown triggered on hit
- [ ] Special move animations play correctly
- [ ] Hit data includes special move flag

**Testing Instructions:**

Update PlayMode test: `Assets/Knockout/Tests/PlayMode/Characters/CharacterCombatTests.cs`

Test cases:
- ExecuteSpecialMove triggers attack with enhanced damage
- Special move knockback stronger than base attack
- Special move triggers SpecialKnockdownState
- Hit data flagged as special move

**Commit Message Template:**
```
feat(special): integrate special moves with combat system

- Add ExecuteSpecialMove method to CharacterCombat
- Apply damage and knockback multipliers
- Trigger special knockdown on hit
- Flag hits as special moves in hit data
```

**Estimated Tokens:** ~7,000

---

### Task 5: Create Character Signature Special Moves

**Goal:** Design and create example special moves for base character.

**Files to Create:**
- `Assets/Knockout/Data/SpecialMoves/BaseCharacter_Haymaker.asset` - Powerful hook variant

**Prerequisites:**
- Task 1 complete (SpecialMoveData exists)

**Implementation Steps:**

1. Create "Haymaker" special move:
   - Name: "Haymaker"
   - Base Attack: Right_hook AttackData
   - Damage Multiplier: 2.0x
   - Knockback Multiplier: 2.5x
   - Cooldown: 45 seconds
   - Stamina Cost: 40
   - Triggers Special Knockdown: true
   - Special Knockdown Duration: 3.0 seconds
   - Animation Trigger: "Haymaker" (or reuse "Hook")
2. Save in `Assets/Knockout/Data/SpecialMoves/`
3. (Optional) Create additional variants for other characters later

**Design Guidance:**
- Haymaker is classic powerful punch special
- High risk (high stamina cost, long cooldown) high reward (big damage)
- Balanced: strong but not one-shot kill (2x damage vs 100 health = 20 damage on base 10dmg attack)
- Future characters will have unique special moves (Liver Shot, Cross Counter, etc.)

**Verification Checklist:**
- [ ] Asset created successfully
- [ ] Values balanced (not overpowered)
- [ ] Base attack reference set
- [ ] Asset assignable to CharacterSpecialMoves

**Testing Instructions:**

Manual testing:
- Assign Haymaker to character
- Execute special move in play mode
- Verify enhanced damage and knockdown

**Commit Message Template:**
```
feat(special): add Haymaker special move for base character

- Create powerful hook variant
- 2x damage, 2.5x knockback
- 45s cooldown, 40 stamina cost
- Triggers special knockdown
```

**Estimated Tokens:** ~3,000

---

### Task 6: Update Character Prefab with Special Moves

**Goal:** Add special move system to character prefab.

**Files to Modify:**
- `Assets/Knockout/Prefabs/Characters/BaseCharacter.prefab` - Add component

**Prerequisites:**
- Task 3 complete (CharacterSpecialMoves exists)
- Task 5 complete (Haymaker asset exists)

**Implementation Steps:**

1. Open BaseCharacter prefab
2. Add `CharacterSpecialMoves` component
3. Assign dependencies:
   - Character Controller
   - Character Input
   - Character Stamina
   - Character Combat
4. Assign SpecialMoveData: Haymaker
5. Verify component initialization
6. Test in play mode

**Design Guidance:**
- Each character will have different special move assigned
- Component setup follows existing pattern

**Verification Checklist:**
- [ ] Component added to prefab
- [ ] Dependencies assigned
- [ ] Special move assigned
- [ ] Component initializes without errors
- [ ] Special move functional in play mode

**Testing Instructions:**

Manual testing:
- Enter play mode
- Build stamina and wait for cooldown
- Execute special move
- Verify damage, knockdown, cooldown

**Commit Message Template:**
```
feat(special): add special move system to character prefab

- Add CharacterSpecialMoves component
- Assign Haymaker special move
- Integrate into component initialization
```

**Estimated Tokens:** ~3,000

---

### Task 7: Create ScoringWeights ScriptableObject

**Goal:** Define configurable weights for judge scoring metrics.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Data/ScoringWeights.cs` - ScriptableObject class

**Prerequisites:**
- Understand DD-005 (Judge Scoring Design)

**Implementation Steps:**

1. Create `ScoringWeights` class in `Knockout.Characters.Data` namespace
2. Add `[CreateAssetMenu]` attribute with menu path `"Knockout/Scoring Weights"` (order = 9)
3. Define scoring weight fields (all floats):
   - **Offensive Weights:**
     - `cleanHitPoints` - Points for landing clean hit
     - `comboHitBonus` - Bonus per hit in combo (additive)
     - `comboSequencePoints` - Points for completing predefined sequence
     - `specialMovePoints` - Points for landing special move
     - `knockdownPoints` - Points for knockdown
     - `damageDealtWeight` - Multiplier for total damage dealt
   - **Defensive Weights:**
     - `blockPoints` - Points per successful block
     - `parryPoints` - Points per successful parry
     - `dodgePoints` - Points per successful dodge (avoided hit)
   - **Control Weights:**
     - `aggressionPointsPerSecond` - Points for time spent in offensive range
     - `ringControlBonus` - Bonus for controlling center (optional, can be 0)
   - **Penalties:**
     - `exhaustionPenalty` - Points lost per exhaustion occurrence
     - `missedAttackPenalty` - Small penalty for whiffed attacks (optional)
4. Expose public read-only properties
5. Implement `OnValidate()` to clamp values to reasonable ranges
6. Add XML documentation explaining each metric

**Design Guidance:**
- Default values from DD-005:
  - Clean hit: 1 point
  - Combo sequence: 5 points
  - Special move: 8 points
  - Knockdown: 10 points
  - Parry: 2 points
  - Aggression: 0.1 points/sec
- Weights allow balancing (e.g., emphasize aggression vs technical defense)
- Positive weights only (penalties are separate)

**Verification Checklist:**
- [ ] ScriptableObject created successfully
- [ ] All metrics have configurable weights
- [ ] OnValidate clamps to valid ranges
- [ ] Documentation explains each metric

**Testing Instructions:**

Create EditMode test: `Assets/Knockout/Tests/EditMode/Scoring/ScoringWeightsTests.cs`

Test cases:
- ScriptableObject creation succeeds
- OnValidate clamps negative values
- Default values match design spec

**Commit Message Template:**
```
feat(scoring): add ScoringWeights ScriptableObject

- Define configurable weights for all scoring metrics
- Support offensive, defensive, and control scoring
- Allow balance tuning via data
```

**Estimated Tokens:** ~4,000

---

### Task 8: Create CharacterScoring Component

**Goal:** Track all combat actions and calculate accumulated score for judge decision.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterScoring.cs` - Component class

**Prerequisites:**
- Task 7 complete (ScoringWeights exists)
- All previous phases (events from combat, combos, defense)

**Implementation Steps:**

1. Create `CharacterScoring` component in `Knockout.Characters.Components` namespace
2. Implement component lifecycle pattern:
   - Dependencies: `CharacterController`, `ScoringWeights`, all action components (Combat, Combo, Dodge, Parry, Stamina, SpecialMoves)
   - Subscribe to ALL relevant events in `Initialize()`:
     - CharacterCombat: OnHitLanded, OnAttackBlocked, OnHitTaken
     - CharacterComboTracker: OnComboSequenceCompleted
     - CharacterDodge: OnDodgeStarted (if hit avoided)
     - CharacterParry: OnParrySuccess
     - CharacterStamina: OnStaminaDepleted
     - CharacterSpecialMoves: OnSpecialMoveUsed
3. Track comprehensive statistics:
   - Clean hits landed (int)
   - Total damage dealt (float)
   - Combos completed (int)
   - Predefined sequences landed (int)
   - Special moves landed (int)
   - Knockdowns inflicted (int)
   - Blocks successful (int)
   - Parries successful (int)
   - Dodges successful (int)
   - Aggression time accumulated (float, seconds)
   - Exhaustion count (int)
4. Calculate total score:
   - Method: `CalculateTotalScore()` → float
   - Apply weights from ScoringWeights to each statistic
   - Return accumulated score
5. Track aggression time:
   - In `Update()`: check if character in offensive range (query distance to opponent)
   - If yes: accumulate aggression time
   - Offensive range: < 3 units from opponent (configurable)
6. Expose properties: `TotalScore`, `CleanHitsLanded`, etc. (all stats)
7. Add method `ResetScore()` to clear stats (called between rounds)
8. Fire event: `OnScoreChanged(float newScore)` whenever score updates

**Design Guidance:**
- Scoring is cumulative throughout round
- Score updates in real-time (for UI display)
- Aggression time rewards offensive pressure
- All stats tracked independently, then weighted for final score

**Verification Checklist:**
- [ ] Component initializes correctly
- [ ] All combat actions tracked
- [ ] Statistics increment correctly
- [ ] Total score calculation accurate
- [ ] Aggression time tracked
- [ ] Events fire correctly

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/Scoring/CharacterScoringTests.cs`

Test cases:
- Landing hit increments clean hits and updates score
- Completing combo sequence adds sequence points
- Successful parry adds parry points
- Special move landed adds special move points
- Exhaustion increments exhaustion count
- CalculateTotalScore applies weights correctly
- ResetScore clears all statistics

**Commit Message Template:**
```
feat(scoring): implement CharacterScoring component

- Track all combat actions comprehensively
- Calculate weighted score from statistics
- Track aggression time for momentum scoring
- Fire score update events for UI
```

**Estimated Tokens:** ~12,000

---

### Task 9: Integrate Scoring with RoundManager

**Goal:** Use scoring system to determine round winner when time expires.

**Files to Modify:**
- `Assets/Knockout/Scripts/Systems/RoundManager.cs` - Add judge decision logic

**Prerequisites:**
- Task 8 complete (CharacterScoring exists)
- Understand RoundManager round flow

**Implementation Steps:**

1. In RoundManager:
   - Get references to both CharacterScoring components (player and opponent)
   - Subscribe to round timer expiry event
2. Implement judge decision logic:
   - Method: `DetermineJudgeWinner()` → Character (or null for draw)
   - When round timer expires without knockout:
     - Query both characters' `TotalScore`
     - Compare scores
     - Higher score wins round
     - Equal scores: draw (or sudden death, configurable)
   - Award round to winner via existing round win logic
3. Add configuration:
   - Field: `allowDraws` (bool, default false)
   - If draw and draws not allowed: continue round (sudden death) or tie-breaker rule
4. Display judge decision:
   - Fire event: `OnJudgeDecision(Character winner, float score1, float score2)`
   - UI will display decision (Phase 5)
5. Reset scores at round start

**Design Guidance:**
- Judge decision only happens on timer expiry
- Knockout still primary win condition (scoring is fallback)
- Scoring differential represents momentum advantage
- Clear feedback to players on who won and why

**Verification Checklist:**
- [ ] Judge decision logic functional
- [ ] Round awarded to higher score on timer expiry
- [ ] Draw handling implemented
- [ ] Scores reset between rounds
- [ ] Events fire correctly

**Testing Instructions:**

Update PlayMode test: `Assets/Knockout/Tests/PlayMode/Systems/RoundManagerTests.cs`

Test cases:
- Timer expiry triggers judge decision
- Higher score wins round
- Equal scores handled (draw or tie-breaker)
- Scores reset on new round start
- OnJudgeDecision event fires with correct data

**Commit Message Template:**
```
feat(scoring): integrate judge decision with RoundManager

- Determine round winner by score on timer expiry
- Award round to higher scoring character
- Handle draws and tie-breakers
- Reset scores between rounds
```

**Estimated Tokens:** ~7,000

---

### Task 10: Add Round Timer to RoundManager

**Goal:** Implement round timer that expires and triggers judge decision.

**Files to Modify:**
- `Assets/Knockout/Scripts/Systems/RoundManager.cs` - Add timer logic

**Prerequisites:**
- Understand RoundManager state machine

**Implementation Steps:**

1. Add timer configuration:
   - Field: `roundDuration` (float, seconds, default 180s = 3 minutes)
   - Field: `enableRoundTimer` (bool, default true, allows disabling for practice)
2. Track timer state:
   - Current round time remaining (float)
   - Property: `RoundTimeRemaining` (for UI)
   - Property: `RoundProgress` (0-1, for UI timer bar)
3. Implement timer countdown:
   - In `Update()` during Fighting state:
     - Decrement time remaining
     - Fire `OnRoundTimeChanged(float timeRemaining)` event (for UI)
     - When time reaches 0: trigger judge decision
4. Reset timer on round start
5. Pause timer during round end/countdown states

**Design Guidance:**
- Standard round duration: 3 minutes (180 seconds)
- Timer only counts during active fighting (not countdown, not round end)
- Timer expiry triggers judge decision (previous task)
- UI will display timer (Phase 5)

**Verification Checklist:**
- [ ] Timer counts down during round
- [ ] Timer resets on new round
- [ ] Timer expiry triggers judge decision
- [ ] Timer pauses during non-fighting states
- [ ] Events fire for UI updates

**Testing Instructions:**

Update PlayMode test: `Assets/Knockout/Tests/PlayMode/Systems/RoundManagerTests.cs`

Test cases:
- Timer counts down from round duration
- Timer reaches 0 and triggers judge decision
- Timer resets on new round start
- Timer pauses during countdown state
- OnRoundTimeChanged events fire

**Commit Message Template:**
```
feat(scoring): add round timer to RoundManager

- Implement configurable round duration timer
- Count down during active fighting
- Trigger judge decision on expiry
- Fire events for UI timer display
```

**Estimated Tokens:** ~5,000

---

### Task 11: Create Default ScoringWeights Asset

**Goal:** Create default scoring configuration with balanced weights.

**Files to Create:**
- `Assets/Knockout/Data/Scoring/DefaultScoringWeights.asset` - ScoringWeights instance

**Prerequisites:**
- Task 7 complete (ScoringWeights exists)

**Implementation Steps:**

1. Create DefaultScoringWeights asset
2. Configure with balanced default values from DD-005:
   - Clean Hit: 1
   - Combo Hit Bonus: 0.5 (per hit)
   - Combo Sequence: 5
   - Special Move: 8
   - Knockdown: 10
   - Damage Dealt Weight: 0.1 (1 damage = 0.1 points)
   - Block: 0.5
   - Parry: 2
   - Dodge: 1
   - Aggression: 0.1 points/sec
   - Exhaustion Penalty: -3
3. Save in `Assets/Knockout/Data/Scoring/`

**Design Guidance:**
- Values emphasize offensive aggression (landing hits) over pure defense
- Parries worth more than blocks (skill-based)
- Special moves and knockdowns highest value (rare, impactful)
- These can be tuned based on playtesting

**Verification Checklist:**
- [ ] Asset created successfully
- [ ] Values balanced and match design
- [ ] Asset assignable to CharacterScoring

**Testing Instructions:**

Manual verification:
- Asset loads without errors
- Values reasonable (no extreme outliers)

**Commit Message Template:**
```
feat(scoring): add default scoring weights asset

- Configure balanced default weights
- Emphasize offensive aggression
- Reward skill-based defense (parry > block)
```

**Estimated Tokens:** ~2,000

---

### Task 12: Update Character Prefab with Scoring

**Goal:** Add scoring component to character prefab.

**Files to Modify:**
- `Assets/Knockout/Prefabs/Characters/BaseCharacter.prefab` - Add component

**Prerequisites:**
- Task 8 complete (CharacterScoring exists)
- Task 11 complete (DefaultScoringWeights exists)

**Implementation Steps:**

1. Open BaseCharacter prefab
2. Add `CharacterScoring` component
3. Assign dependencies (auto-find where possible):
   - Character Controller
   - All action components (Combat, Combo, Dodge, Parry, Stamina, SpecialMoves)
4. Assign ScoringWeights: DefaultScoringWeights
5. Configure aggression range: 3 units
6. Verify initialization
7. Test in play mode

**Design Guidance:**
- Scoring component subscribes to all other components' events
- Initialize last (after all other components ready)

**Verification Checklist:**
- [ ] Component added to prefab
- [ ] Dependencies assigned
- [ ] Scoring weights assigned
- [ ] Component initializes without errors
- [ ] Score tracking functional in play mode

**Testing Instructions:**

Manual testing:
- Enter play mode
- Perform various actions (attack, block, parry, combo)
- Verify score increases
- Check score calculation accurate

**Commit Message Template:**
```
feat(scoring): add scoring component to character prefab

- Add CharacterScoring component
- Assign default scoring weights
- Integrate into component initialization
```

**Estimated Tokens:** ~3,000

---

### Task 13: Implement Momentum Tracking

**Goal:** Visualize momentum advantage via scoring differential (optional enhancement).

**Files to Create:**
- `Assets/Knockout/Scripts/Systems/MomentumTracker.cs` - Utility component (optional)

**Prerequisites:**
- Task 8 complete (CharacterScoring exists)

**Implementation Steps:**

1. Create `MomentumTracker` utility (optional, can be integrated into RoundManager)
2. Track scoring differential:
   - Query both characters' scores
   - Calculate momentum: `(player.score - opponent.score) / maxScore`
   - Momentum range: [-1, 1] (negative = opponent advantage, positive = player advantage)
3. Expose property: `MomentumValue` (float, -1 to 1)
4. Fire event: `OnMomentumChanged(float momentum)` for UI
5. Momentum represents "who's winning" in current round

**Design Guidance:**
- Momentum is derived from scoring, not separate resource
- Purely informational (doesn't affect gameplay mechanics)
- UI can display momentum bar (Phase 5)
- Reflects DD-006 (momentum-based combat philosophy)

**Verification Checklist:**
- [ ] Momentum calculated correctly from score differential
- [ ] Momentum range clamped to [-1, 1]
- [ ] Events fire for UI updates
- [ ] Momentum resets each round

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/Systems/MomentumTrackerTests.cs` (if implemented)

Test cases:
- Momentum positive when player scoring more
- Momentum negative when opponent scoring more
- Momentum zero when scores equal
- Momentum resets on new round

**Commit Message Template:**
```
feat(scoring): add momentum tracking system

- Calculate momentum from scoring differential
- Expose momentum value for UI display
- Reflect momentum-based combat philosophy
```

**Estimated Tokens:** ~5,000

---

### Task 14: Comprehensive Integration Testing

**Goal:** Verify special moves and scoring systems work correctly in all scenarios.

**Files to Create:**
- `Assets/Knockout/Tests/PlayMode/Integration/SpecialMovesIntegrationTests.cs`
- `Assets/Knockout/Tests/PlayMode/Integration/ScoringIntegrationTests.cs`

**Prerequisites:**
- All previous tasks complete

**Implementation Steps:**

1. Create special moves integration tests:
   - Test: Special move execution with cooldown + stamina gating
   - Test: Special move triggers special knockdown
   - Test: Special move damage and knockback multipliers
   - Test: Cooldown prevents spam
   - Test: Insufficient stamina blocks special move
   - Test: Special move during invalid state fails
2. Create scoring integration tests:
   - Test: All combat actions tracked and scored
   - Test: Score calculation accurate with weights
   - Test: Judge decision on timer expiry
   - Test: Higher score wins round
   - Test: Scores reset between rounds
   - Test: Momentum tracks correctly (if implemented)
   - Test: Comprehensive round scenario (attacks, blocks, parries, combos, specials)
3. Test edge cases:
   - Special move with exactly enough stamina
   - Timer expiry during attack animation
   - Equal scores (draw handling)
   - Score overflow (very long round)
   - Knockdown during special move animation

**Design Guidance:**
- Test realistic combat flows
- Verify scoring accuracy (manual calculation vs component)
- Check event firing order

**Verification Checklist:**
- [ ] All integration tests pass
- [ ] Special moves functional in gameplay
- [ ] Scoring accurate and reliable
- [ ] Judge decisions correct
- [ ] No console errors or warnings
- [ ] Performance maintained (60fps)

**Testing Instructions:**

Run all tests:
- EditMode tests (special moves, scoring)
- PlayMode tests (all Phase 4 tests)
- Integration tests
- Verify all green

**Commit Message Template:**
```
test(special,scoring): add comprehensive integration tests

- Test special move execution and gating
- Test comprehensive scoring tracking
- Test judge decision logic
- Cover edge cases and timing scenarios
```

**Estimated Tokens:** ~10,000

---

### Task 15: Documentation and Code Cleanup

**Goal:** Document special moves and scoring systems, clean up code, finalize Phase 4.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/SPECIAL_MOVES_SYSTEM.md`
- `Assets/Knockout/Scripts/Characters/Components/SCORING_SYSTEM.md`

**Files to Modify:**
- All Phase 4 scripts - Remove debug logs, finalize comments

**Prerequisites:**
- All previous tasks complete
- All tests passing

**Implementation Steps:**

1. Create SPECIAL_MOVES_SYSTEM.md:
   - Overview of special move mechanics
   - How to create character signature moves
   - Cooldown and stamina gating explanation
   - Special knockdown configuration
   - Balancing tips (damage, cooldown, cost)
   - Troubleshooting
2. Create SCORING_SYSTEM.md:
   - Overview of comprehensive scoring
   - Explanation of all scoring metrics
   - How to configure scoring weights
   - Judge decision logic
   - Momentum-based philosophy
   - Balancing tips (weights tuning)
   - Troubleshooting
3. Review all scripts:
   - Remove debug logs
   - Finalize XML comments
   - Verify naming conventions
4. Update main documentation

**Design Guidance:**
- Documentation helps designers create balanced special moves
- Explain scoring philosophy for weight tuning
- Provide examples of common configurations

**Verification Checklist:**
- [ ] SPECIAL_MOVES_SYSTEM.md comprehensive
- [ ] SCORING_SYSTEM.md comprehensive
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
docs(special,scoring): add system documentation

- Create SPECIAL_MOVES_SYSTEM.md guide
- Create SCORING_SYSTEM.md guide
- Explain balancing and configuration
- Remove debug logging and finalize code
```

**Estimated Tokens:** ~6,000

---

## Phase 4 Verification

**Completion Checklist:**
- [ ] All 15 tasks completed
- [ ] All tests passing (EditMode + PlayMode)
- [ ] Special move system functional (cooldown + stamina gating)
- [ ] Special knockdown triggered by special moves
- [ ] Comprehensive scoring tracking all actions
- [ ] Judge decision logic working correctly
- [ ] Round timer functional
- [ ] Character prefab updated with special moves and scoring
- [ ] Default configurations created
- [ ] Documentation complete
- [ ] Code reviewed and cleaned
- [ ] No console errors or warnings
- [ ] Performance maintained (60fps, Profiler shows CharacterSpecialMoves and CharacterScoring < 0.1ms per frame each, no GC allocations)

**Integration Points for Next Phase:**
- Special move events available for UI (cooldown indicator - Phase 5)
- Scoring events available for UI (score display, momentum bar - Phase 5)
- Round timer available for UI (timer display - Phase 5)
- Judge decision events for UI (round winner announcement - Phase 5)

**Known Limitations:**
- Special move and scoring UI not implemented (Phase 5)
- AI doesn't use special moves strategically (post-Phase 5)
- No special move variety (one per character, can expand later)
- Scoring weights not playtested (requires Phase 5 UI for tuning)

---

## Next Phase

After Phase 4 completion and verification, proceed to:

**[Phase 5: UI Systems & Training Mode](Phase-5.md)**

Implement contextual UI for all new systems and basic training/practice mode.
