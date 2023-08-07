using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextDisplayPanel : BasePanel
{


    public TextMeshProUGUI textDisplay;


    private Task closeDelay;

    public void Setup(string text, float duration = 0f) {
        textDisplay.text = text;

        if(duration > 0f) {
            closeDelay = new Task(CloseAfterDelay(duration));
        }
    }

    public override void Close() {
        base.Close();

        if(closeDelay != null && closeDelay.Running == true) {
            closeDelay.Stop();
        }

        closeDelay = null;
    }



    private IEnumerator CloseAfterDelay(float delay) {
        WaitForSeconds waiter = new WaitForSeconds(delay);
        yield return waiter;

        Close();
    }
}
