using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System.Security.Cryptography;

public class AbilityChoiceEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    public Image skillIcon;
    public TextMeshProUGUI abilityNameText;
    public TextMeshProUGUI abilityDescriptionText;
    public GameObject shimmer;
    public CanvasGroup fader;

    public Image borderImage;
    public Image bgImage;
    public Image outerFrameImage;
    public CanvasGroup outerFramefader;
    public Sprite activeBorderSprite;
    public Sprite passiveBordersprite;

    public Sprite[] bgImageVariants;

    public Mask passiveMask;

    [Header("VFX")]
    public ParticleSystem selectionEffect;
    public CanvasGroup flashFader;

    [Header("Tilt")]
    public float manualTiltAmount = 5f;
    public float autoTiltAmount = 2.5f;
    public float tiltSpeed = 30f;

    public Ability AbilityChoice { get; private set; }


    private LevelUpPanel levelUpPanel;
    private bool mouseHovering;

    private Color baseOuterFrameColor;
    public Color highlightedOuterFrameColor;
    //private bool startTilt;

    public void Setup(Ability ability, LevelUpPanel levelUpPanel) {
        this.AbilityChoice = ability;
        this.levelUpPanel = levelUpPanel;

        baseOuterFrameColor = outerFrameImage.color;
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
        int bgIndex = Random.Range(0, bgImageVariants.Length);
        bgImage.sprite = bgImageVariants[bgIndex];
    }

    private void Update() {

        //if (startTilt == true)
        //    TiltRotation();
    }

    public void RotateAnimation(float duration = 0.3f) {
        transform.localEulerAngles = new Vector3(0f, -90f, 0f);
        transform.DOLocalRotate(Vector3.zero, duration).SetEase(Ease.OutBounce)/*.onComplete += StartTilt*/;
    }

    //private void StartTilt() {
    //    startTilt = true;
    //}

    private void TiltRotation() {
        float sine = Mathf.Sin(Time.time + transform.parent.GetSiblingIndex()) * (mouseHovering ? .2f : 1);
        float cosine = Mathf.Cos(Time.time + transform.parent.GetSiblingIndex()) * (mouseHovering ? .2f : 1);

        Vector3 offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        float tiltX = mouseHovering ? ((offset.y * -1) * manualTiltAmount) : 0f;
        float tiltY = mouseHovering ? ((offset.x) * manualTiltAmount) : 0f;
        float tiltZ = mouseHovering ? transform.eulerAngles.z : 0f; //: (curveRotationOffset * (curve.rotationInfluence * transform.parent.childCount - 1));

        float lerpX = Mathf.LerpAngle(transform.eulerAngles.x, tiltX + (sine * autoTiltAmount), tiltSpeed * Time.deltaTime);
        float lerpY = Mathf.LerpAngle(transform.eulerAngles.y, tiltY + (cosine * autoTiltAmount), tiltSpeed * Time.deltaTime);
        float lerpZ = Mathf.LerpAngle(transform.eulerAngles.z, tiltZ, tiltSpeed / 2 * Time.deltaTime);

        transform.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);

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
        //TooltipManager.Show(AbilityChoice.GetTooltip(), AbilityChoice.Data.abilityName);
        mouseHovering = true;
        outerFramefader.DOFade(0.9f, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData) {
        //TooltipManager.Hide();
        mouseHovering = false;
        outerFramefader.DOFade(0.6f, 0.2f);
    }

    #endregion

}
