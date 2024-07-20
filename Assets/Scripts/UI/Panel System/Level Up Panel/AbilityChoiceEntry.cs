using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class AbilityChoiceEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    public Image skillIcon;
    public TextMeshProUGUI abilityNameText;
    public GameObject shimmer;
    public CanvasGroup fader;
    
    public Ability AbilityChoice { get; private set; }


    private LevelUpPanel levelUpPanel;

    public void Setup(Ability ability, LevelUpPanel levelUpPanel) {
        this.AbilityChoice = ability;
        this.levelUpPanel = levelUpPanel;

        abilityNameText.text = ability.Data.abilityName;
        skillIcon.sprite = ability.Data.abilityIcon;
        fader.alpha = 0f;
        shimmer.transform.DOLocalMove(new Vector2(-134f, -126f), 1f);
        fader.DOFade(1f, 0.3f);
    }

    private void OnDisable() {
        DOTween.Kill(shimmer.transform);
    }



    #region UI CALLBACKS

    public void OnPointerClick(PointerEventData eventData) {
        levelUpPanel.OnAbilitySelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        TooltipManager.Show(AbilityChoice.GetTooltip(), AbilityChoice.Data.abilityName);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }

    #endregion

}
