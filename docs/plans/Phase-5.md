# Phase 5: UI Systems & Training Mode

## Phase Goal

Implement contextual UI elements for all Core Fighting Mechanics systems (stamina, combos, scoring, special moves, defense cooldowns) and create a basic training/practice mode for learning and testing mechanics.

**Success Criteria:**
- Contextual UI that appears when relevant (stamina bars, combo counter, scoring display, cooldown indicators)
- UI integrates with all Phase 1-4 systems via events
- Basic training mode with dummy AI, unlimited resources toggle, and damage numbers
- All UI functional and responsive (60fps maintained)
- UI follows existing project style patterns
- Training mode accessible and useful for testing

**Estimated Tokens:** ~100,000

---

## Prerequisites

**Required Reading:**
- [Phase 0: Foundation](Phase-0.md) - UI design principles (contextual display)
- [Phase 1: Core Resource Systems](Phase-1.md) - Stamina events
- [Phase 2: Advanced Defense](Phase-2.md) - Dodge/parry events
- [Phase 3: Combo System](Phase-3.md) - Combo events
- [Phase 4: Special Moves & Judge Scoring](Phase-4.md) - Special move and scoring events

**Existing Systems to Understand:**
- `HealthBarUI` - Existing UI component pattern (Assets/Knockout/Scripts/UI/HealthBarUI.cs)
- `RoundUI` - Existing round display (Assets/Knockout/Scripts/UI/RoundUI.cs)
- All new components from Phases 1-4 (for event subscriptions)

**Environment:**
- Unity 2021.3.8f1 LTS
- Unity UI (uGUI)

---

## Tasks

### Task 1: Create StaminaBarUI Component

**Goal:** Display character stamina as contextual bar that appears when stamina depleting.

**Files to Create:**
- `Assets/Knockout/Scripts/UI/StaminaBarUI.cs` - UI component class

**Prerequisites:**
- Review `HealthBarUI.cs` for UI component pattern
- Phase 1 complete (CharacterStamina exists)

**Implementation Steps:**

1. Create `StaminaBarUI` component in `Knockout.UI` namespace
2. Implement UI component pattern:
   - Dependencies: `CharacterStamina`, UI Image for fill bar, UI Text for optional percentage display
   - Subscribe to `CharacterStamina.OnStaminaChanged` in `Initialize()`
3. Visual elements:
   - Background bar (full width)
   - Fill bar (colored, scaled based on stamina percentage)
   - Optional: numerical display (e.g., "75/100")
4. Implement contextual display:
   - Hide stamina bar when at full (100%)
   - Show stamina bar when below 100%
   - Optional: pulse or flash when stamina low (<25%) or depleted
5. Implement bar update:
   - On OnStaminaChanged event: update fill bar width/scale
   - Smooth interpolation (lerp) for visual polish
6. Color coding:
   - Green: >50% stamina
   - Yellow: 25-50% stamina
   - Red: <25% stamina (danger zone)
7. Position above or below health bar (non-overlapping)

**Design Guidance:**
- Follow existing HealthBarUI style and positioning
- Contextual: only visible when relevant (not at full stamina)
- Smooth updates (lerp) feel more polished than instant jumps
- Clear visual feedback for stamina state

**Verification Checklist:**
- [ ] Stamina bar displays correctly
- [ ] Bar updates on stamina changes
- [ ] Contextual display (hidden at full)
- [ ] Color coding reflects stamina state
- [ ] Smooth visual updates

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/UI/StaminaBarUITests.cs`

Test cases:
- Bar hidden when stamina at 100%
- Bar appears when stamina depletes
- Bar fill updates correctly on stamina change
- Color changes based on stamina percentage

Manual testing:
- Enter play mode
- Perform attacks to deplete stamina
- Verify bar appears and updates
- Check color changes

**Commit Message Template:**
```
feat(ui): implement StaminaBarUI component

