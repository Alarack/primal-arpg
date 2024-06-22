using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum CursorMode
{
    Standard,
    Pulse,
    Pride
}

public enum CursorEdgeThickness
{
    Small,
    Medium,
    Large
}

public class CustomCursor : MonoBehaviour
{
    /// <summary>
    /// Standard Mode Cursor only has a single outline stroke.
    /// Pulse Mode Cursor fades between two selectable colors.
    /// Pride Mode Cursor fades between a rainbow spectrum list of colors.
    /// </summary>

    [Header("Cursor Sprites")]
    public SpriteRenderer cursorSprite;
    public SpriteRenderer cursorEdgeSmall;
    public SpriteRenderer cursorEdgeMedium;
    public SpriteRenderer cursorEdgeLarge;

    [Header("Cursor Settings")]
    //The speed at which the color scrolling occurs. Bounded Range should be .2f to 3f
    public float colorScrollSpeed = .5f;
    //Enables the cursors pulse or color changing to only do so while the cursor is in motion. Assuming I can even get this to work.
    public bool motionOnlyScroll;
    //The currently selected cursor mode. Defaults to standard.
    public CursorMode currentCursorMode = CursorMode.Standard;
    //The currently selected cursor thickness. Defaults to small.
    public CursorEdgeThickness currentEdgeThickness = CursorEdgeThickness.Small;

    [Header("Color Settings")]
    public float edgeOpacityValue = 1;
    public Color cursorColor = Color.white;
    public Color edgeColorOne = Color.white;
    public Color edgeColorTwo = Color.blue;
    public List<Color> prideColorList = new List<Color>();


    private Color endColor;
    private Task colorLerpCoroutine;


    private void Start()
    {
        endColor = edgeColorTwo;

        if (Cursor.visible == true)
        {
            Cursor.visible = false;
        }
    }

    private void Update()
    {

        if (IsColorLerpRunning() == false && currentCursorMode != CursorMode.Standard)
        {
            colorLerpCoroutine = new Task(PulseOrPride());
        }
        
        //If something causes the mode to change this needs to be here to halt the coroutine from starting again.
        if (IsColorLerpRunning() == true && currentCursorMode == CursorMode.Standard)
        {
            UpdateDefaultEdgeColor();
        }


#if UNITY_EDITOR

        if (Cursor.visible == true)
        {
            Cursor.visible = false;
        }
#endif

        FollowCursor();
    }

