using UnityEngine;

public static class WaypointStorage
{
    public static Vector3 waypointStart = Vector3.negativeInfinity;
    public static Vector3 waypointEnd = Vector3.negativeInfinity;


    public static bool IsValidWaypoint(Vector3 v) =>
    !float.IsNegativeInfinity(v.x) &&
    !float.IsNegativeInfinity(v.y) &&
    !float.IsNegativeInfinity(v.z);
}
