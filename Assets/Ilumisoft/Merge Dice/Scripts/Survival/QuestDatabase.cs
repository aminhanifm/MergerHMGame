using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Survival/QuestDatabase", order = 2)]
public class QuestDatabase : ScriptableObject
{
    public QuestData[] quests;
}
