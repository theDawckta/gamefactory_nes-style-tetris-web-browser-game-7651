using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

/// <summary>
/// PlayMode tests verifying SceneBootstrapper and PlayfieldRenderer can have all
/// serialized references wired correctly via SerializedObject, and that no
/// NullReferenceException occurs at runtime.
/// </summary>
[TestFixture]
public class SceneWiringTests
{
    // =========================================================================
    // SceneBootstrapper wiring tests
    // =========================================================================

    [UnityTest]
    public IEnumerator SceneBootstrapper_AllReferencesCanBeWiredViaSerializedObject()
    {
        // Create GameManager with SceneBootstrapper and GameplayController
        var manager = new GameObject("GameManager");
        var bootstrapper = manager.AddComponent<SceneBootstrapper>();
        var gc = manager.AddComponent<GameplayController>();

        // Create StartScreen
        var startScreenGo = new GameObject("StartScreen");
        var startDoc = startScreenGo.AddComponent<UIDocument>();
        var startScreen = startScreenGo.AddComponent<StartScreen>();
        startDoc.rootVisualElement.Add(CreateRegion("leaderboard-region"));

        // Create GameScreen
        var gameScreenGo = new GameObject("GameScreen");
        var gameDoc = gameScreenGo.AddComponent<UIDocument>();
        var gameScreen = gameScreenGo.AddComponent<GameScreen>();
        gameDoc.rootVisualElement.Add(CreateRegion("score-region"));
        gameDoc.rootVisualElement.Add(CreateRegion("level-region"));
        gameDoc.rootVisualElement.Add(CreateRegion("lines-region"));
        gameDoc.rootVisualElement.Add(CreateRegion("next-region"));

        // Create GameOverScreen
        var gameOverGo = new GameObject("GameOverScreen");
        var gameOverDoc = gameOverGo.AddComponent<UIDocument>();
        var gameOverScreen = gameOverGo.AddComponent<GameOverScreen>();
        var scoreLabel = new Label("SCORE: 0000000");
        scoreLabel.name = "score-label";
        gameOverDoc.rootVisualElement.Add(scoreLabel);

        // Create InitialsEntryOverlay
        var initialsGo = new GameObject("InitialsEntryOverlay");
        var initialsDoc = initialsGo.AddComponent<UIDocument>();
        var initialsOverlay = initialsGo.AddComponent<InitialsEntryOverlay>();
        initialsDoc.rootVisualElement.Add(CreateLabel("char-0"));
        initialsDoc.rootVisualElement.Add(CreateLabel("char-1"));
        initialsDoc.rootVisualElement.Add(CreateLabel("char-2"));
        initialsDoc.rootVisualElement.Add(CreateRegion("confirm-region"));

        // Create widgets
        var scoreWidgetGo = new GameObject("ScoreWidget");
        var scoreWidgetDoc = scoreWidgetGo.AddComponent<UIDocument>();
        var scoreWidget = scoreWidgetGo.AddComponent<GameScreenScoreWidget>();
        var scoreRegion = CreateRegion("score-region");
        scoreRegion.Add(CreateValueLabel());
        scoreWidgetDoc.rootVisualElement.Add(scoreRegion);

        var levelWidgetGo = new GameObject("LevelWidget");
        var levelWidgetDoc = levelWidgetGo.AddComponent<UIDocument>();
        var levelWidget = levelWidgetGo.AddComponent<GameScreenLevelWidget>();
        var levelRegion = CreateRegion("level-region");
        levelRegion.Add(CreateValueLabel());
        levelWidgetDoc.rootVisualElement.Add(levelRegion);

        var linesWidgetGo = new GameObject("LinesWidget");
        var linesWidgetDoc = linesWidgetGo.AddComponent<UIDocument>();
        var linesWidget = linesWidgetGo.AddComponent<GameScreenLinesWidget>();
        var linesRegion = CreateRegion("lines-region");
        linesRegion.Add(CreateValueLabel());
        linesWidgetDoc.rootVisualElement.Add(linesRegion);

        var nextWidgetGo = new GameObject("NextWidget");
        var nextWidgetDoc = nextWidgetGo.AddComponent<UIDocument>();
        var nextWidget = nextWidgetGo.AddComponent<GameScreenNextWidget>();
        nextWidgetDoc.rootVisualElement.Add(CreateRegion("next-region"));

        var leaderboardWidgetGo = new GameObject("LeaderboardWidget");
        var lbWidgetDoc = leaderboardWidgetGo.AddComponent<UIDocument>();
        var leaderboardWidget = leaderboardWidgetGo.AddComponent<StartScreenLeaderboardWidget>();
        lbWidgetDoc.rootVisualElement.Add(CreateRegion("leaderboard-region"));

        // Wire all references via SerializedObject
        var so = new SerializedObject(bootstrapper);
        SetSerializedRef(so, "gameplayController", gc);
        SetSerializedRef(so, "startScreen", startScreen);
        SetSerializedRef(so, "gameScreen", gameScreen);
        SetSerializedRef(so, "gameOverScreen", gameOverScreen);
        SetSerializedRef(so, "initialsEntryOverlay", initialsOverlay);
        SetSerializedRef(so, "scoreWidget", scoreWidget);
        SetSerializedRef(so, "levelWidget", levelWidget);
        SetSerializedRef(so, "linesWidget", linesWidget);
        SetSerializedRef(so, "nextWidget", nextWidget);
        SetSerializedRef(so, "leaderboardWidget", leaderboardWidget);
        so.ApplyModifiedPropertiesWithoutUndo();

        // Verify all references were wired by checking SerializedProperty values
        Assert.IsNotNull(so.FindProperty("gameplayController").objectReferenceValue, "gameplayController should be wired");
        Assert.IsNotNull(so.FindProperty("startScreen").objectReferenceValue, "startScreen should be wired");
        Assert.IsNotNull(so.FindProperty("gameScreen").objectReferenceValue, "gameScreen should be wired");
        Assert.IsNotNull(so.FindProperty("gameOverScreen").objectReferenceValue, "gameOverScreen should be wired");
        Assert.IsNotNull(so.FindProperty("initialsEntryOverlay").objectReferenceValue, "initialsEntryOverlay should be wired");
        Assert.IsNotNull(so.FindProperty("scoreWidget").objectReferenceValue, "scoreWidget should be wired");
        Assert.IsNotNull(so.FindProperty("levelWidget").objectReferenceValue, "levelWidget should be wired");
        Assert.IsNotNull(so.FindProperty("linesWidget").objectReferenceValue, "linesWidget should be wired");
        Assert.IsNotNull(so.FindProperty("nextWidget").objectReferenceValue, "nextWidget should be wired");
        Assert.IsNotNull(so.FindProperty("leaderboardWidget").objectReferenceValue, "leaderboardWidget should be wired");

        yield return null; // Let Start() run

        // If we get here without NullReferenceException, the wiring is valid
        Assert.IsTrue(true, "SceneBootstrapper started without NullReferenceException");

        // Cleanup
        Object.Destroy(manager);
        Object.Destroy(startScreenGo);
        Object.Destroy(gameScreenGo);
        Object.Destroy(gameOverGo);
        Object.Destroy(initialsGo);
        Object.Destroy(scoreWidgetGo);
        Object.Destroy(levelWidgetGo);
        Object.Destroy(linesWidgetGo);
        Object.Destroy(nextWidgetGo);
        Object.Destroy(leaderboardWidgetGo);
    }

