# Phase 0: Foundation - Architecture & Strategy

## Purpose

This phase establishes the architectural decisions, upgrade strategy, testing approach, and shared patterns that will guide the entire Unity 6.0 upgrade process. Read this document completely before beginning any implementation phases.

**This phase requires NO implementation** - it is purely educational and strategic.

---

## Architecture Decision Records (ADRs)

### ADR-001: Direct Upgrade vs. Staged Upgrade

**Decision:** Perform a direct upgrade from Unity 2021.3.45f2 ’ Unity 6.0

**Rationale:**
- User requested direct upgrade for faster completion
- Project has comprehensive test suite (52+ test files) to catch issues
- Timeline allows for 2-3 weeks of focused debugging
- Intermediate versions (2022 LTS, 2023) would add overhead without significant benefit given test coverage

**Consequences:**
- More compilation errors and API changes to fix at once
- Higher initial debugging burden
- Faster overall completion
- Single migration learning curve

### ADR-002: Upgrade Strategy - Project in Place vs. New Project

**Decision:** Upgrade project in place using Unity Hub

**Rationale:**
- Preserves all project settings, scene references, and asset GUIDs
- Minimizes risk of broken prefab/scene references
- Unity's upgrade pipeline is designed for in-place upgrades
- Backup strategy mitigates risks

**Consequences:**
- Cannot easily compare old vs. new project side-by-side
- Requires robust backup and version control
- Slightly higher risk of corruption (mitigated by backups)

### ADR-003: Package Update Strategy

**Decision:** Update packages sequentially in dependency order, not all at once

**Rationale:**
- URP must be updated before rendering-dependent code is fixed
- Input System changes affect character controllers
- Cinemachine depends on Unity core APIs
- Sequential updates allow isolating breakage per package

**Update Order:**
1. Unity core packages (Editor, Timeline, etc.)
2. URP (render pipeline)
3. Input System
4. Cinemachine
5. Other Unity packages (ProBuilder, TextMeshPro, etc.)
6. Third-party Asset Store packages

**Consequences:**
- Longer Phase 3, but more controlled
- Easier debugging
- Clear failure attribution

### ADR-004: Test Suite Migration Strategy

**Decision:** Fix test compilation before fixing test failures

**Rationale:**
- Unity 6 may have Test Framework changes
- Test compilation errors block running any tests
- Once compiled, can use test failures as TODO list
- Failing tests guide production code fixes

**Approach:**
1. Fix all test compilation errors
2. Run full test suite, document failures
3. Fix test failures systematically (by component/system)
4. Use test results to guide production code migration

**Consequences:**
- Clear separation between compilation fixes and behavioral fixes
- Tests become the upgrade progress indicator
- May temporarily disable tests that can't be quickly fixed

### ADR-005: Third-Party Asset Strategy

**Decision:** Verify compatibility first, upgrade if available, replace if obsolete

**Rationale:**
- Asset Store packages may not support Unity 6 yet
- Some packages may be abandoned
- Replacement cost must be weighed against upgrade timeline

**Assets to Verify:**
- **AAAnimators Boxing** - Animation package (likely compatible, check Asset Store)
- **Asset Unlock - 3D Prototyping** - Prototyping tools (check compatibility)
- **StarterAssets** - May need re-download from Unity (official package)

**Fallback Plan:**
- If asset incompatible and critical: delay upgrade or find replacement
- If asset incompatible and non-critical: remove and refactor
- If asset abandoned: find modern alternative

**Consequences:**
- May need to budget time for asset replacement
- Some features may need re-implementation
- Opportunity to remove unused assets

### ADR-006: WebGL Build Validation Strategy

**Decision:** Validate WebGL builds at each major phase milestone

