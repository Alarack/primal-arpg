using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    [Header("Text Fields")]
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI contentText;

    [Header("Options")]
    public int characterWrapLimit;
    public float padding;
    public Vector2 offset;

    //[Header("Feedbacks")]
    //public MMF_Player fadeInFeedback;
    //public MMF_Player fadeOutFeedback;

    private LayoutElement layoutElement;
    [SerializeField]
    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;

    private RectTransform view;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    private Vector2 tempOffset;

    private void Awake() {
        view = transform.Find("View").GetComponent<RectTransform>();
        canvas = GetComponent<Canvas>();
        canvasRectTransform = canvas.GetComponent<RectTransform>();

        //rectTransform = view.GetComponent<RectTransform>();
        layoutElement = GetComponentInChildren<LayoutElement>(true);


    }

    public void Show(float xOffset = 0f, float yOffset = 0f) {
        view.gameObject.SetActive(true);

        tempOffset = new Vector2(xOffset, yOffset);
        //fadeInFeedback.PlayFeedbacks();
    }

    public void Hide() {
        view.gameObject.SetActive(false);

        tempOffset = Vector2.zero;
        //fadeInFeedback.StopFeedbacks();
        //fadeOutFeedback.PlayFeedbacks();

    }

    private void AnchorBottom() {
        view.anchorMin = new Vector2(0.5f, 0);
        view.anchorMax = new Vector2(0.5f, 0);
        view.pivot = new Vector2(0.5f, 0);
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 0);
        rectTransform.pivot = new Vector2(0.5f, 0);
    }

    private void AnchorTop() {
        view.anchorMin = new Vector2(0.5f, 1f);
        view.anchorMax = new Vector2(0.5f, 1f);
        view.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
    }

    public void SetText(string content, string header = "") {

        if (string.IsNullOrEmpty(header) == true) {
            headerText.gameObject.SetActive(false);
        }
        else {
            headerText.gameObject.SetActive(true);
            headerText.text = header;
        }

        contentText.text = content;

        layoutElement.enabled = headerText.text.Length > characterWrapLimit || contentText.text.Length > characterWrapLimit;

    }

    private void Update() {


        SetTooltipPosition();
    }


    private void SetTooltipPosition() {

        if (view.gameObject.activeSelf == false)
            return;

        Vector2 newPos = (Vector2)Input.mousePosition + offset + tempOffset;

        //newPos.z = 0f;
        float rightEdgeToScreenEdgeDistance = Screen.width - (newPos.x + rectTransform.rect.width * canvas.scaleFactor / 2f) - padding;
        if (rightEdgeToScreenEdgeDistance < 0f) {
            newPos.x += rightEdgeToScreenEdgeDistance;
        }
        float leftEdgeToScreenEdgeDistance = 0f - (newPos.x - rectTransform.rect.width * canvas.scaleFactor / 2f) + padding;
        if (leftEdgeToScreenEdgeDistance > 0f) {
            newPos.x += leftEdgeToScreenEdgeDistance;
        }
        float topEdgeToScreenEdgeDistance = Screen.height - (newPos.y + rectTransform.rect.height * 2f * canvas.scaleFactor / 2f) - padding;
        if (topEdgeToScreenEdgeDistance < 0f) {
            newPos.y += topEdgeToScreenEdgeDistance;

            float topEdge = (newPos.y + rectTransform.rect.height * 2f * canvas.scaleFactor / 2f) - padding;
            float mouseY = Input.mousePosition.y;

            float difference = topEdge - Input.mousePosition.y;

            newPos.y -= difference + (offset.y * (canvas.scaleFactor * 2f));
        }

        float bottomEdgeToScreenEdgeDistance = 0f - (newPos.y - rectTransform.rect.height * canvas.scaleFactor /2f) + padding;
        if (bottomEdgeToScreenEdgeDistance > 0f) {
            //newPos.y += bottomEdgeToScreenEdgeDistance;
        }

        view.transform.position = newPos;
    }
}
