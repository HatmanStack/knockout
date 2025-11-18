# Phase 0.5: Asset Migration & Organization

## Phase Goal

Reorganize existing assets into the planned folder structure, retarget boxing animations to your character model, archive legacy code, and prepare the project for implementing the component-based architecture from Phase 1. This phase bridges the gap between the existing scattered assets and the clean structure needed for the plan.

**Success Criteria:**
- All assets organized under `Assets/Knockout/` following Phase 0 structure
- Boxing animations retargeted and working with chosen character model
- Legacy scripts archived (not deleted) for reference
- Character prefabs created with proper rig configuration
- Project ready for Phase 1 implementation
- All verification tests passing

**Estimated Tokens:** ~30,000

---

## Prerequisites

### Must Complete First
- Read Phase 0 thoroughly
- Unity project successfully opened (✓ Complete)
- Boxing animations imported (✓ Complete)
- Character models imported (✓ Complete)

### Assets Available
- ✓ Boxing Basic Mocap Pack (24 animation clips)
- ✓ Player.fbx character model
- ✓ Elizabeth Warren character models (2 variations)
- ✓ Existing locomotion animations (Idle, Run variations, Jump, Header)
- ✓ Existing scenes and environment assets

### Decision Required
- **Choose your primary character model** (recommended before starting):
  - Option A: Player.fbx (simpler, already used in existing scenes)
  - Option B: Elizabeth Warren character (political theme, more detailed)

---

## Tasks

### Task 1: Choose and Configure Character Model

**Goal:** Select the primary character model and verify rig compatibility with boxing animations.

**Files to Examine:**
- `Assets/Asset Unlock - 3D Prototyping/Character/Models/Player.fbx`
- `Assets/Elizabeth Warren caricature/Mecanim Elizabeth Warren rigged 1.fbx`
- `Assets/AAAnimators_Boxing_basic1.1/Models/Robot_Tpose.fbx` (reference)

**Prerequisites:**
- None

**Implementation Steps:**

1. **Inspect character models in Unity:**
   - Select `Player.fbx` in Project window
   - In Inspector, check "Rig" tab → verify "Animation Type" is "Humanoid"
   - Click "Configure..." to open Avatar configuration
   - Verify all bones are mapped correctly (green checkmarks)
   - Repeat for Elizabeth Warren model

2. **Test animation compatibility:**
   - Drag `Player.fbx` into the Scene view
   - In Inspector, expand the model and select the child object with the rig
   - In the Animation window (Window → Animation → Animation)
   - Drag `Left_Jab.fbx` onto the character
   - Play the animation - does it work without errors?
   - Check for: weird poses, missing limbs, incorrect rotations

3. **Choose your character:**
   - Based on compatibility testing, pick one as primary
   - Note: Both should work (humanoid rigs), but one may need less tweaking
   - **Recommendation:** Start with Player.fbx (already integrated in scenes)

4. **Document your choice:**
   - Create a note in this file or Phase 1 about which character you chose
   - If animations need retargeting, note which ones

**Verification Checklist:**
- [ ] Character model has Humanoid rig configured
- [ ] Avatar configuration shows all bones mapped correctly
- [ ] At least one boxing animation plays on the character without errors
- [ ] Character chosen and documented

**Testing Instructions:**
No automated tests. Visual verification in Unity Scene view.

