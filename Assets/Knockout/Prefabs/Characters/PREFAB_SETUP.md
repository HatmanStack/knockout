# Character Prefabs Setup Instructions

## Overview
This document provides step-by-step instructions for creating the Player and AI character prefabs in Unity Editor.

## Prerequisites
- GameplayTest scene created (see `Assets/Knockout/Scenes/SCENE_SETUP.md`)
- Elizabeth Warren model imported at `Assets/Knockout/Models/Characters/BaseCharacter/ElizabethWarren.fbx`
- BaseCharacterStats asset created (see `Assets/Knockout/Scripts/Characters/Data/SCRIPTABLE_OBJECTS_SETUP.md`)

## Step 1: Create PlayerCharacter Prefab

### Import Model into Scene

1. Open **GameplayTest.unity** scene
2. In Project window, navigate to `Assets/Knockout/Models/Characters/BaseCharacter/`
3. Drag **ElizabethWarren.fbx** into the scene Hierarchy
4. Rename the GameObject to **"PlayerCharacter"**
5. Set Transform:
   - Position: `(-5, 0, 0)` (at PlayerSpawnPoint)
   - Rotation: `(0, 90, 0)` (facing right toward AI spawn)
   - Scale: `(1, 1, 1)`

### Add Required Components

Select **PlayerCharacter** and add the following components:

#### Rigidbody

1. **Add Component > Rigidbody**
2. Configure:
   - Mass: `70` (average human mass in kg)
   - Drag: `0`
   - Angular Drag: `0.05`
   - Use Gravity: ☑ True
   - Is Kinematic: ☑ True (for animated character, not physics-driven)
   - Constraints:
     - Freeze Rotation X: ☑
     - Freeze Rotation Y: ☑
     - Freeze Rotation Z: ☑

#### Capsule Collider

1. **Add Component > Capsule Collider**
2. Configure:
   - Center: `(0, 1, 0)`
   - Radius: `0.3`
   - Height: `1.8`
   - Direction: Y-Axis
   - Is Trigger: ☐ False (solid collider for character body)

#### CharacterController Script

1. **Add Component > Character Controller** (our custom script)
2. In Inspector:
   - Character Stats: Drag **BaseCharacterStats** asset from `Scripts/Characters/Data/`

### Verify Animator Component

The Animator should already exist from the imported FBX:

