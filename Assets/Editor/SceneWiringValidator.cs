using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only script that validates all MonoBehaviour serialized references
/// in the Main scene for null ObjectReference fields.
/// Run via: Unity -batchmode -projectPath &lt;path&gt; -executeMethod SceneWiringValidator.Run
/// </summary>
public static class SceneWiringValidator
{
    /// <summary>
    /// Property names that are expected to be null in certain configurations
    /// and should NOT be flagged as errors.
    /// - m_ParentUI: only used for WorldSpace UIDocuments (null for ScreenSpaceOverlay)
    /// - m_WorldSpaceCollider: only used for WorldSpace UIDocuments
    /// </summary>
    private static readonly HashSet<string> s_IgnoredPropertyNames = new HashSet<string>
    {
        "m_ParentUI",
        "m_WorldSpaceCollider"
    };

    [MenuItem("Tools/Validate Scene Wiring %&v")]
    public static void Run()
    {
        Debug.Log("[SceneWiring] Starting scene wiring validation...");

        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(0);
        if (!scene.isLoaded)
        {
            Debug.LogError("[SceneWiring] No scene is loaded!");
            EditorApplication.Exit(1);
            return;
        }

        int totalNulls = 0;

        // Iterate all root GameObjects in the scene
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            CheckGameObject(root, ref totalNulls);
        }

        if (totalNulls > 0)
        {
            Debug.LogWarning("[SceneWiring] Found " + totalNulls + " null reference(s) in scene.");
            EditorApplication.Exit(1);
        }
        else
        {
            Debug.Log("[SceneWiring] All serialized references are wired correctly.");
            EditorApplication.Exit(0);
        }
    }

    private static void CheckGameObject(GameObject go, ref int totalNulls)
    {
        var components = go.GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            var so = new SerializedObject(component);
            CheckSerializedProperties(so, component, go, ref totalNulls);
        }

        // Recurse into children
        for (int i = 0; i < go.transform.childCount; i++)
        {
            CheckGameObject(go.transform.GetChild(i).gameObject, ref totalNulls);
        }
    }

    private static void CheckSerializedProperties(SerializedObject so, MonoBehaviour component, GameObject go, ref int totalNulls)
    {
        var iterator = so.GetIterator();

        // Iterate over all properties (enter children = true)
        while (iterator.NextVisible(true))
        {
            // Only check ObjectReference properties that are null
            if (iterator.propertyType == SerializedPropertyType.ObjectReference)
            {
                // Skip properties in our ignore list
                if (s_IgnoredPropertyNames.Contains(iterator.name))
                {
                    continue;
                }

                if (iterator.objectReferenceValue == null)
                {
                    Debug.Log("[SceneWiring] NULL: " + iterator.name + " on " +
                        component.GetType().Name + " in " + go.name);
                    totalNulls++;
                }
            }
        }
    }
}
