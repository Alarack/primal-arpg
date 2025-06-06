using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStarterPackageEntry : MonoBehaviour
{

    [Header("Template")]
    public StarterItemDisplayEntry template;
    public Transform holder;

    [Header("Fader")]
    public CanvasGroup fader;
    public GameObject shimmer;

    private List<StarterItemDisplayEntry> itemEntries = new List<StarterItemDisplayEntry>();
    private CharacterSelectPanel characterSelectPanel;

    private Vector2 shimmerStartPos;

    [SerializeField]
    private Vector2 shimmerEndPos;

    private void Awake() {
        template.gameObject.SetActive(false);
        shimmerStartPos = shimmer.transform.localPosition;
    }

    public void Setup(CharacterSelectPanel selectionPanel, params ItemDefinition[] items) {
        characterSelectPanel = selectionPanel;

        itemEntries.PopulateList(items.Length, template, holder, true);
        for (int i = 0; i < items.Length; i++) {
            itemEntries[i].Setup(items[i]);
        }

    }


    public void OnSelectClicked() {
        for (int i = 0;i < itemEntries.Count;i++) {
            ItemSpawner.SpawnItem(itemEntries[i].ItemDef, transform.position, true);
        }

        characterSelectPanel.StartGame();
    }

    public void ShowShimmer() {
        shimmer.transform.localPosition = shimmerStartPos;
        shimmer.transform.DOLocalMove(shimmerEndPos, 0.8f).SetEase(Ease.OutSine);
    }


}
