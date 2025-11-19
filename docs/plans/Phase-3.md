# Phase 3: Package Updates & Compatibility

## Phase Goal

Update all Unity packages to their Unity 6-compatible versions, starting with URP (Universal Render Pipeline), then Input System, Cinemachine, and other packages. This phase resolves rendering, input, and package-related runtime issues introduced during the Unity 6 upgrade.

**Success Criteria:**
- All Unity packages updated to Unity 6-compatible versions
- URP rendering works correctly (scenes render properly)
- Input System functional (character input responds)
- Cinemachine cameras working
- Package-related compilation errors resolved
- Project still compiles with zero errors

**Estimated tokens:** ~28,000

---

## Prerequisites

- Phase 2 complete (project compiles in Unity 6)
- Unity 6 project opens successfully
- Git tag "post-unity6-upgrade" exists
- No compilation errors

---

## Tasks

### Task 1: Update Universal Render Pipeline (URP)

**Goal:** Update URP from 12.1.15 to Unity 6-compatible version

**Files to Modify/Create:**
- `Packages/manifest.json` (Package Manager will modify)
- Various shader and material files (may be automatically upgraded)
- URP asset settings

**Prerequisites:**
- Phase 2 complete

**Implementation Steps:**

1. Open Package Manager (Window > Package Manager)
2. Switch filter to "Unity Registry" (top-left dropdown)
3. Find "Universal RP" in the package list
4. Check current version (should be 12.1.15 from Unity 2021.3)
5. Click "Update to [version]" button (Unity 6 should show 17.x or 18.x)
6. Wait for package download and installation (may take several minutes)
7. Unity will prompt about shader upgrades:
   - Review the shader upgrade dialog
   - Accept shader upgrades (Unity will automatically update shaders)
8. Wait for asset reimport to complete
9. Check Console for errors (some expected - will fix next)
10. Open MainScene and check rendering:
    - Scene view should render properly
    - Game view should render when entering Play mode
11. Review URP asset settings (Settings/ folder):
    - Check Universal Render Pipeline Asset
    - Verify Renderer settings are intact
    - Update any deprecated settings Unity flags

**Verification Checklist:**
- [ ] URP package updated to Unity 6 version (17.x or 18.x)
- [ ] Shader upgrades accepted and completed
- [ ] Assets reimported successfully
- [ ] Scenes render in Scene view
- [ ] Game renders in Play mode (may have visual issues - acceptable)
- [ ] URP asset settings visible and intact

**Testing Instructions:**
- Load MainScene and check if scene renders
- Enter Play mode and verify rendering works
- Check Console for critical rendering errors
- Inspect Game view for major visual issues (minor issues acceptable)

**Commit Message Template:**
```
feat(urp): update Universal Render Pipeline to Unity 6

Updated URP from 12.1.15 to [new version]
Accepted automatic shader upgrades

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~5,000

---

### Task 2: Fix URP-Related Compilation Errors

**Goal:** Resolve any compilation errors introduced by URP package update

**Files to Modify/Create:**
- Scripts that reference URP APIs
- Custom shaders (if any)
- Renderer features (if any)

**Prerequisites:**
- Task 1 complete (URP updated)

**Implementation Steps:**

1. Check Console for compilation errors related to URP
2. Common URP API changes to look for:
   - `RenderPipelineManager` API changes
   - `ScriptableRenderPass` changes
   - Renderer Feature API updates
   - Volume system changes
3. For each URP-related error:
   - Identify the obsolete or changed API
   - Check Unity 6 URP documentation for replacement
   - Update the code with new API
   - Save and verify compilation
4. If custom shaders exist:
   - Check for shader compilation errors
   - Update shader code to Unity 6 URP shader API
   - Test shaders in Scene view
5. If using custom Renderer Features:
   - Update to new Renderer Feature API
   - Test features in rendering pipeline
6. Fix namespace changes related to URP:
   - `UnityEngine.Rendering.Universal` may have changes
   - Update using statements
7. Once all URP errors fixed, recompile project

**Verification Checklist:**
- [ ] Zero compilation errors related to URP
- [ ] URP API calls use Unity 6-compatible APIs
- [ ] Custom shaders compile (if any exist)
- [ ] Renderer features work (if any exist)
- [ ] Scenes render correctly

**Testing Instructions:**
- Verify Console shows zero errors
- Load and render all main scenes
- Check that rendering looks reasonable
- Enter Play mode and verify rendering works

**Commit Message Template:**
```
fix(urp): resolve URP API compatibility issues

