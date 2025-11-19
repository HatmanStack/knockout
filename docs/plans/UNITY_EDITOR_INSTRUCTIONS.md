# Unity Editor Instructions for Unity 6.0 Migration

## Important Note

**These instructions require Unity Editor access.** Most of the Unity 6.0 migration work requires opening Unity Editor, which may not be available during initial plan execution. This document consolidates all Unity Editor-dependent tasks for execution when Editor access is available.

---

## Unity Editor Access Required For

The following migration phases **require Unity Editor** and cannot be completed without it:

- **Phase 1**: Pre-Upgrade Preparation (38% Unity Editor - Tasks 1, 5, 8)
- **Phase 2**: Unity 6 Upgrade Execution (100% Unity Editor)
- **Phase 3**: Package Updates (100% Unity Editor)
- **Phase 4**: Test Suite Migration (100% Unity Editor)
- **Phase 5**: Asset Store Package Updates (90% Unity Editor)
- **Phase 6**: Modernization (80% Unity Editor)
- **Phase 7**: WebGL Optimization (100% Unity Editor)

---

## Phase 1: Unity Editor Tasks (Pre-Upgrade Preparation)

### Task 1-1: Verify Environment and Unity Versions

**Requires Unity Editor (Unity Hub)**

1. Open Unity Hub
2. Verify Unity 2021.3.45f2 is installed and listed
3. Verify Unity 6.0 LTS is installed and listed
4. Launch Unity 2021.3 to verify it works
5. Close Unity 2021.3
6. Launch Unity 6.0 to verify it works (do NOT open Knockout project yet)
7. Close Unity 6.0
8. Check disk space (minimum 25-30GB free)

**Status:** Required before Phase 2
**Estimated Time:** 15-30 minutes

### Task 1-5: Establish Performance Baselines

**Requires Unity Editor** (CRITICAL TASK)

1. Open Knockout project in Unity 2021.3.45f2
2. Wait for project to fully load and compile
3. Open Test Runner (Window > General > Test Runner)
4. Run all EditMode tests:
   - Note total execution time
   - Note pass/fail count
   - Document any failures
5. Run all PlayMode tests:
   - Note total execution time
   - Note pass/fail count
   - Document any failures
6. Build WebGL version:
   - File > Build Settings > WebGL
   - Enable "Development Build" for profiling
   - Click Build and note build time
   - Measure build folder size
   - Note WASM file size specifically
7. Test WebGL build in browser:
   - Start local web server (e.g., `python -m http.server`)
   - Open build in Chrome
   - Measure load time (time until interactive)
   - Play for 2-3 minutes
   - Record frame rate (F12 > Performance or Unity Profiler)
   - Note memory usage
   - Document any console errors/warnings
8. Create `docs/migration/PERFORMANCE_BASELINE.md` with all metrics
9. Commit the baseline document

**Status:** CRITICAL - Required for Phase 7 comparison
**Estimated Time:** 1-2 hours (includes build time)

**Metrics to Record:**
- EditMode test time: _____ seconds
- EditMode test pass rate: ___/___
- PlayMode test time: _____ seconds
- PlayMode test pass rate: ___/___
- WebGL build time: _____ minutes
- WebGL build size: _____ MB
- WASM file size: _____ MB
- Load time: _____ seconds
- Average FPS: _____
- Min FPS: _____
- Memory usage: _____ MB

### Task 1-8: Verify Project Opens in Unity 2021.3

**Requires Unity Editor**

1. Open Knockout project in Unity 2021.3.45f2
2. Check Console for errors on load (Window > General > Console)
3. Document any errors found
4. Open MainScene.unity (Assets/Knockout/Scenes/MainScene.unity)
5. Verify scene loads without errors
6. Enter Play mode (click Play button)
7. Test basic gameplay:
   - Character moves
   - Can attack
   - UI displays
   - No critical errors in Console
8. Exit Play mode
9. Run quick test suite check:
   - Open Test Runner
   - Run a few representative tests
   - Verify tests execute (pass/fail rate less important than execution)
