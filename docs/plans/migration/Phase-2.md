# Phase 2: Unity 6 Upgrade Execution

## Phase Goal

Upgrade the Knockout project from Unity 2021.3.45f2 to Unity 6.0 by opening the project in Unity 6, allowing automatic migration, and fixing immediate compilation errors. This phase focuses on getting the project to compile and open in Unity 6, not on achieving full functionality.

**Success Criteria:**
- Project opens in Unity 6.0 without fatal errors
- Unity automatic migration completes successfully
- All scripts compile (no compilation errors in Console)
- Editor is functional (can navigate, open scenes, enter Play mode)
- Changes committed with "post-unity6-upgrade" tag

**Estimated tokens:** ~24,000

---

## Prerequisites

- Phase 1 complete (backups created, baselines established)
- Unity 6.0 LTS installed
- Git tag "pre-unity6-upgrade" exists
- External backup verified
- All changes committed

---

## Tasks

### Task 1: Open Project in Unity 6.0 for First Time

**Goal:** Launch Unity 6 and open the Knockout project, allowing automatic upgrade to proceed

**Files to Modify/Create:**
- `ProjectSettings/ProjectVersion.txt` (Unity will modify automatically)
- `Packages/manifest.json` (Unity may update automatically)
- Various `.meta` files (Unity may update)

**Prerequisites:**
- Phase 1 complete
- Unity 6.0 installed

**Implementation Steps:**

1. **IMPORTANT:** Close Unity 2021.3 if it's open
2. Launch Unity Hub
3. In Unity Hub, find the Knockout project in your projects list
4. Click the project, then change the Unity version dropdown to Unity 6.0.x
5. Unity Hub will warn you about upgrading - **confirm and proceed**
6. Unity 6 will launch and begin opening the project
7. Unity will show "Upgrading" dialog - **allow it to complete** (may take several minutes)
8. Watch for the "API Update Required" dialog:
   - Unity's API Updater will run automatically
   - Review the proposed changes if shown
   - Accept the automated updates
9. Wait for Unity to finish importing all assets (this may take 10-30 minutes)
10. Once import completes, check the Console for errors
11. **Do not close Unity yet** - proceed to next task

**Verification Checklist:**
- [ ] Unity 6 opened the project successfully
- [ ] API Updater completed (dialog shown and accepted)
- [ ] Asset import finished (progress bar complete)
- [ ] Console is visible (may have errors - this is expected)
- [ ] Project view shows assets (folders visible)

**Testing Instructions:**
- Check Console for error count (errors expected at this stage)
- Verify Project window shows `Assets/` folder structure
- Verify Scene Hierarchy is visible (even if scene not loaded)
- Confirm Unity Editor is responsive (not frozen)

**Commit Message Template:**
```
chore(migration): open project in Unity 6.0 for first time

Unity automatic upgrade process completed
API Updater ran successfully

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~3,000

---

### Task 2: Review and Document Initial Errors

**Goal:** Catalog all compilation errors to understand the scope of fixes needed

**Files to Modify/Create:**
- `docs/migration/PHASE2_ERRORS.md` (new file to create)

**Prerequisites:**
- Task 1 complete (Unity 6 open with project loaded)

**Implementation Steps:**

1. In Unity Editor, open the Console window (Window > General > Console)
2. Click "Clear on Play" and "Error Pause" to disable them
3. Click "Collapse" to group similar errors
4. Count total number of unique errors
5. Create `docs/migration/PHASE2_ERRORS.md` file (outside Unity, in your text editor)
6. For each unique error, document:
   - Error message (full text)
   - Affected file(s)
   - Error category (API change, namespace change, obsolete API, etc.)
7. Group errors by category to identify patterns
8. Prioritize errors:
   - **Critical:** Prevent compilation entirely
   - **High:** Affect core systems (combat, AI, character controller)
   - **Medium:** Affect secondary systems (UI, audio)
   - **Low:** Affect utilities or test helpers
9. Save documentation file

**Verification Checklist:**
- [ ] `docs/migration/PHASE2_ERRORS.md` exists
- [ ] Total error count documented
- [ ] All unique errors cataloged
- [ ] Errors grouped by category
- [ ] Errors prioritized

**Testing Instructions:**
- Review PHASE2_ERRORS.md for completeness
- Verify error count matches Console
- Confirm categorization makes sense
- Check that priorities align with project architecture

**Commit Message Template:**
```
docs(migration): catalog Unity 6 compilation errors