**Commit Message Template:**
```
feat(characters): configure character rig for boxing animations

- Verified [CharacterName] has humanoid rig
- Tested boxing animation compatibility
- Selected [CharacterName] as primary character
- Avatar configuration validated

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated Tokens:** ~8,000

---

### Task 2: Create Knockout Folder Structure

**Goal:** Create the organized folder structure from Phase 0, preparing to receive migrated assets.

**Files to Create:**
- Folder structure under `Assets/Knockout/` (see Phase 0 Project Structure)

**Prerequisites:**
- Task 1 complete (character chosen)

**Implementation Steps:**

1. **Create root Knockout folder:**
   - In Unity Project window, right-click on Assets
   - Create → Folder → name it "Knockout"

2. **Create primary subfolders:**
   - Inside Knockout, create: `Animations`, `Materials`, `Models`, `Prefabs`, `Scenes`, `Scripts`, `Tests`

3. **Create animation substructure:**
   - `Knockout/Animations/Characters/BaseCharacter/`
   - Inside BaseCharacter: `AnimationClips`, `AnimatorMasks`
   - Leave placeholder for `AnimatorController.controller` (created in Phase 2)

4. **Create materials subfolders:**
   - `Knockout/Materials/Characters/`

5. **Create models subfolders:**
   - `Knockout/Models/Characters/BaseCharacter/`
   - Inside: `Textures/`

6. **Create prefabs subfolders:**
   - `Knockout/Prefabs/Characters/`
   - `Knockout/Prefabs/Systems/`

7. **Create scripts subfolders (empty for now):**
   - `Knockout/Scripts/Characters/` → inside: `Components/`, `Data/`
   - `Knockout/Scripts/Combat/` → inside: `HitDetection/`, `States/`
   - `Knockout/Scripts/AI/` → inside: `States/`
   - `Knockout/Scripts/Input/`
   - `Knockout/Scripts/Utilities/` → inside: `Extensions/`, `Helpers/`

8. **Create tests subfolders:**
   - `Knockout/Tests/EditMode/` → inside: `Combat/`, `Characters/`
   - `Knockout/Tests/PlayMode/` → inside: `Combat/`, `Characters/`

9. **Create Legacy folder for old code:**
   - `Assets/Legacy/` (outside Knockout, for archived scripts)

**Verification Checklist:**
- [ ] All folders created and visible in Unity Project window
- [ ] Folder structure matches Phase 0 exactly
- [ ] No Unity console errors
- [ ] Legacy folder created for old scripts

**Testing Instructions:**
Visual verification in Unity Project window. Compare against Phase 0 structure.

**Commit Message Template:**
```
feat(structure): create Knockout folder hierarchy for organized assets

- Created Knockout/ root with all subfolders
- Matches Phase 0 architecture structure
- Added Legacy/ for old scripts
- Ready for asset migration

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated Tokens:** ~5,000

---

### Task 3: Migrate Boxing Animations

**Goal:** Move boxing animations into the organized structure and prepare them for use with your character.

**Files to Move:**
- From: `Assets/AAAnimators_Boxing_basic1.1/Animations/*.fbx`
- To: `Assets/Knockout/Animations/Characters/BaseCharacter/AnimationClips/`

**Prerequisites:**
- Task 2 complete (folder structure created)

**Implementation Steps:**