10. Close Unity Editor cleanly
11. Document results in migration notes

**Status:** Required before Phase 2
**Estimated Time:** 30-45 minutes

**Verification Checklist:**
- [ ] Project opens without fatal errors
- [ ] MainScene loads successfully
- [ ] Play mode works
- [ ] Basic gameplay functional
- [ ] Tests can run
- [ ] Editor closes without crash

---

## Phase 2: Unity Editor Tasks (Unity 6 Upgrade Execution)

**Phase Goal:** Upgrade Knockout project from Unity 2021.3.45f2 to Unity 6.0

**Success Criteria:**
- Project opens in Unity 6.0 without fatal errors
- All scripts compile (zero compilation errors)
- Editor is functional (can navigate, open scenes, enter Play mode)
- Changes committed with "post-unity6-upgrade" tag

**Estimated Time:** 4-8 hours (depending on compilation errors)

**Prerequisites:**
- Phase 1 Tasks 1, 5, 8 complete
- Unity 6.0 LTS installed
- All changes committed
- Git tag "pre-unity6-upgrade" exists

---

### Task 2-1: Open Project in Unity 6.0 for First Time

**Requires Unity Editor** ⚠️

**Goal:** Launch Unity 6 and open Knockout project, allowing automatic upgrade

**Detailed Steps:**

1. **IMPORTANT:** Close Unity 2021.3 if it's open
2. Launch Unity Hub
3. In Unity Hub, find the Knockout project in projects list
4. Click the project, then change Unity version dropdown to Unity 6.0.x
5. Unity Hub will warn about upgrading - **confirm and proceed**
6. Unity 6 will launch and begin opening the project
7. Unity will show "Upgrading" dialog - **allow it to complete** (may take several minutes)
8. Watch for "API Update Required" dialog:
   - Unity's API Updater will run automatically
   - Review proposed changes if shown
   - Accept automated updates
9. Wait for Unity to finish importing all assets (10-30 minutes)
10. Once import completes, check Console for errors
11. **Do not close Unity yet** - proceed to Task 2-2

**Verification Checklist:**
- [ ] Unity 6 opened project successfully
- [ ] API Updater completed (dialog shown and accepted)
- [ ] Asset import finished (progress bar complete)
- [ ] Console is visible (may have errors - expected)
- [ ] Project view shows assets (folders visible)

**Files Modified Automatically:**
- `ProjectSettings/ProjectVersion.txt` (Unity version updated)
- `Packages/manifest.json` (Unity may update package versions)
- Various `.meta` files (Unity may update serialization)

**Commit After This Task:**
```
chore(migration): open project in Unity 6.0 for first time

Unity automatic upgrade process completed
API Updater ran successfully

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated Time:** 30-60 minutes (mostly waiting for Unity)

---

### Task 2-2: Review and Document Initial Errors

**Requires Unity Editor** ⚠️

**Goal:** Catalog all compilation errors to understand scope of fixes needed

**Detailed Steps:**

1. In Unity Editor, open Console window (Window > General > Console)
2. Click "Clear on Play" and "Error Pause" to disable them
3. Click "Collapse" to group similar errors
4. Count total number of unique errors
5. Create `docs/migration/PHASE2_ERRORS.md` file (in text editor outside Unity)
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

**Commit After This Task:**
```
docs(migration): catalog Unity 6 compilation errors

Documented [X] unique errors across [Y] categories

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated Time:** 30-60 minutes

---

### Task 2-3: Fix Critical Compilation Errors (Namespace Changes)

**Requires Unity Editor** ⚠️

**Goal:** Fix namespace import errors that prevent compilation

**Detailed Steps:**

1. Review PHASE2_ERRORS.md for namespace-related errors
2. Common Unity 6 namespace changes to look for:
   - `UnityEngine.Experimental` → new namespaces
   - Input System namespace changes
   - URP namespace changes (though full package update in Phase 3)
3. Fix namespace errors systematically:
   - Open file with error in IDE (double-click error in Console)
   - Update `using` statements at top of file
   - Add new required namespaces
   - Remove obsolete namespaces
   - Save file
