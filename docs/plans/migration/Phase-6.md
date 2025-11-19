# Phase 6: Modernization & Unity 6 Features

## Phase Goal

Adopt Unity 6-specific features and modernize the codebase by replacing deprecated APIs, implementing performance improvements, and leveraging new Unity 6 capabilities. This phase transforms the project from "compatible with Unity 6" to "optimized for Unity 6."

**Success Criteria:**
- All deprecation warnings resolved
- Deprecated code patterns replaced with modern equivalents
- Unity 6 rendering improvements implemented (URP enhancements)
- Performance optimizations applied
- Code quality improved
- Developer experience enhanced with Unity 6 tools
- Project ready for final WebGL optimization

**Estimated tokens:** ~28,000

---

## Prerequisites

- Phase 5 complete (assets compatible)
- All tests passing
- Game fully functional
- Git tag "post-asset-updates" exists

---

## Tasks

### Task 1: Audit and Document Deprecation Warnings

**Goal:** Identify all deprecation warnings and obsolete API usage

**Files to Modify/Create:**
- `docs/migration/DEPRECATION_AUDIT.md` (new file)

**Prerequisites:**
- Phase 5 complete

**Implementation Steps:**

1. Clear Unity Console
2. Recompile project (Assets > Reimport All)
3. Review Console for warnings (not errors - those should be fixed already)
4. Filter Console to show only warnings
5. Identify deprecation warnings:
   - Look for "deprecated", "obsolete", or "will be removed" messages
   - Note Unity APIs marked for future removal
   - Identify obsolete patterns still in use
6. Create `docs/migration/DEPRECATION_AUDIT.md` with:
   - Total warning count
   - List of deprecation warnings
   - Affected files for each warning
   - Unity's recommended replacement (usually in warning message)
   - Priority (critical for soon-to-be-removed, low for distant future)
7. Group warnings by category:
   - Rendering/URP deprecations
   - Input System deprecations
   - Physics deprecations
   - General Unity API deprecations
8. Also check for non-deprecation warnings worth fixing:
   - Performance warnings
   - Best practice warnings
   - Code quality warnings

**Verification Checklist:**
- [ ] Console warnings reviewed
- [ ] DEPRECATION_AUDIT.md created
- [ ] All deprecation warnings documented
- [ ] Warnings categorized by system
- [ ] Priorities assigned
- [ ] Replacement APIs identified

**Testing Instructions:**
- Clear and recompile to get fresh warning list
- Verify all warnings documented
- Check that priorities are reasonable

**Commit Message Template:**
```
docs(modernization): audit deprecation warnings

Documented all Unity 6 deprecation warnings
Total warnings: X

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~3,500

---

### Task 2: Replace Deprecated APIs

**Goal:** Systematically replace deprecated APIs with Unity 6 equivalents

**Files to Modify/Create:**
- Various `.cs` files with deprecated API usage

**Prerequisites:**
- Task 1 complete (deprecations documented)

**Implementation Steps:**

1. Review DEPRECATION_AUDIT.md for deprecated APIs
2. Prioritize replacements:
   - APIs marked for removal in near future (high priority)
   - APIs with better Unity 6 alternatives (medium priority)
   - APIs deprecated but still functional (low priority)
3. For each deprecated API:
   - Find all uses in codebase (use IDE search or grep)
   - Identify Unity 6 replacement from documentation
   - Replace API calls with modern equivalent
   - Update method signatures if needed
   - Test that functionality still works
   - Verify no new errors introduced
4. Common Unity 6 API modernizations:
   - Old physics APIs ’ new physics APIs
   - Legacy rendering ’ URP rendering patterns
   - Obsolete component access ’ modern patterns
   - Deprecated utility methods ’ new utilities
5. Fix APIs incrementally by category
6. Commit after each category fixed
7. Re-run tests after each batch to ensure no regressions

**Verification Checklist:**
- [ ] All high-priority deprecated APIs replaced
- [ ] Medium-priority deprecated APIs replaced
- [ ] Low-priority deprecated APIs addressed (replaced or documented)
- [ ] Console deprecation warnings significantly reduced
- [ ] Tests still pass after API updates
- [ ] Functionality preserved

**Testing Instructions:**
- Run test suite after each batch of API replacements
- Play game to verify functionality unchanged
- Check Console for reduced warning count
- Verify no new errors introduced

**Commit Message Template:**
```
refactor(modernization): replace deprecated APIs with Unity 6 equivalents

