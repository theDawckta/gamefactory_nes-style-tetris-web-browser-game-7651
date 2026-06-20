using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

/// <summary>
/// Editor-only script to validate scene wiring.
/// Run via: Unity -batchmode -projectPath &lt;path&gt; -executeMethod SceneWiringValidator.RunAndExit
/// </summary>
public static class SceneWiringValidator
{
    [MenuItem("Tools/Validate Scene Wiring %&v")]
    public static void RunAndExit()
    {
        Run();
        EditorApplication.Exit(0);
    }

    public static void Run()
    {
        Debug.Log("[SceneWiring] Starting validation...");

        // Load the Main scene
        var scene = SceneManager.GetSceneAt(0);
        if (!scene.isLoaded)
        {
            scene = EditorSceneManager.OpenScene("Assets/Game/Scenes/Main.unity");
        }

        int nullCount = 0;

        // Check all GameObjects in the scene
        foreach (var go in scene.GetRootGameObjects())
        {
            CheckGameObject(go, ref nullCount);
        }

        Debug.Log("[SceneWiring] Validation complete. NULL fields found: " + nullCount);

        if (nullCount > 0)
        {
            Debug.LogError("[SceneWiring] FAILED: " + nullCount + " NULL fields found");
        }
        else
        {
            Debug.Log("[SceneWiring] PASSED: No NULL fields found");
        }
    }

    private static void CheckGameObject(GameObject go, ref int nullCount)
    {
        // Check all MonoBehaviours on this GameObject
        var behaviours = go.GetComponents<MonoBehaviour>();
        foreach (var behaviour in behaviours)
        {
            var so = new SerializedObject(behaviour);
            var iterator = so.GetIterator();

            while (iterator.NextVisible(true))
            {
                // Skip internal Unity properties
                if (iterator.name.StartsWith("m_") && !iterator.name.StartsWith("m_ParentUI") && !iterator.name.StartsWith("m_WorldSpaceCollider"))
                {
                    continue;
                }

                // Skip array size properties
                if (iterator.name == "Array.size")
                {
                    continue;
                }

                // Check object reference properties for NULL
                if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (iterator.objectReferenceValue == null)
                    {
                        Debug.Log("[SceneWiring] NULL: " + iterator.name + " on " + behaviour.GetType().Name + " in " + go.name);
                        nullCount++;
                    }
                }
            }
        }

        // Recurse into children
        for (int i = 0; i < go.transform.childCount; i++)
        {
            CheckGameObject(go.transform.GetChild(i).gameObject, ref nullCount);
        }
    }
}
