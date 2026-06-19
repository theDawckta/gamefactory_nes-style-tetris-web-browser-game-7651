using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

[TestFixture]
public class GameScreenNextWidgetTests
{
    private GameObject _go;
    private UIDocument _doc;
    private GameScreenNextWidget _widget;
    private Sprite[] _testSprites;

    [SetUp]
    public void SetUp()
    {
        // Create test sprites: index 0 = empty, 1-7 = piece types
        _testSprites = new Sprite[8];
        for (int i = 0; i < 8; i++)
        {
            _testSprites[i] = Sprite.Create(
                new Texture2D(1, 1),
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f)
            );
        }

        _go = new GameObject("TestNextWidget");
        _doc = _go.AddComponent<UIDocument>();

        // Build the next-region VisualElement with NEXT header label
        var nextRegion = new VisualElement();
        nextRegion.name = "next-region";
        nextRegion.AddToClassList("next-region");

        var headerLabel = new Label("NEXT");
        headerLabel.AddToClassList("region-label");
        nextRegion.Add(headerLabel);

        _doc.rootVisualElement.Add(nextRegion);

        // Add the widget after the visual tree is set up
        _widget = _go.AddComponent<GameScreenNextWidget>();
        _widget.SetBlockSprites(_testSprites);
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null)
        {
            Object.Destroy(_go);
            _go = null;
        }
        // Clean up test sprites
        if (_testSprites != null)
        {
            foreach (var sprite in _testSprites)
            {
                if (sprite != null)
                    Object.Destroy(sprite);
            }
        }
    }

    [UnityTest]
    public IEnumerator Awake_CreatesGridContainer()
    {
        yield return null; // Wait for Awake to run

        var nextRegion = _doc.rootVisualElement.Q<VisualElement>("next-region");
        Assert.IsNotNull(nextRegion, "#next-region should exist");

        var gridContainer = nextRegion.Q<VisualElement>("next-grid");
        Assert.IsNotNull(gridContainer, "next-grid container should be created");
    }

    [UnityTest]
    public IEnumerator Awake_Creates16ImageCells()
    {
        yield return null; // Wait for Awake to run

        var nextRegion = _doc.rootVisualElement.Q<VisualElement>("next-region");
        var gridContainer = nextRegion.Q<VisualElement>("next-grid");

        int imageCount = gridContainer.Query<Image>().ToList().Count;

        Assert.AreEqual(16, imageCount, "Should create 16 Image cells (4x4 grid)");
    }

    [UnityTest]
    public IEnumerator UpdateNextPiece_I_DisplaysHorizontalBar()
    {
        yield return null; // Wait for Awake to run

        _widget.UpdateNextPiece(PieceType.I);
        yield return null;

        // I piece rotation 0: {0,0, 0,-1, 0,1, 0,2}
        // All in row 0, cols -1,0,1,2 -> width=4, height=1
        // Centered: rowOffset=(4-1)/2=1, colOffset=(4-4)/2=0
        // Grid positions: row 1, cols 0,1,2,3

        var nextRegion = _doc.rootVisualElement.Q<VisualElement>("next-region");
        var gridContainer = nextRegion.Q<VisualElement>("next-grid");

        // Check row 1 has all 4 cells filled with I sprite (index 1)
        for (int col = 0; col < 4; col++)
        {
            var cell = gridContainer.Q<Image>("next-grid-cell-1-" + col);
            Assert.IsNotNull(cell, "Cell at row 1, col " + col + " should exist");
            Assert.AreEqual(_testSprites[1], cell.sprite,
                "Cell at row 1, col " + col + " should show I piece sprite");
        }

        // Check rows 0, 2, 3 are all empty
        foreach (int row in new[] { 0, 2, 3 })
        {
            for (int col = 0; col < 4; col++)
            {
                var cell = gridContainer.Q<Image>("next-grid-cell-" + row + "-" + col);
                Assert.IsNotNull(cell, "Cell at row " + row + ", col " + col + " should exist");
                Assert.AreEqual(_testSprites[0], cell.sprite,
                    "Empty cell at row " + row + ", col " + col + " should show empty sprite");
            }
        }
    }

    [UnityTest]
    public IEnumerator UpdateNextPiece_O_Displays2x2Square()
    {
        yield return null; // Wait for Awake to run

        _widget.UpdateNextPiece(PieceType.O);
        yield return null;

        // O piece rotation 0: {0,0, 0,1, 1,0, 1,1}
        // Rows 0-1, cols 0-1 -> width=2, height=2
        // Centered: rowOffset=(4-2)/2=1, colOffset=(4-2)/2=1
        // Grid positions: rows 1,2 and cols 1,2

        var nextRegion = _doc.rootVisualElement.Q<VisualElement>("next-region");
        var gridContainer = nextRegion.Q<VisualElement>("next-grid");

        // Check the 2x2 filled cells with O sprite (index 2)
        for (int row = 1; row <= 2; row++)
        {
            for (int col = 1; col <= 2; col++)
            {
                var cell = gridContainer.Q<Image>("next-grid-cell-" + row + "-" + col);
                Assert.IsNotNull(cell, "Cell at row " + row + ", col " + col + " should exist");
                Assert.AreEqual(_testSprites[2], cell.sprite,
                    "Cell at row " + row + ", col " + col + " should show O piece sprite");
            }
        }

        // Check all other cells are empty
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                if (row >= 1 && row <= 2 && col >= 1 && col <= 2)
                    continue; // Skip filled cells

                var cell = gridContainer.Q<Image>("next-grid-cell-" + row + "-" + col);
                Assert.IsNotNull(cell, "Cell at row " + row + ", col " + col + " should exist");
                Assert.AreEqual(_testSprites[0], cell.sprite,
                    "Empty cell at row " + row + ", col " + col + " should show empty sprite");
            }
        }
    }

    [UnityTest]
    public IEnumerator UpdateNextPiece_T_DisplaysCorrectShape()
    {
        yield return null; // Wait for Awake to run

        _widget.UpdateNextPiece(PieceType.T);
        yield return null;

        // T piece rotation 0: {0,-1, 0,0, 0,1, 1,0}
        // Rows 0-1, cols -1,0,1 -> width=3, height=2
        // Centered: rowOffset=(4-2)/2=1, colOffset=(4-3)/2=0
        // Grid positions: row 1 cols 0,1,2 and row 2 col 1

        var nextRegion = _doc.rootVisualElement.Q<VisualElement>("next-region");
        var gridContainer = nextRegion.Q<VisualElement>("next-grid");

        // Check row 1: cols 0,1,2 filled with T sprite (index 3)
        for (int col = 0; col < 3; col++)
        {
            var cell = gridContainer.Q<Image>("next-grid-cell-1-" + col);
            Assert.IsNotNull(cell, "Cell at row 1, col " + col + " should exist");
            Assert.AreEqual(_testSprites[3], cell.sprite,
                "Cell at row 1, col " + col + " should show T piece sprite");
        }

        // Check row 2: col 1 filled
        var cell2_1 = gridContainer.Q<Image>("next-grid-cell-2-1");
        Assert.IsNotNull(cell2_1, "Cell at row 2, col 1 should exist");
        Assert.AreEqual(_testSprites[3], cell2_1.sprite,
            "Cell at row 2, col 1 should show T piece sprite");

        // Check row 2: cols 0,2,3 are empty
        foreach (int col in new[] { 0, 2, 3 })
        {
            var cell = gridContainer.Q<Image>("next-grid-cell-2-" + col);
            Assert.AreEqual(_testSprites[0], cell.sprite,
                "Empty cell at row 2, col " + col + " should show empty sprite");
        }

        // Check rows 0 and 3 are all empty
        foreach (int row in new[] { 0, 3 })
        {
            for (int col = 0; col < 4; col++)
            {
                var cell = gridContainer.Q<Image>("next-grid-cell-" + row + "-" + col);
                Assert.AreEqual(_testSprites[0], cell.sprite,
                    "Empty cell at row " + row + ", col " + col + " should show empty sprite");
            }
        }
    }

    [UnityTest]
    public IEnumerator UpdateNextPiece_MultipleCalls_UpdatesCorrectly()
    {
        yield return null; // Wait for Awake to run

        _widget.UpdateNextPiece(PieceType.I);
        yield return null;

        var nextRegion = _doc.rootVisualElement.Q<VisualElement>("next-region");
        var gridContainer = nextRegion.Q<VisualElement>("next-grid");

        // Row 1, col 0 should be I sprite
        var cell = gridContainer.Q<Image>("next-grid-cell-1-0");
        Assert.AreEqual(_testSprites[1], cell.sprite, "Should show I piece sprite");

        _widget.UpdateNextPiece(PieceType.O);
        yield return null;

        // Same cell should now be empty (O is centered at rows 1-2, cols 1-2)
        Assert.AreEqual(_testSprites[0], cell.sprite,
            "Cell should be empty after switching to O piece");

        // O piece cell should be filled
        var oCell = gridContainer.Q<Image>("next-grid-cell-1-1");
        Assert.AreEqual(_testSprites[2], oCell.sprite,
            "Cell at row 1, col 1 should show O piece sprite");
    }

    [UnityTest]
    public IEnumerator HeaderLabel_RemainsUnchanged()
    {
        yield return null; // Wait for Awake to run

        var nextRegion = _doc.rootVisualElement.Q<VisualElement>("next-region");
        var headerLabel = nextRegion.Children().OfType<Label>().FirstOrDefault(
            l => l.ClassListContains("region-label"));

        Assert.IsNotNull(headerLabel, "region-label should exist");
        Assert.AreEqual("NEXT", headerLabel.text, "Header should be NEXT");

        _widget.UpdateNextPiece(PieceType.T);
        yield return null;

        Assert.AreEqual("NEXT", headerLabel.text, "Header should still be NEXT after UpdateNextPiece");
    }

    [UnityTest]
    public IEnumerator UpdateNextPiece_WithoutSprites_DoesNotThrow()
    {
        yield return null; // Wait for Awake to run

        // Set sprites to null
        _widget.SetBlockSprites(null);

        // Should not throw
        _widget.UpdateNextPiece(PieceType.I);
        yield return null;

        Assert.IsTrue(true, "UpdateNextPiece should handle null sprites without throwing");
    }

    [UnityTest]
    public IEnumerator UpdateNextPiece_AllPieceTypes_DoesNotThrow()
    {
        yield return null; // Wait for Awake to run

        foreach (PieceType type in System.Enum.GetValues(typeof(PieceType)))
        {
            _widget.UpdateNextPiece(type);
            yield return null;
        }

        Assert.IsTrue(true, "All piece types should update without throwing");
    }

    [UnityTest]
    public IEnumerator BlockSpritesProperty_ReturnsAssignedSprites()
    {
        yield return null; // Wait for Awake to run

        Assert.AreEqual(_testSprites, _widget.BlockSprites,
            "BlockSprites property should return the assigned sprites array");
    }
}
