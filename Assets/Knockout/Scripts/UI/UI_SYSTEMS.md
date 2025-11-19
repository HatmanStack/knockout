# UI Systems Documentation

## Overview

This document describes the UI systems implemented for the Core Fighting Mechanics. All UI components are **contextual** (appear when relevant), **event-driven** (subscribe to game events), and **performance-optimized** (minimal GC allocations).

## UI Components

### 1. StaminaBarUI

**Purpose:** Displays character stamina as a contextual bar.

**Key Features:**
- Hidden when at full stamina (contextual display)
- Color-coded: Green (>50%), Yellow (25-50%), Red (<25%)
- Smooth lerp animations
- Depletion flash effect when stamina reaches zero
- Pulse animation when stamina low

**Setup:**
1. Create UI hierarchy: Root GameObject → Fill Image + Background Image
2. Add `StaminaBarUI` component to root
3. Assign references:
   - `staminaBarFill`: The fill Image (use Filled type)
   - `staminaBarBackground`: Background Image (optional, for flash effect)
   - `staminaBarRoot`: Root GameObject (for show/hide)
   - `characterStamina`: Character's CharacterStamina component
4. Configure colors and animation settings in Inspector

**Events Subscribed:**
- `CharacterStamina.OnStaminaChanged`
- `CharacterStamina.OnStaminaDepleted`

### 2. ComboCounterUI

**Purpose:** Displays combo hit count during active combos with animations.

**Key Features:**
- Appears when combo >= 2 hits (configurable)
- Scale punch animation on each hit
- Font size increases with combo count
- Sequence name display on completion with special animation
- Fades out when combo ends

**Setup:**
1. Create UI hierarchy: Root GameObject → Count Text + Label Text + Sequence Text
2. Add `ComboCounterUI` component
3. Assign TextMeshPro references
4. Configure minimum hits to show and animation settings

**Events Subscribed:**
- `CharacterComboTracker.OnComboHitLanded`
- `CharacterComboTracker.OnComboSequenceCompleted`
- `CharacterComboTracker.OnComboEnded`
- `CharacterComboTracker.OnComboBroken`

### 3. SpecialMoveCooldownUI

**Purpose:** Displays special move cooldown with readiness indication.

**Key Features:**
- Radial or linear fill overlay showing cooldown progress
- Icon color changes: White (ready), Gray (cooldown), Darker (insufficient stamina)
- Dual-gating: Shows both cooldown AND stamina requirements
- Pulse animation when fully ready
- Optional countdown timer text

**Setup:**
1. Create UI hierarchy: Icon Image + Cooldown Overlay Image + Ready Indicator
2. Add `SpecialMoveCooldownUI` component
3. Assign Image references
4. Set fill type (radial recommended)
5. Enable pulse and timer as desired

**Events Subscribed:**
- `CharacterSpecialMoves.OnSpecialMoveUsed`
- `CharacterSpecialMoves.OnSpecialMoveReady`
- `CharacterStamina.OnStaminaChanged` (for dual-gating)

### 4. ScoringDisplayUI

**Purpose:** Displays real-time scores and judge decision breakdown.

**Key Features:**
- Real-time scores during round (optional, can be hidden)
- Detailed breakdown panel on judge decision
- Side-by-side stats comparison
- Winner highlighting
- Fade in/out animations
- Auto-hides after configurable duration

**Setup:**
1. Create two panels: Real-time scores (HUD) and Breakdown panel (overlay)
2. Add `ScoringDisplayUI` component
3. Assign all TextMeshPro references for both panels
4. Configure display duration and colors
5. Enable/disable real-time scores as desired

**Events Subscribed:**
- `CharacterScoring.OnScoreChanged` (both characters)
- `RoundManager.OnJudgeDecision`

### 5. RoundTimerUI

**Purpose:** Displays round timer with color-coded warnings.

**Key Features:**
- MM:SS format
- Color changes: White (>30s), Yellow (10-30s), Red (<10s)
- Pulse animation when time low
- Clear visual urgency feedback

**Setup:**
1. Create TextMeshPro component for timer
2. Add optional background Image for pulse effect
3. Add `RoundTimerUI` component
4. Assign text reference
5. Configure warning thresholds and colors

**Events Subscribed:**
- `RoundManager.OnRoundTimeChanged`

### 6. DodgeCooldownUI

**Purpose:** Subtle cooldown indicator for dodge (0.2s cooldown).

**Key Features:**
- Contextual appearance (only when on cooldown)
- Radial fill shows progress
- Very subtle and non-intrusive
- Can be omitted if feels cluttered

**Setup:**
1. Create small icon with overlay
2. Add `DodgeCooldownUI` component
3. Enable "only show when on cooldown"
4. Keep small and in corner of HUD

**Events Subscribed:**
- `CharacterDodge.OnDodgeReady`

### 7. MomentumBarUI (Optional, Not Implemented)

**Purpose:** Visual momentum advantage indicator.

