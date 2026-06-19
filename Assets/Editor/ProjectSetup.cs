using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using System.IO;

/// <summary>
/// Editor-only script to initialize the Unity 6 WebGL project for NES Tetris.
/// Run via: Unity -batchmode -projectPath &lt;path&gt; -executeMethod ProjectSetup.RunAndExit
/// This script is deleted after execution.
/// </summary>
public static class ProjectSetup
{
    [MenuItem("Tools/InitializeProject %&i")]
    public static void RunAndExit()
    {
        Debug.Log("[ProjectSetup] Running project initialization...");

        // 1. Select WebGL build target
        var prevTarget = EditorUserBuildSettings.activeBuildTarget;
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            Debug.Log("[ProjectSetup] Switched active build target to WebGL");
        }
        else
        {
            Debug.Log("[ProjectSetup] Already on WebGL target");
        }

        // 2. Configure Player Settings for WebGL
        // Color Space: Gamma (0) for NES palette fidelity
        if (PlayerSettings.colorSpace != ColorSpace.Gamma)
        {
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            Debug.Log("[ProjectSetup] Set color space to Gamma");
        }
        else
        {
            Debug.Log("[ProjectSetup] Color space already Gamma");
        }

        // General player settings
        if (PlayerSettings.productName != "" && !PlayerSettings.productName.Contains("tetris") && !PlayerSettings.productName.Contains("Tetris"))
        {
            PlayerSettings.productName = "NES-Style Tetris";
            Debug.Log("[ProjectSetup] Set product name");
        }

        if (PlayerSettings.companyName != "" && !PlayerSettings.companyName.Contains("onetime"))
        {
            PlayerSettings.companyName = "OneTime Games";
            Debug.Log("[ProjectSetup] Set company name");
        }

        Debug.Log("[ProjectSetup] Player settings configured");

        // 3. Create folders if they don't exist
        string[] folders = new string[]
        {
            "Assets/Game/Scripts/UI",
            "Assets/Game/Scripts/Gameplay",
            "Assets/Game/Scenes",
            "Assets/Game/Art",
            "Assets/Editor"
        };

        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Debug.Log($"[ProjectSetup] Created directory: {folder}");
            }
            else
            {
                Debug.Log($"[ProjectSetup] Directory already exists: {folder}");
            }
        }

        // 4. Create empty Main scene
        string scenePath = "Assets/Game/Scenes/Main.unity";
        bool createScene = true;

        if (File.Exists(scenePath))
        {
            Debug.Log($"[ProjectSetup] Scene already exists: {scenePath}");
            createScene = false;
        }

        if (createScene)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[ProjectSetup] Created empty scene: {scenePath}");
        }

        // 5. Set Main scene as build scene
        var buildScenes = EditorBuildSettings.scenes;
        bool sceneInBuildList = false;
        foreach (var bs in buildScenes)
        {
            if (bs.path == scenePath)
            {
                sceneInBuildList = true;
                break;
            }
        }

        if (!sceneInBuildList)
        {
            var newScenes = new EditorBuildSettingsScene[buildScenes.Length + 1];
            System.Array.Copy(buildScenes, newScenes, buildScenes.Length);
            newScenes[buildScenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
            Debug.Log($"[ProjectSetup] Added {scenePath} to build settings");
        }
        else
        {
            Debug.Log($"[ProjectSetup] Scene already in build settings");
        }

        // 6. Force save all modified settings
        EditorUtility.SetDirty(null);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ProjectSetup] Project initialization complete!");

        // 7. Delete this editor script's file and exit
        string scriptPath = "Assets/Editor/ProjectSetup.cs";
        if (File.Exists(scriptPath))
        {
            var metaPath = scriptPath + ".meta";
#if UNITY_EDITOR_WIN
            System.IO.File.Delete(metaPath);
#endif
            System.IO.File.Delete(scriptPath);
            Debug.Log($"[ProjectSetup] Deleted editor script: {scriptPath}");
        }

        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }
}
