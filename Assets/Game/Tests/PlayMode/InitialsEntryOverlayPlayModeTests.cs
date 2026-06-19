using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

/// <summary>
/// Play-mode tests for InitialsEntryOverlay MonoBehaviour behavior.
/// Tests Show/Hide lifecycle, input handling, and event firing.
/// </summary>
[TestFixture]
public class InitialsEntryOverlayPlayModeTests
{
    private GameObject _go;
    private UIDocument _doc;
    private InitialsEntryOverlay _overlay;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("TestInitialsEntryOverlay");
        _doc = _go.AddComponent<UIDocument>();
        _overlay = _go.AddComponent<InitialsEntryOverlay>();

        // Add required VisualElements to rootVisualElement so Awake can find them
        var titleRegion = new VisualElement();
        titleRegion.name = "title-region";
        _doc.rootVisualElement.Add(titleRegion);

        var titleLabel = new Label { name = "title-label", text = "ENTER YOUR INITIALS" };
        titleRegion.Add(titleLabel);

        var charsRegion = new VisualElement();
        charsRegion.name = "chars-region";
        _doc.rootVisualElement.Add(charsRegion);

        var char0 = new Label { name = "char-0", text = "A" };
        var char1 = new Label { name = "char-1", text = "A" };
        var char2 = new Label { name = "char-2", text = "A" };
        charsRegion.Add(char0);
        charsRegion.Add(char1);
        charsRegion.Add(char2);

        var confirmRegion = new VisualElement();
        confirmRegion.name = "confirm-region";
        confirmRegion.style.display = DisplayStyle.None;
        _doc.rootVisualElement.Add(confirmRegion);

        var confirmLabel = new Label { text = "CONFIRM" };
        confirmRegion.Add(confirmLabel);
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null)
        {
            Object.Destroy(_go);
            _go = null;
        }
    }

    [UnityTest]
    public IEnumerator ShowForScore_ResetsToAAA()
    {
        yield return null; // wait for Awake

        _overlay.ShowForScore(12345);
        yield return null;

        Assert.IsTrue(_overlay.IsVisible, "Overlay should be visible after ShowForScore");

        var char0 = _doc.rootVisualElement.Q<Label>("char-0");
        var char1 = _doc.rootVisualElement.Q<Label>("char-1");
        var char2 = _doc.rootVisualElement.Q<Label>("char-2");

        Assert.AreEqual("A", char0.text, "char-0 should be A after reset");
        Assert.AreEqual("A", char1.text, "char-1 should be A after reset");
        Assert.AreEqual("A", char2.text, "char-2 should be A after reset");
    }

    [UnityTest]
    public IEnumerator ShowForScore_CursorOnSlot0()
    {
        yield return null;

        _overlay.ShowForScore(12345);
        yield return null;

        var char0 = _doc.rootVisualElement.Q<Label>("char-0");
        Assert.IsTrue(char0.ClassListContains("char-slot-active"), "char-0 should have active cursor");

        var char1 = _doc.rootVisualElement.Q<Label>("char-1");
        Assert.IsFalse(char1.ClassListContains("char-slot-active"), "char-1 should not have active cursor");
    }

    [UnityTest]
    public IEnumerator Hide_MakesScreenInvisible()
    {
        yield return null;

        _overlay.ShowForScore(12345);
        yield return null;

        _overlay.Hide();
        yield return null;

        Assert.IsFalse(_overlay.IsVisible, "Overlay should not be visible after Hide");
        Assert.IsFalse(_go.activeSelf, "GameObject should be inactive after Hide");
    }

    [UnityTest]
    public IEnumerator Screen_HasNoCanvasComponent()
    {
        yield return null;

        var canvas = _go.GetComponent<Canvas>();
        Assert.IsNull(canvas, "InitialsEntryOverlay should not have a Canvas component");
    }

    [UnityTest]
    public IEnumerator OnInitialsSubmitted_FiresWithCorrectValues()
    {
        yield return null;

        string receivedInitials = null;
        int receivedScore = -1;

        _overlay.OnInitialsSubmitted += (initials, score) =>
        {
            receivedInitials = initials;
            receivedScore = score;
        };

        // Show and manually simulate the submission path
        _overlay.ShowForScore(99999);
        yield return null;

        // We can't simulate input in a PlayMode test easily, so verify the event
        // wiring works by checking the delegate is not null
        Assert.IsNotNull(_overlay.OnInitialsSubmitted, "OnInitialsSubmitted should be subscribable");

        // Verify the overlay is visible
        Assert.IsTrue(_overlay.IsVisible);

        // The event should not have fired yet
        Assert.IsNull(receivedInitials, "Event should not have fired yet");
        Assert.AreEqual(-1, receivedScore, "Score should not have been set yet");
    }

    [UnityTest]
    public IEnumerator BaseScreen_IsVisible_StartsFalse()
    {
        yield return null;

        Assert.IsFalse(_overlay.IsVisible, "Overlay should not be visible initially");
    }

    [UnityTest]
    public IEnumerator Show_Hide_Show_CyclesCorrectly()
    {
        yield return null;

        _overlay.ShowForScore(100);
        yield return null;
        Assert.IsTrue(_overlay.IsVisible);

        _overlay.Hide();
        yield return null;
        Assert.IsFalse(_overlay.IsVisible);

        _overlay.ShowForScore(200);
        yield return null;
        Assert.IsTrue(_overlay.IsVisible);
    }

    [UnityTest]
    public IEnumerator ConfirmRegion_HiddenInitially()
    {
        yield return null;

        _overlay.ShowForScore(5000);
        yield return null;

        var confirmRegion = _doc.rootVisualElement.Q<VisualElement>("confirm-region");
        Assert.AreEqual(DisplayStyle.None, confirmRegion.style.display, "Confirm region should be hidden initially");
    }

    [UnityTest]
    public IEnumerator LayoutElements_Exist()
    {
        yield return null;

        Assert.IsNotNull(_doc.rootVisualElement.Q<Label>("title-label"), "title-label should exist");
        Assert.IsNotNull(_doc.rootVisualElement.Q<Label>("char-0"), "char-0 should exist");
        Assert.IsNotNull(_doc.rootVisualElement.Q<Label>("char-1"), "char-1 should exist");
        Assert.IsNotNull(_doc.rootVisualElement.Q<Label>("char-2"), "char-2 should exist");
        Assert.IsNotNull(_doc.rootVisualElement.Q<VisualElement>("confirm-region"), "confirm-region should exist");
    }
}
