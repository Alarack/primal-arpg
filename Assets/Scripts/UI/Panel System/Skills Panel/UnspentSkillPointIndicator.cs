using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UnspentSkillPointIndicator : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    private Image image;


    private void Awake() {
        image = GetComponent<Image>();
    }

    private void Start() {
        PulseGlow();
    }


    private void PulseGlow() {
        image.material.DOFloat(1.2f, "_HsvBright", 1f).SetLoops(-1, LoopType.Yoyo);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        float skillPoints = EntityManager.ActivePlayer.Stats[StatName.SkillPoint];

        TooltipManager.Show("You have " + skillPoints + " unspent Skill Points", "Unspent Skill Points");
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }
}
