# Phase 3: Combat Mechanics & Hit Detection

## Phase Goal

Implement the complete combat system including hit detection with hitboxes/hurtboxes, damage calculation, combat state machine, character health system, and player input integration. By the end of this phase, you will have a fully playable single-player experience where the player can control a character to fight (though the opponent will be stationary until Phase 4).

**Success Criteria:**
- Hit detection system functional with hitbox and hurtbox colliders
- Combat state machine enforces valid state transitions
- CharacterCombat component controls attacks and defense
- CharacterHealth component manages damage and death
- CharacterInput component connects Unity Input System to character
- Player can move, attack, block, and damage opponent
- All tests passing

**Estimated Tokens:** ~105,000

---

## Prerequisites

### Must Complete First
- Phase 2 complete (animation system fully functional)
- Animator Controller with all layers working
- CharacterAnimator component created

### Verify Before Starting
1. Character prefabs animate correctly
2. AnimationTester works for manual testing
3. Animation events fire correctly

---

## Tasks

### Task 1: Create Hit Detection Data Structures

**Goal:** Create the data structures and ScriptableObjects that define hit properties, hurt regions, and damage calculations.

**Files to Create:**
- `Assets/Knockout/Scripts/Combat/HitDetection/HitData.cs` (struct)
- `Assets/Knockout/Scripts/Combat/HitDetection/HurtboxData.cs` (component)
- `Assets/Knockout/Scripts/Combat/HitDetection/HitboxData.cs` (component)

**Prerequisites:**
- Phase 1 Task 7 complete (AttackData ScriptableObject exists)

**Implementation Steps:**

1. **Create HitData struct:**

   ```csharp
   using UnityEngine;

   namespace Knockout.Combat.HitDetection
   {
       /// <summary>
       /// Data structure representing a hit event (passed from attacker to target).
       /// </summary>
       public struct HitData
       {
           public GameObject Attacker;
           public float Damage;
           public float Knockback;
           public Vector3 HitPoint;
           public Vector3 HitDirection;
           public int HitType; // 0=light, 1=medium, 2=heavy
           public string AttackName;

           public HitData(GameObject attacker, float damage, float knockback, Vector3 hitPoint, Vector3 hitDirection, int hitType, string attackName)
           {
               Attacker = attacker;
               Damage = damage;
               Knockback = knockback;
               HitPoint = hitPoint;
               HitDirection = hitDirection;
               HitType = hitType;
               AttackName = attackName;
           }
       }
   }
   ```

2. **Create HurtboxData component:**

   ```csharp
   using UnityEngine;

   namespace Knockout.Combat.HitDetection
   {
       /// <summary>
       /// Defines a hurt region on a character (head, body, etc.).
       /// Attached to trigger colliders that detect incoming hits.
       /// </summary>
       [RequireComponent(typeof(Collider))]
       public class HurtboxData : MonoBehaviour
       {
           [Header("Hurtbox Properties")]
           [SerializeField] [Tooltip("Damage multiplier for this region (e.g., head = 1.5x)")]
           private float damageMultiplier = 1f;

           [SerializeField] [Tooltip("Hit type override (0=light, 1=medium, 2=heavy, -1=use attack's type)")]
           private int hitTypeOverride = -1;

           [Header("References")]
           [SerializeField] [Tooltip("The character this hurtbox belongs to")]
           private GameObject ownerCharacter;

           public float DamageMultiplier => damageMultiplier;
           public int HitTypeOverride => hitTypeOverride;
           public GameObject OwnerCharacter => ownerCharacter;

           private void Awake()
           {
               // Ensure collider is trigger
               Collider col = GetComponent<Collider>();
               if (!col.isTrigger)
               {
                   Debug.LogWarning($"[HurtboxData] {gameObject.name} collider should be a trigger. Setting to trigger.");
                   col.isTrigger = true;
               }
           }

           private void OnValidate()
           {
               // Auto-assign owner if not set
               if (ownerCharacter == null)
                   ownerCharacter = GetComponentInParent<CharacterController>()?.gameObject;
           }
       }
   }
   ```

