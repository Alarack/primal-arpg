using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using System.Linq;

public static class PanelManager
{
    static Dictionary<string, BasePanel> currentPanels = new Dictionary<string, BasePanel>();

    public static BasePanel LastOpenedPanel { get; private set; }

    [RuntimeInitializeOnLoadMethod]
    private static void InitStatic() {
        currentPanels = new Dictionary<string, BasePanel>();
    }


    public static T OpenPanel<T>() where T : BasePanel
    {
        var panel = GetPanel<T>();

        if (panel == null)
        {
            Debug.LogError("[PANEL MANAGER] Panel not found in panel manager: " + typeof(T).ToString());
            return null;
        }

        LastOpenedPanel = panel;

        panel.Open();

        return panel;
    }

    public static void TogglePanel<T>() where T : BasePanel {
        var panel = GetPanel<T>();

        if (panel == null) {
            Debug.LogError("[PANEL MANAGER] Panel not found in panel manager: " + typeof(T).ToString());
            return;
        }

        panel.Toggle();

    }

    public static BasePanel OpenPanel(string panelID)
    {
        BasePanel panel = GetPanel(panelID);

        if (panel == null)
        {
            panel = CreatePanel(null, panelID);
        }

        if (panel == null)
        {
#if UNITY_EDITOR
            Debug.LogError("[PANEL MANAGER] Panel ID not found in panel manager: " + panelID);
#endif
            return null;
        }

        LastOpenedPanel = panel;

        panel.Open();

        return panel;
    }

    private static BasePanel CreatePanel(Transform canvasRoot, string panelId)
    {
        BasePanel targetPanel = PanelDataManager.Instance.panelMapData.GetPanelPrefab(panelId);

        if (targetPanel == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Panel with ID: " + panelId + " not found in panel map");
            return null;
#endif
        }

        if (canvasRoot == null)
            canvasRoot = PanelDataManager.Instance.canvasRoot;

        BasePanel activePanel = GameObject.Instantiate(targetPanel, canvasRoot) as BasePanel;
        activePanel.Initialize(panelId);
        currentPanels.Add(panelId, activePanel);

        return activePanel;
    }

    private static T CreatePanel<T>(Transform canvasRoot) where T : BasePanel
    {
        PanelMapData.PanelMapEntry targetPanel = PanelDataManager.Instance.panelMapData.GetPanelMapEntry<T>();

        if (targetPanel == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Panel with type: " + typeof(T).ToString() + " not found in panel map");
            return null;
#endif
        }

        if (canvasRoot == null)
            canvasRoot = PanelDataManager.Instance.canvasRoot;

        if(targetPanel.panelPrefab == null) {
            Debug.LogError("No panel prefab found for: " + targetPanel.panelID);
        }

        BasePanel activePanel = GameObject.Instantiate(targetPanel.panelPrefab, canvasRoot) as BasePanel;
        activePanel.Initialize(targetPanel.panelID, targetPanel.closeOnEscape);
        currentPanels.Add(targetPanel.panelID, activePanel);

        if (activePanel.defaultState == BasePanelState.Closed) {
            activePanel.Hide();
        }

        return activePanel as T;
    }

    public static void CloseAllPanels(BasePanel exceptThisPanel)
    {
        foreach (var panel in currentPanels)
        {
            if(panel.Value != null && panel.Value != exceptThisPanel)
                panel.Value.Close();
        }
    }

    public static void CloseAllPanels()
    {
        foreach(var panel in currentPanels)
        {
            if(panel.Value != null)
                panel.Value.Close();
        }
    }

    public static bool AreAnyPanelsOpen()
    {
        foreach (var panel in currentPanels)
        {
            if (panel.Value != null && panel.Value.IsOpen)
                return true;
        }

        return false;
    }

    public static bool IsEscapeClosingPanelOpen() {
        foreach (var panel in currentPanels) {
            if (panel.Value != null && panel.Value.IsOpen == true && panel.Value.CloseOnEscape == true) {

                //Debug.Log(panel.Key + " is open");
                
                return true;
            }
        }

        return false;
    }

    public static bool IsPanelOpen<T>() where T : BasePanel {
        BasePanel Panel = GetPanel<T>();

        return Panel.IsOpen;
    }


    /// <summary>
    /// Close any registered panel by panel type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void ClosePanel<T>() where T : BasePanel
    {
        BasePanel panel = GetPanel<T>();

        if (panel != null)
        {
            if (panel.IsOpen == true)
                panel.Close();
        }
    }

    public static BasePanel GetPanel(string panelID)
    {
        if (currentPanels.TryGetValue(panelID, out BasePanel targetPanel))
        {
            //Debug.Log(targetPanel.GetType().ToString() + " found ");
            return targetPanel;
        }

        return null;
    }

    public static T GetPanel<T>(bool createIfNotFound = true) where T : BasePanel
    {
        foreach (KeyValuePair<string, BasePanel> entry in currentPanels)
        {
            if (entry.Value.GetType() == typeof(T))
            {

                if(entry.Value == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("[PANEL MANAGER] Null Panel Found: " + typeof(T) + ". Recreating the panel");
#endif
                    currentPanels.Remove(entry.Key);
                    break;
                }

                return entry.Value as T;
            }
        }

        if(createIfNotFound == true)
        {
            var panel = CreatePanel<T>(null);
            if (panel == null)
            {
                return null;
            }

            return panel as T;
        }

        return null;
       
    }

    public static bool IsBlockingPanelOpen() {
        List<string> blockingIds = PanelDataManager.blockingPanels;

        for (int i = 0; i < blockingIds.Count; i++) {
            BasePanel panel = GetPanel(blockingIds[i]);

            if (panel == null) {
                continue;
            }

            if (panel.IsOpen == true) {
                return true;
            }
        }

        

        return false;

    }

    public static void ResetPanelManager()
    {
        currentPanels.Clear();
    }

}

