using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Text;
using TMPro;


public class UnspentSkillPointIndicator : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    private Image image;
    public TextMeshProUGUI skillPointsText;


    private void Awake() {
        image = GetComponent<Image>();
    }

    private void Start() {
        Material runtimeMat = new Material(image.material);
        image.material = runtimeMat;
        PulseGlow();
    }

    public void UpdateSkillPoints(float skillPoints) {
        skillPointsText.text = skillPoints.ToString();
    }

    private void OnEnable() {
        float skillPoints = EntityManager.ActivePlayer.Stats[StatName.SkillPoint];
        skillPointsText.text = skillPoints.ToString();

    }


    private void PulseGlow() {
        image.material.DOFloat(1.2f, "_HsvBright", 1f).SetLoops(-1, LoopType.Yoyo);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        float skillPoints = EntityManager.ActivePlayer.Stats[StatName.SkillPoint];



        StringBuilder builder = new StringBuilder();
        builder.AppendLine("You have " + skillPoints + " unspent Skill Points");
        builder.AppendLine();
        builder.AppendLine("Right Click any Skill to spend Skill Points.");
      
        TooltipManager.Show(builder.ToString(), "Unspent Skill Points");


    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }
}
