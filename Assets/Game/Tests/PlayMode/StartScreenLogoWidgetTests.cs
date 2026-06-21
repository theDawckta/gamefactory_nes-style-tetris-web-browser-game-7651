using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

public class StartScreenLogoWidgetTests
{
    private GameObject _go;
    private StartScreenLogoWidget _widget;
    private PanelSettings _panelSettings;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _go.SetActive(false);
        _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var doc = _go.AddComponent<UIDocument>();
        doc.panelSettings = _panelSettings;
        _widget = _go.AddComponent<StartScreenLogoWidget>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_go);
        Object.Destroy(_panelSettings);
    }

    [UnityTest]
    public IEnumerator Awake_WithNoLogoRegion_DoesNotThrow()
    {
        _go.SetActive(true);
        yield return null;
        Assert.IsNotNull(_widget);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_NullSprite_RegionRemainsEmpty()
    {
        _go.SetActive(true);
        yield return null;
        _widget.TitleLogoSprite = null;
        var logoRegion = new VisualElement { name = "logo-region" };
        _widget.PopulateRegion(logoRegion);
        Assert.AreEqual(0, logoRegion.childCount);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_WithSprite_AddsImageToRegion()
    {
        _go.SetActive(true);
        yield return null;
        var texture = new Texture2D(64, 32);
        var sprite = Sprite.Create(texture, new Rect(0, 0, 64, 32), new Vector2(0.5f, 0.5f));
        _widget.TitleLogoSprite = sprite;
        var logoRegion = new VisualElement { name = "logo-region" };
        _widget.PopulateRegion(logoRegion);
        Assert.AreEqual(1, logoRegion.childCount);
        Assert.IsInstanceOf<Image>(logoRegion[0]);
        Object.Destroy(texture);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_WithSprite_ImageSizedToNativeDimensions()
    {
        _go.SetActive(true);
        yield return null;
        var texture = new Texture2D(64, 32);
        var sprite = Sprite.Create(texture, new Rect(0, 0, 64, 32), new Vector2(0.5f, 0.5f));
        _widget.TitleLogoSprite = sprite;
        var logoRegion = new VisualElement { name = "logo-region" };
        _widget.PopulateRegion(logoRegion);
        var image = (Image)logoRegion[0];
        Assert.AreEqual(64f, image.style.width.value.value);
        Assert.AreEqual(32f, image.style.height.value.value);
        Object.Destroy(texture);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_AppliesCenteringStylesToRegion()
    {
        _go.SetActive(true);
        yield return null;
        var logoRegion = new VisualElement { name = "logo-region" };
        _widget.PopulateRegion(logoRegion);
        Assert.AreEqual(Align.Center, logoRegion.style.alignItems.value);
        Assert.AreEqual(Justify.Center, logoRegion.style.justifyContent.value);
    }
}
