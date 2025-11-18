# Phase 2: Animation System & State Machine

## Phase Goal

Build the complete animation system including the Animator Controller with layered state machines, blend trees for locomotion, animation states for all combat actions, and the CharacterAnimator component to manage animation playback. By the end of this phase, characters will have fully functional animations that respond to manual testing inputs (no gameplay input yet, but you can trigger animations via Inspector for testing).

**Success Criteria:**
- Animator Controller created with three layers (Base, Upper Body, Full Body Override)
- All animation clips integrated into state machines
- Blend trees configured for locomotion (idle, walk, strafe)
- CharacterAnimator component controls animation state
- Animation events fire correctly and trigger code callbacks
- Character prefabs animate correctly in test scene
- All tests passing


---

## Prerequisites

### Must Complete First
- Phase 1 complete (all tasks and verification)
- Character model and animations imported
- Character prefabs created

### Verify Before Starting
1. Open GameplayTest scene without errors
2. Character prefabs instantiate correctly
3. All animation clips visible in Project window
4. Animator component exists on character prefabs

---

## Tasks

### Task 1: Create Avatar Masks for Animation Layers

**Goal:** Create Avatar Masks to control which bones each animation layer affects, enabling simultaneous locomotion and upper-body attacks.

**Files to Create:**
- `Assets/Knockout/Animations/Characters/BaseCharacter/AnimatorMasks/UpperBodyMask.mask`
- `Assets/Knockout/Animations/Characters/BaseCharacter/AnimatorMasks/FullBodyMask.mask`

**Prerequisites:**
- Phase 1 Task 2 complete (character model with Humanoid rig imported)

**Implementation Steps:**

1. **Create Upper Body Mask:**
   - Navigate to `Assets/Knockout/Animations/Characters/BaseCharacter/AnimatorMasks/`
   - Right-click > Create > Avatar Mask
   - Name it "UpperBodyMask"
   - Double-click to open in Inspector

2. **Configure Upper Body Mask:**
   - In Humanoid section, enable/disable body parts:
     - **Disabled (grayed out):** Root, IK (all), Left Leg (all), Right Leg (all), Hips
     - **Enabled (green):** Spine, Chest, Upper Chest, Neck, Head, Left Shoulder, Left Arm (all), Right Shoulder, Right Arm (all)
   - This allows upper body animations (punches, blocks) while legs continue locomotion
   - Save changes (Ctrl+S)

3. **Create Full Body Mask:**
   - Right-click > Create > Avatar Mask
   - Name it "FullBodyMask"
   - Double-click to open

4. **Configure Full Body Mask:**
   - In Humanoid section, **enable all body parts** (all should be green)
   - This allows full-body overrides for hit reactions, knockdowns, knockouts
   - Save changes

5. **Verify masks:**
   - Select each mask in Project window
   - Inspector should show green (enabled) and grayed (disabled) body parts correctly
   - No console warnings

**Verification Checklist:**
- [ ] UpperBodyMask.mask exists and configured correctly
- [ ] FullBodyMask.mask exists with all body parts enabled
- [ ] Masks visible in Project window
- [ ] No console errors or warnings

**Testing Instructions:**
No automated tests for Avatar Masks (Unity assets). Visual verification in Inspector is sufficient.

**Commit Message Template:**
```
feat(animation): create avatar masks for animation layers

- Add UpperBodyMask for spine and arms only
- Add FullBodyMask for full-body overrides
- Configure humanoid body part selections
```


---

### Task 2: Create Animator Controller with Base Layer

**Goal:** Create the Animator Controller asset and set up the Base Layer (locomotion) with blend trees for movement.

**Files to Create:**
- `Assets/Knockout/Animations/Characters/BaseCharacter/BaseCharacterAnimatorController.controller`

**Prerequisites:**
- Task 1 complete (Avatar Masks created)
- Phase 1 Task 3 complete (animation clips imported)

**Implementation Steps:**

1. **Create Animator Controller:**
   - Navigate to `Assets/Knockout/Animations/Characters/BaseCharacter/`
   - Right-click > Create > Animator Controller
   - Name it "BaseCharacterAnimatorController"
   - Double-click to open Animator window

2. **Configure Base Layer:**
   - Rename "Base Layer" to "Locomotion" (right-click layer name)
   - Keep Weight at 1.0
   - No Avatar Mask (affects full body)

3. **Create parameters for locomotion:**
   - In Parameters tab, add:
     - `MoveSpeed` (Float) - default 0
     - `MoveDirectionX` (Float) - default 0
     - `MoveDirectionY` (Float) - default 0

4. **Create Idle state:**
   - The "Entry" state should connect to default state
   - Right-click in Animator graph > Create State > Empty
   - Name it "Idle"
   - Assign animation: Find and assign "Idle" animation clip
   - Set as default state (right-click > Set as Layer Default State)
   - Make it orange (default state color)

