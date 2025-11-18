# ScriptableObjects Setup Instructions

## Overview
CharacterStats and AttackData ScriptableObject classes have been created. This document explains how to create the actual asset instances in Unity Editor.

## Required Assets to Create

### 1. BaseCharacterStats Asset

**Location:** `Assets/Knockout/Scripts/Characters/Data/BaseCharacterStats.asset`

**Steps:**
1. In Unity Project window, navigate to `Assets/Knockout/Scripts/Characters/Data/`
2. Right-click > **Create > Knockout > Character Stats**
3. Name it **"BaseCharacterStats"**
4. In Inspector, set the following values:
   - Max Health: **100**
   - Move Speed: **5**
   - Rotation Speed: **10**
   - Damage Multiplier: **1.0**
   - Damage Taken Multiplier: **1.0**

### 2. AttackData Assets

Create three attack data assets for the basic punch types:

#### AttackData_Jab

**Location:** `Assets/Knockout/Scripts/Characters/Data/AttackData_Jab.asset`

1. Right-click > **Create > Knockout > Attack Data**
2. Name it **"AttackData_Jab"**
3. In Inspector, set:
   - Attack Name: **"Jab"**
   - Damage: **10**
   - Knockback: **0.5**
   - Startup Frames: **6**
   - Active Frames: **3**
   - Recovery Frames: **6**
   - Animation Trigger: **"AttackTrigger"**
   - Attack Type Index: **0**

#### AttackData_Hook

**Location:** `Assets/Knockout/Scripts/Characters/Data/AttackData_Hook.asset`

1. Right-click > **Create > Knockout > Attack Data**
2. Name it **"AttackData_Hook"**
3. In Inspector, set:
   - Attack Name: **"Hook"**
   - Damage: **18**
   - Knockback: **1.5**
   - Startup Frames: **10**
   - Active Frames: **4**
   - Recovery Frames: **12**
   - Animation Trigger: **"AttackTrigger"**
   - Attack Type Index: **1**

#### AttackData_Uppercut

**Location:** `Assets/Knockout/Scripts/Characters/Data/AttackData_Uppercut.asset`

1. Right-click > **Create > Knockout > Attack Data**
2. Name it **"AttackData_Uppercut"**
3. In Inspector, set:
   - Attack Name: **"Uppercut"**
   - Damage: **30**
   - Knockback: **3.0**
   - Startup Frames: **15**
   - Active Frames: **5**
   - Recovery Frames: **18**
   - Animation Trigger: **"AttackTrigger"**
   - Attack Type Index: **2**

## Verification

After creating all assets, run the tests in `Assets/Knockout/Tests/EditMode/Characters/CharacterDataTests.cs`:

1. Open **Window > General > Test Runner**
2. Select **EditMode** tab
3. Find and run **CharacterDataTests**
4. Tests marked as [Ignore] should now pass

## Frame Data Explanation

Frame data is measured at **60 frames per second (fps)**:

- **Startup Frames:** Time before the hitbox becomes active (vulnerable period)
- **Active Frames:** Window when the attack can hit the opponent
- **Recovery Frames:** Time after the attack before character can act again (vulnerable period)

**Example (Jab):**
- 6 startup + 3 active + 6 recovery = 15 total frames
- At 60fps: 15 frames = 0.25 seconds total

## Design Philosophy

**Jab:** Fast, low damage, safe
**Hook:** Medium speed, medium damage, moderate risk
**Uppercut:** Slow, high damage, high risk

These values can be tuned during playtesting (Phase 5).

## Reference

See Phase-0.md for the full Combat System Design and frame data philosophy.
