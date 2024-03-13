using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LL.Events;

public class Timer
{
    public string ID { get; private set; }
    public float Ratio { get { return timeElapsed / Duration; } }
    public float Duration { get; private set; }
    public bool Repeating { get { return resetTimerOnComplete; } }

    private float timeElapsed;
    private bool resetTimerOnComplete;
    private Action<EventData> onCompleteCallback;

    private EventData callbackData;

    public Timer(float duration, Action<EventData> onCompleteCallback, bool resetTimerOnComplete = true, EventData callbackData = null)
    {
        if(duration <= 0)
        {
            Debug.LogError("A timer was given a duration of 0 or less. This timer will not work.");
        }

        Duration = duration;
        this.resetTimerOnComplete = resetTimerOnComplete;
        this.onCompleteCallback = onCompleteCallback;
        this.callbackData = callbackData;
    }

    public void UpdateClock()
    {
        if(timeElapsed < Duration)
        {
            timeElapsed += Time.deltaTime;

            if(timeElapsed >= Duration)
            {
                if (onCompleteCallback != null)
                    onCompleteCallback(callbackData);

                //Debug.Log("Time Elapsed. Duration: " + Duration);

                if(resetTimerOnComplete == true)
                {
                    ResetTimer();
                }
            }
        }
    }

    public void ResetTimer()
    {
        timeElapsed = 0f;
    }

    public void ModifyDuration(float mod)
    {
        Duration += mod;

        if (Duration <= 0f)
        {
            Duration = 0f;
        }

        if (timeElapsed > Duration)
        {
            timeElapsed = 0f;
        }
    }

    public void SetDuration(float duration) {
        Duration = duration;

        if (Duration <= 0f) {
            Duration = 0f;
        }

        if (timeElapsed > Duration) {
            timeElapsed = 0f;
        }

        //Debug.Log("Setting Duration to: " + duration);
    }


}
