# Current Project State (Unity 2021.3.45f2)

## Document Purpose

This document captures the complete state of the Knockout project before beginning the Unity 6.0 upgrade. It serves as a reference for comparison after upgrade and ensures we can verify all components were successfully migrated.

**Date:** 2025-11-19
**Git Tag:** pre-unity6-upgrade
**Backup:** /root/knockout-2021.3-2025-11-19.tar.gz (1.3GB)

---

## Unity Version

**Editor Version:** 2021.3.45f2
**Editor Version (Full):** 2021.3.45f2 (88f88f591b2e)

---

## Unity Package Versions

### Core Rendering & Graphics
- `com.unity.render-pipelines.universal`: 12.1.15 (URP)
- `com.unity.ugui`: 1.0.0
- `com.unity.textmeshpro`: 3.0.6
- `com.unity.visualscripting`: 1.9.4

### Input & Camera
- `com.unity.inputsystem`: 1.7.0
- `com.unity.cinemachine`: 2.10.1

### Development Tools
- `com.unity.test-framework`: 1.1.33
- `com.unity.timeline`: 1.6.5
- `com.unity.probuilder`: 5.2.3
- `com.unity.collab-proxy`: 2.5.2

### IDE Integration
- `com.unity.ide.rider`: 3.0.31
- `com.unity.ide.visualstudio`: 2.0.22
- `com.unity.ide.vscode`: 1.2.5

### Unity Modules (1.0.0)
All standard Unity modules are included at version 1.0.0:
- ai, animation, audio, physics, physics2d
- ui, uielements, particlesystem
- assetbundle, imageconversion, jsonserialize
- terrain, tilemap, video, vehicles, vr, xr, wind
- unitywebrequest (all variants)
- screencapture, cloth, director

---

## Third-Party Assets

### Asset Store Packages

1. **AAAnimators Boxing (v1.1)**
   - Location: `Assets/AAAnimators_Boxing_basic1.1/`
   - Purpose: Boxing animation set for characters
   - Type: Animation package

2. **Asset Unlock - 3D Prototyping**
   - Location: `Assets/Asset Unlock - 3D Prototyping/`
   - Purpose: 3D prototyping assets and tools
   - Type: Prototyping/modeling tools

3. **StarterAssets** (Unity Official)
   - Location: Integrated into project structure
   - Purpose: Character controller templates and input handling
   - Type: Official Unity sample assets

### Custom Assets

4. **Elizabeth Warren Caricature**
   - Location: `Assets/Elizabeth Warren caricature/`
   - Purpose: Character model/asset
   - Type: Custom 3D model

---

## Project Structure

### Game Code
**Location:** `Assets/Knockout/Scripts/`

**Components:**
- `AI/` - AI state machine and behaviors (6 states: Observe, Approach, Attack, Defend, Retreat)
- `Audio/` - Audio management and character voice data
- `Characters/` - Character controllers and components
  - `Components/` - CharacterCombat, CharacterHealth, CharacterMovement, CharacterStamina, etc.
  - `Data/` - ScriptableObject definitions (AttackData, CharacterStats, ComboData, etc.)
- `Combat/` - Combat system and hit detection
  - `States/` - Combat states (Idle, Attacking, Blocking, Dodging, Exhausted, KnockedDown, etc.)
  - `HitDetection/` - Hitbox and hurtbox data
- `Systems/` - Round management and game systems
- `UI/` - UI components (HealthBar, StaminaBar, ComboCounter, RoundTimer, etc.)
- `Utilities/` - Camera controller and utilities
- `Editor/` - Editor tooling and setup scripts

**Key Files Count:**
- 70+ C# scripts
- Component-based architecture
- State machine pattern for AI and combat

### Tests
**Location:** `Assets/Knockout/Tests/`

**Test Coverage:**
- 52+ test files covering:
  - EditMode tests (unit tests for data classes, state machines, AI)
  - PlayMode tests (integration tests for components, systems)
  - Performance tests
  - UI tests
  - Integration tests (combat, defense, stamina, combos)

