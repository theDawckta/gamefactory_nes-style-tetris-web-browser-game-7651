using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

/// <summary>
/// Editor script to create the GameOverScreen PanelSettings asset.
/// Run with: Unity.exe -batchMode -quit -projectPath <path> -executeMethod CreateGameOverPanelSettings.CreateAndExit
/// </summary>
public class CreateGameOverPanelSettings
{
    [MenuItem("Tools/Create GameOverScreen Panel Settings")]
    public static void CreateAndExit()
    {
        string assetPath = "Assets/Game/UI/GameOverScreenPanelSettings.asset";

        // Check if already exists
        var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(assetPath);
        if (existing != null)
        {
            Debug.Log("PanelSettings already exists at " + assetPath + ", updating sort order.");
            var existingSo = new SerializedObject(existing);
            existingSo.FindProperty("m_SortingOrder").intValue = 2;
            existingSo.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("Sort order updated to 2.");
            EditorApplication.Exit(0);
            return;
        }

        // Create new PanelSettings
        var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        panelSettings.name = "GameOverScreenPanelSettings";

        // Set sort order to 2 (above GameScreen at 1)
        var so = new SerializedObject(panelSettings);
        so.FindProperty("m_SortingOrder").intValue = 2;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Save the asset
        AssetDatabase.CreateAsset(panelSettings, assetPath);
        AssetDatabase.SaveAssets();

        Debug.Log("Created PanelSettings at " + assetPath + " with sort order 2.");
        EditorApplication.Exit(0);
    }
}
