using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

public class InitialsEntryOverlayTests
{
    private GameObject _go;
    private InitialsEntryOverlay _overlay;
    private PanelSettings _panelSettings;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _go.SetActive(false);
        _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var doc = _go.AddComponent<UIDocument>();
        doc.panelSettings = _panelSettings;
        _overlay = _go.AddComponent<InitialsEntryOverlay>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_go);
        Object.Destroy(_panelSettings);
    }

    [UnityTest]
    public IEnumerator Awake_DoesNotThrow_WithNoUXML()
    {
        Assert.DoesNotThrow(() => _go.SetActive(true));
        yield return null;
    }

    [UnityTest]
    public IEnumerator Awake_StartsHidden()
    {
        _go.SetActive(true);
        yield return null;
        Assert.IsFalse(_overlay.IsVisible);
    }

    [UnityTest]
    public IEnumerator ShowForScore_SetsIsVisibleTrue()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(0);
        Assert.IsTrue(_overlay.IsVisible);
    }

    [UnityTest]
    public IEnumerator ShowForScore_ResetsCharsToAAA()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(500);
        Assert.AreEqual('A', _overlay.GetCurrentChar(0));
        Assert.AreEqual('A', _overlay.GetCurrentChar(1));
        Assert.AreEqual('A', _overlay.GetCurrentChar(2));
    }

    [UnityTest]
    public IEnumerator ShowForScore_SetsCursorToSlot0()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(100);
        Assert.AreEqual(0, _overlay.ActiveSlot);
    }

    [UnityTest]
    public IEnumerator ShowForScore_AfterPreviousSession_ResetsChars()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(0);
        _overlay.CycleActiveChar(5);
        _overlay.ShowForScore(0);
        Assert.AreEqual('A', _overlay.GetCurrentChar(0));
        Assert.AreEqual(0, _overlay.ActiveSlot);
    }

    [UnityTest]
    public IEnumerator CycleActiveChar_Right_AdvancesFromAToB()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(0);
        _overlay.CycleActiveChar(1);
        Assert.AreEqual('B', _overlay.GetCurrentChar(0));
    }

    [UnityTest]
    public IEnumerator CycleActiveChar_Right_WrapsZTo0()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(0);
        for (int i = 0; i < 25; i++) _overlay.CycleActiveChar(1);
        Assert.AreEqual('Z', _overlay.GetCurrentChar(0));
        _overlay.CycleActiveChar(1);
        Assert.AreEqual('0', _overlay.GetCurrentChar(0));
    }

    [UnityTest]
    public IEnumerator CycleActiveChar_Right_Wraps9ToA()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(0);
        for (int i = 0; i < 35; i++) _overlay.CycleActiveChar(1);
        Assert.AreEqual('9', _overlay.GetCurrentChar(0));
        _overlay.CycleActiveChar(1);
        Assert.AreEqual('A', _overlay.GetCurrentChar(0));
    }

    [UnityTest]
    public IEnumerator CycleActiveChar_Left_CyclesBackwardFromA()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(0);
        _overlay.CycleActiveChar(-1);
        Assert.AreEqual('9', _overlay.GetCurrentChar(0));
    }

    [UnityTest]
    public IEnumerator CycleActiveChar_OnConfirmSlot_IsNoOp()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(0);
        _overlay.AdvanceSlot();
        _overlay.AdvanceSlot();
        _overlay.AdvanceSlot();
        Assert.AreEqual(3, _overlay.ActiveSlot);
        _overlay.CycleActiveChar(1);
        Assert.AreEqual('A', _overlay.GetCurrentChar(2));
    }

    [UnityTest]
    public IEnumerator AdvanceSlot_OnSlot0_MovesCursorToSlot1()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(0);
        _overlay.AdvanceSlot();
        Assert.AreEqual(1, _overlay.ActiveSlot);
    }

    [UnityTest]
    public IEnumerator AdvanceSlot_OnSlot1_MovesCursorToSlot2()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(0);
        _overlay.AdvanceSlot();
        _overlay.AdvanceSlot();
        Assert.AreEqual(2, _overlay.ActiveSlot);
    }

    [UnityTest]
    public IEnumerator AdvanceSlot_OnSlot2_MovesToConfirmSlot()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(0);
        _overlay.AdvanceSlot();
        _overlay.AdvanceSlot();
        _overlay.AdvanceSlot();
        Assert.AreEqual(3, _overlay.ActiveSlot);
    }

    [UnityTest]
    public IEnumerator AdvanceSlot_OnConfirm_FiresOnInitialsSubmitted()
    {
        _go.SetActive(true);
        yield return null;
        string receivedInitials = null;
        int receivedScore = -1;
        _overlay.OnInitialsSubmitted += (initials, score) => { receivedInitials = initials; receivedScore = score; };
        _overlay.ShowForScore(1234);
        _overlay.AdvanceSlot();
        _overlay.AdvanceSlot();
        _overlay.AdvanceSlot();
        _overlay.AdvanceSlot();
        Assert.AreEqual("AAA", receivedInitials);
        Assert.AreEqual(1234, receivedScore);
    }

    [UnityTest]
    public IEnumerator AdvanceSlot_OnConfirm_HidesOverlay()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(0);
        _overlay.AdvanceSlot();
        _overlay.AdvanceSlot();
        _overlay.AdvanceSlot();
        _overlay.AdvanceSlot();
        Assert.IsFalse(_overlay.IsVisible);
    }

    [UnityTest]
    public IEnumerator AdvanceSlot_OnConfirm_SubmitsCorrectInitials()
    {
        _go.SetActive(true);
        yield return null;
        string received = null;
        _overlay.OnInitialsSubmitted += (initials, _) => received = initials;
        _overlay.ShowForScore(0);
        _overlay.CycleActiveChar(1);
        _overlay.AdvanceSlot();
        _overlay.CycleActiveChar(2);
        _overlay.AdvanceSlot();
        _overlay.CycleActiveChar(3);
        _overlay.AdvanceSlot();
        _overlay.AdvanceSlot();
        Assert.AreEqual("BCD", received);
    }

    [UnityTest]
    public IEnumerator RetreatSlot_OnSlot1_MovesBackToSlot0()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(0);
        _overlay.AdvanceSlot();
        _overlay.RetreatSlot();
        Assert.AreEqual(0, _overlay.ActiveSlot);
    }

    [UnityTest]
    public IEnumerator RetreatSlot_OnSlot0_IsNoOp()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(0);
        _overlay.RetreatSlot();
        Assert.AreEqual(0, _overlay.ActiveSlot);
    }

    [UnityTest]
    public IEnumerator Hide_SetsIsVisibleFalse()
    {
        _go.SetActive(true);
        yield return null;
        _overlay.ShowForScore(0);
        _overlay.Hide();
        Assert.IsFalse(_overlay.IsVisible);
    }
}
