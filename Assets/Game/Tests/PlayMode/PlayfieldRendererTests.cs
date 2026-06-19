using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class PlayfieldRendererTests
{
    private GameObject _rendererObject;
    private PlayfieldRenderer _renderer;
    private GameObject _controllerObject;
    private GameplayController _controller;

    [SetUp]
    public void Setup()
    {
        // Create the GameplayController
        _controllerObject = new GameObject("GameplayControllerTest");
        _controller = _controllerObject.AddComponent<GameplayController>();

        // Create the PlayfieldRenderer
        _rendererObject = new GameObject("PlayfieldRendererTest");
        _renderer = _rendererObject.AddComponent<PlayfieldRenderer>();

        // Link them together via public API
        _renderer.SetGameplayController(_controller);
    }

    [TearDown]
    public void Teardown()
    {
        if (_rendererObject != null)
        {
            UnityEngine.Object.Destroy(_rendererObject);
        }
        if (_controllerObject != null)
        {
            UnityEngine.Object.Destroy(_controllerObject);
        }
    }

    [UnityTest]
    public IEnumerator PlayfieldRenderer_Awake_CreatesCorrectNumberOfCells()
    {
        yield return null; // Wait for Awake to run

        // Count SpriteRenderer children
        int rendererCount = 0;
        foreach (Transform child in _rendererObject.transform)
        {
            if (child.gameObject.name.StartsWith("Cell_"))
            {
                rendererCount++;
            }
        }

        // Should be 10 columns x 20 visible rows = 200 cells
        Assert.AreEqual(GameRules.PLAYFIELD_WIDTH * GameRules.PLAYFIELD_VISIBLE_HEIGHT, rendererCount,
            "Should create 200 cell SpriteRenderers (10x20 visible grid)");
    }

    [UnityTest]
    public IEnumerator PlayfieldRenderer_Awake_CreatesWellBorder()
    {
        yield return null; // Wait for Awake to run

        GameObject border = null;
        foreach (Transform child in _rendererObject.transform)
        {
            if (child.gameObject.name == "WellBorder")
            {
                border = child.gameObject;
                break;
            }
        }

        Assert.IsNotNull(border, "WellBorder GameObject should be created");
        Assert.IsNotNull(border.GetComponent<SpriteRenderer>(), "WellBorder should have a SpriteRenderer");
    }

    [UnityTest]
    public IEnumerator PlayfieldRenderer_Awake_CellsHaveCorrectNames()
    {
        yield return null; // Wait for Awake to run

        // Check a few specific cell names
        Transform cell2_0 = _rendererObject.transform.Find("Cell_2_0");
        Transform cell21_9 = _rendererObject.transform.Find("Cell_21_9");
        Transform cell10_5 = _rendererObject.transform.Find("Cell_10_5");

        Assert.IsNotNull(cell2_0, "Cell_2_0 (bottom-left visible) should exist");
        Assert.IsNotNull(cell21_9, "Cell_21_9 (top-right visible) should exist");
        Assert.IsNotNull(cell10_5, "Cell_10_5 (middle) should exist");

        // Buffer rows should NOT exist
        Transform cell0_0 = _rendererObject.transform.Find("Cell_0_0");
        Transform cell1_0 = _rendererObject.transform.Find("Cell_1_0");

        Assert.IsNull(cell0_0, "Cell_0_0 (buffer row) should NOT exist");
        Assert.IsNull(cell1_0, "Cell_1_0 (buffer row) should NOT exist");
    }

    [UnityTest]
    public IEnumerator PlayfieldRenderer_Awake_EachCellHasSpriteRenderer()
    {
        yield return null; // Wait for Awake to run

        int cellsWithRenderer = 0;
        foreach (Transform child in _rendererObject.transform)
        {
            if (child.gameObject.name.StartsWith("Cell_"))
            {
                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                Assert.IsNotNull(sr, "Cell " + child.gameObject.name + " should have a SpriteRenderer");
                cellsWithRenderer++;
            }
        }

        Assert.AreEqual(GameRules.PLAYFIELD_WIDTH * GameRules.PLAYFIELD_VISIBLE_HEIGHT, cellsWithRenderer,
            "All 200 cells should have SpriteRenderers");
    }

    [UnityTest]
    public IEnumerator PlayfieldRenderer_CellSize_DefaultIs0_5()
    {
        yield return null; // Wait for Awake to run

        Assert.AreEqual(0.5f, _renderer.CellSize, "Default CellSize should be 0.5");
    }

    [UnityTest]
    public IEnumerator PlayfieldRenderer_OriginOffset_DefaultIsZero()
    {
        yield return null; // Wait for Awake to run

        Assert.AreEqual(Vector2.zero, _renderer.OriginOffset, "Default OriginOffset should be Vector2.zero");
    }

    [UnityTest]
    public IEnumerator PlayfieldRenderer_CellsArePositionedCorrectly()
    {
        yield return null; // Wait for Awake to run

        // With default CellSize 0.5 and OriginOffset (0,0):
        // Cell_2_0 should be at (0, 0)
        // Cell_2_1 should be at (0.5, 0)
        // Cell_3_0 should be at (0, 0.5)
        // Cell_21_9 should be at (4.5, 9.5)

        Transform cell2_0 = _rendererObject.transform.Find("Cell_2_0");
        Transform cell2_1 = _rendererObject.transform.Find("Cell_2_1");
        Transform cell3_0 = _rendererObject.transform.Find("Cell_3_0");
        Transform cell21_9 = _rendererObject.transform.Find("Cell_21_9");

        Assert.IsNotNull(cell2_0);
        Assert.IsNotNull(cell2_1);
        Assert.IsNotNull(cell3_0);
        Assert.IsNotNull(cell21_9);

        Assert.AreEqual(new Vector3(0f, 0f, 0f), cell2_0.position, "Cell_2_0 should be at origin");
        Assert.AreEqual(new Vector3(0.5f, 0f, 0f), cell2_1.position, "Cell_2_1 should be at (0.5, 0)");
        Assert.AreEqual(new Vector3(0f, 0.5f, 0f), cell3_0.position, "Cell_3_0 should be at (0, 0.5)");
        Assert.AreEqual(new Vector3(4.5f, 9.5f, 0f), cell21_9.position, "Cell_21_9 should be at (4.5, 9.5)");
    }

    [UnityTest]
    public IEnumerator PlayfieldRenderer_BorderIsBehindGrid()
    {
        yield return null; // Wait for Awake to run

        GameObject border = null;
        foreach (Transform child in _rendererObject.transform)
        {
            if (child.gameObject.name == "WellBorder")
            {
                border = child.gameObject;
                break;
            }
        }

        Assert.IsNotNull(border);
        SpriteRenderer borderRenderer = border.GetComponent<SpriteRenderer>();
        Assert.AreEqual(-1, borderRenderer.sortingOrder, "Border sortingOrder should be -1 (behind grid)");

        // Check a cell has sortingOrder 0
        Transform cell2_0 = _rendererObject.transform.Find("Cell_2_0");
        SpriteRenderer cellRenderer = cell2_0.GetComponent<SpriteRenderer>();
        Assert.AreEqual(0, cellRenderer.sortingOrder, "Cell sortingOrder should be 0");
    }

    [UnityTest]
    public IEnumerator PlayfieldRenderer_RendersLockedCells()
    {
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
        _renderer.SetBlockSprites(sprites);

        yield return null; // Wait for Awake
        yield return null; // Wait for Start

        // Lock a piece in the playfield
        PlayfieldModel model = _controller.Playfield;
        model.SetCell(10, 5, 3); // Row 10, Col 5, piece type T (value 3)
        model.SetCell(11, 5, 1); // Row 11, Col 5, piece type I (value 1)

        yield return null; // Let Update render

        // Find the cell renderers and check their sprites
        Transform cell10_5 = _rendererObject.transform.Find("Cell_10_5");
        Transform cell11_5 = _rendererObject.transform.Find("Cell_11_5");

        SpriteRenderer sr10_5 = cell10_5.GetComponent<SpriteRenderer>();
        SpriteRenderer sr11_5 = cell11_5.GetComponent<SpriteRenderer>();

        Assert.AreEqual(sprites[3], sr10_5.sprite, "Cell_10_5 should show sprite for piece type T (index 3)");
        Assert.AreEqual(sprites[1], sr11_5.sprite, "Cell_11_5 should show sprite for piece type I (index 1)");

        // Clean up test textures
        foreach (var sprite in sprites)
        {
            if (sprite != null)
                UnityEngine.Object.Destroy(sprite);
        }
    }

    [UnityTest]
    public IEnumerator PlayfieldRenderer_EmptyCellsShowEmptySprite()
    {
        Sprite[] sprites = new Sprite[8];
        for (int i = 0; i < 8; i++)
        {
            sprites[i] = Sprite.Create(
                new Texture2D(1, 1),
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f)
            );
        }
        _renderer.SetBlockSprites(sprites);

        yield return null; // Wait for Awake
        yield return null; // Wait for Start
        yield return null; // Let Update render

        // Check an empty cell
        Transform cell5_0 = _rendererObject.transform.Find("Cell_5_0");
        SpriteRenderer sr = cell5_0.GetComponent<SpriteRenderer>();

        Assert.AreEqual(sprites[0], sr.sprite, "Empty cell should show empty sprite (index 0)");

        // Clean up
        foreach (var sprite in sprites)
        {
            if (sprite != null)
                UnityEngine.Object.Destroy(sprite);
        }
    }

    [UnityTest]
    public IEnumerator PlayfieldRenderer_OnStateChanged_TriggersReRender()
    {
        Sprite[] sprites = new Sprite[8];
        for (int i = 0; i < 8; i++)
        {
            sprites[i] = Sprite.Create(
                new Texture2D(1, 1),
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f)
            );
        }
        _renderer.SetBlockSprites(sprites);

        yield return null; // Wait for Awake
        yield return null; // Wait for Start

        // Set a cell value
        _controller.Playfield.SetCell(10, 5, 2); // O piece
        yield return null; // Render

        Transform cell10_5 = _rendererObject.transform.Find("Cell_10_5");
        SpriteRenderer sr = cell10_5.GetComponent<SpriteRenderer>();
        Assert.AreEqual(sprites[2], sr.sprite, "Cell should show O piece sprite");

        // Change the cell value
        _controller.Playfield.SetCell(10, 5, 4); // S piece
        yield return null; // Render

        Assert.AreEqual(sprites[4], sr.sprite, "Cell should update to S piece sprite after change");

        // Clean up
        foreach (var sprite in sprites)
        {
            if (sprite != null)
                UnityEngine.Object.Destroy(sprite);
        }
    }

    [UnityTest]
    public IEnumerator PlayfieldRenderer_WithoutGameplayController_DoesNotThrow()
    {
        // Create a renderer without a controller reference
        GameObject standaloneRenderer = new GameObject("StandaloneRenderer");
        var standalone = standaloneRenderer.AddComponent<PlayfieldRenderer>();
        // _gameplayController is null by default

        yield return null; // Awake
        yield return null; // Start
        yield return null; // Update

        // If we reached here without crashing, no exception was thrown
        Assert.IsNotNull(standalone, "Renderer should exist and not have thrown");
        UnityEngine.Object.Destroy(standaloneRenderer);
    }

    [UnityTest]
    public IEnumerator PlayfieldRenderer_BlockSpritesNull_DoesNotThrow()
    {
        yield return null; // Wait for Awake
        yield return null; // Wait for Start
        yield return null; // Let Update render (with null _blockSprites)

        Assert.IsTrue(true, "Renderer should handle null BlockSprites without throwing");
    }
}