3. **Create HitboxData component:**

   ```csharp
   using UnityEngine;
   using System.Collections.Generic;
   using Knockout.Characters.Data;

   namespace Knockout.Combat.HitDetection
   {
       /// <summary>
       /// Defines a hitbox on an attacking limb (hand, foot, etc.).
       /// Enabled during attack's active frames, detects collisions with hurtboxes.
       /// </summary>
       [RequireComponent(typeof(Collider))]
       public class HitboxData : MonoBehaviour
       {
           [Header("Hitbox Properties")]
           [SerializeField] [Tooltip("The character this hitbox belongs to")]
           private GameObject ownerCharacter;

           [Header("Current Attack Data")]
           private AttackData _currentAttack;
           private HashSet<GameObject> _hitTargets = new HashSet<GameObject>();

           private Collider _collider;

           public GameObject OwnerCharacter => ownerCharacter;

           private void Awake()
           {
               _collider = GetComponent<Collider>();

               // Hitboxes start disabled
               _collider.enabled = false;

               // Ensure collider is trigger
               if (!_collider.isTrigger)
               {
                   Debug.LogWarning($"[HitboxData] {gameObject.name} collider should be a trigger. Setting to trigger.");
                   _collider.isTrigger = true;
               }
           }

           public void ActivateHitbox(AttackData attackData)
           {
               _currentAttack = attackData;
               _hitTargets.Clear();
               _collider.enabled = true;
           }

           public void DeactivateHitbox()
           {
               _collider.enabled = false;
               _currentAttack = null;
               _hitTargets.Clear();
           }

           private void OnTriggerEnter(Collider other)
           {
               // Check if other collider is a hurtbox
               HurtboxData hurtbox = other.GetComponent<HurtboxData>();
               if (hurtbox == null) return;

               // Don't hit self
               if (hurtbox.OwnerCharacter == ownerCharacter) return;

               // Don't hit same target multiple times in one attack
               if (_hitTargets.Contains(hurtbox.OwnerCharacter)) return;

               // Register hit
               _hitTargets.Add(hurtbox.OwnerCharacter);

               // Calculate hit data
               HitData hitData = CalculateHitData(hurtbox, other.ClosestPoint(transform.position));

               // Send hit event to target
               SendHitToTarget(hurtbox.OwnerCharacter, hitData);
           }

           private HitData CalculateHitData(HurtboxData hurtbox, Vector3 hitPoint)
           {
               if (_currentAttack == null)
               {
                   Debug.LogError("[HitboxData] No current attack data when calculating hit!");
                   return default;
               }

               // Calculate damage with hurtbox multiplier
               float baseDamage = _currentAttack.Damage;
               float finalDamage = baseDamage * hurtbox.DamageMultiplier;

               // Determine hit direction (from attacker to target)
               Vector3 hitDirection = (hurtbox.transform.position - transform.position).normalized;

               // Determine hit type
               int hitType = hurtbox.HitTypeOverride >= 0 ? hurtbox.HitTypeOverride : DetermineHitType(finalDamage);

               return new HitData(
                   attacker: ownerCharacter,
                   damage: finalDamage,
                   knockback: _currentAttack.Knockback,
                   hitPoint: hitPoint,
                   hitDirection: hitDirection,
                   hitType: hitType,
                   attackName: _currentAttack.AttackName
               );
           }

           private int DetermineHitType(float damage)
           {
               // Simple damage-based hit type
               if (damage < 15f) return 0; // Light
               if (damage < 30f) return 1; // Medium
               return 2; // Heavy
           }

           private void SendHitToTarget(GameObject target, HitData hitData)
           {
               // Find CharacterHealth component on target
               var characterHealth = target.GetComponent<Knockout.Characters.Components.CharacterHealth>();
               if (characterHealth != null)
               {
                   characterHealth.TakeDamage(hitData);
               }
               else
               {
                   Debug.LogWarning($"[HitboxData] Target {target.name} has no CharacterHealth component!");
               }
           }

           private void OnValidate()
           {
               if (ownerCharacter == null)
                   ownerCharacter = GetComponentInParent<CharacterController>()?.gameObject;
           }
       }
   }
   ```

**Verification Checklist:**
- [ ] All three scripts created and compile without errors
- [ ] HitData struct has all required fields
- [ ] HurtboxData and HitboxData components have RequireComponent attributes
- [ ] Auto-validation in OnValidate() works

**Testing Instructions:**
Create edit mode tests for data structures:

```csharp
// File: Assets/Knockout/Tests/EditMode/Combat/HitDataTests.cs
[Test]
public void HitData_Constructor_SetsAllFields()
{
    // Arrange & Act
    GameObject attacker = new GameObject();
    HitData hitData = new HitData(
        attacker: attacker,
        damage: 20f,
        knockback: 1.5f,
        hitPoint: Vector3.zero,
        hitDirection: Vector3.forward,
        hitType: 1,
        attackName: "TestAttack"
    );

    // Assert
    Assert.AreEqual(attacker, hitData.Attacker);
    Assert.AreEqual(20f, hitData.Damage);
    Assert.AreEqual(1.5f, hitData.Knockback);
    Assert.AreEqual(1, hitData.HitType);
    Assert.AreEqual("TestAttack", hitData.AttackName);

    // Cleanup
    Object.DestroyImmediate(attacker);
}
```

