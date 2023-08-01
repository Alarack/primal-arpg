using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LL.Events;
using System;

public class TimerManager : Singleton<TimerManager> {

    private static List<Timer> activeTimers = new List<Timer>();

    private static List<Action> timerActions = new List<Action>();

    private static List<Action> removeList = new List<Action>();

    private void Update() {
        UpdateTimers();

    }

    private void UpdateTimers() {
        for (int i = activeTimers.Count - 1; i >= 0; i--) {

            Timer currentTimer = activeTimers[i];
            currentTimer.UpdateClock();

            if (currentTimer.Repeating == false & currentTimer.Ratio >= 1f) {
                activeTimers.RemoveAt(i);
                continue;
            }
        }

        for (int i = timerActions.Count - 1; i >= 0; i--) {

            if (removeList.Contains(timerActions[i])) {
                removeList.Remove(timerActions[i]);
                timerActions.RemoveAt(i);
                continue;
            }

            if (timerActions[i] == null) {
                timerActions.RemoveAt(i);
                continue;
            }

            timerActions[i]?.Invoke();

        }

    }

    public static void AddTimerAction(Action action) {
        timerActions.Add(action);
    }

    public static void RemoveTimerAction(Action action) {
        removeList.Add(action);
        
        //timerActions.Remove(action);
    }

    public static void AddTimer(AbilityTrigger trigger, float duration, bool repeat) {
        EventData timerEventData = new EventData();
        timerEventData.AddTrigger("Trigger", trigger);
        timerEventData.AddEntity("Owner", trigger.SourceEntity);

        Timer timer = new Timer(duration, OnTimerComplete, repeat, timerEventData);
        activeTimers.Add(timer);
    }



    private static void OnTimerComplete(EventData timerEventData) {

        EventManager.SendEvent(GameEvent.TimerFinished, timerEventData);
    }




}
