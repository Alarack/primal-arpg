using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Panel Map Data")]
public class PanelMapData : ScriptableObject
{
    public List<PanelMapEntry> panelPrefabs;

    public BasePanel GetPanelPrefab(string id)
    {
        int count = panelPrefabs.Count;
        for (int i = 0; i < count; i++)
        {
            if (panelPrefabs[i].panelID == id)
                return panelPrefabs[i].panelPrefab;
        }

        return null;
    }

    public T GetPanelPrefab<T>() where T : BasePanel
    {
        int count = panelPrefabs.Count;
        for (int i = 0; i < count; i++)
        {
            if (panelPrefabs[i].panelPrefab is T)
                return panelPrefabs[i].panelPrefab as T;
        }

        return null;
    }

    public PanelMapEntry GetPanelMapEntry<T>() where T : BasePanel
    {
        int count = panelPrefabs.Count;
        for (int i = 0; i < count; i++)
        {
            if (panelPrefabs[i].panelPrefab is T)
                return panelPrefabs[i];
        }

        return null;
    }

    public PanelMapEntry GetPanelMapEntry(string id) {
        for (int i = 0;i < panelPrefabs.Count; i++) {
            if(panelPrefabs[i].panelID == id)
                return panelPrefabs[i];
        }

        return null;
    }

    public List<string> GetBlockingPanels() {

        List<string> ids = new List<string>();

        for (int i = 0; i < panelPrefabs.Count; i++) {
            if (panelPrefabs[i].preventAttacks == true) {
                ids.Add(panelPrefabs[i].panelID);
            }
        }

        return ids;
    }

    public List<string> GetEscapeClosingPanels() {
        List<string> ids = new List<string>();

        for (int i = 0; i < panelPrefabs.Count; i++) {
            if (panelPrefabs[i].closeOnEscape == true) {
                ids.Add(panelPrefabs[i].panelID);
            }
        }

        return ids;
    }

    [Serializable]
    public class PanelMapEntry
    {
        public string panelID
        {
            get
            {
                if (panelPrefab != null)
                    return panelPrefab.GetType().Name;

                Debug.LogError("Panel Map Data: There is a null prefab in Panel Map Data");
                return String.Empty;
            }
        }

        public BasePanel panelPrefab;
        public bool autoOpen;
        public bool preventAttacks;
        public bool closeOnEscape;
        public bool blockPause;
    }
}

