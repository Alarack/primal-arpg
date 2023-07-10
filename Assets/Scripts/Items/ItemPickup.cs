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
    }


    public virtual void Setup(Item item) {
        this.Item = item;
        SetupImage();
        tooltip.Setup(this);
    }

    public virtual void Setup(ItemData itemData) {

        if (itemData.validSlots.Contains(ItemSlot.Weapon)) {
            Item = new ItemWeapon(itemData, null);
        }
        else {
            Item = new Item(itemData, null);
            
        }

        Setup(Item);
    }

    protected void SetupImage() {
        mainSprite.sprite = Item.Data.pickupIcon;
        shadowSprite.sprite = Item.Data.pickupIcon;
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

        VFXUtility.SpawnVFX(collectVFX, transform, 2f);

        EntityManager.ActivePlayer.Inventory.Add(Item);
        Destroy(gameObject);
    }

}