Documented [X] unique errors across [Y] categories

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~3,500

---

### Task 3: Fix Critical Compilation Errors (Namespace Changes)

**Goal:** Fix namespace import errors that prevent compilation

**Files to Modify/Create:**
- Various `.cs` files with namespace errors (determined from Console errors)

**Prerequisites:**
- Task 2 complete (errors documented)

**Implementation Steps:**

1. Review PHASE2_ERRORS.md for namespace-related errors
2. Common Unity 6 namespace changes to look for:
   - `UnityEngine.Experimental` ’ new namespaces
   - Input System namespace changes
   - URP namespace changes (though package update happens in Phase 3)
3. Fix namespace errors systematically:
   - Open file with error in IDE (double-click error in Console)
   - Update `using` statements at top of file
   - Add new required namespaces
   - Remove obsolete namespaces
   - Save file
4. Return to Unity and wait for recompilation
5. Verify error is resolved in Console
6. Repeat for all namespace errors
7. Once namespace errors are fixed, recompile entire project (Assets > Reimport All)

**Verification Checklist:**
- [ ] All namespace errors resolved
- [ ] Updated files compile successfully
- [ ] Console shows reduced error count
- [ ] No new errors introduced by namespace changes

**Testing Instructions:**
- Check Console after each file fix
- Verify error count decreases
- Ensure no new errors appear
- Confirm project recompiles after "Reimport All"

**Commit Message Template:**
```
fix(migration): resolve Unity 6 namespace changes

Updated using statements for Unity 6 APIs
Fixed namespace errors in [X] files

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,000

---

### Task 4: Fix Obsolete API Errors

**Goal:** Replace obsolete Unity APIs with their Unity 6 equivalents

**Files to Modify/Create:**
- Various `.cs` files with obsolete API usage

**Prerequisites:**
- Task 3 complete (namespace errors fixed)

**Implementation Steps:**

1. Review remaining errors in Console for "obsolete" or "deprecated" messages
2. For each obsolete API error:
   - Identify the obsolete API being used
   - Find the Unity 6 replacement (error message usually suggests replacement)
   - Open the file in IDE
   - Replace obsolete API call with new API
   - Update any changed parameters or signatures
   - Save file
3. Common Unity 6 obsolete APIs to watch for:
   - Physics APIs (Unity may have updated collision detection APIs)
   - Input System APIs (if using old input)
   - Rendering APIs (some URP APIs may be obsolete)
   - Camera APIs
4. After fixing each file, verify compilation in Unity
5. Document any APIs that don't have clear replacements in PHASE2_ERRORS.md
6. Commit changes incrementally (every 5-10 files fixed)

**Verification Checklist:**
- [ ] All obsolete API errors identified
- [ ] Replacements found for each obsolete API
- [ ] Files updated with new APIs
- [ ] Code compiles after changes
- [ ] No functionality broken by API changes (verify in next tasks)

**Testing Instructions:**
- After each API fix, check Console for new errors
- Verify method signatures match new API expectations
- Ensure return values are handled correctly
- Confirm code compiles cleanly

**Commit Message Template:**
```
fix(migration): replace obsolete APIs with Unity 6 equivalents

Updated [API names] to modern Unity 6 APIs
Fixed obsolete API errors in [X] files

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,500

---

### Task 5: Fix Type and Signature Errors

**Goal:** Resolve method signature mismatches and type errors introduced by Unity 6

**Files to Modify/Create:**
- Various `.cs` files with type/signature errors

**Prerequisites:**
- Task 4 complete (obsolete APIs fixed)

**Implementation Steps:**

1. Review remaining errors for type mismatches and signature errors
2. Common issues to look for:
   - MonoBehaviour method signature changes (e.g., OnCollision, OnTrigger)
   - Unity component property type changes
   - Generic type constraints that changed
   - Enum value changes or additions
