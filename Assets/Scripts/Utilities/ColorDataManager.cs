using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorDataManager : Singleton<ColorDataManager>
{


    public ColorData[] colors;

    public Color this[string name] { get { return GetColorByName(name); } }

    private void Awake() {
        colors = Resources.LoadAll<ColorData>("");
    }


    public static Color GetColorByName(string name) {
        for (int i = 0; i < Instance.colors.Length; i++) {
            if (Instance.colors[i].name == name)
                return Instance.colors[i].color;
        }

        return Color.white;
    }


}
