using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{

    protected Item item;

    [Header("Mask")]
    public LayerMask layerMask;

    [Header("VFX")]
    public GameObject collectVFX;
    public GameObject spawnVFX;

    protected Rigidbody2D rb;


    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start() {
        VFXUtility.SpawnVFX(spawnVFX, transform, 2f);
    }


    public virtual void Setup(Item item) {
        this.item = item;
    }

    public virtual void Setup(ItemData itemData) {
        item = new Item(itemData, null);
    }


    protected virtual void OnTriggerEnter2D(Collider2D other) {
        if (LayerTools.IsLayerInMask(layerMask, other.gameObject.layer) == false)
            return;


        Collect();

    }



    protected virtual void Collect() {

        if(item == null) 
            return;

        VFXUtility.SpawnVFX(collectVFX, transform, 2f);

        EntityManager.ActivePlayer.Inventory.Add(item);
        Destroy(gameObject);
    }

}
