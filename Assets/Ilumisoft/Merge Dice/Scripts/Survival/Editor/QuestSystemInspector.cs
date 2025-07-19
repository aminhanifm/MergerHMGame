using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector;

namespace Ilumisoft.MergeDice.Survival
{
    #if UNITY_EDITOR
    [CustomEditor(typeof(QuestSystem))]
    public class QuestSystemInspector : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            QuestSystem questSystem = (QuestSystem)target;
            
            if (Application.isPlaying && questSystem.CurrentQuestProgress != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Runtime Quest Progress", EditorStyles.boldLabel);
                
                // Show quest title and description
                EditorGUILayout.LabelField("Title", questSystem.CurrentQuestProgress.title);
                EditorGUILayout.LabelField("Description", questSystem.CurrentQuestProgress.description, EditorStyles.wordWrappedLabel);
                
                EditorGUILayout.Space();
                
                // Show overall progress
                var overallProgress = questSystem.CurrentQuestProgress.OverallProgress;
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), overallProgress, $"Overall Progress: {overallProgress:P0}");
                
                EditorGUILayout.Space();
                
                // Show individual requirements
                EditorGUILayout.LabelField("Requirements:", EditorStyles.boldLabel);
                
                for (int i = 0; i < questSystem.CurrentQuestProgress.requirements.Length; i++)
                {
                    var req = questSystem.CurrentQuestProgress.requirements[i];
                    
                    EditorGUILayout.BeginHorizontal();
                    
                    // Level indicator
                    EditorGUILayout.LabelField($"Level {req.tileLevel}:", GUILayout.Width(60));
                    
                    // Progress bar
                    var progress = req.ProgressPercentage;
                    var progressRect = EditorGUILayout.GetControlRect();
                    EditorGUI.ProgressBar(progressRect, progress, $"{req.currentAmount}/{req.targetAmount}");
                    
                    // Completion status
                    var statusColor = req.IsComplete ? Color.green : Color.white;
                    var prevColor = GUI.color;
                    GUI.color = statusColor;
                    EditorGUILayout.LabelField(req.IsComplete ? "✓" : "○", GUILayout.Width(20));
                    GUI.color = prevColor;
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.Space();
                
                // Quest completion status
                if (questSystem.CurrentQuestProgress.IsComplete)
                {
                    var prevColor = GUI.color;
                    GUI.color = Color.green;
                    EditorGUILayout.LabelField("QUEST COMPLETE!", EditorStyles.boldLabel);
                    GUI.color = prevColor;
                }
                
                // Debug buttons in play mode
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Debug Actions", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset Progress"))
                {
                    questSystem.ResetCurrentQuestProgress();
                }
                if (GUILayout.Button("Complete Quest"))
                {
                    // Complete all requirements for debugging
                    if (questSystem.CurrentQuestProgress != null)
                    {
                        questSystem.CompleteCurrentQuestDebug();
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Next Day"))
                {
                    questSystem.NextDay();
                }
                EditorGUILayout.EndHorizontal();
                
                // Force repaint to show live updates
                if (Application.isPlaying)
                {
                    EditorUtility.SetDirty(target);
                    Repaint();
                }
            }
            else if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("No active quest", EditorStyles.centeredGreyMiniLabel);
            }
        }
    }
    #endif
}
