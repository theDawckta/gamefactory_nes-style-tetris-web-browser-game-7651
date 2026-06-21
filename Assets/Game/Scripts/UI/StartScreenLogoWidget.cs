using UnityEngine;
using UnityEngine.UIElements;

public class StartScreenLogoWidget : MonoBehaviour
{
    public Sprite TitleLogoSprite;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var logoRegion = root.Q("logo-region");
        if (logoRegion != null)
            PopulateRegion(logoRegion);
    }

    public void PopulateRegion(VisualElement logoRegion)
    {
        logoRegion.style.alignItems = Align.Center;
        logoRegion.style.justifyContent = Justify.Center;

        if (TitleLogoSprite == null)
            return;

        var image = new Image();
        image.sprite = TitleLogoSprite;
        image.style.width = TitleLogoSprite.rect.width;
        image.style.height = TitleLogoSprite.rect.height;
        logoRegion.Add(image);
    }
}
