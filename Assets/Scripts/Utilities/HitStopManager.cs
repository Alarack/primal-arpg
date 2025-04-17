using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStopManager : Singleton<HitStopManager>
{

    private static bool waiting;

    public static void Stop(float duration = 0.1f) {
        if (waiting == true)
            return;
        
        Time.timeScale = 0f;
        new Task(Wait(duration));
    }

    private static IEnumerator Wait(float duration) {
        waiting = true;
        yield return new WaitForSecondsRealtime(duration);
        waiting = false;
        float savedSpeed = PlayerPrefs.GetFloat("GameSpeed");

        if (savedSpeed > 0f) {
            Time.timeScale = savedSpeed;
        }
        else {
            Time.timeScale = 1f;
        }

    }

}
