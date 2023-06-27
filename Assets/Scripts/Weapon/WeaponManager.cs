using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WeaponManager : MonoBehaviour {


    public List<Weapon> weapons = new List<Weapon>();


    private Entity owner;


    private void Awake() {
        owner = GetComponent<Entity>();
        weapons = GetComponentsInChildren<Weapon>().ToList();

        SetupWeapons();

    }

    private void SetupWeapons() {

        for (int i = 0; i < weapons.Count; i++) {
            weapons[i].Setup();
        }

    }

    public void FireAllWeapons() {
        for (int i = 0; i < weapons.Count; i++) {
            if (weapons[i].CanAttack == true)
                StartCoroutine(weapons[i].FireWithDelay());
        }
    }

}