5. **Create Movement blend tree:**
   - Right-click > Create State > From New Blend Tree
   - Name it "Movement"
   - Double-click to enter blend tree

6. **Configure Movement blend tree:**
   - Change Blend Type to "2D Freeform Directional"
   - Set Parameters: MoveDirectionX (horizontal), MoveDirectionY (vertical)
   - Add motion fields (click +):
     - Add "Idle" clip: Position (0, 0)
     - Add "WalkForward" clip: Position (0, 1)
     - Add "WalkBack" clip: Position (0, -1)
     - Add "StrafeLeft" clip: Position (-1, 0)
     - Add "StrafeRight" clip: Position (1, 0)
     - Optional: Add diagonal walk animations if available (e.g., (1, 1) for forward-right)
   - Unity will auto-compute blending

7. **Set up transitions:**
   - Navigate back to Locomotion layer (click "Locomotion" in breadcrumb)
   - Create transition: Idle → Movement
     - Condition: MoveSpeed > 0.1
     - Has Exit Time: False
     - Transition Duration: 0.2
   - Create transition: Movement → Idle
     - Condition: MoveSpeed < 0.1
     - Has Exit Time: False
     - Transition Duration: 0.2

8. **Test blend tree:**
   - Select "Movement" blend tree
   - In Inspector, manually adjust MoveDirectionX and MoveDirectionY sliders
   - Preview window should show smooth blending between walk animations
   - Verify no foot sliding (animations should be configured as in-place or root motion)

**Verification Checklist:**
- [ ] BaseCharacterAnimatorController.controller exists
- [ ] Locomotion layer created with Idle and Movement states
- [ ] Movement blend tree configured with at least 5 motion fields
- [ ] Transitions between Idle and Movement work correctly
- [ ] Parameters (MoveSpeed, MoveDirectionX, MoveDirectionY) created
- [ ] Preview shows smooth animation blending

**Testing Instructions:**
Manual testing in Animator window preview is sufficient for this task. Automated testing of Animator Controllers is complex and not recommended for initial setup.

**Commit Message Template:**
```
feat(animation): create animator controller with locomotion layer

- Add BaseCharacterAnimatorController with Locomotion layer
- Create Idle and Movement blend tree states
- Configure 2D blend tree for directional movement
- Add parameters: MoveSpeed, MoveDirectionX, MoveDirectionY
- Set up transitions between Idle and Movement
```


---

### Task 3: Create Upper Body Layer for Attacks and Defense

**Goal:** Add the Upper Body layer to the Animator Controller for attack and block animations that can play while character is moving.

**Files to Modify:**
- `Assets/Knockout/Animations/Characters/BaseCharacter/BaseCharacterAnimatorController.controller`

**Prerequisites:**
- Task 1 complete (UpperBodyMask created)
- Task 2 complete (Base layer created)

**Implementation Steps:**

1. **Add Upper Body layer:**
   - Open BaseCharacterAnimatorController in Animator window
   - In Layers tab, click "+"
   - Name new layer "UpperBody"
   - Set Weight to 1.0
   - Set Mask to "UpperBodyMask"
   - Set Blending to "Override"

2. **Create parameters for attacks:**
   - Add parameters:
     - `AttackTrigger` (Trigger)
     - `AttackType` (Int) - default 0
     - `IsBlocking` (Bool) - default false
     - `UpperBodyWeight` (Float) - default 0

3. **Create Empty state (default):**
   - In UpperBody layer, right-click > Create State > Empty
   - Name it "Empty"
   - Set as default state (right-click > Set as Layer Default State)
   - This state plays nothing (upper body follows base layer)

4. **Create attack states:**
   - First, **inspect imported animation clips** from Phase 1 to determine naming:
     - Check `Assets/Knockout/Animations/Characters/BaseCharacter/AnimationClips/`
     - Look for clips named with left/right variants (e.g., "JabLeft", "JabRight") OR generic names (e.g., "Jab")
   - **If left/right variants exist:** Create 6 states (JabLeft, JabRight, HookLeft, HookRight, UppercutLeft, UppercutRight)
   - **If only generic clips exist:** Create 3 states (Jab, Hook, Uppercut) and adjust AttackType parameter to have only 3 values
   - For each state:
     - Assign corresponding animation clip
     - Ensure Motion does not loop
     - Configure transitions in step 7

5. **Create Recovery state:**
   - Create state "AttackRecovery"
   - Leave motion empty (brief pause)
   - Set Fixed Duration: 0.1 seconds
   - This represents recovery time after attack

6. **Create Block state:**
   - Create state "Block"
   - Assign animation clip: "Block" (should be looping)

