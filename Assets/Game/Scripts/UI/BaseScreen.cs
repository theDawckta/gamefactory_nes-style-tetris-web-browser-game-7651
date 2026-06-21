using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public abstract class BaseScreen : MonoBehaviour
{
    protected UIDocument Document { get; private set; }
    protected VisualElement Root { get; private set; }
    public bool IsVisible { get; private set; }

    protected virtual void Awake()
    {
        Document = GetComponent<UIDocument>();
        Root = Document.rootVisualElement;
        Hide();
    }

    public virtual void Show()
    {
        Root.style.display = DisplayStyle.Flex;
        IsVisible = true;
    }

    public virtual void Hide()
    {
        Root.style.display = DisplayStyle.None;
        IsVisible = false;
    }
}
