# Unity Editor Tasks - Phase 4

This document contains step-by-step instructions for tasks that require Unity Editor GUI access.

## Task 5: Create Haymaker Special Move Asset

**Location:** `Assets/Knockout/Data/SpecialMoves/`

**Steps:**

1. **Create the folder structure** (if it doesn't exist):
   - Right-click in Project window
   - Navigate to `Assets/Knockout/Data/`
   - Create New Folder → `SpecialMoves`

2. **Create the Special Move asset:**
   - Right-click in `Assets/Knockout/Data/SpecialMoves/`
   - Select `Create > Knockout > Special Move Data`
   - Name it: `BaseCharacter_Haymaker`

3. **Configure in Inspector:**
   ```
   Identity:
   - Special Move Name: "Haymaker"

   Base Attack Reference:
   - Base Attack Data: [Drag Right_hook AttackData asset here]

   Damage Modifiers:
   - Damage Multiplier: 2.0
   - Knockback Multiplier: 2.5

   Resource Costs:
   - Cooldown Seconds: 45
   - Stamina Cost: 40

   Special Knockdown:
   - Triggers Special Knockdown: ✓ (checked)
   - Special Knockdown Duration: 3.0

   Animation:
   - Animation Trigger: [Leave empty - will use Hook animation]

   Visual/Audio (Optional):
   - Visual Effect Id: [Leave empty or add custom effect name]
   - Audio Effect Id: [Leave empty or add custom sound name]
   ```

4. **Save:** Ctrl+S or File > Save Project

**Expected Result:**
- Asset appears in Project window at `Assets/Knockout/Data/SpecialMoves/BaseCharacter_Haymaker`
- Inspector shows all configured values
- No errors in Console

---

## Task 6: Add Special Moves Component to Character Prefab

**Location:** `Assets/Knockout/Prefabs/Characters/BaseCharacter.prefab`

**Prerequisites:**
- Task 5 complete (Haymaker asset created)
- CharacterInput, CharacterStamina, CharacterCombat already on prefab

**Steps:**

1. **Open the prefab:**
   - In Project window, navigate to `Assets/Knockout/Prefabs/Characters/`
   - Double-click `BaseCharacter.prefab` to open in Prefab mode

2. **Add CharacterSpecialMoves component:**
   - Select the root GameObject in Hierarchy
   - In Inspector, click **Add Component**
   - Type: `CharacterSpecialMoves`
   - Select `CharacterSpecialMoves` from dropdown

3. **Verify auto-populated dependencies:**
   - Check that these fields are automatically filled:
     - Character Input: [Should reference CharacterInput component]
     - Character Stamina: [Should reference CharacterStamina component]
     - Character Combat: [Should reference CharacterCombat component]
   - If any are missing, drag them from the same GameObject

4. **Assign Special Move Data:**
   - Find **Special Move Data** field (should show "None (SpecialMoveData)")
   - Drag `BaseCharacter_Haymaker` asset from Project window to this field
   - Field should now show: `BaseCharacter_Haymaker (SpecialMoveData)`

5. **Verify component order:**
   - CharacterSpecialMoves should be after:
     - CharacterInput
     - CharacterStamina
     - CharacterCombat
   - This ensures dependencies initialize first

6. **Save prefab:**
   - Click **Prefab > Save** in Hierarchy
   - Or press Ctrl+S

**Expected Result:**
- CharacterSpecialMoves component visible in Inspector
- All dependencies assigned (no "None" fields except optional ones)
- No errors in Console when entering Play mode

**Testing:**
1. Enter Play Mode
2. Press **R key** (or gamepad right trigger)
3. Should see in Console: "Special Move 'Haymaker' executed!"
4. Stamina bar should decrease by 40
5. Cooldown should start (45 seconds)
6. Pressing R again immediately should fail with "OnCooldown" message

---

## Task 11: Create Default ScoringWeights Asset

**Location:** `Assets/Knockout/Data/Scoring/`

**Steps:**

1. **Create the folder structure:**
   - Right-click in Project window
   - Navigate to `Assets/Knockout/Data/`
   - Create New Folder → `Scoring`

2. **Create the Scoring Weights asset:**
   - Right-click in `Assets/Knockout/Data/Scoring/`
   - Select `Create > Knockout > Scoring Weights`
   - Name it: `DefaultScoringWeights`

3. **Configure in Inspector:**
   ```
   Offensive Scoring:
   - Clean Hit Points: 1
   - Combo Hit Bonus: 0.5
   - Combo Sequence Points: 5
   - Special Move Points: 8
   - Knockdown Points: 10
   - Damage Dealt Weight: 0.1

   Defensive Scoring:
   - Block Points: 0.5
   - Parry Points: 2
   - Dodge Points: 1

   Control Scoring:
   - Aggression Points Per Second: 0.1
   - Ring Control Bonus: 0

   Penalties:
   - Exhaustion Penalty: 3
   - Missed Attack Penalty: 0
   ```

4. **Save:** Ctrl+S

**Design Notes:**
- Values emphasize offensive aggression (landing hits) over pure defense
- Parries worth more than blocks (skill-based)
- Special moves and knockdowns highest value (rare, impactful)
- These can be tuned based on playtesting

**Expected Result:**
- Asset created with balanced default values
- No errors in Console
- Asset can be assigned to CharacterScoring components

---

## Task 12: Add Scoring Component to Character Prefab

**Location:** `Assets/Knockout/Prefabs/Characters/BaseCharacter.prefab`

**Prerequisites:**
- Task 11 complete (DefaultScoringWeights created)
- Task 8 complete (CharacterScoring component implemented)
- All Phase 1-3 components on prefab (Combat, Combo, Dodge, Parry, Stamina)

**Steps:**

1. **Open the prefab:**
   - Double-click `BaseCharacter.prefab` in Project window

2. **Add CharacterScoring component:**
   - Select root GameObject
   - Inspector > **Add Component**
   - Type: `CharacterScoring`
   - Select from dropdown

3. **Verify auto-populated dependencies:**
   - Character Controller: [Auto-assigned]
   - Character Combat: [Auto-assigned]
   - Character Combo Tracker: [Auto-assigned]
   - Character Dodge: [Auto-assigned]
   - Character Parry: [Auto-assigned]
   - Character Stamina: [Auto-assigned]
   - Character Special Moves: [Auto-assigned]

4. **Assign Scoring Weights:**
   - Scoring Weights: Drag `DefaultScoringWeights` asset here

5. **Configure Aggression Range:**
   - Aggression Range: 3
   - (Distance in units - character scores aggression when < 3 units from opponent)

6. **Component initialization order:**
   - CharacterScoring should be **last** or near-last
   - It needs to initialize AFTER all action components
   - Order should be approximately:
     1. CharacterInput
     2. CharacterStamina
     3. CharacterCombat
     4. CharacterDodge
     5. CharacterParry
     6. CharacterComboTracker
     7. CharacterSpecialMoves
     8. **CharacterScoring** ← Add last

7. **Save prefab:** Ctrl+S

**Expected Result:**
- CharacterScoring component visible in Inspector
- All dependencies assigned
- No "Missing Reference" errors
- No errors when entering Play mode

**Testing:**
1. Enter Play Mode
2. Perform various actions: attack, block, parry, dodge, combo
3. Check Console for scoring events (if debug logging enabled)
4. Verify score increases based on configured weights
5. At round end, judge decision should trigger based on scores

---

## Troubleshooting

### "Missing Reference" Errors

**Problem:** Component shows "None (ComponentType)" for required dependencies

**Solution:**
1. Ensure all prerequisite components exist on GameObject
2. Manually drag component references from GameObject to fields
3. Check component execution order (Script Execution Order in Project Settings)

### Special Move Doesn't Execute

**Problem:** Pressing R does nothing or shows errors

**Solutions:**
1. Verify Input Actions asset has SpecialMove action: `Assets/Knockout/Resources/Input/KnockoutInputActions.inputactions`
2. Check CharacterInput has OnSpecialMovePressed event wired up
3. Ensure character has enough stamina (40+)
4. Check cooldown isn't active (wait 45 seconds after last use)
5. Console should show reason for failure (OnCooldown, InsufficientStamina, etc.)

### Scoring Not Tracking

**Problem:** Score doesn't increase when actions performed

**Solutions:**
1. Verify CharacterScoring component initialized (check Start() called)
2. Ensure all action components exist and fire events
3. Check ScoringWeights asset assigned
4. Enable debug logging in CharacterScoring to see event subscriptions
5. Verify component initialization order (CharacterScoring must init after action components)

### Animation Issues

**Problem:** Special move animation doesn't play or looks wrong

**Solutions:**
1. Check Base Attack Data reference is correct (should be Hook for Haymaker)
2. Verify Animation Trigger field matches animator parameter (or leave empty to use base)
3. Ensure animator has trigger parameter set up
4. Check Animator Controller has transition for attack type

---

## Verification Checklist

After completing all Unity Editor tasks:

- [ ] Special move asset created and configured
- [ ] CharacterSpecialMoves component added to prefab
- [ ] Special move executes in Play mode (R key works)
- [ ] Cooldown prevents spam
- [ ] Stamina consumed correctly
- [ ] Enhanced damage visible (2x base)
- [ ] Special knockdown triggers
- [ ] Scoring weights asset created
- [ ] CharacterScoring component added to prefab
- [ ] All dependencies assigned
- [ ] Scoring tracks actions correctly
- [ ] No Console errors in Play mode
- [ ] Round timer visible and counts down
- [ ] Judge decision triggers at time expiry

---

## Next Steps

After completing these Unity Editor tasks:
1. Run all PlayMode tests to verify integration
2. Playtest special moves and scoring
3. Tune ScoringWeights values based on feel
4. Proceed to Phase 5 (UI Systems & Training Mode)