7. **Set up attack transitions:**
   - For each attack state (Jab, Hook, Uppercut):
     - **Empty → Attack:**
       - Condition: AttackTrigger (trigger), AttackType == [corresponding index]
         - 0 = Jab, 1 = Hook, 2 = Uppercut
       - Has Exit Time: False
       - Transition Duration: 0.05 (quick snap into attack)
     - **Attack → AttackRecovery:**
       - Condition: None
       - Has Exit Time: True
       - Exit Time: 0.95 (near end of animation)
       - Transition Duration: 0.1
     - **AttackRecovery → Empty:**
       - Condition: None
       - Has Exit Time: True
       - Exit Time: 1.0
       - Transition Duration: 0.1

8. **Set up block transitions:**
   - **Empty → Block:**
     - Condition: IsBlocking == true
     - Has Exit Time: False
     - Transition Duration: 0.1
   - **Block → Empty:**
     - Condition: IsBlocking == false
     - Has Exit Time: False
     - Transition Duration: 0.2

9. **Test Upper Body layer:**
   - Select Animator Controller
   - In Parameters tab, manually trigger AttackTrigger and set AttackType
   - Verify transitions work correctly
   - Verify upper body animates while base layer locomotion continues

**Verification Checklist:**
- [ ] UpperBody layer created with UpperBodyMask assigned
- [ ] All attack states created with correct animation clips
- [ ] AttackRecovery state exists
- [ ] Block state created
- [ ] Parameters created: AttackTrigger, AttackType, IsBlocking, UpperBodyWeight
- [ ] Transitions connect properly: Empty → Attack → Recovery → Empty
- [ ] Block transitions work: Empty ↔ Block

**Testing Instructions:**
Manual testing via Animator window preview. Test by setting parameters and triggering attacks while in Movement state (base layer should continue moving).

**Commit Message Template:**
```
feat(animation): add upper body layer for attacks and defense

- Create UpperBody layer with UpperBodyMask
- Add attack states: Jab, Hook, Uppercut (left/right variants)
- Add AttackRecovery state for attack cooldown
- Add Block state with looping animation
- Create parameters: AttackTrigger, AttackType, IsBlocking, UpperBodyWeight
- Configure transitions for attacks and blocking
```


---

### Task 4: Create Full Body Override Layer for Hit Reactions

**Goal:** Add the Full Body Override layer for hit reactions, knockdowns, and knockouts that override all other animations.

**Files to Modify:**
- `Assets/Knockout/Animations/Characters/BaseCharacter/BaseCharacterAnimatorController.controller`

**Prerequisites:**
- Task 1 complete (FullBodyMask created)
- Task 3 complete (UpperBody layer created)

**Implementation Steps:**

1. **Add Full Body Override layer:**
   - In Layers tab, click "+"
   - Name new layer "FullBodyOverride"
   - Set Weight to 0.0 (will be set to 1.0 when active via code)
   - Set Mask to "FullBodyMask"
   - Set Blending to "Override"

2. **Create parameters:**
   - Add parameters:
     - `HitReaction` (Trigger)
     - `HitType` (Int) - default 0
     - `KnockedDown` (Bool) - default false
     - `KnockedOut` (Bool) - default false
     - `OverrideWeight` (Float) - default 0

3. **Create Empty state (default):**
   - Create state "Empty"
   - Set as default state
   - No animation assigned

4. **Create hit reaction states:**
   - Create state "HitReactionLight"
     - Assign animation: "HitReactionLight"
     - Should not loop
   - Create state "HitReactionMedium"
     - Assign animation: "HitReactionMedium"
   - Create state "HitReactionHeavy"
     - Assign animation: "HitReactionHeavy"

5. **Create knockdown states:**
   - Create state "KnockedDown"
     - Assign animation: "Knockdown"
     - Should not loop
   - Create state "GetUp"
     - Assign animation: "GetUp"
     - Should not loop

6. **Create knockout state:**
   - Create state "KnockedOut"
     - Assign animation: "Knockout"
     - Should not loop (or loop lying on ground)

7. **Set up hit reaction transitions:**
   - **Empty → HitReactionLight:**
     - Condition: HitReaction (trigger), HitType == 0
     - Has Exit Time: False
     - Transition Duration: 0.05
   - **Empty → HitReactionMedium:**
     - Condition: HitReaction (trigger), HitType == 1
     - Has Exit Time: False
     - Transition Duration: 0.05
   - **Empty → HitReactionHeavy:**
     - Condition: HitReaction (trigger), HitType == 2
     - Has Exit Time: False
     - Transition Duration: 0.05
   - **All HitReaction states → Empty:**
     - Condition: None
     - Has Exit Time: True
     - Exit Time: 0.9
     - Transition Duration: 0.1

8. **Set up knockdown transitions:**
   - **Empty → KnockedDown:**
     - Condition: KnockedDown == true
     - Has Exit Time: False
     - Transition Duration: 0.05
   - **KnockedDown → GetUp:**
     - Condition: None
     - Has Exit Time: True
     - Exit Time: 0.95
     - Transition Duration: 0.2
   - **GetUp → Empty:**
     - Condition: None
     - Has Exit Time: True
     - Exit Time: 0.95
     - Transition Duration: 0.1
     - Also set: KnockedDown = false (in transition)

