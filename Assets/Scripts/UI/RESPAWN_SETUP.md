# Respawn System Setup Guide

## Overview
The respawn system allows players to respawn at their death location after dying. It consists of two main components:

1. **RespawnManager** - Handles respawn logic
2. **DeathRespawnUI** - Displays death screen and respawn option

## Setup Instructions

### 1. RespawnManager Setup
1. Create an empty GameObject in your scene (name it "RespawnManager")
2. Add the `RespawnManager` component to it
3. The RespawnManager will automatically:
   - Listen for player death events
   - Save the death position
   - Handle respawn logic

**Settings:**
- `respawnDelay`: Delay before respawn option appears (default: 2 seconds)
- `respawnAtDeathLocation`: Whether to respawn at death location (default: true)

### 2. DeathRespawnUI Setup
1. Create a Canvas in your scene (if you don't have one)
2. Create a Panel as a child of the Canvas (name it "DeathPanel")
3. Add the `DeathRespawnUI` component to the DeathPanel
4. In the Inspector, set up the following UI elements:

**Required UI Elements:**
- `deathPanel`: The main panel GameObject (usually the same GameObject)
- `respawnButton`: A Button component for clicking to respawn
- `deathText`: TextMeshProUGUI showing "You Died" message
- `respawnPromptText`: TextMeshProUGUI showing "Do you want to revive? Press [J] to Revive"

**Optional Settings:**
- `deathMessage`: Custom death message (default: "You Died")
- `respawnPrompt`: Custom respawn prompt (default: "Do you want to revive?\nPress [J] to Revive")
- `showDelay`: Delay before showing respawn option (default: 2 seconds)

### 3. UI Layout Example
```
Canvas
└── DeathPanel (Panel with DeathRespawnUI component)
    ├── DeathText (TextMeshProUGUI)
    ├── RespawnPromptText (TextMeshProUGUI)
    └── RespawnButton (Button)
```

### 4. How It Works
1. When player dies:
   - Death animation plays
   - RespawnManager saves death position
   - Death screen shows "You Died" message
   - After delay, shows "Do you want to revive? Press [J] to Revive"
   - Player can press [J] or click button to revive

2. When player revives:
   - Player respawns at death location
   - Health is restored to full
   - All player state is reset (movement, attacks, etc.)
   - Ammo is reloaded
   - 3 seconds of invincibility (to prevent immediate re-death)
   - Death screen is hidden

## Controls
- **J Key**: Revive/Respawn (when death screen is visible)
- **Respawn Button**: Click to revive/respawn

**Note:** The J key is used instead of R because R is already used for shooting/ranged attacks.

## Notes
- The RespawnManager persists across scenes (DontDestroyOnLoad)
- The player respawns at the exact location where they died
- All player state (health, ammo, position) is fully reset on respawn

