# Knockout - Fighting Game

A physics-based fighting game built with Unity, featuring character combat, AI opponents, and a complete round system.

## Features

### Core Gameplay
- **Player vs AI Combat:** Full fighting game mechanics with attacks, blocking, and hit reactions
- **Complete Moveset:** Jab, hook, and uppercut attacks with unique damage and timing
- **Defensive Mechanics:** Blocking reduces damage by 75%
- **Round System:** Best-of-3 rounds with automatic round reset and match flow
- **Dynamic Camera:** Automatically frames both fighters and adjusts zoom based on distance
- **Health System:** Visual health bars with color gradients and damage flash effects

### AI System
- **Behavioral State Machine:** AI with Observe, Approach, Attack, Defend, and Retreat states
- **Intelligent Decision-Making:** Range-based attack selection and health-aware defensive play
- **Personality Variation:** Configurable aggression and defensive tendencies
- **Target Detection:** Automatic opponent detection and distance tracking

### Technical Features
- **Component-Based Architecture:** Clean separation of concerns (Animation, Combat, Movement, Health, AI)
- **Event-Driven Communication:** Loose coupling through C# events
- **Combat State Machine:** Explicit states for Idle, Attacking, Blocking, Hit-Stunned, Knocked Down, Knocked Out
- **Audio System:** Positional 3D audio with object pooling and character voice line support
- **Comprehensive Testing:** 80%+ test coverage with unit, integration, and performance tests
- **Performance Optimized:** Targeting stable 60fps on desktop platforms

## Project Structure

```
Assets/Knockout/
├── Animations/          # Animation clips and Animator controllers
├── Audio/               # Sound effects and music
├── Materials/           # Character and environment materials
├── Models/              # Character models (FBX/GLB)
├── Prefabs/             # Character and system prefabs
├── Scenes/              # Unity scenes
├── Scripts/             # All C# code
│   ├── Characters/      # Character controllers and components
│   ├── Combat/          # Combat system and hit detection
│   ├── AI/              # AI state machine and behaviors
│   ├── Systems/         # Round manager and game systems
│   ├── UI/              # Health bars and round UI
│   ├── Audio/           # Audio manager and character audio
│   ├── Utilities/       # Camera controller and helpers
│   └── Editor/          # Editor tools and utilities
└── Tests/               # Unit, integration, and performance tests
    ├── EditMode/        # Tests that don't require Unity runtime
    └── PlayMode/        # Tests that require Unity runtime
```

## Getting Started

