using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StatusManager : Singleton<StatusManager> {

    //Deprecated



    //private static Dictionary<Entity, StatusEntry> statusDictionary = new Dictionary<Entity, StatusEntry>();


    //private void Update() {
    //    //foreach (var entry in statusDictionary.Values.ToList()) {
    //    //    entry.ManagedUpdate();
    //    //}

    //    UpdateStatuses();
    //}

    //private void UpdateStatuses() {
    //    for (int i = 0; i < statusDictionary.Keys.Count; i++) {
    //        statusDictionary.ElementAt(i).Value.ManagedUpdate();
    //    }
    //}

    //public static void AddStatus(Entity target, Status status) {
    //    if (statusDictionary.TryGetValue(target, out StatusEntry entry) == true) {
    //        entry.AddStatus(status);
    //    }
    //    else {
    //        statusDictionary.Add(target, new StatusEntry(target, status));
    //    }
    //}

    //public static void RemoveStatus(Entity target, Status status) {
    //    if (statusDictionary.TryGetValue(target, out StatusEntry entry) == true) {
    //        entry.RemoveStatus(status);

    //        if (entry.activeStatuses.Count == 0)
    //            statusDictionary.Remove(target);
    //    }
    //}

    //public static bool HasStatus(Entity target, Status.StatusName statusName) {
    //    if (statusDictionary.TryGetValue(target, out StatusEntry entry) == true) {
    //        return entry.HasStatus(statusName);
    //    }

    //    return false;
    //}


    //public class StatusEntry {
    //    public Entity target;
    //    public Dictionary<Status.StatusName, List<Status>> activeStatuses = new Dictionary<Status.StatusName, List<Status>>();


    //    public StatusEntry(Entity target, Status initalStatus) {
    //        this.target = target;
    //        activeStatuses.Add(initalStatus.statusName, new List<Status> { initalStatus });
    //        initalStatus.FirstApply();
    //        //Debug.Log("adding a new status");
    //    }

    //    public bool HasStatus(Status.StatusName statusName) {
    //        return activeStatuses.ContainsKey(statusName);
    //    }

    //    public void AddStatus(Status status) {

    //        //Check if the new status is of a type already on this target.
    //        if (activeStatuses.TryGetValue(status.statusName, out List<Status> existingStatus) == true) {

    //            //Check if the new status has the same source as a status already on this tagret.
    //            Status preexisting = GetStatusFromSameSource(status, existingStatus);
    //            if (preexisting != null) {
    //                StackStatus(preexisting);
    //                return;
    //            }

    //            //Apply the new status
    //            activeStatuses[status.statusName].Add(status);
    //            status.FirstApply();
    //        }
    //        else {
    //            //apply the new status
    //            activeStatuses.Add(status.statusName, new List<Status> { status });
    //            status.FirstApply();
    //        }
    //    }


    //    private Status GetStatusFromSameSource(Status newStatus, List<Status> existingStatuses) {
    //        for (int i = 0; i < existingStatuses.Count; i++) {
    //            if (existingStatuses[i].Source == newStatus.Source)
    //                return existingStatuses[i];
    //        }

    //        return null;
    //    }

    //    private void StackStatus(Status existstingStatus) {
    //        switch (existstingStatus.stackMethod) {
    //            case Status.StackMethod.None:
    //                existstingStatus.RefreshDuration();
    //                break;
    //            case Status.StackMethod.LimitedStacks:
    //                if (existstingStatus.IsStackCapped == true) {
    //                    existstingStatus.RefreshDuration();
    //                }
    //                else {
    //                    existstingStatus.Stack();
    //                }

    //                break;
    //            case Status.StackMethod.Infinite:
    //                existstingStatus.Stack();
    //                break;
    //        }
    //    }

    //    public void RemoveStatus(Status status) {
    //        //Check if we have a status of the incomming type
    //        if (activeStatuses.TryGetValue(status.statusName, out List<Status> existingStatus) == true) {
    //            if (existingStatus.Contains(status) == true)
    //                existingStatus.Remove(status);

    //            if (existingStatus.Count == 0)
    //                activeStatuses.Remove(status.statusName);

    //            //Debug.Log("removeing a status");
    //        }
    //    }

    //    public void ManagedUpdate() {
    //        //foreach (var status in activeStatuses.Values.ToList()) {
    //        //    for (int i = 0; i < status.Count; i++) {
    //        //        status[i].ManagedUpdate();
    //        //    }
    //        //}

    //        UpdateStatuses();
    //    }


    //    public void UpdateStatuses() {
    //        for (int i = 0; i < activeStatuses.Keys.Count; i++) {

    //            List<Status> currentList = activeStatuses.ElementAt(i).Value;
    //            for (int j = 0; j < currentList.Count; j++) {
    //                currentList[j].ManagedUpdate();
    //            }
    //        }
    //    }

    //}
}