3. Fix each error:
   - Open file in IDE
   - Identify the type mismatch or signature issue
   - Update method signature or type declaration
   - Update any calling code if necessary
   - Ensure compatibility with existing code
   - Save file
4. Pay special attention to:
   - Character controller interfaces (ICharacterController)
   - Physics callbacks (OnCollision*, OnTrigger*)
   - Input System callbacks (if used)
5. Verify no runtime issues introduced (will test more in later tasks)
6. Commit changes as you fix categories of errors

**Verification Checklist:**
- [ ] All type mismatch errors resolved
- [ ] Method signatures match Unity 6 expectations
- [ ] Interface implementations correct
- [ ] Generics compile successfully
- [ ] No new type errors introduced

**Testing Instructions:**
- Check Console after each fix
- Verify method signatures match Unity documentation
- Ensure interface implementations complete
- Confirm code compiles

**Commit Message Template:**
```
fix(migration): resolve Unity 6 type and signature changes

Updated method signatures and types for Unity 6
Fixed type errors in [X] files

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,000

---

### Task 6: Achieve Zero Compilation Errors

**Goal:** Fix all remaining compilation errors to achieve clean compilation

**Files to Modify/Create:**
- Any remaining files with compilation errors

**Prerequisites:**
- Tasks 3-5 complete (major error categories fixed)

**Implementation Steps:**

1. Review Console for any remaining compilation errors
2. Group remaining errors by similarity
3. Fix errors systematically:
   - Start with errors in core systems (CharacterController, Combat, AI)
   - Move to UI and audio systems
   - Finish with utilities and helpers
4. For each error:
   - Understand the root cause
   - Determine the fix (API change, logic update, etc.)
   - Make minimal changes to resolve error
   - Test compilation
   - Commit if multiple files fixed
5. If stuck on any error:
   - Search Unity 6 documentation
   - Check Unity forums for similar issues
   - Document the blocker in PHASE2_ERRORS.md
   - Consider temporary workaround if safe
6. Once Console shows zero errors, perform full recompilation (Assets > Reimport All)
7. Verify zero errors persist after reimport

**Verification Checklist:**
- [ ] Console shows 0 compilation errors
- [ ] Full project reimport completes successfully
- [ ] No errors after reimport
- [ ] All scripts in project compile
- [ ] DLLs built successfully (visible in Library folder)

**Testing Instructions:**
- Clear Console and recompile
- Verify zero errors
- Check for warnings (document but don't block on them)
- Confirm Editor is responsive

**Commit Message Template:**
```
fix(migration): achieve zero compilation errors in Unity 6

All scripts now compile successfully
Resolved remaining [X] errors across project

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~5,000

---

### Task 7: Verify Basic Editor Functionality

**Goal:** Confirm Unity 6 Editor works correctly with the project after compilation fixes

**Files to Modify/Create:**
- None (verification only)

**Prerequisites:**
- Task 6 complete (zero compilation errors)

**Implementation Steps:**

1. Open MainScene.unity from Project window
2. Verify scene loads without errors
3. Check Hierarchy view shows scene objects
4. Check Inspector shows component properties
5. Navigate around the scene in Scene view (use mouse to pan/zoom)
6. Enter Play mode:
   - Click Play button
   - Wait for game to initialize
   - Verify game starts (no immediate crash)
   - Observe Console for runtime errors (some expected - will fix in later phases)
   - Exit Play mode after 30-60 seconds
7. Test basic Editor operations:
   - Create new GameObject in scene
   - Add component to GameObject
   - Delete GameObject
   - Undo/Redo works
8. Open another scene (greybox.unity or Sample.unity) to verify multi-scene support
9. Close and reopen Unity to verify project state persists

**Verification Checklist:**
- [ ] MainScene loads without errors
- [ ] Scene view navigation works
- [ ] Play mode starts successfully
- [ ] Basic Editor operations functional (create, modify, delete)
- [ ] Multiple scenes can be opened
- [ ] Project state persists across Editor restarts