Replaced deprecated APIs:
- [Category 1]: X APIs updated
- [Category 2]: Y APIs updated

Reduced deprecation warnings by Z%

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~5,000

---

### Task 3: Implement Unity 6 URP Rendering Improvements

**Goal:** Adopt Unity 6 URP rendering enhancements for better visual quality

**Files to Modify/Create:**
- URP asset settings in `Settings/` folder
- Renderer settings
- Material properties (if needed)
- Lighting settings in scenes

**Prerequisites:**
- Task 2 complete (deprecated APIs replaced)

**Implementation Steps:**

1. Review Unity 6 URP improvements:
   - Check Unity 6 URP documentation for new features
   - Identify applicable improvements for this project
   - Focus on WebGL-compatible features
2. URP improvements to consider:
   - **Improved lighting:**
     - Check if Unity 6 has better real-time lighting
     - Update light settings if beneficial
   - **Better shadows:**
     - Review shadow quality settings
     - Adjust shadow distance/resolution if needed
   - **Post-processing updates:**
     - Check for new post-processing effects
     - Enable appropriate effects for fighting game aesthetic
   - **Rendering optimizations:**
     - Check URP Renderer settings
     - Enable GPU instancing if not already enabled
     - Review batching settings
3. Update URP Asset settings:
   - Open UniversalRenderPipelineAsset in Inspector
   - Review Quality settings section
   - Adjust Rendering section for Unity 6 best practices
   - Check Lighting section for improvements
   - Update Shadows section if beneficial
4. Update Renderer settings:
   - Open Renderer asset in Inspector
   - Review Rendering Path
   - Check for new Renderer Features in Unity 6
   - Enable beneficial features
5. Test rendering changes:
   - Load scenes and check visual quality
   - Compare to previous rendering
   - Ensure changes are improvements
   - Verify performance not degraded
6. Balance quality vs. WebGL performance (Phase 7 will optimize further)

**Verification Checklist:**
- [ ] URP asset settings reviewed and updated
- [ ] Renderer settings optimized for Unity 6
- [ ] Lighting improved (if applicable)
- [ ] Shadows optimized
- [ ] Visual quality maintained or improved
- [ ] Performance not significantly degraded
- [ ] Tests still pass

**Testing Instructions:**
- Load all scenes and check rendering
- Enter Play mode and verify visual quality
- Compare to Unity 2021.3 screenshots if available
- Check frame rate hasn't dropped significantly
- Verify rendering looks good in WebGL (if testing WebGL builds)

**Commit Message Template:**
```
feat(urp): implement Unity 6 URP rendering improvements

Updated URP settings for Unity 6:
- Enhanced lighting settings
- Optimized shadow quality
- Improved rendering configuration

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~5,000

---

### Task 4: Optimize Code Patterns for Unity 6

**Goal:** Refactor code to use Unity 6 best practices and performance patterns

**Files to Modify/Create:**
- Various `.cs` files with suboptimal patterns

**Prerequisites:**
- Task 3 complete (URP improvements implemented)

**Implementation Steps:**

1. Identify optimization opportunities:
   - Review CLAUDE.md principles (DRY, YAGNI)
   - Look for repetitive code that can be refactored
   - Identify performance bottlenecks (use Profiler if needed)
   - Find code that can leverage Unity 6 improvements
2. Code pattern improvements:
   - **Component caching:**
     - Cache GetComponent calls in Awake/Start
     - Avoid repeated GetComponent in Update
   - **Object pooling:**
     - Consider object pooling for frequently instantiated objects (damage numbers, effects)
     - Reduce GC pressure
   - **Unity 6 Jobs System (if applicable):**
     - Identify CPU-intensive operations
     - Consider Jobs System for parallel processing
     - (May be advanced - only if clear benefit for WebGL)
   - **Physics optimizations:**
     - Review physics update patterns
     - Optimize collision checks
     - Use layer-based collision matrix
3. Refactor systematically:
   - Focus on hot paths (frequently executed code)
   - Optimize combat system (runs every frame during fights)
   - Optimize AI decision-making (can be expensive)
   - Optimize UI updates (only update when needed)
4. Maintain code quality:
   - Follow DRY principle
   - Keep refactors small and testable
   - Run tests after each refactor
   - Document complex optimizations
5. Use Unity 6 debugging tools:
   - Profiler to identify bottlenecks
   - Frame Debugger to analyze rendering
   - Memory Profiler to check allocations

**Verification Checklist:**
- [ ] Component caching implemented where beneficial
- [ ] Object pooling added for frequently created objects (if needed)
- [ ] Hot path code optimized
- [ ] Combat system optimized
- [ ] AI system optimized
- [ ] UI update logic optimized
- [ ] Tests pass after optimizations
- [ ] Code quality maintained

**Testing Instructions:**
- Run full test suite after each optimization
- Profile game before and after optimizations
- Measure frame rate improvements
- Check for reduced GC allocations
- Verify gameplay still feels correct

**Commit Message Template:**
```
perf(optimization): optimize code patterns for Unity 6

