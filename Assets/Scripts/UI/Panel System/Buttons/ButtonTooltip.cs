using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class ButtonTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public string header;
    public string body;
    
    
    public void OnPointerEnter(PointerEventData eventData) {
        if(string.IsNullOrEmpty(header) == false)
            TooltipManager.Show(body, header);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }
}