### Scenes
- `MainScene.unity` - Primary game scene
- `Sample.unity` - Sample/demo scene
- `greybox.unity` - Greybox testing scene
- `test.unity` - Test scene

### Other Notable Directories
- `Assets/Audio/` - Audio files and sound effects
- `Assets/TrumpSound/` - Character voice lines and audio clips
- `Assets/Materials/` - Material assets
- `Assets/ProBuilder Data/` - ProBuilder-generated data
- `Assets/Legacy/` - Legacy assets (likely unused)

---

## Target Platform

**Build Target:** WebGL only

**WebGL Settings:**
- Development builds enabled for testing
- Compression: Unknown (to be documented in baseline)
- Memory size: Unknown (to be documented in baseline)

---

## Architecture Patterns

### Design Patterns Used
1. **Component-Based Architecture**
   - Characters composed of multiple MonoBehaviour components
   - Loose coupling via interfaces (ICharacterController)
   - Separation of concerns (Combat, Health, Movement, Stamina, etc.)

2. **State Machine Pattern**
   - AI behaviors (AIStateMachine with 6 states)
   - Combat system (CombatStateMachine with 9+ states)
   - Clear state transitions and conditions

3. **ScriptableObject Data**
   - AttackData, CharacterStats, ComboData, StaminaData
   - Separation of data from behavior
   - Designer-friendly configuration

4. **Event-Driven UI**
   - UI components listen to character/game events
   - Decoupled from game logic

### Key Systems
- **Combat System:** Attacks, blocking, dodging, parrying, special moves
- **Stamina System:** Resource management for actions
- **Combo System:** Chain attacks for bonuses
- **Defense Systems:** Blocking, dodging, parrying mechanics
- **AI System:** Opponent behavior and decision-making
- **Scoring System:** Judge decisions and round scoring
- **Round Management:** Time limits, round structure
- **Training Mode:** Practice and learning features

---

## Known Issues (Pre-Upgrade)

### Git Status Before Upgrade
- All changes committed
- No uncommitted modifications
- Tag "pre-unity6-upgrade" created
- Branch: main

### Test Status
- Test pass rate: Unknown (requires Unity Editor to run)
- No known critical test failures documented
- Tests should be run in Unity 2021.3 to establish baseline

### Audio Files
- Git LFS configured for binary asset management
- Audio files tracked via LFS (WAV and MP3 files)

---

## Dependencies Summary

### Critical Dependencies for Upgrade
1. **URP 12.1.15 → Unity 6 URP** (major version jump)
2. **Input System 1.7.0 → Unity 6 version** (likely 1.8+)
3. **Cinemachine 2.10.1 → Unity 6 version** (likely 3.x - major version)
4. **Test Framework 1.1.33 → Latest**

### Third-Party Compatibility Concerns
- AAAnimators Boxing v1.1 - Unity 6 support unknown
- Asset Unlock - 3D Prototyping - Unity 6 support unknown
- StarterAssets - Should have Unity 6 version available

---

## Metrics to Measure After Upgrade

### Test Suite
- [ ] Test execution time (EditMode)
- [ ] Test execution time (PlayMode)
- [ ] Test pass rate

### WebGL Build
- [ ] Build time
- [ ] Build size (total)
- [ ] WASM file size
- [ ] Load time in browser
- [ ] Frame rate during gameplay
- [ ] Memory usage

### Code Quality
- [ ] Compilation warnings
- [ ] API deprecation warnings
- [ ] Console errors

---

## Next Steps

After documenting current state:
1. Establish performance baselines (Task 5 - requires Unity Editor)
2. Research third-party asset compatibility (Task 6)
3. Create migration checklist (Task 7)
4. Proceed to Phase 2: Unity 6 Upgrade Execution

---

**Document Status:** Complete
**Created By:** Claude Code
**Git Commit:** Will be committed with Task 4
