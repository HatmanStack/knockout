# Phase 1: Project Structure & Asset Integration

## Phase Goal

Establish the Unity project folder structure, import and configure character models and animations, create the test scene, and set up the Unity Input System. By the end of this phase, you will have a basic character prefab that can be placed in a scene and is ready for animation hookup in Phase 2.

**Success Criteria:**
- Unity project follows the folder structure defined in Phase 0
- Character model imported and configured with Humanoid rig
- All animation clips imported and properly looped/configured
- Test scene created with basic environment (ground plane, lighting, camera)
- Player and AI character prefabs created
- Input System configured with action maps for movement and combat
- All tests passing (infrastructure tests)

**Estimated Tokens:** ~95,000

---

## Prerequisites

### Must Complete First
- Read Phase 0 thoroughly

### External Dependencies
- Character model files (FBX/GLB) with Humanoid rig
- Animation clip files (FBX/GLB) for full fighting moveset
- Unity 2021.3.8f1 LTS installed
- IDE configured (Visual Studio or Rider)

### Verify Environment
Run these checks before starting:
1. Unity Editor opens project without errors
2. All packages in `Packages/manifest.json` are installed
3. No console errors on project load
4. Test Framework package visible in Window > General > Test Runner

---

## Tasks

### Task 1: Create Folder Structure

**Goal:** Set up the project folder hierarchy following the architecture defined in Phase 0, ensuring consistency and organization for all future assets and code.