Updated URP API calls to Unity 6 equivalents
Fixed rendering-related compilation errors

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,500

---

### Task 3: Update Input System Package

**Goal:** Update Input System from 1.7.0 to Unity 6-compatible version

**Files to Modify/Create:**
- `Packages/manifest.json` (Package Manager will modify)
- Input Action assets (may need regeneration)
- Scripts using Input System

**Prerequisites:**
- Tasks 1-2 complete (URP updated and working)

**Implementation Steps:**

1. Open Package Manager (Window > Package Manager)
2. Find "Input System" in Unity Registry
3. Check current version (should be 1.7.0)
4. Update to latest Unity 6-compatible version (likely 1.8.x or higher)
5. Wait for package update and reimport
6. Check for Input System migration dialogs:
   - Unity may prompt about input action asset upgrades
   - Accept automatic upgrades if prompted
7. Verify Input Actions assets:
   - Open Input Actions window (Window > Analysis > Input Debugger)
   - Check that input actions are still defined
8. Regenerate Input Action C# classes if needed:
   - Select Input Actions asset in Project
   - Check "Generate C# Class" is enabled
   - Click "Apply" to regenerate
9. Check Console for Input System errors
10. Test input functionality in Play mode (character should respond to input)

**Verification Checklist:**
- [ ] Input System updated to Unity 6 version
- [ ] Input Action assets intact and functional
- [ ] C# classes regenerated (if applicable)
- [ ] No Input System compilation errors
- [ ] Character input responsive in Play mode

**Testing Instructions:**
- Enter Play mode
- Test character movement (WASD or configured keys)
- Test attack inputs
- Test dodge/block inputs
- Verify all input responses work
- Check Console for input-related errors

**Commit Message Template:**
```
feat(input): update Input System to Unity 6

Updated Input System from 1.7.0 to [new version]
Regenerated Input Action classes

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,500

---

### Task 4: Fix Input System API Changes

**Goal:** Resolve Input System API changes in character controller and input scripts

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterInput.cs`
- Any scripts referencing Input System APIs
- Input Action callback implementations

**Prerequisites:**
- Task 3 complete (Input System updated)

**Implementation Steps:**

1. Check Console for Input System-related errors
2. Common Input System changes to address:
   - Input Action callback signature changes
   - PlayerInput component API changes
   - Input value reading API changes
   - Device API changes (less relevant for WebGL)
3. Update CharacterInput.cs and related input scripts:
   - Check callback method signatures (OnMove, OnAttack, etc.)
   - Update to match new Input System API
   - Verify input reading methods (ReadValue, triggered, etc.)
   - Update input context handling
4. Test input bindings:
   - Verify keyboard input works
   - Check that input actions fire correctly
   - Ensure input values are read correctly
5. Fix any input-related state machine issues:
   - Combat state machine may depend on input
   - Verify input flows to combat/movement systems
6. Commit changes once input works in Play mode

**Verification Checklist:**
- [ ] Zero Input System compilation errors
- [ ] CharacterInput.cs compiles and works
- [ ] Input callbacks fire correctly
- [ ] Character responds to keyboard input
- [ ] Combat inputs work (attack, block, dodge)
- [ ] Movement inputs work (WASD, directional)

**Testing Instructions:**
- Enter Play mode with MainScene
- Test all input bindings:
  - Movement (WASD or arrows)
  - Light attack
  - Heavy attack
  - Block
  - Dodge