### Prerequisites
- Unity 2021.3.8f1 LTS or higher
- Visual Studio 2019+ or JetBrains Rider
- Git for version control

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/knockout.git
   cd knockout
   ```

2. Open the project in Unity Hub:
   - Click "Add" and select the project folder
   - Open with Unity 2021.3.8f1 LTS

3. Let Unity import all assets (may take a few minutes)

4. Open the main scene:
   - Navigate to `Assets/Knockout/Scenes/`
   - Open `GameplayTest.unity` (or your main scene)

### Quick Start

1. **Set up characters:**
   - Player character needs: CharacterController, CharacterHealth, CharacterCombat, CharacterInput, CharacterAnimator
   - AI character needs: CharacterController, CharacterHealth, CharacterCombat, CharacterAI, CharacterAnimator

2. **Set up Round Manager:**
   - Add RoundManager to scene
   - Assign player and AI CharacterHealth references

3. **Set up Camera:**
   - Add Cinemachine Virtual Camera
   - Add CameraController and assign player/AI transforms

4. **Set up UI:**
   - Create Canvas with health bars
   - Add HealthBarUI components and assign character health references
   - Add RoundUI component and assign round manager

5. **Press Play!**

## Controls

| Action | Key |
|--------|-----|
| Move Forward | W |
| Move Back | S |
| Strafe Left | A |
| Strafe Right | D |
| Jab (Left) | Q |
| Jab (Right) | E |
| Hook (Left) | 1 |
| Hook (Right) | 2 |
| Uppercut (Left) | Z |
| Uppercut (Right) | C |
| Block | Left Shift |

*Note: Controls are configured via Unity's Input System and can be modified in `Assets/Knockout/Scripts/Input/InputActions.inputactions`*

## Architecture

The game follows a component-based architecture with clear separation of concerns:

### Character Components
- **CharacterController:** Main coordinator, holds component references
- **CharacterAnimator:** Manages Animator and animation state
- **CharacterInput:** Handles player input (player only)
- **CharacterMovement:** Controls locomotion and positioning
- **CharacterCombat:** Executes attacks, manages combat state
- **CharacterHealth:** Tracks health, applies damage
- **CharacterAI:** AI decision-making (AI only)

### Combat System
- **Combat State Machine:** Manages combat states and transitions
- **Hit Detection:** Trigger-based hitbox/hurtbox system
- **Attack Data:** ScriptableObjects for attack properties
- **Character Stats:** ScriptableObjects for character attributes

### AI System
- **AI State Machine:** Behavioral states (Observe, Attack, Defend, etc.)
- **Target Detection:** Automatic opponent finding and tracking
- **Decision-Making:** Range-based, health-aware AI logic

For detailed architecture documentation, see `docs/plans/Phase-0.md`.

## Testing

### Running Tests

1. Open Unity Test Runner:
   - Window > General > Test Runner

2. Run tests:
   - **EditMode:** Tests that don't require Unity runtime (fast)
   - **PlayMode:** Tests that require Unity runtime (slower but comprehensive)

3. View results:
   - All tests should pass
   - Current coverage: 80%+ for critical systems

### Test Categories

- **Unit Tests:** Individual component behavior
- **Integration Tests:** Component interactions and full gameplay flow
- **Performance Tests:** Frame rate stability and memory allocation

See `Assets/Knockout/Tests/` for test implementation.

## Performance

### Target Metrics
- **Frame Rate:** 60fps (16.67ms per frame)
- **Platform:** Desktop (PC/Mac/Linux)
- **Resolution:** 1920x1080

### Optimization Tools

Access via `Knockout > Performance Optimization Helper` menu:
- Automated animator optimization
- Physics settings verification
- URP settings check

See `docs/PERFORMANCE_OPTIMIZATION.md` for detailed optimization guide.

## Development Workflow

### Adding New Characters

1. Create character model (humanoid rig)
2. Import animations
3. Create CharacterStats ScriptableObject
4. Create character prefab with all required components
5. Configure Animator controller
6. (Optional) Create CharacterVoiceData for voice lines

### Adding New Attacks

1. Create AttackData ScriptableObject
2. Set damage, animation trigger, active frames
3. Add animation to Animator controller
4. Add hitbox activation animation events
5. Connect to CharacterCombat component

### Extending AI Behavior

1. Create new AIState subclass
2. Implement Enter, Update, Exit methods
3. Add state transitions in existing states
4. Test behavior in PlayMode

## Contributing

### Code Style
- Follow C# naming conventions (PascalCase for public, camelCase with underscore for private)
- Add XML documentation to all public classes and methods
- Write tests for new features
- Follow TDD principles (test first, then implement)

### Commit Messages
Use conventional commits format:
```
feat(scope): brief description

- Detailed change 1
- Detailed change 2

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

Types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `style`, `perf`

## Documentation

- **Architecture:** `docs/plans/Phase-0.md` - Design decisions and patterns
- **Implementation Plans:** `docs/plans/Phase-1.md` through `Phase-5.md`
- **Performance:** `docs/PERFORMANCE_OPTIMIZATION.md` - Optimization guide
- **Code Documentation:** XML comments in source files

## Future Enhancements

### Planned Features
- Additional characters (political figures with unique movesets)
- Physics-based AI ([Facebook Research paper](https://research.facebook.com/publications/control-strategies-for-physically-simulated-characters-performing-two-player-competitive-sports/) implementation)
- Character voice lines integration (soundbytes from political speeches)
- Enhanced VFX for hits and knockouts
- Training mode with combo practice
- Tournament mode
- Online multiplayer

### Extension Points
See `docs/plans/Phase-0.md` section "Future Considerations" for architecture details.

## Credits

### Development
- Implementation: Claude Code (Anthropic)
- Architecture: Based on Phase 0-5 implementation plan

### Assets
- Character Model: Elizabeth Warren caricature (Assets/Elizabeth Warren caricature/)
- Animations: AAAnimators Boxing pack (Assets/AAAnimators_Boxing_basic1.1/)
- Voice Clips: (Assets/TrumpSound/) - Character-specific soundbytes

### Technology
- **Engine:** Unity 2021.3.8f1 LTS
- **Render Pipeline:** Universal Render Pipeline (URP) 12.1.7
- **Input:** Unity Input System 1.4.4
- **Camera:** Cinemachine 2.8.9

## License

This project is licensed under the MIT License.

## Support

For issues, questions, or contributions:
- Create an issue on GitHub
- See `docs/plans/` for implementation details
- Check `docs/PERFORMANCE_OPTIMIZATION.md` for performance issues

---

**Built with Unity 2021.3.8f1 LTS**
