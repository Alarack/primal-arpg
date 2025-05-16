using UnityEngine;
using TMPro;
using DG.Tweening;

public class PossibleAffixDisplay : MonoBehaviour
{

    public TextMeshProUGUI affixText;

    private StatName stat;
    private CanvasGroup fader;


    private void Awake() {
        fader = GetComponent<CanvasGroup>();
        fader.alpha = 0f;
    }


    public void Setup(StatName stat) {
        this.stat = stat;
        SetupDisplay();
        fader.DOFade(1f, 0.1f);
    }

    private void SetupDisplay() {
        affixText.text = TextHelper.PretifyStatName(stat);
    }

}