**Note:** Momentum is already tracked implicitly via scoring differential. This UI is purely optional enhancement. Marked as optional in Phase 5.

---

## UI Hierarchy Example

```
Canvas (Screen Space - Overlay)
├── HUD Panel (Always visible)
│   ├── Player Health Bar (existing)
│   ├── Player Stamina Bar (new)
│   ├── Opponent Health Bar (existing)
│   ├── Opponent Stamina Bar (new)
│   ├── Round Timer (new)
│   ├── Player Real-Time Score (new, optional)
│   ├── Opponent Real-Time Score (new, optional)
│   ├── Special Move Cooldown (new)
│   └── Dodge Cooldown (new, optional)
│
├── Combat Feedback Panel (Contextual)
│   └── Combo Counter (new)
│
└── Overlay Panel (Round end)
    └── Scoring Breakdown (new)
```

---

## Contextual Display Philosophy

All new UI elements follow the **contextual display** principle:

- **StaminaBarUI:** Hidden when at 100% stamina
- **ComboCounterUI:** Only appears during active combos (2+ hits)
- **SpecialMoveCooldownUI:** Dims when unavailable, pulses when ready
- **ScoringDisplayUI Breakdown:** Only on judge decision
- **DodgeCooldownUI:** Only when on cooldown

This keeps the screen clean during normal gameplay and provides information exactly when needed.

---

## Color Coding Standards

### Stamina Bars
- Green: >50%
- Yellow: 25-50%
- Red: <25%

### Timer
- White: >30s
- Yellow: 10-30s
- Red: <10s

### Special Move Cooldown
- White: Ready
- Gray: On cooldown
- Dark Gray: Insufficient stamina

### Scoring
- Blue: Player
- Red: Opponent
- Yellow: Winner highlight

---

## Performance Considerations

All UI components are optimized for 60fps gameplay:

1. **Minimal GC allocations:**
   - No string concatenation in Update loops
   - Object pooling for damage numbers
   - Cached component references

2. **Efficient updates:**
   - Event-driven (not polling)
   - Lerp animations instead of coroutines where possible
   - Canvas groups for batch fading

3. **Target performance:**
   - All UI combined: <0.1ms per frame total
   - No GC allocations from UI updates
   - Profiler verified under load

---

## Customization

### Changing Colors

All UI components expose color settings in Inspector. Modify these without code changes:
- `highStaminaColor`, `mediumStaminaColor`, `lowStaminaColor` (StaminaBarUI)
- `normalColor`, `warningColor`, `criticalColor` (RoundTimerUI)
- `comboCountColor`, `sequenceNameColor` (ComboCounterUI)
- etc.

### Changing Animation Speeds

Animation speeds and durations are exposed as Inspector fields:
- `updateSpeed`, `pulseSpeed` (StaminaBarUI)
- `scalePunchSpeed`, `fadeOutDuration` (ComboCounterUI)
- `pulseSpeed`, `pulseScale` (SpecialMoveCooldownUI)

### Hiding/Showing Elements

Use `showRealTimeScores` (ScoringDisplayUI), `hideWhenFull` (StaminaBarUI), `onlyShowWhenOnCooldown` (DodgeCooldownUI) toggles to control visibility behavior.

---

## Troubleshooting

### Issue: UI not updating
**Cause:** Events not subscribed
**Fix:** Ensure character component references are assigned in Inspector and `Start()` has been called

### Issue: UI overlapping
**Cause:** Layout issue
**Fix:** Adjust anchors and positions in Canvas. Use Layout Groups if needed.

### Issue: UI causing frame drops
**Cause:** Too many active UI elements or inefficient updates
**Fix:** Check Profiler. Disable optional elements (real-time scores, dodge cooldown). Verify no GC allocations.

### Issue: Stamina bar not hiding at full
**Cause:** `hideWhenFull` disabled or `staminaBarRoot` not assigned
**Fix:** Enable toggle and assign root GameObject reference

---

## Integration with Existing Systems

All UI components integrate cleanly with existing Phase 1-4 systems:

- **Phase 1 (Stamina):** StaminaBarUI subscribes to CharacterStamina events
- **Phase 2 (Defense):** DodgeCooldownUI uses CharacterDodge
- **Phase 3 (Combos):** ComboCounterUI tracks CharacterComboTracker
- **Phase 4 (Special Moves & Scoring):** SpecialMoveCooldownUI and ScoringDisplayUI integrate with those systems

No modifications to Phase 1-4 code required - all integration is event-based.

---

## Future Enhancements

Potential improvements not in current scope:

- **Damage indicators:** Directional hit indicators
- **Announcer text:** "PERFECT!", "COMBO BREAKER!", etc.
- **Hit confirmation:** Brief screen flash or haptic feedback
- **Advanced stats:** Heat maps, punch accuracy graphs
- **Replay UI:** Timeline scrubbing for replay mode
- **Tutorial overlays:** Contextual hints for new players

These can be added following the same event-driven, contextual patterns established here.
