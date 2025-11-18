# Phase 2: Animation System Setup Instructions

This document provides step-by-step instructions for completing Phase 2 setup in Unity Editor.

## Prerequisites

- Unity Editor 2021.3.8f1 LTS running
- Phase 2 scripts committed to repository
- MainScene.unity open in Unity Editor

## Step 1: Run Phase 2 Asset Generator

1. **Open Unity Editor** with the Knockout project
2. **Wait for compilation** - Ensure Console shows no errors
3. **Run the generator:**
   - Menu: **Tools > Knockout > Generate Phase 2 Assets**
   - This will create:
     - PlayerCharacter.prefab and AICharacter.prefab
     - UpperBodyMask.mask and FullBodyMask.mask
     - BaseCharacterAnimatorController.controller

4. **Verify creation:**
   - Check Console for success messages
   - Navigate to `Assets/Knockout/Prefabs/Characters/` - should see 2 prefabs
   - Navigate to `Assets/Knockout/Animations/Characters/BaseCharacter/AnimatorMasks/` - should see 2 masks
   - Navigate to `Assets/Knockout/Animations/Characters/BaseCharacter/` - should see controller

## Step 2: Add CharacterAnimator Component to Prefabs

### For PlayerCharacter:

1. **Open prefab:**
   - Navigate to `Assets/Knockout/Prefabs/Characters/`
   - Double-click **PlayerCharacter.prefab** to open in Prefab Mode

2. **Add CharacterAnimator component:**
   - Select root GameObject in Hierarchy
   - **Add Component** button in Inspector
   - Search for **"CharacterAnimator"**
   - Click to add component

