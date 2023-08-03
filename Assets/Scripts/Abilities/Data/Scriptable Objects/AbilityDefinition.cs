using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability Def", menuName = "Abilities/Ability Def")]
public class AbilityDefinition : ScriptableObject
{

    public AbilityData AbilityData;



    public Ability FetchAbilityForDisplay(Entity owner) {
        Ability ability = AbilityFactory.CreateAbility(AbilityData, owner);

        return ability;
    }

}