- Display stamina as contextual bar
- Color-coded based on stamina level
- Hidden when at full stamina
- Smooth visual updates
```

**Estimated Tokens:** ~7,000

---

### Task 2: Create ComboCounterUI Component

**Goal:** Display combo counter that appears during combos and shows hit count.

**Files to Create:**
- `Assets/Knockout/Scripts/UI/ComboCounterUI.cs` - UI component class

**Prerequisites:**
- Phase 3 complete (CharacterComboTracker exists)

**Implementation Steps:**

1. Create `ComboCounterUI` component in `Knockout.UI` namespace
2. Implement UI component pattern:
   - Dependencies: `CharacterComboTracker`, UI Text for combo count, UI Text for combo label (optional)
   - Subscribe to combo events: `OnComboHitLanded`, `OnComboEnded`, `OnComboSequenceCompleted`
3. Visual elements:
   - Combo count text (e.g., "3 HIT COMBO!")
   - Optional: sequence name when predefined sequence completed (e.g., "1-2-HOOK!")
4. Implement contextual display:
   - Hide when no combo active
   - Show when combo count >= 2
   - Scale/pulse animation on each hit (visual feedback)
   - Fade out when combo ends
5. Implement combo display:
   - On OnComboHitLanded: update count text, play hit animation
   - On OnComboSequenceCompleted: briefly display sequence name with special effect
   - On OnComboEnded: fade out and hide
6. Positioning: center-screen or upper area (highly visible during combo)
7. Visual polish:
   - Increase font size with combo count (bigger for longer combos)
   - Flash or pulse on sequence completion
   - Sound effect hooks (events for audio)

**Design Guidance:**
- Combos are exciting - UI should reflect that (bold, animated)
- Contextual: only appears during active combo
- Sequence completion is special (distinct visual/audio cue)
- Clear, readable at a glance

**Verification Checklist:**
- [ ] Combo counter appears on combo start
- [ ] Count updates on each hit
- [ ] Sequence name displays on completion
- [ ] Counter fades out on combo end
- [ ] Animations smooth and polished

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/UI/ComboCounterUITests.cs`

Test cases:
- Counter hidden when no combo
- Counter appears when combo >= 2 hits
- Count updates correctly on each hit
- Counter hides when combo ends
- Sequence name displays on completion

Manual testing:
- Execute combos in play mode
- Verify counter appears and updates
- Check animations and visibility

**Commit Message Template:**
```
feat(ui): implement ComboCounterUI component

- Display hit count during active combos
- Show sequence name on completion
- Contextual appearance with animations
- Visual feedback for combo progression
```

**Estimated Tokens:** ~8,000

---

### Task 3: Create SpecialMoveCooldownUI Component

**Goal:** Display special move cooldown indicator showing readiness state.

**Files to Create:**
- `Assets/Knockout/Scripts/UI/SpecialMoveCooldownUI.cs` - UI component class

**Prerequisites:**
- Phase 4 complete (CharacterSpecialMoves exists)

**Implementation Steps:**

1. Create `SpecialMoveCooldownUI` component in `Knockout.UI` namespace
2. Implement UI component pattern:
   - Dependencies: `CharacterSpecialMoves`, UI Image for cooldown fill, UI Image for special move icon
   - Subscribe to events: `OnSpecialMoveUsed`, `OnSpecialMoveReady`
3. Visual elements:
   - Special move icon (fixed image)
   - Cooldown overlay (radial fill or progress bar)
   - Optional: numerical cooldown timer (e.g., "30s")
   - "Ready" indicator (glow, color change, or text)
4. Implement cooldown display:
   - While on cooldown: show fill overlay draining (progress from full to empty)
   - Update every frame: query `CharacterSpecialMoves.CooldownProgress`
   - When ready: remove overlay, show "Ready" indicator
5. Implement contextual behavior:
   - Dim icon when insufficient stamina (even if cooldown ready)
   - Flash or pulse when both cooldown and stamina ready
6. Positioning: HUD corner or near character portrait
7. Tooltip or label: special move name

**Design Guidance:**
- Clear at-a-glance readiness check
- Cooldown visual (radial fill) intuitive and common in games
- Dual-gating feedback: cooldown AND stamina must be ready
- Special moves are important - UI should emphasize readiness

**Verification Checklist:**
- [ ] Cooldown overlay displays correctly
- [ ] Overlay updates smoothly (radial fill or progress bar)
- [ ] "Ready" indicator appears when cooldown complete
- [ ] Icon dims when stamina insufficient
- [ ] Visual feedback clear and intuitive

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/UI/SpecialMoveCooldownUITests.cs`

Test cases:
- Cooldown overlay fills on special move use
- Overlay updates based on CooldownProgress
- "Ready" indicator appears when cooldown complete
- Icon dims when stamina insufficient

Manual testing:
- Use special move in play mode
- Watch cooldown overlay progress
- Verify "Ready" state at completion

**Commit Message Template:**
```
feat(ui): implement SpecialMoveCooldownUI component

