# Dark Souls-Style Action Game

A 3D action game built in Unity featuring third-person combat, enemy AI, and responsive controls.

## Features

- **Third-Person Camera**: Smooth camera follow with mouse look and collision handling
- **Combat System**: Light and heavy attacks with cooldowns, damage calculation, and knockback physics
- **Dodge Roll**: Invincibility frames with coyote time for responsive controls
- **Enemy AI**: Detection range, player pursuit, and facing behavior
- **Health System**: Damage and death handling for player and enemies
- **Targeting System**: Lock-on targeting for combat

## Controls

- **WASD** - Movement
- **Mouse** - Camera look
- **Shift** - Run
- **Space** - Jump
- **Left Click** - Light attack
- **1** - Heavy attack
- **Dodge** - Roll with i-frames

## Technical Details

Built with Unity 2022.3 LTS. The project demonstrates:

- Component-based architecture
- CharacterController physics
- Animator state management
- Raycast-based ground detection
- Layer-based collision filtering

## Scripts Overview

| Script | Purpose |
|--------|---------|
| `EnhancedPlayerController.cs` | Camera-relative movement, jumping, dodge roll |
| `ThirdPersonCamera.cs` | Orbit camera with collision |
| `CombatSystem.cs` | Attack handling, damage, knockback |
| `EnemyAI.cs` | Detection and pursuit behavior |
| `Health.cs` | Damage and death system |
| `TargetingSystem.cs` | Lock-on targeting |
| `WeaponSystem.cs` | Weapon management |
| `UIManager.cs` | Health bars and UI |

## Author

Adam Bentley - [github.com/adamfbentley](https://github.com/adamfbentley)
