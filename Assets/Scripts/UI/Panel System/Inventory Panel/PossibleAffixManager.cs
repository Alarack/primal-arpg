using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using System.Linq;

public class PossibleAffixManager : MonoBehaviour
{

    [Header("Template")]
    public PossibleAffixDisplay template;
    public Transform holder;


    private List<PossibleAffixDisplay> entries = new List<PossibleAffixDisplay>();
    private CanvasGroup fader;


    private void Awake() {
        fader = GetComponent<CanvasGroup>();
        fader.alpha = 0f;
        template.gameObject.SetActive(false);
    }

    public void Show(List<StatName> stats) {

        List<StatName> relevantstats = new List<StatName>();

        for (int i = 0; i < stats.Count; i++) {
            if (ItemSpawner.Instance.IsStatRelevant(stats[i])) {
                relevantstats.Add(stats[i]);
            }
        }

        relevantstats = relevantstats.OrderBy(n => n.ToString()).ToList();

        fader.DOFade(1f, 0.2f);

        entries.PopulateList(relevantstats.Count, template, holder, true);

        new Task(FadeInStats(relevantstats));

    }

    public void Hide() {
        Tween fadeOut = fader.DOFade(0f, 0.2f);
        fadeOut.onComplete += CleanUp;
    }


    private void CleanUp() {
        entries.ClearList();
    }

    private IEnumerator FadeInStats(List<StatName> stats) {
        WaitForSeconds waiter = new WaitForSeconds(0.1f);

        for (int i = 0; i < stats.Count; i++) {
            entries[i].Setup(stats[i]);
            yield return waiter;
        }

    }
}
