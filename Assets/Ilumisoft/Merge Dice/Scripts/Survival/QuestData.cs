using UnityEngine;

[System.Serializable]
public class TileRequirement
{
    public int tileLevel;
    public int targetAmount;
}

[CreateAssetMenu(fileName = "QuestData", menuName = "Survival/QuestData", order = 1)]
public class QuestData : ScriptableObject
{
    public string title;
    public string description;
    public TileRequirement[] requirements;
}
