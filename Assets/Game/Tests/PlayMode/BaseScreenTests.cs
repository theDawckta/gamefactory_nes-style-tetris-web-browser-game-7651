using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

public class BaseScreenTests
{
    private class ConcreteScreen : BaseScreen
    {
        public VisualElement ExposedRoot => Root;
    }

    private GameObject _go;
    private PanelSettings _panelSettings;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _go.SetActive(false);
        _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var doc = _go.AddComponent<UIDocument>();
        doc.panelSettings = _panelSettings;
        _go.AddComponent<ConcreteScreen>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_go);
        Object.Destroy(_panelSettings);
    }

    [UnityTest]
    public IEnumerator BaseScreen_StartsHidden_AfterAwake()
    {
        _go.SetActive(true);
        yield return null;
        var screen = _go.GetComponent<ConcreteScreen>();
        Assert.IsFalse(screen.IsVisible);
        Assert.AreEqual(DisplayStyle.None, screen.ExposedRoot.style.display.value);
    }

    [UnityTest]
    public IEnumerator Show_SetsIsVisibleTrue_AndDisplayFlex()
    {
        _go.SetActive(true);
        yield return null;
        var screen = _go.GetComponent<ConcreteScreen>();
        screen.Show();
        Assert.IsTrue(screen.IsVisible);
        Assert.AreEqual(DisplayStyle.Flex, screen.ExposedRoot.style.display.value);
    }

    [UnityTest]
    public IEnumerator Hide_SetsIsVisibleFalse_AndDisplayNone()
    {
        _go.SetActive(true);
        yield return null;
        var screen = _go.GetComponent<ConcreteScreen>();
        screen.Show();
        screen.Hide();
        Assert.IsFalse(screen.IsVisible);
        Assert.AreEqual(DisplayStyle.None, screen.ExposedRoot.style.display.value);
    }

    [UnityTest]
    public IEnumerator ShowThenHide_LeavesIsVisibleFalse()
    {
        _go.SetActive(true);
        yield return null;
        var screen = _go.GetComponent<ConcreteScreen>();
        screen.Show();
        screen.Hide();
        Assert.IsFalse(screen.IsVisible);
    }
}