**Testing Instructions:**
- Load each main scene and verify it opens
- Enter Play mode and run game for 1 minute
- Check Console for runtime errors (document but don't fix yet)
- Test basic editing operations
- Verify no Editor crashes

**Commit Message Template:**
```
test(migration): verify Unity 6 Editor functionality

Confirmed scenes load, Play mode works, basic editing functional
Runtime errors exist but Editor is stable

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,000

---

### Task 8: Tag Post-Upgrade State and Create Checkpoint

**Goal:** Create git tag for post-upgrade state as rollback point and milestone

**Files to Modify/Create:**
- None (git operations only)

**Prerequisites:**
- Task 7 complete (Editor functionality verified)
- All changes committed

**Implementation Steps:**

1. Verify all changes are committed:
   - Run git status
   - Stage and commit any uncommitted changes
2. Create annotated git tag "post-unity6-upgrade":
   - Tag the current commit
   - Include message describing successful Unity 6 upgrade
   - Note that compilation is clean but runtime issues may exist
3. Update `docs/migration/UPGRADE_CHECKLIST.md`:
   - Mark Phase 2 as complete
   - Note any deferred issues for later phases
   - Record current state (compiles, Editor functional)
4. Create summary of Phase 2 results:
   - Number of errors fixed
   - Files modified
   - Time spent
   - Known remaining issues
5. Commit documentation updates
6. Push commits and tags to remote (if using remote repository)

**Verification Checklist:**
- [ ] All code changes committed
- [ ] Git tag "post-unity6-upgrade" exists
- [ ] UPGRADE_CHECKLIST.md updated
- [ ] Phase 2 summary documented
- [ ] Changes pushed to remote (if applicable)

**Testing Instructions:**
- Run `git tag` and verify "post-unity6-upgrade" is listed
- Run `git log --oneline -5` to see recent commits
- Check that no uncommitted changes exist (git status clean)

**Commit Message Template:**
```
chore(migration): complete Phase 2 Unity 6 upgrade

 Project compiles in Unity 6.0
 Editor functional and stable
 Scenes load and Play mode works
   Runtime issues exist (to be fixed in Phase 3-4)

Tagged as 'post-unity6-upgrade'

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~3,000

---

## Phase Verification

After completing all tasks in Phase 2, verify the following:

### Compilation Success
- [ ] Console shows 0 compilation errors
- [ ] All scripts compile successfully
- [ ] Project reimports without errors
- [ ] DLLs built in Library folder

### Editor Functionality
- [ ] Project opens in Unity 6.0 successfully
- [ ] Scenes load without fatal errors
- [ ] Play mode can be entered and exited
- [ ] Basic editing operations work
- [ ] Editor doesn't crash during normal use

### Documentation
- [ ] PHASE2_ERRORS.md documents all errors encountered
- [ ] Error fixes documented in commit messages
- [ ] UPGRADE_CHECKLIST.md updated with Phase 2 completion

### Version Control
- [ ] All changes committed
- [ ] Git tag "post-unity6-upgrade" exists
- [ ] Commit history shows incremental fixes
- [ ] Clean git status (no uncommitted changes)

### Known Issues (Expected)
- [ ] Runtime errors may exist (will fix in Phase 3-4)
- [ ] Tests may not pass yet (Phase 4)
- [ ] Warnings may exist (acceptable for now)
- [ ] Visual issues may be present (package updates in Phase 3)

---

## Known Limitations and Technical Debt

**Deferred to Later Phases:**
- Package updates (Phase 3) - URP, Input System, Cinemachine
- Test failures (Phase 4) - tests compile but may fail
- Third-party asset issues (Phase 5) - Asset Store packages may have issues
- Deprecation warnings (Phase 6) - will address during modernization

**Mitigation:**
- Runtime errors are expected at this stage
- Package updates in Phase 3 will resolve many issues
- Test suite in Phase 4 will systematically address remaining bugs

---

## Next Phase

Once Phase 2 verification is complete:
- Proceed to [Phase-3.md](Phase-3.md) - Package Updates & Compatibility
- Phase 3 will update URP, Input System, and other Unity packages
- Many runtime issues will be resolved by package updates

**Estimated time for Phase 2:** 4-8 hours (depending on number of compilation errors)
