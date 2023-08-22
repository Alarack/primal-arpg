using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum WorldPositionConstant {
    UpperLeft,
    UpperRight, 
    LowerRight,
    LowerLeft
}


public class TargetHelper : Singleton<TargetHelper>
{

    public Transform upperLeftCorner;
    public Transform upperRightCorner;
    public Transform lowerLeftCorner;
    public Transform lowerRightCorner;

    public void Awake() {
        SetTransformLocations();
    }

    private void SetTransformLocations() {
        Vector2 upperLeft = Camera.main.ViewportToWorldPoint(new Vector2(0.05f, 0.90f));
        upperLeftCorner = new GameObject().transform;
        upperLeftCorner.gameObject.name = "Upper Left Corner";
        upperLeftCorner.transform.position = upperLeft;
        upperLeftCorner.SetParent(transform, true);

        Vector2 upperRight = Camera.main.ViewportToWorldPoint(new Vector2(0.95f, 0.90f));
        upperRightCorner = new GameObject().transform;
        upperRightCorner.gameObject.name = "Upper Right Corner";
        upperRightCorner.transform.position = upperRight;
        upperRightCorner.SetParent(transform, true);

        Vector2 lowerRight = Camera.main.ViewportToWorldPoint(new Vector2(0.95f, 0.25f));
        lowerRightCorner = new GameObject().transform;
        lowerRightCorner.gameObject.name = "Lower Right Corner";
        lowerRightCorner.transform.position = lowerRight;
        lowerRightCorner.SetParent(transform, true);

        Vector2 lowerLeft = Camera.main.ViewportToWorldPoint(new Vector2(0.05f, 0.25f));
        lowerLeftCorner = new GameObject().transform;
        lowerLeftCorner.gameObject.name = "Lower Left Corner";
        lowerLeftCorner.transform.position = lowerLeft;
        lowerLeftCorner.SetParent(transform, true);

    }

    public static List<Vector2> GetAllCorners() {
        List<Vector2> result = new List<Vector2> {
            Instance.upperLeftCorner.transform.position,
            Instance.upperRightCorner.transform.position,
            Instance.lowerRightCorner.transform.position,
            Instance.lowerLeftCorner.transform.position
        };

        return result;
    }


    public static Vector2 GetWorldPoint(WorldPositionConstant point) {
        Vector2 result = point switch {
            WorldPositionConstant.UpperLeft => Instance.upperLeftCorner.transform.position,
            WorldPositionConstant.UpperRight => Instance.upperRightCorner.transform.position,
            WorldPositionConstant.LowerRight => Instance.lowerRightCorner.transform.position,
            WorldPositionConstant.LowerLeft => Instance.lowerLeftCorner.transform.position,
            _ => Vector2.zero,
        };


        return result;
    }

    public static Vector2 GetMidpoint(WorldPositionConstant pointA, WorldPositionConstant pointB) {
        return Vector2.Lerp(GetWorldPoint(pointA), GetWorldPoint(pointB), 0.5f);
    }

}