1. Select **PlayerCharacter**
2. Verify **Animator** component exists
3. Configure (if needed):
   - Controller: Leave empty (will be assigned in Phase 2)
   - Avatar: Should auto-assign to ElizabethWarren Avatar
   - Apply Root Motion: ☑ Checked (we'll selectively apply in code)
   - Update Mode: **Normal**
   - Culling Mode: **Always Animate** (for testing, optimize later)

**IMPORTANT:** Verify Avatar is Humanoid:
1. Select the **ElizabethWarren.fbx** file in Project
2. Inspector > Rig tab
3. Animation Type should be **"Humanoid"**
4. If not, set it to Humanoid and click **Apply**
5. Click **Configure** to verify bone mapping (all should be green)

### Add Child GameObjects for Colliders

#### Hitboxes Container

1. Select **PlayerCharacter**
2. **Right-click > Create Empty**
3. Rename to **"Hitboxes"**
4. This will contain hand colliders for punches (Phase 3)
5. Leave empty for now

#### Hurtboxes Container

1. Select **PlayerCharacter**
2. **Right-click > Create Empty**
3. Rename to **"Hurtboxes"**
4. This will contain head and body colliders for taking hits (Phase 3)
5. Leave empty for now

### Create Prefab Asset

1. In Project window, navigate to `Assets/Knockout/Prefabs/Characters/`
2. Drag **PlayerCharacter** from Hierarchy to the Project folder
3. Unity creates **PlayerCharacter.prefab**
4. The Hierarchy instance should now show blue text (prefab instance)
5. Delete the instance from the scene (we'll instantiate programmatically)

## Step 2: Create AICharacter Prefab Variant

### Create Variant

1. In Project window at `Assets/Knockout/Prefabs/Characters/`
2. Right-click **PlayerCharacter.prefab**
3. **Create > Prefab Variant**
4. Rename to **"AICharacter"**

### Customize AI Visual Appearance

To distinguish AI from player during testing:

1. Double-click **AICharacter.prefab** to open in Prefab Mode
2. Expand the prefab hierarchy to find the mesh object (child of root)
3. Select the mesh object
4. In **Mesh Renderer** component:
   - Materials > Element 0: Click the material
5. In the material Inspector:
   - Change **Base Map** color tint to red: `(1.0, 0.5, 0.5, 1.0)`
   - Or create a new material variant with different color

6. Click **Save** in Prefab Mode header
7. Exit Prefab Mode (click **< | Prefabs** in Hierarchy)

## Step 3: Test Prefab Instantiation

### Manual Test

1. Open **GameplayTest** scene
2. Drag **PlayerCharacter.prefab** to scene at PlayerSpawnPoint position `(-5, 0, 0)`
3. Drag **AICharacter.prefab** to scene at AISpawnPoint position `(5, 0, 0)`
4. Enter **Play Mode**
5. Verify:
   - Both characters appear in scene
   - Characters are at correct positions
   - Characters face each other
   - No console errors
6. Exit Play Mode
7. Delete both instances from scene

### Verify Component Setup

1. Select **PlayerCharacter.prefab** in Project
2. In Inspector, verify components are present:
   - ☑ Animator (with valid Humanoid Avatar)
   - ☑ Rigidbody (kinematic, rotations frozen)
   - ☑ Capsule Collider (not trigger)
   - ☑ Character Controller (our script)
   - ☑ Hitboxes child GameObject (empty)
   - ☑ Hurtboxes child GameObject (empty)

3. Verify **CharacterController** script:
   - Character Stats: **BaseCharacterStats** asset assigned
   - No errors in Console

## Step 4: Run Automated Tests

1. **Window > General > Test Runner**
2. Select **PlayMode** tab
3. Find **CharacterPrefabTests** (in `Assets/Knockout/Tests/PlayMode/Characters/`)
4. Click **Run All** or **Run Selected**

Tests should verify:
- PlayerCharacter prefab has required components
- AICharacter prefab has required components
- Animator has Humanoid avatar
- Rigidbody exists
- Capsule Collider exists

## Prefab Structure

**Final prefab hierarchy:**

```
PlayerCharacter (prefab)
├── Animator
├── Rigidbody
├── Capsule Collider
├── CharacterController (script)
├── [Mesh/Skeleton from FBX]
│   ├── Armature
│   │   └── [Bones...]
│   └── Mesh
├── Hitboxes (empty container)
└── Hurtboxes (empty container)
```

## Known Limitations at Phase 1

- **No animation controller** assigned (Phase 2)
- **No input handling** (Phase 2)
- **No movement logic** (Phase 2)
- **No hitboxes/hurtboxes** configured (Phase 3)
- **No combat logic** (Phase 3)
- **No AI behavior** (Phase 4)

This is expected! Phase 1 only sets up the structure. Functionality comes in later phases.

## Troubleshooting

### "Missing Avatar" Error

**Problem:** Animator shows "No Avatar" warning

**Solution:**
1. Select the **ElizabethWarren.fbx** in Project
2. Inspector > Rig tab > Animation Type: **Humanoid**
3. Click **Apply**
4. Avatar should appear as sub-asset of FBX

### "Missing Script" Warning

**Problem:** CharacterController shows "Script Missing"

**Solution:**
1. Verify `CharacterController.cs` exists at `Assets/Knockout/Scripts/Characters/`
2. Check for compilation errors in Console
3. Re-assign script if needed

### Character Appears Tiny/Huge

**Problem:** Character scale is wrong

**Solution:**
1. Check import scale in FBX import settings
2. Adjust Transform scale or FBX import scale factor

### Character Sinks Through Ground

**Problem:** Character falls through ground in Play Mode

**Solution:**
1. Verify Rigidbody is set to **Kinematic**
2. Verify Ground has a collider component
3. Check that character Capsule Collider is not a trigger

## Next Steps

- **Phase 2:** Create Animator Controller and hook up animations
- **Phase 2:** Add CharacterInput component for player control
- **Phase 2:** Add CharacterMovement component for locomotion
- **Phase 3:** Configure Hitboxes and Hurtboxes with colliders
- **Phase 3:** Add CharacterCombat and CharacterHealth components
- **Phase 4:** Add CharacterAI component to AICharacter prefab

## Reference

See Phase-1.md Task 7 for the full prefab creation specification.
