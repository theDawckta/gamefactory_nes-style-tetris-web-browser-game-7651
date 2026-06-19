using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Editor-only script to assemble the Main.unity scene with all gameplay systems,
/// controllers, UI screens, and wiring for NES Tetris.
/// Run via: Unity -batchmode -projectPath &lt;path&gt; -executeMethod AssembleMainScene.RunAndExit
/// </summary>
public static class AssembleMainScene
{
    private const string ScenePath = "Assets/Game/Scenes/Main.unity";
    private const string PanelSettingsPath = "Assets/Game/UI/PanelSettings.asset";
    private const string SpritesPath = "Assets/Game/Art/Sprites";

    [MenuItem("Tools/Assemble Main Scene %&m")]
    public static void RunAndExit()
    {
        Debug.Log("[AssembleMainScene] Starting scene assembly...");

        // 1. Create directories
        EnsureDirectory("Assets/Game/Scenes");
        EnsureDirectory("Assets/Game/UI");
        EnsureDirectory(SpritesPath);

        // 2. Create shared PanelSettings asset
        CreateSharedPanelSettings();

        // 3. Create sprite assets
        Sprite[] blockSprites = CreateBlockSprites();
        Sprite borderSprite = CreateBorderSprite();

        // 4. Create or open the Main scene
        Scene scene;
        if (File.Exists(ScenePath))
        {
            scene = EditorSceneManager.OpenScene(ScenePath);
            Debug.Log("[AssembleMainScene] Opened existing scene: " + ScenePath);
        }
        else
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[AssembleMainScene] Created new scene: " + ScenePath);
        }

        // 5. Remove any existing camera
        var cam = UnityEngine.Object.FindObjectOfType<Camera>();
        if (cam != null)
        {
            UnityEngine.Object.DestroyImmediate(cam.gameObject);
        }

        // 6. Create MainCamera
        CreateCamera();

        // 7. Create GameManager with SceneBootstrapper, GameplayController
        GameObject gameManager = CreateGameManager();

        // 8. Create PlayfieldRenderer
        GameObject playfieldRenderer = CreatePlayfieldRenderer(blockSprites, borderSprite);

        // 9. Create UI screen GameObjects
        var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
        GameObject startScreenUI = CreateStartScreenUI(panelSettings);
        GameObject gameScreenUI = CreateGameScreenUI(panelSettings);
        GameObject gameOverScreenUI = CreateGameOverScreenUI(panelSettings);
        GameObject initialsEntryUI = CreateInitialsEntryUI(panelSettings);

        // 10. Wire up SceneBootstrapper references
        WireSceneBootstrapper(gameManager, startScreenUI, gameScreenUI, gameOverScreenUI, initialsEntryUI, blockSprites);

        // 11. Wire PlayfieldRenderer to GameplayController
        var pr = playfieldRenderer.GetComponent<PlayfieldRenderer>();
        var gc = gameManager.GetComponent<GameplayController>();
        pr.SetGameplayController(gc);

        // 12. Add scene to build settings
        AddSceneToBuildSettings(ScenePath);

        // 13. Save everything
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[AssembleMainScene] Scene assembly complete!");

        // 14. Delete this editor script and exit
        DeleteSelf();

