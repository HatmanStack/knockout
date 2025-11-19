# Phase 1: Pre-Upgrade Preparation

## Phase Goal

Prepare the project for Unity 6.0 upgrade by creating backups, auditing dependencies, documenting the current state, and establishing performance baselines. This phase ensures we have a safe rollback point and objective metrics to measure upgrade success.

**Success Criteria:**
- Project fully backed up (git + external backup)
- Current state documented (packages, settings, assets)
- Performance baselines established (build size, load time, frame rate)
- All changes committed to version control
- Unity 2021.3 and Unity 6.0 both installed

**Estimated tokens:** ~18,000

---

## Prerequisites

- Unity 2021.3.45f2 installed and working
- Unity 6.0 LTS installed via Unity Hub
- Git configured and working
- Sufficient disk space (25-30GB free)
- You have read Phase-0.md completely

---

## Tasks

### Task 1: Verify Environment and Unity Versions

**Goal:** Confirm Unity Hub, Unity 2021.3, and Unity 6.0 are properly installed

**Files to Modify/Create:**
- None (verification only)

**Prerequisites:**
- None

**Implementation Steps:**

1. Open Unity Hub and verify Unity 2021.3.45f2 is listed in installed versions
2. Verify Unity 6.0 LTS is installed (if not, install it via Unity Hub)
3. Note the exact Unity 6.0 version number for documentation
4. Verify both versions can be launched from Unity Hub
5. Check available disk space (should have at least 25-30GB free for upgrade process)

**Verification Checklist:**
- [ ] Unity Hub shows Unity 2021.3.45f2 installed
- [ ] Unity Hub shows Unity 6.0.x LTS installed
- [ ] Both Unity versions launch successfully
- [ ] Sufficient disk space available (25-30GB minimum)

**Testing Instructions:**
- Launch Unity 2021.3 and verify it opens to the project list
- Close Unity 2021.3
- Launch Unity 6.0 and verify it opens to the project list (do NOT open the Knockout project yet)
- Close Unity 6.0

**Commit Message Template:**
```
chore(migration): verify Unity 2021.3 and Unity 6.0 installations

Confirmed both Unity versions are installed and functional

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~500

---

### Task 2: Commit All Current Changes

**Goal:** Ensure version control has all current changes before starting upgrade

**Files to Modify/Create:**
- Any uncommitted files in the project

**Prerequisites:**
- Git is configured
- Working in appropriate branch

**Implementation Steps:**

1. Check git status to see any uncommitted changes
2. Review any modified files (several audio files shown in git status)
3. Stage all changes that should be committed
4. Create a commit with a clear message indicating pre-upgrade state
5. Tag this commit as "pre-unity6-upgrade" for easy rollback
6. Verify commit was successful and tag exists

**Verification Checklist:**
- [ ] `git status` shows clean working directory (no uncommitted changes)
- [ ] Git tag "pre-unity6-upgrade" exists
- [ ] Latest commit is visible in git log

**Testing Instructions:**
- Run `git status` - should show "working tree clean"
- Run `git tag` - should show "pre-unity6-upgrade"
- Run `git log --oneline -1` - should show your pre-upgrade commit

**Commit Message Template:**
```
chore(migration): commit all changes before Unity 6.0 upgrade

Pre-upgrade state captured. Tagged as 'pre-unity6-upgrade'

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~1,000

---

### Task 3: Create External Backup

**Goal:** Create a full backup of the project outside of git for catastrophic failure recovery

**Files to Modify/Create:**
- None (backup creation only)

**Prerequisites:**
- Task 2 complete (all changes committed)
- External drive or backup location available

**Implementation Steps:**

1. Determine backup location (external drive, cloud storage, or separate directory)
2. Create a compressed archive of the entire project directory
3. Include the `.git` folder in the backup (preserves full version history)
4. Name the backup with date and Unity version (e.g., `knockout-2021.3.45f2-2024-11-19.tar.gz` or `.zip`)
5. Verify backup was created successfully and is readable
6. Document backup location for future reference

**Verification Checklist:**
- [ ] Backup file created and exists at backup location
- [ ] Backup file size is reasonable (several GB expected)
- [ ] Backup file can be opened/extracted (test extraction to temp folder)
- [ ] Backup location documented

**Testing Instructions:**
- Check backup file exists at documented location
- Verify file size (should be several gigabytes for this project)
- Test extraction: extract to a temporary location and verify folders exist
- Delete temporary extraction after verification

