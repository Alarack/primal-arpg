using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorSwatchHelper : MonoBehaviour, IPointerClickHandler
{

    public enum ColorSwatchDesignation
    {
        EdgeOne,
        EdgeTwo
    }

    public ColorSwatchDesignation colorSwatchDesignation;

    private OptionsPanel optionsPanel;

    public Image swatchImage;

    private void Awake()
    {
        optionsPanel = GetComponentInParent<OptionsPanel>();
    }

    public void OnColorSelected(Color targetColor)
    {
        swatchImage.color = targetColor;
    }


    #region UI Callbacks

    public void OnPointerClick(PointerEventData eventData)
    {
        optionsPanel.OnEdgeSwatchClicked(this);
    }

    #endregion

}
