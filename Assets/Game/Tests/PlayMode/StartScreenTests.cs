using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;
using Game.Gameplay;

public class StartScreenTests
{
    private GameObject _go;
    private StartScreen _screen;
    private PanelSettings _panelSettings;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _go.SetActive(false);
        _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var doc = _go.AddComponent<UIDocument>();
        doc.panelSettings = _panelSettings;
        _screen = _go.AddComponent<StartScreen>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_go);
        Object.Destroy(_panelSettings);
    }

    [UnityTest]
    public IEnumerator Awake_StartsHidden()
    {
        _go.SetActive(true);
        yield return null;
        Assert.IsFalse(_screen.IsVisible, "StartScreen should be hidden after Awake");
    }

    [UnityTest]
    public IEnumerator Show_SetsIsVisible_True()
    {
        _go.SetActive(true);
        yield return null;
        _screen.Show();
        Assert.IsTrue(_screen.IsVisible);
    }

    [UnityTest]
    public IEnumerator Hide_SetsIsVisible_False()
    {
        _go.SetActive(true);
        yield return null;
        _screen.Show();
        _screen.Hide();
        Assert.IsFalse(_screen.IsVisible);
    }

    [UnityTest]
    public IEnumerator Show_WithNullService_DoesNotThrow()
    {
        _go.SetActive(true);
        yield return null;
        Assert.DoesNotThrow(() => _screen.Show());
    }

    [UnityTest]
    public IEnumerator RefreshLeaderboard_WithNullEntries_DoesNotThrow()
    {
        _go.SetActive(true);
        yield return null;
        Assert.DoesNotThrow(() => _screen.RefreshLeaderboard(null));
    }

    [UnityTest]
    public IEnumerator RefreshLeaderboard_WithEntries_DoesNotThrow()
    {
        _go.SetActive(true);
        yield return null;
        var entries = new LeaderboardEntry[]
        {
            new LeaderboardEntry { name = "AAA", score = 1000 }
        };
        Assert.DoesNotThrow(() => _screen.RefreshLeaderboard(entries));
    }

    [UnityTest]
    public IEnumerator OnStartRequested_CanSubscribeAndUnsubscribe()
    {
        _go.SetActive(true);
        yield return null;
        bool fired = false;
        System.Action handler = () => fired = true;
        _screen.OnStartRequested += handler;
        _screen.OnStartRequested -= handler;
        Assert.IsFalse(fired);
    }

    [UnityTest]
    public IEnumerator Show_RootDisplayStyle_IsFlexAfterShow()
    {
        _go.SetActive(true);
        yield return null;
        _screen.Show();
        var doc = _go.GetComponent<UIDocument>();
        Assert.AreEqual(DisplayStyle.Flex, doc.rootVisualElement.style.display.value);
    }

    [UnityTest]
    public IEnumerator Hide_RootDisplayStyle_IsNoneAfterHide()
    {
        _go.SetActive(true);
        yield return null;
        _screen.Show();
        _screen.Hide();
        var doc = _go.GetComponent<UIDocument>();
        Assert.AreEqual(DisplayStyle.None, doc.rootVisualElement.style.display.value);
    }
}
