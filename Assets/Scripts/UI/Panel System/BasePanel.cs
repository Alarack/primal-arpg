using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;


public enum BasePanelState
{
    Open,
    Closed
}

public class BasePanel : MonoBehaviour
{
    public bool IsOpen { get; private set; }
    public bool CloseOnEscape { get; private set; }

    public string PanelID { get; protected set; }

    public float fadeTime = 0.25f;

    public BasePanelState defaultState;
    private CanvasGroup panelFader;

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
    private Action onCloseCallback;

    protected virtual void Awake()
    {
        GetView();
        panelFader = GetComponent<CanvasGroup>();
    }

    protected virtual void Start()
    {
        switch (defaultState)
        {
            case BasePanelState.Open:
                break;
            case BasePanelState.Closed:
                //Close();
                break;
        }
    }

    protected virtual void Update() {
        if(CloseOnEscape == true && Input.GetKey(KeyCode.Escape)) {
            Close();
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

    public void Initialize(string id, bool closeOnEscape = false)
    {
        PanelID = id;
        this.CloseOnEscape = closeOnEscape;

        if (view.activeInHierarchy == true)
            IsOpen = true;

        //IsOpen = defaultState == BasePanelState.Open;
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

            if(panelFader != null) {
                FadeIn();
            }

            view.SetActive(true);
            transform.SetAsLastSibling();
            IsOpen = true;

            OnOpen();
        }
    }

    protected virtual void FadeIn() {
        panelFader.alpha = 0f;
        Tween fadeIn = panelFader.DOFade(1f, fadeTime);
        fadeIn.onComplete += OnFadeInComplete;
    }

    protected virtual void OnFadeInComplete() {

    }

    public void SetOnCloseCallback(Action oncloseCallback) {
        this.onCloseCallback = oncloseCallback;
    }

    public virtual void Close()
    {
        if (view != null)
        {

            if(panelFader != null) {
                FadeOut();
            }
            else {
                view.SetActive(false);
            }

            //Debug.LogWarning("Closing: " + GetType());

            // Only send the close events if we actually closed
            if (IsOpen)
            {
                IsOpen = false;
                OnClose();
                onCloseCallback?.Invoke();
            }
            else
            {
                IsOpen = false;
            }
        }
    }

    protected virtual void FadeOut() {
        Tween fadeOut = panelFader.DOFade(0f, fadeTime);
        fadeOut.onComplete += OnFadeOutComplete;
    }

    protected virtual void OnFadeOutComplete() {
        view.SetActive(false);
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

