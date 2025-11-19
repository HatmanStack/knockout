# Core Fighting Mechanics - Implementation Plan

## Feature Overview

This plan implements advanced fighting mechanics for the Knockout boxing game, transforming it from a basic combat system into a deep, skill-based fighting experience. The implementation adds stamina management, combo systems with both natural chains and predefined sequences, character-specific signature special moves, advanced defensive mechanics (dodging with i-frames and timed parrying), and a comprehensive judge scoring system for round decisions. The combat philosophy is momentum-based: offensive play builds advantage while defensive play resets momentum, creating dynamic ebb-and-flow gameplay similar to real boxing.

The existing codebase provides a solid foundation with a component-based architecture, state machines for combat and AI, ScriptableObject-driven data configuration, and 80%+ test coverage. We'll extend this foundation by adding new components, data structures, and systems while maintaining the existing patterns and code quality standards.

All systems are designed to be tunable via ScriptableObjects, allowing designers to balance gameplay without code changes. Character differentiation is achieved through unique combo sequences and signature special moves, while sharing the core attack framework (Jab, Hook, Uppercut). The implementation follows TDD principles with comprehensive unit and integration tests for all new systems.

## Prerequisites

**Unity Environment:**
- Unity 2021.3.8f1 LTS
- Universal Render Pipeline (URP) 12.1.7
- Input System 1.4.4
- Cinemachine 2.8.9

**Existing Codebase Knowledge:**
- Familiarity with component-based character architecture (`CharacterController`, `CharacterCombat`, `CharacterAnimator`)
- Understanding of `CombatStateMachine` and combat states
- Knowledge of ScriptableObject data patterns (`CharacterStats`, `AttackData`)
- Experience with Unity's test framework (EditMode and PlayMode tests)

**Available Assets:**
- Character animations: Jab, Hook, Uppercut (left/right variants)
- Defensive animations: Block, left_dodge, right_dodge
- Hit reaction animations: hit_by_jab, hit_by_hook, etc.

## Phase Summary

| Phase | Goal | Task Tokens | Status |
|-------|------|-------------|--------|
| [Phase 0](Phase-0.md) | Foundation - Architecture & Design Decisions | N/A | Not Started |
| [Phase 1](Phase-1.md) | Core Resource Systems - Stamina & Enhanced Knockdowns | ~61,000 | Not Started |
| [Phase 2](Phase-2.md) | Advanced Defense - Dodge & Parry Systems | ~89,000 | Not Started |
| [Phase 3](Phase-3.md) | Combo System - Chains & Predefined Sequences | ~81,000 | Not Started |
| [Phase 4](Phase-4.md) | Special Moves & Judge Scoring | ~88,000 | Not Started |
| [Phase 5](Phase-5.md) | UI Systems & Training Mode | ~106,000 | Not Started |

**Total Task Tokens:** ~425,000

**Note:** Token estimates reflect pure implementation work. Expect additional overhead for debugging, iteration, and testing. Plan spans multiple Claude Code sessions (200k context budget per session).

## Navigation

- **[Phase 0: Foundation](Phase-0.md)** - Start here to understand architecture decisions and design patterns
- **[Phase 1: Core Resource Systems](Phase-1.md)** - Stamina management and exhaustion mechanics
- **[Phase 2: Advanced Defense](Phase-2.md)** - Dodging with i-frames and parry timing
- **[Phase 3: Combo System](Phase-3.md)** - Natural chains and predefined sequence bonuses
- **[Phase 4: Special Moves & Judge Scoring](Phase-4.md)** - Signature techniques and comprehensive scoring
- **[Phase 5: UI Systems & Training Mode](Phase-5.md)** - Contextual UI and practice environment

---

## Critical Implementation Notes

### Phase Dependencies (IMPORTANT)

Each phase **MUST** be 100% complete (all tasks done, all tests passing) before starting the next:

- **Phase 1 → Phase 2:** CharacterStamina required for dodge/parry stamina-free design
- **Phase 2 → Phase 3:** Blocking state required for combo interruption mechanics
- **Phase 3 → Phase 4:** Combo tracking required for scoring integration
- **Phase 4 → Phase 5:** All systems required for UI integration

**Do not start a phase until all prerequisite phases are complete and tested.**

### Dealing with Unknown Code

When tasks reference events, methods, or systems that might not exist:

1. **Search first**: Use Grep/Glob tools to find similar patterns in existing code
2. **Follow naming conventions**: Look for patterns like `OnEventName`, `ExecuteAction()`, `ProcessInput()`
3. **Check similar components**: If looking for stamina event, check how health events work
4. **Create if missing**: Add events/methods following existing patterns (document what you added)
5. **Document discoveries**: Note actual names found in task verification checklist
6. **Ask if blocked**: Use AskUserQuestion only if fundamentally stuck (rare)

**Example**: Task says "Subscribe to CharacterCombat.OnAttackStarted (or similar event)"
- Search: `Grep pattern: "public event.*Attack" in CharacterCombat.cs`
- Found: `OnAttackExecuted` or `OnAttackBegin`? Use it.
- Not found: Add `public event Action<AttackData> OnAttackStarted;` following existing event pattern
- Document: Note in verification what you used/created

---

## Implementation Workflow

1. **Read Phase 0** thoroughly to understand architectural decisions and patterns
2. **Implement phases sequentially** - each phase builds on previous work
3. **Complete all tasks** in a phase before moving to the next
4. **Run tests frequently** - maintain 80%+ coverage throughout
5. **Commit atomically** - use conventional commit format after each task
6. **Verify phase completion** - check all verification criteria before proceeding

## Development Principles

- **TDD (Test-Driven Development):** Write tests before implementation when possible
- **DRY (Don't Repeat Yourself):** Extract shared logic into utilities/base classes
- **YAGNI (You Aren't Gonna Need It):** Implement only what's specified, no scope creep
- **Component-Based:** Follow existing architecture patterns
- **ScriptableObject Configuration:** All tunable values in ScriptableObjects
- **Conventional Commits:** Format: `type(scope): description`

## Support

For questions about existing codebase patterns, refer to:
- `/docs/EXTENSION_GUIDE.md` - Extension points and patterns
- `/Assets/Knockout/Scripts/Characters/Data/SCRIPTABLE_OBJECTS_SETUP.md` - Data configuration
- `/docs/README.md` - Overall architecture documentation
