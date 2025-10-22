# Weather-Based Weapon Mechanics

This document describes the new weather-based weapon mechanics system implemented in AdaptiveArsenal.

## Overview

The weather-based weapon mechanics system adds realism to weapon behavior by simulating the effects of environmental conditions on firearm performance. Weapons are now affected by:

- **Weather conditions** (temperature, precipitation)
- **Water exposure** (when player falls in water)
- **Freezing mechanics** (wet weapons freeze in cold)

## System Components

### 1. WeaponCondition Component

A component attached to all gun items (except flare guns) that tracks:

- **Wetness Level** (0.0 to 1.0): How wet the weapon is
- **Freezing Level** (0.0 to 1.0): How frozen the weapon is
- **Current State**: Dry, Wet, Freezing, or Frozen

#### Weapon States

```
Dry      - Normal operation
Wet      - Increased jamming, reduced damage
Freezing - High jamming, preparing to freeze
Frozen   - Cannot be used at all
```

### 2. WeatherWeaponModifier System

Provides modifiers for weapon stats based on weather:

#### Accuracy Modifiers
- Cold weather (< 0°C): 10% accuracy reduction
- Very cold weather (< -15°C): Additional 5% reduction

#### Damage Modifiers
- Extreme cold (< -20°C): 5% damage reduction
- Wet weapon: 10% damage reduction
- **Combined**: Up to 15% damage reduction

#### Deterioration Modifiers
- Cold weather (< -10°C): 15% faster deterioration
- Wet weapon: 25% faster deterioration
- **Combined**: Significantly accelerated wear

### 3. Water Detection

The system detects when the player is in water by monitoring:
- Freezing component activity
- Temperature conditions
- Periodic checks every second

When detected, weapons in hands become wet.

## Game Mechanics

### Weapon Wetting

**When it happens:**
- Player falls into water
- Player is swimming
- Detected via freezing mechanics

**Effect:**
- Wetness level increases by 50% per water exposure
- Can stack up to 100% wetness

### Weapon Drying

**Conditions:**
- **Indoors or warm (>10°C)**: 2x drying rate (0.02/second)
- **Moderate (0-10°C)**: Normal drying rate (0.01/second)
- **Freezing (<0°C)**: No drying (may freeze instead)

**Time to dry:**
- Full wet weapon indoors: ~50 seconds
- Full wet weapon in moderate conditions: ~100 seconds

### Weapon Freezing

**Conditions:**
- Weapon is wet (>10% wetness)
- Outdoors in freezing temperature (<-5°C)
- Freezing rate: 0.02/second

**Effect:**
- Once frozen (≥90% frozen level), weapon cannot be fired
- Player will see prevention of firing action

**Thawing:**
- Occurs when brought indoors or to warmth (>0°C)
- Thawing rate: 0.015/second
- Frozen weapon thaws in ~60 seconds when warm

### Increased Jamming

Jamming probability increases based on weapon state:

| State    | Jamming Multiplier | Base Chance (5%) |
|----------|-------------------|------------------|
| Dry      | 1.0x              | 5%               |
| Wet      | 1.5x - 2.0x       | 7.5% - 10%       |
| Freezing | 2.0x - 3.5x       | 10% - 17.5%      |
| Frozen   | ∞ (blocked)       | Cannot fire      |

## Integration with Existing Systems

### Accuracy Calculation

Weather modifiers are applied in `ProjectileItem.CalculateAccuracy()`:
```csharp
accuracyMultiplier *= WeatherWeaponModifier.GetAccuracyModifier();
```

### Damage Calculation

Weather and wetness modifiers applied in `ProjectileItem.TryInflictDamage()`:
```csharp
var weatherModifier = WeatherWeaponModifier.GetDamageModifier(weaponCondition);
damage *= weatherModifier;
```

### Fire Prevention

Frozen weapons are blocked from firing in `vp_FPSShooterPatches.FireProjectile()`:
```csharp
if (weaponCondition != null && weaponCondition.IsFrozen)
{
    return; // Don't fire if frozen
}
```

## Configuration

All parameters are defined as constants in the source code and can be adjusted:

### WeaponCondition.cs
- `WetnessDryingRate`: 0.01 (wetness reduction per second)
- `FreezingRate`: 0.02 (freezing increase per second in cold)
- `ThawingRate`: 0.015 (thawing rate when warm)
- `FreezingTemperature`: -5°C (temp below which wet weapons freeze)
- `DryingTemperature`: 10°C (temp above which weapons dry faster)

### WeatherWeaponModifier.cs
- `BadWeatherAccuracyPenalty`: 0.90 (10% reduction)
- `WindAccuracyPenalty`: 0.95 (5% reduction)
- `ColdDamagePenalty`: 0.95 (5% reduction)
- `WetDamagePenalty`: 0.90 (10% reduction)
- `ColdDeteriorationIncrease`: 1.15 (15% faster)
- `WetDeteriorationIncrease`: 1.25 (25% faster)

## Performance Considerations

- WeaponCondition updates every frame only for active weapons
- Water detection runs once per second
- Minimal performance impact expected
- No save/load persistence (state resets on reload)

## Future Enhancements

Potential improvements for future versions:

1. **Save/Load Support**: Persist weapon wetness/frozen state
2. **Drying Action**: Manual weapon drying near fire
3. **Visual Indicators**: Show weapon state in UI
4. **Frost Effects**: Visual frost buildup on frozen weapons
5. **Sound Effects**: Audio cues for frozen weapon fire attempts
6. **Item-Specific Rates**: Different weapons dry/freeze at different rates
7. **Inventory Weapons**: Track wetness for all inventory items, not just equipped

## Compatibility

This system is designed to work alongside all existing AdaptiveArsenal features:
- ✅ Projectile bullet system
- ✅ Accuracy mechanics
- ✅ Damage calculations
- ✅ HUD animations
- ✅ All weapon types (rifles and revolvers)

## Technical Notes

- Component is attached in `GunItem.Awake()` patch
- Flare guns are excluded from the system
- Uses existing game systems for weather and temperature
- Integrates with Harmony patches for minimal code impact