9. **Set up knockout transition:**
   - **Empty → KnockedOut:**
     - Condition: KnockedOut == true
     - Has Exit Time: False
     - Transition Duration: 0.1
   - No exit from KnockedOut (match over)

10. **Configure layer weight control:**
    - OverrideWeight parameter will be controlled by code
    - When hit reaction/knockdown occurs, code sets OverrideWeight to 1.0
    - When returning to Empty, code sets OverrideWeight to 0.0

**Verification Checklist:**
- [ ] FullBodyOverride layer created with FullBodyMask
- [ ] All hit reaction states created (Light, Medium, Heavy)
- [ ] Knockdown and GetUp states created
- [ ] KnockedOut state created
- [ ] Parameters created: HitReaction, HitType, KnockedDown, KnockedOut, OverrideWeight
- [ ] Transitions work correctly
- [ ] Layer weight starts at 0.0

**Testing Instructions:**
Manual testing: Set OverrideWeight to 1.0, trigger HitReaction with different HitTypes. Verify hit reaction plays and overrides locomotion and attacks.

**Commit Message Template:**
```
feat(animation): add full body override layer for hit reactions

- Create FullBodyOverride layer with FullBodyMask
- Add hit reaction states: Light, Medium, Heavy
- Add knockdown sequence: KnockedDown → GetUp
- Add KnockedOut state (no exit)
- Create parameters: HitReaction, HitType, KnockedDown, KnockedOut, OverrideWeight
- Configure transitions for all reaction types
```


---

### Task 5: Assign Animator Controller to Character Prefabs

**Goal:** Assign the completed Animator Controller to the character prefabs and verify animations play in the scene.

**Files to Modify:**
- `Assets/Knockout/Prefabs/Characters/PlayerCharacter.prefab`
- `Assets/Knockout/Prefabs/Characters/AICharacter.prefab`

**Prerequisites:**
- Task 4 complete (all Animator Controller layers created)

**Implementation Steps:**

1. **Assign controller to PlayerCharacter:**
   - Open PlayerCharacter.prefab
   - Select root GameObject
   - In Animator component, assign "BaseCharacterAnimatorController" to "Controller" field
   - Verify Avatar is still assigned (should be BaseCharacter avatar)
   - Apply Root Motion: Checked
   - Update Mode: Normal
   - Culling Mode: Always Animate
   - Save prefab

2. **Assign controller to AICharacter:**
   - Open AICharacter.prefab
   - Same steps as above
   - Save prefab

3. **Test in scene:**
   - Open GameplayTest scene
   - Drag PlayerCharacter prefab into scene at PlayerSpawnPoint
   - Enter Play mode
   - Open Animator window (Window > Animation > Animator)
   - Select character instance in Hierarchy
   - Verify Animator shows Idle animation playing
   - Manually set MoveSpeed parameter to 0.5
   - Verify character transitions to Movement blend tree
   - Exit Play mode

4. **Test attack animations:**
   - In Play mode, set AttackType to 0, trigger AttackTrigger
   - Verify Jab animation plays
   - Test other attack types (1 = Hook, 2 = Uppercut)
   - Verify transitions back to Empty after attack completes

5. **Test hit reactions:**
   - Set OverrideWeight to 1.0
   - Trigger HitReaction with HitType 0
   - Verify HitReactionLight plays and overrides other animations
   - Test other hit types

**Verification Checklist:**
- [ ] Animator Controller assigned to both prefabs
- [ ] Idle animation plays automatically on start
- [ ] Movement blend tree transitions work when MoveSpeed changes
- [ ] Attack animations trigger correctly
- [ ] Hit reactions override other animations
- [ ] No console errors in Play mode

**Testing Instructions:**
Manual testing in Play mode is sufficient. Verify all animation states are reachable by manipulating parameters in Animator window while in Play mode.

**Commit Message Template:**
```
feat(animation): assign animator controller to character prefabs

- Assign BaseCharacterAnimatorController to PlayerCharacter and AICharacter
- Verify Avatar assignment and root motion settings
- Test all animation states in Play mode
- Confirm locomotion, attacks, and hit reactions work correctly
```


---

### Task 6: Create CharacterAnimator Component

