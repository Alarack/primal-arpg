using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public enum BasePanelState
{
    Open,
    Closed
}

public class BasePanel : MonoBehaviour
{
    public bool IsOpen { get; private set; }

    public string PanelID { get; protected set; }

    public BasePanelState defaultState;

    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
    protected virtual void OnOpen()
    {
        OnOpenEvent?.Invoke();
    }
    protected virtual void OnClose()
    {
        OnClosedEvent?.Invoke();
    }

    public UnityEvent OnOpenEvent;
    public UnityEvent OnClosedEvent;

    protected GameObject view;

    protected virtual void Awake()
    {
        GetView();
    }

    protected virtual void Start()
    {
        switch (defaultState)
        {
            case BasePanelState.Open:
                break;
            case BasePanelState.Closed:
                Close();
                break;
        }
    }

    private void GetView()
    {
        try
        {
            view = transform.Find("View").gameObject;
        }
        catch
        {
#if UNITY_EDITOR
            Debug.LogError("[" + GetType().ToString() + "] Could not find View GameObject. " +
               "Ensure all Panel Content is inside an empty child called View. :: " + " " + gameObject.name);
            ParentDebugHelper();
#endif
            return;
        }

        if (view == null)
        {
#if UNITY_EDITOR
            Debug.LogError("[" + GetType().ToString() + "] Could not find View GameObject. " +
                "Ensure all Panel Content is inside an empty child called View. :: " + " " + gameObject.name);
            ParentDebugHelper();
#endif
            return;
        }
    }

    public void Initialize(string id)
    {
        PanelID = id;

        if (view.activeInHierarchy == true)
            IsOpen = true;
    }

    public virtual void Refresh()
    {

    }

    protected virtual void ResetVerticalLayout(RectTransform transform) {
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
    }

    private void ParentDebugHelper()
    {
        Transform[] parents = GetComponentsInParent<Transform>();

        int count = parents.Length;
        for (int i = 0; i < count; i++)
        {
            Debug.Log(parents[i].name);
        }
    }

    public virtual void Open()
    {
        if (view != null)
        {
            view.SetActive(true);
            transform.SetAsLastSibling();
            IsOpen = true;

            OnOpen();
        }
    }

    public virtual void Close()
    {
        if (view != null)
        {
            view.SetActive(false);
            //Debug.LogWarning("Closing: " + GetType());

            // Only send the close events if we actually closed
            if (IsOpen)
            {
                IsOpen = false;
                OnClose();
            }
            else
            {
                IsOpen = false;
            }
        }
    }

    public virtual void Hide()
    {
        if (view != null)
        {
            view.SetActive(false);

            IsOpen = false;
        }
    }

    public virtual void Show()
    {
        if (view != null)
        {
            view.SetActive(true);

            IsOpen = true;
        }
    }

    public virtual void Toggle() {
        if(IsOpen == false) {
            Open();
        }
        else {
            Close();
        }
    }

}

