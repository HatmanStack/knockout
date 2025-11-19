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

## Phase 2: Unity Editor Tasks (Opening Unity 6 for First Time)

### Task 2-1: Open Project in Unity 6.0

**Requires Unity Editor**

1. Close any open Unity Editor instances
2. Launch Unity Hub
3. Locate Knockout project in project list
4. Change Unity version to Unity 6.0.x
5. Confirm upgrade warning
6. Wait for Unity to open and run automatic upgrade (10-30 minutes)
7. Accept API Updater changes when prompted
8. Wait for asset import to complete

**Cannot proceed without Unity Editor**

### Task 2-2: Review Console Errors

**Requires Unity Editor**

1. Open Unity Console (Window > General > Console)
2. Review compilation errors
3. Document errors in `docs/migration/PHASE2_ERRORS.md`
4. Categorize errors by type

### Task 2-3 through 2-8: Fix Compilation Errors

**Requires Unity Editor**

All compilation error fixes require:
- Opening scripts in IDE from Unity
- Unity to recompile after changes
- Console to verify error resolution
- Ability to test in Play mode

**Defer until Unity Editor access available**

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