- Display special move cooldown progress
- Show readiness indicator when available
- Dim icon when stamina insufficient
- Clear dual-gating feedback
```

**Estimated Tokens:** ~7,000

---

### Task 4: Create ScoringDisplayUI Component

**Goal:** Display current score and scoring breakdown (contextual at round end).

**Files to Create:**
- `Assets/Knockout/Scripts/UI/ScoringDisplayUI.cs` - UI component class

**Prerequisites:**
- Phase 4 complete (CharacterScoring exists)

**Implementation Steps:**

1. Create `ScoringDisplayUI` component in `Knockout.UI` namespace
2. Implement UI component pattern:
   - Dependencies: Both `CharacterScoring` components (player and opponent), `RoundManager`
   - Subscribe to events: `CharacterScoring.OnScoreChanged`, `RoundManager.OnJudgeDecision`
3. Visual elements:
   - Real-time score display (optional, top HUD): "Score: 45" for each character
   - Round-end scoring breakdown (panel):
     - Both characters' scores side-by-side
     - Breakdown of stats (hits, combos, parries, etc.)
     - Winner announcement
     - "JUDGE DECISION" header
4. Implement contextual display:
   - Real-time scores: always visible (or hidden during active combat, player preference)
   - Breakdown panel: only appears on judge decision (round timer expiry)
   - Breakdown shows for 3-5 seconds then fades
5. Implement breakdown content:
   - List key stats for both characters:
     - Clean Hits: X vs Y
     - Combos: X vs Y
     - Parries: X vs Y
     - Special Moves: X vs Y
     - Total Score: X vs Y
   - Highlight winner's score (bold, color)
6. Positioning: center-screen overlay for breakdown, HUD for real-time scores

**Design Guidance:**
- Breakdown provides transparency (why did I win/lose?)
- Real-time scores optional (can be distracting, but informative)
- Clear winner indication
- Breakdown should be readable but not block gameplay too long

**Verification Checklist:**
- [ ] Real-time scores display correctly
- [ ] Breakdown appears on judge decision
- [ ] Breakdown shows accurate stats
- [ ] Winner highlighted clearly
- [ ] Breakdown fades after timeout

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/UI/ScoringDisplayUITests.cs`

Test cases:
- Real-time scores update on score changes
- Breakdown appears on OnJudgeDecision event
- Breakdown displays correct stats
- Winner highlighted
- Breakdown auto-hides after duration

Manual testing:
- Let round timer expire
- Verify judge decision breakdown appears
- Check stats accuracy
- Confirm winner indicated

**Commit Message Template:**
```
feat(ui): implement ScoringDisplayUI component

- Display real-time scores during round
- Show scoring breakdown on judge decision
- Highlight winner and key stats
- Contextual appearance at round end
```

**Estimated Tokens:** ~9,000

---

### Task 5: Create RoundTimerUI Component

**Goal:** Display round timer counting down with visual warnings.

**Files to Create:**
- `Assets/Knockout/Scripts/UI/RoundTimerUI.cs` - UI component class

**Prerequisites:**
- Phase 4 complete (RoundManager with timer)

**Implementation Steps:**

1. Create `RoundTimerUI` component in `Knockout.UI` namespace
2. Implement UI component pattern:
   - Dependencies: `RoundManager`
   - Subscribe to: `RoundManager.OnRoundTimeChanged`
3. Visual elements:
   - Timer text (e.g., "2:45" for 2 minutes 45 seconds)
   - Optional: circular progress bar around timer
   - Warning indicator when time low (<30s)
4. Implement timer display:
   - Format time as MM:SS
   - Update on OnRoundTimeChanged event
   - Flash or pulse when time low
   - Color change: white → yellow → red as time expires
5. Positioning: top-center HUD (common in fighting games)
6. Visual warnings:
   - <30s: yellow text, pulse animation
   - <10s: red text, faster pulse, countdown beeps (audio hook)

**Design Guidance:**
- Timer should be always visible during round
- Clear time remaining at a glance
- Visual/audio warnings for urgency
- Standard fighting game timer format (MM:SS)

**Verification Checklist:**
- [ ] Timer displays correctly in MM:SS format
- [ ] Timer updates on time changes
- [ ] Color changes based on time remaining
- [ ] Warning animations at low time
- [ ] Clear and readable

**Testing Instructions:**

Create PlayMode test: `Assets/Knockout/Tests/PlayMode/UI/RoundTimerUITests.cs`

Test cases:
- Timer formats time correctly (MM:SS)
- Timer updates on OnRoundTimeChanged
- Color changes when time low
- Warning triggers at threshold

Manual testing:
- Start round and watch timer count down
- Verify color changes and warnings
- Check format accuracy

**Commit Message Template:**
```
feat(ui): implement RoundTimerUI component

- Display round timer in MM:SS format
- Color-coded warnings as time expires
- Pulse animation when time low
- Always visible during round
```

