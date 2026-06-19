using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

/// <summary>
/// Editor script to create the InitialsEntryOverlay PanelSettings asset.
/// Run with: Unity.exe -batchMode -quit -projectPath &lt;path&gt; -executeMethod CreateInitialsEntryPanelSettings.CreateAndExit
/// </summary>
public class CreateInitialsEntryPanelSettings
{
    [MenuItem("Tools/Create InitialsEntryOverlay Panel Settings")]
    public static void CreateAndExit()
    {
        string assetPath = "Assets/Game/UI/InitialsEntryOverlayPanelSettings.asset";

        // Check if already exists
        var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(assetPath);
        if (existing != null)
        {
            Debug.Log("PanelSettings already exists at " + assetPath + ", updating sort order.");
            var existingSo = new SerializedObject(existing);
            existingSo.FindProperty("m_SortingOrder").intValue = 3;
            existingSo.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("Sort order updated to 3.");
            EditorApplication.Exit(0);
            return;
        }

        // Create new PanelSettings
        var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        panelSettings.name = "InitialsEntryOverlayPanelSettings";

        // Set sort order to 3 (above GameOverScreen at 2)
        var so = new SerializedObject(panelSettings);
        so.FindProperty("m_SortingOrder").intValue = 3;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Save the asset
        AssetDatabase.CreateAsset(panelSettings, assetPath);
        AssetDatabase.SaveAssets();

        Debug.Log("Created PanelSettings at " + assetPath + " with sort order 3.");
        EditorApplication.Exit(0);
    }
}