**Rationale:**
- WebGL is the target platform
- WebGL has unique limitations (threading, memory, WASM)
- Early WebGL testing catches platform-specific issues
- Editor functionality ` WebGL functionality

**Validation Points:**
- After Phase 2 (Unity 6 upgrade complete)
- After Phase 3 (packages updated)
- After Phase 4 (tests passing)
- After Phase 7 (final validation)

**Consequences:**
- Longer build times at milestones
- Earlier detection of WebGL-specific issues
- Higher confidence in final deliverable

### ADR-007: Rollback Strategy

**Decision:** Use git branches + Unity version pinning for rollback

**Rollback Procedure:**
1. Keep Unity 2021.3.45f2 installed alongside Unity 6.0
2. Git branches track upgrade progress
3. Can revert to pre-upgrade state via git + Unity version switch
4. Maintain backups outside git for catastrophic failures

**Branch Strategy:**
- Work happens in current branch (no branch name dependencies - branch agnostic)
- Tag critical milestones for easy rollback points
- Commit atomically to enable partial rollback

**Consequences:**
- Disk space for two Unity versions
- Clear rollback path at any point
- Confidence to proceed aggressively

### ADR-008: Performance Baseline Strategy

**Decision:** Establish performance baselines before and after upgrade

**Metrics to Track:**
- WebGL build size
- WebGL load time
- Runtime frame rate (average, min, max)
- Memory usage
- Test suite execution time

**Approach:**
1. Measure baselines in Unity 2021.3 (Phase 1)
2. Measure again after Unity 6 upgrade (Phase 2)
3. Measure after each major phase
4. Compare and optimize in Phase 7

**Consequences:**
- Clear before/after comparison
- Identifies performance regressions early
- Justifies optimization efforts

---

## Upgrade Strategy Overview

### Phase Flow

```
Phase 0: Foundation (this document)
         “
Phase 1: Pre-Upgrade Preparation
         - Backup project
         - Audit dependencies
         - Document current state
         - Establish performance baselines
         “
Phase 2: Unity 6 Upgrade Execution
         - Install Unity 6.0
         - Open project in Unity 6
         - Fix immediate compilation errors
         - Verify Editor functionality
         “
Phase 3: Package Updates & Compatibility
         - Update URP
         - Update Input System
         - Update Cinemachine
         - Update other Unity packages
         - Fix package-related compilation errors
         “
Phase 4: Test Suite Migration & Fixes
         - Fix test compilation errors
         - Run tests, document failures
         - Fix test failures systematically
         - Achieve test parity with 2021.3
         “
Phase 5: Asset Store Package Updates
         - Verify AAAnimators compatibility
         - Verify Asset Unlock - 3D Prototyping compatibility
         - Update or replace incompatible assets
         “
Phase 6: Modernization & Unity 6 Features
         - Adopt Unity 6 rendering improvements
         - Optimize with Unity 6 APIs
         - Update deprecated code patterns
         “
Phase 7: WebGL Optimization & Final Validation
         - WebGL-specific optimizations
         - Build size optimization
         - Performance profiling
         - Final acceptance testing
```

### Success Criteria (Overall)

The upgrade is complete when:
-  Project opens without errors in Unity 6.0
-  All scripts compile successfully
-  All tests pass (52+ test files)
-  WebGL build succeeds
-  Game plays correctly in browser
-  Performance meets or exceeds Unity 2021.3 baseline
-  All critical features functional (combat, AI, UI, audio, rounds)

---

## Testing Strategy

### Test Categories

**EditMode Tests (Unit Tests)**
- Fast, no Unity runtime required
- Test ScriptableObjects, data classes, utility functions
- Should pass first after upgrade

**PlayMode Tests (Integration Tests)**
- Require Unity runtime
- Test MonoBehaviour components, physics, scenes
- May fail initially due to runtime changes

**Performance Tests**
- Measure frame rates, memory usage
- Establish baselines
- Identify regressions

**Manual Testing**
- Gameplay flow (character movement, combat, UI)
- WebGL-specific issues
- Visual/audio quality

### Testing Workflow

1. **After Phase 2 (Unity 6 upgrade):**
   - Run EditMode tests (expect many failures)
   - Document failure patterns
   - Fix critical compilation errors

2. **During Phase 3 (package updates):**
   - Run EditMode tests after each package update
   - Track improvement in pass rate
   - Fix package-specific test failures

3. **Phase 4 (dedicated testing):**
   - Fix all test compilation errors
   - Achieve 100% test compilation
   - Fix all test failures systematically
   - Reach test parity with 2021.3

4. **After Phase 5, 6, 7:**
   - Run full test suite as regression check
   - Add new tests for Unity 6-specific features
   - Performance testing in Phase 7

### Test Failure Triage

**Priority 1 (Critical):**
- Combat system tests
- Character controller tests
- Health/damage tests
- Core gameplay tests

**Priority 2 (Important):**
- AI tests
- UI tests
- Audio tests
- Round management tests

**Priority 3 (Nice to have):**
- Performance tests (may need new baselines)
- Edge case tests
- Visual tests

---

## Common Patterns & Conventions

### Conventional Commits Format

All commits must follow this format with HatmanStack as the author:

```
<type>(<scope>): <description>

