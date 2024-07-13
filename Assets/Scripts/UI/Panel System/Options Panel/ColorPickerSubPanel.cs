using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPickerSubPanel : BasePanel
{

    private ColorChartHelper helper;


    protected override void Awake()
    {
        base.Awake();

        helper = GetComponentInChildren<ColorChartHelper>();

    }




}
