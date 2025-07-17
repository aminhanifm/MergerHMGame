# Survival Game Mode Improvements

This document outlines the changes made to implement randomized, easily combinable tiles in the survival game mode with the ability to combine at least 2 tiles instead of 3.

## Key Changes Made:

### 1. Minimum Selection Size Reduced to 2 Tiles
- **File Modified**: `GameRules.cs`
- **Change**: Changed `MinSelectionSize` from 3 to 2
- **Effect**: Players can now combine just 2 matching tiles instead of requiring 3

### 2. Destructive Merge Behavior in Survival Mode
- **File Modified**: `MergeSelection.cs`
- **Change**: Added survival mode detection in `LevelUp()` method
- **Effect**: In survival mode, merged tiles are destroyed instead of leveling up the last tile
- **Benefit**: Prevents progression bloat and maintains randomized tile generation

### 3. Survival-Optimized Scoring System
- **File Modified**: `MergeSelection.cs`
- **Change**: Added survival-specific scoring in `IncreaseScore()` method
- **Effect**: Score is based on number of tiles merged (10 points per tile) with bonuses for larger merges
- **Bonuses**: 
  - 4+ tiles = 2x multiplier
  - 6+ tiles = 4x multiplier (2x applied twice)

### 4. Strategic Tile Generation
- **File Modified**: `DiceLevelBehaviour.cs`
- **Change**: Added `GetSurvivalRandomLevel()` method with weighted probability
- **Effect**: 
  - 70% chance to spawn levels 0-1 (easy to combine)
  - 30% chance to spawn levels 2-3 (moderate challenge)
- **Benefit**: Ensures there are always easily combinable tiles on the board

### 5. Enhanced Fill Operation for Survival Mode
- **File Modified**: `FillEmptyCells.cs`
- **Change**: Added survival mode detection and strategic level assignment
- **Effect**: New tiles spawned to fill empty cells use survival-optimized levels
- **Benefit**: Maintains good tile distribution throughout gameplay

### 6. Survival Game Mode Board Management
- **File Modified**: `SurvivalGameMode.cs`
- **Changes**:
  - Added survival mode settings (destructiveMerge, maxTileLevel, lowLevelBias)
  - Added `ApplySurvivalSettingsToBoard()` method
  - Added `GetStrategicLevel()` method
  - Enhanced board reset functionality
- **Effect**: Proper initialization and management of survival-specific tile generation

## How It Works:

1. **Game Start**: When survival mode starts, all tiles are assigned strategic levels using weighted randomization
2. **Tile Generation**: New tiles (from fills or resets) favor lower levels (0-1) for easier combinations
3. **Merging**: Players can merge 2+ tiles of the same level
4. **Destruction**: All merged tiles are destroyed (no leveling up)
5. **Scoring**: Points awarded based on merge size with bonuses for larger merges
6. **Refill**: Empty spaces are filled with strategically generated tiles

## Benefits:

- **Easier Gameplay**: Reduced minimum merge size from 3 to 2 tiles
- **Better Balance**: Strategic tile generation ensures combinable tiles are always available
- **No Progression Bloat**: Destructive merging prevents tiles from becoming too high level
- **Engaging Scoring**: Rewards larger merges while keeping base gameplay accessible
- **Sustainable Loop**: Board resets maintain good tile distribution

## Technical Implementation:

The changes are implemented using runtime detection of the SurvivalGameMode component, making them non-intrusive to other game modes. When a SurvivalGameMode is detected:

- Tile generation uses survival-optimized algorithms
- Merge operations become destructive
- Scoring uses survival-specific calculations
- All existing functionality remains intact for other game modes
