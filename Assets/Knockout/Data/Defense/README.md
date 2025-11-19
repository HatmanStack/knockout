# Defense Data Assets

This directory contains ScriptableObject asset instances for dodge and parry configuration.

## Required Assets

### DefaultDodge.asset

Create via: Right-click > Create > Knockout > Dodge Data

**Default Configuration:**
- Dodge Duration: 18 frames (~0.3s at 60fps)
- I-Frame Start: 2 frames
- I-Frame Duration: 8 frames
- Dodge Distance: 1.5 units
- Dodge Speed Multiplier: 1.0
- Cooldown: 12 frames

### DefaultParry.asset

Create via: Right-click > Create > Knockout > Parry Data

**Default Configuration:**
- Parry Window: 6 frames (~0.1s before hit)
- Parry Success Duration: 12 frames
- Attacker Stagger Duration: 0.5s
- Counter Window Duration: 0.5s
- Cooldown: 18 frames

## Usage

These default assets serve as templates for character-specific customization.

To create character-specific variants:
1. Duplicate DefaultDodge/DefaultParry
2. Rename to match character (e.g., "FastCharacterDodge")
3. Adjust values for character differentiation
4. Assign to character prefab

## Character Differentiation Examples

### Fast Character
- Shorter dodge duration (15 frames)
- Same i-frame count (higher % invulnerable)
- Shorter cooldown (8 frames)

### Heavy Character
- Longer dodge duration (24 frames)
- Same i-frame count (lower % invulnerable)
- Longer dodge distance (2.0 units)

### Parry Specialist
- Longer parry window (8-10 frames)
- Shorter cooldown (12 frames)
- Longer counter window (0.7s)

See `DEFENSE_SYSTEMS.md` for detailed configuration guide.