<body - optional>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
```

**Types:**
- `feat`: New feature or enhancement
- `fix`: Bug fix
- `refactor`: Code restructuring without behavior change
- `test`: Test-related changes
- `chore`: Tooling, dependencies, project maintenance
- `docs`: Documentation updates
- `perf`: Performance improvements

**Scopes:**
- `migration`: Upgrade-related changes
- `urp`: Universal Render Pipeline changes
- `input`: Input System changes
- `tests`: Test suite updates
- `combat`, `ai`, `ui`, `audio`: Component-specific changes

**Examples:**
```
fix(migration): resolve URP shader compilation errors

Updated custom shaders to Unity 6 URP API

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

```
test(combat): fix CharacterCombatTests for Unity 6

Updated test assertions to match new physics behavior

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

### Code Migration Patterns

**Pattern 1: Obsolete API Detection**
- Unity 6 will show warnings/errors for obsolete APIs
- Use Unity's API Updater when possible (Edit > Project Settings > Editor)
- Search for deprecation warnings in Console
- Replace obsolete APIs with modern equivalents

**Pattern 2: Namespace Changes**
- Input System may have namespace changes
- URP namespaces may have changed
- Update using statements systematically

**Pattern 3: Serialization Field Changes**
- Some Unity types may serialize differently
- Check for serialization warnings
- May need to reassign inspector references

**Pattern 4: Physics/Rendering Changes**
- URP rendering may look different
- Physics behavior may have subtle changes
- Use tests to validate behavior consistency
- Adjust parameters if needed

### Common Pitfalls to Avoid

1. **Don't skip backups**
   - Always commit before major changes
   - Maintain external backups
   - Test rollback procedure

2. **Don't fix everything at once**
   - Fix compilation errors systematically (file by file, component by component)
   - Use TODO comments for deferred fixes
   - Track technical debt

3. **Don't ignore warnings**
   - Warnings often indicate future problems
   - Fix deprecation warnings as you encounter them
   - Clean console output is the goal

4. **Don't skip testing**
   - Run tests frequently
   - Fix broken tests immediately
   - Don't accumulate test debt

5. **Don't forget WebGL**
   - Test WebGL builds at milestones
   - WebGL has unique constraints
   - Editor success ` WebGL success

6. **Don't commit broken code**
   - Each commit should compile
   - Tests can fail, but code must compile
   - Use `git add -p` for surgical commits

---

## Known Unity 6.0 Breaking Changes (Summary)

Based on Unity upgrade guides, expect changes in these areas:

### URP (Universal Render Pipeline)

- **Shader API changes**: Custom shaders may need updates
- **Renderer Feature API**: Possible breaking changes to render features
- **Post-processing**: May have API changes
- **Lighting**: New lighting model may affect appearance
- **Expected Impact**: Medium (minimal custom shaders in this project)

### Input System

- **API stability**: Input System is mature, expect minor changes
- **Device support**: Improved gamepad support (not relevant for WebGL)
- **Expected Impact**: Low to Medium

### Cinemachine

- **Version jump**: Likely Cinemachine 3.x in Unity 6
- **API changes**: Possible component naming changes
- **Expected Impact**: Low (standard camera usage)

### Physics

- **PhysX updates**: Possible subtle behavior changes
- **Collision detection**: May be more accurate (affects combat hitboxes)
- **Expected Impact**: Low to Medium (test suite will catch issues)

### WebGL

- **WASM updates**: Improved WebAssembly support
- **Threading**: Better worker support (may enable optimizations)
- **Build size**: Possibly smaller builds with better compression
- **Expected Impact**: Positive (performance improvements expected)