**Goal:** Create the CharacterAnimator component that wraps the Animator and provides a clean API for other systems to control animations.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterAnimator.cs`

**Prerequisites:**
- Task 5 complete (Animator Controller assigned to prefabs)

**Implementation Steps:**

1. **Create CharacterAnimator.cs:**
   - Create new C# script: `Assets/Knockout/Scripts/Characters/Components/CharacterAnimator.cs`
   - Inherit from `MonoBehaviour` in the `Knockout.Characters.Components` namespace
   - Add `[RequireComponent(typeof(Animator))]` attribute
   - **Create nested static class AnimatorParams** with const string fields for all animator parameter names:
     - MoveSpeed, MoveDirectionX, MoveDirectionY, AttackTrigger, AttackType, IsBlocking, UpperBodyWeight
     - HitReaction, HitType, KnockedDown, KnockedOut, OverrideWeight
     - Purpose: Centralize parameter names, avoid string typos
   - **Define C# events** for animation callbacks (Phase 2 Task 7 will trigger these):
     - OnAttackStart, OnHitboxActivate, OnHitboxDeactivate, OnAttackRecoveryStart, OnAttackEnd
     - OnHitReactionEnd, OnKnockedDownComplete, OnGetUpComplete
   - **Cache Animator reference** in private field, get via `GetComponent<Animator>()` in Awake
   - **Implement locomotion methods:**
     - `SetMovement(Vector2 direction, float speed)`: Set MoveDirectionX/Y and MoveSpeed parameters
   - **Implement attack methods:**
     - `TriggerAttack(int attackType)`: Set AttackType and trigger AttackTrigger
     - `TriggerJab()`, `TriggerHook()`, `TriggerUppercut()`: Convenience wrappers calling TriggerAttack with indices 0, 1, 2
   - **Implement defense methods:**
     - `SetBlocking(bool isBlocking)`: Set IsBlocking parameter
   - **Implement hit reaction methods:**
     - `TriggerHitReaction(int hitType)`: Set HitType, trigger HitReaction, call SetOverrideLayerWeight(1.0)
     - `TriggerKnockdown()`: Set KnockedDown bool, enable override layer
     - `TriggerKnockout()`: Set KnockedOut bool, enable override layer
   - **Implement layer weight control methods:**
     - `SetUpperBodyLayerWeight(float weight)`: Set parameter and call `_animator.SetLayerWeight(1, weight)` for UpperBody layer
     - `SetOverrideLayerWeight(float weight)`: Set parameter and call `_animator.SetLayerWeight(2, weight)` for FullBodyOverride layer
   - **Implement public Animation Event receiver methods** (Unity Animation Events call these):
     - `AnimEvent_OnAttackStart()`: Invoke OnAttackStart event
     - `AnimEvent_OnHitboxActivate()`: Invoke OnHitboxActivate event
     - `AnimEvent_OnHitboxDeactivate()`: Invoke OnHitboxDeactivate event
     - `AnimEvent_OnAttackRecoveryStart()`: Invoke OnAttackRecoveryStart event
     - `AnimEvent_OnAttackEnd()`: Invoke OnAttackEnd event, reset upper body layer weight to 0
     - `AnimEvent_OnHitReactionEnd()`: Invoke OnHitReactionEnd event, reset override layer weight to 0
     - `AnimEvent_OnKnockedDownComplete()`: Invoke OnKnockedDownComplete event
     - `AnimEvent_OnGetUpComplete()`: Invoke OnGetUpComplete event, set KnockedDown to false, reset override weight
     - **Note:** These methods MUST be public for Unity to find them
   - **Implement OnValidate()** for editor-time caching of Animator reference
   - Follow Phase-0 code organization and naming conventions

2. **Add CharacterAnimator to prefabs:**
   - Open PlayerCharacter.prefab
   - Add CharacterAnimator component
   - Save prefab
   - Repeat for AICharacter.prefab

3. **Update CharacterController to reference CharacterAnimator:**
   - Open `CharacterController.cs`
   - Uncomment or add field:
     ```csharp
     private CharacterAnimator _characterAnimator;
     ```
   - In `CacheComponents()`, add:
     ```csharp
     _characterAnimator = GetComponent<CharacterAnimator>();
     ```
   - Optionally expose public property:
     ```csharp
     public CharacterAnimator CharacterAnimator => _characterAnimator;
     ```

**Verification Checklist:**
- [ ] CharacterAnimator.cs created and compiles
- [ ] Component added to both character prefabs
- [ ] CharacterController caches reference to CharacterAnimator
- [ ] All public methods are accessible
- [ ] Animation Event receiver methods are public

**Testing Instructions:**
Create play mode test to verify CharacterAnimator API:

```csharp
// File: Assets/Knockout/Tests/PlayMode/Characters/CharacterAnimatorTests.cs
[UnityTest]
public IEnumerator CharacterAnimator_SetMovement_UpdatesAnimatorParameters()
{
    // Arrange
    GameObject characterGO = new GameObject("TestCharacter");
    Animator animator = characterGO.AddComponent<Animator>();
    CharacterAnimator charAnimator = characterGO.AddComponent<CharacterAnimator>();

    // Need to assign animator controller
    string controllerPath = "Assets/Knockout/Animations/Characters/BaseCharacter/BaseCharacterAnimatorController.controller";
    RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
    animator.runtimeAnimatorController = controller;

    yield return null;

    // Act
    charAnimator.SetMovement(new Vector2(0.5f, 1f), 0.8f);
    yield return null;

    // Assert
    Assert.AreEqual(0.5f, animator.GetFloat("MoveDirectionX"), 0.01f);
    Assert.AreEqual(1f, animator.GetFloat("MoveDirectionY"), 0.01f);
    Assert.AreEqual(0.8f, animator.GetFloat("MoveSpeed"), 0.01f);

    // Cleanup
    Object.Destroy(characterGO);
}

