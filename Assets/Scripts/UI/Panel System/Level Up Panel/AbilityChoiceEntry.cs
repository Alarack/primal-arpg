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
    public TextMeshProUGUI abilityDescriptionText;
    public GameObject shimmer;
    public CanvasGroup fader;

    public Image borderImage;
    public Sprite activeBorderSprite;
    public Sprite passiveBordersprite;

    public Mask passiveMask;

    [Header("VFX")]
    public ParticleSystem selectionEffect;
    public CanvasGroup flashFader;

    public Ability AbilityChoice { get; private set; }


    private LevelUpPanel levelUpPanel;

    public void Setup(Ability ability, LevelUpPanel levelUpPanel) {
        this.AbilityChoice = ability;
        this.levelUpPanel = levelUpPanel;

        abilityNameText.text = ability.Data.abilityName;
        abilityDescriptionText.text = AbilityChoice.GetTooltip();
        skillIcon.sprite = ability.Data.abilityIcon;
        fader.alpha = 0f;
        shimmer.transform.DOLocalMove(new Vector2(-134f, -126f), 1f);
        fader.DOFade(1f, 0.3f);
        RotateAnimation();

        if(ability.Data.category == AbilityCategory.PassiveSkill) {
            passiveMask.enabled = true;
            borderImage.sprite = passiveBordersprite;
        }
        else {
            passiveMask.enabled = false;
            borderImage.sprite = activeBorderSprite;
        }
    }

    public void RotateAnimation(float duration = 0.3f) {
        transform.localEulerAngles = new Vector3(0f, -90f, 0f);
        transform.DOLocalRotate(Vector3.zero, duration).SetEase(Ease.OutBounce);
    }

    private void OnDisable() {
        DOTween.Kill(shimmer.transform);
    }


    public void ShowSelectionEffect() {
        selectionEffect.Play();
        flashFader.DOFade(0.95f, 0.25f);
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
