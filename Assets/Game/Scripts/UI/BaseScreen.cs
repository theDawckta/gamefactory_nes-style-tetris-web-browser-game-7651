using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Base MonoBehaviour for all game screens that use UIDocument for rendering.
/// Provides Show/Hide lifecycle and IsVisible tracking.
/// </summary>
public class BaseScreen : MonoBehaviour
{
    private bool _isVisible;

    /// <summary>
    /// Whether this screen is currently visible.
    /// </summary>
    public bool IsVisible
    {
        get { return _isVisible; }
    }

    /// <summary>
    /// Shows this screen. Sets the GameObject active and makes the root VisualElement visible.
    /// Override OnShow() for subclass-specific show logic.
    /// </summary>
    public void Show()
    {
        _isVisible = true;
        gameObject.SetActive(true);

        var doc = GetComponent<UIDocument>();
        if (doc != null && doc.rootVisualElement != null)
        {
            doc.rootVisualElement.style.display = DisplayStyle.Flex;
        }

        OnShow();
    }

    /// <summary>
    /// Hides this screen. Makes the root VisualElement invisible and deactivates the GameObject.
    /// Override OnHide() for subclass-specific hide logic.
    /// </summary>
    public void Hide()
    {
        OnHide();

        var doc = GetComponent<UIDocument>();
        if (doc != null && doc.rootVisualElement != null)
        {
            doc.rootVisualElement.style.display = DisplayStyle.None;
        }

        gameObject.SetActive(false);
        _isVisible = false;
    }

    /// <summary>
    /// Called by Show() after the screen is made visible. Override for custom show logic.
    /// </summary>
    protected virtual void OnShow()
    {
    }

    /// <summary>
    /// Called by Hide() before the screen is made invisible. Override for custom hide logic.
    /// </summary>
    protected virtual void OnHide()
    {
    }
}
