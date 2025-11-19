# Defense Systems Documentation

## Overview

Phase 2 implements advanced defensive mechanics for the Knockout boxing game:
- **Dodge System**: Directional dodging with invincibility frames (i-frames)
- **Parry System**: Timing-based perfect block that negates damage and staggers attacker

Both systems are **stamina-free** (defensive actions don't consume stamina) and use **frame-based timing** for precision (60fps target).

---

## Dodge System

### Components

**DodgeData** (`Assets/Knockout/Scripts/Characters/Data/DodgeData.cs`)
- ScriptableObject defining dodge configuration
- Frame-based timing for i-frame precision
- Default values: 18 frames total, 8 i-frames starting at frame 2

**DodgingState** (`Assets/Knockout/Scripts/Combat/States/DodgingState.cs`)
- Combat state for active dodge
- Tracks current frame and i-frame window
- `IsInvulnerable` property for hit detection
- Directional movement via Rigidbody velocity

**CharacterDodge** (`Assets/Knockout/Scripts/Characters/Components/CharacterDodge.cs`)
- Manages dodge execution and cooldown
- Subscribes to dodge input events
- Exposes `CanDodge`, `IsDodging`, `CooldownProgress` properties

### How It Works

1. **Input**: Player presses Q (left), E (right), or Left Ctrl (back)
2. **Validation**: CharacterDodge checks cooldown and valid state
3. **Execution**: CombatStateMachine.TriggerDodge() transitions to DodgingState
4. **I-Frames**: DodgingState.IsInvulnerable is true during frames 2-10
5. **Hit Detection**: CharacterHealth checks IsInvulnerable before applying damage
6. **Completion**: Auto-transitions to IdleState when dodge completes

### Configuration

**Tunable Parameters** (DodgeData):
- `DodgeDurationFrames`: Total dodge animation length (default: 18 frames)
- `IFrameStartFrame`: When invincibility begins (default: 2 frames)
- `IFrameDurationFrames`: How long invincibility lasts (default: 8 frames)
- `DodgeDistance`: How far character moves (default: 1.5 units)
- `CooldownFrames`: Prevent spam (default: 12 frames)

**Frame-to-Second Conversion** (60fps):
- 6 frames = 0.1s (tight timing)
- 12 frames = 0.2s (medium timing)
- 18 frames = 0.3s (generous timing)

### Valid State Transitions

**Can dodge from:**
- IdleState
- BlockingState (cancel block into dodge)
- ExhaustedState (defensive action allowed)

**Cannot dodge from:**
- AttackingState
- DodgingState (already dodging)
- KnockedDownState, KnockedOutState

---

## Parry System

### Components

**ParryData** (`Assets/Knockout/Scripts/Characters/Data/ParryData.cs`)
- ScriptableObject defining parry configuration
- Frame-based parry window (default: 6 frames = ~0.1s)
- Stagger and counter window durations

**ParryStaggerState** (`Assets/Knockout/Scripts/Combat/States/ParryStaggerState.cs`)
- Combat state for attacker after being parried
- Cannot perform any actions during stagger
- Auto-transitions to IdleState when complete

**CharacterParry** (`Assets/Knockout/Scripts/Characters/Components/CharacterParry.cs`)
- Detects parry timing (block pressed within window)
- Executes parry success (stagger attacker, open counter window)
- Manages cooldown and counter window tracking

### How It Works

1. **Input**: Player presses block button (Left Shift)
2. **Timing Record**: CharacterParry records frame when block pressed
3. **Hit Detection**: CharacterHealth.TakeDamage() calls TryParry()
4. **Validation**: Check if block press was within parry window (6 frames)
5. **Success**: Negate damage, stagger attacker, open counter window
6. **Counter Window**: Defender has 0.5s to counterattack while attacker staggered
7. **Completion**: Attacker auto-transitions to Idle after stagger duration

### Configuration

**Tunable Parameters** (ParryData):
- `ParryWindowFrames`: How early before hit parry is valid (default: 6 frames)
- `ParrySuccessDurationFrames`: Parry state duration (default: 12 frames)
- `AttackerStaggerDuration`: How long attacker is vulnerable (default: 0.5s)
- `CounterWindowDuration`: How long defender can counter (default: 0.5s)
- `ParryCooldownFrames`: Prevent spam (default: 18 frames)

### Defense Priority Order

When hit lands, systems check in this order:

1. **Dodge I-Frames** (highest priority)
   - If `IsInvulnerable == true`, hit ignored completely

2. **Parry Timing**
   - If block pressed within parry window, hit parried
   - Attacker staggered, no damage taken

3. **Normal Block**
   - If blocking, damage reduced by 75%

4. **Full Damage**
   - No defense active, full damage applied

---

## Integration with Existing Systems

### Input System

**New Actions Added** (`KnockoutInputActions.inputactions`):
- `DodgeLeft`: Q key
- `DodgeRight`: E key
- `DodgeBack`: Left Ctrl key

**CharacterInput Events**:
- `OnDodgeLeftPressed`
- `OnDodgeRightPressed`
- `OnDodgeBackPressed`

### Combat State Machine

**New States**:
- `DodgingState`: Active dodge with i-frames
- `ParryStaggerState`: Attacker stagger after parry

**New Transitions**:
- Idle/Blocking/Exhausted → Dodging
- Dodging → Idle (auto on completion)
- Any → ParryStagger (when parried)
- ParryStagger → Idle (auto on completion)

**Auto-Transition Logic** (`CharacterCombat.HandleAutoTransitions()`):
- Checks `DodgingState.IsDodgeComplete()`
- Checks `ParryStaggerState.IsStaggerComplete()`
- Automatically returns to Idle when complete

### Hit Detection

**CharacterHealth.TakeDamage() Flow**:
```csharp
1. Check if dead → return
2. Check dodge i-frames → OnHitDodged, return
3. Check parry timing → OnHitParried, stagger attacker, return
4. Calculate damage (with block reduction if blocking)
5. Apply damage and trigger reaction
```

**New Events**:
- `CharacterHealth.OnHitDodged`: Hit ignored during i-frames
- `CharacterHealth.OnHitParried`: Hit parried successfully

### Animations

**New Animator Parameters**:
- `DodgeLeft`: Trigger for left dodge animation
- `DodgeRight`: Trigger for right dodge animation
- `DodgeBack`: Trigger for back dodge animation

**CharacterAnimator Methods**:
- `TriggerDodgeLeft()`
- `TriggerDodgeRight()`
- `TriggerDodgeBack()`

**Animation Fallbacks** (if animations missing):
- Parry: Uses existing Block animation
- Back dodge: Can mirror left/right dodge
- Stagger: Uses light hit reaction animation

---

## Setup Instructions (Unity Editor)

### 1. Create Data Assets

**Create DodgeData**:
1. Right-click in Project > Create > Knockout > Dodge Data
2. Name it `DefaultDodge`
3. Configure values (defaults are balanced)
4. Save in `Assets/Knockout/Data/Defense/`

**Create ParryData**:
1. Right-click in Project > Create > Knockout > Parry Data
2. Name it `DefaultParry`
3. Configure values
4. Save in `Assets/Knockout/Data/Defense/`

### 2. Update Character Prefab

**Add Components to BaseCharacter**:
1. Add `CharacterDodge` component
   - Assign DodgeData
   - Dependencies auto-find (CharacterInput, CombatStateMachine)

2. Add `CharacterParry` component
   - Assign ParryData
   - Dependencies auto-find

3. Call `Initialize()` in CharacterController:
```csharp
void Start()
{
    // ... existing initialization
    characterDodge?.Initialize();
    characterParry?.Initialize();
}
```

### 3. Configure Animator Controller

**Add Dodge Triggers**:
1. Open BaseCharacterAnimator.controller
2. Add parameters:
   - DodgeLeft (Trigger)
   - DodgeRight (Trigger)
   - DodgeBack (Trigger)

3. Create states:
   - DodgeLeft → left_dodge.fbx
   - DodgeRight → right_dodge.fbx
   - DodgeBack → mirror left or right dodge

4. Add transitions:
   - Any State → Dodge states (on trigger)
   - Dodge states → Idle (on animation complete)

5. Set transition times to ~0.1s for responsive feel

### 4. Configure Input Actions Asset

**Add to KnockoutInputActions.inputactions** (or regenerate):
1. Open Input Actions window
2. Add to Gameplay action map:
   - DodgeLeft (Button, binding: Q)
   - DodgeRight (Button, binding: E)
   - DodgeBack (Button, binding: Left Ctrl)
3. Regenerate C# class if using code generation
4. Copy to Resources/Input/ for runtime loading

---

## Testing

### Manual Testing Checklist

**Dodge System**:
- [ ] Press Q/E/Left Ctrl triggers dodge in correct direction
- [ ] Character is invulnerable during early dodge frames (test vs incoming hit)
- [ ] Character can be hit during dodge recovery (late frames)
- [ ] Cooldown prevents dodge spam
- [ ] Cannot dodge while attacking
- [ ] Can dodge from blocking (cancel block into dodge)

**Parry System**:
- [ ] Holding block = normal block (damage reduced)
- [ ] Tapping block just before hit = parry (no damage, attacker staggered)
- [ ] Tapping block too early = normal block
- [ ] Attacker cannot act during stagger
- [ ] Defender can attack during counter window
- [ ] Cooldown prevents parry spam

**Integration**:
- [ ] Dodge i-frames take priority over parry
- [ ] Parry takes priority over normal block
- [ ] Systems work when stamina depleted (defensive actions free)
- [ ] No console errors during gameplay

### Performance Testing

**Target**: 60fps maintained with dodge/parry active

**Profile**:
- CharacterDodge.FixedUpdate() < 0.1ms
- CharacterParry.FixedUpdate() < 0.1ms
- No GC allocations during dodge/parry

---

## Troubleshooting

### Dodge Not Working

**Symptom**: Dodge input does nothing
- Check CharacterDodge is initialized (`Initialize()` called)
- Check DodgeData is assigned
- Check Input Actions asset is loaded
- Check character is in valid state (not attacking/knocked down)

**Symptom**: Dodge works but no i-frames
- Check DodgeData i-frame values (start + duration <= total duration)
- Check CharacterHealth has CombatStateMachine reference
- Check `DodgingState.IsInvulnerable` property

### Parry Not Working

**Symptom**: Block always normal, never parries
- Check CharacterParry is initialized
- Check ParryData is assigned
- Check block input is firing (CharacterInput.OnBlockPressed)
- Check parry window frames (may be too tight, increase to 10-12 frames for testing)

**Symptom**: Parry works but attacker not staggered
- Check HitData contains Attacker GameObject
- Check attacker has CombatStateMachine
- Check ParryStaggerState is in correct namespace

### Animation Issues

**Symptom**: Dodge animation doesn't play
- Check Animator has dodge triggers (DodgeLeft/Right/Back)
- Check dodge states exist in Animator Controller
- Check transitions from Any State to dodge states

**Symptom**: Animation plays but wrong direction
- Check DodgeDirection enum usage in DodgingState.Enter()
- Check CharacterAnimator trigger methods called correctly

---

## Advanced Tuning

### Difficulty Balancing

**Easy Mode** (forgiving):
- Dodge i-frames: 10-12 frames (66% of dodge)
- Dodge cooldown: 6 frames (almost no cooldown)
- Parry window: 10 frames (~0.167s)
- Parry cooldown: 12 frames

**Normal Mode** (balanced):
- Dodge i-frames: 8 frames (44% of dodge, current default)
- Dodge cooldown: 12 frames
- Parry window: 6 frames (~0.1s)
- Parry cooldown: 18 frames

**Hard Mode** (tight timing):
- Dodge i-frames: 6 frames (33% of dodge)
- Dodge cooldown: 18 frames
- Parry window: 4 frames (~0.067s)
- Parry cooldown: 24 frames

### Character Differentiation

Create character-specific DodgeData/ParryData variants:

**Fast Character**:
- Shorter dodge duration (15 frames)
- Same i-frame count (8 frames = higher % invulnerable)
- Shorter cooldown (8 frames)

**Slow Character**:
- Longer dodge duration (24 frames)
- Same i-frame count (8 frames = lower % invulnerable)
- Longer dodge distance (2.0 units)

**Parry Specialist**:
- Longer parry window (8-10 frames)
- Shorter cooldown (12 frames)
- Longer counter window (0.7s)

---

## Events for UI/VFX Integration

### Dodge Events

**DodgingState**:
- `OnDodgeStarted(CharacterCombat, DodgeDirection)`: Visual cue start
- `OnDodgeEnded(CharacterCombat)`: Visual cue end

**CharacterDodge**:
- `OnDodgeReady`: Cooldown complete (UI indicator)

**CharacterHealth**:
- `OnHitDodged(HitData)`: Successful dodge (VFX, sound, "EVADED" text)

### Parry Events

**ParryStaggerState**:
- `OnParryStaggered(CharacterCombat)`: Stagger start (VFX on attacker)
- `OnParryStaggerEnded(CharacterCombat)`: Stagger end

**CharacterParry**:
- `OnParrySuccess(CharacterCombat)`: Successful parry (VFX, sound, "PARRIED" text)
- `OnParryReady`: Cooldown complete
- `OnCounterWindowOpened`: Counter window start (UI indicator)
- `OnCounterWindowClosed`: Counter window end

**CharacterHealth**:
- `OnHitParried(HitData)`: Hit was parried

---

## Performance Optimization

### Frame-Based Timing Benefits

1. **Deterministic**: Same timing across all hardware
2. **Precise**: No floating-point drift
3. **Efficient**: Integer comparisons vs time delta accumulation

### Cooldown Tracking

- Uses frame counters (int) instead of timers (float)
- FixedUpdate ensures consistent frame rate
- No per-frame time calculations

### Event Efficiency

- Events fire only on state changes (not every frame)
- No polling for dodge/parry status
- UI subscribes to events, not Update polling

---

## Future Enhancements

### Potential Additions

1. **Dodge Attacks**: Attack during dodge recovery for mix-ups
2. **Perfect Dodge**: Bonus if dodge at exact moment (frame-perfect)
3. **Parry Counter Auto-Attack**: Automatic riposte on parry
4. **Dodge Direction Indicator**: Show available dodge directions
5. **Parry Training Mode**: Visual timing indicator for learning
6. **Advanced Parries**: Different parries for different attacks (high/low)

### AI Integration

Both systems designed for AI use:

**CharacterDodge.TryDodge(direction)**:
- AI can call directly
- Returns success/failure for decision making

**CharacterParry**:
- AI monitors incoming attacks
- Calls block input at calculated timing

---

## Related Documentation

- `Phase-0.md`: Architecture decisions (ADR-003, ADR-004, DD-003)
- `Phase-2.md`: Implementation tasks and verification
- `STAMINA_SYSTEM.md`: Stamina integration (dodge/parry are stamina-free)
- `INPUT_SYSTEM_SETUP.md`: Input configuration

---

## Changelog

**Phase 2 - Advanced Defense (v1.0.0)**:
- Initial implementation of dodge and parry systems
- Frame-based timing for precision
- Hit detection integration
- Combat state machine integration