- Verify character responds correctly
- Check Console for input errors

**Commit Message Template:**
```
fix(input): update Input System API calls for Unity 6

Updated input callbacks and value reading APIs
Fixed character input integration

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,000

---

### Task 5: Update Cinemachine Package

**Goal:** Update Cinemachine from 2.10.1 to Unity 6-compatible version (likely 3.x)

**Files to Modify/Create:**
- `Packages/manifest.json` (Package Manager will modify)
- Cinemachine camera components (may need reconfiguration)
- Scripts referencing Cinemachine APIs

**Prerequisites:**
- Tasks 1-4 complete (URP and Input System updated)

**Implementation Steps:**

1. Open Package Manager
2. Find "Cinemachine" in Unity Registry
3. Check current version (2.10.1 from Unity 2021.3)
4. **NOTE:** Unity 6 likely uses Cinemachine 3.x (major version jump)
5. Review Cinemachine upgrade guide if major version change
6. Update Cinemachine package to latest Unity 6 version
7. Unity may show migration dialog for Cinemachine components:
   - Review proposed changes
   - Accept automatic migration
8. Wait for reimport to complete
9. Open scenes with Cinemachine cameras (MainScene likely has virtual cameras)
10. Check that Cinemachine Virtual Cameras still exist in Hierarchy
11. Verify camera components haven't lost settings:
    - Check Follow target (should still reference player)
    - Check Look At target
    - Check Body settings (Framing Transposer or similar)
    - Check Aim settings
12. Test camera in Play mode (camera should follow character)

**Verification Checklist:**
- [ ] Cinemachine updated to Unity 6 version (likely 3.x)
- [ ] Virtual Camera components still exist in scenes
- [ ] Camera settings intact (Follow, Look At, Body, Aim)
- [ ] No Cinemachine compilation errors
- [ ] Camera follows character in Play mode

**Testing Instructions:**
- Load MainScene
- Check Hierarchy for Cinemachine Virtual Camera
- Inspect camera in Inspector (settings should be intact)
- Enter Play mode
- Move character and verify camera follows
- Test camera framing and behavior

**Commit Message Template:**
```
feat(cinemachine): update Cinemachine to Unity 6

Updated Cinemachine from 2.10.1 to [new version]
Migrated virtual camera components

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,500

---

### Task 6: Fix Cinemachine API Changes

**Goal:** Update any scripts that reference Cinemachine APIs to Unity 6 version

**Files to Modify/Create:**
- Scripts that reference Cinemachine (if any exist)
- Camera controller scripts

**Prerequisites:**
- Task 5 complete (Cinemachine updated)

**Implementation Steps:**

1. Check Console for Cinemachine-related compilation errors
2. Search codebase for Cinemachine references:
   - Look for `using Cinemachine;` statements
   - Check for CinemachineVirtualCamera references
   - Look for CinemachineBrain usage
3. Common Cinemachine 3.x changes (if applicable):
   - Namespace changes
   - Component name changes (VirtualCamera API updates)
   - Extension API changes
4. Update scripts with Cinemachine API calls:
   - Update namespaces if changed
   - Update component references
   - Fix method calls that changed
5. If no custom Cinemachine scripts exist, verify automatic migration worked:
   - Check that cameras in scenes still function
   - Verify no broken component references
6. Test camera behavior in Play mode thoroughly

**Verification Checklist:**
- [ ] Zero Cinemachine compilation errors
- [ ] Cinemachine API calls updated (if any custom scripts exist)
- [ ] Camera follows character smoothly
- [ ] Camera framing works correctly
- [ ] No broken Cinemachine component references

**Testing Instructions:**
- Search project for "using Cinemachine" to find affected scripts
- Compile and verify zero errors
- Enter Play mode and test camera:
  - Camera follows character
  - Camera framing appropriate
  - No jittering or errors
  - Camera transitions work (if multiple cameras exist)

