# Unity Editor Setup Instructions

## Overview

This guide consolidates all Unity Editor setup instructions for the Core Fighting Mechanics Phase 5 implementation. All code components are complete and ready for scene integration.

**What's Ready:**
- All UI components implemented and tested
- All training mode components implemented
- Comprehensive documentation and tests

**What Needs Unity Editor:**
- Scene integration of UI elements
- Training mode scene creation
- Prefab creation for damage numbers
- Final testing and balancing

---

## Table of Contents

1. [UI Components Integration](#ui-components-integration)
2. [Training Mode Scene Setup](#training-mode-scene-setup)
3. [Prefab Creation](#prefab-creation)
4. [Testing in Unity Editor](#testing-in-unity-editor)
5. [Troubleshooting](#troubleshooting)

---

## UI Components Integration

### Prerequisites

- Unity 2021.3.8f1 LTS
- Main game scene accessible
- TextMeshPro imported

### Step 1: Create UI Canvas Hierarchy

Open your main game scene and create/organize the Canvas structure:

```
Canvas (Screen Space - Overlay)
├── HUD Panel
│   ├── Player Health Bar (existing)
│   ├── Player Stamina Bar (NEW)
│   ├── Opponent Health Bar (existing)
│   ├── Opponent Stamina Bar (NEW)
│   ├── Round Timer (NEW)
│   ├── Player Score Text (NEW - optional)
│   ├── Opponent Score Text (NEW - optional)
│   ├── Special Move Cooldown (NEW)
│   └── Dodge Cooldown (NEW - optional)
│
├── Combat Feedback Panel
│   └── Combo Counter (NEW)
│
└── Overlay Panel
    └── Scoring Breakdown (NEW)
```

### Step 2: Setup StaminaBarUI

**For EACH character (Player and Opponent):**

1. **Create UI Hierarchy:**
   - Create Empty GameObject: `StaminaBarRoot`
   - Parent to HUD Panel
   - Position below health bar

2. **Create Fill Bar:**
   - Create UI > Image: `StaminaBarFill`
   - Parent to StaminaBarRoot
   - Set Image Type: `Filled`
   - Fill Method: `Horizontal`
   - Fill Origin: `Left`
   - Set color to green: `RGB(0, 204, 0)`

3. **Create Background (Optional):**
   - Create UI > Image: `StaminaBarBackground`
   - Parent to StaminaBarRoot
   - Position behind fill bar
   - Set color to dark: `RGB(50, 50, 50)`

4. **Add Component:**
   - Select StaminaBarRoot
   - Add Component > `StaminaBarUI`
   - Assign references:
     - **Stamina Bar Fill:** StaminaBarFill Image
     - **Stamina Bar Background:** StaminaBarBackground Image
     - **Stamina Bar Root:** StaminaBarRoot GameObject
     - **Character Stamina:** Player/Opponent's CharacterStamina component
   - Configure settings:
     - **Hide When Full:** ✓ (checked)
     - **Pulse When Low:** ✓ (checked)
     - Adjust colors if desired

5. **Layout:**
   - Anchor to appropriate corner (top-left for player, top-right for opponent)
   - Size: Width ~200px, Height ~20px
   - Position just below health bar

### Step 3: Setup ComboCounterUI

1. **Create UI Hierarchy:**
   - Create Empty GameObject: `ComboCounterRoot`
   - Parent to Combat Feedback Panel
   - Position center-screen or upper area

2. **Create Text Elements:**
   - Create UI > Text - TextMeshPro: `ComboCountText`
     - Parent to ComboCounterRoot
     - Font Size: 72
     - Alignment: Center
     - Color: Gold `RGB(255, 217, 0)`

   - Create UI > Text - TextMeshPro: `ComboLabelText`
     - Parent to ComboCounterRoot
     - Font Size: 36
     - Alignment: Center
     - Text: "HIT COMBO!"
     - Position below count text

   - Create UI > Text - TextMeshPro: `SequenceNameText`
     - Parent to ComboCounterRoot
     - Font Size: 48
     - Alignment: Center
     - Color: Orange `RGB(255, 128, 0)`
     - Initially hidden (disabled)

3. **Add Component:**
   - Select ComboCounterRoot
   - Add Component > `ComboCounterUI`
   - Assign references:
     - **Combo Count Text:** ComboCountText
     - **Combo Label Text:** ComboLabelText
     - **Sequence Name Text:** SequenceNameText
     - **Combo Counter Root:** ComboCounterRoot
     - **Combo Tracker:** Player's CharacterComboTracker
   - Configure:
     - **Minimum Hits To Show:** 2
     - **Fade Out Duration:** 0.8s

4. **Layout:**
   - Anchor to center-top
   - Position ~200px from top

### Step 4: Setup SpecialMoveCooldownUI

**For EACH character:**

1. **Create UI Hierarchy:**
   - Create UI > Image: `SpecialMoveIcon`
   - Parent to HUD Panel
   - Set sprite to special move icon (or use placeholder)

2. **Create Cooldown Overlay:**
   - Create UI > Image: `CooldownOverlay`
   - Parent to SpecialMoveIcon
   - Set Image Type: `Filled`
   - Fill Method: `Radial 360`
   - Fill Origin: `Top`
   - Clockwise: Unchecked
   - Color: Black with alpha `RGBA(0, 0, 0, 180)`
   - Stretch to fill parent (anchors: min 0,0 max 1,1)

3. **Create Ready Indicator:**
   - Create UI > Image: `ReadyGlow`
   - Parent to SpecialMoveIcon
   - Set sprite to glow/star (or use solid color)
   - Color: Bright yellow
   - Slightly larger than icon
   - Initially hidden (disabled)

4. **Optional Timer Text:**
   - Create UI > Text - TextMeshPro: `CooldownTimer`
   - Parent to SpecialMoveIcon
   - Font Size: 24
   - Alignment: Center
   - Color: White

5. **Add Component:**
   - Select SpecialMoveIcon
   - Add Component > `SpecialMoveCooldownUI`
   - Assign references:
     - **Icon Image:** SpecialMoveIcon Image
     - **Cooldown Fill Overlay:** CooldownOverlay Image
     - **Ready Indicator:** ReadyGlow GameObject
     - **Cooldown Timer Text:** CooldownTimer (if using)
     - **Special Moves:** Character's CharacterSpecialMoves
     - **Character Stamina:** Character's CharacterStamina
   - Configure:
     - **Use Radial Fill:** ✓
     - **Show Countdown Timer:** ✓ (optional)
     - **Pulse When Ready:** ✓

6. **Layout:**
   - Position in corner of HUD (bottom-left for player, bottom-right for opponent)
   - Size: ~80x80px

### Step 5: Setup ScoringDisplayUI

1. **Create Real-Time Score Panel (Optional):**
   - Create Empty GameObject: `RealTimeScoreRoot`
   - Parent to HUD Panel

   - Create UI > Text - TextMeshPro: `PlayerScoreText`
     - Parent to RealTimeScoreRoot
     - Text: "Score: 0"
     - Color: Blue `RGB(77, 153, 255)`
     - Position top-left

   - Create UI > Text - TextMeshPro: `OpponentScoreText`
     - Parent to RealTimeScoreRoot
     - Text: "Score: 0"
     - Color: Red `RGB(255, 77, 77)`
     - Position top-right

2. **Create Breakdown Panel:**
   - Create UI > Panel: `ScoringBreakdownPanel`
   - Parent to Overlay Panel
   - Center screen
   - Size: ~600x400px
   - Add CanvasGroup component (for fading)
   - Initially hidden (disabled)

3. **Add Breakdown UI Elements:**
   Inside ScoringBreakdownPanel:

   - **Title:**
     - Create UI > Text - TextMeshPro: `BreakdownTitle`
     - Text: "JUDGE DECISION"
     - Font Size: 48
     - Alignment: Center
     - Position: Top center

   - **Player Column:**
     - Create UI > Text - TextMeshPro: `PlayerStatsText`
     - Alignment: Left
     - Font Size: 28
     - Position: Left side

   - **Opponent Column:**
     - Create UI > Text - TextMeshPro: `OpponentStatsText`
     - Alignment: Right
     - Font Size: 28
     - Position: Right side

   - **Total Scores:**
     - Create UI > Text - TextMeshPro: `PlayerTotalScore`
     - Position below player stats
     - Create UI > Text - TextMeshPro: `OpponentTotalScore`
     - Position below opponent stats

   - **Winner Announcement:**
     - Create UI > Text - TextMeshPro: `WinnerAnnouncement`
     - Text: "PLAYER WINS!"
     - Font Size: 56
     - Color: Yellow
     - Position: Bottom center

4. **Add Component:**
   - Select ScoringBreakdownPanel or create controller object
   - Add Component > `ScoringDisplayUI`
   - Assign ALL text references
   - Assign references:
     - **Real Time Score Root:** RealTimeScoreRoot (if using)
     - **Breakdown Panel Root:** ScoringBreakdownPanel
     - **Player Scoring:** Player's CharacterScoring
     - **Opponent Scoring:** Opponent's CharacterScoring
     - **Round Manager:** Scene's RoundManager
   - Configure:
     - **Show Real Time Scores:** ✓ (optional)
     - **Breakdown Display Duration:** 5 seconds

### Step 6: Setup RoundTimerUI

1. **Create Timer UI:**
   - Create UI > Text - TextMeshPro: `RoundTimer`
   - Parent to HUD Panel
   - Text: "3:00"
   - Font Size: 56
   - Alignment: Center
   - Color: White
   - Position: Top-center

2. **Optional Background:**
   - Create UI > Image: `TimerBackground`
   - Parent to RoundTimer
   - Behind text
   - For pulse effect

3. **Add Component:**
   - Select RoundTimer
   - Add Component > `RoundTimerUI`
   - Assign references:
     - **Timer Text:** RoundTimer TextMeshPro
     - **Timer Background:** TimerBackground Image (optional)
     - **Round Manager:** Scene's RoundManager
   - Configure:
     - **Warning Threshold:** 30 seconds
     - **Critical Threshold:** 10 seconds
     - **Pulse When Low:** ✓
     - **Normal Color:** White
     - **Warning Color:** Yellow
     - **Critical Color:** Red

### Step 7: Setup DodgeCooldownUI (Optional)

1. **Create Indicator:**
   - Create UI > Image: `DodgeIcon`
   - Parent to HUD Panel
   - Small size (~40x40px)
   - Position in corner
   - Set sprite to dodge icon (or use placeholder)

2. **Create Overlay:**
   - Create UI > Image: `DodgeCooldownOverlay`
   - Parent to DodgeIcon
   - Radial fill (same as special move)

3. **Add Component:**
   - Select DodgeIcon
   - Add Component > `DodgeCooldownUI`
   - Assign references:
     - **Indicator Image:** DodgeIcon
     - **Cooldown Fill Overlay:** DodgeCooldownOverlay
     - **Indicator Root:** DodgeIcon GameObject
     - **Character Dodge:** Character's CharacterDodge
   - Configure:
     - **Only Show When On Cooldown:** ✓
     - **Use Radial Fill:** ✓

**Note:** Given the very short cooldown (~0.2s), this indicator is optional and can be omitted if feels cluttered.

---

## Training Mode Scene Setup

### Step 1: Create Training Mode Scene

1. **Duplicate Main Scene:**
   - In Project window, find your main game scene
   - Right-click > Duplicate
   - Rename to `TrainingMode`
   - Move to `Assets/Knockout/Scenes/` (create folder if needed)

2. **Open Training Mode Scene:**
   - Double-click to open

### Step 2: Modify Scene for Training

1. **Round Manager (Optional Modification):**
   - Find RoundManager in scene
   - Option A: Disable it entirely
   - Option B: Set very long round timer (e.g., 600 seconds)
   - Option C: Disable timer: `Enable Round Timer` = unchecked

2. **AI Character Setup:**
   - Select opponent/AI character
   - **Disable** `CharacterAI` component (standard AI)
   - **Add Component** > `DummyAI`
   - Assign references in DummyAI:
     - Character Combat: Auto-assigned (GetComponent)
     - Character Input: Auto-assigned
     - Combat State Machine: Auto-assigned
     - Character Dodge: Auto-assigned
   - Set `Current Behavior`: Passive (default)

3. **Simplify Environment (Optional):**
   - Remove round transitions if desired
   - Simplify lighting for better visibility
   - Add grid floor for positioning reference

### Step 3: Create Training UI Panel

1. **Create Settings Panel:**
   - In Canvas, create UI > Panel: `TrainingModePanel`
   - Position off-screen or in corner
   - Size: ~400x350px
   - Add background (semi-transparent)
   - Initially hidden (disabled)

2. **Add Title:**
   - Create UI > Text - TextMeshPro: `Title`
   - Text: "Training Mode Settings"
   - Font Size: 32
   - Alignment: Center
   - Position at top of panel

3. **Add Unlimited Stamina Toggle:**
   - Create UI > Toggle: `UnlimitedStaminaToggle`
   - Label text: "Unlimited Stamina"
   - Position below title

4. **Add Unlimited Health Toggle:**
   - Create UI > Toggle: `UnlimitedHealthToggle`
   - Label text: "Unlimited Health"
   - Position below stamina toggle

5. **Add Damage Numbers Toggle:**
   - Create UI > Toggle: `ShowDamageNumbersToggle`
   - Label text: "Show Damage Numbers"
   - Position below health toggle

6. **Add Dummy Behavior Dropdown:**
   - Create UI > Dropdown - TextMeshPro: `DummyBehaviorDropdown`
   - Label text: "Dummy Behavior:"
   - Clear default options (will be populated by code)
   - Position below damage toggle

7. **Add Reset Button:**
   - Create UI > Button - TextMeshPro: `ResetButton`
   - Text: "Reset Positions"
   - Position at bottom of panel
   - Set colors (normal/hover/pressed)

### Step 4: Setup TrainingModeUI Component

1. **Create Controller Object (Optional):**
   - Create Empty GameObject: `TrainingModeController`
   - Parent to Canvas or scene root

2. **Add Component:**
   - Select TrainingModeController (or Canvas)
   - Add Component > `TrainingModeUI`

3. **Assign Character References:**
   - **Player Character:** Drag player GameObject
   - **Dummy Character:** Drag opponent GameObject

4. **Assign UI References:**
   - **Unlimited Stamina Toggle:** Drag toggle
   - **Unlimited Health Toggle:** Drag toggle
   - **Show Damage Numbers Toggle:** Drag toggle
   - **Dummy Behavior Dropdown:** Drag dropdown
   - **Reset Button:** Drag button
   - **Settings Panel Root:** Drag TrainingModePanel

5. **Configure:**
   - **Toggle Panel Key:** Tab (default)
   - **Damage Numbers UI:** (assign after creating DamageNumbersUI)

### Step 5: Setup DamageNumbersUI Component

1. **Create Damage Number Prefab** (See [Prefab Creation](#prefab-creation) section below)

2. **Create DamageNumbers Controller:**
   - Create Empty GameObject: `DamageNumbersController`
   - Parent to scene root (not Canvas)
   - Add Component > `DamageNumbersUI`

3. **Assign References:**
   - **Damage Number Prefab:** Drag prefab from Project window
   - **Player Combat:** Player's CharacterCombat
   - **Opponent Combat:** Opponent's CharacterCombat

4. **Configure:**
   - **Spawn Height Offset:** 1.0
   - **Random Spread:** 0.3
   - **Float Speed:** 2.0
   - **Animation Duration:** 1.5
   - **Pool Size:** 20
   - Adjust colors as desired

5. **Link to Training UI:**
   - Go back to TrainingModeUI component
   - Assign **Damage Numbers UI:** DamageNumbersController

### Step 6: Test Training Mode

1. **Enter Play Mode:**
   - Press Play in Unity Editor

2. **Test Settings Panel:**
   - Press Tab - panel should appear
   - Press Tab again - panel should hide

3. **Test Toggles:**
   - Enable "Unlimited Stamina"
   - Perform attacks - stamina should stay full
   - Enable "Unlimited Health"
   - Take damage - health should restore
   - Enable "Show Damage Numbers"
   - Hit dummy - numbers should appear

4. **Test Dummy Behaviors:**
   - Select "Blocking" - dummy should block
   - Select "Dodging" - dummy should dodge
   - Select "Counter" - dummy should counter-attack
   - Select "Passive" - dummy should stand idle

5. **Test Reset Button:**
   - Move characters around
   - Deplete health/stamina
   - Click Reset
   - Characters should return to start positions at full health/stamina

---

## Prefab Creation

### Damage Number Prefab

**Required for:** DamageNumbersUI component

1. **Create Prefab Structure:**
   - Create Empty GameObject in scene: `DamageNumber`
   - Add Component > TextMeshProUGUI

2. **Configure Text:**
   - Font: Choose bold font
   - Font Size: 36
   - Alignment: Center
   - Color: White (will be overridden by code)
   - Wrapping: Disabled
   - Auto Size: Disabled

3. **Add RectTransform:**
   - Width: 100
   - Height: 50
   - Anchor: Center

4. **Add Outline (Optional):**
   - Add Component > TextMeshPro > Material Preset: Outline
   - Or add Outline component for better visibility

5. **Create Prefab:**
   - Drag `DamageNumber` from Hierarchy to Project window
   - Save in: `Assets/Knockout/Prefabs/UI/`
   - Delete from scene after creating prefab

6. **Test Prefab:**
   - Drag prefab back into scene
   - Verify text is readable
   - Delete test instance

### Special Move Icon (Optional)

**For:** SpecialMoveCooldownUI

If you want custom icons instead of placeholders:

1. Import icon sprites (PNG, 128x128 recommended)
2. Set Texture Type: Sprite (2D and UI)
3. Create sprite slice if needed
4. Assign to SpecialMoveIcon Image component

---

## Testing in Unity Editor

### Pre-Integration Testing

Before integrating into main game:

1. **Test Individual Components:**
   - Create test scene with one character
   - Add one UI component at a time
   - Verify events fire and UI updates
   - Check for console errors

2. **Performance Testing:**
   - Open Profiler window (Window > Analysis > Profiler)
   - Enter Play Mode with all UI active
   - Watch CPU usage:
     - UI.Update should be <0.1ms
     - Rendering.UI should be <0.5ms
     - No GC allocations from UI
   - If performance issues, disable optional UI elements

### Post-Integration Testing

After all UI integrated:

1. **Visual Testing:**
   - Enter Play Mode
   - Trigger all mechanics (stamina, combos, special moves, dodge, scoring)
   - Verify each UI element appears correctly
   - Check for overlapping elements
   - Test at different resolutions (16:9, 16:10, 21:9)

2. **Event Testing:**
   - Verify UI updates when:
     - Stamina depletes/regenerates
     - Combos start/end
     - Special move used/ready
     - Dodge performed
     - Judge decision made
   - Check for delayed updates or missing events

3. **Edge Case Testing:**
   - Simultaneous events (combo + special + stamina depletion)
   - Rapid inputs
   - Scene transitions
   - Pause/resume

### Training Mode Testing

1. **Toggle Testing:**
   - Enable each toggle individually
   - Verify functionality
   - Test combinations (all on, all off, mixed)

2. **Dummy AI Testing:**
   - Test each behavior mode
   - Verify consistent reactions
   - Test behavior switching mid-match

3. **Reset Testing:**
   - Move characters to different positions
   - Deplete resources
   - Hit Reset
   - Verify full reset (position, health, stamina, state)

### Performance Benchmarks

**Target Performance:**
- Total UI: <0.1ms per frame
- No GC allocations from UI updates
- 60fps maintained with all UI active

**If performance issues:**
1. Disable optional elements (real-time scores, dodge cooldown, momentum bar)
2. Check for coroutine leaks
3. Verify object pooling working (damage numbers)
4. Profile each UI component individually

---

## Troubleshooting

### Common Issues

#### Issue: UI Not Appearing

**Symptoms:** UI elements don't show up in game
**Causes:**
- GameObject disabled
- Canvas not rendering
- References not assigned

**Fixes:**
1. Check GameObject is active (checkbox in Inspector)
2. Verify Canvas is Screen Space - Overlay
3. Check all references assigned in Inspector
4. Ensure character components initialized

#### Issue: UI Not Updating

**Symptoms:** UI shows but doesn't change
**Causes:**
- Events not subscribed
- Component not initialized
- References incorrect

**Fixes:**
1. Check Console for errors
2. Verify character component references correct
3. Ensure Start() called (check script execution order)
4. Debug: Add Debug.Log in event handlers to verify firing

#### Issue: UI Overlapping

**Symptoms:** UI elements cover each other
**Causes:**
- Z-order incorrect
- Anchors/positions wrong
- Layout not configured

**Fixes:**
1. Adjust sorting order in Canvas
2. Use Layout Groups for automatic spacing
3. Verify anchors set correctly
4. Check RectTransform positions

#### Issue: Performance Drops

**Symptoms:** Frame rate drops when UI active
**Causes:**
- Too many UI updates
- GC allocations
- Inefficient animations

**Fixes:**
1. Open Profiler and identify bottleneck
2. Disable optional UI elements
3. Check for string concatenation in Update()
4. Verify object pooling working

#### Issue: Training Mode Toggles Not Working

**Symptoms:** Toggles don't affect gameplay
**Causes:**
- Character references not assigned
- Components missing
- Events not connected

**Fixes:**
1. Verify Player Character and Dummy Character assigned in TrainingModeUI
2. Check characters have CharacterStamina and CharacterHealth
3. Verify toggle onValueChanged events connected
4. Check Console for missing reference warnings

#### Issue: Damage Numbers Not Spawning

**Symptoms:** No floating numbers on hits
**Causes:**
- Prefab not assigned
- DamageNumbersUI not enabled
- Combat events not firing

**Fixes:**
1. Verify Damage Number Prefab assigned
2. Check "Show Damage Numbers" toggle is ON
3. Verify combat component references correct
4. Check pool not exhausted (increase Pool Size)

#### Issue: Dummy AI Not Responding

**Symptoms:** Dummy doesn't react to behavior changes
**Causes:**
- DummyAI component missing
- Standard CharacterAI still enabled
- References not assigned

**Fixes:**
1. Verify DummyAI component on opponent character
2. Disable CharacterAI component
3. Check DummyAI component references
4. Verify behavior dropdown connected to DummyAI

### Debugging Tips

1. **Console Warnings:**
   - Check Console for missing reference warnings
   - All components log warnings if references missing

2. **Event Testing:**
   - Add temporary Debug.Log in event handlers
   - Verify events firing when expected

3. **Reference Checking:**
   - Select UI component in Inspector
   - Verify all fields have assigned references (not "None")

4. **Component Execution:**
   - Ensure scripts compiled (no compile errors)
   - Check script execution order (Edit > Project Settings > Script Execution Order)

5. **Scene Setup:**
   - Compare your scene to hierarchy examples above
   - Verify Canvas settings (Render Mode, Sort Order)

---

## Quick Reference Checklists

### UI Integration Checklist

- [ ] Canvas created with proper hierarchy
- [ ] StaminaBarUI added for each character
- [ ] ComboCounterUI added and positioned
- [ ] SpecialMoveCooldownUI added for each character
- [ ] ScoringDisplayUI added with both panels
- [ ] RoundTimerUI added and positioned
- [ ] DodgeCooldownUI added (optional)
- [ ] All component references assigned
- [ ] All UI tested in Play Mode
- [ ] No console errors
- [ ] Performance acceptable (<0.1ms for UI)

### Training Mode Checklist

- [ ] TrainingMode scene created
- [ ] DummyAI component added to opponent
- [ ] Standard CharacterAI disabled
- [ ] Training settings panel created
- [ ] All toggles and controls added
- [ ] TrainingModeUI component added
- [ ] All references assigned
- [ ] Damage number prefab created
- [ ] DamageNumbersUI component added
- [ ] All toggles tested and working
- [ ] All dummy behaviors tested
- [ ] Reset button working
- [ ] No console errors

### Final Verification

- [ ] Main game scene: All UI functional
- [ ] Training mode scene: All features working
- [ ] Performance: 60fps maintained
- [ ] No console errors or warnings
- [ ] All references assigned (no "None" in Inspector)
- [ ] UI scales properly at different resolutions
- [ ] All documentation read and understood

---

## Additional Resources

### Documentation

- **UI_SYSTEMS.md** - Detailed UI component documentation
- **TRAINING_MODE.md** - Training mode user guide
- **Phase-5-Completion-Summary.md** - Implementation overview

### Unity Documentation

- [Unity UI](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/index.html)
- [TextMeshPro](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html)
- [Canvas](https://docs.unity3d.com/Manual/UICanvas.html)
- [Event System](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/EventSystem.html)

### Support

If you encounter issues not covered here:

1. Check Console for errors
2. Review component documentation in `UI_SYSTEMS.md`
3. Verify all prerequisites met
4. Compare your setup to examples in this guide

---

## Estimated Time

**First-time Setup:**
- UI Integration: 2-3 hours
- Training Mode Scene: 1-2 hours
- Prefab Creation: 30 minutes
- Testing and Debugging: 1-2 hours
- **Total: 5-8 hours**

**Subsequent Iterations:**
- UI tweaks: 15-30 minutes
- Training mode adjustments: 15-30 minutes

**Tips for Efficiency:**
- Set up one character's UI completely before duplicating for second character
- Use prefabs for repeated elements
- Test incrementally (don't add all UI at once)
- Keep Profiler open to catch performance issues early

---

## Summary

All code components are complete and ready for Unity Editor integration. Follow these instructions to:

1. ✅ Integrate UI components into main game scene
2. ✅ Create and configure training mode scene
3. ✅ Create necessary prefabs
4. ✅ Test and verify all functionality

After completing these steps, the Core Fighting Mechanics will be fully integrated and ready for playtesting and balancing!

**Good luck with the integration! 🎮**