Implemented performance improvements:
- Component caching in hot paths
- Optimized combat system updates
- Reduced GC allocations
- Improved AI decision-making efficiency

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~5,500

---

### Task 5: Enhance Developer Experience with Unity 6 Tools

**Goal:** Adopt Unity 6 Editor improvements for better development workflow

**Files to Modify/Create:**
- Editor scripts in `Assets/Knockout/Scripts/Editor/`
- Custom inspectors (if beneficial)
- Editor tools and utilities

**Prerequisites:**
- Task 4 complete (code patterns optimized)

**Implementation Steps:**

1. Review Unity 6 Editor improvements:
   - Check for new Editor APIs
   - Identify helpful Editor features
   - Look for debugging improvements
2. Editor enhancements to consider:
   - **Custom Inspectors:**
     - Improve inspector UI for ScriptableObjects (AttackData, CharacterStats, etc.)
     - Add helpful visualizations
     - Add validation and warnings in inspector
   - **Editor Tools:**
     - Create or improve existing editor tools
     - Note: Phase1AssetGenerator, Phase2Setup, Phase4Setup already exist
     - Update these tools for Unity 6 if needed
   - **Gizmos and Handles:**
     - Add visual debugging aids
     - Draw hitboxes, ranges, AI detection zones
     - Help with level design and debugging
   - **Editor Preferences:**
     - Set up useful Editor preferences for team
     - Configure for WebGL development focus
3. Update existing Editor scripts:
   - Review `InputActionsGenerator.cs`
   - Review `PerformanceOptimizationHelper.cs`
   - Review phase setup scripts
   - Update to Unity 6 Editor APIs if needed
   - Add new helpful functionality
4. Add debugging visualizations:
   - Draw combat ranges in Scene view
   - Visualize AI state in Scene view
   - Show hitbox/hurtbox overlays
   - Display stamina/health visually in Editor
5. Test Editor enhancements:
   - Verify Editor tools work
   - Check custom inspectors display correctly
   - Test gizmos and debug visualizations
   - Ensure no Editor-only errors

**Verification Checklist:**
- [ ] Existing Editor scripts reviewed and updated
- [ ] Custom inspectors improved (if beneficial)
- [ ] Debug visualizations added (if helpful)
- [ ] Editor tools functional in Unity 6
- [ ] Development workflow improved
- [ ] No Editor errors

**Testing Instructions:**
- Test Editor tools and verify functionality
- Check custom inspectors in Inspector window
- Verify gizmos draw correctly in Scene view
- Ensure Editor enhancements help development
- Confirm no errors in Editor scripts