4. Return to Unity and wait for recompilation
5. Verify error is resolved in Console
6. Repeat for all namespace errors
7. Once namespace errors fixed, recompile entire project (Assets > Reimport All)

**Verification Checklist:**
- [ ] All namespace errors resolved
- [ ] Updated files compile successfully
- [ ] Console shows reduced error count
- [ ] No new errors introduced by namespace changes

**Commit After This Task:**
```
fix(migration): resolve Unity 6 namespace changes

Updated using statements for Unity 6 APIs
Fixed namespace errors in [X] files

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated Time:** 1-2 hours

---

### Task 2-4: Fix Obsolete API Errors

**Requires Unity Editor** ⚠️

**Goal:** Replace obsolete Unity APIs with Unity 6 equivalents

**Detailed Steps:**

1. Review remaining Console errors for "obsolete" or "deprecated" messages
2. For each obsolete API error:
   - Identify obsolete API being used
   - Find Unity 6 replacement (error message usually suggests replacement)
   - Open file in IDE
   - Replace obsolete API call with new API
   - Update any changed parameters or signatures
   - Save file
3. Common Unity 6 obsolete APIs to watch for:
   - Physics APIs (collision detection updates)
   - Input System APIs (if using old input)
   - Rendering APIs (some URP APIs may be obsolete)
   - Camera APIs
4. After fixing each file, verify compilation in Unity
5. Document any APIs without clear replacements in PHASE2_ERRORS.md
6. Commit changes incrementally (every 5-10 files fixed)

**Verification Checklist:**
- [ ] All obsolete API errors identified
- [ ] Replacements found for each obsolete API
- [ ] Files updated with new APIs
- [ ] Code compiles after changes
- [ ] No functionality broken by API changes

**Commit After This Task:**
```
fix(migration): replace obsolete APIs with Unity 6 equivalents

Updated [API names] to modern Unity 6 APIs
Fixed obsolete API errors in [X] files

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated Time:** 1-2 hours

---

### Task 2-5: Fix Type and Signature Errors

**Requires Unity Editor** ⚠️

**Goal:** Resolve method signature mismatches and type errors introduced by Unity 6

**Detailed Steps:**

1. Review remaining errors for type mismatches and signature errors
2. Common issues to look for:
   - MonoBehaviour method signature changes (OnCollision, OnTrigger)
   - Unity component property type changes
   - Generic type constraints that changed
   - Enum value changes or additions
3. Fix each error:
   - Open file in IDE
   - Identify type mismatch or signature issue
   - Update method signature or type declaration
   - Update any calling code if necessary
   - Ensure compatibility with existing code
   - Save file
4. Pay special attention to:
   - Character controller interfaces (ICharacterController)
   - Physics callbacks (OnCollision*, OnTrigger*)
   - Input System callbacks (if used)
5. Verify no runtime issues introduced (test more in later tasks)
6. Commit changes as you fix categories of errors

**Verification Checklist:**
- [ ] All type mismatch errors resolved
- [ ] Method signatures match Unity 6 expectations
- [ ] Interface implementations correct
- [ ] Generics compile successfully
- [ ] No new type errors introduced

**Commit After This Task:**
```
fix(migration): resolve Unity 6 type and signature changes

Updated method signatures and types for Unity 6
Fixed type errors in [X] files

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated Time:** 1-2 hours

---

### Task 2-6: Achieve Zero Compilation Errors

**Requires Unity Editor** ⚠️

**Goal:** Fix all remaining compilation errors to achieve clean compilation

**Detailed Steps:**

1. Review Console for any remaining compilation errors
2. Group remaining errors by similarity
3. Fix errors systematically:
   - Start with core systems (CharacterController, Combat, AI)
   - Move to UI and audio systems
   - Finish with utilities and helpers
4. For each error:
   - Understand root cause
   - Determine fix (API change, logic update, etc.)
   - Make minimal changes to resolve error
   - Test compilation
   - Commit if multiple files fixed
5. If stuck on any error:
   - Search Unity 6 documentation
   - Check Unity forums for similar issues
   - Document blocker in PHASE2_ERRORS.md
   - Consider temporary workaround if safe
6. Once Console shows zero errors, perform full recompilation (Assets > Reimport All)
7. Verify zero errors persist after reimport

**Verification Checklist:**
- [ ] Console shows 0 compilation errors
- [ ] Full project reimport completes successfully
- [ ] No errors after reimport
- [ ] All scripts in project compile
- [ ] DLLs built successfully (visible in Library folder)

**Commit After This Task:**
```
fix(migration): achieve zero compilation errors in Unity 6

