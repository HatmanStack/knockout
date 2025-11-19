# Combo System Documentation

## Overview

The combo system enables players to chain attacks together for increased damage and special effects. It features:
- **Natural Chains**: Any attack can chain into any other within timing windows
- **Variable Timing**: Different attacks have different chain windows (Jab easy, Uppercut precise)
- **Damage Scaling**: Heavy scaling to prevent infinite combos (75%, 50% floor)
- **Predefined Sequences**: Character-specific combos with damage bonuses
- **Combo Breaking**: Defender can interrupt by blocking or hitting back
- **Integration**: Works with parry system (counter windows) and round management

## Architecture

### Core Components

**CharacterComboTracker** (`CharacterComboTracker.cs`)
- Tracks combo state (count, sequence history, timing)
- Detects natural chains and predefined sequences
- Applies damage scaling and sequence bonuses
- Handles combo interruption and timeout
- Fires events for UI/VFX integration

**Data Configuration**

**ComboChainData** (`ComboChainData.cs`)
- Natural chain timing windows per attack type
- Global combo timeout (prevents infinite combos)
- Damage scaling progression array
- Combo break window configuration
- Counter window damage bonus

**ComboSequenceData** (`ComboSequenceData.cs`)
- Predefined attack sequence pattern
- Timing window for sequence completion
- Damage bonus multiplier
- Special properties (enhanced knockback, guaranteed stagger)
- Optional VFX/audio clips

## How It Works

### Natural Combo Chains

Players can chain any attack into any other attack if performed within the chain window:

```
Jab → (18 frames / ~0.3s) → Any Attack
Hook → (12 frames / ~0.2s) → Any Attack
Uppercut → (6 frames / ~0.1s) → Any Attack
```

**Chain Window**: Starts after previous attack's recovery frames end.

**Example Flow**:
1. Player lands Jab (attack type 0)
2. CharacterComboTracker registers hit, starts combo (count = 1)
3. Player lands Hook within 18 frames
4. Combo continues (count = 2)
5. Player lands Uppercut within 12 frames
6. Combo continues (count = 3)

### Damage Scaling

Each hit in a combo deals progressively less damage:

| Hit # | Damage Multiplier | Example (10 damage base) |
|-------|-------------------|--------------------------|
| 1st   | 100% (1.0x)       | 10 damage                |
| 2nd   | 75% (0.75x)       | 7.5 damage               |
| 3rd   | 50% (0.5x)        | 5 damage                 |
| 4th+  | 50% (0.5x)        | 5 damage (floor)         |

**Application Order**:
1. Base attack damage (from AttackData)
2. Hurtbox damage multiplier (head vs body)
3. **→ Combo scaling (CharacterComboTracker)**
4. Sequence bonus (if predefined combo completed)
5. Character stats multipliers

### Predefined Combo Sequences

Characters can have unique combo sequences that grant bonuses:

**Example Sequences**:
- **"1-2" Combo**: Jab-Jab (1.25x damage bonus on 2nd Jab)
- **"1-2-Hook"**: Jab-Jab-Hook (1.5x bonus + enhanced knockback)
- **"Hook-Uppercut Finisher"**: Hook-Uppercut (2.0x bonus + guaranteed stagger)

**Detection**:
- System checks last N attacks against all predefined sequences
- When match found, fires `OnComboSequenceCompleted` event
- Bonus applies to final hit AFTER combo scaling

**Example**:
```
Jab (10 dmg) → Jab (7.5 dmg) → Hook (5 dmg base × 1.5 bonus = 7.5 dmg)
```

### Combo Interruption

Combos can be broken in two ways:

**1. Defender Blocks**
- If defender blocks a hit during combo, attacker's combo breaks
- Immediate break (no strict timing window in current implementation)
- Fires `OnComboBroken` event

**2. Attacker Gets Hit**
- If attacker takes damage, their combo immediately breaks
- Prevents "trading" hits while maintaining combo

### Combo Timeout

Combos automatically reset after inactivity:

- **Global Timeout**: 90 frames (~1.5 seconds) between any hits
- If no hit lands within timeout, combo resets
- Prevents combos from persisting indefinitely

### Parry Counter Window Integration

Successful parries open a counter window:

- Defender can attack during attacker's parry stagger
- **Counter Bonus**: First hit during counter window gets damage boost (1.25x)
- Counter window tracked by CharacterComboTracker
- Bonus applies in addition to sequence bonuses

### Round Reset

Combos reset at round transitions:

- **Round Start**: All combos cleared (prevents cross-round persistence)
- **Round End**: All combos cleared for clean slate
- Handled by RoundManager.ResetAllCombos()

## Configuration Guide

### Creating Default Combo Chain Data

1. In Unity, right-click in Project → Create → Knockout → Combo Chain Data
2. Configure timing windows:
   - **Jab Chain Window**: 18 frames (forgiving)
   - **Hook Chain Window**: 12 frames (medium)
   - **Uppercut Chain Window**: 6 frames (tight)
3. Set damage scaling: [1.0, 0.75, 0.5, 0.5]
4. Set global timeout: 90 frames
5. Set combo break window: 8 frames
6. Set counter window bonus: 1.25x

### Creating Character-Specific Combo Sequences

1. Right-click → Create → Knockout → Combo Sequence Data
2. Name it descriptively (e.g., "BaseCharacter_Jab12Combo")
3. Configure:
   - **Sequence Name**: Display name (e.g., "1-2 Combo")
   - **Attack Sequence**: Array of attack types [0, 0] = Jab-Jab
   - **Timing Window**: Tighter than natural (e.g., 15 frames)
   - **Damage Bonus Multiplier**: 1.25x - 2.0x
   - **Enhanced Knockback**: Enable for finisher combos
   - **Guaranteed Stagger**: Enable for interrupt combos
4. Assign to CharacterComboTracker on character prefab

### Assigning to Character

1. Open character prefab
2. Add CharacterComboTracker component (if not present)
3. Assign:
   - **Combo Chain Data**: Default or character-specific
   - **Combo Sequences**: Array of character's unique sequences
4. Save prefab

## Events for UI/VFX Integration

CharacterComboTracker fires events for visual feedback:

**OnComboStarted** `()`
- Fires when first hit lands
- Use for: Combo counter UI appears

**OnComboHitLanded** `(int hitNumber, float damageDealt)`
- Fires for each hit in combo
- Use for: Combo counter increment, damage numbers, hit sparks

**OnComboSequenceCompleted** `(ComboSequenceData sequence)`
- Fires when predefined sequence detected
- Use for: Special VFX, screen flash, combo name display

**OnComboBroken** `(int finalComboCount)`
- Fires when combo interrupted by block/hit
- Use for: UI reset, "Combo Broken!" message

**OnComboEnded** `(int finalComboCount, float totalDamage)`
- Fires when combo times out naturally
- Use for: UI fade out, combo summary

## Balancing Combos

### Timing Windows

**Tight windows** (6-8 frames):
- Require precise input
- Used for hard combos (Uppercut chains)
- Higher skill ceiling

**Medium windows** (10-15 frames):
- Accessible but require attention
- Used for standard combos (Hook chains)
- Balanced difficulty

**Loose windows** (15-20 frames):
- Easy to execute
- Used for beginner combos (Jab chains)
- Low skill floor

### Damage Bonuses

**Conservative** (1.1x - 1.25x):
- Slight reward for execution
- Used for easy sequences
- Doesn't break balance

**Moderate** (1.3x - 1.5x):
- Meaningful reward
- Used for medium difficulty sequences
- Competitive advantage

**Strong** (1.6x - 2.0x):
- High reward for high execution
- Used for difficult sequences (tight timing)
- Can turn fights around

**Rule of Thumb**: Harder sequences → Tighter timing → Higher rewards

### Testing Balance

1. Play test combos extensively
2. Measure:
   - **Execution Rate**: How often players complete sequences
   - **Damage Output**: Total damage vs time investment
   - **Fun Factor**: Does it feel rewarding?
3. Adjust:
   - If too easy: Tighten timing or reduce bonus
   - If too hard: Loosen timing or increase bonus
   - If overpowered: Reduce bonus or require tighter timing

## Troubleshooting

### Combo Not Registering

**Symptom**: Attacks land but combo count doesn't increment

**Checks**:
1. CharacterComboTracker component present and initialized?
2. ComboChainData assigned?
3. Chain window sufficient? (Check AttackData.RecoveryFrames + chain window)
4. Hitting within global timeout? (90 frames default)

