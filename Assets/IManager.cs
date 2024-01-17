using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;

public class IManager : MonoBehaviour
{
    [SerializeField] private Camera arCam;
    [SerializeField] private ARRaycastManager _raycastManager;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject prefabToSpawn;
    
    private List<ARRaycastHit> _hits = new List<ARRaycastHit>();
    private Pose pose;
    private bool hasSpawned = false;

    void Update()
    {
        // Update crosshair position
        CrosshairCalculation();

        // Check for touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Check for touch beginning and not over UI
            if (touch.phase == TouchPhase.Began && !IsPointerOverUI(touch) && !hasSpawned)
            {
                // Perform raycast from the screen point touched by the user
                Ray ray = arCam.ScreenPointToRay(touch.position);
                List<ARRaycastHit> hits = new List<ARRaycastHit>();

                // Perform the raycast
                if (_raycastManager.Raycast(ray, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
                {
                    // Get the pose of the hit
                    pose = hits[0].pose;

                    // Spawn the prefab at the hit pose
                    SpawnPrefab();
                }
            }
        }
    }

    void CrosshairCalculation()
    {
        // Calculate crosshair position based on raycast hit
        Vector3 origin = arCam.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));
        Ray ray = arCam.ScreenPointToRay(origin);

        if (_raycastManager.Raycast(ray, _hits))
        {
            pose = _hits[0].pose;
            crosshair.transform.position = pose.position;
            crosshair.transform.eulerAngles = new Vector3(90, 0, 0);
        }
    }

    void SpawnPrefab()
    {
        // Instantiate the prefab at the hit pose
        Instantiate(prefabToSpawn, pose.position, pose.rotation);
        crosshair.SetActive(false);

        // Set the flag to true to indicate that the prefab has been spawned
        hasSpawned = true;
    }

    bool IsPointerOverUI(Touch touch)
    {
        // Check if the touch is over a UI element
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = new Vector2(touch.position.x, touch.position.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}