**Estimated Tokens:** ~6,000

---

### Task 6: Create DodgeCooldownUI Component

**Goal:** Display dodge cooldown indicator (subtle, contextual).

**Files to Create:**
- `Assets/Knockout/Scripts/UI/DodgeCooldownUI.cs` - UI component class

**Prerequisites:**
- Phase 2 complete (CharacterDodge exists)

**Implementation Steps:**

1. Create `DodgeCooldownUI` component in `Knockout.UI` namespace
2. Implement UI component pattern:
   - Dependencies: `CharacterDodge`
   - Subscribe to: `OnDodgeStarted`, `OnDodgeReady`
3. Visual elements:
   - Small cooldown indicator (circular progress or icon with overlay)
   - Dodge icon
   - Optional: cooldown fill
4. Implement contextual display:
   - Appear when dodge used (on cooldown)
   - Fade in cooldown overlay
   - Hide when dodge ready again
   - Very short cooldown (~0.2s) so indicator may be brief
5. Positioning: HUD near controls or character portrait (subtle)

**Design Guidance:**
- Dodge cooldown very short (0.2s) - indicator should be subtle
- Optional: can be omitted if feels too cluttered
- If implemented, should not distract from combat
- Consider: only show when dodge on cooldown (contextual)

**Verification Checklist:**
- [ ] Indicator appears on dodge use
- [ ] Cooldown progress displays
- [ ] Indicator hides when ready
- [ ] Non-intrusive and subtle

**Testing Instructions:**

Manual testing:
- Dodge repeatedly
- Check if cooldown indicator appears
- Verify timing matches DodgeData cooldown

**Commit Message Template:**
```
feat(ui): implement DodgeCooldownUI component

- Display dodge cooldown indicator
- Contextual appearance (only when on cooldown)
- Subtle and non-intrusive
```

**Estimated Tokens:** ~5,000

---

### Task 7: Create MomentumBarUI Component (Optional)

**Goal:** Display momentum advantage as visual bar (optional enhancement).

**Files to Create:**
- `Assets/Knockout/Scripts/UI/MomentumBarUI.cs` - UI component class (optional)

**Prerequisites:**
- Phase 4 complete (CharacterScoring or MomentumTracker if implemented)

**Implementation Steps:**

1. Create `MomentumBarUI` component in `Knockout.UI` namespace (optional)
2. Implement UI component pattern:
   - Dependencies: Both `CharacterScoring` components (or `MomentumTracker`)
   - Subscribe to score change events
3. Visual elements:
   - Horizontal bar (center = neutral)
   - Fill extends left (opponent advantage) or right (player advantage)
   - Color-coded: blue (player) vs red (opponent)
   - Center marker (neutral point)
4. Implement momentum display:
   - Calculate momentum: `(playerScore - opponentScore) / (playerScore + opponentScore)`
   - Map to bar fill (-1 to 1 → left to right)
   - Update smoothly (lerp)
5. Positioning: top or bottom HUD (horizontal bar)

**Design Guidance:**
- Momentum bar is optional (nice-to-have, not critical)
- Provides at-a-glance "who's winning" feedback
- Should not clutter main action area
- Can be toggled on/off by player preference

**Verification Checklist:**
- [ ] Momentum bar displays correctly
- [ ] Fill direction matches score advantage
- [ ] Color-coded clearly
- [ ] Smooth updates

**Testing Instructions:**

Manual testing:
- Perform actions to build score
- Watch momentum bar shift
- Verify direction and color

**Commit Message Template:**
```
feat(ui): implement MomentumBarUI component (optional)

- Display momentum advantage visually
- Horizontal bar with center neutral point
- Color-coded player vs opponent
- Smooth updates based on scoring
```

**Estimated Tokens:** ~6,000

---

### Task 8: Integrate All UI Components into Game Scene

**Goal:** Add all UI components to game scene and wire to characters.

**Files to Modify:**
- Game scene(s) - Add UI GameObjects and components

**Prerequisites:**
- Tasks 1-7 complete (all UI components exist)

**Implementation Steps:**

1. Open main game scene
2. Create UI Canvas hierarchy:
   - Canvas (if not exists)
   - HUD Panel (always visible elements):
     - Health bars (existing)
     - Stamina bars (new)
     - Round timer (new)
     - Real-time scores (new, optional)
     - Special move cooldown (new)
     - Dodge cooldown (new, optional)
     - Momentum bar (new, optional)
   - Combat Feedback Panel (contextual elements):
     - Combo counter (new)
   - Overlay Panel (round end elements):
     - Scoring breakdown (new)
     - Judge decision announcement (new)
