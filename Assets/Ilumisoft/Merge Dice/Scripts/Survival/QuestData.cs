using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[System.Serializable]
public class TileRequirement
{
    [BoxGroup("Tile Requirement")]
    [HorizontalGroup("Tile Requirement/Row")]
    [LabelWidth(80)]
    [Range(0, 10)]
    [Tooltip("The level of tile that needs to be destroyed")]
    public int tileLevel;
    
    [HorizontalGroup("Tile Requirement/Row")]
    [LabelWidth(80)]
    [MinValue(1)]
    [Tooltip("How many tiles of this level need to be destroyed")]
    public int targetAmount;
    
    [BoxGroup("Tile Requirement")]
    [ReadOnly]
    [ShowInInspector]
    [PropertyTooltip("Level info with dice visual properties")]
    public string LevelInfo => $"Level {tileLevel} - {targetAmount} required";
    
    // TODO: Enable visual previews once Unity compiles DiceLevelManager
    // Will show dice sprites and colors based on level
}

[CreateAssetMenu(fileName = "QuestData", menuName = "Survival/QuestData", order = 1)]
public class QuestData : ScriptableObject
{
    [TitleGroup("Quest Information")]
    [LabelWidth(80)]
    [Tooltip("The display name of the quest")]
    public string title;
    
    [TitleGroup("Quest Information")]
    [LabelWidth(80)]
    [TextArea(2, 4)]
    [Tooltip("Detailed description of what the player needs to do")]
    public string description;
    
    [TitleGroup("Requirements")]
    [TableList(ShowIndexLabels = true, DrawScrollView = true, MaxScrollViewHeight = 300)]
    [Tooltip("List of tile requirements that must be completed")]
    public TileRequirement[] requirements;
    
    [FoldoutGroup("Quest Settings")]
    [LabelWidth(100)]
    [Tooltip("Time limit for this quest in seconds (0 = no limit)")]
    [MinValue(0)]
    public float timeLimit = 0f;
    
    [FoldoutGroup("Quest Settings")]
    [LabelWidth(100)]
    [Tooltip("Priority of this quest (higher numbers appear first)")]
    public int priority = 0;
    
    [FoldoutGroup("Quest Settings")]
    [LabelWidth(100)]
    [Tooltip("Can this quest be skipped by the player?")]
    public bool canBeSkipped = false;
    
    [FoldoutGroup("Rewards")]
    [LabelWidth(100)]
    [Tooltip("Score bonus awarded for completing this quest")]
    [MinValue(0)]
    public int scoreReward = 100;
    
    [FoldoutGroup("Rewards")]
    [LabelWidth(100)]
    [Tooltip("Time bonus awarded for completing this quest (in seconds)")]
    [MinValue(0)]
    public float timeBonus = 0f;
    
    [TitleGroup("Quest Status", "Configuration Only - Progress tracked at runtime")]
    [ReadOnly]
    [ShowInInspector]
    [PropertyTooltip("Total tiles that need to be destroyed")]
    public int TotalTilesRequired
    {
        get
        {
            int total = 0;
            if (requirements != null)
            {
                foreach (var req in requirements)
                {
                    total += req.targetAmount;
                }
            }
            return total;
        }
    }
    
    [TitleGroup("Quest Status")]
    [ReadOnly]
    [ShowInInspector]
    [PropertyTooltip("Number of different tile levels required")]
    public int RequirementCount => requirements != null ? requirements.Length : 0;
    
    
    [Button(ButtonSizes.Large)]
    [GUIColor(0.8f, 1f, 0.8f)]
    [PropertyTooltip("Add a new tile requirement to this quest")]
    private void AddRequirement()
    {
        if (requirements == null)
        {
            requirements = new TileRequirement[1];
        }
        else
        {
            System.Array.Resize(ref requirements, requirements.Length + 1);
        }
        requirements[requirements.Length - 1] = new TileRequirement();
    }
    
    [InfoBox("This quest has no requirements! Add at least one requirement.", InfoMessageType.Warning)]
    [ShowIf("@requirements == null || requirements.Length == 0")]
    [Button("Quick Setup - Easy Quest")]
    private void SetupEasyQuest()
    {
        title = "Destroy Basic Tiles";
        description = "Destroy tiles to clear the board.";
        requirements = new TileRequirement[]
        {
            new TileRequirement { tileLevel = 0, targetAmount = 5 },
            new TileRequirement { tileLevel = 1, targetAmount = 3 }
        };
        scoreReward = 150;
        timeLimit = 60f;
    }
    
    [Button("Quick Setup - Medium Quest")]
    [ShowIf("@requirements == null || requirements.Length == 0")]
    private void SetupMediumQuest()
    {
        title = "Mixed Tile Challenge";
        description = "Destroy a variety of different tile levels.";
        requirements = new TileRequirement[]
        {
            new TileRequirement { tileLevel = 1, targetAmount = 8 },
            new TileRequirement { tileLevel = 2, targetAmount = 5 },
            new TileRequirement { tileLevel = 3, targetAmount = 2 }
        };
        scoreReward = 300;
        timeLimit = 90f;
    }
    
    [Button("Quick Setup - Hard Quest")]
    [ShowIf("@requirements == null || requirements.Length == 0")]
    private void SetupHardQuest()
    {
        title = "High Level Challenge";
        description = "Focus on destroying higher level tiles.";
        requirements = new TileRequirement[]
        {
            new TileRequirement { tileLevel = 3, targetAmount = 10 },
            new TileRequirement { tileLevel = 4, targetAmount = 5 },
            new TileRequirement { tileLevel = 5, targetAmount = 2 }
        };
        scoreReward = 500;
        timeLimit = 120f;
        priority = 1;
    }
}
