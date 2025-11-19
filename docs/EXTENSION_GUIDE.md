# Extension Guide

This guide documents how to extend the Knockout game with new features and systems.

## Table of Contents

1. [Adding New Characters](#adding-new-characters)
2. [Physics-Based AI Integration](#physics-based-ai-integration)
3. [Multiplayer Networking](#multiplayer-networking)
4. [Character Voice Lines](#character-voice-lines)
5. [Custom Attacks and Movesets](#custom-attacks-and-movesets)
6. [Extension Points](#extension-points)

---

## Adding New Characters

The game is designed to easily support multiple characters through ScriptableObject-driven data.

### Step-by-Step Guide

#### 1. Prepare Character Assets

**Requirements:**
- Character model (FBX/GLB format)
- Humanoid rig (Unity's humanoid avatar system)
- Boxing animations (or retarget existing animations)

**Example:**
```
Assets/Knockout/Models/Characters/Biden/
├── Biden.fbx
├── Textures/
│   ├── Biden_Diffuse.png
│   ├── Biden_Normal.png
│   └── Biden_Specular.png
└── Materials/
    └── Biden_Material.mat
```

#### 2. Create CharacterStats ScriptableObject

```csharp
// In Unity Editor:
// Assets > Create > Knockout > Character Stats

// Configure stats:
Max Health: 100
Move Speed: 5.0
Damage Multiplier: 1.0 (character-specific scaling)
Damage Taken Multiplier: 1.0 (for balancing)
```

**Example for different playstyles:**
```csharp
// Tank character (high health, slow, reduced damage)
Max Health: 150
Move Speed: 3.5
Damage Multiplier: 0.8
Damage Taken Multiplier: 0.7

// Glass cannon (low health, fast, high damage)
Max Health: 75
Move Speed: 6.5
Damage Multiplier: 1.3
Damage Taken Multiplier: 1.2
```

#### 3. Create Character Prefab

**Required Components:**
- CharacterController (coordinator)
- CharacterAnimator
- CharacterHealth (assign CharacterStats)
- CharacterCombat (assign AttackData)
- CharacterMovement
- CharacterInput (for player) OR CharacterAI (for AI)
- Animator (with configured AnimatorController)

**Example Hierarchy:**
```
Biden (Prefab)
├── CharacterController
├── CharacterAnimator
├── CharacterHealth (Stats: Biden_Stats)
├── CharacterCombat
├── CharacterMovement
├── CharacterInput
├── Animator (Controller: BaseCharacter)
├── Model (Biden.fbx instance)
├── Hitboxes
│   ├── LeftHandHitbox (trigger collider)
│   └── RightHandHitbox (trigger collider)
└── Hurtboxes
    ├── HeadHurtbox (trigger collider)
    └── BodyHurtbox (trigger collider)
```

#### 4. Configure Animator Controller

Option 1: **Use existing BaseCharacter controller** (shares animations)
Option 2: **Create character-specific controller** (unique animations)

For unique animations:
```
Assets > Create > Animator Controller > Biden_Animator

Add states:
- Base Layer: Idle, Walk, Run
- Upper Body: Jab, Hook, Uppercut, Block
- Full Body Override: Hit Reactions, Knockdown, Knockout

Set parameters (same as base controller):
- MoveSpeed, MoveDirectionX, MoveDirectionY
- AttackTrigger, AttackType, IsBlocking
- HitReaction, HitType, KnockedDown, KnockedOut
```

#### 5. (Optional) Create Character Voice Data

```csharp
// Assets > Create > Knockout > Character Voice Data

// Organize voice clips:
Assets/Knockout/Audio/Voices/Biden/
├── Attack/
│   ├── Biden_Taunt1.wav
│   └── Biden_Taunt2.wav
├── Hit/
│   ├── Biden_Grunt1.wav
│   └── Biden_Grunt2.wav
├── Knockout/
│   └── Biden_Defeat.wav
├── Victory/
│   └── Biden_Victory.wav
└── Intro/
    └── Biden_Intro.wav

// Assign clips to CharacterVoiceData ScriptableObject
// Attach to CharacterAudioPlayer component
```

#### 6. Test Character

1. Add character prefab to scene
2. Assign to RoundManager (if replacing AI)
3. Play and verify:
   - Animations play correctly
   - Attacks deal correct damage
   - Health system works
   - Character stats affect gameplay

---

## Physics-Based AI Integration

Based on the Facebook Research paper: [Control Strategies for Physically Simulated Characters Performing Two-player Competitive Sports](https://research.facebook.com/publications/control-strategies-for-physically-simulated-characters-performing-two-player-competitive-sports/)

### Architecture for Physics-Based Control

The game is designed to support swapping control systems via `ICharacterController` interface.

#### Current Architecture (Animation-Based)

```
CharacterController (coordinator)
├── CharacterAnimator (plays animations)
├── CharacterCombat (triggers attacks)
└── CharacterAI (decision-making)
```

#### Future Architecture (Physics-Based)

```
CharacterController (coordinator)
├── PhysicsBasedController (implements ICharacterController)
│   ├── ArticulationBody joints
│   ├── PD Controllers
│   └── Animation pose references
├── CharacterCombat (triggers attack targets)
└── CharacterAI (decision-making, outputs target poses)
```

### Implementation Steps

#### 1. Implement ICharacterController for Physics

```csharp
using UnityEngine;
using Knockout.Characters;

public class PhysicsBasedController : MonoBehaviour, ICharacterController
{
    // Physics simulation
    private ArticulationBody[] _joints;
    private PDController[] _pdControllers;

    // Target pose from animation
    private AnimationPose _targetPose;

    // Current control mode
    private bool _isActive;

    public bool IsActive => _isActive;
    public Transform Transform => transform;

    public void Initialize()
    {
        // Set up articulation bodies for ragdoll
        SetupPhysicsRig();

        // Initialize PD controllers for each joint
        InitializePDControllers();
    }

    public void FixedUpdateControl()
    {
        if (!_isActive) return;

        // Apply PD control to match target pose
        for (int i = 0; i < _joints.Length; i++)
        {
            Vector3 torque = _pdControllers[i].ComputeTorque(
                _joints[i].transform.rotation,
                _targetPose.rotations[i],
                _joints[i].angularVelocity,
                _targetPose.angularVelocities[i]
            );

            _joints[i].AddTorque(torque, ForceMode.Force);
        }

        // Balance and stability controllers
        ApplyBalanceControl();
    }

    public bool ExecuteJab()
    {
        // Set target pose to jab animation
        _targetPose = GetAnimationPose("Jab");
        return true;
    }

    private void SetupPhysicsRig()
    {
        // Convert character rig to ArticulationBody chain
        // Pelvis (root) -> Spine -> Head
        //               -> Left/Right Arms
        //               -> Left/Right Legs
    }

    private void ApplyBalanceControl()
    {
        // Inverted pendulum control for standing balance
        // See paper Section 3.2: Balance Controller
    }
}
```

#### 2. PD Controller Implementation

```csharp
public class PDController
{
    public float kp = 1000f; // Proportional gain
    public float kd = 100f;  // Derivative gain

    public Vector3 ComputeTorque(
        Quaternion currentRotation,
        Quaternion targetRotation,
        Vector3 currentAngularVelocity,
        Vector3 targetAngularVelocity)
    {
        // Compute rotation error
        Quaternion rotationError = targetRotation * Quaternion.Inverse(currentRotation);
        rotationError.ToAngleAxis(out float angle, out Vector3 axis);

        // Normalize angle to [-180, 180]
        if (angle > 180f) angle -= 360f;

        Vector3 rotationErrorVec = axis * angle * Mathf.Deg2Rad;

        // Compute angular velocity error
        Vector3 velocityError = targetAngularVelocity - currentAngularVelocity;

        // PD control law
        Vector3 torque = kp * rotationErrorVec + kd * velocityError;

        return torque;
    }
}
```

#### 3. Animation Pose Extraction

```csharp
public class AnimationPose
{
    public Quaternion[] rotations;
    public Vector3[] angularVelocities;

    public static AnimationPose ExtractFromAnimator(Animator animator, string stateName)
    {
        // Sample animation at current time
        // Extract rotation for each bone
        // Compute angular velocities from previous frame

        var pose = new AnimationPose();
        // ... implementation
        return pose;
    }
}
```

#### 4. Integration with Existing Systems

**No changes needed to:**
- CharacterHealth (damage system)
- CharacterCombat (combat state machine)
- RoundManager (match flow)
- UI systems

**Only swap:**
```csharp
// Replace CharacterAnimator + CharacterAI
// with PhysicsBasedController

characterController.SetController(new PhysicsBasedController());
```

### Resources

- [Paper PDF](https://research.facebook.com/publications/control-strategies-for-physically-simulated-characters-performing-two-player-competitive-sports/)
- Unity ArticulationBody Documentation
- PD Control Tutorials

---

## Multiplayer Networking

The architecture is designed to support online multiplayer with minimal changes.

### Network-Ready Design Patterns

#### 1. Input-Based Architecture

Current:
```
CharacterInput (local) → CharacterCombat → Execute Attack
```

Network:
```
CharacterInput (local) → NetworkManager → Server
Server → NetworkManager → CharacterCombat → Execute Attack
```

#### 2. Deterministic Combat

- Frame-based combat system (60fps fixed)
- Explicit state machine (no randomness in transitions)
- Damage calculated server-side
- State synchronized via network

#### 3. State Synchronization Points

**Critical state to sync:**
- Position and rotation
- Current combat state
- Health value
- Current animation state
- Round state

**Example network sync:**
```csharp
public class NetworkedCharacter : NetworkBehaviour
{
    [SyncVar] private Vector3 networkPosition;
    [SyncVar] private Quaternion networkRotation;
    [SyncVar] private float networkHealth;
    [SyncVar] private int networkCombatState;

    [Command]
    void CmdExecuteAttack(int attackType)
    {
        // Server validates and executes
        characterCombat.ExecuteAttack(attackType);

        // Broadcast to all clients
        RpcOnAttackExecuted(attackType);
    }

    [ClientRpc]
    void RpcOnAttackExecuted(int attackType)
    {
        // Play effects and animations on all clients
        characterAnimator.PlayAttackAnimation(attackType);
    }
}
```

### Networking Implementation Steps

#### Using Unity Netcode for GameObjects

1. **Add Netcode package:**
   ```
   Package Manager > Add > Netcode for GameObjects
   ```

2. **Add NetworkManager to scene:**
   ```csharp
   GameObject networkManager = new GameObject("NetworkManager");
   networkManager.AddComponent<Unity.Netcode.NetworkManager>();
   ```

3. **Make character prefab network-spawnable:**
   ```csharp
   - Add NetworkObject component
   - Add NetworkedCharacter component (inherits NetworkBehaviour)
   - Register prefab in NetworkManager
   ```

4. **Implement input forwarding:**
   ```csharp
   if (IsOwner)
   {
       // Capture input
       if (Input.GetKeyDown(KeyCode.Q))
       {
           CmdExecuteJab();
       }
   }
   ```

5. **Sync critical state:**
   ```csharp
   [ServerRpc]
   void UpdatePositionServerRpc(Vector3 position)
   {
       networkPosition = position;
   }

   void Update()
   {
       if (IsOwner)
       {
           UpdatePositionServerRpc(transform.position);
       }
       else
       {
           // Interpolate to network position
           transform.position = Vector3.Lerp(
               transform.position,
               networkPosition,
               Time.deltaTime * 10f
           );
       }
   }
   ```

---

## Character Voice Lines

The audio system is designed for easy integration of character-specific voice clips.

### Voice Line Integration

#### 1. Organize Voice Clips

```
Assets/Knockout/Audio/Voices/[CharacterName]/
├── Attack/       # Taunts, battle cries
├── Hit/          # Grunts, exclamations
├── Knockout/     # Defeat lines
├── Victory/      # Victory lines
└── Intro/        # Round start intros
```

**Example for political character:**
```
Assets/Knockout/Audio/Voices/Trump/
├── Attack/
│   ├── Trump_MakeAmericaGreatAgain.wav
│   ├── Trump_Yourefired.wav
│   └── Trump_Winning.wav
├── Hit/
│   ├── Trump_Wrong.wav
│   └── Trump_Sad.wav
├── Knockout/
│   └── Trump_Loser.wav
└── Victory/
    └── Trump_BigWin.wav
```

#### 2. Create CharacterVoiceData ScriptableObject

```
Assets > Create > Knockout > Character Voice Data

Name: Trump_VoiceData

Assign clips:
- Attack Voice Lines: [3 clips from Attack/]
- Hit Voice Lines: [2 clips from Hit/]
- Knockout Voice Lines: [1 clip from Knockout/]
- Victory Voice Lines: [1 clip from Victory/]
- Intro Voice Lines: [0 clips] (optional)
```

#### 3. Assign to CharacterAudioPlayer

```csharp
// On character prefab:
CharacterAudioPlayer component
├── Voice Data: Trump_VoiceData
├── Voice Line Chance: 0.3 (30% chance to play)
└── Voice Line Volume: 0.8
```

#### 4. Hook Points

Voice lines automatically play at these events:
- **OnAttackExecuted:** When attack starts (CharacterCombat event)
- **OnHitTaken:** When taking damage (CharacterHealth event)
- **OnDeath:** When knocked out (CharacterHealth event)

Manual triggers (called by RoundManager or custom systems):
- **PlayVictoryVoiceLine():** Call when round won
- **PlayIntroVoiceLine():** Call at round start

**Example integration with RoundManager:**
```csharp
public class RoundManager : MonoBehaviour
{
    private CharacterAudioPlayer playerAudio;
    private CharacterAudioPlayer aiAudio;

    private void OnRoundStart(int roundNumber)
    {
        // Play intro voice lines
        playerAudio.PlayIntroVoiceLine();
        aiAudio.PlayIntroVoiceLine();
    }

    private void OnRoundEnd(bool playerWon)
    {
        if (playerWon)
        {
            playerAudio.PlayVictoryVoiceLine();
        }
        else
        {
            aiAudio.PlayVictoryVoiceLine();
        }
    }
}
```

---

## Custom Attacks and Movesets

### Adding New Attack Types

#### 1. Create AttackData ScriptableObject

```
Assets > Create > Knockout > Attack Data

Name: SpecialPunch_Data

Configure:
- Attack Type Index: 3 (next available)
- Damage: 35
- Knockback Force: 15
- Hit Type: 1 (medium)
- Animation Trigger: "SpecialPunch"
```

#### 2. Add Animation to Animator Controller

```
1. Import animation clip
2. Add to Animator Controller (Upper Body layer)
3. Create transition: Empty → SpecialPunch → Recovery → Empty
4. Add parameter: AttackType == 3
5. Add Animation Events:
   - OnHitboxActivate (frame 10)
   - OnHitboxDeactivate (frame 20)
   - OnAttackEnd (frame 30)
```

#### 3. Add to CharacterCombat

```csharp
// Option 1: Extend CharacterCombat
public bool ExecuteSpecialPunch()
{
    return ExecuteAttack(specialPunchData);
}

// Option 2: Use generic ExecuteAttack
characterCombat.ExecuteAttack(specialPunchAttackData);
```

#### 4. Add to Input System

```csharp
// In CharacterInput
inputActions.Gameplay.SpecialPunch.performed += ctx => OnSpecialPunchPressed?.Invoke();

// Wire to CharacterCombat
characterInput.OnSpecialPunchPressed += characterCombat.ExecuteSpecialPunch;
```

---

## Extension Points

### Summary of Extension Mechanisms

| System | Extension Point | Use Case |
|--------|----------------|----------|
| Character Control | ICharacterController | Physics-based AI, Network control |
| Character Data | CharacterStats SO | New characters, balancing |
| Combat | AttackData SO | New attacks, movesets |
| AI Behavior | AIState subclass | Custom AI behaviors |
| Audio | CharacterVoiceData SO | Character voice lines |
| UI | Event subscriptions | Custom UI elements |

### Design Principles for Extensions

1. **Use ScriptableObjects for Data:** No code changes needed for content
2. **Use Interfaces for Swappable Systems:** ICharacterController example
3. **Use Events for Communication:** Loose coupling, easy to extend
4. **Use State Pattern for Behaviors:** Easy to add new states
5. **Document Extension Points:** Make it clear where to extend

---

## Conclusion

The Knockout architecture is designed for extensibility:
- Add new characters with ScriptableObjects
- Swap control systems with interfaces
- Extend AI with new states
- Add voice lines with CharacterVoiceData
- Network-ready with input-based architecture

For questions or contributions, see the main README.

---

Last Updated: Phase 5 Implementation