**Commit Message Template:**
```
feat(combat): create hit detection data structures

- Add HitData struct for hit event information
- Add HurtboxData component for damage regions
- Add HitboxData component for attack hitboxes
- Implement hit detection logic in OnTriggerEnter
- Calculate damage with hurtbox multipliers
- Prevent multiple hits per attack on same target
- Add edit mode tests for hit data
```

**Estimated Tokens:** ~12,000

---

### Task 2: Add Hitboxes and Hurtboxes to Character Prefab

**Goal:** Add collider GameObjects with HitboxData and HurtboxData components to the character prefab.

**Files to Modify:**
- `Assets/Knockout/Prefabs/Characters/PlayerCharacter.prefab`
- `Assets/Knockout/Prefabs/Characters/AICharacter.prefab`

**Prerequisites:**
- Task 1 complete (HitboxData and HurtboxData components exist)

**Implementation Steps:**

1. **Open PlayerCharacter prefab.**

2. **Create hurtbox colliders:**
   - Expand "Hurtboxes" child GameObject (created in Phase 1)
   - Create child GameObject under Hurtboxes: "Hurtbox_Head"
     - Add Sphere Collider
       - Is Trigger: True
       - Center: (0, 1.6, 0) - approximate head position
       - Radius: 0.15
     - Add HurtboxData component
       - Damage Multiplier: 1.5 (headshots do more damage)
       - Hit Type Override: -1 (use attack's type)
       - Owner Character: (auto-assigned to prefab root)

   - Create child GameObject: "Hurtbox_Body"
     - Add Capsule Collider
       - Is Trigger: True
       - Center: (0, 1.0, 0)
       - Radius: 0.25
       - Height: 0.8
     - Add HurtboxData component
       - Damage Multiplier: 1.0
       - Hit Type Override: -1
       - Owner Character: (auto-assigned)

3. **Create hitbox colliders:**
   - Expand "Hitboxes" child GameObject
   - Create child GameObject: "Hitbox_LeftHand"
     - Add Sphere Collider
       - Is Trigger: True
       - Center: (0, 0, 0) - will be positioned by animation rig
       - Radius: 0.1
       - **Enabled: False** (starts disabled)
     - Add HitboxData component
       - Owner Character: (auto-assigned)

   - Create child GameObject: "Hitbox_RightHand"
     - Same as LeftHand

4. **Position hitboxes on hands:**
   - **Option A: Manual positioning** (if no animation rigging):
     - Position Hitbox_LeftHand at approximate left hand position during idle
     - Position Hitbox_RightHand at right hand position
     - Hitboxes will move with character but not track hands precisely

   - **Option B: Constraint-based positioning** (recommended):
     - Select Hitbox_LeftHand GameObject
     - Add component: Parent Constraint
       - Add Source: Find "LeftHand" bone in character rig hierarchy
       - Weight: 1.0
       - Activate constraint
       - Now hitbox follows left hand bone
     - Repeat for Hitbox_RightHand with "RightHand" bone

5. **Verify setup:**
   - In Scene view, hurtbox colliders should be visible as green wireframes (when GameObject selected)
   - Hitbox colliders visible as yellow/blue wireframes
   - Colliders should roughly match character proportions

6. **Test collider placement:**
   - Enter Play mode
   - Select character in Hierarchy
   - In Scene view, verify hurtbox positions match head and body
   - Manually enable a hitbox collider (via Inspector)
   - Verify it follows hand position

7. **Save prefab changes.**

8. **Apply to AICharacter prefab:**
   - Open AICharacter prefab
   - Repeat steps 2-7 (or overrides should inherit from PlayerCharacter if it's a prefab variant)

**Verification Checklist:**
- [ ] Hurtbox_Head and Hurtbox_Body created with correct colliders
- [ ] Hitbox_LeftHand and Hitbox_RightHand created
- [ ] All colliders are triggers
- [ ] Hitbox colliders start disabled
- [ ] HurtboxData components have correct damage multipliers
- [ ] HitboxData components reference owner character
- [ ] Hitboxes constrained to hand bones (if using Parent Constraint)
- [ ] Changes saved to both prefabs

**Testing Instructions:**
Manual verification in Scene view and Play mode. Visual inspection of collider gizmos is sufficient.

**Commit Message Template:**
```
feat(combat): add hitbox and hurtbox colliders to character prefabs

- Create head and body hurtbox colliders with HurtboxData
- Create left and right hand hitbox colliders with HitboxData
- Set head hurtbox to 1.5x damage multiplier
- Constrain hitboxes to hand bones for accurate tracking
- Configure all colliders as triggers
- Apply setup to both player and AI prefabs
```

**Estimated Tokens:** ~10,000

---

### Task 3-8: Combat System Components (Condensed)

**Remaining tasks for Phase 3 (condensed format):**

**Task 3: Create Combat State Machine** (~15k tokens)
- Create `CombatState.cs` abstract base class with Enter/Update/Exit/CanTransitionTo methods
- Implement states: IdleState, AttackingState, BlockingState, DodgingState, HitStunnedState, KnockedDownState, KnockedOutState
- Create `CombatStateMachine.cs` to manage state transitions
- Each state validates transitions and updates CharacterAnimator accordingly

**Task 4: Create CharacterCombat Component** (~18k tokens)
- Create `Assets/Knockout/Scripts/Characters/Components/CharacterCombat.cs`
- Manages combat state machine, processes attack inputs, controls hitbox activation
- Subscribe to CharacterAnimator events (OnHitboxActivate, OnAttackEnd, etc.)
- Implement methods: ExecuteAttack(AttackData), StartBlocking(), StopBlocking()
- Activate/deactivate hitboxes via HitboxData references
- Track current attack for hitbox data

**Task 5: Create CharacterHealth Component** (~16k tokens)
- Create `Assets/Knockout/Scripts/Characters/Components/CharacterHealth.cs`
- Manages current health, applies damage, triggers death
- Implement TakeDamage(HitData) method - apply damage multipliers, blocking reduction
- Trigger hit reactions based on damage amount
- Events: OnHealthChanged, OnDeath, OnHitTaken
- Integration with CharacterAnimator for hit reactions

**Task 6: Create CharacterMovement Component** (~14k tokens)
- Create `Assets/Knockout/Scripts/Characters/Components/CharacterMovement.cs`
- Handles locomotion, rotation toward opponent
- Apply movement input to Rigidbody or Transform
- Selective root motion application (attacks use root motion, locomotion doesn't)
- Implement OnAnimatorMove() to control root motion

**Task 7: Create CharacterInput Component** (~16k tokens)
- Create `Assets/Knockout/Scripts/Characters/Components/CharacterInput.cs`
- Subscribe to KnockoutInputActions (from Phase 1)
- Raise events for movement, attacks, defense that other components subscribe to
- Enable/disable input based on character state
- Events: OnMoveInput(Vector2), OnJabPressed, OnHookPressed, OnUppercutPressed, OnBlockPressed/Released, OnDodgePressed

**Task 8: Integrate All Components** (~16k tokens)
- Update CharacterController to cache all component references
- Wire up event subscriptions between components
- CharacterInput → CharacterMovement (movement)
- CharacterInput → CharacterCombat (attacks, defense)
- CharacterCombat → CharacterAnimator (trigger animations)
- CharacterAnimator → CharacterCombat (animation events)
- HitboxData → CharacterHealth (damage)
- Add all components to character prefabs
- Test full integration with player-controlled character attacking stationary AI

---

## Phase Verification

**All Tasks Complete:**
- [ ] Hit detection data structures created
- [ ] Hitboxes and hurtboxes added to prefabs
- [ ] Combat state machine implemented
- [ ] CharacterCombat, CharacterHealth, CharacterMovement, CharacterInput components created
- [ ] All components integrated and wired up
- [ ] Player can move, attack, block using keyboard/mouse input
- [ ] Attacks trigger hitboxes, deal damage to opponent
- [ ] Damage reduces health, triggers hit reactions

**Integration Points:**
- [ ] Input System → CharacterInput → CharacterMovement/CharacterCombat
- [ ] CharacterCombat → CharacterAnimator → Animation system
- [ ] Hit detection → CharacterHealth → Hit reactions
- [ ] All components communicate via events (loose coupling)

**Tests Passing:**
- [ ] All EditMode and PlayMode tests pass
- [ ] Manual gameplay test: Player can fight stationary AI dummy

**Known Limitations:**
- AI opponent is stationary (Phase 4 will add AI)
- No camera follow (can add in Phase 5)
- Basic combat feel (Phase 5 will polish)

---

**Phase 3 Complete!** Proceed to [Phase 4: AI Opponent Foundation](Phase-4.md).
