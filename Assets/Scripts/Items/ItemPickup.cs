using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{

    [Header("Sprites")]
    public SpriteRenderer mainSprite;
    public SpriteRenderer shadowSprite;

    [Header("Mask")]
    public LayerMask layerMask;

    [Header("VFX")]
    public GameObject collectVFX;
    public GameObject spawnVFX;
    public float vfxScale = 1f;

    [Header("Lifetime")]
    public float lifetime;

    protected Rigidbody2D rb;
    protected ItemPickupTooltip tooltip;
    protected Bouncer bouncer;
    public Item Item { get; private set; }

    public bool IsGrounded { get { return bouncer.IsGrounded; } }

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        tooltip = GetComponentInChildren<ItemPickupTooltip>();
        bouncer = GetComponent<Bouncer>();
    }

    protected virtual void Start() {
        VFXUtility.SpawnVFX(spawnVFX, transform, 2f);

        if(lifetime > 0) {
            Destroy(gameObject, lifetime);
        }
    }


    public virtual void Setup(Item item) {
        this.Item = item;
        SetupImage();
        if(tooltip != null) 
            tooltip.Setup(this);
    }

    public virtual void Setup(ItemData itemData) {

        Item = ItemFactory.CreateItem(itemData, null);


        //if (itemData.validSlots.Contains(ItemSlot.Weapon)) {
        //    Item = new ItemWeapon(itemData, null);
        //}
        //else {
        //    Item = new Item(itemData, null);
            
        //}

        Setup(Item);
    }

    protected void SetupImage() {

        if(Item.Data.pickupIcon != null) {
            mainSprite.sprite = Item.Data.pickupIcon;
            shadowSprite.sprite = Item.Data.pickupIcon;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other) {
        if (LayerTools.IsLayerInMask(layerMask, other.gameObject.layer) == false)
            return;

        if (Item != null && Item.Data.pickupOnCollision == false)
            return;

        Collect();

    }

    public virtual void Collect() {

        if(Item == null) 
            return;

        VFXUtility.SpawnVFX(collectVFX, transform.position, Quaternion.identity, null, 2f, vfxScale);

        EntityManager.ActivePlayer.Inventory.Add(Item);
        Destroy(gameObject);
    }

}
