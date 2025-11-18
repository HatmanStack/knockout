# Input System Setup Instructions

## Overview
This document explains how to set up the Unity Input System for the Knockout game. The Input System is already installed (version 1.4.4) but needs to be configured.

## Steps to Complete in Unity Editor

### 1. Enable Input System (IMPORTANT - Must be done first!)

1. Open Unity Editor
2. Go to **Edit > Project Settings > Player**
3. Under "Other Settings", find **Active Input Handling**
4. Set to **"Input System Package (New)"** or **"Both"**
5. Unity will prompt to restart - **Accept and restart**

### 2. Create Input Actions Asset

1. Navigate to `Assets/Knockout/Scripts/Input/`
2. Right-click > **Create > Input Actions**
3. Name it **"KnockoutInputActions"**
4. Double-click to open the Input Actions window

### 3. Configure Action Maps

Create two Action Maps: **Gameplay** and **UI**

#### Gameplay Action Map

Add the following actions:

**Movement** (Value, Vector2):
- Binding: WASD Composite
  - Up: W
  - Down: S
  - Left: A
  - Right: D
- Binding: Left Stick (Gamepad)

**Jab** (Button):
- Binding: Mouse Left Button
- Binding: West Button (Gamepad - X/Square)

**Hook** (Button):
- Binding: Mouse Right Button
- Binding: South Button (Gamepad - A/X)

**Uppercut** (Button):
- Binding: Mouse Middle Button
- Binding: North Button (Gamepad - Y/Triangle)

**Block** (Button):
- Binding: Left Shift (Keyboard)
- Binding: East Button (Gamepad - B/Circle)

#### UI Action Map

**Pause** (Button):
- Binding: Escape (Keyboard)
- Binding: Start Button (Gamepad)

### 4. Generate C# Class

1. In Input Actions window, check **"Generate C# Class"**
2. Set path to: `Assets/Knockout/Scripts/Input/KnockoutInputActions.cs`
3. Click **"Apply"**
4. Unity will generate the C# wrapper class

### 5. Verify Setup

Run the edit mode tests in `Assets/Knockout/Tests/EditMode/Input/InputActionsTests.cs` to verify the setup is correct.

## Architecture Notes

- CharacterInput component will subscribe to these Input Actions
- AI characters will trigger the same events programmatically
- This makes character code input-source agnostic (works for both player and AI)

## Reference

See Phase-0.md ADR-004 for the full Input System architecture decision.