**Commit Message Template:**
```
fix(cinemachine): resolve Cinemachine API compatibility

Updated Cinemachine API calls for Unity 6
Fixed camera integration

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~3,500

---

### Task 7: Update Remaining Unity Packages

**Goal:** Update all other Unity packages to Unity 6-compatible versions

**Files to Modify/Create:**
- `Packages/manifest.json`

**Prerequisites:**
- Tasks 1-6 complete (URP, Input, Cinemachine updated)

**Implementation Steps:**

1. Open Package Manager
2. Update remaining packages in order of importance:
   - **TextMesh Pro** (3.0.6 ’ Unity 6 version)
   - **ProBuilder** (5.2.3 ’ Unity 6 version, likely 6.x)
   - **Timeline** (1.6.5 ’ Unity 6 version)
   - **Test Framework** (1.1.33 ’ Unity 6 version)
   - **Visual Scripting** (1.9.4 ’ Unity 6 version, if used)
3. For each package:
   - Select package in Package Manager
   - Click "Update to [version]"
   - Wait for download and import
   - Check Console for errors
   - Verify no breaking changes
4. Check for package-specific migration dialogs and accept upgrades
5. After all packages updated, perform full project reimport (Assets > Reimport All)
6. Verify all packages updated in Package Manager
7. Verify `Packages/manifest.json` shows new versions

**Verification Checklist:**
- [ ] All Unity packages updated to Unity 6 versions
- [ ] TextMesh Pro updated (UI should still work)
- [ ] ProBuilder updated (if scenes use ProBuilder geometry)
- [ ] Timeline updated
- [ ] Test Framework updated
- [ ] Full reimport completed successfully
- [ ] No compilation errors

**Testing Instructions:**
- Check Package Manager - all packages should show Unity 6 versions
- Load scenes and verify no broken references
- Check UI elements (TextMesh Pro)
- Verify ProBuilder geometry still intact (if used)
- Run a timeline (if project uses timelines)
- Check Console for errors

**Commit Message Template:**
```
feat(packages): update all Unity packages to Unity 6

Updated TextMesh Pro, ProBuilder, Timeline, Test Framework
All packages now Unity 6-compatible

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,000

---

### Task 8: Test Core Gameplay After Package Updates

**Goal:** Verify core gameplay systems work after all package updates

**Files to Modify/Create:**
- None (testing only)

**Prerequisites:**
- Task 7 complete (all packages updated)

**Implementation Steps:**

1. Open MainScene in Unity Editor
2. Enter Play mode and perform comprehensive gameplay test:
   - **Character Movement:**
     - Move character with WASD/arrows
     - Verify smooth movement
     - Check character responds to input correctly
   - **Combat System:**
     - Perform light attacks
     - Perform heavy attacks
     - Test blocking
     - Test dodging
     - Verify combat animations play
   - **AI System:**
     - Check that AI opponent exists
     - Verify AI responds to player
     - Test AI attacks player
     - Check AI state machine behavior
   - **UI System:**
     - Verify health bars display
     - Check stamina bar works
     - Test combo counter
     - Verify round timer displays
   - **Camera:**
     - Verify camera follows player
     - Check camera framing
     - Move around and test camera behavior
   - **Audio:**
     - Listen for audio cues
     - Verify sound effects play
     - Check background music (if any)
3. Play for 3-5 minutes and note any issues:
   - Document runtime errors in Console
   - Note visual glitches
   - Record input issues
   - Document AI problems
4. Exit Play mode cleanly
5. Document findings in `docs/migration/PHASE3_GAMEPLAY_TEST.md`

**Verification Checklist:**
- [ ] Character movement works
- [ ] Combat system functional (attacks, blocking, dodging)
- [ ] AI opponent responsive
- [ ] UI displays correctly
- [ ] Camera follows character
- [ ] Audio plays (if applicable)
- [ ] No critical runtime errors (warnings acceptable)
- [ ] Game is playable (even if bugs exist)

