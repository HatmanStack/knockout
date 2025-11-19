# Unity Editor Instructions for Unity 6.0 Migration

## Important Note

**These instructions require Unity Editor access.** Most of the Unity 6.0 migration work requires opening Unity Editor, which may not be available during initial plan execution. This document consolidates all Unity Editor-dependent tasks for execution when Editor access is available.

---

## Unity Editor Access Required For

The following migration phases **require Unity Editor** and cannot be completed without it:

- **Phase 2**: Unity 6 Upgrade Execution (100% Unity Editor)
- **Phase 3**: Package Updates (100% Unity Editor)
- **Phase 4**: Test Suite Migration (100% Unity Editor)
- **Phase 5**: Asset Store Package Updates (90% Unity Editor)
- **Phase 6**: Modernization (80% Unity Editor)
- **Phase 7**: WebGL Optimization (100% Unity Editor)

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