        EditorApplication.Exit(0);
    }

    private static void DeleteSelf()
    {
        string scriptPath = "Assets/Editor/AssembleMainScene.cs";
        string metaPath = scriptPath + ".meta";
        if (File.Exists(metaPath)) File.Delete(metaPath);
        File.Delete(scriptPath);
        AssetDatabase.Refresh();
    }

    // -----------------------------------------------------------------------
    // Directory helpers
    // -----------------------------------------------------------------------

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Debug.Log("[AssembleMainScene] Created directory: " + path);
        }
    }

    // -----------------------------------------------------------------------
    // Shared PanelSettings
    // -----------------------------------------------------------------------

    private static void CreateSharedPanelSettings()
    {
        var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
        if (existing != null)
        {
            Debug.Log("[AssembleMainScene] PanelSettings already exists at " + PanelSettingsPath);
            return;
        }

        var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        panelSettings.name = "PanelSettings";

        var so = new SerializedObject(panelSettings);
        so.FindProperty("m_SortingOrder").intValue = 0;
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(panelSettings, PanelSettingsPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[AssembleMainScene] Created shared PanelSettings at " + PanelSettingsPath);
    }

    // -----------------------------------------------------------------------
    // Sprite creation
    // -----------------------------------------------------------------------

    private static Sprite[] CreateBlockSprites()
    {
        // NES Tetris colors for each piece type
        Color[] pieceColors = new Color[]
        {
            Color.clear,          // 0: empty
            new Color(0f, 1f, 1f, 1f),    // 1: I - cyan
            new Color(1f, 1f, 0f, 1f),    // 2: O - yellow
            new Color(0.5f, 0f, 0.5f, 1f), // 3: T - purple
            new Color(0f, 1f, 0f, 1f),    // 4: S - green
            new Color(1f, 0f, 0f, 1f),    // 5: Z - red
            new Color(0f, 0f, 1f, 1f),    // 6: J - blue
            new Color(1f, 0.5f, 0f, 1f)   // 7: L - orange
        };

        string[] spriteNames = new string[]
        {
            "block_empty", "block_I", "block_O", "block_T",
            "block_S", "block_Z", "block_J", "block_L"
        };

        Sprite[] sprites = new Sprite[8];

        for (int i = 0; i < 8; i++)
        {
            string path = SpritesPath + "/" + spriteNames[i] + ".png";
            if (AssetDatabase.LoadAssetAtPath<Sprite>(path) != null)
            {
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Debug.Log("[AssembleMainScene] Using existing sprite: " + path);
                continue;
            }

            sprites[i] = CreateBlockSprite(path, pieceColors[i], spriteNames[i]);
        }

        return sprites;
    }

    private static Sprite CreateBlockSprite(string path, Color color, string name)
    {
        int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.ARGB32, false);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float shade = 1.0f;
                if (x < 4) shade += 0.1f;
                if (y < 4) shade += 0.1f;
                if (x >= size - 4) shade -= 0.15f;
                if (y >= size - 4) shade -= 0.15f;
                if (x == 0 || y == 0 || x == size - 1 || y == size - 1)
                    shade *= 0.4f;

                shade = Mathf.Clamp01(shade);

                if (color.a == 0)
                {
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
                else
                {
                    tex.SetPixel(x, y, new Color(
                        Mathf.Clamp01(color.r * shade),
                        Mathf.Clamp01(color.g * shade),
                        Mathf.Clamp01(color.b * shade),
                        color.a
                    ));
                }
            }
        }

        tex.Apply();
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        byte[] pngBytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, pngBytes);
        UnityEngine.Object.DestroyImmediate(tex);

        // Force import with sprite settings
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        // Set import settings to make it a sprite
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.anisoLevel = 0;
            importer.SaveAndReimport();
            Debug.Log("[AssembleMainScene] Set sprite import settings for: " + path);
        }

        // Wait for import to complete
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        Debug.Log("[AssembleMainScene] Created sprite: " + path + " -> " + (sprite != null ? sprite.name : "NULL"));

        return sprite;
    }

    private static Sprite CreateBorderSprite()
    {
        string path = SpritesPath + "/well_border.png";
        if (AssetDatabase.LoadAssetAtPath<Sprite>(path) != null)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        int width = 320;
        int height = 640;

        var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isBorder = x < 4 || x >= width - 4 || y < 4 || y >= height - 4;
                Color c;
                if (isBorder)
                {
                    c = new Color(0.3f, 0.3f, 0.35f, 1f);
                }
                else
                {
                    c = new Color(0.05f, 0.05f, 0.08f, 1f);
                }
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        byte[] pngBytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, pngBytes);
        UnityEngine.Object.DestroyImmediate(tex);

        // Force import with sprite settings
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.anisoLevel = 0;
            importer.SaveAndReimport();
            Debug.Log("[AssembleMainScene] Set sprite import settings for: " + path);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        Debug.Log("[AssembleMainScene] Created border sprite: " + path + " -> " + (sprite != null ? sprite.name : "NULL"));

        return sprite;
    }

    // -----------------------------------------------------------------------
    // Camera
    // -----------------------------------------------------------------------

    private static void CreateCamera()
    {
        var camGO = new GameObject("MainCamera");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.4f;
        cam.transform.position = new Vector3(5f, 10f, -10f);
        cam.backgroundColor = Color.black;

        Debug.Log("[AssembleMainScene] Created MainCamera");
    }

    // -----------------------------------------------------------------------
    // GameManager
    // -----------------------------------------------------------------------

    private static GameObject CreateGameManager()
    {
        var go = new GameObject("GameManager");
        go.AddComponent<SceneBootstrapper>();
        go.AddComponent<GameplayController>();

        Debug.Log("[AssembleMainScene] Created GameManager with SceneBootstrapper and GameplayController");
        return go;
    }

    // -----------------------------------------------------------------------
    // PlayfieldRenderer
    // -----------------------------------------------------------------------

    private static GameObject CreatePlayfieldRenderer(Sprite[] blockSprites, Sprite borderSprite)
    {
        var go = new GameObject("PlayfieldRenderer");
        var pr = go.AddComponent<PlayfieldRenderer>();

        pr.SetBlockSprites(blockSprites);

        // Set border sprite via reflection (it's private)
        var borderField = typeof(PlayfieldRenderer).GetField("_borderSprite",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (borderField != null)
        {
            borderField.SetValue(pr, borderSprite);
        }

        Debug.Log("[AssembleMainScene] Created PlayfieldRenderer");
        return go;
    }

    // -----------------------------------------------------------------------
    // UI Screens
    // -----------------------------------------------------------------------

    private static GameObject CreateStartScreenUI(PanelSettings panelSettings)
    {
        var go = new GameObject("StartScreenUI");
        var doc = go.AddComponent<UIDocument>();

        var so = new SerializedObject(doc);
        so.FindProperty("m_PanelSettings").objectReferenceValue = panelSettings;
        so.ApplyModifiedPropertiesWithoutUndo();

        var psSo = new SerializedObject(panelSettings);
        psSo.FindProperty("m_SortingOrder").intValue = 0;
        psSo.ApplyModifiedPropertiesWithoutUndo();

        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/StartScreen.uxml");
        so = new SerializedObject(doc);
        so.FindProperty("sourceAsset").objectReferenceValue = uxml;
        so.ApplyModifiedPropertiesWithoutUndo();

        var srcAsset = so.FindProperty("sourceAsset").objectReferenceValue;
        Debug.Log("[AssembleMainScene] StartScreenUI sourceAsset: " + (srcAsset != null ? srcAsset.name : "NULL"));

        go.AddComponent<StartScreen>();
        go.AddComponent<StartScreenLeaderboardWidget>();

        Debug.Log("[AssembleMainScene] Created StartScreenUI");
        return go;
    }

    private static GameObject CreateGameScreenUI(PanelSettings panelSettings)
    {
        var go = new GameObject("GameScreenUI");
        var doc = go.AddComponent<UIDocument>();

        var so = new SerializedObject(doc);
        so.FindProperty("m_PanelSettings").objectReferenceValue = panelSettings;
        so.ApplyModifiedPropertiesWithoutUndo();

        var psSo = new SerializedObject(panelSettings);
        psSo.FindProperty("m_SortingOrder").intValue = 1;
        psSo.ApplyModifiedPropertiesWithoutUndo();

        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/GameScreen.uxml");
        so = new SerializedObject(doc);
        so.FindProperty("sourceAsset").objectReferenceValue = uxml;
        so.ApplyModifiedPropertiesWithoutUndo();

        var srcAsset = so.FindProperty("sourceAsset").objectReferenceValue;
        Debug.Log("[AssembleMainScene] GameScreenUI sourceAsset: " + (srcAsset != null ? srcAsset.name : "NULL"));

        go.AddComponent<GameScreen>();
        go.AddComponent<GameScreenScoreWidget>();
        go.AddComponent<GameScreenLevelWidget>();
        go.AddComponent<GameScreenLinesWidget>();
        go.AddComponent<GameScreenNextWidget>();

        Debug.Log("[AssembleMainScene] Created GameScreenUI");
        return go;
    }

    private static GameObject CreateGameOverScreenUI(PanelSettings panelSettings)
    {
        var go = new GameObject("GameOverScreenUI");
        var doc = go.AddComponent<UIDocument>();

        var so = new SerializedObject(doc);
        so.FindProperty("m_PanelSettings").objectReferenceValue = panelSettings;
        so.ApplyModifiedPropertiesWithoutUndo();

        var psSo = new SerializedObject(panelSettings);
        psSo.FindProperty("m_SortingOrder").intValue = 2;
        psSo.ApplyModifiedPropertiesWithoutUndo();

        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/GameOverScreen.uxml");
        so = new SerializedObject(doc);
        so.FindProperty("sourceAsset").objectReferenceValue = uxml;
        so.ApplyModifiedPropertiesWithoutUndo();

        var srcAsset = so.FindProperty("sourceAsset").objectReferenceValue;
        Debug.Log("[AssembleMainScene] GameOverScreenUI sourceAsset: " + (srcAsset != null ? srcAsset.name : "NULL"));

        go.AddComponent<GameOverScreen>();

        Debug.Log("[AssembleMainScene] Created GameOverScreenUI");
        return go;
    }

    private static GameObject CreateInitialsEntryUI(PanelSettings panelSettings)
    {
        var go = new GameObject("InitialsEntryUI");
        var doc = go.AddComponent<UIDocument>();

        var so = new SerializedObject(doc);
        so.FindProperty("m_PanelSettings").objectReferenceValue = panelSettings;
        so.ApplyModifiedPropertiesWithoutUndo();

        var psSo = new SerializedObject(panelSettings);
        psSo.FindProperty("m_SortingOrder").intValue = 3;
        psSo.ApplyModifiedPropertiesWithoutUndo();

        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/InitialsEntryOverlay.uxml");
        so = new SerializedObject(doc);
        so.FindProperty("sourceAsset").objectReferenceValue = uxml;
        so.ApplyModifiedPropertiesWithoutUndo();

        var srcAsset = so.FindProperty("sourceAsset").objectReferenceValue;
        Debug.Log("[AssembleMainScene] InitialsEntryUI sourceAsset: " + (srcAsset != null ? srcAsset.name : "NULL"));

        go.AddComponent<InitialsEntryOverlay>();

        Debug.Log("[AssembleMainScene] Created InitialsEntryUI");
        return go;
    }

    // -----------------------------------------------------------------------
    // Wiring
    // -----------------------------------------------------------------------

    private static void WireSceneBootstrapper(GameObject gameManager,
        GameObject startScreenUI, GameObject gameScreenUI,
        GameObject gameOverScreenUI, GameObject initialsEntryUI,
        Sprite[] blockSprites)
    {
        var bootstrapper = gameManager.GetComponent<SceneBootstrapper>();
        var gc = gameManager.GetComponent<GameplayController>();

        SetPrivateField(bootstrapper, "gameplayController", gc);
        SetPrivateField(bootstrapper, "startScreen", startScreenUI.GetComponent<StartScreen>());
        SetPrivateField(bootstrapper, "gameScreen", gameScreenUI.GetComponent<GameScreen>());
        SetPrivateField(bootstrapper, "gameOverScreen", gameOverScreenUI.GetComponent<GameOverScreen>());
        SetPrivateField(bootstrapper, "initialsEntryOverlay", initialsEntryUI.GetComponent<InitialsEntryOverlay>());
        SetPrivateField(bootstrapper, "scoreWidget", gameScreenUI.GetComponent<GameScreenScoreWidget>());
        SetPrivateField(bootstrapper, "levelWidget", gameScreenUI.GetComponent<GameScreenLevelWidget>());
        SetPrivateField(bootstrapper, "linesWidget", gameScreenUI.GetComponent<GameScreenLinesWidget>());
        SetPrivateField(bootstrapper, "nextWidget", gameScreenUI.GetComponent<GameScreenNextWidget>());
        SetPrivateField(bootstrapper, "leaderboardWidget", startScreenUI.GetComponent<StartScreenLeaderboardWidget>());

        // Set block sprites on NextWidget
        var nextWidget = gameScreenUI.GetComponent<GameScreenNextWidget>();
        nextWidget.SetBlockSprites(blockSprites);

        Debug.Log("[AssembleMainScene] Wired SceneBootstrapper references");
    }

    private static void SetPrivateField<T>(object obj, string fieldName, T value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
            Debug.Log("[AssembleMainScene] Set field " + fieldName + " = " + (value != null ? value.GetType().Name : "null"));
        }
        else
        {
            Debug.LogWarning("[AssembleMainScene] Field " + fieldName + " not found on " + obj.GetType().Name);
        }
    }

    // -----------------------------------------------------------------------
    // Build settings
    // -----------------------------------------------------------------------

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var buildScenes = EditorBuildSettings.scenes;
        bool alreadyInList = false;

        foreach (var bs in buildScenes)
        {
            if (bs.path == scenePath)
            {
                alreadyInList = true;
                break;
            }
        }

        if (!alreadyInList)
        {
            var newScenes = new EditorBuildSettingsScene[buildScenes.Length + 1];
            System.Array.Copy(buildScenes, newScenes, buildScenes.Length);
            newScenes[buildScenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
            Debug.Log("[AssembleMainScene] Added " + scenePath + " to build settings");
        }
        else
        {
            Debug.Log("[AssembleMainScene] Scene already in build settings");
        }
    }
}
