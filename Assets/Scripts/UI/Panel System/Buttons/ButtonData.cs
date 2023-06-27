using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class ButtonData 
{

    public enum ButtonDataType {
        Interactable,
        Informational
    }

    public string buttonText;
    public Action callback;
    public Color buttonTextColor = Color.white;
    public ButtonDataType buttonDataType;

    public ButtonData(string buttonText, Action callback, Color? buttonTextColor = null, ButtonDataType buttonDataType = ButtonDataType.Interactable) {
        this.buttonText = buttonText;
        this.callback = callback;
        this.buttonDataType = buttonDataType;

        this.buttonTextColor = buttonTextColor ?? Color.white;
    }

}
