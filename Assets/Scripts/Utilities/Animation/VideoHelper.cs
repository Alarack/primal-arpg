using DG.Tweening;
using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoHelper : MonoBehaviour
{

    public VideoPlayer player;
    public VideoClip[] clips;

    public CanvasGroup transitionFader;
    public float transitionTime;

    private Timer transitionTimer;
    private Task transitionTask;

    private int clipIndex = 0;

    public MainMenuPanel mainMenuPanel;


    private void Awake() {
        transitionTimer = new Timer(transitionTime, StartVideoTransition, true);
        clipIndex = Random.Range(0,clips.Length);

        player.clip = clips[clipIndex];
    }

    private void Update() {
        if (transitionTimer != null && mainMenuPanel.IsOpen == true) {
            transitionTimer.UpdateClock();
        }
    }

    private void StartVideoTransition(EventData data) {
        transitionTask = new Task(Transition());
    }

    private  IEnumerator Transition() {
        WaitForSeconds waiter = new WaitForSeconds(1.1f);

        transitionFader.DOFade(1f, 1f).onComplete+= SwapVideo;

        yield return waiter;

        transitionFader.DOFade(0f, 1f);

    }


    private void SwapVideo() {
        clipIndex++;

        if(clipIndex >= clips.Length) 
            clipIndex = 0;

        player.clip = clips[clipIndex];

    }
}