**Commit Message Template:**
```
feat(editor): enhance Unity 6 Editor tools and visualizations

Improved developer experience:
- Updated Editor scripts for Unity 6
- Enhanced custom inspectors
- Added debug visualizations for combat/AI

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~5,000

---

### Task 6: Document Unity 6 Modernization

**Goal:** Document all modernization changes and Unity 6 features adopted

**Files to Modify/Create:**
- `docs/migration/MODERNIZATION_SUMMARY.md` (new file)
- Update project README or documentation with Unity 6 notes

**Prerequisites:**
- Tasks 1-5 complete (modernization work done)

**Implementation Steps:**

1. Create `docs/migration/MODERNIZATION_SUMMARY.md` with:
   - List of deprecated APIs replaced
   - URP improvements implemented
   - Code optimizations applied
   - Editor enhancements added
   - Performance improvements measured
   - Before/after comparisons (warnings, performance, etc.)
2. Document Unity 6-specific features used:
   - URP rendering features
   - Editor tools and workflows
   - API improvements
   - Performance optimizations
3. Document remaining deprecation warnings (if any):
   - List warnings that couldn't be addressed
   - Explain why (no replacement, too risky, etc.)
   - Note planned future work
4. Create recommendations for ongoing Unity 6 development:
   - Best practices for new code
   - Patterns to follow
   - Patterns to avoid
5. Update project documentation:
   - Note Unity 6.0 requirement in README
   - Document any Unity 6-specific setup needed
   - Link to migration documentation

**Verification Checklist:**
- [ ] MODERNIZATION_SUMMARY.md created
- [ ] All modernization work documented
- [ ] Unity 6 features adopted listed
- [ ] Remaining warnings documented
- [ ] Recommendations for future development included
- [ ] Project documentation updated

**Testing Instructions:**
- Review MODERNIZATION_SUMMARY.md for completeness
- Verify all modernization work is documented
- Check that documentation is helpful for future developers

**Commit Message Template:**
```
docs(modernization): document Unity 6 modernization

Documented all Unity 6 improvements:
- Deprecated API replacements
- URP enhancements
- Performance optimizations
- Editor improvements

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~3,500

---

### Task 7: Tag Modernization Completion

**Goal:** Create git checkpoint after modernization phase

**Files to Modify/Create:**
- `docs/migration/UPGRADE_CHECKLIST.md` (update)

**Prerequisites:**
- Task 6 complete (modernization documented)

**Implementation Steps:**

1. Verify all modernization changes committed
2. Create git tag "post-modernization"
3. Update `docs/migration/UPGRADE_CHECKLIST.md`:
   - Mark Phase 6 complete
   - Note modernization achievements
   - List Unity 6 features adopted
4. Create Phase 6 summary:
   - Deprecated APIs replaced
   - Warnings reduced
   - Performance improvements
   - Code quality improvements
   - Time spent
5. Commit documentation updates

**Verification Checklist:**
- [ ] All modernization changes committed
- [ ] Git tag "post-modernization" exists
- [ ] UPGRADE_CHECKLIST.md updated
- [ ] Phase 6 summary documented

**Testing Instructions:**
- Run `git tag` and verify tag exists
- Check git status is clean
- Run test suite one final time

**Commit Message Template:**
```
chore(migration): complete Phase 6 modernization

 Deprecated APIs replaced
 URP rendering improved
 Code patterns optimized
 Editor tools enhanced
 Warnings reduced by X%

Tagged as 'post-modernization'

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>

> Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~3,000

---

## Phase Verification

After completing all tasks in Phase 6, verify the following:

### Code Modernization
- [ ] All high-priority deprecated APIs replaced
- [ ] Deprecation warnings significantly reduced
- [ ] Code follows Unity 6 best practices
- [ ] Tests pass after all modernization

### Performance
- [ ] Code patterns optimized
- [ ] Component caching implemented
- [ ] GC allocations reduced
- [ ] Frame rate maintained or improved

### Rendering
- [ ] URP settings optimized for Unity 6
- [ ] Visual quality maintained or improved
- [ ] Rendering configuration modern

### Developer Experience
- [ ] Editor scripts updated for Unity 6
- [ ] Custom inspectors improved (if applicable)
- [ ] Debug visualizations helpful
- [ ] Development workflow enhanced

### Documentation
- [ ] DEPRECATION_AUDIT.md documents warnings
- [ ] MODERNIZATION_SUMMARY.md documents improvements
- [ ] UPGRADE_CHECKLIST.md updated
- [ ] Git tag "post-modernization" created

---

## Known Limitations and Technical Debt

**Acceptable Remaining Issues:**
- Some low-priority deprecation warnings (distant future removal)
- Minor performance optimizations deferred to Phase 7 (WebGL-specific)
- Advanced Unity 6 features not applicable to this project

---

## Next Phase

Once Phase 6 verification is complete:
- Proceed to [Phase-7.md](Phase-7.md) - WebGL Optimization & Final Validation
- Phase 7 is the final phase - WebGL-specific optimizations and shipping
- Final performance tuning, build size optimization, and acceptance testing

**Estimated time for Phase 6:** 8-12 hours (varies based on modernization scope)
