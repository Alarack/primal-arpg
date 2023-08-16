using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallHelper : MonoBehaviour {

    public enum WallDirection {
        NORTH,
        SOUTH,
        EAST,
        WEST
    }

    public WallDirection direciton;
    public SpriteRenderer sprite;
    public BoxCollider2D myCollider;

    private void Awake() {
        //Vector3 position = Camera.main.WorldToViewportPoint(transform.position);

        float screenWidthScale = Camera.main.orthographicSize * 2f * Screen.width / Screen.height;
        float screenHeightScale = Camera.main.orthographicSize * 2f;

        float dist = (transform.position - Camera.main.transform.position).z;

        Vector3 point = Vector3.zero;

        switch (direciton) {
            case WallDirection.NORTH:
                point = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1, dist));
                transform.localScale = new Vector3(screenWidthScale, 1f, 1f);
                break;
            case WallDirection.SOUTH:
                point = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.15f, dist));
                //transform.localScale = new Vector3(screenWidthScale, 1f, 1f);
                sprite.size = new Vector2(screenWidthScale * 0.98f, 2f);
                myCollider.size = new Vector2(screenWidthScale, 2f);
                break;
            case WallDirection.EAST:
                point = Camera.main.ViewportToWorldPoint(new Vector3(1, 0.5f, dist));
                transform.localScale = new Vector3(1f, screenHeightScale, 1f);
                
                break;
            case WallDirection.WEST:
                point = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.5f, dist));
                transform.localScale = new Vector3(1f, screenHeightScale, 1f);
                break;
        }


        transform.position = point;
    }


}
