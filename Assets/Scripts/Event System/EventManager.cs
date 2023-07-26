using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;


namespace LL.Events {

    public static class EventManager {

        private static readonly Dictionary<GameEvent, List<EventListener>> _listeners = new Dictionary<GameEvent, List<EventListener>>();

        public static void SendEvent(GameEvent type, EventData data) {

            if (_listeners.TryGetValue(type, out List<EventListener> listenerList)) {
                List<EventListener> eventListeners = new List<EventListener>(listenerList);

                int count = eventListeners.Count;
                for (int i = 0; i < count; i++) {
#if UNITY_EDITOR
                    MonoBehaviour behaviour = eventListeners[i].callback.Target as MonoBehaviour;
                    if (behaviour == null && behaviour as object != null) {
                        Debug.LogError("[EVENT MANAGER] A listener for event " 
                            + type 
                            + " has a null MonoBehaviour (" + eventListeners[i].listenerClassName + ") instance" 
                            + " (but which is still in memory); did you destroy the script without unregistering its event listeners?");
                        continue;
                    }
#endif

#if UNITY_EDITOR
                    try {
#endif
                        eventListeners[i].callback.Invoke(data);
#if UNITY_EDITOR
                    }
                    catch (Exception exception) { 
                        Debug.LogError("[EVENT MANAGER] An event " + type + " listener for " 
                            + eventListeners[i].listenerClassName 
                            + " -- " + eventListeners[i].target + " threw an exception during its callback Invoke()"); 
                        Debug.LogException(exception is TargetInvocationException ? exception.InnerException : exception); }
#endif
                }
            }
        }

        public static void RegisterListener(GameEvent type, Action<EventData> listener, object target = null) {
#if UNITY_EDITOR
            // Check for duplicates, same event type for same method in same class insatnce
            if (_listeners.ContainsKey(type)) {
                foreach (EventListener checkListener in _listeners[type]) {
                    if (checkListener.target == listener.Target && checkListener.callback == listener) {
                        Debug.LogError("[EVENT MANAGER] We were asked to register the same listener for event " 
                            + type + " to " + listener + " in " + target + " " 
                            + "; the event will be invoked more than once");
                        break;
                    }
                }
            }
#endif
            if (_listeners.TryGetValue(type, out List<EventListener> eventListener)) {
                eventListener.Add(new EventListener(listener, listener.Target.GetType().Name, target));
            }
            else {
                _listeners[type] = new List<EventListener>(){
                new EventListener(listener, listener.Target.GetType().Name, target)
                };
            }
        }

        public static void RemoveListener(GameEvent type, Action<EventData> listener) {
            if (_listeners.ContainsKey(type)) {
                int count = _listeners[type].Count;
                for (int i = 0; i < count; i++) {
                    if (_listeners[type][i].callback.Target == listener.Target) {
                        _listeners[type].RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public static void RemoveMyListeners(object currentListener) {
            foreach (KeyValuePair<GameEvent, List<EventListener>> listeners in _listeners) {
                for (int i = 0; i < listeners.Value.Count; i++) {
                    if (listeners.Value[i].target == currentListener || listeners.Value[i].callback.Target == currentListener) {
                        listeners.Value.RemoveAt(i);
                        i--;
                    }
                }
            }
        }






        public class EventListener {
            public readonly Action<EventData> callback;
            public readonly string listenerClassName;
            ///// <summary> Optionally specified actual target class that recives the callback in case we created it anonymously </summary>
            public readonly object target;

            public EventListener(Action<EventData> callback, string listenerClassName, object target) {
                this.callback = callback;
                this.listenerClassName = listenerClassName;
                this.target = target;
            }
        }

    }


    public enum GameEvent {
        UnitDied,
        UnitStatAdjusted,
        UnitTargeted,
        UnitDetected,
        UnitCollision,
        EffectApplied,
        AbilityResolved,
        UserActivatedAbility,
        TriggerCounterActivated,
        TimerFinished,
        StatusApplied,
        StatusRemoved,
        UnitForgotten,
        WeaponCooldownFinished,
        WeaponCooldownStarted,
        TriggerTimerCompleted,
        StateEntered,
        StateExited,
        ItemAquired,
        ItemEquipped,
        ItemUnequipped,
        ItemDropped,
        AbilityEquipped,
        AbilityUnequipped,
        AbilityLearned,
        AbilityUnlearned,
        AbilityStatAdjusted,
        RuneEquipped,
        RuneUnequipped,
        DashStarted,
        ProjectilePierced,
        ProjectileChained,
        ProjectileSplit,
        UnitDiedWithStatus,
        OverloadTriggered

    }



}
