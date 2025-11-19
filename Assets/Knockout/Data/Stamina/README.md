# Stamina Data Assets

## Default Stamina Asset (Task 9)

**Required:** Create `DefaultStamina.asset` in this directory when opening project in Unity Editor.

### Steps to Complete Task 9:

1. Open project in Unity Editor
2. In Project window, navigate to `Assets/Knockout/Data/Stamina/`
3. Right-click > Create > Knockout > Stamina Data
4. Name it: `DefaultStamina`
5. Configure with these values:
   - **Max Stamina:** 100
   - **Regen Per Second:** 25
   - **Attack Costs:** [10 (Jab), 15 (Hook), 20 (Uppercut)]
   - **Special Move Cost:** 40
   - **Exhaustion Duration:** 2.0
   - **Exhaustion Regen Multiplier:** 0.5
   - **Exhaustion Recovery Threshold:** 25

### Expected Result:

File structure should be:
```
Assets/Knockout/Data/Stamina/
├── DefaultStamina.asset
├── DefaultStamina.asset.meta
└── README.md
```

### Verification:

- [ ] Asset appears in Project window
- [ ] Can be assigned to CharacterStamina component
- [ ] Inspector shows all configured values
- [ ] ScriptableObject icon visible in Project window

---

## Character-Specific Stamina Assets

Create additional stamina configurations per character as needed:

**Examples:**
- `HeavyweightStamina.asset` - High max (120), slow regen (20/s), high costs
- `SpeedsterStamina.asset` - Low max (80), fast regen (35/s), low costs
- `BalancedStamina.asset` - Medium values for all-rounder characters

### Template Configuration:

```
Character Type: [Name]
Max Stamina: [80-120]
Regen Per Second: [15-35]
Jab Cost: [8-12]
Hook Cost: [12-18]
Uppercut Cost: [15-25]
Special Move Cost: [30-50]
Exhaustion Duration: [1.5-3.0]
Exhaustion Regen Multiplier: [0.3-0.7]
Exhaustion Recovery Threshold: [20-35]
```

---

## Integration with Character Prefabs

After creating stamina assets, assign to character prefabs:

1. Open character prefab (e.g., `Assets/Knockout/Prefabs/Characters/BaseCharacter.prefab`)
2. Add `CharacterStamina` component (if not present)
3. Assign stamina data asset to "Stamina Data" field
4. Verify component shows in Inspector with correct references

See: `Assets/Knockout/Prefabs/Characters/PREFAB_SETUP.md` for prefab integration details.

---

## CLI Environment Note

This README exists because Unity assets (.asset files) cannot be created via command line.
All scripts and code are complete - only Unity Editor asset creation remains.

**Completion Status:**
- ✅ StaminaData.cs script created and tested
- ✅ CharacterStamina component implemented
- ✅ All integration code complete
- ⏳ DefaultStamina.asset - **requires Unity Editor** (this task)
- ⏳ Prefab integration - **requires Unity Editor** (Task 10)

Once Unity Editor is opened, these assets will be created and Phase 1 will be 100% complete.