**Commit Message Template:**
```
docs(migration): document external backup location

Created backup: knockout-2021.3.45f2-YYYY-MM-DD.tar.gz
Location: [backup location]

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~1,500

---

### Task 4: Document Current Package Versions

**Goal:** Record all current package versions for comparison after upgrade

**Files to Modify/Create:**
- `docs/migration/CURRENT_STATE.md` (new file to create)

**Prerequisites:**
- Task 2 complete (working directory clean)

**Implementation Steps:**

1. Create `docs/migration/` directory if it doesn't exist
2. Create `CURRENT_STATE.md` file
3. Copy contents of `Packages/manifest.json` into documentation
4. Note Unity version (2021.3.45f2)
5. List all third-party assets (AAAnimators, Asset Unlock, StarterAssets, Elizabeth Warren, etc.)
6. Document active scenes (MainScene, Sample, greybox, test)
7. Note target platform (WebGL only)
8. Document test coverage (52+ test files)

**Verification Checklist:**
- [ ] `docs/migration/CURRENT_STATE.md` exists
- [ ] File contains Unity version (2021.3.45f2)
- [ ] File contains complete package manifest
- [ ] File lists third-party assets
- [ ] File documents platform (WebGL)
- [ ] File notes test coverage

**Testing Instructions:**
- Read `docs/migration/CURRENT_STATE.md`
- Verify all package versions are listed
- Verify third-party assets are documented
- Confirm information is accurate

**Commit Message Template:**
```
docs(migration): document current Unity 2021.3 project state

Recorded packages, assets, platform, and test coverage

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~2,000

---

### Task 5: Establish Performance Baselines

**Goal:** Measure current performance metrics for comparison after upgrade

**Files to Modify/Create:**
- `docs/migration/PERFORMANCE_BASELINE.md` (new file to create)

**Prerequisites:**
- Task 4 complete (documentation structure exists)
- Project opens successfully in Unity 2021.3

**Implementation Steps:**

1. Open project in Unity 2021.3.45f2
2. Create `docs/migration/PERFORMANCE_BASELINE.md` file
3. Run the test suite and record execution time:
   - Open Test Runner (Window > General > Test Runner)
   - Run all EditMode tests, note total time
   - Run all PlayMode tests, note total time
   - Record any test failures (should be zero or minimal)
4. Build WebGL version and record metrics:
   - Build for WebGL (Development Build enabled for profiling)
   - Note build time (how long the build takes)
   - Note build size (total size of Build folder)
   - Note WASM file size specifically
5. Test WebGL build in browser and record performance:
   - Start a local web server to host the build
   - Open in browser (Chrome recommended)
   - Record load time (time until game is interactive)
   - Play for 2-3 minutes and note frame rate (use browser dev tools or Profiler)
   - Record any console errors or warnings
6. Document all metrics in PERFORMANCE_BASELINE.md

**Verification Checklist:**
- [ ] `docs/migration/PERFORMANCE_BASELINE.md` exists
- [ ] Test suite execution times recorded (EditMode and PlayMode)
- [ ] WebGL build size recorded
- [ ] WebGL build time recorded
- [ ] WebGL load time recorded
- [ ] Frame rate metrics recorded
- [ ] Any issues/warnings documented

**Testing Instructions:**
- Review PERFORMANCE_BASELINE.md for completeness
- Verify all metrics are present
- Confirm metrics are reasonable (no obvious errors)

**Commit Message Template:**
```
docs(migration): establish Unity 2021.3 performance baselines

Recorded test suite times, build size, load time, and frame rate

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~3,000

---

### Task 6: Audit Third-Party Assets for Unity 6 Compatibility

**Goal:** Research whether third-party Asset Store packages support Unity 6.0

**Files to Modify/Create:**
- `docs/migration/ASSET_COMPATIBILITY.md` (new file to create)

**Prerequisites:**
- Task 4 complete (assets documented)

**Implementation Steps:**

1. Create `docs/migration/ASSET_COMPATIBILITY.md` file
2. For each third-party asset, research Unity 6.0 compatibility:
   - **AAAnimators Boxing**: Check Asset Store page for Unity 6 support
   - **Asset Unlock - 3D Prototyping**: Check Asset Store page
   - **StarterAssets**: Check if Unity has updated version
   - **Elizabeth Warren caricature**: Custom asset (likely compatible)
3. Document findings for each asset:
   - Is it compatible with Unity 6.0?
   - Is an update available?
   - Last update date
   - Any known issues reported
   - Recommended action (keep, update, replace, remove)
4. Identify any assets that may block the upgrade
5. Research alternatives for incompatible assets if needed

**Verification Checklist:**
- [ ] `docs/migration/ASSET_COMPATIBILITY.md` exists
- [ ] All third-party assets researched
- [ ] Compatibility status documented for each
- [ ] Action plan for each asset (keep/update/replace/remove)
- [ ] Any blockers identified
- [ ] Alternatives researched for incompatible assets

**Testing Instructions:**
- Review ASSET_COMPATIBILITY.md
- Verify all assets are covered
- Confirm action plan is reasonable
- Check that no critical blockers exist

**Commit Message Template:**
```
docs(migration): audit third-party asset Unity 6 compatibility

Researched AAAnimators, Asset Unlock, and other assets

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~2,500

---