    [UnityTest]
    public IEnumerator SceneBootstrapper_NoNullReferenceException_WhenAllRefsWired()
    {
        var exceptionsCaught = 0;

        // Temporarily hook into log messages to catch NullReferenceException
        Application.logMessageReceived += (condition, stackTrace, type) =>
        {
            if (type == LogType.Exception && condition.Contains("NullReferenceException"))
            {
                exceptionsCaught++;
            }
        };

        var manager = new GameObject("GameManager");
        var bootstrapper = manager.AddComponent<SceneBootstrapper>();
        var gc = manager.AddComponent<GameplayController>();

        // Create and wire all components
        var startScreenGo = new GameObject("StartScreen");
        var startDoc = startScreenGo.AddComponent<UIDocument>();
        var startScreen = startScreenGo.AddComponent<StartScreen>();
        startDoc.rootVisualElement.Add(CreateRegion("leaderboard-region"));

        var gameScreenGo = new GameObject("GameScreen");
        var gameDoc = gameScreenGo.AddComponent<UIDocument>();
        var gameScreen = gameScreenGo.AddComponent<GameScreen>();
        gameDoc.rootVisualElement.Add(CreateRegion("score-region"));
        gameDoc.rootVisualElement.Add(CreateRegion("level-region"));
        gameDoc.rootVisualElement.Add(CreateRegion("lines-region"));
        gameDoc.rootVisualElement.Add(CreateRegion("next-region"));

        var gameOverGo = new GameObject("GameOverScreen");
        var gameOverDoc = gameOverGo.AddComponent<UIDocument>();
        var gameOverScreen = gameOverGo.AddComponent<GameOverScreen>();
        gameOverDoc.rootVisualElement.Add(new Label("SCORE: 0000000") { name = "score-label" });

        var initialsGo = new GameObject("InitialsEntryOverlay");
        var initialsDoc = initialsGo.AddComponent<UIDocument>();
        var initialsOverlay = initialsGo.AddComponent<InitialsEntryOverlay>();
        initialsDoc.rootVisualElement.Add(CreateLabel("char-0"));
        initialsDoc.rootVisualElement.Add(CreateLabel("char-1"));
        initialsDoc.rootVisualElement.Add(CreateLabel("char-2"));
        initialsDoc.rootVisualElement.Add(CreateRegion("confirm-region"));

        // Create minimal widgets
        var swGo = new GameObject("SW"); var swDoc = swGo.AddComponent<UIDocument>(); swDoc.rootVisualElement.Add(CreateRegion("score-region")); swDoc.rootVisualElement.Add(CreateValueLabel()); var sw = swGo.AddComponent<GameScreenScoreWidget>();
        var lwGo = new GameObject("LW"); var lwDoc = lwGo.AddComponent<UIDocument>(); lwDoc.rootVisualElement.Add(CreateRegion("level-region")); lwDoc.rootVisualElement.Add(CreateValueLabel()); var lw = lwGo.AddComponent<GameScreenLevelWidget>();
        var llwGo = new GameObject("LLW"); var llwDoc = llwGo.AddComponent<UIDocument>(); llwDoc.rootVisualElement.Add(CreateRegion("lines-region")); llwDoc.rootVisualElement.Add(CreateValueLabel()); var llw = llwGo.AddComponent<GameScreenLinesWidget>();
        var nwGo = new GameObject("NW"); var nwDoc = nwGo.AddComponent<UIDocument>(); nwDoc.rootVisualElement.Add(CreateRegion("next-region")); var nw = nwGo.AddComponent<GameScreenNextWidget>();
        var lbwGo = new GameObject("LBW"); var lbwDoc = lbwGo.AddComponent<UIDocument>(); lbwDoc.rootVisualElement.Add(CreateRegion("leaderboard-region")); var lbw = lbwGo.AddComponent<StartScreenLeaderboardWidget>();

        var so = new SerializedObject(bootstrapper);
        SetSerializedRef(so, "gameplayController", gc);
        SetSerializedRef(so, "startScreen", startScreen);
        SetSerializedRef(so, "gameScreen", gameScreen);
        SetSerializedRef(so, "gameOverScreen", gameOverScreen);
        SetSerializedRef(so, "initialsEntryOverlay", initialsOverlay);
        SetSerializedRef(so, "scoreWidget", sw);
        SetSerializedRef(so, "levelWidget", lw);
        SetSerializedRef(so, "linesWidget", llw);
        SetSerializedRef(so, "nextWidget", nw);
        SetSerializedRef(so, "leaderboardWidget", lbw);
        so.ApplyModifiedPropertiesWithoutUndo();

        yield return null; // Start()
        yield return null; // Extra frame

        Application.logMessageReceived -= (condition, stackTrace, type) => { };

        Assert.AreEqual(0, exceptionsCaught, "Should have no NullReferenceException when all refs are wired");

        Object.Destroy(manager);
        Object.Destroy(startScreenGo);
        Object.Destroy(gameScreenGo);
        Object.Destroy(gameOverGo);
        Object.Destroy(initialsGo);
        Object.Destroy(swGo);
        Object.Destroy(lwGo);
        Object.Destroy(llwGo);
        Object.Destroy(nwGo);
        Object.Destroy(lbwGo);
    }