3. **Verify Animator is configured:**
   - Check **Animator** component exists
   - **Controller** field should be empty (we'll assign in next step)
   - **Avatar** should be assigned automatically from FBX
   - **Apply Root Motion**: Checked
   - **Update Mode**: Normal
   - **Culling Mode**: Always Animate

4. **Save prefab:**
   - Click **Save** button in Prefab Mode header
   - Exit Prefab Mode (click **<** arrow or click scene name)

### For AICharacter:

1. Repeat above steps for **AICharacter.prefab**

## Step 3: Assign Animator Controller to Prefabs

### For PlayerCharacter:

1. **Open prefab** (double-click PlayerCharacter.prefab)
2. **Select root GameObject**
3. **In Animator component:**
   - Click circle icon next to **Controller** field
   - Search for **"BaseCharacterAnimatorController"**
   - Double-click to assign
4. **Verify assignment:**
   - Controller field should now show "BaseCharacterAnimatorController"
   - No errors in Console
5. **Save prefab and exit Prefab Mode**

### For AICharacter:

1. Repeat above steps for **AICharacter.prefab**

## Step 4: Configure Animator Controller Layers (Optional Detail)

The asset generator creates basic layers. You can enhance them in Animator window:

1. **Open Animator window:**
   - Menu: **Window > Animation > Animator**

2. **Open controller:**
   - Project window: Navigate to `Assets/Knockout/Animations/Characters/BaseCharacter/`
   - Select **BaseCharacterAnimatorController**
   - Animator window should show state machine

3. **Review layers (already created by generator):**
   - **Locomotion** layer (Base Layer)
     - Contains: Idle state
     - Weight: 1.0
     - Mask: None
   - **UpperBody** layer
     - Contains: Empty state
     - Weight: 1.0
     - Mask: UpperBodyMask
     - Blending: Override
   - **FullBodyOverride** layer
     - Contains: Empty state
     - Weight: 0.0 (controlled by code)
     - Mask: FullBodyMask
     - Blending: Override

## Step 5: Add Animation States (Expanded Setup)

This step expands the animator controller with all attack and reaction states.

### Add Attack States to UpperBody Layer:

1. **Select UpperBody layer** in Animator window
2. **Create attack states:**
   - Right-click in graph > **Create State > Empty**
   - Name: **"JabLeft"**
   - In Inspector:
     - **Motion**: Drag **"Left_Jab"** animation clip from `AnimationClips/` folder
     - **Speed**: 1.0
     - **Loop Time**: Unchecked

3. **Repeat for other attacks:**
   - **JabRight** → Motion: "Right_Jab" (if it exists, otherwise skip)
   - **HookLeft** → Motion: "left_hook"
   - **HookRight** → Motion: "Right_hook"
   - **UppercutLeft** → Motion: "left_uppercut"
   - **UppercutRight** → Motion: "Right_uppercut"

4. **Create Recovery state:**
   - Create state: **"AttackRecovery"**
   - Motion: Leave empty
   - **Fixed Duration**: 0.1 seconds

5. **Create Block state:**
   - Create state: **"Block"**
   - Motion: "Block" animation clip
   - **Loop Time**: Checked (looping animation)

6. **Create transitions:**
   - **Empty → JabLeft:**
     - Condition: `AttackTrigger` (trigger), `AttackType` == 0
     - Has Exit Time: False
     - Transition Duration: 0.05
   - **JabLeft → AttackRecovery:**
     - No condition
     - Has Exit Time: True
     - Exit Time: 0.95
   - **AttackRecovery → Empty:**
     - No condition
     - Has Exit Time: True
   - **Empty → Block:**
     - Condition: `IsBlocking` == true
     - Has Exit Time: False
   - **Block → Empty:**
     - Condition: `IsBlocking` == false
     - Has Exit Time: False

   (Repeat similar transitions for other attacks)

### Add Hit Reaction States to FullBodyOverride Layer:

1. **Select FullBodyOverride layer**
2. **Create hit reaction states:**
   - **HitReactionLight** → Motion: "hit_by_jab_V1"
   - **HitReactionMedium** → Motion: "right_hit_V1"
   - **HitReactionHeavy** → Motion: "hit_by_hook_V1"
   - **KnockedDown** → Motion: "Knockouts_Countdown_V1"
   - **GetUp** → Motion: (if available, or reuse recovery animation)
   - **KnockedOut** → Motion: "knockout_V1"

3. **Create transitions:**
   - **Empty → HitReactionLight:**
     - Condition: `HitReaction` (trigger), `HitType` == 0
   - **HitReactionLight → Empty:**
     - Has Exit Time: True, Exit Time: 0.9
   - (Similar for Medium and Heavy)
   - **Empty → KnockedDown:**
     - Condition: `KnockedDown` == true
   - **KnockedDown → GetUp:**
     - Has Exit Time: True, Exit Time: 0.95
   - **GetUp → Empty:**
     - Has Exit Time: True
   - **Empty → KnockedOut:**
     - Condition: `KnockedOut` == true
   - No exit from KnockedOut

## Step 6: Add Animation Events to Clips

Animation Events call methods on CharacterAnimator at specific points in animations.

### For Attack Animations (e.g., Left_Jab):

1. **Select animation clip:**
   - Navigate to `Assets/Knockout/Animations/Characters/BaseCharacter/AnimationClips/`
   - Click **Left_Jab.fbx**
   - Expand to see **"Left_Jab"** animation clip sub-asset
   - Select the animation clip

2. **Open Animation window:**
   - Menu: **Window > Animation > Animation**
   - The selected clip should load

3. **Add events:**
   - Scrub timeline to **start (0:00)**
     - Click **Add Event** button (or click on event bar)
     - **Function**: `AnimEvent_OnAttackStart`
   - Scrub to **~40% of animation** (when fist extends)
     - Add event: `AnimEvent_OnHitboxActivate`
   - Scrub to **~60%** (when fist retracts)
     - Add event: `AnimEvent_OnHitboxDeactivate`
   - Scrub to **~80%**
     - Add event: `AnimEvent_OnAttackRecoveryStart`
   - Scrub to **end (1:00)**
     - Add event: `AnimEvent_OnAttackEnd`

4. **Save changes:**
   - Events are saved automatically with animation
   - Verify: White event markers should appear on timeline

5. **Repeat for all attack animations:**
   - Left_Jab, Right_Jab, left_hook, Right_hook, left_uppercut, Right_uppercut
   - Adjust timing based on animation (faster attacks activate hitbox earlier)

### For Hit Reaction Animations:

1. **Select hit reaction clip** (e.g., hit_by_jab_V1)
2. **Add event at 90% of animation:**
   - Function: `AnimEvent_OnHitReactionEnd`
3. **Repeat for:** right_hit_V1, hit_by_hook_V1

### For Knockdown Animations:

1. **Select Knockouts_Countdown_V1**
2. **Add event at 95%:**
   - Function: `AnimEvent_OnKnockedDownComplete`

3. **If GetUp animation exists:**
   - Add event at 95%: `AnimEvent_OnGetUpComplete`

## Step 7: Test Animations in Scene

1. **Open MainScene.unity**

2. **Add PlayerCharacter to scene:**
   - Drag **PlayerCharacter.prefab** from Project to Hierarchy
   - Position: **(-5, 0, 0)**
   - Rotation: **(0, 90, 0)**

3. **Add AnimationTester component:**
   - Select PlayerCharacter instance in Hierarchy
   - **Add Component** > Search: **"AnimationTester"**
   - Component should auto-assign CharacterAnimator reference

4. **Enter Play Mode:**
   - Click **Play** button
   - On-screen UI should show controls

5. **Test all animations:**
   - **Movement:** Press WASD - character should play Idle (no movement animations configured yet)
   - **Attacks:** Press 1, 2, 3 - should trigger Jab, Hook, Uppercut
   - **Block:** Hold LeftShift - should play Block animation
   - **Reactions:** Press 4, 5, 6 - should play hit reactions
   - **Knockdown:** Press 7 - should play knockdown
   - **Knockout:** Press 8 - should play knockout

6. **Verify Animation Events:**
   - Open **Console** window
   - Trigger attacks - should see Animation Event logs (if you added Debug.Log calls)
   - If events don't fire, double-check event names match method names exactly

7. **Exit Play Mode**

8. **Remove AnimationTester from prefab:**
   - This is a debug tool only
   - Do NOT save it to the prefab
   - Delete AnimationTester component from scene instance

## Step 8: Run Tests

1. **Open Test Runner:**
   - Menu: **Window > General > Test Runner**

2. **Run PlayMode tests:**
   - Select **PlayMode** tab
   - Click **Run All**
   - Tests should pass (or be ignored if assets not created)

3. **Verify results:**
   - All CharacterAnimatorTests should pass
   - If tests fail, check Console for error messages
   - Common issues:
     - Animator controller not assigned
     - Animation clips missing
     - Parameter names mismatch

## Completion Checklist

- [ ] Phase 2 asset generator run successfully
- [ ] Character prefabs created with Animator and CharacterAnimator components
- [ ] Avatar masks created (UpperBodyMask, FullBodyMask)
- [ ] Animator controller created with 3 layers
- [ ] Animator controller assigned to both prefabs
- [ ] Animation states created and transitions configured
- [ ] Animation Events added to attack and reaction clips
- [ ] AnimationTester tested all animations successfully
- [ ] PlayMode tests pass
- [ ] No console errors

## Troubleshooting

### "Animator has no controller" error

- Verify BaseCharacterAnimatorController.controller exists
- Assign it manually to Animator component on prefab

### Animation events don't fire

- Check event function names match exactly (case-sensitive)
- Verify CharacterAnimator component is on the GameObject
- Events must call public methods

### Animations don't play

- Verify Animator Controller is assigned
- Check transitions have correct conditions
- Verify parameters are set correctly (use Animator window in Play mode)

### Hit reactions don't override movement

- Check FullBodyOverride layer weight is set to 1.0 when triggered
- Verify FullBodyMask has all body parts enabled
- Check blending mode is "Override"

## Next Steps

- **Phase 3:** Implement combat mechanics and hit detection
- **Phase 4:** Add AI opponent
- **Phase 5:** Polish and integration

## Reference

See Phase-2.md for detailed implementation specification.
