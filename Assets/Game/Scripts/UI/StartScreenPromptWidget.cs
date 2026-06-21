using UnityEngine;
using UnityEngine.UIElements;

public class StartScreenPromptWidget : MonoBehaviour
{
    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var promptRegion = root.Q("prompt-region");
        if (promptRegion != null)
            PopulateRegion(promptRegion);
    }

    public void PopulateRegion(VisualElement promptRegion)
    {
        promptRegion.style.alignItems = Align.Center;
        promptRegion.style.justifyContent = Justify.Center;

        var label = new Label("PRESS ENTER TO START");
        promptRegion.Add(label);
    }
}
