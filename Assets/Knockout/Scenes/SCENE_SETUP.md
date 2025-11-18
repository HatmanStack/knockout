# GameplayTest Scene Setup Instructions

## Overview
This document provides step-by-step instructions for creating the GameplayTest scene in Unity Editor. This scene will be used for character development and testing throughout the implementation phases.

## Prerequisites
- Unity Editor 2021.3.8f1 LTS open
- Cinemachine package installed (verify in Package Manager)

## Step 1: Create New Scene

1. **File > New Scene**
2. Choose **"Basic (URP)"** template (since project uses Universal Render Pipeline)
3. **File > Save As...**
4. Save to: `Assets/Knockout/Scenes/GameplayTest.unity`

## Step 2: Set Up Environment

### Ground Plane

1. **GameObject > 3D Object > Plane**
2. Rename to **"Ground"**
3. In Inspector, set Transform:
   - Position: `(0, 0, 0)`
   - Rotation: `(0, 0, 0)`
   - Scale: `(5, 1, 5)` ← Creates a 50x50 unit arena

### Boundary Walls (Optional for Testing)

1. **GameObject > 3D Object > Cube**
2. Rename to **"Wall_North"**
3. Transform:
   - Position: `(0, 2.5, 25)`
   - Scale: `(50, 5, 1)`

4. Duplicate (Ctrl+D) and rename to **"Wall_South"**
   - Position: `(0, 2.5, -25)`

5. Duplicate and rename to **"Wall_East"**
   - Position: `(25, 2.5, 0)`
   - Scale: `(1, 5, 50)`

6. Duplicate and rename to **"Wall_West"**
   - Position: `(-25, 2.5, 0)`
   - Scale: `(1, 5, 50)`

## Step 3: Configure Lighting

### Main Directional Light

1. Select existing **"Directional Light"** (created by default)
2. Rename to **"MainLight"**
3. In Inspector:
   - Rotation: `(50, -30, 0)`
   - Intensity: `1.0`
   - Color: White

### Fill Light

1. **GameObject > Light > Directional Light**
2. Rename to **"FillLight"**
3. In Inspector:
   - Rotation: `(-20, 120, 0)`
   - Intensity: `0.4`
   - Color: Slight blue tint `(R: 0.9, G: 0.95, B: 1.0)`

## Step 4: Set Up Camera with Cinemachine

### Remove Default Camera

1. Select **"Main Camera"** in Hierarchy
2. Press Delete (Cinemachine will create its own)

### Create Virtual Camera

1. **GameObject > Cinemachine > Virtual Camera**
2. Rename to **"GameplayCamera"**
3. In Inspector, configure **CinemachineVirtualCamera** component:

**Priority:**
- Set to `10`

**Body:**
- Select **"Framing Transposer"**
- Camera Distance: `8`
- Camera Side: `0`
- Screen X: `0.5`
- Screen Y: `0.4` ← Character slightly below center
- Dead Zone Width: `0.1`
- Dead Zone Height: `0.1`

**Aim:**
- Select **"Composer"**
- Tracked Object Offset Y: `1.5` ← Aims at chest/head area

**Follow and Look At:**
- Leave empty for now (will assign character prefab in Phase 2)

## Step 5: Create Spawn Points

### Player Spawn Point

1. **GameObject > Create Empty**
2. Rename to **"PlayerSpawnPoint"**
3. Transform:
   - Position: `(-5, 0, 0)`
   - Rotation: `(0, 90, 0)` ← Facing right

### AI Spawn Point

1. **GameObject > Create Empty**
2. Rename to **"AISpawnPoint"**
3. Transform:
   - Position: `(5, 0, 0)`
   - Rotation: `(0, -90, 0)` ← Facing left

### Parent Spawn Points

1. **GameObject > Create Empty**
2. Rename to **"SpawnPoints"**
3. Drag **PlayerSpawnPoint** and **AISpawnPoint** onto **SpawnPoints** to make them children

## Step 6: Organize Hierarchy

Create organizational parent objects for clarity:

### Environment Parent

1. **GameObject > Create Empty**
2. Rename to **"Environment"**
3. Drag **Ground** and all **Wall_*** objects onto **Environment**

### Lighting Parent

1. **GameObject > Create Empty**
2. Rename to **"Lighting"**
3. Drag **MainLight** and **FillLight** onto **Lighting**

### Cameras Parent

1. **GameObject > Create Empty**
2. Rename to **"Cameras"**
3. Drag **GameplayCamera** onto **Cameras**

**Final Hierarchy Structure:**
```
GameplayTest
├── Environment
│   ├── Ground
│   ├── Wall_North
│   ├── Wall_South
│   ├── Wall_East
│   └── Wall_West
├── Lighting
│   ├── MainLight
│   └── FillLight
├── Cameras
│   └── GameplayCamera
└── SpawnPoints
    ├── PlayerSpawnPoint
    └── AISpawnPoint
```

## Step 7: Configure Scene Settings

### Lighting Settings

1. **Window > Rendering > Lighting**
2. **Environment** tab:
   - Skybox Material: Use default or URP skybox
   - Sun Source: MainLight
   - Environment Lighting > Source: Skybox
   - Intensity Multiplier: `1.0`

3. For now, keep everything **realtime** (faster iteration)
   - Mixed Lighting > Baked Global Illumination: ☐ Unchecked
   - This can be optimized in Phase 5

### Quality Settings

1. **Edit > Project Settings > Quality**
2. Ensure URP Quality is set appropriately for development

## Step 8: Save Scene

1. **File > Save** (Ctrl+S)
2. Verify scene is saved at `Assets/Knockout/Scenes/GameplayTest.unity`

## Verification

After completing setup:

- [ ] Scene loads without errors
- [ ] Ground plane is visible
- [ ] Walls create a bounded arena
- [ ] Lighting looks balanced (not too dark, not blown out)
- [ ] Cinemachine virtual camera exists
- [ ] Spawn points are positioned correctly (facing each other)
- [ ] Hierarchy is organized with parent GameObjects
- [ ] No console errors

## Testing

To verify Cinemachine is working:

1. Temporarily drag a primitive object (e.g., Cube) into the scene
2. Position it at `(0, 1, 0)`
3. Assign it to GameplayCamera's **Follow** and **Look At** fields
4. Enter **Play Mode**
5. The camera should frame the cube
6. Exit Play Mode and delete the test cube

## Next Steps

After scene creation:
- **Phase 2:** Character prefabs will be placed at spawn points
- **Phase 2:** GameplayCamera will be assigned to follow the player character
- **Phase 3:** Combat testing will use this scene
- **Phase 4:** AI testing will spawn AI at AISpawnPoint

## Reference

See Phase-1.md Task 6 for the full scene creation specification.
