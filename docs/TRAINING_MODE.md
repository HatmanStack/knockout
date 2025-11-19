# Training Mode Guide

## Overview

Training Mode is a dedicated practice environment for learning and testing the Core Fighting Mechanics. It provides unlimited resources, controllable dummy AI, damage feedback, and quick reset capabilities.

---

## Accessing Training Mode

**Unity Editor:**
1. Open `TrainingMode.unity` scene (create if not exists)
2. Enter Play Mode

**Build:**
- Navigate to Training Mode from main menu (requires menu integration)

---

## Features

### 1. Unlimited Resources Toggles

#### Unlimited Stamina
**What it does:** Continuously restores stamina to maximum
**Use case:** Practice combos and special moves without stamina constraints
**Toggle:** Checkbox in Settings Panel

#### Unlimited Health
**What it does:** Continuously restores health to maximum
**Use case:** Test damage output without risk of knockout
**Toggle:** Checkbox in Settings Panel

**Note:** Both player and dummy have unlimited resources when toggled.

### 2. Damage Numbers Display

**What it does:** Shows floating damage values on every hit
**Benefits:**
- See exact damage dealt
- Verify combo scaling is working
- Compare different attack damages
- Test special move damage multipliers

**Toggle:** Checkbox in Settings Panel

**Color Coding:**
- White: Normal hits
- Yellow: Combo hits
- Red: Special move hits
- Orange: Critical hits

### 3. Dummy AI Behavior Controls

The dummy opponent can be set to different behaviors for specific practice scenarios:

#### Passive (Default)
- Stands idle
- Doesn't attack or defend
- Pure punching bag for damage testing

**Best for:**
- Learning attack timings
- Practicing combos
- Testing damage output

#### Blocking
- Always holds block
- Never attacks
- Tests combo breaking mechanics

**Best for:**
- Practicing chip damage
- Learning when combos get blocked
- Testing guard break scenarios

#### Dodging
- Attempts to dodge incoming attacks
- Reactive timing (~0.1s delay)
- No attacks

**Best for:**
- Practicing attack timing and prediction
- Testing dodge windows
- Learning to bait dodges

#### Counter
- Blocks incoming attacks
- Counter-attacks after successful block
- Tests spacing and defense

**Best for:**
- Practicing defensive play
- Learning to deal with counter-attackers
- Testing parry timing

**Behavior Selection:** Dropdown in Settings Panel (Passive / Blocking / Dodging / Counter)

### 4. Reset Button

**What it does:** Resets both characters to starting positions and states

**Resets:**
- Position and rotation
- Health to maximum
- Stamina to maximum
- Combo state
- Knockdown state

**Use case:** Quickly restart practice scenario without reloading scene

**Button:** "Reset" in Settings Panel

---

## Settings Panel

### Opening/Closing
- **Toggle Key:** Tab (configurable)
- **Default State:** Hidden

### Panel Contents
```
Training Mode Settings
----------------------
[ ] Unlimited Stamina
[ ] Unlimited Health
[ ] Show Damage Numbers

Dummy Behavior: [Passive ▼]

[Reset Positions]
```

---

## Setup Instructions (Unity Editor)

### Creating Training Mode Scene

1. **Duplicate Main Game Scene:**
   - Copy existing game scene
   - Rename to `TrainingMode.unity`
   - Save in `Assets/Knockout/Scenes/`

2. **Modify Scene:**
   - Remove or disable RoundManager (or set very long round timer)
   - Set AI character to use DummyAI instead of CharacterAI
   - Optionally simplify environment (grid floor, neutral lighting)

3. **Add Training UI:**
   ```
   Canvas
   └── TrainingModePanel
       ├── Title Text: "Training Mode Settings"
       ├── Unlimited Stamina Toggle
       ├── Unlimited Health Toggle
       ├── Show Damage Numbers Toggle
       ├── Dummy Behavior Dropdown
       └── Reset Button
   ```

4. **Add TrainingModeUI Component:**
   - Add to Canvas or dedicated GameObject
   - Assign character references (Player, Dummy)
   - Assign all UI element references
   - Configure toggle panel key (default: Tab)

5. **Setup DamageNumbersUI:**
   - Create damage number prefab (TextMeshProUGUI with animation)
   - Add DamageNumbersUI component
   - Assign prefab and character combat references
   - Link to TrainingModeUI toggle

6. **Setup DummyAI:**
   - Add DummyAI component to opponent character
   - Disable standard CharacterAI component
   - Default behavior: Passive
   - Configure reaction times if desired

### Component Hierarchy

```
Player Character
├── CharacterHealth
├── CharacterStamina
├── CharacterCombat
├── CharacterComboTracker
├── CharacterSpecialMoves
├── CharacterDodge
├── CharacterParry
└── CharacterInput

Dummy Character
├── CharacterHealth
├── CharacterStamina
├── CharacterCombat
├── CharacterComboTracker
├── CharacterSpecialMoves
├── CharacterDodge
├── CharacterParry
├── DummyAI (NEW - replaces CharacterAI)
└── CharacterInput (for simulated inputs)
```

---

## Practice Scenarios

### Scenario 1: Learning Basic Combos
**Setup:**
- Dummy Behavior: Passive
- Unlimited Stamina: ON
- Show Damage Numbers: ON