All scripts now compile successfully
Resolved remaining [X] errors across project

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated Time:** 1-3 hours

---

### Task 2-7: Verify Basic Editor Functionality

**Requires Unity Editor** ⚠️

**Goal:** Confirm Unity 6 Editor works correctly after compilation fixes

**Detailed Steps:**

1. Open MainScene.unity from Project window
2. Verify scene loads without errors
3. Check Hierarchy view shows scene objects
4. Check Inspector shows component properties
5. Navigate around scene in Scene view (use mouse to pan/zoom)
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

**Commit After This Task:**
```
test(migration): verify Unity 6 Editor functionality

Confirmed scenes load, Play mode works, basic editing functional
Runtime errors exist but Editor is stable

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated Time:** 30-45 minutes

---

### Task 2-8: Tag Post-Upgrade State and Create Checkpoint

**Requires Unity Editor** ⚠️ (Partially - mainly git operations)

**Goal:** Create git tag for post-upgrade state as rollback point and milestone

**Detailed Steps:**

1. Verify all changes are committed:
   - Run `git status`
   - Stage and commit any uncommitted changes
2. Create annotated git tag "post-unity6-upgrade":
   - Tag current commit
   - Include message describing successful Unity 6 upgrade
   - Note compilation is clean but runtime issues may exist
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

**Git Commands:**
```bash
# Verify clean status
git status

# Create annotated tag
git tag -a post-unity6-upgrade -m "Unity 6.0 upgrade complete - project compiles and Editor functional"

# Verify tag
git tag

# Push to remote (if applicable)
git push origin main
git push origin post-unity6-upgrade
```

**Verification Checklist:**
- [ ] All code changes committed
- [ ] Git tag "post-unity6-upgrade" exists
- [ ] UPGRADE_CHECKLIST.md updated
- [ ] Phase 2 summary documented
- [ ] Changes pushed to remote (if applicable)

**Commit After This Task:**
```
chore(migration): complete Phase 2 Unity 6 upgrade

✅ Project compiles in Unity 6.0
✅ Editor functional and stable
✅ Scenes load and Play mode works
⚠️  Runtime issues exist (to be fixed in Phase 3-4)

Tagged as 'post-unity6-upgrade'

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated Time:** 15-30 minutes

---

### Phase 2 Verification Checklist

After completing all Task 2-1 through 2-8, verify:

**Compilation Success:**
- [ ] Console shows 0 compilation errors
- [ ] All scripts compile successfully
- [ ] Project reimports without errors
- [ ] DLLs built in Library folder

**Editor Functionality:**
- [ ] Project opens in Unity 6.0 successfully
- [ ] Scenes load without fatal errors
- [ ] Play mode can be entered and exited
- [ ] Basic editing operations work
- [ ] Editor doesn't crash during normal use

**Documentation:**
- [ ] PHASE2_ERRORS.md documents all errors encountered
- [ ] Error fixes documented in commit messages
- [ ] UPGRADE_CHECKLIST.md updated with Phase 2 completion

**Version Control:**
- [ ] All changes committed
- [ ] Git tag "post-unity6-upgrade" exists
- [ ] Commit history shows incremental fixes
- [ ] Clean git status (no uncommitted changes)

**Known Issues (Expected):**
- [ ] Runtime errors may exist (will fix in Phase 3-4)
- [ ] Tests may not pass yet (Phase 4)
- [ ] Warnings may exist (acceptable for now)
- [ ] Visual issues may be present (package updates in Phase 3)

---

