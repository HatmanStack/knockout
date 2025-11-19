# Phase 4: Test Suite Migration & Fixes

## Phase Goal

Systematically migrate and fix the comprehensive test suite (52+ test files) to work with Unity 6.0. This phase focuses on achieving test parity with Unity 2021.3 by fixing test compilation errors first, then addressing test failures. The test suite serves as both validation and a guide for remaining production code issues.

**Success Criteria:**
- All test files compile successfully (EditMode and PlayMode)
- Test pass rate matches or exceeds Unity 2021.3 baseline
- No critical test failures in core systems (combat, AI, characters)
- Test execution completes without crashes
- Test results documented and tracked

**Estimated tokens:** ~32,000

---

## Prerequisites

- Phase 3 complete (packages updated, gameplay functional)
- Unity 6 project compiles with zero errors
- Core gameplay systems working
- Git tag "post-package-updates" exists

---

## Tasks

### Task 1: Establish Test Baseline and Document Current State

**Goal:** Run test suite and document current test pass/fail state as baseline

**Files to Modify/Create:**
- `docs/migration/TEST_BASELINE.md` (new file)

**Prerequisites:**
- Phase 3 complete

**Implementation Steps:**

1. Open Unity Test Runner (Window > General > Test Runner)
2. Switch to EditMode tab
3. Click "Run All" for EditMode tests
4. Document results:
   - Total EditMode tests
   - Passing EditMode tests
   - Failing EditMode tests
   - Compilation errors (tests that won't run)
5. Switch to PlayMode tab
6. Click "Run All" for PlayMode tests
7. Document results:
   - Total PlayMode tests
   - Passing PlayMode tests
   - Failing PlayMode tests
   - Compilation errors
8. Create `docs/migration/TEST_BASELINE.md` with:
   - Current test counts (EditMode and PlayMode)
   - Pass/fail rates
   - List of tests with compilation errors
   - List of failing tests (grouped by system)
   - Comparison to Unity 2021.3 baseline (from Phase 1)
9. Categorize test failures by system:
   - Combat tests
   - Character tests
   - AI tests
   - UI tests
   - Audio tests
   - Integration tests
   - Performance tests

**Verification Checklist:**
- [ ] TEST_BASELINE.md created
- [ ] EditMode test results documented
- [ ] PlayMode test results documented
- [ ] Compilation errors listed
- [ ] Failing tests categorized by system
- [ ] Comparison to Unity 2021.3 baseline included

**Testing Instructions:**
- Run Test Runner for both EditMode and PlayMode
- Verify documentation matches actual results
- Check categorization is logical
- Confirm baseline is comprehensive

**Commit Message Template:**
```
test(migration): establish Unity 6 test baseline

Documented current test state:
- EditMode: X/Y passing
- PlayMode: X/Y passing
Compilation errors: Z tests

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~3,500

---

### Task 2: Fix EditMode Test Compilation Errors

**Goal:** Resolve all compilation errors in EditMode tests (unit tests)

**Files to Modify/Create:**
- EditMode test files in `Assets/Knockout/Tests/EditMode/`

**Prerequisites:**
- Task 1 complete (baseline documented)

**Implementation Steps:**

1. Review TEST_BASELINE.md for EditMode compilation errors
2. Prioritize by system:
   - Character data tests (CharacterDataTests, AttackDataTests, etc.)
   - Combat tests (CombatStateMachineTests, HitDataTests)
   - AI tests (AIStateMachineTests, AITargetDetectorTests)
   - Defense tests (DodgeDataTests, ParryDataTests)
   - Other EditMode tests
3. For each test file with compilation errors:
   - Open in IDE
   - Fix namespace issues (using statements)
   - Update Test Framework API calls if changed
   - Fix obsolete Unity APIs in test code
   - Update assertion methods if Test Framework changed
   - Save and verify compilation in Unity
4. Common test compilation issues:
   - NUnit API changes (unlikely but check)
   - Unity Test Framework namespace changes
   - Unity API changes in test setup/teardown
   - Mock object creation issues
   - Test attribute changes
5. Fix tests incrementally and commit every 5-10 files
6. Run Test Runner after each batch to verify tests compile and can execute

**Verification Checklist:**
- [ ] All EditMode tests compile
- [ ] Zero compilation errors in EditMode tab
- [ ] Test Runner shows all EditMode tests (not hidden due to errors)
- [ ] Tests can be executed (even if they fail)

**Testing Instructions:**
- Open Test Runner EditMode tab
- Verify all tests listed (no compilation errors)
- Try running a few tests to confirm they execute
- Check Console for test compilation errors (should be zero)

**Commit Message Template:**
```
fix(tests): resolve EditMode test compilation errors

Fixed namespace and API issues in EditMode tests
All EditMode tests now compile successfully

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,500

---

### Task 3: Fix PlayMode Test Compilation Errors

**Goal:** Resolve all compilation errors in PlayMode tests (integration tests)

**Files to Modify/Create:**
- PlayMode test files in `Assets/Knockout/Tests/PlayMode/`

**Prerequisites:**
- Task 2 complete (EditMode tests compile)

**Implementation Steps:**

1. Review TEST_BASELINE.md for PlayMode compilation errors
2. Prioritize by system:
   - Character component tests (CharacterControllerTests, CharacterCombatTests, etc.)
   - Combat state tests (ExhaustedStateTests, SpecialKnockdownStateTests)
   - Defense tests (CharacterDodgeTests, CharacterParryTests)
   - Integration tests (ComboIntegrationTests, DefenseIntegrationTests)
   - UI tests (HealthBarUITests, RoundUITests)
   - Performance tests
3. For each PlayMode test file with compilation errors:
   - Open in IDE
   - Fix namespace issues
   - Update Test Framework API calls
   - Fix Unity MonoBehaviour test patterns
   - Update scene loading if API changed
   - Fix GameObject creation/destruction patterns
   - Update assertion methods
   - Save and verify compilation
4. Common PlayMode test issues:
   - Scene loading API changes
   - GameObject instantiation in tests
   - Physics simulation in tests (Unity 6 physics changes)
   - Input System testing patterns
   - Coroutine-based test patterns
5. Fix tests incrementally and commit every 5-10 files
6. Run Test Runner after each batch to verify compilation

**Verification Checklist:**
- [ ] All PlayMode tests compile
- [ ] Zero compilation errors in PlayMode tab
- [ ] Test Runner shows all PlayMode tests
- [ ] Tests can be executed (even if they fail)

**Testing Instructions:**
- Open Test Runner PlayMode tab
- Verify all tests listed (no compilation errors)
- Try running a few tests to confirm they execute
- Check Console for test compilation errors (should be zero)

**Commit Message Template:**
```
fix(tests): resolve PlayMode test compilation errors

Fixed integration test compilation issues
All PlayMode tests now compile successfully

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,500

---

### Task 4: Run Full Test Suite and Document Failures

**Goal:** Execute all tests and comprehensively document which tests pass and fail

**Files to Modify/Create:**
- `docs/migration/TEST_RESULTS_UNITY6.md` (new file)

**Prerequisites:**
- Tasks 2-3 complete (all tests compile)

**Implementation Steps:**

1. Open Test Runner
2. Run ALL EditMode tests:
   - Click "Run All" in EditMode tab
   - Wait for completion (may take several minutes)
   - Document results
3. Run ALL PlayMode tests:
   - Click "Run All" in PlayMode tab
   - Wait for completion (may take 10-30 minutes given 52+ test files)
   - Document results
4. Create `docs/migration/TEST_RESULTS_UNITY6.md` with:
   - Total tests: X
   - Passing tests: Y
   - Failing tests: Z
   - Pass rate: Y/X (percentage)
5. Group failing tests by category:
   - **Critical failures:** Core combat, character controller, health
   - **High priority:** AI, input, movement
   - **Medium priority:** UI, audio, stamina
   - **Low priority:** Performance tests, edge cases
6. For each failing test, document:
   - Test name and file
   - Failure reason (from error message)
   - Likely cause (API change, behavior change, etc.)
   - Priority (critical/high/medium/low)
7. Identify patterns in failures (e.g., all physics tests failing ’ physics API change)
8. Compare to Unity 2021.3 baseline from Phase 1

**Verification Checklist:**
- [ ] All EditMode tests executed
- [ ] All PlayMode tests executed
- [ ] TEST_RESULTS_UNITY6.md created
- [ ] Passing/failing counts documented
- [ ] Failing tests categorized by priority
- [ ] Failure patterns identified

**Testing Instructions:**
- Run full test suite and verify execution completes
- Check TEST_RESULTS_UNITY6.md for accuracy
- Verify categorization makes sense
- Confirm priorities align with project needs

**Commit Message Template:**
```
test(migration): document Unity 6 test results

Test suite results:
- Total: X tests
- Passing: Y tests (Z%)
- Failing: W tests

Categorized failures by priority for systematic fixing

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,000

---

### Task 5: Fix Critical Test Failures (Combat & Character Core)

**Goal:** Fix test failures in critical systems: combat, character controller, health

**Files to Modify/Create:**
- Production code in `Assets/Knockout/Scripts/Characters/`
- Production code in `Assets/Knockout/Scripts/Combat/`
- Test files if test logic needs updating

**Prerequisites:**
- Task 4 complete (failures documented)

**Implementation Steps:**

1. Review TEST_RESULTS_UNITY6.md for critical failures
2. Focus on core combat and character tests:
   - CharacterCombatTests
   - CharacterControllerTests
   - CharacterHealthTests
   - CombatStateMachineTests
   - HitDataTests
3. For each failing test:
   - Run the specific test in Test Runner
   - Read the failure message carefully
   - Determine if failure is due to:
     - Production code needs fixing (Unity 6 API change)
     - Test expectations need updating (behavior changed in Unity 6)
     - Test setup/teardown issue
   - Fix the issue (prefer fixing production code over changing test expectations)
   - Re-run test to verify fix
   - Move to next failing test
4. Common issues to expect:
   - Physics behavior changes (collision detection, forces)
   - Timing changes (frame-based operations)
   - GameObject lifecycle changes
   - Component initialization order changes
5. Fix tests systematically by file/component
6. Commit after each component's tests are fixed
7. Re-run full critical test suite after all fixes to verify no regressions

**Verification Checklist:**
- [ ] All critical combat tests passing
- [ ] All critical character tests passing
- [ ] CharacterController tests passing
- [ ] CharacterCombat tests passing
- [ ] CharacterHealth tests passing
- [ ] CombatStateMachine tests passing
- [ ] No regressions in previously passing tests

**Testing Instructions:**
- Run critical tests individually to isolate issues
- Verify each fix doesn't break other tests
- Run full critical suite at end to verify all pass
- Test in Play mode to verify production code still works

**Commit Message Template:**
```
fix(combat): resolve Unity 6 combat and character test failures

Fixed critical test failures:
- CharacterCombat: [specific fixes]
- CombatStateMachine: [specific fixes]
All critical tests now passing

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~5,000

---

### Task 6: Fix High Priority Test Failures (AI, Input, Movement)

**Goal:** Fix test failures in high-priority systems: AI, input, movement

**Files to Modify/Create:**
- Production code in `Assets/Knockout/Scripts/AI/`
- Production code in `Assets/Knockout/Scripts/Characters/Components/`
- Test files if needed

**Prerequisites:**
- Task 5 complete (critical tests passing)

**Implementation Steps:**

1. Review TEST_RESULTS_UNITY6.md for high priority failures
2. Focus on AI, input, and movement tests:
   - AIStateMachineTests
   - AIStatesTests (ApproachState, AttackState, etc.)
   - CharacterInputTests
   - CharacterMovementTests
   - AITargetDetectorTests
3. For each failing test:
   - Run test and review failure
   - Identify root cause
   - Fix production code or update test expectations
   - Verify fix
4. Common AI issues:
   - State machine transitions may behave differently
   - AI decision-making timing changes
   - Target detection physics changes
5. Common Input issues:
   - Input System API behavior changes
   - Callback timing differences
   - Input value reading changes
6. Common Movement issues:
   - Physics-based movement behavior changes
   - Character controller physics updates
   - Transform manipulation timing
7. Fix systematically and commit per component
8. Re-run high priority test suite to verify all passing

**Verification Checklist:**
- [ ] All AI tests passing
- [ ] AIStateMachine tests passing
- [ ] AI state tests passing (Approach, Attack, Defend, etc.)
- [ ] CharacterInput tests passing
- [ ] CharacterMovement tests passing
- [ ] No regressions in critical tests

**Testing Instructions:**
- Run AI tests and verify AI behavior in Play mode
- Test input responsiveness
- Verify movement feels correct
- Check for regressions in critical systems

**Commit Message Template:**
```
fix(ai,input): resolve Unity 6 AI and input test failures

Fixed high priority test failures:
- AIStateMachine: [specific fixes]
- CharacterInput: [specific fixes]
- CharacterMovement: [specific fixes]

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~5,000

---

### Task 7: Fix Medium Priority Test Failures (UI, Audio, Stamina)

**Goal:** Fix test failures in medium-priority systems: UI, audio, stamina, combos

**Files to Modify/Create:**
- Production code in `Assets/Knockout/Scripts/UI/`
- Production code in `Assets/Knockout/Scripts/Audio/`
- Production code for stamina and combo systems
- Test files if needed

**Prerequisites:**
- Task 6 complete (high priority tests passing)

**Implementation Steps:**

1. Review TEST_RESULTS_UNITY6.md for medium priority failures
2. Focus on UI, audio, and secondary systems:
   - HealthBarUITests, StaminaBarUITests
   - RoundUITests, ComboCounterUITests
   - AudioManagerTests, CharacterAudioPlayerTests
   - CharacterStaminaTests
   - CharacterComboTrackerTests
   - SpecialMovesTests
3. For each failing test:
   - Run test and review failure
   - Identify root cause
   - Fix production code or update test expectations
   - Verify fix
4. Common UI issues:
   - TextMesh Pro API changes (unlikely but check)
   - UI layout changes
   - Canvas rendering changes
5. Common Audio issues:
   - Audio source API changes
   - Audio manager timing
6. Common stamina/combo issues:
   - Timing-based calculations
   - State tracking
7. Fix systematically and commit per system
8. Re-run medium priority test suite to verify all passing

**Verification Checklist:**
- [ ] All UI tests passing
- [ ] HealthBar, StaminaBar, RoundUI tests passing
- [ ] Audio tests passing
- [ ] Stamina system tests passing
- [ ] Combo system tests passing
- [ ] Special moves tests passing
- [ ] No regressions in critical or high priority tests

**Testing Instructions:**
- Run UI tests and verify UI displays correctly in Play mode
- Test audio plays correctly
- Verify stamina drains and recovers
- Test combo system functionality

**Commit Message Template:**
```
fix(ui,audio): resolve Unity 6 UI and audio test failures

Fixed medium priority test failures:
- UI systems: [specific fixes]
- Audio systems: [specific fixes]
- Stamina/Combo: [specific fixes]

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,500

---

### Task 8: Fix Integration and Performance Tests

**Goal:** Fix integration tests and establish new performance baselines

**Files to Modify/Create:**
- Integration test files in `Assets/Knockout/Tests/PlayMode/Integration/`
- Performance test files
- Production code if integration issues found

**Prerequisites:**
- Task 7 complete (medium priority tests passing)

**Implementation Steps:**

1. Focus on integration tests:
   - ComboIntegrationTests
   - DefenseIntegrationTests
   - GameplayIntegrationTests
   - StaminaIntegrationTests
   - UIIntegrationTests
2. Integration tests verify multiple systems working together:
   - May fail if individual system behaviors changed slightly
   - Focus on overall integration, not exact behavior matching
   - Update test expectations if Unity 6 behavior is reasonable but different
3. For each failing integration test:
   - Run test and observe full execution
   - Determine if failure is:
     - Real integration bug (fix production code)
     - Changed behavior that's acceptable (update test expectations)
     - Test timing issue (adjust test waits/delays)
   - Fix appropriately
4. Focus on performance tests:
   - PerformanceTests may need new baselines
   - Unity 6 performance may differ from 2021.3
   - Update performance expectations if Unity 6 is reasonable
   - Document performance changes (improvement or regression)
5. Fix integration tests systematically
6. Re-run all integration tests to verify
7. Document new performance baselines in TEST_RESULTS_UNITY6.md

**Verification Checklist:**
- [ ] All integration tests passing or documented as expected differences
- [ ] ComboIntegration tests passing
- [ ] DefenseIntegration tests passing
- [ ] GameplayIntegration tests passing
- [ ] Performance tests passing or have updated baselines
- [ ] Performance changes documented

**Testing Instructions:**
- Run integration tests and observe full execution
- Play game manually to verify integration feels correct
- Check performance in Play mode
- Compare to Unity 2021.3 performance baseline

**Commit Message Template:**
```
fix(tests): resolve integration and performance test issues

Fixed integration tests, updated performance baselines
All integration tests now passing

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,500

---

### Task 9: Achieve Test Parity and Final Validation

**Goal:** Run full test suite and achieve test parity with Unity 2021.3 baseline

**Files to Modify/Create:**
- Any remaining test or production code files
- `docs/migration/TEST_PARITY_ACHIEVED.md` (new file)

**Prerequisites:**
- Tasks 2-8 complete (all major test categories fixed)

**Implementation Steps:**

1. Run complete test suite (EditMode + PlayMode)
2. Compare results to Unity 2021.3 baseline from Phase 1
3. Target: Match or exceed Unity 2021.3 pass rate
4. For any remaining failing tests:
   - Triage: Critical bug or acceptable difference?
   - Fix critical bugs
   - Document acceptable differences
   - Consider temporarily disabling tests that can't be quickly fixed (add TODO)
5. Run full test suite multiple times to verify stability:
   - Tests should pass consistently
   - No flaky tests
   - No intermittent failures
6. Create `docs/migration/TEST_PARITY_ACHIEVED.md`:
   - Final test counts (EditMode and PlayMode)
   - Pass rate comparison (Unity 2021.3 vs Unity 6)
   - List of any disabled tests with justification
   - List of acceptable behavioral differences
   - Performance comparison
7. Verify test execution time is reasonable (should be similar to Unity 2021.3)
8. Celebrate achieving test parity! <‰

**Verification Checklist:**
- [ ] Test pass rate meets or exceeds Unity 2021.3 baseline
- [ ] All critical and high priority tests passing
- [ ] Medium priority tests passing or differences documented
- [ ] Integration tests passing
- [ ] Test suite runs stably (no flaky tests)
- [ ] TEST_PARITY_ACHIEVED.md created and comprehensive

**Testing Instructions:**
- Run full test suite 3 times to verify stability
- Compare pass rates to Unity 2021.3
- Verify no intermittent failures
- Check test execution time is reasonable

**Commit Message Template:**
```
test(migration): achieve test parity with Unity 2021.3

 Test suite fully migrated to Unity 6
 Pass rate: X% (Unity 2021.3: Y%)
 All critical systems tested and passing

Test migration complete!

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,500

---

### Task 10: Tag Test Suite Completion

**Goal:** Create git checkpoint after successful test suite migration

**Files to Modify/Create:**
- `docs/migration/UPGRADE_CHECKLIST.md` (update)

**Prerequisites:**
- Task 9 complete (test parity achieved)

**Implementation Steps:**

1. Verify all test changes committed
2. Create git tag "post-test-migration"
3. Update `docs/migration/UPGRADE_CHECKLIST.md`:
   - Mark Phase 4 complete
   - Note test pass rate achieved
   - Document any known test issues
4. Create Phase 4 summary:
   - Tests fixed (count)
   - Production code changes made
   - Test parity status
   - Time spent
5. Commit documentation updates

**Verification Checklist:**
- [ ] All test changes committed
- [ ] Git tag "post-test-migration" exists
- [ ] UPGRADE_CHECKLIST.md updated
- [ ] Phase 4 summary documented

**Testing Instructions:**
- Run `git tag` and verify tag exists
- Check git status is clean
- Run test suite one final time to verify

**Commit Message Template:**
```
chore(migration): complete Phase 4 test migration

 All tests compile successfully
 Test parity achieved (X% pass rate)
 Critical, high, and medium priority tests passing
 Integration tests passing

Tagged as 'post-test-migration'

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~3,000

---

## Phase Verification

After completing all tasks in Phase 4, verify the following:

### Test Compilation
- [ ] All EditMode tests compile (zero compilation errors)
- [ ] All PlayMode tests compile (zero compilation errors)
- [ ] Test Runner shows all 52+ tests

### Test Execution
- [ ] All EditMode tests execute
- [ ] All PlayMode tests execute
- [ ] Test suite runs to completion without crashes

### Test Pass Rate
- [ ] Critical tests: 100% passing
- [ ] High priority tests: 100% passing
- [ ] Medium priority tests: e95% passing
- [ ] Overall pass rate meets or exceeds Unity 2021.3 baseline

### Documentation
- [ ] TEST_BASELINE.md documents starting state
- [ ] TEST_RESULTS_UNITY6.md documents all test results
- [ ] TEST_PARITY_ACHIEVED.md documents final state
- [ ] UPGRADE_CHECKLIST.md updated

### Version Control
- [ ] All test and production code changes committed
- [ ] Git tag "post-test-migration" exists
- [ ] Clean git status

---

## Known Limitations and Technical Debt

**Acceptable Differences:**
- Minor performance variations (Unity 6 may be faster or slower in some areas)
- Slight physics behavior differences (collision timing, force application)
- Minor timing differences in integration tests

**Deferred Issues:**
- Third-party asset compatibility (Phase 5)
- Deprecation warnings (Phase 6)
- WebGL build testing (Phase 7)
- Performance optimization (Phase 7)

---

## Next Phase

Once Phase 4 verification is complete:
- Proceed to [Phase-5.md](Phase-5.md) - Asset Store Package Updates
- Phase 5 will verify and update third-party Asset Store packages
- With tests passing, we have confidence to tackle asset compatibility

**Estimated time for Phase 4:** 16-24 hours (largest phase due to 52+ test files)

**This is the most time-consuming phase but provides the highest confidence in the upgrade quality.**