### C# and Scripting

- **API Updater**: Will handle most automatic migrations
- **Obsolete APIs**: Some Unity 2021 APIs may be removed
- **Expected Impact**: Low (API Updater handles most cases)

---

## Reference Information

### Current Project State (Unity 2021.3.45f2)

**Core Packages:**
- `com.unity.render-pipelines.universal`: 12.1.15
- `com.unity.inputsystem`: 1.7.0
- `com.unity.cinemachine`: 2.10.1
- `com.unity.textmeshpro`: 3.0.6
- `com.unity.probuilder`: 5.2.3
- `com.unity.timeline`: 1.6.5
- `com.unity.test-framework`: 1.1.33

**Project Structure:**
- **Game Code**: `Assets/Knockout/Scripts/` (AI, Audio, Characters, Combat, UI, Systems)
- **Tests**: `Assets/Knockout/Tests/` (EditMode, PlayMode, Integration, Performance)
- **Third-Party**: AAAnimators Boxing, Asset Unlock - 3D Prototyping, StarterAssets
- **Scenes**: MainScene, Sample, greybox, test
- **Platform**: WebGL only

**Test Coverage:**
- 52+ test files
- EditMode tests (unit tests for data classes, state machines, AI)
- PlayMode tests (integration tests for components, systems)
- Performance tests
- UI tests
- Integration tests (combat, defense, stamina, combos)

**Architecture:**
- Component-based character system
- State machine for combat and AI
- Event-driven UI
- ScriptableObject-based data (AttackData, CharacterStats, ComboData, etc.)

### Unity 6.0 Target State (Estimated)

**Expected Package Versions:**
- `com.unity.render-pipelines.universal`: 17.x or 18.x (Unity 6 URP)
- `com.unity.inputsystem`: 1.8.x or higher
- `com.unity.cinemachine`: 3.x (major version bump)
- `com.unity.textmeshpro`: 3.2.x or 4.x
- `com.unity.probuilder`: 6.x
- `com.unity.timeline`: 1.8.x or 2.x
- `com.unity.test-framework`: Latest

**New Capabilities to Explore (Phase 6):**
- Improved URP rendering (better lighting, shadows)
- GPU Resident Drawer (performance)
- Shader Graph improvements
- Better WebGL performance
- Improved build size optimization

---

## Preparation Checklist (Before Phase 1)

Before beginning Phase 1, ensure:

- [ ] Unity Hub is installed and up to date
- [ ] Unity 6.0 LTS is installed via Unity Hub
- [ ] Unity 2021.3.45f2 remains installed (for rollback)
- [ ] Git is configured and working
- [ ] All changes in current branch are committed
- [ ] You understand the commit message format (conventional commits)
- [ ] You have sufficient disk space (25-30GB free)
- [ ] You have a backup strategy (external drive or cloud backup)
- [ ] You have read this Phase-0 document completely

---

## Next Steps

Once you've read and understood this foundation document:

1. Verify Unity 6.0 is installed
2. Proceed to [Phase-1.md](Phase-1.md) - Pre-Upgrade Preparation
3. Do not skip ahead - phases build on each other

**Estimated Time for Entire Upgrade:** 2-3 weeks (given test coverage and normal priority timeline)

**Estimated Time Per Phase:**
- Phase 1: 2-4 hours (preparation and baselines)
- Phase 2: 4-8 hours (upgrade and immediate fixes)
- Phase 3: 8-16 hours (package updates)
- Phase 4: 16-24 hours (test migration - largest phase)
- Phase 5: 4-8 hours (Asset Store packages)
- Phase 6: 8-12 hours (modernization)
- Phase 7: 4-8 hours (WebGL optimization)

**Total: 46-80 hours of focused work**

---

## Questions or Issues?

If you encounter scenarios not covered in this plan:
- Check Unity 6.0 Upgrade Guide: https://docs.unity3d.com/Manual/UpgradeGuides.html
- Check package-specific upgrade guides (URP, Input System, Cinemachine)
- Use Unity forums or documentation for specific error messages
- Consider creating an issue or discussion in project repository