3. Add UI components to corresponding GameObjects
4. Wire dependencies:
   - Find and assign character component references (CharacterStamina, CharacterComboTracker, etc.)
   - Assign UI element references (Image, Text components)
5. Layout and positioning:
   - Follow existing UI style and spacing
   - Ensure no overlaps
   - Test at different resolutions (UI should scale)
6. Test all UI elements in play mode

**Design Guidance:**
- Organize UI hierarchy clearly (HUD, Feedback, Overlays)
- Use Canvas scaler for resolution independence
- Follow existing UI style (colors, fonts, sizing)
- Test UI at 16:9, 16:10, 21:9 aspect ratios

**Verification Checklist:**
- [ ] All UI components added to scene
- [ ] Dependencies wired correctly
- [ ] UI elements positioned and styled
- [ ] No overlaps or layout issues
- [ ] UI scales at different resolutions

**Testing Instructions:**

Manual testing:
- Enter play mode
- Verify all UI elements appear and function
- Test at different resolutions
- Check for visual issues or overlaps

**Commit Message Template:**
```
feat(ui): integrate all UI components into game scene

- Add UI Canvas hierarchy
- Wire all UI components to characters
- Position and style UI elements
- Test at multiple resolutions
```

**Estimated Tokens:** ~6,000

---

### Task 9: Create Training Mode Scene

**Goal:** Create dedicated training/practice scene for learning mechanics.

**Files to Create:**
- `Assets/Knockout/Scenes/TrainingMode.unity` - New scene

**Prerequisites:**
- Understand existing scene structure

**Implementation Steps:**

1. Create new scene "TrainingMode"
2. Copy base game scene setup:
   - Camera, lighting, environment
   - Player character
   - Opponent character (dummy AI)
   - UI Canvas with all HUD elements
3. Add training mode specific elements:
   - Training UI panel (settings, toggles)
   - Damage numbers display (floating text on hits)
   - Reset button (reset characters to starting positions)
