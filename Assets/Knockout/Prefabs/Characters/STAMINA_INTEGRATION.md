# Character Prefab Stamina Integration (Task 10)

## Required Action

Add CharacterStamina component to character prefabs when opening project in Unity Editor.

---

## Steps to Complete Task 10:

### 1. Locate Character Prefab

Character prefabs should be in one of these locations:
- `Assets/Knockout/Prefabs/Characters/BaseCharacter.prefab`
- `Assets/Knockout/Prefabs/Characters/PlayerCharacter.prefab`
- `Assets/Knockout/Prefabs/Characters/AICharacter.prefab`

**Note:** If prefabs don't exist yet, they will be created in a future phase. Skip this task until prefabs are available.

### 2. Add CharacterStamina Component

For each character prefab:

1. Open prefab in Unity Editor
2. Select root GameObject
3. Add Component > Scripts > Knockout.Characters.Components > CharacterStamina
4. Assign dependencies:
   - **Stamina Data:** DefaultStamina (or character-specific asset)
5. Verify component appears in Inspector

### 3. Verify Initialization Order

CharacterStamina requires:
- ✅ CharacterCombat component (already present)
- ✅ CharacterAnimator component (already present)
- ✅ Animator component (already present)

Component initialization handled automatically by Unity lifecycle:
- `Awake()` - Caches references
- `Start()` - Initializes stamina and subscribes to events

### 4. Test in Play Mode

1. Open scene with character
2. Enter Play Mode
3. Select character in Hierarchy
4. Verify in Inspector:
   - Current Stamina = 100 (max)
   - Stamina Percentage = 1.0 (100%)
   - Is Depleted = false
5. Perform attacks - stamina should decrease
6. Wait idle - stamina should regenerate

---

## Expected Prefab Structure

After integration, character prefab should have:

```
BaseCharacter (GameObject)
├── Animator (Unity Component)
├── Rigidbody (Unity Component)
├── CharacterController (Knockout Component)
├── CharacterAnimator (Knockout Component)
├── CharacterInput (Knockout Component)
├── CharacterMovement (Knockout Component)
├── CharacterCombat (Knockout Component)
│   └── References: CharacterStamina (auto-cached)
├── CharacterHealth (Knockout Component)
├── CharacterStamina (Knockout Component) ← NEW
│   ├── Stamina Data: DefaultStamina.asset
│   └── References: CharacterCombat (auto-cached)
└── CharacterAI (Knockout Component - if AI character)
```

---

## Validation Checklist

After adding CharacterStamina to prefabs:

- [ ] Component visible in Inspector
- [ ] Stamina Data field assigned
- [ ] No missing script warnings
- [ ] No missing reference warnings
- [ ] Play Mode: Stamina initializes to max
- [ ] Play Mode: Attacks consume stamina
- [ ] Play Mode: Stamina regenerates when idle
- [ ] Play Mode: Exhaustion triggers at 0 stamina
- [ ] Console: No errors during initialization
- [ ] Console: No errors during gameplay

---

## Troubleshooting

### "CharacterStamina component not found"

**Solution:**
1. Rebuild Unity project (Assets > Reimport All)
2. Check `Assets/Knockout/Scripts/Characters/Components/CharacterStamina.cs` exists
3. Verify no compile errors in Console

### "Stamina Data field is null"

**Solution:**
1. Create DefaultStamina asset (see `Assets/Knockout/Data/Stamina/README.md`)
2. Assign asset to CharacterStamina component
3. Save prefab

### "Missing reference to CharacterCombat"

**Solution:**
1. Ensure CharacterCombat component is on same GameObject
2. CharacterStamina.Awake() auto-caches reference via GetComponent
3. If still missing, check component execution order

### "Stamina not regenerating in Play Mode"

**Solution:**
1. Check character is not attacking (regeneration pauses during attacks)
2. Verify StaminaData.RegenPerSecond > 0
3. Check for ExhaustedState (regeneration slower during exhaustion)

---

## CLI Environment Note

Character prefabs are binary Unity assets (.prefab files) that cannot be edited via command line.

**Completion Status:**
- ✅ CharacterStamina script complete and tested
- ✅ Integration code in CharacterCombat complete
- ✅ All events and references configured
- ⏳ Prefab component addition - **requires Unity Editor** (this task)

Once Unity Editor is opened and CharacterStamina is added to prefabs, Task 10 will be complete.

---

## Alternative: Test Without Prefabs

If prefabs don't exist yet, stamina system can still be tested:

### Option 1: Runtime GameObject

```csharp
GameObject character = new GameObject("TestCharacter");
character.AddComponent<Animator>();
character.AddComponent<CharacterAnimator>();
character.AddComponent<CharacterCombat>();

CharacterStamina stamina = character.AddComponent<CharacterStamina>();
// Assign stamina data via reflection (for testing)
```

### Option 2: Integration Tests

All functionality validated in:
- `Assets/Knockout/Tests/PlayMode/Stamina/CharacterStaminaTests.cs`
- `Assets/Knockout/Tests/PlayMode/Integration/StaminaIntegrationTests.cs`

Tests create GameObjects at runtime and verify all stamina behavior.

---

## See Also

- Stamina System Documentation: `Assets/Knockout/Scripts/Characters/Components/STAMINA_SYSTEM.md`
- Prefab Setup Guide: `Assets/Knockout/Prefabs/Characters/PREFAB_SETUP.md`
- Phase 1 Implementation Plan: `docs/plans/Phase-1.md`