**Files to Create:**
- Folder structure under `Assets/Knockout/` (see Phase 0 for complete structure)
- `.gitkeep` files in empty folders (to ensure they're tracked by git)

**Prerequisites:**
- None

**Implementation Steps:**

1. **Create root folders** under `Assets/`:
   - Create `Knockout` folder (all project assets live here)
   - Inside `Knockout`, create primary folders: `Animations`, `Materials`, `Models`, `Prefabs`, `Scenes`, `Scripts`, `Tests`

2. **Create animation subfolders:**
   - Under `Animations/`, create `Characters/BaseCharacter/`
   - Under that, create `AnimationClips/`, `AnimatorMasks/`
   - Place a placeholder `AnimatorController.controller` will be created later

3. **Create materials subfolders:**
   - Under `Materials/`, create `Characters/`

4. **Create models subfolders:**
   - Under `Models/`, create `Characters/BaseCharacter/`
   - Under that, create `Textures/`

5. **Create prefabs subfolders:**
   - Under `Prefabs/`, create `Characters/` and `Systems/`

6. **Create scripts subfolders:**
   - Under `Scripts/`, create folders per Phase 0: `Characters/`, `Combat/`, `AI/`, `Input/`, `Utilities/`
   - Under `Characters/`, create `Components/` and `Data/`
   - Under `Combat/`, create `HitDetection/` and `States/`
   - Under `AI/`, create `States/`
   - Under `Utilities/`, create `Extensions/` and `Helpers/`

7. **Create tests subfolders:**
   - Under `Tests/`, create `EditMode/` and `PlayMode/`
   - Under each, create `Combat/` and `Characters/`

8. **Add .gitkeep files:**
   - For any empty folders that won't have assets yet, create `.gitkeep` file
   - This ensures git tracks the folder structure

**Verification Checklist:**
- [ ] All folders match Phase 0 structure exactly
- [ ] Folders visible in Unity Project window
- [ ] No Unity console errors
- [ ] `.gitkeep` files in place for empty folders

**Testing Instructions:**
No automated tests for this task. Visual verification in Unity Project window is sufficient.

**Commit Message Template:**
```
feat(structure): create project folder hierarchy

- Add Knockout root folder with subfolders
- Create folders for Animations, Materials, Models, Prefabs, Scenes, Scripts, Tests
- Add .gitkeep files for empty folders to ensure git tracking
- Follow architecture structure from Phase-0.md
```

**Estimated Tokens:** ~2,000

---

### Task 2: Import and Configure Character Model

**Goal:** Import the character model (FBX/GLB), configure it with Unity's Humanoid rig, and verify the rig is correctly mapped for animation retargeting.

**Files to Create/Modify:**
- `Assets/Knockout/Models/Characters/BaseCharacter/BaseCharacter.fbx` (imported asset)
- Model import settings (configured via Inspector)

**Prerequisites:**
- Task 1 complete (folder structure exists)
- Character model file available

**Implementation Steps:**

1. **Import the character model:**
   - Copy the character FBX/GLB file into `Assets/Knockout/Models/Characters/BaseCharacter/`
   - Unity will auto-import it
   - Wait for import to complete

2. **Configure import settings:**
   - Select the model file in Project window
   - In Inspector, switch to "Rig" tab
   - Set Animation Type to "Humanoid"
   - Click "Apply"
   - Click "Configure..." to open Avatar Configuration

3. **Verify humanoid rig mapping:**
   - In Avatar Configuration window, ensure all required bones are mapped (green)
   - Required bones: Hips, Spine, Chest, Neck, Head, Shoulders, Upper Arms, Lower Arms, Hands, Upper Legs, Lower Legs, Feet
   - Optional bones (if present): Toes, Eyes, Jaw
   - Fix any red (missing) or yellow (incorrect) mappings

4. **Configure model scale and materials:**
   - In "Model" tab, verify Scale Factor is appropriate (usually 1.0 or 0.01 depending on source)
   - In "Materials" tab, set Material Creation Mode to "Standard" or "Universal Render Pipeline" (URP)
   - Check "Extract Textures..." if materials are embedded
   - Extract materials to `Assets/Knockout/Materials/Characters/`
   - Extract textures to `Assets/Knockout/Models/Characters/BaseCharacter/Textures/`

5. **Create Avatar asset:**
   - Avatar asset should be auto-created by Unity when rig type is set to Humanoid
   - Verify it appears as a sub-asset of the model file

**Verification Checklist:**
- [ ] Model imports without errors (check Console)
- [ ] Animation Type is "Humanoid"
- [ ] Avatar Configuration shows all required bones mapped (green)
- [ ] Avatar asset exists as sub-asset of model
- [ ] Materials and textures extracted to correct folders
- [ ] Model visible in Scene when dragged in (test then delete)

**Testing Instructions:**
Create edit mode test to verify model import:

```csharp
// File: Assets/Knockout/Tests/EditMode/Characters/CharacterModelTests.cs
[Test]
public void BaseCharacterModel_Imports_WithHumanoidAvatar()
{
    // Arrange
    string modelPath = "Assets/Knockout/Models/Characters/BaseCharacter/BaseCharacter.fbx";

    // Act
    GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
    Animator animator = model.GetComponent<Animator>();

    // Assert
    Assert.IsNotNull(model, "Model should exist at path");
    Assert.IsNotNull(animator, "Model should have Animator component");
    Assert.IsTrue(animator.avatar.isHuman, "Avatar should be Humanoid type");
    Assert.IsTrue(animator.avatar.isValid, "Avatar should be valid");
}
```

Run test via Window > General > Test Runner > EditMode > Run All.

**Commit Message Template:**
```
feat(models): import and configure base character model

- Import BaseCharacter.fbx with Humanoid rig
- Configure Avatar bone mapping
- Extract materials to Materials/Characters/
- Extract textures to Models/Characters/BaseCharacter/Textures/
- Add edit mode test to verify model import
```

**Estimated Tokens:** ~5,000

---

### Task 3: Import and Configure Animation Clips

**Goal:** Import all animation clips for the full fighting moveset, configure loop settings, and organize them in the project structure.

**Files to Create/Modify:**
- `Assets/Knockout/Animations/Characters/BaseCharacter/AnimationClips/` (multiple animation clip assets)

**Prerequisites:**
- Task 2 complete (character model imported)
- Animation clip files available (FBX/GLB format)

**Implementation Steps:**

1. **Import animation files:**
   - Copy all animation FBX/GLB files into `Assets/Knockout/Animations/Characters/BaseCharacter/AnimationClips/`
   - Unity will auto-import each file

2. **Configure each animation clip:**
   - For **locomotion animations** (Idle, Walk, Run, Strafe):
     - Select the FBX in Project window
     - In Inspector, go to "Rig" tab, set Animation Type to "Humanoid"
     - Go to "Animation" tab
     - Enable "Loop Time"
     - Disable "Root Transform Position (Y)" baking (keep feet planted)
     - Enable "Root Transform Rotation" baking if character should face forward
     - Set "Root Transform Position (XZ)" based on animation (bake for in-place, don't bake for moving)
     - Click "Apply"

   - For **attack animations** (Jab, Hook, Uppercut):
     - Set Animation Type to "Humanoid"
     - **Disable** "Loop Time"
     - **Enable** "Root Transform Position (XZ)" baking (we'll apply root motion selectively in code)
     - Disable "Root Transform Position (Y)" baking
     - Enable "Root Transform Rotation" baking
     - Click "Apply"

   - For **defense animations** (Block, Dodge):
     - Set Animation Type to "Humanoid"
     - "Block" should have Loop Time enabled (held animation)
     - "Dodge" should have Loop Time disabled (quick motion)
     - Configure root transform baking similarly to attacks
     - Click "Apply"

   - For **reaction animations** (Hit reactions, Knockdown, Knockout):
     - Set Animation Type to "Humanoid"
     - Disable Loop Time (all are one-shot)
     - Enable root motion baking for knockback effects
     - Click "Apply"

3. **Verify animation clip extraction:**
   - Each imported FBX should show an animation clip as a sub-asset (triangle icon to expand)
   - If multiple clips are in one FBX, configure each clip separately in the Animation tab list

4. **Rename clips for clarity:**
   - If FBX filenames aren't descriptive, rename the extracted clips
   - Use naming: `Idle`, `WalkForward`, `WalkBack`, `StrafeLeft`, `StrafeRight`, `Run`, `JabLeft`, `JabRight`, `HookLeft`, `HookRight`, `UppercutLeft`, `UppercutRight`, `Block`, `DodgeLeft`, `DodgeRight`, `DodgeBack`, `HitReactionLight`, `HitReactionMedium`, `HitReactionHeavy`, `Knockdown`, `GetUp`, `Knockout`

5. **Test animations individually:**
   - Create a temporary test scene
   - Drag character model into scene
   - In Animator component, temporarily assign an animation clip to test playback
   - Verify animation plays correctly, loops if expected, doesn't have foot sliding
   - Delete test setup

**Verification Checklist:**
- [ ] All required animation clips imported (minimum ~20 clips for full moveset)
- [ ] Locomotion animations have Loop Time enabled
- [ ] Attack/reaction animations have Loop Time disabled
- [ ] Root motion settings configured per animation type
- [ ] Animation clips visible as sub-assets of FBX files
- [ ] No console errors or import warnings
- [ ] Spot-check 3-4 animations by previewing in Inspector Animation tab

**Testing Instructions:**
Create edit mode test to verify animation clips exist:

```csharp
// File: Assets/Knockout/Tests/EditMode/Characters/AnimationClipsTests.cs
[Test]
public void AnimationClips_Exist_ForRequiredAnimations()
{
    // Arrange
    string[] requiredClips = new string[]
    {
        "Idle", "WalkForward", "JabLeft", "JabRight", "Block", "HitReactionLight", "Knockdown", "Knockout"
    };

    string basePath = "Assets/Knockout/Animations/Characters/BaseCharacter/AnimationClips/";

    // Act & Assert
    foreach (string clipName in requiredClips)
    {
        // Search for clip by name (may be in various FBX files)
        AnimationClip clip = FindAnimationClip(clipName);
        Assert.IsNotNull(clip, $"Animation clip '{clipName}' should exist");
    }
}

private AnimationClip FindAnimationClip(string clipName)
{
    string[] guids = AssetDatabase.FindAssets($"{clipName} t:AnimationClip");
    if (guids.Length == 0) return null;
    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
    return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
}
```

Run test in EditMode Test Runner.

**Commit Message Template:**
```
feat(animations): import and configure character animation clips

- Import all locomotion, attack, defense, and reaction animations
- Configure loop settings per animation type
- Set root motion baking for attacks and reactions
- Organize clips in AnimationClips/ folder
- Add edit mode test to verify required clips exist
```

**Estimated Tokens:** ~8,000

---

### Task 4: Set Up Input System

**Goal:** Create an Input Actions asset defining all player inputs (movement, attacks, defense) and configure the Input System for the project.

**Files to Create:**
- `Assets/Knockout/Scripts/Input/KnockoutInputActions.inputactions` (Input Actions asset)
- `Assets/Knockout/Scripts/Input/KnockoutInputActions.cs` (auto-generated C# wrapper)

**Prerequisites:**
- Task 1 complete (folder structure exists)
- Unity Input System package installed (verify in Package Manager)

**Implementation Steps:**

1. **Enable Input System:**
   - Go to Edit > Project Settings > Player
   - Under "Other Settings", set "Active Input Handling" to "Input System Package (New)" or "Both"
   - Unity will prompt to restart - accept and restart

2. **Create Input Actions asset:**
   - Right-click in `Assets/Knockout/Scripts/Input/`
   - Create > Input Actions
   - Name it `KnockoutInputActions`
   - Double-click to open Input Actions window

3. **Create Action Maps:**
   - Create two Action Maps: `Gameplay` and `UI`

4. **Configure Gameplay Action Map:**
   - Select "Gameplay" action map
   - Add the following actions:

   **Movement (Value, Vector2):**
   - Binding: WASD keys (Keyboard) - Use 2D Vector Composite
     - Up: W, Down: S, Left: A, Right: D
   - Binding: Left Stick (Gamepad) - for future controller support

   **Jab (Button):**
   - Binding: Mouse Left Button (Mouse)
   - Binding: West Button (Gamepad) - X on Xbox, Square on PS

   **Hook (Button):**
   - Binding: Mouse Right Button (Mouse)
   - Binding: South Button (Gamepad) - A on Xbox, X on PS

   **Uppercut (Button):**
   - Binding: Mouse Middle Button (Mouse)
   - Binding: North Button (Gamepad) - Y on Xbox, Triangle on PS

   **Block (Button):**
   - Binding: Left Shift (Keyboard)
   - Binding: East Button (Gamepad) - B on Xbox, Circle on PS

   **Dodge (Button):**
   - Binding: Space (Keyboard)
   - Binding: Left Shoulder (Gamepad) - LB on Xbox, L1 on PS

5. **Configure UI Action Map:**
   - Select "UI" action map
   - Add action: `Pause` (Button)
   - Binding: Escape (Keyboard)
   - Binding: Start Button (Gamepad)

6. **Configure action properties:**
   - For **Button actions** (Jab, Hook, etc.):
     - Action Type: Button
     - Interactions: Default (Press)
   - For **Movement**:
     - Action Type: Value
     - Control Type: Vector2

7. **Generate C# script:**
   - In Input Actions window, check "Generate C# Class"
   - Set path to `Assets/Knockout/Scripts/Input/KnockoutInputActions.cs`
   - Click "Apply"
   - Unity will generate a C# wrapper class

8. **Verify generation:**
   - Close Input Actions window
   - Check that `KnockoutInputActions.cs` exists in `Scripts/Input/`
   - Open file and verify it contains action map classes

**Verification Checklist:**
- [ ] Input System is active input handling method (verify in Project Settings)
- [ ] `KnockoutInputActions.inputactions` exists
- [ ] Gameplay action map contains: Movement, Jab, Hook, Uppercut, Block, Dodge
- [ ] UI action map contains: Pause
- [ ] C# class generated at correct path
- [ ] No compilation errors

**Testing Instructions:**
Create edit mode test to verify Input Actions asset:

```csharp
// File: Assets/Knockout/Tests/EditMode/Input/InputActionsTests.cs
[Test]
public void InputActions_Asset_Exists()
{
    // Arrange
    string assetPath = "Assets/Knockout/Scripts/Input/KnockoutInputActions.inputactions";

    // Act
    var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);

    // Assert
    Assert.IsNotNull(inputActions, "Input Actions asset should exist");
}

[Test]
public void InputActions_ContainsGameplayActionMap()
{
    // Arrange
    var inputActions = new KnockoutInputActions();

    // Act
    var gameplayMap = inputActions.Gameplay;

    // Assert
    Assert.IsNotNull(gameplayMap, "Gameplay action map should exist");
    Assert.IsNotNull(gameplayMap.Movement, "Movement action should exist");
    Assert.IsNotNull(gameplayMap.Jab, "Jab action should exist");
    Assert.IsNotNull(gameplayMap.Hook, "Hook action should exist");
    Assert.IsNotNull(gameplayMap.Uppercut, "Uppercut action should exist");
    Assert.IsNotNull(gameplayMap.Block, "Block action should exist");
    Assert.IsNotNull(gameplayMap.Dodge, "Dodge action should exist");
}
```

Run tests in EditMode Test Runner.

**Commit Message Template:**
```
feat(input): set up Input System with action maps

- Create KnockoutInputActions.inputactions asset
- Define Gameplay action map (Movement, Jab, Hook, Uppercut, Block, Dodge)
- Define UI action map (Pause)
- Configure keyboard and gamepad bindings
- Generate C# wrapper class
- Add edit mode tests for Input Actions
```

**Estimated Tokens:** ~10,000

---

### Task 5: Create Test Scene

**Goal:** Set up a basic test scene for character development with ground plane, lighting, and camera configured with Cinemachine.

**Files to Create:**
- `Assets/Knockout/Scenes/GameplayTest.unity` (new scene)

**Prerequisites:**
- Task 1 complete (folder structure exists)
- Cinemachine package installed (verify in Package Manager)

**Implementation Steps:**

1. **Create new scene:**
   - File > New Scene
   - Choose "Basic (Built-in)" or "Basic (URP)" depending on render pipeline
   - Save as `Assets/Knockout/Scenes/GameplayTest.unity`

2. **Set up environment:**
   - Create ground plane:
     - GameObject > 3D Object > Plane
     - Name it "Ground"
     - Set Transform: Position (0, 0, 0), Scale (5, 1, 5) - creates 50x50 unit arena
   - Create boundary walls (optional for testing):
     - GameObject > 3D Object > Cube, name "Wall_North", position (0, 2.5, 25), scale (50, 5, 1)
     - Duplicate for other sides (Wall_South, Wall_East, Wall_West)

3. **Configure lighting:**
   - Select "Directional Light" (should exist by default)
   - Rename to "MainLight"
   - Set Rotation: (50, -30, 0) for good character lighting
   - Set Intensity: 1.0
   - Add fill light:
     - GameObject > Light > Directional Light
     - Name "FillLight"
     - Rotation: (-20, 120, 0)
     - Intensity: 0.4
     - Color: Slight blue tint (for visual variety)

4. **Set up camera with Cinemachine:**
   - Delete default Main Camera (Cinemachine will create one)
   - GameObject > Cinemachine > Virtual Camera
   - Name it "GameplayCamera"
   - In Inspector (CinemachineVirtualCamera component):
     - Set Priority: 10
     - Body: Framing Transposer
       - Camera Distance: 8
       - Camera Side: 0
       - Screen X: 0.5, Screen Y: 0.4 (character slightly below center)
     - Aim: Composer
       - Tracked Object Offset Y: 1.5 (aim at character's chest/head area)
     - Leave "Follow" and "Look At" empty for now (will assign character later)

5. **Create spawn points:**
   - Create empty GameObject, name "PlayerSpawnPoint"
   - Position: (-5, 0, 0)
   - Create empty GameObject, name "AISpawnPoint"
   - Position: (5, 0, 0)
   - Parent both under empty GameObject named "SpawnPoints"

6. **Organize hierarchy:**
   - Create empty GameObjects for organization:
     - "Environment" - parent Ground and Walls
     - "Lighting" - parent MainLight and FillLight
     - "Cameras" - parent GameplayCamera
     - "SpawnPoints" - parent PlayerSpawnPoint and AISpawnPoint

7. **Configure scene settings:**
   - Window > Rendering > Lighting
   - Environment tab:
     - Skybox Material: Default or URP skybox
   - Baked Lighting (if desired for performance):
     - Set Lightmapping Settings for static objects
     - For now, keep everything realtime (faster iteration)

8. **Save scene:**
   - File > Save (Ctrl+S)
   - Verify scene is in `Assets/Knockout/Scenes/GameplayTest.unity`

**Verification Checklist:**
- [ ] Scene saves without errors
- [ ] Ground plane visible and positioned at origin
- [ ] Lighting looks balanced (not too dark, not blown out)
- [ ] Cinemachine virtual camera exists with correct settings
- [ ] Spawn points positioned correctly facing each other
- [ ] Scene hierarchy organized with parent GameObjects
- [ ] Scene opens without console errors

**Testing Instructions:**
Create edit mode test to verify scene structure:

```csharp
// File: Assets/Knockout/Tests/EditMode/Scenes/GameplayTestSceneTests.cs
[Test]
public void GameplayTestScene_Exists()
{
    // Arrange
    string scenePath = "Assets/Knockout/Scenes/GameplayTest.unity";

    // Act
    SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

    // Assert
    Assert.IsNotNull(scene, "GameplayTest scene should exist");
}

[UnityTest]
public IEnumerator GameplayTestScene_ContainsRequiredObjects()
{
    // Arrange
    string scenePath = "Assets/Knockout/Scenes/GameplayTest.unity";

    // Act
    EditorSceneManager.OpenScene(scenePath);
    yield return null;

    // Assert
    Assert.IsNotNull(GameObject.Find("Ground"), "Scene should contain Ground");
    Assert.IsNotNull(GameObject.Find("PlayerSpawnPoint"), "Scene should contain PlayerSpawnPoint");
    Assert.IsNotNull(GameObject.Find("AISpawnPoint"), "Scene should contain AISpawnPoint");
    Assert.IsNotNull(GameObject.FindObjectOfType<CinemachineVirtualCamera>(), "Scene should contain Cinemachine camera");
}
```

Run test in PlayMode Test Runner (scene loading requires play mode).

**Commit Message Template:**
```
feat(scenes): create gameplay test scene

- Add GameplayTest.unity scene with ground plane and walls
- Configure lighting with main and fill lights
- Set up Cinemachine virtual camera for gameplay view
- Create player and AI spawn points
- Organize scene hierarchy with parent GameObjects
- Add play mode test to verify scene structure
```

**Estimated Tokens:** ~12,000

---

### Task 6: Create Base Character Prefab

**Goal:** Create a prefab for the base character with model, Animator component, and placeholder components for future implementation.

**Files to Create:**
- `Assets/Knockout/Prefabs/Characters/PlayerCharacter.prefab`
- `Assets/Knockout/Prefabs/Characters/AICharacter.prefab`

**Prerequisites:**
- Task 2 complete (character model imported)
- Task 5 complete (test scene exists)

**Implementation Steps:**

1. **Create character GameObject in scene:**
   - Open `GameplayTest.unity` scene
   - Drag `BaseCharacter.fbx` from `Models/Characters/BaseCharacter/` into scene
   - Rename GameObject to "PlayerCharacter"
   - Position at PlayerSpawnPoint: (-5, 0, 0)
   - Rotation: (0, 90, 0) - facing right toward AI spawn

2. **Add required components:**
   - Select PlayerCharacter GameObject
   - Add component: Rigidbody
     - Mass: 70 (average human mass in kg)
     - Drag: 0
     - Angular Drag: 0.05
     - Use Gravity: True
     - Is Kinematic: True (for now, animated character, not physics-driven)
     - Constraints: Freeze Rotation X, Y, Z (prevent physics rotation)
   - Add component: Capsule Collider
     - Center: (0, 1, 0)
     - Radius: 0.3
     - Height: 1.8
     - Is Trigger: False (solid collider for character)

3. **Verify Animator component:**
   - Animator component should already exist from imported model
   - If not, add Animator component
   - Leave "Controller" empty (will assign in Phase 2)
   - Set Avatar to the BaseCharacter avatar (should be auto-assigned)
   - Apply Root Motion: Check (we'll selectively apply in code)
   - Update Mode: Normal
   - Culling Mode: Always Animate (for testing, can optimize later)

4. **Add child GameObject for hitboxes (future):**
   - Create empty child GameObject, name "Hitboxes"
   - This will contain hand colliders for punches (Phase 3)
   - Leave empty for now

5. **Add child GameObject for hurtboxes (future):**
   - Create empty child GameObject, name "Hurtboxes"
   - This will contain head and body colliders for taking hits (Phase 3)
   - Leave empty for now

6. **Create prefab:**
   - Drag PlayerCharacter from Hierarchy to `Assets/Knockout/Prefabs/Characters/`
   - Unity creates prefab asset
   - Delete PlayerCharacter from scene (we'll instantiate via prefab)

7. **Create AI character prefab variant:**
   - Right-click PlayerCharacter.prefab in Project window
   - Create > Prefab Variant
   - Name it "AICharacter.prefab"
   - Open prefab variant
   - Change color/material slightly to distinguish AI (temporary visual difference)
     - Select the mesh object (child of root)
     - In Mesh Renderer, change material color to different tint (e.g., red tint for AI)

8. **Test prefab instantiation:**
   - Open GameplayTest scene
   - Drag PlayerCharacter prefab to PlayerSpawnPoint position
   - Drag AICharacter prefab to AISpawnPoint position
   - Enter Play mode
   - Verify both characters appear in scene, have correct positions
   - Exit Play mode, delete instances

**Verification Checklist:**
- [ ] PlayerCharacter.prefab exists in Prefabs/Characters/
- [ ] AICharacter.prefab exists in Prefabs/Characters/
- [ ] Both prefabs have Animator component with valid Avatar
- [ ] Both prefabs have Rigidbody (kinematic) and Capsule Collider
- [ ] Hitboxes and Hurtboxes child GameObjects exist (empty)
- [ ] Prefabs instantiate correctly in scene
- [ ] No console errors when instantiating or entering play mode

**Testing Instructions:**
Create play mode test to verify prefab structure:

```csharp
// File: Assets/Knockout/Tests/PlayMode/Characters/CharacterPrefabTests.cs
[UnityTest]
public IEnumerator PlayerCharacterPrefab_HasRequiredComponents()
{
    // Arrange
    string prefabPath = "Assets/Knockout/Prefabs/Characters/PlayerCharacter.prefab";
    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

    // Act
    GameObject instance = Object.Instantiate(prefab);
    yield return null;

    // Assert
    Assert.IsNotNull(instance.GetComponent<Animator>(), "Should have Animator");
    Assert.IsNotNull(instance.GetComponent<Rigidbody>(), "Should have Rigidbody");
    Assert.IsNotNull(instance.GetComponent<CapsuleCollider>(), "Should have CapsuleCollider");
    Assert.IsTrue(instance.GetComponent<Animator>().avatar.isHuman, "Should have Humanoid avatar");

    // Cleanup
    Object.Destroy(instance);
}

[UnityTest]
public IEnumerator AICharacterPrefab_HasRequiredComponents()
{
    // Arrange
    string prefabPath = "Assets/Knockout/Prefabs/Characters/AICharacter.prefab";
    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

    // Act
    GameObject instance = Object.Instantiate(prefab);
    yield return null;

    // Assert
    Assert.IsNotNull(instance.GetComponent<Animator>(), "Should have Animator");
    Assert.IsNotNull(instance.GetComponent<Rigidbody>(), "Should have Rigidbody");
    Assert.IsNotNull(instance.GetComponent<CapsuleCollider>(), "Should have CapsuleCollider");

    // Cleanup
    Object.Destroy(instance);
}
```

Run tests in PlayMode Test Runner.

**Commit Message Template:**
```
feat(prefabs): create player and AI character prefabs

- Create PlayerCharacter prefab with model, Animator, Rigidbody, Capsule Collider
- Add Hitboxes and Hurtboxes child GameObjects for future use
- Create AICharacter prefab variant with visual distinction
- Add play mode tests to verify prefab components
```

**Estimated Tokens:** ~15,000

---

### Task 7: Create Character Data ScriptableObjects

**Goal:** Create ScriptableObject classes for character stats and attack data, and create initial data assets for the base character.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Data/CharacterStats.cs` (ScriptableObject class)
- `Assets/Knockout/Scripts/Characters/Data/AttackData.cs` (ScriptableObject class)
- `Assets/Knockout/Scripts/Characters/Data/BaseCharacterStats.asset` (data instance)
- `Assets/Knockout/Scripts/Characters/Data/AttackData_Jab.asset` (data instance)
- `Assets/Knockout/Scripts/Characters/Data/AttackData_Hook.asset` (data instance)
- `Assets/Knockout/Scripts/Characters/Data/AttackData_Uppercut.asset` (data instance)

**Prerequisites:**
- Task 1 complete (folder structure exists)

**Implementation Steps:**

1. **Create CharacterStats ScriptableObject:**
   - Create new C# script: `Assets/Knockout/Scripts/Characters/Data/CharacterStats.cs`
   - Define ScriptableObject with character attributes:

   ```csharp
   using UnityEngine;

   namespace Knockout.Characters.Data
   {
       /// <summary>
       /// Defines character attributes like health, speed, and damage modifiers.
       /// </summary>
       [CreateAssetMenu(fileName = "CharacterStats", menuName = "Knockout/Character Stats")]
       public class CharacterStats : ScriptableObject
       {
           [Header("Health")]
           [SerializeField] [Tooltip("Maximum health points")]
           private float maxHealth = 100f;

           [Header("Movement")]
           [SerializeField] [Range(1f, 10f)] [Tooltip("Movement speed multiplier")]
           private float moveSpeed = 5f;

           [SerializeField] [Range(1f, 20f)] [Tooltip("Rotation speed in degrees per second")]
           private float rotationSpeed = 10f;

           [Header("Combat")]
           [SerializeField] [Range(0.5f, 2f)] [Tooltip("Outgoing damage multiplier")]
           private float damageMultiplier = 1f;

           [SerializeField] [Range(0.5f, 2f)] [Tooltip("Incoming damage multiplier (lower = more resistant)")]
           private float damageTakenMultiplier = 1f;

           // Public properties (read-only)
           public float MaxHealth => maxHealth;
           public float MoveSpeed => moveSpeed;
           public float RotationSpeed => rotationSpeed;
           public float DamageMultiplier => damageMultiplier;
           public float DamageTakenMultiplier => damageTakenMultiplier;
       }
   }
   ```

2. **Create AttackData ScriptableObject:**
   - Create new C# script: `Assets/Knockout/Scripts/Characters/Data/AttackData.cs`
   - Define ScriptableObject with attack properties:

   ```csharp
   using UnityEngine;

   namespace Knockout.Characters.Data
   {
       /// <summary>
       /// Defines properties of an attack: damage, knockback, frame data, animation.
       /// </summary>
       [CreateAssetMenu(fileName = "AttackData", menuName = "Knockout/Attack Data")]
       public class AttackData : ScriptableObject
       {
           [Header("Identity")]
           [SerializeField] [Tooltip("Attack name (e.g., Jab, Hook)")]
           private string attackName = "Attack";

           [Header("Damage")]
           [SerializeField] [Tooltip("Base damage amount")]
           private float damage = 10f;

           [SerializeField] [Tooltip("Knockback force applied on hit")]
           private float knockback = 1f;

           [Header("Frame Data (at 60fps)")]
           [SerializeField] [Tooltip("Frames before hitbox activates")]
           private int startupFrames = 6;

           [SerializeField] [Tooltip("Frames where hitbox is active")]
           private int activeFrames = 3;

           [SerializeField] [Tooltip("Frames after hitbox deactivates")]
           private int recoveryFrames = 6;

           [Header("Animation")]
           [SerializeField] [Tooltip("Animator parameter name to trigger this attack")]
           private string animationTrigger = "AttackTrigger";

           [SerializeField] [Tooltip("Attack type index (0=jab, 1=hook, 2=uppercut)")]
           private int attackTypeIndex = 0;

           // Public properties
           public string AttackName => attackName;
           public float Damage => damage;
           public float Knockback => knockback;
           public int StartupFrames => startupFrames;
           public int ActiveFrames => activeFrames;
           public int RecoveryFrames => recoveryFrames;
           public int TotalFrames => startupFrames + activeFrames + recoveryFrames;
           public string AnimationTrigger => animationTrigger;
           public int AttackTypeIndex => attackTypeIndex;
       }
   }
   ```

3. **Create BaseCharacterStats asset:**
   - In Project window, navigate to `Assets/Knockout/Scripts/Characters/Data/`
   - Right-click > Create > Knockout > Character Stats
   - Name it "BaseCharacterStats"
   - In Inspector, set values:
     - Max Health: 100
     - Move Speed: 5
     - Rotation Speed: 10
     - Damage Multiplier: 1.0
     - Damage Taken Multiplier: 1.0

4. **Create AttackData assets:**
   - Right-click > Create > Knockout > Attack Data (3 times)
   - Name them: "AttackData_Jab", "AttackData_Hook", "AttackData_Uppercut"

   - **AttackData_Jab:**
     - Attack Name: "Jab"
     - Damage: 10
     - Knockback: 0.5
     - Startup Frames: 6
     - Active Frames: 3
     - Recovery Frames: 6
     - Animation Trigger: "AttackTrigger"
     - Attack Type Index: 0

   - **AttackData_Hook:**
     - Attack Name: "Hook"
     - Damage: 18
     - Knockback: 1.5
     - Startup Frames: 10
     - Active Frames: 4
     - Recovery Frames: 12
     - Animation Trigger: "AttackTrigger"
     - Attack Type Index: 1

   - **AttackData_Uppercut:**
     - Attack Name: "Uppercut"
     - Damage: 30
     - Knockback: 3.0
     - Startup Frames: 15
     - Active Frames: 5
     - Recovery Frames: 18
     - Animation Trigger: "AttackTrigger"
     - Attack Type Index: 2

5. **Verify assets:**
   - All ScriptableObject assets should be visible in Project window
   - Selecting each should show properties in Inspector
   - No compilation errors

**Verification Checklist:**
- [ ] CharacterStats.cs and AttackData.cs scripts exist and compile
- [ ] CreateAssetMenu attributes work (menu items appear)
- [ ] BaseCharacterStats.asset exists with correct values
- [ ] All three AttackData assets exist with correct values
- [ ] Assets can be selected and inspected
- [ ] No console errors

**Testing Instructions:**
Create edit mode test to verify ScriptableObject data:

```csharp
// File: Assets/Knockout/Tests/EditMode/Characters/CharacterDataTests.cs
[Test]
public void BaseCharacterStats_Asset_Exists()
{
    // Arrange
    string assetPath = "Assets/Knockout/Scripts/Characters/Data/BaseCharacterStats.asset";

    // Act
    CharacterStats stats = AssetDatabase.LoadAssetAtPath<CharacterStats>(assetPath);

    // Assert
    Assert.IsNotNull(stats, "BaseCharacterStats asset should exist");
    Assert.AreEqual(100f, stats.MaxHealth, "MaxHealth should be 100");
    Assert.AreEqual(5f, stats.MoveSpeed, "MoveSpeed should be 5");
}

[Test]
public void AttackData_Jab_Asset_Exists()
{
    // Arrange
    string assetPath = "Assets/Knockout/Scripts/Characters/Data/AttackData_Jab.asset";

    // Act
    AttackData attack = AssetDatabase.LoadAssetAtPath<AttackData>(assetPath);

    // Assert
    Assert.IsNotNull(attack, "AttackData_Jab asset should exist");
    Assert.AreEqual("Jab", attack.AttackName);
    Assert.AreEqual(10f, attack.Damage);
    Assert.AreEqual(15, attack.TotalFrames, "Total frames should be 6+3+6=15");
}
```

Run tests in EditMode Test Runner.

**Commit Message Template:**
```
feat(data): create ScriptableObjects for character stats and attacks

- Add CharacterStats ScriptableObject class
- Add AttackData ScriptableObject class
- Create BaseCharacterStats asset with default values
- Create AttackData assets for Jab, Hook, Uppercut
- Add edit mode tests to verify data assets
```

**Estimated Tokens:** ~18,000

---

### Task 8: Create Character Controller Foundation Script

**Goal:** Create the main CharacterController component that will coordinate all character subsystems (animation, input, movement, combat). This is a minimal foundation script with no functionality yet.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/CharacterController.cs`

**Prerequisites:**
- Task 7 complete (CharacterStats ScriptableObject exists)

**Implementation Steps:**

1. **Create CharacterController.cs:**
   - Create new C# script: `Assets/Knockout/Scripts/Characters/CharacterController.cs`
   - Implement basic coordinator structure:

   ```csharp
   using UnityEngine;
   using Knockout.Characters.Data;

   namespace Knockout.Characters
   {
       /// <summary>
       /// Main coordinator component for character behavior.
       /// Manages references to character subsystems and coordinates their interactions.
       /// </summary>
       [RequireComponent(typeof(Animator))]
       [RequireComponent(typeof(Rigidbody))]
       public class CharacterController : MonoBehaviour
       {
           [Header("Character Data")]
           [SerializeField] [Tooltip("Character stats (health, speed, damage)")]
           private CharacterStats characterStats;

           [Header("Component References")]
           // These will be populated in Phase 2-4 as components are created
           // private CharacterAnimator _characterAnimator;
           // private CharacterInput _characterInput;
           // private CharacterMovement _characterMovement;
           // private CharacterCombat _characterCombat;
           // private CharacterHealth _characterHealth;

           // Cached Unity components
           private Animator _animator;
           private Rigidbody _rigidbody;

           // Public properties
           public CharacterStats Stats => characterStats;
           public Animator Animator => _animator;
           public Rigidbody Rigidbody => _rigidbody;

           private void Awake()
           {
               CacheComponents();
               ValidateSetup();
           }

           private void CacheComponents()
           {
               _animator = GetComponent<Animator>();
               _rigidbody = GetComponent<Rigidbody>();
           }

           private void ValidateSetup()
           {
               if (characterStats == null)
               {
                   Debug.LogError($"[CharacterController] {gameObject.name} is missing CharacterStats reference!", this);
               }

               if (_animator == null)
               {
                   Debug.LogError($"[CharacterController] {gameObject.name} is missing Animator component!", this);
               }

               if (_animator != null && !_animator.avatar.isHuman)
               {
                   Debug.LogError($"[CharacterController] {gameObject.name} Animator must use Humanoid avatar!", this);
               }

               if (_rigidbody == null)
               {
                   Debug.LogError($"[CharacterController] {gameObject.name} is missing Rigidbody component!", this);
               }
           }

           private void Start()
           {
               // Future: Initialize character subsystems
               // _characterAnimator?.Initialize(this);
               // _characterInput?.Initialize(this);
               // etc.
           }

           private void OnValidate()
           {
               // Editor-time validation
               if (characterStats == null)
               {
                   Debug.LogWarning($"[CharacterController] {gameObject.name} should have CharacterStats assigned.");
               }
           }
       }
   }
   ```

2. **Add CharacterController to prefabs:**
   - Open PlayerCharacter.prefab
   - Add CharacterController component
   - Assign BaseCharacterStats to "Character Stats" field
   - Save prefab
   - Repeat for AICharacter.prefab

3. **Test in scene:**
   - Open GameplayTest scene
   - Instantiate PlayerCharacter prefab at PlayerSpawnPoint
   - Enter Play mode
   - Check Console for any errors (should be none)
   - Verify CharacterController Awake() runs without errors
   - Exit Play mode, delete instance

**Verification Checklist:**
- [ ] CharacterController.cs exists and compiles
- [ ] Script has RequireComponent attributes for Animator and Rigidbody
- [ ] ValidateSetup() method checks for required components and data
- [ ] OnValidate() provides editor-time warnings
- [ ] Component can be added to character prefabs
- [ ] No console errors when entering Play mode with component

**Testing Instructions:**
Create play mode test to verify CharacterController:

```csharp
// File: Assets/Knockout/Tests/PlayMode/Characters/CharacterControllerTests.cs
[UnityTest]
public IEnumerator CharacterController_Initializes_WithoutErrors()
{
    // Arrange
    GameObject characterGO = new GameObject("TestCharacter");
    characterGO.AddComponent<Animator>();
    Rigidbody rb = characterGO.AddComponent<Rigidbody>();
    rb.isKinematic = true;

    CharacterStats stats = ScriptableObject.CreateInstance<CharacterStats>();
    CharacterController controller = characterGO.AddComponent<CharacterController>();

    // Use reflection to set private field (since no public setter)
    var field = typeof(CharacterController).GetField("characterStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    field.SetValue(controller, stats);

    // Act
    yield return null; // Wait for Awake/Start

    // Assert
    Assert.IsNotNull(controller.Stats, "Stats should be assigned");
    Assert.IsNotNull(controller.Animator, "Animator should be cached");
    Assert.IsNotNull(controller.Rigidbody, "Rigidbody should be cached");

    // Cleanup
    Object.Destroy(characterGO);
    Object.Destroy(stats);
}
```

Run test in PlayMode Test Runner.

**Commit Message Template:**
```
feat(characters): create CharacterController foundation script

- Add CharacterController.cs as main coordinator component
- Cache references to Animator and Rigidbody
- Implement component validation in Awake and OnValidate
- Add RequireComponent attributes for dependencies
- Add CharacterController to player and AI prefabs
- Add play mode test to verify initialization
```

**Estimated Tokens:** ~12,000

---

## Phase Verification

Before proceeding to Phase 2, verify the following:

### All Tasks Complete
- [ ] Task 1: Folder structure created and matches Phase 0
- [ ] Task 2: Character model imported with Humanoid rig
- [ ] Task 3: All animation clips imported and configured
- [ ] Task 4: Input System set up with action maps
- [ ] Task 5: Test scene created with environment and camera
- [ ] Task 6: Player and AI character prefabs created
- [ ] Task 7: CharacterStats and AttackData ScriptableObjects created
- [ ] Task 8: CharacterController foundation script created

### Integration Points
- [ ] Character prefabs can be instantiated in test scene
- [ ] Animator component has valid Humanoid avatar
- [ ] Input Actions asset generates C# class without errors
- [ ] All ScriptableObject data assets load and display correctly

### Tests Passing
- [ ] All EditMode tests pass (run via Test Runner)
- [ ] All PlayMode tests pass (run via Test Runner)
- [ ] No console errors when opening GameplayTest scene
- [ ] No console errors when entering Play mode with character prefabs

### Git Status
- [ ] All new files committed with appropriate commit messages
- [ ] Commit history follows conventional commits format
- [ ] Branch is clean (no uncommitted changes)

### Known Limitations
- Character prefabs have no scripted behavior yet (Phase 2)
- Animator has no controller assigned (Phase 2)
- Input System not connected to character (Phase 2)
- No hit detection or combat mechanics (Phase 3)

---

**Phase 1 Complete!** Proceed to [Phase 2: Animation System & State Machine](Phase-2.md).
