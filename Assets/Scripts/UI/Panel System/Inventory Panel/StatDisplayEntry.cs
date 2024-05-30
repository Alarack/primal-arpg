using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatDisplayEntry : MonoBehaviour
{
    public TextMeshProUGUI statText;


    public void Setup(string text) {
        statText.text = text; 
    }


}