4. Configure opponent as dummy:
   - AI disabled or set to passive (doesn't attack)
   - Infinite health (optional, for testing combos)
   - Can be set to block/dodge on command (advanced)
5. Scene settings:
   - No round timer (or very long timer)
   - No round limits (continuous practice)
6. Save scene in `Assets/Knockout/Scenes/`

**Design Guidance:**
- Training mode is for practice, not competition
- Dummy AI should be predictable and controllable
- Environment can be simple (grid floor, neutral lighting)
- Focus on functionality over aesthetics

**Verification Checklist:**
- [ ] Scene created and functional
- [ ] Player and dummy characters present
- [ ] UI elements working
- [ ] Dummy AI passive or controllable
- [ ] Scene accessible from menu (if menu exists)

**Testing Instructions:**

Manual testing:
- Open training scene
- Enter play mode
- Verify player can practice mechanics
- Test dummy AI behavior

**Commit Message Template:**
```
feat(training): create training mode scene

- Set up dedicated practice environment
- Add player and dummy AI opponent
- Configure for continuous practice
- Prepare for training UI features
```

**Estimated Tokens:** ~5,000

---

### Task 10: Create TrainingModeUI Component

**Goal:** Implement training mode settings UI for toggles and controls.

**Files to Create:**
- `Assets/Knockout/Scripts/UI/TrainingModeUI.cs` - UI component class

**Prerequisites:**
- Task 9 complete (TrainingMode scene exists)

**Implementation Steps:**

1. Create `TrainingModeUI` component in `Knockout.UI` namespace
2. Implement training mode controls:
   - Dependencies: Player and dummy character references, UI Toggle components
3. Training options (UI toggles):
   - Unlimited stamina (toggle)
   - Unlimited health (toggle)
   - Show damage numbers (toggle)
   - Dummy AI behavior: Passive / Block / Dodge (dropdown or cycle button)
   - Reset positions button
4. Implement toggle functionality:
   - Unlimited stamina: Set CharacterStamina regeneration very high or disable consumption
   - Unlimited health: Disable damage or instant heal
   - Show damage numbers: Enable/disable damage number display
   - Dummy behavior: Set AI state or disable AI
5. Implement reset button:
   - Reset characters to starting positions and states
   - Reset health, stamina to full
   - Clear combo state
6. Visual elements:
   - Settings panel (can be toggled on/off with key press, e.g., Tab)
   - Checkboxes/toggles for each option
   - Buttons for actions
7. Positioning: Side panel or overlay (non-intrusive)

**Design Guidance:**
- Training UI should be accessible but not blocking combat view
- Toggles should have immediate effect
- Reset button useful for repeated practice of specific scenarios
- Settings persist during session (not across game restarts unless saved)

**Verification Checklist:**
- [ ] Settings panel displays correctly
- [ ] Toggles functional (unlimited stamina, health)
- [ ] Dummy behavior controls work
- [ ] Reset button functional
- [ ] UI toggleable on/off

**Testing Instructions:**

Manual testing in TrainingMode scene:
- Toggle unlimited stamina and perform attacks
- Toggle unlimited health and take damage
- Change dummy AI behavior
- Use reset button
- Verify all settings work

**Commit Message Template:**
```
feat(training): implement TrainingModeUI component

- Add training mode settings panel
- Unlimited stamina and health toggles
- Dummy AI behavior controls
- Reset button for practice scenarios
```

**Estimated Tokens:** ~8,000

---

### Task 11: Create DamageNumbersUI Component

**Goal:** Display floating damage numbers on hits (training mode feature).

**Files to Create:**
- `Assets/Knockout/Scripts/UI/DamageNumbersUI.cs` - UI component class

**Prerequisites:**
- Understand Unity UI WorldSpace or floating text techniques

**Implementation Steps:**

1. Create `DamageNumbersUI` component in `Knockout.UI` namespace
2. Implement damage number spawning:
   - Dependencies: `CharacterCombat` (for both characters)
   - Subscribe to: `OnHitLanded` events
3. Create damage number prefab:
   - UI Text element (or TextMeshPro)
   - Animation: float upward and fade out over ~1 second
   - Color-coded: white (normal), yellow (combo), red (special move)
4. Implement number display:
   - On hit: instantiate damage number at hit position (world space)
   - Display damage value (rounded to integer or 1 decimal)
   - Play float-up animation
   - Destroy after animation completes
5. Pool damage number instances (avoid constant instantiation)
6. Option to enable/disable (controlled by TrainingModeUI toggle)

**Design Guidance:**
- Damage numbers provide precise feedback (how much damage dealt)
- Useful for testing and balancing
- Animation should be quick (don't clutter screen)
- Color coding helps distinguish hit types
- Optional: show combo scaling indicator (e.g., "15 (75%)")

**Verification Checklist:**
- [ ] Damage numbers spawn on hits
- [ ] Numbers display correct damage values
- [ ] Animation plays smoothly
- [ ] Color-coded by hit type
- [ ] Can be toggled on/off

**Testing Instructions:**

Manual testing:
- Enable damage numbers in training mode
- Perform various attacks
- Verify damage values accurate
- Check animations and colors

**Commit Message Template:**
```
feat(training): implement DamageNumbersUI component

- Display floating damage numbers on hits
- Color-coded by hit type
- Float-up and fade animation
- Toggleable in training mode
```

**Estimated Tokens:** ~7,000

---

### Task 12: Implement Dummy AI for Training Mode

**Goal:** Create controllable dummy AI opponent for practice.

**Files to Create:**
- `Assets/Knockout/Scripts/AI/DummyAI.cs` - Dummy AI controller

**Prerequisites:**
- Understand existing AI system (CharacterAI, AIStateMachine)

**Implementation Steps:**

1. Create `DummyAI` component (or extend existing AI)
2. Implement dummy behaviors (selectable via TrainingModeUI):
   - **Passive:** Stand idle, doesn't attack or defend
   - **Blocking:** Always blocks incoming attacks
   - **Dodging:** Attempts to dodge incoming attacks (tests dodge timing)
   - **Counter:** Blocks then counter-attacks (tests parry follow-up)
3. Disable aggressive AI:
   - No autonomous attacks
   - No approach behavior
   - Reactive only
4. Implement behavior switching:
   - Public method `SetBehavior(DummyBehavior behavior)` called by TrainingModeUI
   - Enum `DummyBehavior { Passive, Blocking, Dodging, Counter }`
5. Integrate with character:
   - Replace or disable CharacterAI on dummy character
   - Add DummyAI component

**Design Guidance:**
- Dummy should be predictable and consistent
- Blocking dummy: always blocks (tests combo breaking)
- Dodging dummy: dodges at consistent timing (tests attack timing)
- Passive dummy: punching bag (pure damage testing)
- Counter dummy: blocks and counter-attacks (tests defense and spacing)

**Verification Checklist:**
- [ ] DummyAI component functional
- [ ] Passive behavior works (stands idle)
- [ ] Blocking behavior works (always blocks)
- [ ] Dodging behavior works (attempts dodges)
- [ ] Behavior switching functional

**Testing Instructions:**

Manual testing:
- Set dummy to each behavior mode
- Test player attacks against each mode
- Verify dummy responds as expected

**Commit Message Template:**
```
feat(training): implement DummyAI for training mode

- Create controllable dummy AI behaviors
- Passive, Blocking, Dodging, Counter modes
- Predictable and consistent for practice
- Switchable via training UI
```

**Estimated Tokens:** ~7,000

---

### Task 13: Polish UI Animations and Feedback

**Goal:** Add polish animations, transitions, and visual feedback to UI.

**Files to Modify:**
- All UI components - Add animations and transitions

**Prerequisites:**
- All UI components implemented (Tasks 1-11)

**Implementation Steps:**

1. Add UI animations:
   - **Combo counter:** Scale pulse on each hit, flash on sequence completion
   - **Special move cooldown:** Glow or pulse when ready
   - **Stamina bar:** Flash red when depleted
   - **Scoring breakdown:** Slide-in animation on appear, fade-out on disappear
   - **Damage numbers:** Float-up and fade (already implemented)
2. Add smooth transitions:
   - Lerp (smooth interpolation) for bar fills (stamina, cooldown, momentum)
   - Fade-in/fade-out for contextual elements
3. Add visual feedback:
   - Hit flash (screen flash or character outline) on combo hit
   - Special effects on parry success (particle effect or screen effect)
   - Audio hooks for UI events (combo ding, special ready beep, etc.)
4. Optimize animations:
   - Use Unity Animator or DOTween for smooth animations
   - Ensure animations don't impact performance (60fps maintained)

**Design Guidance:**
- Polish makes UI feel responsive and satisfying
- Subtle animations better than overly flashy
- Animations should enhance readability, not distract
- Audio cues reinforce visual feedback

**Verification Checklist:**
- [ ] All UI elements have appropriate animations
- [ ] Transitions smooth and polished
- [ ] Visual feedback clear and satisfying
- [ ] Performance maintained (60fps)

**Testing Instructions:**

Manual testing:
- Trigger all UI elements in play mode
- Verify animations play smoothly
- Check for performance issues
- Get feedback on feel/polish

**Commit Message Template:**
```
feat(ui): add polish animations and visual feedback

- Animate UI elements for responsiveness
- Smooth transitions and interpolations
- Visual feedback for game events
- Maintain 60fps performance
```

**Estimated Tokens:** ~6,000

---

### Task 14: Comprehensive UI and Training Integration Testing

**Goal:** Verify all UI and training mode systems work correctly.

**Files to Create:**
- `Assets/Knockout/Tests/PlayMode/UI/UIIntegrationTests.cs`
- `Assets/Knockout/Tests/PlayMode/Training/TrainingModeTests.cs`

**Prerequisites:**
- All previous tasks complete

**Implementation Steps:**

1. Create UI integration tests:
   - Test: All UI elements subscribe to correct events
   - Test: UI updates on gameplay events (stamina change, combo hit, score change)
   - Test: Contextual UI appears/disappears correctly
   - Test: UI displays accurate data (values, percentages, timers)
   - Test: UI doesn't cause performance issues (frame rate test)
2. Create training mode tests:
   - Test: Training mode scene loads correctly
   - Test: Unlimited stamina toggle works
   - Test: Unlimited health toggle works
   - Test: Dummy AI behaviors work
   - Test: Reset button resets state correctly
   - Test: Damage numbers display accurately
3. Test edge cases:
   - UI with multiple simultaneous events (combo + special move + parry)
   - UI at different resolutions
   - Training toggles mid-combat
   - Rapid reset button presses

**Design Guidance:**
- UI should never cause gameplay issues
- All UI data should be accurate (no desyncs)
- Training mode should be stable with all toggles

**Verification Checklist:**
- [ ] All integration tests pass
- [ ] UI functional in gameplay
- [ ] Training mode functional
- [ ] No console errors or warnings
- [ ] Performance maintained (60fps)

**Testing Instructions:**

Run all tests:
- UI integration tests
- Training mode tests
- Manual gameplay test with all UI active

**Commit Message Template:**
```
test(ui,training): add comprehensive integration tests

- Test UI event subscriptions and updates
- Test training mode toggles and features
- Verify UI accuracy and performance
- Cover edge cases and simultaneous events
```

**Estimated Tokens:** ~8,000

---

### Task 15: Documentation and Final Polish

**Goal:** Document UI systems and training mode, finalize Phase 5 and overall project.

**Files to Create:**
- `Assets/Knockout/Scripts/UI/UI_SYSTEMS.md` - UI documentation
- `docs/TRAINING_MODE.md` - Training mode guide
- `docs/CORE_MECHANICS_IMPLEMENTATION.md` - Overall implementation summary

**Files to Modify:**
- All Phase 5 scripts - Remove debug logs, finalize comments

**Prerequisites:**
- All previous tasks complete
- All tests passing

**Implementation Steps:**

1. Create UI_SYSTEMS.md:
   - Overview of all UI components
   - How to configure UI elements
   - Contextual display behavior explanation
   - Customization guide (colors, positioning, animations)
   - Troubleshooting
2. Create TRAINING_MODE.md:
   - How to use training mode
   - Explanation of all toggles and features
   - Dummy AI behavior guide
   - Practice tips and use cases
3. Create CORE_MECHANICS_IMPLEMENTATION.md:
   - Summary of all implemented systems (Phases 1-5)
   - Architecture overview
   - How systems integrate
   - Extension points for future work
   - Known limitations and future enhancements
4. Review all Phase 5 scripts:
   - Remove debug logs
   - Finalize XML comments
   - Verify naming conventions
5. Update main README with links to all documentation

**Design Guidance:**
- Documentation should be comprehensive and accessible
- Guides should help new developers understand the systems
- Training mode guide should help players learn mechanics

**Verification Checklist:**
- [ ] All documentation complete and comprehensive
- [ ] All debug logs removed
- [ ] XML comments complete
- [ ] No compiler warnings
- [ ] Main README updated

**Testing Instructions:**

Final checks:
- Build project (verify no errors)
- Run all tests (verify all pass)
- Code review
- Documentation review

**Commit Message Template:**
```
docs(ui,training): add comprehensive documentation

- Create UI_SYSTEMS.md guide
- Create TRAINING_MODE.md user guide
- Create CORE_MECHANICS_IMPLEMENTATION.md summary
- Remove debug logging and finalize code
- Update main README
```

**Estimated Tokens:** ~7,000

---

## Phase 5 Verification

**Completion Checklist:**
- [ ] All 15 tasks completed
- [ ] All tests passing (EditMode + PlayMode)
- [ ] All UI components functional and polished
- [ ] Contextual UI appears/disappears correctly
- [ ] Training mode scene created and functional
- [ ] Training mode toggles and features working
- [ ] Dummy AI behaviors implemented
- [ ] Damage numbers display accurate
- [ ] UI performance acceptable (60fps maintained, Profiler shows all UI components < 0.1ms per frame total, no GC allocations from UI updates)
- [ ] Documentation complete
- [ ] Code reviewed and cleaned
- [ ] No console errors or warnings

**All Phases Complete:**
- ✅ Phase 0: Foundation
- ✅ Phase 1: Core Resource Systems (Stamina, Enhanced Knockdowns)
- ✅ Phase 2: Advanced Defense (Dodge, Parry)
- ✅ Phase 3: Combo System (Chains, Sequences)
- ✅ Phase 4: Special Moves & Judge Scoring
- ✅ Phase 5: UI Systems & Training Mode

**Core Fighting Mechanics - COMPLETE**

---

## Post-Implementation Considerations

**Future Enhancements (Not in Scope):**
- AI enhancement to use all new mechanics strategically
- Online multiplayer integration (architecture supports it)
- Additional characters with unique combos and special moves
- Advanced training mode features (combo trials, challenges)
- Replay system
- Enhanced VFX and audio integration
- Character customization and progression systems

**Known Limitations:**
- AI uses basic behavioral state machine (not advanced fighting game AI)
- One special move per character (can be extended)
- Judge scoring weights not playtested (will require balancing)
- Training mode is basic (no tutorials or guided training)

**Balancing and Tuning:**
- After implementation, extensive playtesting required
- Tune stamina costs, regeneration rates
- Balance combo damage scaling and sequence bonuses
- Adjust special move cooldowns and costs
- Fine-tune judge scoring weights
- Test all systems with various player skill levels

---

## Celebration

🎉 **Congratulations!** 🎉

All Core Fighting Mechanics have been implemented:
- Stamina management with exhaustion penalties
- Directional dodge with i-frames and parry system
- Hybrid combo system with natural chains and predefined sequences
- Character signature special moves with cooldown + stamina gating
- Comprehensive judge scoring and round decisions
- Full contextual UI for all systems
- Training mode for practice and learning

The Knockout boxing game now has a deep, skill-based fighting system ready for playtesting and polish. Great work!

---

**End of Implementation Plan**