**Testing Instructions:**
- Play full round of combat against AI
- Test all combat moves
- Verify all systems respond
- Document bugs but don't fix yet (Phase 4)
- Exit Play mode without crashes

**Commit Message Template:**
```
test(migration): verify core gameplay after package updates

Tested movement, combat, AI, UI, camera, audio
Core systems functional, documented runtime issues for Phase 4

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~5,000

---

### Task 9: Tag Package Update Completion

**Goal:** Create git checkpoint after successful package updates

**Files to Modify/Create:**
- `docs/migration/UPGRADE_CHECKLIST.md` (update progress)

**Prerequisites:**
- Task 8 complete (gameplay tested)

**Implementation Steps:**

1. Verify all changes committed
2. Create git tag "post-package-updates"
3. Update `docs/migration/UPGRADE_CHECKLIST.md`:
   - Mark Phase 3 complete
   - Note package versions achieved
   - Document any deferred issues
4. Create Phase 3 summary:
   - Packages updated (URP, Input System, Cinemachine, etc.)
   - Gameplay status (functional/issues)
   - Time spent
   - Next phase focus (testing)
5. Commit documentation updates

**Verification Checklist:**
- [ ] All code changes committed
- [ ] Git tag "post-package-updates" exists
- [ ] UPGRADE_CHECKLIST.md updated
- [ ] Phase 3 summary documented
- [ ] Packages/ manifest.json shows Unity 6 versions

**Testing Instructions:**
- Run `git tag` and verify tag exists
- Check `Packages/manifest.json` for updated versions
- Verify git status is clean

**Commit Message Template:**
```
chore(migration): complete Phase 3 package updates

 All Unity packages updated to Unity 6
 URP, Input System, Cinemachine functional
 Core gameplay systems working
   Some runtime bugs exist (Phase 4 will address)

Tagged as 'post-package-updates'

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~3,000

---

## Phase Verification

After completing all tasks in Phase 3, verify the following:

### Package Updates Complete
- [ ] URP updated to Unity 6 version
- [ ] Input System updated
- [ ] Cinemachine updated
- [ ] TextMesh Pro updated
- [ ] ProBuilder updated
- [ ] Timeline updated
- [ ] Test Framework updated
- [ ] All packages show Unity 6-compatible versions

### Compilation Clean
- [ ] Zero compilation errors
- [ ] All scripts compile successfully
- [ ] Package-related APIs updated

### Gameplay Functional
- [ ] Character movement responsive
- [ ] Combat system works (attacks, blocking, dodging)
- [ ] AI opponent functional
- [ ] UI displays correctly
- [ ] Camera follows character
- [ ] Audio plays

### Documentation
- [ ] PHASE3_GAMEPLAY_TEST.md documents gameplay status
- [ ] UPGRADE_CHECKLIST.md updated
- [ ] Git tag "post-package-updates" created

### Known Issues (Expected)
- [ ] Some test failures (Phase 4 will fix)
- [ ] Minor visual glitches (acceptable)
- [ ] Occasional runtime warnings (acceptable)
- [ ] Performance not optimized yet (Phase 7)

---

## Known Limitations and Technical Debt

**Deferred to Later Phases:**
- Test failures (Phase 4) - tests may not all pass yet
- Performance optimization (Phase 7) - haven't optimized yet
- WebGL build testing (Phase 7) - haven't built for WebGL yet

**Potential Issues:**
- Cinemachine 3.x may have different behavior than 2.x (test thoroughly)
- URP rendering may look slightly different (expected with major version jump)
- Input System may have subtle timing differences

---

## Next Phase

Once Phase 3 verification is complete:
- Proceed to [Phase-4.md](Phase-4.md) - Test Suite Migration & Fixes
- Phase 4 will systematically fix all test failures
- Tests will guide remaining bug fixes

**Estimated time for Phase 3:** 8-16 hours (package updates and testing)
