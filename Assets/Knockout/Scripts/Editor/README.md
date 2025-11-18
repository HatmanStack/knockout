# Phase 1 Asset Generation - Editor Scripts

## Overview

This folder contains Unity Editor scripts that programmatically generate Phase 1 assets. These scripts solve the CLI limitation by creating Unity binary assets (.inputactions, .asset, .unity, .prefab) that cannot be created via command-line tools.

## Quick Start

After opening the project in Unity Editor 2021.3.8f1:

1. **Generate All Phase 1 Assets:**
   ```
   Tools > Knockout > Generate Phase 1 Assets
   ```
   This runs all generation scripts in sequence.

**OR** run individual generators:

2. **Input Actions:**
   ```
   Tools > Knockout > Generate Input Actions
   ```
   Then manually:
   - Select `KnockoutInputActions.inputactions` in Project
   - Check "Generate C# Class" in Inspector
   - Set path to `Assets/Knockout/Scripts/Input/KnockoutInputActions.cs`
   - Click Apply

3. **ScriptableObject Assets:**
   ```
   Tools > Knockout > Generate ScriptableObject Assets
   ```

4. **GameplayTest Scene:**
   ```
   Tools > Knockout > Generate GameplayTest Scene
   ```

5. **Character Prefabs:**
   ```
   Tools > Knockout > Generate Character Prefabs
   ```

## What Gets Generated

### InputActionsGenerator.cs
Creates: `KnockoutInputActions.inputactions`
- Gameplay action map (Movement, Jab, Hook, Uppercut, Block)
- UI action map (Pause)
- Keyboard and gamepad bindings

**Manual Step Required:** You must generate the C# wrapper class via Inspector.

### Phase1AssetGenerator.cs
Creates multiple assets:

**ScriptableObjects:**
- `BaseCharacterStats.asset` - Character stats with default values
- `AttackData_Jab.asset` - Jab attack properties
- `AttackData_Hook.asset` - Hook attack properties
- `AttackData_Uppercut.asset` - Uppercut attack properties

**Scene:**
- `GameplayTest.unity` - Test scene with:
  - Ground plane (50x50 arena)
  - Boundary walls
  - Main and fill lights
  - Spawn points for player and AI
  - Organized hierarchy

**Prefabs:**
- `PlayerCharacter.prefab` - Player character with:
  - Animator (Humanoid avatar)
  - Rigidbody (kinematic)
  - Capsule Collider
  - CharacterController script
  - Hitboxes/Hurtboxes containers
  - BaseCharacterStats assigned
- `AICharacter.prefab` - Prefab variant of PlayerCharacter

## Verification

After running the generators, verify via Test Runner:

**Window > General > Test Runner**

**EditMode Tests:**
- `InputActionsTests` - Verify Input Actions asset structure
- `CharacterDataTests` - Verify ScriptableObject assets
- `GameplayTestSceneTests` - Verify scene structure

**PlayMode Tests:**
- `CharacterPrefabTests` - Verify prefab components

All tests should pass after asset generation.

## Manual Steps Still Required

### 1. Enable Input System (One-Time Setup)
- Edit > Project Settings > Player
- Other Settings > Active Input Handling: "Input System Package (New)" or "Both"
- Restart Unity when prompted

### 2. Generate Input Actions C# Class
After running `Generate Input Actions`:
- Select `KnockoutInputActions.inputactions`
- Inspector > Generate C# Class: ☑
- C# Class File: `Assets/Knockout/Scripts/Input/KnockoutInputActions.cs`
- Click Apply

### 3. Configure Model Import Settings (If Needed)
If ElizabethWarren.fbx is not configured as Humanoid:
- Select `Assets/Knockout/Models/Characters/BaseCharacter/ElizabethWarren.fbx`
- Inspector > Rig tab
- Animation Type: Humanoid
- Click Apply
- Click Configure to verify bone mapping

### 4. Configure Animation Import Settings (Optional)
Retarget animations if needed:
- Select animation FBX in `AnimationClips/`
- Inspector > Rig tab
- Animation Type: Humanoid
- Avatar Definition: Copy from Other Avatar
- Source: ElizabethWarren Avatar
- Click Apply

## Troubleshooting

### "Could not find character model" Error
**Problem:** Character model not found at expected path.

**Solution:**
1. Verify model exists: `Assets/Knockout/Models/Characters/BaseCharacter/ElizabethWarren.fbx`
2. If model is missing, run the migration from the original location

### "CharacterStats not found" Error
**Problem:** BaseCharacterStats.asset doesn't exist yet.

**Solution:**
Run `Tools > Knockout > Generate ScriptableObject Assets` first, then generate prefabs.

### Input Actions C# Class Not Generating
**Problem:** C# class not created after running generator.

**Solution:**
This is a manual step. The generator creates the .inputactions file, but Unity's Input System requires manual C# generation via Inspector.

### Tests Still Failing
**Problem:** Tests fail after running generators.

**Solution:**
1. Check Console for errors during asset generation
2. Verify all assets exist at expected paths
3. Run generators in order: ScriptableObjects → Scene → Prefabs
4. Refresh Asset Database: Right-click in Project > Refresh

## Architecture Notes

These scripts use Unity Editor APIs:
- `ScriptableObject.CreateInstance<T>()` - Create ScriptableObject instances
- `AssetDatabase.CreateAsset()` - Save assets to disk
- `EditorSceneManager` - Create and save scenes
- `PrefabUtility` - Create and save prefabs
- `SerializedObject/SerializedProperty` - Modify private fields

The scripts are in an Editor assembly and only run in Unity Editor, not in builds.

## Phase 2 and Beyond

Similar editor scripts can be created for future phases:
- Animator Controller generation (Phase 2)
- Animation State Machine setup (Phase 2)
- Combat prefab configuration (Phase 3)

This pattern enables automated project setup while working within Unity's asset serialization constraints.
