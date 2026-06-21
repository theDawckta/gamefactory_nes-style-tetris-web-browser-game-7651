using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Gameplay;

public class PlayfieldRendererTests
{
    private GameObject _rendererGo;
    private PlayfieldRenderer _renderer;

    [SetUp]
    public void SetUp()
    {
        _rendererGo = new GameObject("PlayfieldRenderer");
        _renderer = _rendererGo.AddComponent<PlayfieldRenderer>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_rendererGo != null)
            Object.Destroy(_rendererGo);
    }

    private Sprite CreateTestSprite()
    {
        var tex = new Texture2D(2, 2);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
    }

    private Sprite[] CreateTestSpriteArray()
    {
        var sprites = new Sprite[8];
        for (int i = 0; i < 8; i++)
            sprites[i] = CreateTestSprite();
        return sprites;
    }

    [UnityTest]
    public IEnumerator Awake_Cells_AreCreatedIn20x10Grid()
    {
        yield return null;
        Assert.AreEqual(GameRules.PLAYFIELD_VISIBLE_HEIGHT, _renderer.Cells.GetLength(0));
        Assert.AreEqual(GameRules.PLAYFIELD_WIDTH, _renderer.Cells.GetLength(1));
    }

    [UnityTest]
    public IEnumerator Awake_AllCells_HaveSpriteRenderer()
    {
        yield return null;
        for (int r = 0; r < GameRules.PLAYFIELD_VISIBLE_HEIGHT; r++)
            for (int c = 0; c < GameRules.PLAYFIELD_WIDTH; c++)
                Assert.IsNotNull(_renderer.Cells[r, c]);
    }

    [UnityTest]
    public IEnumerator Awake_Cells_AreNamedWithModelRowCoords()
    {
        yield return null;
        // visRow 0 = model row PLAYFIELD_BUFFER_ROWS (2), col 0
        Assert.IsNotNull(_rendererGo.transform.Find("Cell_2_0"), "Cell_2_0 should exist");
        // visRow 19 = model row 21, col 9
        Assert.IsNotNull(_rendererGo.transform.Find("Cell_21_9"), "Cell_21_9 should exist");
    }

    [UnityTest]
    public IEnumerator Awake_BufferRowCells_AreNotCreated()
    {
        yield return null;
        Assert.IsNull(_rendererGo.transform.Find("Cell_0_0"), "Buffer row Cell_0_0 must not exist");
        Assert.IsNull(_rendererGo.transform.Find("Cell_1_0"), "Buffer row Cell_1_0 must not exist");
    }

    [UnityTest]
    public IEnumerator Awake_WellBorder_IsCreated()
    {
        yield return null;
        Assert.IsNotNull(_renderer.WellBorder, "WellBorder SpriteRenderer must be set");
        Assert.IsNotNull(_rendererGo.transform.Find("WellBorder"), "WellBorder child must exist");
    }

    [UnityTest]
    public IEnumerator Awake_CellPositions_MatchDefaultCellSizeAndOrigin()
    {
        yield return null;
        // Default: CellSize=0.5, OriginOffset=(0,0)
        // Cell[0,0]: x=0, y=0
        var pos00 = _renderer.Cells[0, 0].transform.localPosition;
        Assert.That(pos00.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(pos00.y, Is.EqualTo(0f).Within(0.001f));

        // Cell[0,1]: x=0.5, y=0
        var pos01 = _renderer.Cells[0, 1].transform.localPosition;
        Assert.That(pos01.x, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(pos01.y, Is.EqualTo(0f).Within(0.001f));

        // Cell[1,0]: x=0, y=-0.5
        var pos10 = _renderer.Cells[1, 0].transform.localPosition;
        Assert.That(pos10.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(pos10.y, Is.EqualTo(-0.5f).Within(0.001f));
    }

    [UnityTest]
    public IEnumerator Render_WithNullController_DoesNotThrow()
    {
        yield return null;
        _renderer.GameplayController = null;
        Assert.DoesNotThrow(() => _renderer.Render());
    }

    [UnityTest]
    public IEnumerator Render_EmptyPlayfield_AllCellsUseEmptySprite()
    {
        yield return null;
        _renderer.BlockSprites = CreateTestSpriteArray();

        var controllerGo = new GameObject("Controller");
        var controller = controllerGo.AddComponent<GameplayController>();
        _renderer.GameplayController = controller;

        _renderer.Render();

        // Playfield is empty and default active piece spawns at model row 0 (buffer row),
        // so no visible cells should be affected by the piece overlay
        for (int r = 0; r < GameRules.PLAYFIELD_VISIBLE_HEIGHT; r++)
            for (int c = 0; c < GameRules.PLAYFIELD_WIDTH; c++)
                Assert.AreEqual(_renderer.BlockSprites[0], _renderer.Cells[r, c].sprite,
                    $"Empty cell [{r},{c}] must use BlockSprites[0]");

        Object.Destroy(controllerGo);
    }

    [UnityTest]
    public IEnumerator Render_LockedCell_UsesCorrectPieceSprite()
    {
        yield return null;
        var sprites = CreateTestSpriteArray();
        _renderer.BlockSprites = sprites;

        var controllerGo = new GameObject("Controller");
        var controller = controllerGo.AddComponent<GameplayController>();
        _renderer.GameplayController = controller;

        // Lock an I-piece cell at the top of the visible area (model row 2 = visRow 0, col 0)
        controller.Playfield.SetCell(GameRules.PLAYFIELD_BUFFER_ROWS, 0, 1);

        _renderer.Render();

        Assert.AreEqual(sprites[1], _renderer.Cells[0, 0].sprite,
            "Locked I-piece cell must use BlockSprites[1]");

        Object.Destroy(controllerGo);
    }

    [UnityTest]
    public IEnumerator Render_AfterStartGame_CompletesWithoutError()
    {
        yield return null;
        _renderer.BlockSprites = CreateTestSpriteArray();

        var controllerGo = new GameObject("Controller");
        var controller = controllerGo.AddComponent<GameplayController>();
        controller.StartGame();
        _renderer.GameplayController = controller;

        Assert.DoesNotThrow(() => _renderer.Render());

        Object.Destroy(controllerGo);
    }

    [UnityTest]
    public IEnumerator Render_AfterLineClear_UpdatesDisplay()
    {
        yield return null;
        var sprites = CreateTestSpriteArray();
        _renderer.BlockSprites = sprites;

        var controllerGo = new GameObject("Controller");
        var controller = controllerGo.AddComponent<GameplayController>();
        _renderer.GameplayController = controller;

        // Fill the bottom visible row (model row 21 = visRow 19) with I-piece cells
        int bottomModelRow = GameRules.PLAYFIELD_TOTAL_HEIGHT - 1;
        for (int c = 0; c < GameRules.PLAYFIELD_WIDTH; c++)
            controller.Playfield.SetCell(bottomModelRow, c, 1);

        // Render: bottom row should show I-piece sprite
        _renderer.Render();
        Assert.AreEqual(sprites[1], _renderer.Cells[GameRules.PLAYFIELD_VISIBLE_HEIGHT - 1, 0].sprite,
            "Bottom row must show I-piece sprite before clear");

        // Clear lines
        controller.Playfield.ClearLines();

        // Render again: bottom row should now be empty
        _renderer.Render();
        Assert.AreEqual(sprites[0], _renderer.Cells[GameRules.PLAYFIELD_VISIBLE_HEIGHT - 1, 0].sprite,
            "Bottom row must show empty sprite after line clear");

        Object.Destroy(controllerGo);
    }
}
