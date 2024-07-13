using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorChartHelper : MonoBehaviour, IPointerClickHandler
{

    public Image chartImage;
    private OptionsPanel optionsPanel;

    public void Awake()
    {
        optionsPanel = GetComponentInParent<OptionsPanel>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Color outputColor = Pick(chartImage, eventData);
        optionsPanel.OnColorChosen(outputColor);
    }


    //https://github.com/mmaletin/UnityColorPicker
    //investigate different color picker

    private Color Pick(Image imageToPick, PointerEventData eventData)
    {
        Texture2D textureToSample = imageToPick.sprite.texture;

        Vector2 localCursor;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            imageToPick.rectTransform, eventData.position, eventData.pressEventCamera, out localCursor);

        // Convert from local position to pixel position
        Vector2 pixelPosition = new Vector2(
            (int)((localCursor.x + imageToPick.rectTransform.rect.width / 2) * textureToSample.width / imageToPick.rectTransform.rect.width),
            (int)((localCursor.y + imageToPick.rectTransform.rect.height / 2) * textureToSample.height / imageToPick.rectTransform.rect.height));

        // Get the color of the pixel at the computed position
        Color color = textureToSample.GetPixel((int)pixelPosition.x, (int)pixelPosition.y);

        return color;
    }

}
