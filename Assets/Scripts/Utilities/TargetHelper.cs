using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum WorldPositionConstant {
    UpperLeft,
    UpperRight, 
    UpperMiddle,
    LowerRight,
    LowerLeft,
    LowerMiddle,
    Center,
    LeftMiddle,
    RightMiddle,


}


public class TargetHelper : Singleton<TargetHelper>
{

    private Transform upperLeftCorner;
    private Transform upperRightCorner;
    private Transform lowerLeftCorner;
    private Transform lowerRightCorner;

    public void Awake() {
        SetTransformLocations();
        //DebugCreateRow();
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
            WorldPositionConstant.UpperMiddle =>GetMidpoint(WorldPositionConstant.UpperLeft, WorldPositionConstant.UpperRight),
            WorldPositionConstant.LowerMiddle => GetMidpoint(WorldPositionConstant.LowerLeft, WorldPositionConstant.LowerRight),
            WorldPositionConstant.LeftMiddle => GetMidpoint(WorldPositionConstant.UpperLeft, WorldPositionConstant.LowerLeft),
            WorldPositionConstant.RightMiddle => GetMidpoint(WorldPositionConstant.UpperRight, WorldPositionConstant.LowerRight),
            WorldPositionConstant.Center => GetMidpoint(WorldPositionConstant.LeftMiddle, WorldPositionConstant.RightMiddle),

            _ => Vector2.zero,
        };


        return result;
    }

    public static Vector2 GetMidpoint(WorldPositionConstant pointA, WorldPositionConstant pointB) {
        return Vector2.Lerp(GetWorldPoint(pointA), GetWorldPoint(pointB), 0.5f);
    }

    public static List<Vector2> GetPointSequence(Vector2 startLocation, Vector2 direction, int count, float step) {
        //Vector2 viewportStart = Camera.main.WorldToViewportPoint(startLocation);

        List<Vector2> result = new List<Vector2>();

        Vector2 lastPoint = startLocation;

        for (int i = 0; i < count; i++) {
            Vector2 nextPoint = lastPoint + direction * step * i;
            result.Add(nextPoint);
            lastPoint = nextPoint;
        }


        return result;
    }

    public static List<Vector2> GetWorldSpacePointSequence(Vector2 startPoint, Vector3 endPoint, int count, bool includeRangePoints = true) {
        List<Vector2> results = new List<Vector2>();

        if (includeRangePoints == true)
            results.Add(startPoint);

        for (int i = 0; i < count; i++) {
            Vector2 targetPos = Vector2.Lerp(startPoint, endPoint, (i + 0.5f) / count);
            results.Add(targetPos);
        }

        if (includeRangePoints == true)
            results.Add(endPoint);

        return results;
    }

    public static List<Vector2> GetWorldSpacePointSequence(WorldPositionConstant start, WorldPositionConstant end, int count, bool includeRangePoints = true) {
        
        return GetWorldSpacePointSequence(GetWorldPoint(start), GetWorldPoint(end), count, includeRangePoints);
    }

    public static List<Vector2> GetViewportPointSequence(Vector2 startLocation, Vector2 direction, int count) {
        List<Vector2> result = new List<Vector2>();

        Vector2 viewportStart = Camera.main.WorldToViewportPoint(startLocation);
        Vector2 viewportDirection = new Vector2( direction.x / count, direction.y / count); //Camera.main.WorldToViewportPoint(direction);

        Vector2 lastPoint = viewportStart;

        result.Add(viewportStart);

        for (int i = 0; i < count -1; i++) {
            Vector2 nextPoint = lastPoint + viewportDirection;

            result.Add(nextPoint);
            lastPoint = nextPoint;
        }

        return result;
    }

    public static List<Vector2> GetCenterRow(int count) {
        Vector2 start = GetWorldPoint(WorldPositionConstant.LeftMiddle); //GetMidpoint(WorldPositionConstant.UpperLeft, WorldPositionConstant.LowerLeft);
        Vector2 end = GetWorldPoint(WorldPositionConstant.RightMiddle); //GetMidpoint(WorldPositionConstant.UpperRight, WorldPositionConstant.LowerRight);

        List<Vector2> points = GetWorldSpacePointSequence(start, end, count);
 
        return points;
    }

    public static void DebugCreateRow() {
        

        
        
        List<Vector2> points;// = GetCenterRow(4);


        //Vector2 start = GetMidpoint(WorldPositionConstant.UpperLeft, WorldPositionConstant.LowerLeft);
        //Vector2 end = GetMidpoint(WorldPositionConstant.UpperRight, WorldPositionConstant.LowerRight);

        points = GetCenterRow(3); // GetWorldSpacePointSequence(start, end, 3);

        for (int i = 0; i < points.Count; i++) {
            GameObject testPoint = new GameObject();
            testPoint.name = "Test Point: " + i;

            Vector3 targetPosition = points[i]; //Camera.main.ViewportToWorldPoint(points[i]);

            testPoint.transform.position = new Vector3(targetPosition.x, targetPosition.y, 0f);
        }

    }

    //public static List<Vector2> GetCenterRow(int count) {

    //    Vector2 startPoint = GetMidpoint(WorldPositionConstant.UpperLeft, WorldPositionConstant.LowerLeft);

    //    Vector2 direction = Vector2.right;

    //    List<Vector2> result = GetViewportPointSequence(startPoint, direction, count);

    //    return result;
    //}

}