1. **Copy (don't move) animation files:**
   - Select all `.fbx` files in `AAAnimators_Boxing_basic1.1/Animations/`
   - Right-click → Copy
   - Navigate to `Knockout/Animations/Characters/BaseCharacter/AnimationClips/`
   - Right-click → Paste
   - **Why copy?** Keep originals as backup until verified working

2. **Rename animations for consistency:**
   - Unity convention: PascalCase without underscores
   - Examples:
     - `Left_Jab.fbx` → `LeftJab.fbx`
     - `left_uppercut.fbx` → `LeftUppercut.fbx`
     - `hit_by_hook_V1.fbx` → `HitByHook.fbx`
   - **Note:** Renaming is optional but improves consistency

3. **Configure animation import settings:**
   - Select a boxing animation (e.g., LeftJab.fbx)
   - In Inspector → Rig tab:
     - Animation Type: "Humanoid"
     - Avatar Definition: "Copy from Other Avatar"
     - Source: Select your chosen character's Avatar
   - Click Apply
   - **Repeat for all animations** (or select multiple and apply in batch)

4. **Set animation loop settings:**
   - Select animations in groups by type:
   - **Loop animations:** Idle, Idle_tired, Block, Win
     - Inspector → Animation tab → Loop Time: ✓ checked
   - **Non-loop animations:** All attacks, dodges, hit reactions, knockouts
     - Loop Time: ✗ unchecked
   - Click Apply for each group

5. **Test animation preview:**
   - Select LeftJab animation
   - In Inspector → Animation tab → Preview at bottom
   - Click Play button - verify animation plays correctly
   - Check a few more animations to confirm they work

**Verification Checklist:**
- [ ] All 24 boxing animations copied to Knockout/Animations/.../AnimationClips/
- [ ] Animations configured with Humanoid rig
- [ ] Avatar source set to your chosen character
- [ ] Loop settings configured correctly (idle/block loop, attacks don't)
- [ ] Preview shows animations working correctly
- [ ] No console errors

**Testing Instructions:**
Preview animations in Inspector. Verify smooth playback without errors.

**Commit Message Template:**
```
feat(animations): migrate boxing animations to Knockout structure

- Copied 24 boxing animations to AnimationClips/
- Configured humanoid rig for all animations
- Set loop settings (idle/block loop, attacks single-play)
- Verified animations preview correctly
- Retargeted to [CharacterName] avatar

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated Tokens:** ~8,000

---

### Task 4: Migrate Character Model and Materials

**Goal:** Move your chosen character model into the organized structure with all associated materials and textures.

**Files to Move:**
- Character model (Player.fbx or Elizabeth Warren model)
- Associated materials and textures

**Prerequisites:**
- Task 3 complete (animations migrated)
- Character chosen (Task 1)

**Implementation Steps:**

1. **Copy character model:**
   - **If using Player.fbx:**
     - Copy from: `Assets/Asset Unlock - 3D Prototyping/Character/Models/Player.fbx`
     - To: `Assets/Knockout/Models/Characters/BaseCharacter/Player.fbx`
   - **If using Elizabeth Warren:**
     - Copy: `Mecanim Elizabeth Warren rigged 1.fbx`
     - To: `Assets/Knockout/Models/Characters/BaseCharacter/ElizabethWarren.fbx`

2. **Copy textures:**
   - Find associated texture files (should be near the model)
   - Copy to: `Assets/Knockout/Models/Characters/BaseCharacter/Textures/`

3. **Copy or recreate materials:**
   - **If using Player.fbx:**
     - Copy `Home.mat` from Asset Unlock pack
     - To: `Assets/Knockout/Materials/Characters/`
   - **If using Elizabeth Warren:**
     - Copy materials from `Elizabeth Warren caricature/Materials/`
     - To: `Assets/Knockout/Materials/Characters/`

4. **Reconnect material references:**
   - Select the character model in Knockout/Models/
   - In Inspector → Materials tab
   - Verify materials point to the ones in Knockout/Materials/
   - If broken (pink textures), drag correct materials from Knockout/Materials/ onto the model

5. **Verify model integrity:**
   - Drag character from Knockout/Models/ into Scene
   - Check that:
     - Model appears correctly (not pink)
     - Textures applied properly
     - Rig is still configured (Inspector → Rig tab shows Humanoid)
   - Delete from scene after verification

**Verification Checklist:**
- [ ] Character model copied to Knockout/Models/Characters/BaseCharacter/
- [ ] All textures copied to Textures/ subfolder
- [ ] Materials copied to Knockout/Materials/Characters/
- [ ] Material references connected correctly (no pink textures)
- [ ] Model displays correctly when placed in scene
- [ ] Rig still configured as Humanoid

**Testing Instructions:**
Drag model into scene, verify visual appearance, delete after check.

**Commit Message Template:**
```
feat(models): migrate character model and materials to Knockout

- Moved [CharacterName] model to Models/Characters/BaseCharacter/
- Copied all textures to Textures/ subfolder
- Migrated materials with correct references
- Verified model displays correctly in scene
- Humanoid rig configuration preserved

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated Tokens:** ~6,000

---

### Task 5: Archive Legacy Scripts

**Goal:** Move existing scripts to Legacy folder for reference, clearing the way for new component-based architecture.

**Files to Move:**
- All scripts in `Assets/Scripts/`
- Scripts at `Assets/` root level (NightScript.cs, timecontroller.cs)

**Prerequisites:**
- Task 2 complete (Legacy folder created)

**Implementation Steps:**

1. **Move all existing scripts to Legacy:**
   - Select all files in `Assets/Scripts/`
   - Right-click → Cut (or Ctrl+X)
   - Navigate to `Assets/Legacy/`
   - Right-click → Paste
   - Unity will move the scripts and update references

2. **Move root-level scripts:**
   - Move `Assets/NightScript.cs` → `Assets/Legacy/`
   - Move `Assets/timecontroller.cs` → `Assets/Legacy/`

3. **Keep the Scripts folder for new code:**
   - Delete the now-empty `Assets/Scripts/` folder
   - The new scripts will go in `Assets/Knockout/Scripts/` instead

4. **Note broken references:**
   - Unity Console may show warnings about missing scripts on GameObjects
   - This is expected - those scripts will be replaced in Phase 1-2
   - **Don't fix these yet** - we'll remove old components when adding new ones

5. **Document what was archived:**
   - Create a text file: `Assets/Legacy/README.txt`
   - Contents:
     ```
     Legacy Scripts Archive

     These scripts are from the original project before the component-based
     refactor. They are kept for reference only.

     Key scripts:
     - PlayerController.cs: Old input handling (uses Input.GetAxis)
     - BossLookDirection.cs: Camera/look logic
     - RotateCamera.cs: Camera controls

     Do not modify these. New implementations in Assets/Knockout/Scripts/
     ```

**Verification Checklist:**
- [ ] All scripts moved to Assets/Legacy/
- [ ] Assets/Scripts/ folder removed (old location)
- [ ] README.txt created in Legacy folder
- [ ] Console shows expected warnings about missing scripts (normal)
- [ ] No compile errors (just warnings about missing components)

**Testing Instructions:**
Check Unity Console - should have warnings (not errors) about missing script references.

**Commit Message Template:**
```
refactor(scripts): archive legacy scripts for reference

- Moved all existing scripts to Assets/Legacy/
- Created README documenting legacy code
- Cleared way for new component-based architecture
- Expected warnings about missing script components

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated Tokens:** ~3,000

---

## Phase Completion

### Final Verification

Before moving to Phase 1, verify:

- [ ] **Folder Structure:** Assets/Knockout/ contains all planned subfolders
- [ ] **Animations:** 24 boxing animations in Knockout/Animations/.../AnimationClips/
- [ ] **Character Model:** In Knockout/Models/Characters/BaseCharacter/ with materials
- [ ] **Legacy Code:** All old scripts archived in Assets/Legacy/
- [ ] **No Errors:** Unity Console shows warnings (expected) but no compile errors
- [ ] **Character Chosen:** Documented which model you're using
- [ ] **Rig Verified:** Character has working Humanoid rig with animations

### What You Should Have Now

```
Assets/
├── Knockout/                          # New organized structure
│   ├── Animations/
│   │   └── Characters/BaseCharacter/
│   │       └── AnimationClips/        # 24 boxing animations
│   ├── Materials/
│   │   └── Characters/                # Character materials
│   ├── Models/
│   │   └── Characters/BaseCharacter/
│   │       ├── [YourCharacter].fbx
│   │       └── Textures/
│   ├── Prefabs/
│   │   ├── Characters/                # Empty (Phase 1)
│   │   └── Systems/                   # Empty (Phase 1)
│   ├── Scenes/                        # Empty (Phase 1)
│   ├── Scripts/                       # Empty (Phase 1)
│   └── Tests/                         # Empty (Phase 1)
│
├── Legacy/                            # Archived old code
│   ├── PlayerController.cs
│   ├── [... other old scripts]
│   └── README.txt
│
└── [Original asset folders]           # Keep originals as backup
    ├── AAAnimators_Boxing_basic1.1/
    ├── Asset Unlock - 3D Prototyping/
    └── Elizabeth Warren caricature/
```

### Ready for Phase 1?

If all verification checks pass, you're ready to proceed with:
- **Phase 1:** Project Structure & Asset Integration
  - Create Input System actions
  - Build character prefabs
  - Set up initial animator controller

**Estimated time to complete Phase 0.5:** 1-2 hours (mostly Unity editor work, minimal coding)

---

## Notes & Tips

### Animation Retargeting Issues

If boxing animations don't play correctly on your character:

1. **Check Avatar mapping:**
   - Select animation → Inspector → Rig tab
   - Verify "Source" avatar matches your character

2. **Common issues:**
   - **T-pose appears:** Avatar not configured correctly
   - **Weird rotations:** Bone mapping mismatch
   - **Missing limbs:** Rig hierarchy differs from animation source

3. **Fix:**
   - Re-configure character Avatar (click Configure in Rig tab)
   - Ensure all required bones are mapped (green checkmarks)
   - May need to manually map bones if auto-mapping failed

### Keeping Original Assets

We're copying (not moving) original assets because:
- Backup if something goes wrong
- Can compare old vs new setup
- Reference materials (demo scenes, etc.)

After Phase 2-3 is working, you can delete:
- `AAAnimators_Boxing_basic1.1/` (if all animations working)
- Old `Asset Unlock - 3D Prototyping/` (if not needed)
- Unused character model (if you chose one over the other)

### ChromeOS/Crostini Note

If you encounter file permission issues:
- All work should be in `/home/christophergalliart/today/knockout/`
- Avoid moving files outside Unity (use Unity's interface)
- If files appear corrupted, try reimporting the affected assets

---

**Phase 0.5 Complete!** → Proceed to [Phase 1: Project Structure & Asset Integration](Phase-1.md)
