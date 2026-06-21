using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;
using Game.Gameplay;

public class GameScreenNextWidgetTests
{
    private GameObject _go;
    private GameScreenNextWidget _widget;
    private PanelSettings _panelSettings;
    private Sprite[] _testSprites;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _go.SetActive(false);
        _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var doc = _go.AddComponent<UIDocument>();
        doc.panelSettings = _panelSettings;
        _widget = _go.AddComponent<GameScreenNextWidget>();

        _testSprites = new Sprite[8];
        for (int i = 0; i < 8; i++)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, new Color(i / 8f, 0, 0));
            tex.Apply();
            _testSprites[i] = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
        }
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.Destroy(_go);
        UnityEngine.Object.Destroy(_panelSettings);
        foreach (var s in _testSprites)
        {
            if (s != null)
            {
                UnityEngine.Object.Destroy(s.texture);
                UnityEngine.Object.Destroy(s);
            }
        }
    }

    [UnityTest]
    public IEnumerator Awake_WithNoNextRegion_DoesNotThrow()
    {
        _go.SetActive(true);
        yield return null;
        Assert.IsNotNull(_widget);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_Creates16ImageCells()
    {
        _go.SetActive(true);
        yield return null;
        var nextRegion = new VisualElement();
        _widget.PopulateRegion(nextRegion);
        var images = nextRegion.Query<Image>().ToList();
        Assert.AreEqual(16, images.Count);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_HeaderLabel_IsNext()
    {
        _go.SetActive(true);
        yield return null;
        var nextRegion = new VisualElement();
        _widget.PopulateRegion(nextRegion);
        var header = (Label)nextRegion.ElementAt(0);
        Assert.AreEqual("NEXT", header.text);
    }

    [UnityTest]
    public IEnumerator UpdateNextPiece_I_DisplaysHorizontalBar()
    {
        _widget.BlockSprites = _testSprites;
        _go.SetActive(true);
        yield return null;
        var nextRegion = new VisualElement();
        _widget.PopulateRegion(nextRegion);
        _widget.UpdateNextPiece(PieceType.I);

        // I piece R0: (0,0),(0,1),(0,2),(0,3) -- indices 0,1,2,3
        var images = nextRegion.Query<Image>().ToList();
        Assert.AreEqual(_testSprites[1], images[0].sprite, "I piece cell (0,0)");
        Assert.AreEqual(_testSprites[1], images[1].sprite, "I piece cell (0,1)");
        Assert.AreEqual(_testSprites[1], images[2].sprite, "I piece cell (0,2)");
        Assert.AreEqual(_testSprites[1], images[3].sprite, "I piece cell (0,3)");
        Assert.AreEqual(_testSprites[0], images[4].sprite, "empty cell (1,0)");
        Assert.AreEqual(_testSprites[0], images[8].sprite, "empty cell (2,0)");
        Assert.AreEqual(_testSprites[0], images[12].sprite, "empty cell (3,0)");
    }

    [UnityTest]
    public IEnumerator UpdateNextPiece_O_Displays2x2Square()
    {
        _widget.BlockSprites = _testSprites;
        _go.SetActive(true);
        yield return null;
        var nextRegion = new VisualElement();
        _widget.PopulateRegion(nextRegion);
        _widget.UpdateNextPiece(PieceType.O);

        // O piece R0: (0,0),(0,1),(1,0),(1,1) -- indices 0,1,4,5
        var images = nextRegion.Query<Image>().ToList();
        Assert.AreEqual(_testSprites[2], images[0].sprite, "O piece cell (0,0)");
        Assert.AreEqual(_testSprites[2], images[1].sprite, "O piece cell (0,1)");
        Assert.AreEqual(_testSprites[2], images[4].sprite, "O piece cell (1,0)");
        Assert.AreEqual(_testSprites[2], images[5].sprite, "O piece cell (1,1)");
        Assert.AreEqual(_testSprites[0], images[2].sprite, "empty cell (0,2)");
        Assert.AreEqual(_testSprites[0], images[3].sprite, "empty cell (0,3)");
    }

    [UnityTest]
    public IEnumerator UpdateNextPiece_T_DisplaysCorrectShape()
    {
        _widget.BlockSprites = _testSprites;
        _go.SetActive(true);
        yield return null;
        var nextRegion = new VisualElement();
        _widget.PopulateRegion(nextRegion);
        _widget.UpdateNextPiece(PieceType.T);

        // T piece R0: (0,0),(0,1),(0,2),(1,1) -- indices 0,1,2,5
        var images = nextRegion.Query<Image>().ToList();
        Assert.AreEqual(_testSprites[3], images[0].sprite, "T piece cell (0,0)");
        Assert.AreEqual(_testSprites[3], images[1].sprite, "T piece cell (0,1)");
        Assert.AreEqual(_testSprites[3], images[2].sprite, "T piece cell (0,2)");
        Assert.AreEqual(_testSprites[3], images[5].sprite, "T piece cell (1,1)");
        Assert.AreEqual(_testSprites[0], images[3].sprite, "empty cell (0,3)");
        Assert.AreEqual(_testSprites[0], images[4].sprite, "empty cell (1,0)");
    }

    [UnityTest]
    public IEnumerator UpdateNextPiece_MultipleCalls_UpdatesCorrectly()
    {
        _widget.BlockSprites = _testSprites;
        _go.SetActive(true);
        yield return null;
        var nextRegion = new VisualElement();
        _widget.PopulateRegion(nextRegion);
        _widget.UpdateNextPiece(PieceType.I);
        _widget.UpdateNextPiece(PieceType.O);

        var images = nextRegion.Query<Image>().ToList();
        // After switching to O, cell (0,3) should be empty (was I piece)
        Assert.AreEqual(_testSprites[2], images[0].sprite, "O piece cell (0,0) after switch");
        Assert.AreEqual(_testSprites[0], images[3].sprite, "empty cell (0,3) after switch to O");
    }

    [UnityTest]
    public IEnumerator UpdateNextPiece_WithoutSprites_DoesNotThrow()
    {
        _go.SetActive(true);
        yield return null;
        var nextRegion = new VisualElement();
        _widget.PopulateRegion(nextRegion);
        Assert.DoesNotThrow(() => _widget.UpdateNextPiece(PieceType.I));
    }

    [UnityTest]
    public IEnumerator UpdateNextPiece_AllPieceTypes_DoesNotThrow()
    {
        _go.SetActive(true);
        yield return null;
        var nextRegion = new VisualElement();
        _widget.PopulateRegion(nextRegion);
        foreach (PieceType pt in Enum.GetValues(typeof(PieceType)))
            Assert.DoesNotThrow(() => _widget.UpdateNextPiece(pt));
        yield return null;
    }

    [UnityTest]
    public IEnumerator HeaderLabel_RemainsUnchanged_AfterUpdateNextPiece()
    {
        _widget.BlockSprites = _testSprites;
        _go.SetActive(true);
        yield return null;
        var nextRegion = new VisualElement();
        _widget.PopulateRegion(nextRegion);
        _widget.UpdateNextPiece(PieceType.T);
        var header = (Label)nextRegion.ElementAt(0);
        Assert.AreEqual("NEXT", header.text);
    }

    [UnityTest]
    public IEnumerator BlockSpritesProperty_ReturnsAssignedSprites()
    {
        _widget.BlockSprites = _testSprites;
        _go.SetActive(true);
        yield return null;
        Assert.AreEqual(_testSprites, _widget.BlockSprites);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_CalledTwice_ClearsAndRepopulates()
    {
        _go.SetActive(true);
        yield return null;
        var nextRegion = new VisualElement();
        _widget.PopulateRegion(nextRegion);
        _widget.PopulateRegion(nextRegion);
        var images = nextRegion.Query<Image>().ToList();
        Assert.AreEqual(16, images.Count);
    }
}
