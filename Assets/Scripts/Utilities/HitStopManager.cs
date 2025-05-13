using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStopManager : Singleton<HitStopManager>
{

#pragma warning disable UDR0001 // Domain Reload Analyzer
    private static bool waiting;
#pragma warning restore UDR0001 // Domain Reload Analyzer


    public static void Stop(float duration = 0.1f) {
        if (waiting == true)
            return;
        
        Time.timeScale = 0f;
        new Task(Wait(duration));
    }

    private static IEnumerator Wait(float duration) {
        waiting = true;
        yield return new WaitForSecondsRealtime(duration);

        PausePanel pausePanel = PanelManager.GetPanel<PausePanel>();

        if (pausePanel.IsOpen) {
            waiting = false;
            yield break;
        }
        
        
        waiting = false;
        Time.timeScale = 1f;
        //float savedSpeed = PlayerPrefs.GetFloat("GameSpeed");

        //if (savedSpeed > 0f) {
        //    Time.timeScale = savedSpeed;
        //}
        //else {
        //    Time.timeScale = 1f;
        //}

    }

}