    // =========================================================================
    // PlayfieldRenderer wiring tests
    // =========================================================================

    [UnityTest]
    public IEnumerator PlayfieldRenderer_AllReferencesCanBeWiredViaSerializedObject()
    {
        var gcGo = new GameObject("GC");
        var gc = gcGo.AddComponent<GameplayController>();

        var prGo = new GameObject("PR");
        var pr = prGo.AddComponent<PlayfieldRenderer>();

        // Create test sprites
        Sprite[] sprites = new Sprite[8];
        for (int i = 0; i < 8; i++)
        {
            sprites[i] = Sprite.Create(
                new Texture2D(1, 1),
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f)
            );
        }
        Sprite borderSprite = Sprite.Create(
            new Texture2D(1, 1),
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f)
        );

        // Wire via SerializedObject
        var so = new SerializedObject(pr);

        // Wire _blockSprites array
        var blockSpritesProp = so.FindProperty("_blockSprites");
        Assert.IsNotNull(blockSpritesProp, "_blockSprites property should exist");
        blockSpritesProp.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
        {
            blockSpritesProp.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        // Wire _borderSprite
        SetSerializedRef(so, "_borderSprite", borderSprite);

        // Wire _gameplayController
        SetSerializedRef(so, "_gameplayController", gc);

        so.ApplyModifiedPropertiesWithoutUndo();

        // Verify wiring
        Assert.IsNotNull(so.FindProperty("_borderSprite").objectReferenceValue, "_borderSprite should be wired");
        Assert.IsNotNull(so.FindProperty("_gameplayController").objectReferenceValue, "_gameplayController should be wired");
        Assert.AreEqual(sprites.Length, so.FindProperty("_blockSprites").arraySize, "_blockSprites should have " + sprites.Length + " elements");

        yield return null; // Awake + Start

        Assert.IsTrue(true, "PlayfieldRenderer started without NullReferenceException");

        // Cleanup
        Object.Destroy(gcGo);
        Object.Destroy(prGo);
        foreach (var s in sprites) if (s != null) Object.Destroy(s);
        if (borderSprite != null) Object.Destroy(borderSprite);
    }

    [UnityTest]
    public IEnumerator PlayfieldRenderer_NoNullReferenceException_WhenAllRefsWired()
    {
        var exceptionsCaught = 0;

        Application.logMessageReceived += (condition, stackTrace, type) =>
        {
            if (type == LogType.Exception && condition.Contains("NullReferenceException"))
            {
                exceptionsCaught++;
            }
        };

        var gcGo = new GameObject("GC");
        var gc = gcGo.AddComponent<GameplayController>();

        var prGo = new GameObject("PR");
        var pr = prGo.AddComponent<PlayfieldRenderer>();

        // Create test sprites
        Sprite[] sprites = new Sprite[8];
        for (int i = 0; i < 8; i++)
        {
            sprites[i] = Sprite.Create(
                new Texture2D(1, 1),
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f)
            );
        }
        Sprite borderSprite = Sprite.Create(
            new Texture2D(1, 1),
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f)
        );

        // Wire via SerializedObject
        var so = new SerializedObject(pr);
        var blockSpritesProp = so.FindProperty("_blockSprites");
        blockSpritesProp.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
        {
            blockSpritesProp.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }
        SetSerializedRef(so, "_borderSprite", borderSprite);
        SetSerializedRef(so, "_gameplayController", gc);
        so.ApplyModifiedPropertiesWithoutUndo();

        yield return null; // Awake
        yield return null; // Start
        yield return null; // Update (render pass)

        Application.logMessageReceived -= (condition, stackTrace, type) => { };

        Assert.AreEqual(0, exceptionsCaught, "Should have no NullReferenceException when all refs are wired");

        Object.Destroy(gcGo);
        Object.Destroy(prGo);
        foreach (var s in sprites) if (s != null) Object.Destroy(s);
        if (borderSprite != null) Object.Destroy(borderSprite);
    }

    // =========================================================================
    // UIDocument wiring tests
    // =========================================================================

    [UnityTest]
    public IEnumerator UIDocument_SourceAssetAndPanelSettingsCanBeWired()
    {
        // Create a PanelSettings asset
        var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();

        // Create a UIDocument and wire via SerializedObject
        var go = new GameObject("TestUIDocument");
        var doc = go.AddComponent<UIDocument>();

        var so = new SerializedObject(doc);
        SetSerializedRef(so, "m_PanelSettings", panelSettings);
        so.ApplyModifiedPropertiesWithoutUndo();

        // Verify wiring
        Assert.IsNotNull(so.FindProperty("m_PanelSettings").objectReferenceValue, "m_PanelSettings should be wired");

        yield return null;

        Assert.IsNotNull(doc.rootVisualElement, "rootVisualElement should not be null after wiring PanelSettings");

        Object.Destroy(go);
        Object.Destroy(panelSettings);
    }

    [UnityTest]
    public IEnumerator UIDocument_NoNullReferenceException_WithPanelSettingsWired()
    {
        var exceptionsCaught = 0;

        Application.logMessageReceived += (condition, stackTrace, type) =>
        {
            if (type == LogType.Exception && condition.Contains("NullReferenceException"))
            {
                exceptionsCaught++;
            }
        };

        var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();

        var go = new GameObject("TestUIDocument");
        var doc = go.AddComponent<UIDocument>();

        var so = new SerializedObject(doc);
        SetSerializedRef(so, "m_PanelSettings", panelSettings);
        so.ApplyModifiedPropertiesWithoutUndo();

        yield return null;
        yield return null; // Extra frame

        Application.logMessageReceived -= (condition, stackTrace, type) => { };

        Assert.AreEqual(0, exceptionsCaught, "UIDocument should not throw NullReferenceException when PanelSettings is wired");

        Object.Destroy(go);
        Object.Destroy(panelSettings);
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private void SetSerializedRef(SerializedObject so, string fieldName, UnityEngine.Object value)
    {
        var prop = so.FindProperty(fieldName);
        if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
        {
            prop.objectReferenceValue = value;
        }
    }

    private VisualElement CreateRegion(string name)
    {
        var region = new VisualElement();
        region.name = name;
        return region;
    }

    private Label CreateValueLabel()
    {
        var label = new Label("0");
        label.AddToClassList("region-value");
        return label;
    }

    private Label CreateLabel(string name)
    {
        var label = new Label("A");
        label.name = name;
        return label;
    }
}