### Phase 2 Known Limitations

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

### Next Phase

Once Phase 2 verification complete:
- Proceed to Phase 3 (Package Updates & Compatibility)
- Phase 3 will update URP, Input System, and other Unity packages
- Many runtime issues will be resolved by package updates

---

## Phase 3: Unity Editor Tasks (Package Updates)

### Task 3-1 through 3-9: All Package Update Tasks

**Requires Unity Editor**

All package updates require:
- Package Manager (Unity Editor only)
- Asset reimport (Unity Editor)
- Scene loading and testing (Unity Editor)
- Play mode testing (Unity Editor)

**Defer until Unity Editor access available**

---

## Phase 4: Unity Editor Tasks (Test Suite)

### Task 4-1 through 4-10: All Test Suite Tasks

**Requires Unity Editor**

All test tasks require:
- Unity Test Runner (Unity Editor only)
- Running tests (Unity Editor)
- Fixing production code and tests (requires Unity compilation)
- Play mode for integration tests

**Defer until Unity Editor access available**

---

## Phase 5: Unity Editor Tasks (Asset Store Packages)

### Task 5-1: Research (Can be done without Unity Editor)

This task can be completed in a web browser.

### Task 5-2 through 5-7: Asset Updates and Testing

**Requires Unity Editor**

All asset update and testing tasks require:
- Asset import (Unity Editor)
- Scene loading (Unity Editor)
- Play mode testing (Unity Editor)
- Verification of assets in scenes

**Defer until Unity Editor access available**

---

## Phase 6: Unity Editor Tasks (Modernization)

### Task 6-1: Audit Deprecation Warnings

**Requires Unity Editor**

Requires Console access to view warnings.

### Task 6-2 through 6-7: All Modernization Tasks

**Requires Unity Editor**

All modernization tasks require:
- Code compilation in Unity
- Testing in Play mode
- Editor script updates
- URP asset configuration

**Defer until Unity Editor access available**

---

## Phase 7: Unity Editor Tasks (WebGL Optimization)

### Task 7-1 through 7-9: All WebGL Tasks

**Requires Unity Editor**

All WebGL tasks require:
- Build Settings (Unity Editor)
- Building for WebGL (Unity Editor)
- Player Settings configuration (Unity Editor)
- Quality Settings (Unity Editor)
- Profiling (Unity Editor)

**Defer until Unity Editor access available**

---

## Alternative Workflow: Code-Only Changes

If Unity Editor is not available, the following can be done with IDE/text editor only:

1. **Research and Documentation** (Phase 1, Phase 5 Task 1)
   - Can research packages
   - Can document findings
   - Can create baseline documentation templates

2. **Code Review** (Limited)
   - Can review C# code for obvious issues
   - Can identify potential API usage to update
   - Cannot verify changes compile

3. **Planning** (Phase 0, documentation)
   - Can create migration plans
   - Can document strategies
   - Cannot execute Unity-specific tasks

---

## When Unity Editor Becomes Available

Execute phases in order:

1. **Start with Phase 2** - Open in Unity 6
2. **Fix critical compilation errors** to get project compiling
3. **Proceed through Phase 3** - Update packages
4. **Complete Phase 4** - Get tests passing
5. **Continue with remaining phases**

---

## Important Limitations Without Unity Editor

**Cannot:**
- Compile C# code (Unity-specific compilation)
- Run tests
- Verify API changes work
- Update packages
- Build for WebGL
- Test in Play mode
- Access Unity Console
- Configure Unity settings
- Import assets

**Can:**
- Research and documentation
- Plan and strategy
- Code review (limited value without compilation)
- Prepare scripts for future execution

---

## Recommendation

**Wait for Unity Editor access before executing Phases 2-7.** Complete Phase 1 (documentation and research) first, then pause until Unity Editor is available.

Alternatively, if code changes must be made before Unity Editor access:
1. Make changes in IDE
2. Document all changes
3. Note that changes are **untested and unverified**
4. Plan to validate everything once Unity Editor access is restored
5. Expect to iterate significantly on first Unity Editor open
