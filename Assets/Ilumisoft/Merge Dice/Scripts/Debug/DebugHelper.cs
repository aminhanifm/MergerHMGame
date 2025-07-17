using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Ilumisoft.MergeDice.Operations;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ilumisoft.MergeDice
{
    public class DebugHelper : MonoBehaviour
    {
        [SerializeField]
        private GameBoard gameBoard;

        [SerializeField]
        private GameMode gameMode;

        [SerializeField]
        private float solveDelay = 0.5f; // Delay between automatic moves

        private ISelection selection;
        private SelectableCounter selectableCounter;
        private bool isAutoSolving = false;

        private void Start()
        {
            // Delay the initialization slightly to ensure all components are properly set up
            Invoke("InitializeReferences", 0.5f);
        }

        private void InitializeReferences()
        {
            Debug.Log("DebugHelper: Initializing references...");
            
            // Find references if not assigned
            if (gameBoard == null)
            {
                gameBoard = FindFirstObjectByType<GameBoard>();
                Debug.Log("DebugHelper: GameBoard found: " + (gameBoard != null));
            }

            if (gameMode == null)
            {
                gameMode = FindFirstObjectByType<GameMode>();
                Debug.Log("DebugHelper: GameMode found: " + (gameMode != null));
            }

            if (gameMode != null)
            {
                // Get the private 'selection' field from GameMode
                FieldInfo selectionField = gameMode.GetType().GetField("selection", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (selectionField != null)
                {
                    selection = selectionField.GetValue(gameMode) as ISelection;
                    Debug.Log("DebugHelper: Selection found: " + (selection != null));
                }
                else
                {
                    Debug.LogError("DebugHelper: Could not find 'selection' field in GameMode");
                }
            }
            
            // Create selectable counter for finding matching tiles
            if (gameBoard != null)
            {
                selectableCounter = new SelectableCounter(gameBoard);
                Debug.Log("DebugHelper: SelectableCounter created");
            }
            else
            {
                Debug.LogError("DebugHelper: Cannot create SelectableCounter, GameBoard is null");
            }

            Debug.Log($"DebugHelper: References - GameBoard: {gameBoard != null}, GameMode: {gameMode != null}, Selection: {selection != null}, SelectableCounter: {selectableCounter != null}");
        }

        /// <summary>
        /// Finds the best match on the board and automatically selects those tiles
        /// </summary>
        public void AutoSolveOnce()
        {
            Debug.Log("DebugHelper: Attempting to auto-solve one step");
            
            // Try to re-initialize if any references are missing
            if (gameBoard == null || selection == null || selectableCounter == null)
            {
                Debug.LogWarning("DebugHelper: Missing references. Re-initializing...");
                InitializeReferences();
                
                if (gameBoard == null || selection == null || selectableCounter == null)
                {
                    Debug.LogError("DebugHelper: Cannot auto-solve - missing references");
                    return;
                }
            }

            // Clear existing selection
            selection.Clear();
            
            // Find all possible tiles that can be matched
            List<GameTile> bestMatch = FindBestMatch();
            
            // If we found a match, select those tiles
            if (bestMatch.Count >= GameRules.MinSelectionSize)
            {
                foreach (var tile in bestMatch)
                {
                    selection.Add(tile);
                }
                
                Debug.Log($"DebugHelper: Auto-selected {bestMatch.Count} matching tiles");
            }
            else
            {
                Debug.Log("DebugHelper: No valid matches found");
            }
        }        /// <summary>
        /// Find the best matching set of tiles on the board
        /// </summary>
        private List<GameTile> FindBestMatch()
        {
            List<GameTile> bestMatch = new List<GameTile>();
            
            Debug.Log("Starting FindBestMatch search");
            
            // Use sequential order to ensure we check every tile properly
            // We'll randomize only for equal-size matches
            List<GameTile> allTiles = new List<GameTile>();
            
            Debug.Log($"Scanning board of size {gameBoard.Width}x{gameBoard.Height}");
            
            // First collect all existing tiles
            for (int x = 0; x < gameBoard.Width; x++)
            {
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    if (TryGetGameTile(gameBoard.GetPosition(x, y), out GameTile tile))
                    {
                        if (tile != null && !tile.IsDestroyed)
                        {
                            allTiles.Add(tile);
                            
                            if (tile is DiceGameTile diceTile)
                            {
                                Debug.Log($"Found tile at ({x}, {y}) with level {diceTile.CurrentLevel}");
                            }
                            else
                            {
                                Debug.Log($"Found tile at ({x}, {y}) of type {tile.GetType().Name}");
                            }
                        }
                    }
                }
            }
            
            Debug.Log($"Found {allTiles.Count} valid tiles on board");
            
            // Randomize the order to ensure we don't always pick the same pattern
            Shuffle(allTiles);
            
            // Special handling for the beginning of the game - check if all tiles are the same level
            // This is especially important at game start when the board is filled with level 1 tiles
            if (allTiles.Count > 0 && allTiles[0] is DiceGameTile firstDice)
            {
                bool allSameLevel = true;
                int firstLevel = firstDice.CurrentLevel;
                
                Debug.Log($"Checking if all tiles are the same level (Level {firstLevel})");
                
                // Check if all tiles are the same level
                foreach (var tile in allTiles)
                {
                    if (tile is DiceGameTile diceTile)
                    {
                        if (diceTile.CurrentLevel != firstLevel)
                        {
                            allSameLevel = false;
                            break;
                        }
                    }
                    else
                    {
                        allSameLevel = false;
                        break;
                    }
                }
                
                // If all tiles are the same level, just find 3 adjacent ones
                if (allSameLevel)
                {
                    Debug.Log("All tiles are the same level! Finding first set of adjacent matches");
                    
                    // Try each tile as a starting point
                    foreach (var startTile in allTiles)
                    {
                        List<GameTile> adjacentMatches = new List<GameTile>();
                        adjacentMatches.Add(startTile);
                        
                        // Find adjacent tiles by checking all other tiles for proximity
                        foreach (var otherTile in allTiles)
                        {
                            if (otherTile == startTile) continue; // Skip self
                            
                            float distance = Vector3.Distance(startTile.transform.position, otherTile.transform.position);
                            
                            // If within adjacent distance (using a slightly larger threshold)
                            if (distance <= gameBoard.CellSize * 1.1f)
                            {
                                adjacentMatches.Add(otherTile);
                                
                                // If we have enough matches, return this set
                                if (adjacentMatches.Count >= GameRules.MinSelectionSize)
                                {
                                    Debug.Log($"Found simple adjacent match set with {adjacentMatches.Count} tiles");
                                    return adjacentMatches;
                                }
                            }
                        }
                    }
                }
            }
            
            // First pass - check every tile
            foreach (var startTile in allTiles)
            {
                // Find all simple adjacent matches for this tile
                List<GameTile> matches = FindSimpleAdjacentMatches(startTile);
                
                Debug.Log($"Found {matches.Count} matches for tile at {startTile.transform.position}");
                
                // If we found a good match, use it
                if (matches.Count > bestMatch.Count)
                {
                    Debug.Log($"New best match found with {matches.Count} tiles");
                    bestMatch = new List<GameTile>(matches); // Make a copy
                }
            }
              // If we didn't find a match with the minimum size, check every tile again
            // with a higher search distance
            if (bestMatch.Count < GameRules.MinSelectionSize)
            {
                Debug.Log("No matches found with minimum size. Trying with higher distance tolerance.");
                
                // First try a direct adjacency check for same-level tiles
                if (allTiles.Count >= GameRules.MinSelectionSize)
                {
                    // Check if all tiles are the same level
                    bool allSameLevel = true;
                    int tileLevel = -1;
                    
                    if (allTiles[0] is DiceGameTile firstDiceInner)
                    {
                        tileLevel = firstDiceInner.CurrentLevel;
                        
                        for (int i = 1; i < allTiles.Count; i++)
                        {
                            if (allTiles[i] is DiceGameTile nextDice)
                            {
                                if (nextDice.CurrentLevel != tileLevel)
                                {
                                    allSameLevel = false;
                                    break;
                                }
                            }
                            else
                            {
                                allSameLevel = false;
                                break;
                            }
                        }
                    }
                    
                    if (allSameLevel && tileLevel >= 0)
                    {
                        Debug.Log($"All tiles are same level ({tileLevel}). Finding any adjacent group.");
                        
                        // Find the smallest distance between any tiles
                        float minDistance = float.MaxValue;
                        for (int i = 0; i < allTiles.Count; i++)
                        {
                            for (int j = i + 1; j < allTiles.Count; j++)
                            {
                                float dist = Vector3.Distance(allTiles[i].transform.position, allTiles[j].transform.position);
                                if (dist < minDistance) minDistance = dist;
                            }
                        }
                        
                        Debug.Log($"Smallest distance between tiles: {minDistance}");
                        float searchDistance = Mathf.Max(minDistance * 1.1f, gameBoard.CellSize * 1.1f);
                        
                        // Now find any group of adjacent tiles
                        for (int i = 0; i < allTiles.Count; i++)
                        {
                            List<GameTile> adjacentGroup = new List<GameTile>();
                            adjacentGroup.Add(allTiles[i]);
                            
                            for (int j = 0; j < allTiles.Count; j++)
                            {
                                if (i == j) continue; // Skip self
                                
                                float dist = Vector3.Distance(allTiles[i].transform.position, allTiles[j].transform.position);
                                if (dist <= searchDistance)
                                {
                                    adjacentGroup.Add(allTiles[j]);
                                    
                                    if (adjacentGroup.Count >= GameRules.MinSelectionSize)
                                    {
                                        Debug.Log($"Found adjacent group with {adjacentGroup.Count} tiles");
                                        bestMatch = adjacentGroup;
                                        return bestMatch;
                                    }
                                }
                            }
                        }
                    }
                }
                
                // Fall back to radius search as a last resort
                foreach (var startTile in allTiles)
                {
                    List<GameTile> matches = FindSameTypeTilesInRadius(startTile, gameBoard.CellSize * 2.0f);
                    
                    if (matches.Count > bestMatch.Count)
                    {
                        bestMatch = new List<GameTile>(matches);
                        Debug.Log($"Found fallback match with {matches.Count} tiles");
                    }
                }
            }
            
            return bestMatch;
        }
        
        /// <summary>        /// Find all adjacent matching tiles using a simple algorithm that only connects directly adjacent tiles
        /// </summary>
        private List<GameTile> FindSimpleAdjacentMatches(GameTile startTile)
        {
            List<GameTile> result = new List<GameTile>();
            HashSet<GameTile> visited = new HashSet<GameTile>();
            
            // Abort if invalid start tile
            if (startTile == null || startTile.IsDestroyed)
            {
                Debug.LogWarning("FindSimpleAdjacentMatches: Start tile is null or destroyed");
                return result;
            }
            
            // Add the start tile to our results and visited set
            result.Add(startTile);
            visited.Add(startTile);
            
            // Log starting tile info
            if (startTile is DiceGameTile diceTile)
            {
                Debug.Log($"Starting match search with tile level: {diceTile.CurrentLevel} at {startTile.transform.position}");
            }
            else 
            {
                Debug.Log($"Starting match search with tile of type {startTile.GetType()} at {startTile.transform.position}");
            }
            
            // Queue for breadth-first search
            Queue<GameTile> queue = new Queue<GameTile>();
            queue.Enqueue(startTile);
            
            // Only look in 4 orthogonal directions (no diagonals)
            Vector2[] directions = new Vector2[]
            {
                Vector2.up,
                Vector2.right,
                Vector2.down,
                Vector2.left
            };
              // Use a more appropriate distance based on actual cell size
            float maxDistance = gameBoard.CellSize * 1.1f;
            Debug.Log($"Using max distance of {maxDistance} (cell size: {gameBoard.CellSize})");
            
            // Perform breadth-first search
            while (queue.Count > 0)
            {
                GameTile current = queue.Dequeue();
                
                foreach (var direction in directions)
                {
                    // Cast a ray in each direction, looking for adjacent tiles
                    var raycast = new GameTileRaycast(current.transform.position, direction, maxDistance);
                    
                    if (raycast.Perform(out GameTile neighbor))
                    {
                        // Debug each potential neighbor
                        Debug.Log($"Found potential neighbor at direction {direction} from {current.transform.position}");
                        
                        // Skip invalid or already visited tiles
                        if (neighbor == null)
                        {
                            Debug.Log("Neighbor is null, skipping");
                            continue;
                        }
                        
                        if (neighbor.IsDestroyed)
                        {
                            Debug.Log("Neighbor is destroyed, skipping");
                            continue;
                        }
                        
                        if (visited.Contains(neighbor))
                        {
                            Debug.Log("Neighbor already visited, skipping");
                            continue;
                        }
                            
                        // Calculate the exact distance between tiles
                        float distance = Vector3.Distance(current.transform.position, neighbor.transform.position);
                        
                        // Ensure tiles are truly adjacent
                        if (distance <= maxDistance)
                        {
                            Debug.Log($"Distance check passed: {distance} <= {maxDistance}");
                            
                            // Check if tiles match and log the result
                            bool matches = IsSameType(startTile, neighbor);
                            Debug.Log($"Type check: {(matches ? "MATCH" : "NO MATCH")}");
                            
                            if (matches)
                            {
                                // Add this neighbor to our results and visited set
                                result.Add(neighbor);
                                visited.Add(neighbor);
                                queue.Enqueue(neighbor);
                                
                                Debug.Log($"Found adjacent match at {neighbor.transform.position}, distance: {distance}");
                            }
                        }
                        else
                        {
                            Debug.Log($"Distance too far: {distance} > {maxDistance}");
                        }
                    }
                    else
                    {
                        Debug.Log($"No tile found in direction {direction} from {current.transform.position}");
                    }
                }
            }
            
            Debug.Log($"Found {result.Count} adjacent matching tiles");
            return result;
        }
        
        /// <summary>
        /// Continuously solves the puzzle until game over
        /// </summary>
        public void StartAutoSolve()
        {
            if (!isAutoSolving)
            {
                isAutoSolving = true;
                StartCoroutine(AutoSolveRoutine());
                Debug.Log("DebugHelper: Started auto-solving");
            }
        }

        /// <summary>
        /// Stop automatic solving
        /// </summary>
        public void StopAutoSolve()
        {
            if (isAutoSolving)
            {
                isAutoSolving = false;
                StopAllCoroutines();
                Debug.Log("DebugHelper: Stopped auto-solving");
            }
        }
        
        private IEnumerator AutoSolveRoutine()
        {
            while (isAutoSolving)
            {
                AutoSolveOnce();
                
                if (selection.Count >= GameRules.MinSelectionSize)
                {
                    // Simulate tapping on the last tile to trigger the merge operation
                    SimulateTapOnLastTile();
                    
                    // Wait for the game to process the merge and animations
                    yield return new WaitForSeconds(solveDelay);
                }
                else
                {
                    // No valid moves found, stop solving
                    Debug.Log("DebugHelper: No more valid moves found, stopping auto-solve");
                    isAutoSolving = false;
                    yield break;
                }
            }
        }
        
        // Simulate a tap on the last tile in the selection to trigger merge operations
        private void SimulateTapOnLastTile()
        {
            if (selection.Count >= GameRules.MinSelectionSize)
            {
                GameTile lastTile = selection.GetLast();
                if (lastTile != null)
                {
                    Debug.Log($"Simulating tap on last tile at {lastTile.transform.position}");
                    
                    // Find the operations queue and execute it
                    Debug.Log("Looking for operations field in DefaultGameMode");
                    var fields = gameMode.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                    FieldInfo operationsField = null;
                    
                    foreach (var field in fields)
                    {
                        Debug.Log($"Found field: {field.Name} of type {field.FieldType}");
                        if (field.Name == "operations")
                        {
                            operationsField = field;
                            break;
                        }
                    }
                    
                    if (operationsField == null)
                    {
                        // Try with a different case (might be capitalized differently)
                        foreach (var field in fields)
                        {
                            if (field.Name.ToLower() == "operations")
                            {
                                Debug.Log("Found operations field with different case");
                                operationsField = field;
                                break;
                            }
                        }
                    }
                    
                    if (operationsField != null)
                    {
                        var operations = operationsField.GetValue(gameMode);
                        Debug.Log($"Got operations object: {operations}");
                        
                        var executeMethod = operations.GetType().GetMethod("Execute");
                        
                        if (executeMethod != null)
                        {
                            Debug.Log("DebugHelper: Executing operations to merge selection");
                            StartCoroutine((IEnumerator)executeMethod.Invoke(operations, null));
                        }
                        else
                        {
                            Debug.LogError("DebugHelper: Couldn't find Execute method");
                        }
                    }
                    else
                    {
                        Debug.LogError("DebugHelper: Couldn't find operations field");

                        // Last resort method - try to invoke the operations using known names
                        var gameManagerType = gameMode.GetType();
                        var runGameMethod = gameManagerType.GetMethod("RunGame", BindingFlags.Public | BindingFlags.Instance);
                        if (runGameMethod != null)
                        {
                            Debug.Log("Trying to run the game loop directly");
                            StartCoroutine((IEnumerator)runGameMethod.Invoke(gameMode, null));
                        }
                    }
                }
            }
        }        // Check if two tiles are of the same type/level
        private bool IsSameType(GameTile tile1, GameTile tile2)
        {
            // Validate tiles
            if (tile1 == null || tile2 == null)
            {
                Debug.Log("IsSameType: One of the tiles is null");
                return false;
            }
            
            if (tile1.IsDestroyed || tile2.IsDestroyed)
            {
                Debug.Log("IsSameType: One of the tiles is destroyed");
                return false;
            }
            
            // First check - if they're the exact same tile
            if (tile1 == tile2)
            {
                Debug.Log("IsSameType: Same tile reference, returning false");
                return false; // Don't match a tile with itself
            }
            
            // Log details about the tiles being compared
            string tile1Type = "Unknown";
            string tile2Type = "Unknown";
            int tile1Level = -1;
            int tile2Level = -1;
            
            if (tile1 is DiceGameTile dice1)
            {
                tile1Type = "DiceGameTile";
                tile1Level = dice1.CurrentLevel;
            }
            
            if (tile2 is DiceGameTile dice2)
            {
                tile2Type = "DiceGameTile";
                tile2Level = dice2.CurrentLevel;
            }
            
            Debug.Log($"Comparing tiles: Tile1({tile1Type}, Level:{tile1Level}) with Tile2({tile2Type}, Level:{tile2Level})");
            
            // First check using direct level comparison for DiceGameTiles
            if (tile1 is DiceGameTile d1 && tile2 is DiceGameTile d2)
            {
                bool levelsMatch = d1.CurrentLevel == d2.CurrentLevel;
                Debug.Log($"Direct level comparison: {d1.CurrentLevel} == {d2.CurrentLevel} = {levelsMatch}");
                if (levelsMatch) return true;
            }
            
            // Fallback to using the game's built-in validator
            var validator = new LevelValidator();
            bool isValid = validator.IsValid(tile1, tile2);
            
            // Log the result of the validation
            Debug.Log($"LevelValidator.IsValid returned {isValid}");
            
            return isValid;
        }

        private bool TryGetGameTile(Vector3 position, out GameTile gameTile)
        {
            var raycast = new GameTileRaycast(position, Vector2.zero, 0);
            return raycast.Perform(out gameTile);
        }        // Returns a randomized order of grid cells to check
        private List<Vector2Int> GetRandomizedCellOrder()
        {
            List<Vector2Int> cells = new List<Vector2Int>();
            
            for (int x = 0; x < gameBoard.Width; x++)
            {
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    cells.Add(new Vector2Int(x, y));
                }
            }
            
            // Fisher-Yates shuffle
            for (int i = cells.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                var temp = cells[i];
                cells[i] = cells[j];
                cells[j] = temp;
            }
            
            return cells;
        }
        
        /// <summary>
        /// Generic shuffle method for any list type
        /// </summary>
        private void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
          /// <summary>
        /// Fallback method to find tiles of the same type within a specific radius
        /// </summary>
        private List<GameTile> FindSameTypeTilesInRadius(GameTile startTile, float radius)
        {
            List<GameTile> result = new List<GameTile>();
            HashSet<GameTile> visited = new HashSet<GameTile>();
            
            if (startTile == null || startTile.IsDestroyed)
            {
                Debug.LogWarning("FindSameTypeTilesInRadius: Start tile is null or destroyed");
                return result;
            }
            
            Debug.Log($"Finding same type tiles within radius {radius} of {startTile.transform.position}");
            
            // Add start tile
            result.Add(startTile);
            visited.Add(startTile);
            
            // Find all tiles and check their distance
            List<GameTile> nearbyTiles = new List<GameTile>();
            
            for (int x = 0; x < gameBoard.Width; x++)
            {
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    Vector3 worldPos = gameBoard.GetPosition(x, y);
                    
                    if (TryGetGameTile(worldPos, out GameTile tile))
                    {
                        // Skip invalid or already visited tiles
                        if (tile == null || tile.IsDestroyed || visited.Contains(tile))
                            continue;
                            
                        // Calculate distance
                        float distance = Vector3.Distance(startTile.transform.position, tile.transform.position);
                        
                        // Add to nearby tiles if within radius
                        if (distance <= radius)
                        {
                            nearbyTiles.Add(tile);
                        }
                    }
                }
            }
            
            // Sort by distance (closest first)
            nearbyTiles.Sort((a, b) => 
                Vector3.Distance(a.transform.position, startTile.transform.position)
                .CompareTo(Vector3.Distance(b.transform.position, startTile.transform.position)));
                
            // Check each nearby tile
            foreach (var tile in nearbyTiles)
            {
                // Check if it matches the start tile
                if (IsSameType(startTile, tile))
                {
                    // Calculate distance
                    float distance = Vector3.Distance(startTile.transform.position, tile.transform.position);
                    
                    // Log that we're adding this match
                    Debug.Log($"Added matching tile at distance: {distance}, current count: {result.Count + 1}");
                    
                    // Add this tile to our result
                    result.Add(tile);
                    visited.Add(tile);
                    
                    // If we have enough matches, return early
                    if (result.Count >= GameRules.MinSelectionSize)
                    {
                        Debug.Log($"Found sufficient matches ({result.Count}) - returning early");
                        return result;
                    }
                }
            }
            
            Debug.Log($"Found {result.Count} matching tiles within radius {radius}");
            return result;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DebugHelper))]
    public class DebugHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            DebugHelper debugHelper = (DebugHelper)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Initialize References"))
            {
                debugHelper.SendMessage("InitializeReferences");
            }
            
            if (GUILayout.Button("Auto Solve (One Step)"))
            {
                debugHelper.AutoSolveOnce();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Start Auto Solving"))
            {
                debugHelper.StartAutoSolve();
            }
            
            if (GUILayout.Button("Stop Auto Solving"))
            {
                debugHelper.StopAutoSolve();
            }
        }
    }
#endif
}