### Task 7: Create Migration Checklist

**Goal:** Create a comprehensive checklist for tracking upgrade progress

**Files to Modify/Create:**
- `docs/migration/UPGRADE_CHECKLIST.md` (new file to create)

**Prerequisites:**
- Tasks 1-6 complete (full understanding of project state)

**Implementation Steps:**

1. Create `docs/migration/UPGRADE_CHECKLIST.md` file
2. Structure checklist by phase (Phase 1 through Phase 7)
3. Include high-level milestones:
   - Pre-upgrade preparation complete
   - Unity 6 upgrade complete
   - Packages updated
   - Tests passing
   - Assets compatible
   - Modernization complete
   - WebGL optimized
4. Include rollback checkpoints (where to tag commits for easy revert)
5. Include verification criteria for each phase
6. Add notes section for tracking issues encountered

**Verification Checklist:**
- [ ] `docs/migration/UPGRADE_CHECKLIST.md` exists
- [ ] Checklist covers all 7 phases
- [ ] Rollback checkpoints identified
- [ ] Verification criteria included
- [ ] Notes section exists for tracking issues

**Testing Instructions:**
- Review checklist for completeness
- Verify all phases represented
- Confirm checklist is actionable
- Check that rollback points are clear

**Commit Message Template:**
```
docs(migration): create Unity 6.0 upgrade tracking checklist

Added checklist for all 7 phases with rollback points

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~2,000

---

### Task 8: Verify Project Opens in Unity 2021.3

**Goal:** Final verification that Unity 2021.3 project is healthy before upgrade

**Files to Modify/Create:**
- None (verification only)

**Prerequisites:**
- Tasks 1-7 complete

**Implementation Steps:**

1. Open project in Unity 2021.3.45f2
2. Verify no console errors on project load
3. Open main scene (MainScene.unity) and verify it loads without errors
4. Enter Play mode and verify game starts
5. Run a quick gameplay test (move character, attack, test UI)
6. Exit Play mode cleanly
7. Run test suite (EditMode and PlayMode) to verify baseline health:
   - All or nearly all tests should pass
   - Document any pre-existing test failures
8. Close Unity Editor
9. Confirm all verification passed before proceeding to Phase 2

**Verification Checklist:**
- [ ] Project opens in Unity 2021.3 without errors
- [ ] MainScene loads successfully
- [ ] Play mode works (game is playable)
- [ ] Test suite runs (document pass/fail rate)
- [ ] No critical console errors
- [ ] Editor closes cleanly

**Testing Instructions:**
- Review console for any errors during project open
- Play game for 1-2 minutes and verify core functionality
- Run Test Runner for EditMode and PlayMode tests
- Document test pass rate in preparation notes

**Commit Message Template:**
```
test(migration): verify Unity 2021.3 project health before upgrade

Confirmed project opens, plays, and tests pass

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~2,500

---

## Phase Verification

After completing all tasks in Phase 1, verify the following:

### Documentation Created
- [ ] `docs/migration/CURRENT_STATE.md` exists with complete package and asset inventory
- [ ] `docs/migration/PERFORMANCE_BASELINE.md` exists with measurable metrics
- [ ] `docs/migration/ASSET_COMPATIBILITY.md` exists with compatibility research
- [ ] `docs/migration/UPGRADE_CHECKLIST.md` exists with tracking structure

### Backups and Version Control
- [ ] All changes committed to git
- [ ] Git tag "pre-unity6-upgrade" exists
- [ ] External backup created and verified
- [ ] Backup location documented

### Environment Verified
- [ ] Unity 2021.3.45f2 installed and functional
- [ ] Unity 6.0 LTS installed and functional
- [ ] Sufficient disk space available (25-30GB free)
- [ ] Project opens and runs successfully in Unity 2021.3

### Performance Baselines Established
- [ ] Test suite execution time recorded
- [ ] WebGL build size recorded
- [ ] WebGL load time recorded
- [ ] Frame rate metrics recorded

### Readiness Confirmed
- [ ] No critical issues blocking upgrade
- [ ] Third-party asset compatibility understood
- [ ] Rollback procedure documented and tested
- [ ] Ready to proceed to Phase 2

---

## Known Limitations and Technical Debt

**Current Project State:**
- Git status shows several modified audio WAV files - these should be committed before upgrade
- Some test files may have pre-existing failures - document these as baseline
- Third-party assets may not all support Unity 6.0 immediately

**Mitigation:**
- Commit all changes before proceeding
- Document current test pass rate as baseline
- Phase 5 will address third-party asset compatibility

---

## Next Phase

Once Phase 1 verification is complete:
- Proceed to [Phase-2.md](Phase-2.md) - Unity 6 Upgrade Execution
- Do NOT skip Phase 1 verification - it provides critical rollback safety

**Estimated time for Phase 1:** 2-4 hours (mostly documentation and baseline establishment)
