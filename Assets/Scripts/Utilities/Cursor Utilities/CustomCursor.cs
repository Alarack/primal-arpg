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
    //The speed at which the color scrolling occurs. Shouldn't allow negative values. Probably needs to be a range.
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

    public float lerpTime = 1;

    public float lerpTimeModifier;
    private bool isCoroutineActive = false;
    private int currentColorIndex = 0;
    private int targetColorIndex = 1;
    private float targetPoint;
    private Color startColor;
    private Color endColor;
    //private bool lerpInProgress = false;


    private float lerpColorFloat = 0f;

    private void Start()
    {
        startColor = edgeColorOne;
        endColor = edgeColorTwo;
    }

    private void Update()
    {
        targetPoint += Time.deltaTime / lerpTimeModifier;

        Debug.Log("isCoroutineActive is set to: " + isCoroutineActive);

        //cursorEdgeMedium.color = Color.Lerp(startColor, endColor, Mathf.PingPong(Time.time, 1));

        //Mathf.Lerp

        //lerpedColor = Color.Lerp(startColor, endColor, testColorFloat);
        //lerpedColor = Color.Lerp(startColor, endColor, Mathf.PingPong(Time.time, 1));
        //cursorEdgeMedium.color = lerpedColor;
        //Debug.Log("testColorFloat is: " + testColorFloat);

        //do I just need to call the respective 

        //This is called a Extension method, which allows you to call it from the Color struct directly.


        if (isCoroutineActive == false && currentCursorMode != CursorMode.Standard)
        {
            Debug.Log("Starting coroutine and setting isCoroutineActive to True");
            isCoroutineActive = true;
            StartCoroutine(PulseOrPride());
        }

        //If something causes the mode to change this needs to be here to halt the coroutine from starting again.
        if (isCoroutineActive == true && currentCursorMode == CursorMode.Standard)
        {
            Debug.Log("Cursor mode was changed to Standard, setting coroutine activity to false.");
            isCoroutineActive = false;
        }
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
    public IEnumerator PulseOrPride()
    {




        //I might need to use a timer to get this lerp to smooth out over a specific duration unless I can delay a while loop on a fixed interval

        while (lerpColorFloat <= 1f)
        {
            

            if (currentCursorMode == CursorMode.Pulse && isCoroutineActive == true)
            {

                switch (currentEdgeThickness)
                {
                    case CursorEdgeThickness.Small:
                        PulseLerp(cursorEdgeSmall, targetPoint);
                        break;
                    case CursorEdgeThickness.Medium:
                        Debug.Log("Starting Pulse Lerp");
                        PulseLerp(cursorEdgeMedium, targetPoint);
                        break;
                    case CursorEdgeThickness.Large:
                        PulseLerp(cursorEdgeLarge, targetPoint);
                        break;
                }

                yield return new WaitForSeconds(colorScrollSpeed);
            }

            if (currentCursorMode == CursorMode.Pride && isCoroutineActive == true)
            {

                targetPoint += Time.deltaTime;

                switch (currentEdgeThickness)
                {
                    case CursorEdgeThickness.Small:
                        RainbowEdgeLerp(cursorEdgeSmall, targetPoint);
                        break;
                    case CursorEdgeThickness.Medium:
                        RainbowEdgeLerp(cursorEdgeMedium, targetPoint);
                        break;
                    case CursorEdgeThickness.Large:
                        RainbowEdgeLerp(cursorEdgeLarge, targetPoint);
                        break;
                }

                yield return new WaitForSeconds(colorScrollSpeed);
            }



        }

        if (lerpColorFloat >= 1)
        {
            Debug.Log("Resetting lerpColorFloat value to 0 and isCoroutineActive to false");
            lerpColorFloat = 0;
            isCoroutineActive = false;
        }


        //yield return new WaitForSeconds(3);

        //yield return new WaitForEndOfFrame();



        //if (currentCursorMode != CursorMode.Standard)
        //{
        //    StartCoroutine(PulseOrPride());
        //}
    }

    //Switches the cursor edge color back and forth between the two specified colors.
    public void PulseLerp(SpriteRenderer targetEdge, float targetPoint)
    {
        //float floatFadeValue = 0;

        //if (alternatePulseColor == false)
        //{
        //    startColor = edgeColorOne;
        //    endColor = edgeColorTwo;
        //    alternatePulseColor = true;
        //}
        //else
        //{
        //    startColor = edgeColorTwo;
        //    endColor = edgeColorOne;
        //    alternatePulseColor = false;
        //}

        lerpColorFloat += 0.01f;

        targetEdge.color = Color.Lerp(startColor, endColor, lerpColorFloat);
        Debug.Log("color is: " + targetEdge.color);

        if (endColor == edgeColorTwo && currentCursorMode == CursorMode.Pulse)
        {
            startColor = edgeColorTwo;
            endColor = edgeColorOne;
        }
        else if (endColor == edgeColorOne && currentCursorMode == CursorMode.Pulse)
        {
            startColor = edgeColorOne;
            endColor = edgeColorTwo;
        }

        Debug.Log("lerpColorFloat is equal to: " + lerpColorFloat);

        //if (targetPoint >= 1f)
        //{
        //    targetPoint = 0f;
        //}

        //startColor = edgeColorOne;
        //endColor = edgeColorTwo;

        //targetEdge.color = Color.Lerp(startColor, endColor, Mathf.PingPong(Time.time, 1));


        //I think T in the the color.Lerp method is actually the amount of gradiation between the two?
        //This needs a for loop wrapped around a check to see if the target point fractionally isn't 1 yet, and if it is one it continues to the end point


    }


    //Iterates through the Pride Color list and changes the cursor edge color.
    public void RainbowEdgeLerp(SpriteRenderer targetEdge, float lerpTime)
    {

        //lerp returns a single color, so I need to lerp based on a split number of seconds instead of based on the number of colors in the list?

        for (int i = 0; i < prideColorList.Count; i++)
        {
            targetEdge.color = Color.Lerp(prideColorList[currentColorIndex], prideColorList[targetColorIndex], lerpTime);

            if (targetPoint >= 1f)
            {
                targetPoint = 0f;

                currentColorIndex = targetColorIndex;
                targetColorIndex++;

                if (targetColorIndex == prideColorList.Count)
                {
                    targetColorIndex = 0;
                }
            }
        }

        //float lerpProgress = 0f;

        //while (lerpProgress <= 1)
        //{
        //    Color.Lerp(startColor, endColor, (lerpProgress + .01f * colorScrollSpeed));

        //    lerpProgress += .01f;

        //    //Expose the incremental variable for lerp progress 

        //    if (lerpProgress >= 1)
        //    {
        //        lerpProgress = 0;
        //        //Swap start and end colors 

        //    }
        //}



    }


    #region CURSOR MODE UTILITIES

    //Updates the cursor mode to be the selected mode in settings.
    public void ChangeCursorMode(CursorMode targetMode)
    {
        currentCursorMode = targetMode;
    }


    //Updates the cursor edge thickness to be the selected thickness in settings.
    public void ChangeCursorThickness(CursorEdgeThickness targetThickness)
    {
        currentEdgeThickness = targetThickness;

        switch (currentEdgeThickness)
        {
            case CursorEdgeThickness.Small:
                cursorEdgeSmall.enabled = true;
                cursorEdgeMedium.enabled = false;
                cursorEdgeLarge.enabled = false;
                break;
            case CursorEdgeThickness.Medium:
                cursorEdgeSmall.enabled = false;
                cursorEdgeMedium.enabled = true;
                cursorEdgeLarge.enabled = false;
                break;
            case CursorEdgeThickness.Large:
                cursorEdgeSmall.enabled = false;
                cursorEdgeMedium.enabled = false;
                cursorEdgeLarge.enabled = true;
                break;
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

    #endregion

}
