using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class WaypointPlacement : MonoBehaviour
{
    public GameObject waypointPrefab;
    public Button placeStartButton;
    public Button placeEndButton;

    public LineRenderer lineRenderer;
    public float minDistance = 0.1f;
    private List<Vector3> waypoints = new List<Vector3>();
    private bool isDrawing = false;


    private GameObject waypointStart;
    private GameObject waypointEnd;

    private bool isPlacingStart = true;
    private bool isPlacingEnd = false;

    private bool startAddedLine = false;

    public void PlaceStart()
    {
        isPlacingStart = true;
        isPlacingEnd = false;
        placeStartButton.interactable = false;
        placeEndButton.interactable = true;
    }

    public void PlaceEnd()
    {
        isPlacingEnd = true;
        isPlacingStart = false;
        placeStartButton.interactable = true;
        placeEndButton.interactable = false;
    }

    private void UpdatePoints()
    {
        if ((isPlacingStart || isPlacingEnd) && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (isPlacingStart)
                {
                    if (waypointStart != null)
                    {
                        Destroy(waypointStart);
                    }
                    waypointStart = Instantiate(waypointPrefab, hit.point, Quaternion.identity);
                    isPlacingStart = false;
                    placeStartButton.interactable = true;

                }
                else
                {
                    if (waypointEnd != null)
                    {
                        Destroy(waypointEnd);
                    }
                    waypointEnd = Instantiate(waypointPrefab, hit.point, Quaternion.identity);
                    isPlacingEnd = false;
                    placeEndButton.interactable = true;
                }
            }
        }
    }

    private void UpdateLine()
    {
        if (!startAddedLine)
        {
            waypoints.Add(waypointStart.transform.position);
            lineRenderer.positionCount = waypoints.Count;
            lineRenderer.SetPositions(waypoints.ToArray());
            startAddedLine = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            waypoints.Clear();
            isDrawing = true;
        }
        if (isDrawing && Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (waypoints.Count == 0 || Vector3.Distance(waypoints[waypoints.Count - 1], hit.point) > minDistance)
                {
                    waypoints.Add(hit.point);
                    lineRenderer.positionCount = waypoints.Count;
                    lineRenderer.SetPositions(waypoints.ToArray());
                }
            }
        }
        if (isDrawing && Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
            waypoints.Add(waypointEnd.transform.position);
            lineRenderer.positionCount = waypoints.Count;
            lineRenderer.SetPositions(waypoints.ToArray());
        }
    }
    void Update()
    {
        UpdatePoints();
        if (waypointEnd != null && waypointStart != null) UpdateLine();
    }
}
