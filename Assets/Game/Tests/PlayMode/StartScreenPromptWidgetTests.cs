using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

public class StartScreenPromptWidgetTests
{
    private GameObject _go;
    private StartScreenPromptWidget _widget;
    private PanelSettings _panelSettings;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _go.SetActive(false);
        _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var doc = _go.AddComponent<UIDocument>();
        doc.panelSettings = _panelSettings;
        _widget = _go.AddComponent<StartScreenPromptWidget>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_go);
        Object.Destroy(_panelSettings);
    }

    [UnityTest]
    public IEnumerator Awake_WithNoPromptRegion_DoesNotThrow()
    {
        _go.SetActive(true);
        yield return null;
        Assert.IsNotNull(_widget);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_AddsLabelToRegion()
    {
        _go.SetActive(true);
        yield return null;
        var promptRegion = new VisualElement { name = "prompt-region" };
        _widget.PopulateRegion(promptRegion);
        Assert.AreEqual(1, promptRegion.childCount);
        Assert.IsInstanceOf<Label>(promptRegion[0]);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_LabelHasCorrectText()
    {
        _go.SetActive(true);
        yield return null;
        var promptRegion = new VisualElement { name = "prompt-region" };
        _widget.PopulateRegion(promptRegion);
        var label = (Label)promptRegion[0];
        Assert.AreEqual("PRESS ENTER TO START", label.text);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_AppliesCenteringStylesToRegion()
    {
        _go.SetActive(true);
        yield return null;
        var promptRegion = new VisualElement { name = "prompt-region" };
        _widget.PopulateRegion(promptRegion);
        Assert.AreEqual(Align.Center, promptRegion.style.alignItems.value);
        Assert.AreEqual(Justify.Center, promptRegion.style.justifyContent.value);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_CalledTwice_AddsTwoLabels()
    {
        _go.SetActive(true);
        yield return null;
        var promptRegion = new VisualElement { name = "prompt-region" };
        _widget.PopulateRegion(promptRegion);
        _widget.PopulateRegion(promptRegion);
        Assert.AreEqual(2, promptRegion.childCount);
    }
}