[UnityTest]
public IEnumerator CharacterAnimator_TriggerAttack_SetsAnimatorParameters()
{
    // Arrange
    GameObject characterGO = new GameObject("TestCharacter");
    Animator animator = characterGO.AddComponent<Animator>();
    CharacterAnimator charAnimator = characterGO.AddComponent<CharacterAnimator>();

    string controllerPath = "Assets/Knockout/Animations/Characters/BaseCharacter/BaseCharacterAnimatorController.controller";
    RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
    animator.runtimeAnimatorController = controller;

    yield return null;

    // Act
    charAnimator.TriggerJab();
    yield return null;

    // Assert
    Assert.AreEqual(0, animator.GetInteger("AttackType"));
    // Trigger is consumed immediately, can't assert it

    // Cleanup
    Object.Destroy(characterGO);
}
```

Run tests in PlayMode Test Runner.

**Commit Message Template:**
```
feat(characters): create CharacterAnimator component

- Add CharacterAnimator.cs to manage animation playback
- Implement methods for locomotion, attacks, defense, reactions
- Define animation events receiver methods
- Expose events for animation callbacks
- Add component to character prefabs
- Update CharacterController to cache CharacterAnimator reference
- Add play mode tests for animator API
```


---

### Task 7: Add Animation Events to Animation Clips

**Goal:** Configure Animation Events on attack, hit reaction, and knockdown animations to trigger gameplay callbacks (hitbox activation, state transitions, etc.).

**Files to Modify:**
- Animation clips in `Assets/Knockout/Animations/Characters/BaseCharacter/AnimationClips/`

**Prerequisites:**
- Task 6 complete (CharacterAnimator component created)
- Animation clips imported

**Implementation Steps:**

1. **Understand Animation Events:**
   - Animation Events call public methods on components attached to the GameObject
   - CharacterAnimator has public `AnimEvent_*` methods to receive these calls
   - Events are added at specific normalized times (0.0 to 1.0) in animation timeline

2. **Add events to Jab animation:**
   - Select "JabLeft" animation clip (or "Jab" if no left/right variants)
   - In Inspector, scroll to Animation Events section (or open Animation window)
   - Add event at time 0.0:
     - Function: `AnimEvent_OnAttackStart`
   - Add event at ~40% normalized time (when fist extends):
     - Function: `AnimEvent_OnHitboxActivate`
   - Add event at ~60% normalized time (when fist retracts):
     - Function: `AnimEvent_OnHitboxDeactivate`
   - Add event at ~80% normalized time (recovery phase starts):
     - Function: `AnimEvent_OnAttackRecoveryStart`
   - Add event at 1.0 (end of animation):
     - Function: `AnimEvent_OnAttackEnd`
   - Repeat for "JabRight" if exists

3. **Add events to Hook animation:**
   - Same event structure as Jab, but adjust timing:
     - OnAttackStart: 0.0
     - OnHitboxActivate: ~35% (hook winds up more)
     - OnHitboxDeactivate: ~65%
     - OnAttackRecoveryStart: ~75%
     - OnAttackEnd: 1.0
   - Repeat for left/right variants

4. **Add events to Uppercut animation:**
   - Same structure:
     - OnAttackStart: 0.0
     - OnHitboxActivate: ~40% (uppercut has long windup)
     - OnHitboxDeactivate: ~60%
     - OnAttackRecoveryStart: ~70%
     - OnAttackEnd: 1.0
   - Repeat for variants

5. **Add events to hit reaction animations:**
   - For all hit reactions (Light, Medium, Heavy):
     - OnHitReactionEnd: 0.9 (near end, allows slight blend time)

6. **Add events to knockdown animations:**
   - "Knockdown" animation:
     - OnKnockedDownComplete: 0.95 (near end)
   - "GetUp" animation:
     - OnGetUpComplete: 0.95

7. **Verify events:**
   - Open Animation window (Window > Animation > Animation)
   - Select character prefab instance in scene
   - Select each animation clip in Animation window dropdown
   - Verify event markers appear on timeline
   - Click each marker to verify function name is correct

8. **Test events in Play mode:**
   - Enter Play mode with character in scene
   - Add temporary Debug.Log calls to CharacterAnimator AnimEvent methods
   - Trigger attacks via Animator parameters
   - Verify events fire and logs appear in Console at correct times
   - Remove Debug.Log calls after testing

**Verification Checklist:**
- [ ] All attack animations have animation events added
- [ ] Event timing looks correct (hitbox activates mid-swing)
- [ ] Hit reaction animations have OnHitReactionEnd event
- [ ] Knockdown animations have completion events
- [ ] Events call correct function names (matches CharacterAnimator public methods)
- [ ] Events fire in Play mode (test with Debug.Log)

**Testing Instructions:**
Create play mode test to verify events fire:

```csharp
// File: Assets/Knockout/Tests/PlayMode/Characters/AnimationEventsTests.cs
[UnityTest]
public IEnumerator JabAnimation_FiresAnimationEvents()
{
    // Arrange
    GameObject characterGO = new GameObject("TestCharacter");
    Animator animator = characterGO.AddComponent<Animator>();
    CharacterAnimator charAnimator = characterGO.AddComponent<CharacterAnimator>();

    string controllerPath = "Assets/Knockout/Animations/Characters/BaseCharacter/BaseCharacterAnimatorController.controller";
    animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);

    bool attackStartFired = false;
    bool hitboxActivateFired = false;
    bool hitboxDeactivateFired = false;
    bool attackEndFired = false;

    charAnimator.OnAttackStart += () => attackStartFired = true;
    charAnimator.OnHitboxActivate += () => hitboxActivateFired = true;
    charAnimator.OnHitboxDeactivate += () => hitboxDeactivateFired = true;
    charAnimator.OnAttackEnd += () => attackEndFired = true;

    yield return null;

    // Act
    charAnimator.TriggerJab();

    // Wait for animation to complete (assume ~1 second max)
    float timeout = 2f;
    float elapsed = 0f;
    while (!attackEndFired && elapsed < timeout)
    {
        elapsed += Time.deltaTime;
        yield return null;
    }

    // Assert
    Assert.IsTrue(attackStartFired, "OnAttackStart should fire");
    Assert.IsTrue(hitboxActivateFired, "OnHitboxActivate should fire");
    Assert.IsTrue(hitboxDeactivateFired, "OnHitboxDeactivate should fire");
    Assert.IsTrue(attackEndFired, "OnAttackEnd should fire");

    // Cleanup
    Object.Destroy(characterGO);
}
```

Run test in PlayMode Test Runner.

**Commit Message Template:**
```
feat(animation): add animation events to attack and reaction clips

