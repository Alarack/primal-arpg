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

    private GameObject view;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    private Vector2 tempOffset;

    private void Awake() {
        view = transform.Find("View").gameObject;
        canvas = GetComponent<Canvas>();

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
        Vector2 newPos = (Vector2)Input.mousePosition + offset + tempOffset;
        //newPos.z = 0f;
        float rightEdgeToScreenEdgeDistance = Screen.width - (newPos.x + rectTransform.rect.width * canvas.scaleFactor / 2) - padding;
        if (rightEdgeToScreenEdgeDistance < 0) {
            newPos.x += rightEdgeToScreenEdgeDistance;
        }
        float leftEdgeToScreenEdgeDistance = 0 - (newPos.x - rectTransform.rect.width * canvas.scaleFactor / 2) + padding;
        if (leftEdgeToScreenEdgeDistance > 0) {
            newPos.x += leftEdgeToScreenEdgeDistance;
        }
        float topEdgeToScreenEdgeDistance = Screen.height - (newPos.y + rectTransform.rect.height * 2 * canvas.scaleFactor /2) - padding;
        if (topEdgeToScreenEdgeDistance < 0) {
            newPos.y += topEdgeToScreenEdgeDistance;
        }

        float bottomEdgeToScreenEdgeDistance = 0 - (newPos.y - rectTransform.rect.height * canvas.scaleFactor /2) + padding;
        if (bottomEdgeToScreenEdgeDistance > 0) {
            newPos.y += bottomEdgeToScreenEdgeDistance;
        }

        view.transform.position = newPos;
    }
}
