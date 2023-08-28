using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ResourceGlobeDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public float smoothSpeed = 10f;
    public Image globeFillImage;
    public TextMeshProUGUI resourceStatText;

    public StatRange AssosiatedStat { get; private set; }


    private Task smoothFill;

    private float desiredRatio;

    public void Setup(StatRange stat) {
        AssosiatedStat = stat;
        AssosiatedStat.onValueChanged += OnStatValueChanged;

        UpdateStatText();
    }

    private void OnDisable() {
        AssosiatedStat.onValueChanged -= OnStatValueChanged;
    }


    private void OnStatValueChanged(BaseStat stat, object source, float value) {

        desiredRatio = AssosiatedStat.Ratio;

        if(globeFillImage.fillAmount != desiredRatio /*&& smoothFill.Running == false*/) {
            smoothFill = new Task(SmoothlyAdjustFill());
        }

        //globeFillImage.fillAmount = AssosiatedStat.Ratio;

        UpdateStatText();
    }

    private IEnumerator SmoothlyAdjustFill() {

        //Debug.Log("Starting Smoother");

        while (globeFillImage.fillAmount != desiredRatio) {
            float target = Mathf.MoveTowards(globeFillImage.fillAmount, desiredRatio, Time.deltaTime * smoothSpeed);
            globeFillImage.fillAmount = target;
            //Debug.Log("Smoothing");
            yield return new WaitForEndOfFrame();
        }

    }

    private void UpdateStatText() {
        if (resourceStatText != null) {
            resourceStatText.text = AssosiatedStat.ModifiedValue + "/" + AssosiatedStat.MaxValueStat.ModifiedValue;
        }
    }



    public void OnPointerEnter(PointerEventData eventData) {
        if (resourceStatText != null) {
            resourceStatText.gameObject.SetActive(true);
            //resourceStatText.text = AssosiatedStat.ModifiedValue + " / " + AssosiatedStat.MaxValueStat.ModifiedValue;
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (resourceStatText != null) {
            resourceStatText.gameObject.SetActive(false);
            //resourceStatText.text = AssosiatedStat.ModifiedValue + " / " + AssosiatedStat.MaxValueStat.ModifiedValue;
        }
    }



}