- Add events to Jab, Hook, Uppercut animations
- Configure hitbox activation/deactivation timing
- Add attack start, recovery, and end events
- Add hit reaction end events to all reaction animations
- Add knockdown completion events
- Add play mode test to verify events fire correctly
```


---

### Task 8: Create Animation Testing Scene

**Goal:** Create a simple test UI or script to manually test all animations without needing full gameplay implementation.

**Files to Create:**
- `Assets/Knockout/Scripts/Utilities/AnimationTester.cs` (debug script)

**Prerequisites:**
- Task 7 complete (animation events added)

**Implementation Steps:**

1. **Create AnimationTester.cs:**
   - Create new C# script at path above
   - Implement keyboard controls to test animations:

   ```csharp
   using UnityEngine;
   using Knockout.Characters.Components;

   namespace Knockout.Utilities
   {
       /// <summary>
       /// Debug utility for testing character animations via keyboard input.
       /// Attach to character prefab during development.
       /// </summary>
       public class AnimationTester : MonoBehaviour
       {
           [SerializeField] private CharacterAnimator characterAnimator;

           private Vector2 _moveInput;

           private void Start()
           {
               if (characterAnimator == null)
                   characterAnimator = GetComponent<CharacterAnimator>();
           }

           private void Update()
           {
               HandleMovementInput();
               HandleAttackInput();
               HandleDefenseInput();
               HandleReactionInput();
           }

           private void HandleMovementInput()
           {
               _moveInput.x = Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0f;
               _moveInput.y = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;

               float speed = _moveInput.magnitude;
               characterAnimator.SetMovement(_moveInput, speed);
           }

           private void HandleAttackInput()
           {
               if (Input.GetKeyDown(KeyCode.Alpha1))
               {
                   Debug.Log("Testing Jab animation");
                   characterAnimator.TriggerJab();
               }

               if (Input.GetKeyDown(KeyCode.Alpha2))
               {
                   Debug.Log("Testing Hook animation");
                   characterAnimator.TriggerHook();
               }

               if (Input.GetKeyDown(KeyCode.Alpha3))
               {
                   Debug.Log("Testing Uppercut animation");
                   characterAnimator.TriggerUppercut();
               }
           }

           private void HandleDefenseInput()
           {
               bool blocking = Input.GetKey(KeyCode.LeftShift);
               characterAnimator.SetBlocking(blocking);
           }

           private void HandleReactionInput()
           {
               if (Input.GetKeyDown(KeyCode.Alpha4))
               {
                   Debug.Log("Testing Light Hit Reaction");
                   characterAnimator.TriggerHitReaction(0);
               }

               if (Input.GetKeyDown(KeyCode.Alpha5))
               {
                   Debug.Log("Testing Medium Hit Reaction");
                   characterAnimator.TriggerHitReaction(1);
               }

               if (Input.GetKeyDown(KeyCode.Alpha6))
               {
                   Debug.Log("Testing Heavy Hit Reaction");
                   characterAnimator.TriggerHitReaction(2);
               }

               if (Input.GetKeyDown(KeyCode.Alpha7))
               {
                   Debug.Log("Testing Knockdown");
                   characterAnimator.TriggerKnockdown();
               }

               if (Input.GetKeyDown(KeyCode.Alpha8))
               {
                   Debug.Log("Testing Knockout");
                   characterAnimator.TriggerKnockout();
               }
           }

           private void OnGUI()
           {
               GUILayout.BeginArea(new Rect(10, 10, 300, 400));
               GUILayout.Label("Animation Tester Controls:");
               GUILayout.Label("WASD - Move");
               GUILayout.Label("1 - Jab");
               GUILayout.Label("2 - Hook");
               GUILayout.Label("3 - Uppercut");
               GUILayout.Label("LeftShift - Block");
               GUILayout.Label("4 - Light Hit");
               GUILayout.Label("5 - Medium Hit");
               GUILayout.Label("6 - Heavy Hit");
               GUILayout.Label("7 - Knockdown");
               GUILayout.Label("8 - Knockout");
               GUILayout.EndArea();
           }
       }
   }
   ```

2. **Add AnimationTester to test scene:**
   - Open GameplayTest scene
   - Drag PlayerCharacter prefab into scene
   - Add AnimationTester component to instance (not prefab)
   - CharacterAnimator reference should auto-populate
   - Save scene

3. **Test all animations:**
   - Enter Play mode
   - Use keyboard controls to test:
     - Movement (WASD)
     - Jab (1), Hook (2), Uppercut (3)
     - Block (Hold LeftShift)
     - Hit reactions (4, 5, 6)
     - Knockdown (7), Knockout (8)
   - Verify all animations play correctly
   - Verify animation events fire (check Console for CharacterAnimator logs if added)

4. **Test simultaneous animations:**
   - Walk forward (W) while jabbing (1) - upper body should punch, legs should walk
   - Block while moving - block animation should play
   - Trigger hit reaction while moving - hit reaction should override all

5. **Remove or disable AnimationTester for production:**
   - This script is for development only
   - Remove from character instances before building final game
   - Or add `#if UNITY_EDITOR` preprocessor directive

