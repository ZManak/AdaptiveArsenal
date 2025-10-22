# Weather-Based Weapon Mechanics Testing Guide

This guide describes how to manually test the new weather-based weapon mechanics implemented in AdaptiveArsenal.

## Features to Test

### 1. Weapon Wetness Detection
**Test Scenario:** Player falls into water
- **Expected Behavior:** 
  - Weapon in hands becomes wet
  - Wetness level increases
  - Weapon state changes from Dry to Wet

**How to Test:**
1. Equip a rifle or revolver
2. Fall into freezing water (lake, river, etc.)
3. Check weapon condition (the weapon should now be wet)

### 2. Weapon Drying
**Test Scenario:** Wet weapon dries over time
- **Expected Behavior:**
  - Indoors or in warm weather: weapon dries faster
  - In moderate conditions: weapon dries slowly
  - In cold conditions: weapon may not dry or may freeze

**How to Test:**
1. Get weapon wet (see test 1)
2. Move indoors or to a warm location
3. Wait and observe wetness level decreasing
4. Compare drying speed indoors vs outdoors

### 3. Weapon Freezing
**Test Scenario:** Wet weapon freezes in cold conditions
- **Expected Behavior:**
  - Wet weapon in cold weather (< -5°C) starts to freeze
  - Freezing level increases over time
  - Once frozen (≥90% frozen), weapon cannot be used

**How to Test:**
1. Get weapon wet
2. Stay outdoors in very cold weather (below -5°C)
3. Wait for weapon to freeze
4. Try to fire the weapon - should be blocked

### 4. Weapon Thawing
**Test Scenario:** Frozen weapon thaws when warmed
- **Expected Behavior:**
  - Frozen weapon brought indoors or to warmth thaws
  - Once thawed, weapon can be used again

**How to Test:**
1. Get weapon frozen (see test 3)
2. Go indoors or to a warm shelter
3. Wait for weapon to thaw
4. Weapon should become usable again

### 5. Increased Jamming
**Test Scenario:** Wet weapons jam more frequently
- **Expected Behavior:**
  - Dry weapon: normal jamming rate
  - Wet weapon: 1.5x to 2x jamming rate
  - Freezing weapon: 2x to 3.5x jamming rate

**How to Test:**
1. Fire weapon when dry - note jamming frequency
2. Get weapon wet and fire multiple times
3. Compare jamming rates (wet should jam more often)

### 6. Weather-Based Accuracy Reduction
**Test Scenario:** Cold weather affects accuracy
- **Expected Behavior:**
  - In normal weather: standard accuracy
  - In cold weather (< 0°C): 10% accuracy reduction
  - In very cold weather (< -15°C): additional 5% reduction

**How to Test:**
1. Fire weapon in moderate weather
2. Fire weapon in cold weather
3. Compare shot spread and accuracy

### 7. Weather-Based Damage Reduction
**Test Scenario:** Extreme cold and wet weapons deal less damage
- **Expected Behavior:**
  - Normal conditions: standard damage
  - Extreme cold (< -20°C): 5% damage reduction
  - Wet weapon: 10% damage reduction
  - Combined: up to 15% reduction

**How to Test:**
1. Shoot an animal in normal conditions - note damage
2. Get weapon wet and shoot in extreme cold
3. Compare damage dealt

### 8. Accelerated Deterioration
**Test Scenario:** Weather and wetness affect deterioration
- **Expected Behavior:**
  - Cold weather: 15% faster deterioration
  - Wet weapon: 25% faster deterioration
  - Combined: significantly faster wear

**How to Test:**
1. Note weapon condition percentage
2. Use weapon in various conditions over time
3. Compare deterioration rates

## Test Checklist

- [ ] Weapon becomes wet when player falls in water
- [ ] Weapon dries faster indoors than outdoors
- [ ] Wet weapon freezes in cold conditions (< -5°C)
- [ ] Frozen weapon cannot be fired
- [ ] Frozen weapon thaws when brought to warmth
- [ ] Wet/freezing weapons jam more frequently
- [ ] Cold weather reduces accuracy
- [ ] Extreme cold and wetness reduce damage
- [ ] Cold weather and wetness accelerate deterioration
- [ ] All mechanics work correctly for both rifles and revolvers

## Expected Edge Cases

1. **Multiple water entries:** Wetness should stack up to 100%
2. **Rapid temperature changes:** Weapon should respond appropriately
3. **Flare gun:** Should not be affected by weather mechanics
4. **Weapon switching:** Condition should persist when switching weapons

## Notes

- The system uses simplified indoor detection based on temperature
- Water detection is based on freezing component activity
- All percentages and rates are configurable in the source code
