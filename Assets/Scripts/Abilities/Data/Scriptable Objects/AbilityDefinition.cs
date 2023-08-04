using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability Def", menuName = "Abilities/Ability Def")]
public class AbilityDefinition : ScriptableObject
{

    public AbilityData AbilityData;

    private Ability displayAbility;

    public Ability FetchAbilityForDisplay(Entity owner) {
        
        if(displayAbility == null)
            displayAbility = AbilityFactory.CreateAbility(AbilityData, owner);

        return displayAbility;
    }

}