**Verification Checklist:**
- [ ] AnimationTester.cs created and compiles
- [ ] Component added to test character in scene
- [ ] All keyboard controls work correctly
- [ ] On-screen GUI displays control hints
- [ ] All animations trigger as expected
- [ ] Layer blending works (upper body attacks while moving)
- [ ] Full body override works (hit reactions override all)

**Testing Instructions:**
Manual testing in Play mode. Go through all controls and verify visual results.

**Commit Message Template:**
```
feat(utilities): add animation testing debug script

- Create AnimationTester component with keyboard controls
- Add controls for movement, attacks, defense, and reactions
- Display on-screen control hints via OnGUI
- Add to GameplayTest scene for manual animation testing
```


---

## Phase Verification

Before proceeding to Phase 3, verify the following:

### All Tasks Complete
- [ ] Task 1: Avatar Masks created for animation layers
- [ ] Task 2: Animator Controller Base Layer (locomotion) created
- [ ] Task 3: Upper Body Layer for attacks and defense created
- [ ] Task 4: Full Body Override Layer for hit reactions created
- [ ] Task 5: Animator Controller assigned to character prefabs
- [ ] Task 6: CharacterAnimator component created and added to prefabs
- [ ] Task 7: Animation Events added to all animation clips
- [ ] Task 8: Animation testing script created and tested

### Integration Points
- [ ] All animation clips properly assigned to Animator states
- [ ] Layer masks correctly configured (upper body, full body)
- [ ] CharacterAnimator API controls all animations correctly
- [ ] Animation events fire and trigger C# event callbacks
- [ ] Character prefabs animate correctly in test scene

### Animation Behavior
- [ ] Idle animation plays by default
- [ ] Movement blend tree smoothly blends between walk directions
- [ ] Attack animations play and return to idle
- [ ] Block animation loops correctly
- [ ] Hit reactions override locomotion and attacks
- [ ] Knockdown sequence plays through to get-up
- [ ] Knockout has no exit (character stays down)

### Tests Passing
- [ ] All PlayMode tests pass
- [ ] No console errors in Play mode
- [ ] Manual testing with AnimationTester successful

### Git Status
- [ ] All changes committed with descriptive messages
- [ ] Commit history follows conventional commits format

### Known Limitations
- No input system integration yet (Phase 3)
- No combat mechanics or hit detection (Phase 3)
- No AI (Phase 4)
- AnimationTester is debug tool only (not production code)

---

**Phase 2 Complete!** Proceed to [Phase 3: Combat Mechanics & Hit Detection](Phase-3.md).
