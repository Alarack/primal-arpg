using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions {


    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts) {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }

    /// <summary>
    /// Adds an object to a list, but only if it was not already in that list. Returns True if successful.
    /// </summary>
    public static bool AddUnique<T>(this IList<T> list, T entry) {
        if (list.Contains(entry) == false) {
            list.Add(entry);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes an object from a list, but only if it was contained in that list. Returns True if successful.
    /// </summary>
    public static bool RemoveIfContains<T>(this IList<T> list, T entry) {
        if (list.Contains(entry) == true) {
            list.Remove(entry);
            return true;
        }

        return false;
    }

    public static void ClearList<T>(this List<T> list) where T : MonoBehaviour {
        int count = list.Count;
        for (int i = 0; i < count; i++) {
            GameObject.Destroy(list[i].gameObject);
        }
        list.Clear();
    }

    public static void PopulateList<T>(this List<T> list, int count, T item, Transform holder, bool enableItem) where T : MonoBehaviour {
        list.ClearList();

        for (int i = 0; i < count; i++) {
            T newEntry = GameObject.Instantiate(item, holder) as T;
            newEntry.gameObject.SetActive(enableItem);
            list.Add(newEntry);
        }

    }

    public static bool CaseInsensitiveContains(this string text, string value,
    StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase) {
        return text.IndexOf(value, stringComparison) >= 0;
    }

    public static T ConstainsConstraint<T>(this List<AbilityConstraint> list) where T : AbilityConstraint {
        int count = list.Count;
        for (int i = 0; i < count; i++) {
            if (list[i] is T)
                return list[i] as T;
        }


        return null;
    }
}
