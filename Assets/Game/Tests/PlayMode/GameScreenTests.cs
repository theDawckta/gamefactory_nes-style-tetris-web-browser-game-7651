using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

public class GameScreenTests
{
    private GameObject _go;
    private GameScreen _screen;
    private PanelSettings _panelSettings;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _go.SetActive(false);
        _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var doc = _go.AddComponent<UIDocument>();
        doc.panelSettings = _panelSettings;
        _screen = _go.AddComponent<GameScreen>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_go);
        Object.Destroy(_panelSettings);
    }

    [UnityTest]
    public IEnumerator Awake_WithNoUxml_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _go.SetActive(true));
        yield return null;
    }

    [UnityTest]
    public IEnumerator GameScreen_StartsHidden()
    {
        _go.SetActive(true);
        yield return null;
        Assert.IsFalse(_screen.IsVisible, "GameScreen should be hidden after Awake");
    }

    [UnityTest]
    public IEnumerator Show_SetsIsVisibleTrue()
    {
        _go.SetActive(true);
        yield return null;
        _screen.Show();
        Assert.IsTrue(_screen.IsVisible);
    }

    [UnityTest]
    public IEnumerator Hide_AfterShow_SetsIsVisibleFalse()
    {
        _go.SetActive(true);
        yield return null;
        _screen.Show();
        _screen.Hide();
        Assert.IsFalse(_screen.IsVisible);
    }

    [UnityTest]
    public IEnumerator Show_SetsDisplayFlex()
    {
        _go.SetActive(true);
        yield return null;
        _screen.Show();
        var doc = _go.GetComponent<UIDocument>();
        Assert.AreEqual(DisplayStyle.Flex, doc.rootVisualElement.style.display.value);
    }

    [UnityTest]
    public IEnumerator Hide_SetsDisplayNone()
    {
        _go.SetActive(true);
        yield return null;
        _screen.Show();
        _screen.Hide();
        var doc = _go.GetComponent<UIDocument>();
        Assert.AreEqual(DisplayStyle.None, doc.rootVisualElement.style.display.value);
    }

    [UnityTest]
    public IEnumerator MultipleShow_DoesNotThrow()
    {
        _go.SetActive(true);
        yield return null;
        Assert.DoesNotThrow(() => { _screen.Show(); _screen.Show(); });
    }

    [UnityTest]
    public IEnumerator Hide_WhenAlreadyHidden_DoesNotThrow()
    {
        _go.SetActive(true);
        yield return null;
        Assert.DoesNotThrow(() => _screen.Hide());
    }

    [UnityTest]
    public IEnumerator ScoreRegion_NullWhenNoUxml()
    {
        _go.SetActive(true);
        yield return null;
        Assert.IsNull(_screen.ScoreRegion);
    }

    [UnityTest]
    public IEnumerator LevelRegion_NullWhenNoUxml()
    {
        _go.SetActive(true);
        yield return null;
        Assert.IsNull(_screen.LevelRegion);
    }

    [UnityTest]
    public IEnumerator LinesRegion_NullWhenNoUxml()
    {
        _go.SetActive(true);
        yield return null;
        Assert.IsNull(_screen.LinesRegion);
    }

    [UnityTest]
    public IEnumerator NextRegion_NullWhenNoUxml()
    {
        _go.SetActive(true);
        yield return null;
        Assert.IsNull(_screen.NextRegion);
    }
}
