using UnityEngine;
using UnityEngine.UI;

public class MetricsCalculation : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Text distanceText;
    public Text unevennessText;
    void getTotal3DDistance()
    {
        float totalDistance = 0;
        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            totalDistance += Vector3.Distance(lineRenderer.GetPosition(i), lineRenderer.GetPosition(i + 1));
        }
        Debug.Log("Total Distance (in meters): " + totalDistance);
        distanceText.text = "Total Distance (in meters): " + totalDistance;
    }

    void getTotal2DDistance()
    {
        float totalDistance = 0;
        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            Vector2 point1 = new Vector2(lineRenderer.GetPosition(i).x, lineRenderer.GetPosition(i).z);
            Vector2 point2 = new Vector2(lineRenderer.GetPosition(i + 1).x, lineRenderer.GetPosition(i + 1).z);
            totalDistance += Vector2.Distance(point1, point2);
        }
        Debug.Log("Total Distance (in meters): " + totalDistance);
    }

    void getTotalUnevenness()
    {
        float totalUnevenness = 0;
        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            totalUnevenness += Mathf.Abs(lineRenderer.GetPosition(i).y - lineRenderer.GetPosition(i + 1).y);
        }
        Debug.Log("Total Unevenness (in meters): " + totalUnevenness);
        unevennessText.text = "Total Unevenness (in meters): " + totalUnevenness;
    }

    private void Update()
    {
        getTotal3DDistance();
        getTotalUnevenness();
    }
}