    private void FollowCursor()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePosition;
    }

    private bool IsColorLerpRunning()
    {
        if (colorLerpCoroutine == null)
        {
            return false;
        }

        return colorLerpCoroutine.Running;
    }

    public void CursorModeControl()
    {
        switch (currentCursorMode)
        {
            case CursorMode.Standard:
                break;
            case CursorMode.Pulse:
                StartCoroutine(PulseOrPride());
                break;
            case CursorMode.Pride:
                StartCoroutine(PulseOrPride());
                break;
        }
    }

    //Coroutine that checks which cursor mode is enabled and which edge to target with the effect.
    private IEnumerator PulseOrPride()
    {
        SpriteRenderer targetEdge = GetEdge();

        if (currentCursorMode == CursorMode.Pulse && IsColorLerpRunning() == true)
        {

            while (targetEdge.color != endColor)
            {
                targetEdge.color = targetEdge.color.MoveTowards(endColor, colorScrollSpeed);
                yield return new WaitForEndOfFrame();
            }

            SwapPulseColor();

        }

        if (currentCursorMode == CursorMode.Pride && IsColorLerpRunning() == true)
        {

            Color targetColor = GetTargetPrideColorListColor(targetEdge.color);
            endColor = targetColor;

            while (targetEdge.color != endColor)
            {
                targetEdge.color = targetEdge.color.MoveTowards(endColor, colorScrollSpeed);
                yield return new WaitForEndOfFrame();
            }
        }

        colorLerpCoroutine = null;
        //consider colorLerpCoroutine.Stop();

    }

    private Color GetTargetPrideColorListColor(Color currentEdgeColor)
    {

        Color selectedColor;

        if (prideColorList.Contains(currentEdgeColor))
        {
            int currentColorIndex = prideColorList.IndexOf(currentEdgeColor);
            int nextColorIndex = currentColorIndex + 1;
            if (nextColorIndex >= prideColorList.Count)
            {
                nextColorIndex = 0;
            }

            selectedColor = prideColorList[nextColorIndex];
        }
        else
        {
            selectedColor = prideColorList[0];
        }

        return selectedColor;
    }

    private void SwapPulseColor()
    {
        if (endColor == edgeColorTwo && currentCursorMode == CursorMode.Pulse)
        {
            endColor = edgeColorOne;
        }
        else if (endColor == edgeColorOne && currentCursorMode == CursorMode.Pulse)
        {
            endColor = edgeColorTwo;
        }
    }

    private void UpdateDefaultEdgeColor()
    {
        SpriteRenderer targetEdge = GetEdge();
        targetEdge.color = edgeColorOne;
    }

    private SpriteRenderer GetEdge()
    {

        SpriteRenderer edge = currentEdgeThickness switch
        {
            CursorEdgeThickness.Small => cursorEdgeSmall,
            CursorEdgeThickness.Medium => cursorEdgeMedium,
            CursorEdgeThickness.Large => cursorEdgeLarge,
            _ => null,
        };

        return edge;
    }

    #region CURSOR MODE UTILITIES

    //Updates the cursor mode to be the selected mode in settings.
    public void ChangeCursorMode(CursorMode targetMode)
    {
        currentCursorMode = targetMode;

        if (currentCursorMode == CursorMode.Pulse)
        {
            endColor = edgeColorTwo;
        }
    }


    //Updates the cursor edge thickness to be the selected thickness in settings.
    public void ChangeCursorThickness(CursorEdgeThickness targetThickness)
    {
        currentEdgeThickness = targetThickness;

        switch (currentEdgeThickness)
        {
            case CursorEdgeThickness.Small:
                cursorEdgeSmall.gameObject.SetActive(true);
                cursorEdgeMedium.gameObject.SetActive(false);
                cursorEdgeLarge.gameObject.SetActive(false);
                break;
            case CursorEdgeThickness.Medium:
                cursorEdgeSmall.gameObject.SetActive(false);
                cursorEdgeMedium.gameObject.SetActive(true);
                cursorEdgeLarge.gameObject.SetActive(false);
                break;
            case CursorEdgeThickness.Large:
                cursorEdgeSmall.gameObject.SetActive(false);
                cursorEdgeMedium.gameObject.SetActive(false);
                cursorEdgeLarge.gameObject.SetActive(true);
                break;
        }

        if (currentCursorMode == CursorMode.Standard)
        {
            UpdateDefaultEdgeColor();
        }
    }

    //Updates the cursor edge opacity to be the selected opacity in settings.
    public void ChangeCursorEdgeOpacity(float cursorEdgeOpacity)
    {
        edgeOpacityValue = cursorEdgeOpacity;
        //float targetOpacityValue = (1 / 255) * edgeOpacityValue; this is here just in case I need to convert this later for some reason.
        Color targetColor = edgeColorOne;
        targetColor.a = edgeOpacityValue;
        cursorEdgeSmall.color = targetColor;
        cursorEdgeMedium.color = targetColor;
        cursorEdgeLarge.color = targetColor;

    }

    //Updates the color of the cursor to be the selected color in settings.
    public void ChangeCursorColor(Color targetColor)
    {
        cursorSprite.color = targetColor;
    }

    //Changes the default base edge color around the cursor
    public void ChangeEdgeColorOne(Color targetColor)
    {
        edgeColorOne = targetColor;

        if (currentCursorMode == CursorMode.Standard)
        {
            UpdateDefaultEdgeColor();
        }
    }

    //Changes the secondary color for the edge around the cursor. This only works if Pulse mode is enabled.
    public void ChangeEdgeColorTwo(Color targetColor)
    {
        edgeColorTwo = targetColor;
        endColor = edgeColorTwo;
    }


    //Updates the scroll speed of the cursor's edge if its Pulse or Pride mode to the provided value.
    public void ChangeCursorScrollSpeed(float targetValue)
    {
        colorScrollSpeed = targetValue;
    }



    #endregion

}
