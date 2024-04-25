using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillPreviewEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {



    public Image skillIcon;

    private AbilityDefinition abilityDefinition;
    private Ability displayAbility;


    public void Setup(AbilityDefinition abilityDefinition) {
        this.abilityDefinition = abilityDefinition;

        new Task(WaitForPlayerSpawn());
    }

    private IEnumerator WaitForPlayerSpawn() {
        while(EntityManager.ActivePlayer == null) {
            yield return new WaitForEndOfFrame();
        }

        displayAbility = AbilityFactory.CreateAbility(abilityDefinition.AbilityData, EntityManager.ActivePlayer);
        SetupDisplay();
    }

    private void SetupDisplay() {
        skillIcon.sprite = abilityDefinition.AbilityData.abilityIcon;
    }



    #region UI CALLBACKS


    public void OnPointerEnter(PointerEventData eventData) {
        TooltipManager.Show(displayAbility.GetTooltip(), displayAbility.Data.abilityName);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }

    #endregion

}