**Practice:**
- Execute Jab-Jab-Hook sequence
- Watch damage numbers to see combo scaling
- Try different attack sequences
- Learn timing windows between attacks

### Scenario 2: Testing Special Moves
**Setup:**
- Dummy Behavior: Passive
- Unlimited Stamina: ON (to spam special)
- Show Damage Numbers: ON

**Practice:**
- Use special move repeatedly
- Note damage compared to normal attacks
- Practice special move timing and range
- Test cooldown behavior (turn off unlimited stamina)

### Scenario 3: Practicing Defense
**Setup:**
- Dummy Behavior: Counter
- Unlimited Health: ON
- Unlimited Stamina: OFF (to learn stamina management)

**Practice:**
- Block counter-attacks
- Practice parry timing
- Learn to dodge counter-attacks
- Work on defensive stamina efficiency

### Scenario 4: Combo Breaking Practice
**Setup:**
- Dummy Behavior: Blocking
- Show Damage Numbers: ON

**Practice:**
- Try to land combos
- Notice when combos get blocked
- Learn which attacks can't be blocked mid-combo
- Practice breaking through guard

### Scenario 5: Stamina Management
**Setup:**
- Dummy Behavior: Passive
- Unlimited Stamina: OFF
- Unlimited Health: ON

**Practice:**
- Watch stamina bar depletion
- Learn attack costs
- Practice regeneration timing
- Avoid exhaustion state

---

## Tips and Best Practices

### Effective Training Workflow
1. Start with Passive dummy to learn mechanics
2. Progress to Blocking to understand interruptions
3. Use Dodging to improve timing
4. End with Counter to practice under pressure

### Using Reset Efficiently
- Set up position before starting practice
- Use Reset between attempts instead of manually repositioning
- Reset clears all state for clean practice runs

### Damage Number Interpretation
- Track average damage per combo
- Compare special move vs normal attack damage
- Verify combo scaling is working (decreasing damage per hit)
- Test predefined sequence bonuses

### Dummy Behavior Cycling
- Switch behaviors mid-session to practice different scenarios
- Don't get comfortable with one behavior - real opponents vary
- Counter behavior is closest to real match conditions

---

## Troubleshooting

### Issue: Unlimited toggles not working
**Cause:** Components not found or not assigned
**Fix:** Check TrainingModeUI has valid character references in Inspector

### Issue: Dummy not responding to behavior changes
**Cause:** DummyAI component missing or disabled
**Fix:** Ensure DummyAI is on opponent character and enabled

### Issue: Damage numbers not appearing
**Cause:** DamageNumbersUI not enabled or prefab missing
**Fix:** Toggle "Show Damage Numbers" ON and verify prefab is assigned

### Issue: Settings panel won't open
**Cause:** Panel reference not assigned or wrong toggle key
**Fix:** Check `settingsPanelRoot` is assigned in TrainingModeUI

### Issue: Reset button doesn't work
**Cause:** Starting positions not stored or characters not assigned
**Fix:** Verify characters are assigned before entering Play Mode

---

## Technical Notes

### Performance
- Unlimited resource toggles run in Update() - minimal overhead
- Damage numbers use object pooling (no GC pressure)
- DummyAI reactions are frame-based (deterministic)

### Limitations
- No round timer in training mode (or very long timer)
- No match progression (continuous practice)
- AI behaviors are predictable (by design)
- Not saved between sessions (settings reset on scene reload)

### Extending Training Mode

Want to add more features? Follow these patterns:

**New Toggle:**
1. Add UI Toggle to Settings Panel
2. Subscribe to `onValueChanged` in TrainingModeUI
3. Apply logic in Update() or via reflection

**New AI Behavior:**
1. Add enum value to `DummyAI.DummyBehavior`
2. Add case in `Update()` switch statement
3. Implement behavior method
4. Add to dropdown options

**New Practice Tool:**
1. Create new UI component (e.g., HitboxVisualizer)
2. Add toggle in TrainingModeUI
3. Enable/disable via SetEnabled() pattern

---

## Future Enhancements (Not Implemented)

Potential training mode improvements:

- **Combo Trials:** Pre-built challenges with specific combo requirements
- **Frame Data Display:** Show startup/active/recovery frames
- **Input Recording:** Record and playback dummy actions
- **Hit Confirmation Practice:** Random block timing to practice confirms
- **Situational Training:** Start in specific game states (low health, exhausted, etc.)
- **Save Settings:** Persist training mode preferences
- **Tutorial Integration:** Guided lessons for each mechanic

These can be added following the established patterns in TrainingModeUI and DummyAI.

---

## Related Documentation

- [UI Systems](../Assets/Knockout/Scripts/UI/UI_SYSTEMS.md) - UI component details
- [Phase 1: Stamina](../docs/plans/Phase-1.md) - Stamina mechanics
- [Phase 2: Defense](../docs/plans/Phase-2.md) - Dodge and parry
- [Phase 3: Combos](../docs/plans/Phase-3.md) - Combo system
- [Phase 4: Special Moves](../docs/plans/Phase-4.md) - Special moves and scoring

---

**Training Mode is your sandbox for mastering the Core Fighting Mechanics. Use it often!**
