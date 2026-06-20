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
    private const string SpritesPath = "Assets/Game/Art/Sprites";
	
    // Per-screen PanelSettings paths
    private const string StartScreenPanelSettingsPath = "Assets/Game/UI/StartScreenPanelSettings.asset";
    private const string GameScreenPanelSettingsPath = "Assets/Game/UI/GameScreenPanelSettings.asset";
    private const string GameOverScreenPanelSettingsPath = "Assets/Game/UI/GameOverScreenPanelSettings.asset";
    private const string InitialsEntryPanelSettingsPath = "Assets/Game/UI/InitialsEntryOverlayPanelSettings.asset";
	
    [MenuItem("Tools/Assemble Main Scene %&m")]
    public static void RunAndExit()
    {
        Debug.Log("[AssembleMainScene] Starting scene assembly...");
	
        // 1. Create directories
        EnsureDirectory("Assets/Game/Scenes");
        EnsureDirectory("Assets/Game/UI");
        EnsureDirectory(SpritesPath);
	
        // 2. Create shared PanelSettings (fallback)
        CreateSharedPanelSettings();
	
        // 3. Create per-screen PanelSettings if they don't exist
        EnsurePanelSettings(StartScreenPanelSettingsPath, "StartScreenPanelSettings", 0);
        EnsurePanelSettings(GameScreenPanelSettingsPath, "GameScreenPanelSettings", 1);
        EnsurePanelSettings(GameOverScreenPanelSettingsPath, "GameOverScreenPanelSettings", 2);
        EnsurePanelSettings(InitialsEntryPanelSettingsPath, "InitialsEntryOverlayPanelSettings", 3);
	
        // 4. Create sprite assets
        Sprite[] blockSprites = CreateBlockSprites();
        Sprite borderSprite = CreateBorderSprite();
	
        // 5. Create or open the Main scene
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
	
        // 6. Remove any existing camera
        var cam = UnityEngine.Object.FindAnyObjectByType<Camera>();
        if (cam != null)
        {
            UnityEngine.Object.DestroyImmediate(cam.gameObject);
        }
	
        // 7. Create MainCamera
        Camera mainCamera = CreateCamera();
	
        // 8. Create GameManager with SceneBootstrapper, GameplayController
        GameObject gameManager = CreateGameManager();
	
        // 9. Create PlayfieldRenderer
        GameObject playfieldRenderer = CreatePlayfieldRenderer(blockSprites, borderSprite);
	
        // 10. Load per-screen PanelSettings
        PanelSettings startPanelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(StartScreenPanelSettingsPath);
        PanelSettings gamePanelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(GameScreenPanelSettingsPath);
        PanelSettings gameOverPanelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(GameOverScreenPanelSettingsPath);
        PanelSettings initialsPanelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(InitialsEntryPanelSettingsPath);
	
        // 11. Create UI screen GameObjects
        GameObject startScreenUI = CreateStartScreenUI(startPanelSettings, mainCamera);
        GameObject gameScreenUI = CreateGameScreenUI(gamePanelSettings, mainCamera);
        GameObject gameOverScreenUI = CreateGameOverScreenUI(gameOverPanelSettings, mainCamera);
        GameObject initialsEntryUI = CreateInitialsEntryUI(initialsPanelSettings, mainCamera);
	
        // 12. Wire up SceneBootstrapper references
        WireSceneBootstrapper(gameManager, startScreenUI, gameScreenUI, gameOverScreenUI, initialsEntryUI, blockSprites);
	
        // 13. Wire PlayfieldRenderer to GameplayController via SerializedObject
        var pr = playfieldRenderer.GetComponent<PlayfieldRenderer>();
        var gc = gameManager.GetComponent<GameplayController>();
        var prSo = new SerializedObject(pr);
        var gcProp = prSo.FindProperty("_gameplayController");
        if (gcProp != null)
        {
            gcProp.objectReferenceValue = gc;
            prSo.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[AssembleMainScene] PlayfieldRenderer _gameplayController wired via SerializedObject");
        }
        else
        {
            Debug.LogWarning("[AssembleMainScene] Could not find _gameplayController property on PlayfieldRenderer");
        }
	
        // 14. Add scene to build settings
        AddSceneToBuildSettings(ScenePath);
	
        // 15. Save everything
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
	
        Debug.Log("[AssembleMainScene] Scene assembly complete!");

        EditorApplication.Exit(0);
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
    // Shared PanelSettings (fallback)
    // -----------------------------------------------------------------------
	
    private static void CreateSharedPanelSettings()
    {
        string panelSettingsPath = "Assets/Game/UI/PanelSettings.asset";
        var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelSettingsPath);
        if (existing != null)
        {
            Debug.Log("[AssembleMainScene] Shared PanelSettings already exists at " + panelSettingsPath);
            return;
        }
	
        var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        panelSettings.name = "PanelSettings";
	
        var so = new SerializedObject(panelSettings);
        so.FindProperty("m_SortingOrder").intValue = 0;
        so.ApplyModifiedPropertiesWithoutUndo();
	
        AssetDatabase.CreateAsset(panelSettings, panelSettingsPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[AssembleMainScene] Created shared PanelSettings at " + panelSettingsPath);
    }
	
    // -----------------------------------------------------------------------
    // Per-screen PanelSettings
    // -----------------------------------------------------------------------
	
    private static void EnsurePanelSettings(string path, string name, int sortOrder)
    {
        var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
        if (existing != null)
        {
            // Update sort order
            var so = new SerializedObject(existing);
            so.FindProperty("m_SortingOrder").intValue = sortOrder;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[AssembleMainScene] Updated PanelSettings " + name + " sort order to " + sortOrder);
            return;
        }
	
        var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        panelSettings.name = name;
	
        var psSo = new SerializedObject(panelSettings);
        psSo.FindProperty("m_SortingOrder").intValue = sortOrder;
        psSo.ApplyModifiedPropertiesWithoutUndo();
	
        AssetDatabase.CreateAsset(panelSettings, path);
        AssetDatabase.SaveAssets();
        Debug.Log("[AssembleMainScene] Created PanelSettings " + name + " at " + path + " with sort order " + sortOrder);
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
	
    private static Camera CreateCamera()
    {
        var camGO = new GameObject("MainCamera");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.4f;
        cam.transform.position = new Vector3(5f, 10f, -10f);
        cam.backgroundColor = Color.black;
	
        Debug.Log("[AssembleMainScene] Created MainCamera");
        return cam;
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
	
        // Set block sprites via SerializedObject for proper scene serialization
        var prSo = new SerializedObject(pr);
        var blockSpritesProp = prSo.FindProperty("_blockSprites");
        if (blockSpritesProp != null && blockSpritesProp.isArray)
        {
            blockSpritesProp.arraySize = blockSprites.Length;
            for (int i = 0; i < blockSprites.Length; i++)
            {
                blockSpritesProp.GetArrayElementAtIndex(i).objectReferenceValue = blockSprites[i];
            }
        }
	
        // Set border sprite via SerializedObject for proper scene serialization
        var borderProp = prSo.FindProperty("_borderSprite");
        if (borderProp != null)
        {
            borderProp.objectReferenceValue = borderSprite;
        }
	
        prSo.ApplyModifiedPropertiesWithoutUndo();
	
        Debug.Log("[AssembleMainScene] Created PlayfieldRenderer with " + (borderSprite != null ? "border" : "NO border") + " sprite");
        return go;
    }
	
    // -----------------------------------------------------------------------
    // UI Screens
    // -----------------------------------------------------------------------
	
    private static GameObject CreateStartScreenUI(PanelSettings panelSettings, Camera mainCamera)
    {
        var go = new GameObject("StartScreenUI");
        var doc = go.AddComponent<UIDocument>();
	
        // Set PanelSettings
        var so = new SerializedObject(doc);
        so.FindProperty("m_PanelSettings").objectReferenceValue = panelSettings;
        so.ApplyModifiedPropertiesWithoutUndo();
	
        // Set visual tree asset
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/StartScreen.uxml");
        so = new SerializedObject(doc);
        so.FindProperty("sourceAsset").objectReferenceValue = uxml;
        so.ApplyModifiedPropertiesWithoutUndo();
	
        // NOTE: m_ParentUI and m_WorldSpaceCollider are only for WorldSpace mode.
        // For ScreenSpaceOverlay mode (our setup), they must remain null.
        // Do NOT assign Camera or BoxCollider -- wrong types cause silent failures.

        // Verify sourceAsset
        var srcAsset = so.FindProperty("sourceAsset").objectReferenceValue;
        Debug.Log("[AssembleMainScene] StartScreenUI sourceAsset: " + (srcAsset != null ? srcAsset.name : "NULL"));
	
        go.AddComponent<StartScreen>();
        go.AddComponent<StartScreenLeaderboardWidget>();
	
        Debug.Log("[AssembleMainScene] Created StartScreenUI");
        return go;
    }
	
    private static GameObject CreateGameScreenUI(PanelSettings panelSettings, Camera mainCamera)
    {
        var go = new GameObject("GameScreenUI");
        var doc = go.AddComponent<UIDocument>();
	
        // Set PanelSettings
        var docSo = new SerializedObject(doc);
        docSo.FindProperty("m_PanelSettings").objectReferenceValue = panelSettings;
        docSo.ApplyModifiedPropertiesWithoutUndo();
	
        // Set visual tree asset
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/GameScreen.uxml");
        docSo = new SerializedObject(doc);
        docSo.FindProperty("sourceAsset").objectReferenceValue = uxml;
        docSo.ApplyModifiedPropertiesWithoutUndo();
	
        // NOTE: m_ParentUI and m_WorldSpaceCollider are only for WorldSpace mode.
        // For ScreenSpaceOverlay mode, they must remain null.

        // Verify sourceAsset
        var srcAsset = docSo.FindProperty("sourceAsset").objectReferenceValue;
        Debug.Log("[AssembleMainScene] GameScreenUI sourceAsset: " + (srcAsset != null ? srcAsset.name : "NULL"));
	
        go.AddComponent<GameScreen>();
        go.AddComponent<GameScreenScoreWidget>();
        go.AddComponent<GameScreenLevelWidget>();
        go.AddComponent<GameScreenLinesWidget>();
        go.AddComponent<GameScreenNextWidget>();
	
        Debug.Log("[AssembleMainScene] Created GameScreenUI");
        return go;
    }
	
    private static GameObject CreateGameOverScreenUI(PanelSettings panelSettings, Camera mainCamera)
    {
        var go = new GameObject("GameOverScreenUI");
        var doc = go.AddComponent<UIDocument>();
	
        // Set PanelSettings
        var docSo = new SerializedObject(doc);
        docSo.FindProperty("m_PanelSettings").objectReferenceValue = panelSettings;
        docSo.ApplyModifiedPropertiesWithoutUndo();
	
        // Set visual tree asset
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/GameOverScreen.uxml");
        docSo = new SerializedObject(doc);
        docSo.FindProperty("sourceAsset").objectReferenceValue = uxml;
        docSo.ApplyModifiedPropertiesWithoutUndo();
	
        // NOTE: m_ParentUI and m_WorldSpaceCollider are only for WorldSpace mode.
        // For ScreenSpaceOverlay mode, they must remain null.

        // Verify sourceAsset
        var srcAsset = docSo.FindProperty("sourceAsset").objectReferenceValue;
        Debug.Log("[AssembleMainScene] GameOverScreenUI sourceAsset: " + (srcAsset != null ? srcAsset.name : "NULL"));
	
        go.AddComponent<GameOverScreen>();
	
        Debug.Log("[AssembleMainScene] Created GameOverScreenUI");
        return go;
    }
	
    private static GameObject CreateInitialsEntryUI(PanelSettings panelSettings, Camera mainCamera)
    {
        var go = new GameObject("InitialsEntryUI");
        var doc = go.AddComponent<UIDocument>();
	
        // Set PanelSettings
        var docSo = new SerializedObject(doc);
        docSo.FindProperty("m_PanelSettings").objectReferenceValue = panelSettings;
        docSo.ApplyModifiedPropertiesWithoutUndo();
	
        // Set visual tree asset
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/InitialsEntryOverlay.uxml");
        docSo = new SerializedObject(doc);
        docSo.FindProperty("sourceAsset").objectReferenceValue = uxml;
        docSo.ApplyModifiedPropertiesWithoutUndo();
	
        // NOTE: m_ParentUI and m_WorldSpaceCollider are only for WorldSpace mode.
        // For ScreenSpaceOverlay mode, they must remain null.

        // Verify sourceAsset
        var srcAsset = docSo.FindProperty("sourceAsset").objectReferenceValue;
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
	
        // Use SerializedObject so values persist in the scene file
        var so = new SerializedObject(bootstrapper);
        SetSerializedField(so, "gameplayController", gc);
        SetSerializedField(so, "startScreen", startScreenUI.GetComponent<StartScreen>());
        SetSerializedField(so, "gameScreen", gameScreenUI.GetComponent<GameScreen>());
        SetSerializedField(so, "gameOverScreen", gameOverScreenUI.GetComponent<GameOverScreen>());
        SetSerializedField(so, "initialsEntryOverlay", initialsEntryUI.GetComponent<InitialsEntryOverlay>());
        SetSerializedField(so, "scoreWidget", gameScreenUI.GetComponent<GameScreenScoreWidget>());
        SetSerializedField(so, "levelWidget", gameScreenUI.GetComponent<GameScreenLevelWidget>());
        SetSerializedField(so, "linesWidget", gameScreenUI.GetComponent<GameScreenLinesWidget>());
        SetSerializedField(so, "nextWidget", gameScreenUI.GetComponent<GameScreenNextWidget>());
        SetSerializedField(so, "leaderboardWidget", startScreenUI.GetComponent<StartScreenLeaderboardWidget>());
        so.ApplyModifiedPropertiesWithoutUndo();
	
        // Set block sprites on NextWidget via SerializedObject for scene persistence
        var nextWidget = gameScreenUI.GetComponent<GameScreenNextWidget>();
        var nwSo = new SerializedObject(nextWidget);
        var spritesProp = nwSo.FindProperty("_blockSprites");
        if (spritesProp != null && spritesProp.isArray)
        {
            spritesProp.arraySize = blockSprites.Length;
            for (int i = 0; i < blockSprites.Length; i++)
            {
                spritesProp.GetArrayElementAtIndex(i).objectReferenceValue = blockSprites[i];
            }
            nwSo.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[AssembleMainScene] GameScreenNextWidget _blockSprites wired via SerializedObject");
        }
        else
        {
            Debug.LogWarning("[AssembleMainScene] Could not find _blockSprites property on GameScreenNextWidget");
        }
	
        Debug.Log("[AssembleMainScene] Wired SceneBootstrapper references");
    }
	
    private static void SetSerializedField(SerializedObject so, string fieldName, UnityEngine.Object value)
    {
        var prop = so.FindProperty(fieldName);
        if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
        {
            prop.objectReferenceValue = value;
            Debug.Log("[AssembleMainScene] Set serialized field " + fieldName + " = " + (value != null ? value.GetType().Name : "null"));
        }
        else
        {
            Debug.LogWarning("[AssembleMainScene] Serialized property " + fieldName + " not found");
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