### Damage Scaling Not Applied

**Symptom**: All hits deal full damage

**Checks**:
1. HitboxData modified to query CharacterComboTracker? (Should be integrated)
2. CharacterComboTracker.GetCurrentDamageMultiplier() returning < 1.0?
3. Damage calculation order correct? (Check HitboxData.CalculateHitData())

### Sequence Not Detected

**Symptom**: Correct attacks landed but no sequence bonus

**Checks**:
1. ComboSequenceData assigned to CharacterComboTracker?
2. Attack sequence array matches exactly? [0, 0, 1] = Jab-Jab-Hook
3. Timing within sequence window? (Often tighter than natural chain)
4. Sequence cached correctly? (Check logs on Initialize)

### Combo Not Breaking on Block

**Symptom**: Combos continue even when defender blocks

**Checks**:
1. CharacterHealth.OnHitBlocked event implemented?
2. HitboxData.SendHitToTarget() checking for blocking?
3. CharacterCombat.IsBlocking returning true when blocking?

### Combo Persists Across Rounds

**Symptom**: Combo count doesn't reset between rounds

**Checks**:
1. RoundManager calling ResetAllCombos()?
2. CharacterComboTracker.ResetCombo() public and accessible?
3. Round start/end events firing?

## Performance Considerations

The combo system is optimized for 60fps gameplay:

**Optimizations Applied**:
- Sequence patterns cached on initialization (avoid ScriptableObject access)
- Attack history list pre-allocated (capacity 10)
- No LINQ or allocations in hot path (FixedUpdate)
- Efficient array-based sequence matching
- Early exit on pattern mismatch

**Expected Performance**:
- CharacterComboTracker.FixedUpdate(): < 0.1ms per frame
- Sequence detection: O(N×M) where N=sequences, M=pattern length (typically 3-5)
- No GC allocations during gameplay

## API Reference

### CharacterComboTracker Public Methods

```csharp
// Initialize combo tracker
void Initialize()

// Register hit landed (called by HitboxData)
float RegisterHitLanded(int attackTypeIndex, float baseDamage)

// Get current damage multiplier for next hit
float GetCurrentDamageMultiplier()

// Reset combo state to zero
void ResetCombo()

// Break combo (from external interrupt)
void BreakCombo()
```

### CharacterComboTracker Public Properties

```csharp
int ComboCount { get; }                        // Current combo hit count
bool IsInCombo { get; }                        // Whether combo active
bool IsInCounterWindow { get; }                // In parry counter window
IReadOnlyList<int> CurrentSequence { get; }    // Attack sequence history
```

### CharacterComboTracker Events

```csharp
event Action OnComboStarted;
event Action<int, float> OnComboHitLanded;
event Action<ComboSequenceData> OnComboSequenceCompleted;
event Action<int> OnComboBroken;
event Action<int, float> OnComboEnded;
```

## Design Philosophy

The combo system follows these principles:

**1. Momentum-Based Combat**
- Offense builds advantage (combos, damage scaling)
- Defense resets momentum (blocking breaks combos)
- Ebb and flow gameplay

**2. Skill Expression**
- Variable timing windows reward precision
- Difficult sequences grant higher rewards
- Accessible floor, high ceiling

**3. Character Variety**
- Unique sequences per character
- Different playstyles (rushdown vs spacing)
- Signature combos create identity

**4. Fair Counterplay**
- Defender can break combos by blocking
- Heavy damage scaling prevents TOD (Touch of Death)
- Timeout prevents infinite pressure

## Future Extensions

Potential additions for later phases:

- **Combo Challenges**: Training mode missions for specific sequences
- **Combo Trials**: Character-specific combo tutorials
- **Combo Damage Display**: Real-time damage counter during combos
- **Combo Ender Moves**: Special attacks that end combos with flourish
- **AI Combo Execution**: Train AI to perform character combos
- **Combo Leaderboard**: Track longest/highest damage combos

## Related Systems

- **Stamina System**: Attacks cost stamina (limits combo spam)
- **Parry System**: Counter windows integrate with combo tracking
- **Hit Detection**: HitboxData applies combo damage scaling
- **Round Manager**: Resets combos at round transitions
- **Judge Scoring**: Combos contribute to round scoring (Phase 4)
